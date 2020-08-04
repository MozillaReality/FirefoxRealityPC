//
// FxRLaserPointer.cs
//
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019-2020, Mozilla.
//
// Author(s): Philip Lamb
//
// Alternatively, the contents of this file may be used under the terms
// of the original license as provided for below:
//
// Copyright(c) Valve Corporation
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met
//
// 1. Redistributions of source code must retain the above copyright notice, this
// list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
// this list of conditions and the following disclaimer in the documentation andor
// other materials provided with the distribution.
//
// 3. Neither the name of the copyright holder nor the names of its contributors
// may be used to endorse or promote products derived from this software without
// specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS AS IS AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
// ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
// ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

public class FxRLaserPointer : MonoBehaviour
{
    public SteamVR_Behaviour_Pose pose;

    //public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.__actions_default_in_InteractUI;
    public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.GetBooleanAction("InteractUI");
    public SteamVR_Action_Vector2 scroll2D = SteamVR_Input.GetVector2Action("Scroll2D");

    public Color color = new Color(0.0f, 0.8f, 1.0f, 0.8f);
    public float thickness = 0.002f;
    public Color clickColor = new Color(0.0f, 0.8f, 1.0f, 0.8f);
    public float clickThicknesss = 0.0024f;
    public Texture2D hitTargetTexture;
    public float hitTargetRadius = 0.01f;

    private static List<FxRLaserPointer> AllLaserPointers = new List<FxRLaserPointer>();
    private static FxRLaserPointer ActiveLaserPointer = null;

    private GameObject hitTarget;
    private GameObject holder;
    private GameObject pointer;
    bool isActive = false;
    public bool addRigidBody = false;

    // Subscribe to this event to know when the user pulled the trigger when not pointing at anything
    public delegate void PointerAirClick();

    public static PointerAirClick OnPointerAirClick;

    public event PointerEventHandler PointerIn;
    public event PointerEventHandler PointerOut;
    public event PointerEventHandler PointerClick;

    Transform previousContact = null;

    private void OnDisable()
    {
        LaserShowing = false;
        if (ActiveLaserPointer == this)
        {
            ActiveLaserPointer = null;
        }
    }

    private bool LaserShowing
    {
        set
        {
            if (value != laserShowing)
            {
                laserShowing = value;
                if (pointer != null)
                {
                    pointer.SetActive(laserShowing);
                }

                if (hitTarget != null)
                {
                    hitTarget.SetActive(laserShowing);
                }
            }
        }
    }

    private bool laserShowing = true;

    private void Start()
    {
        if (pose == null)
            pose = this.GetComponent<SteamVR_Behaviour_Pose>();
        if (pose == null)
            Debug.LogError("No SteamVR_Behaviour_Pose component found on this object");

        if (interactWithUI == null)
            Debug.LogError("No UI Interaction action has been set on this component.");
        if (scroll2D == null)
            Debug.LogError("No 2D scrolling action has been set on this component.");

        // Create the hit target.
        hitTarget = new GameObject("Hit target");
        Shader shaderSource = Shader.Find("TextureOverlayNoLight");
        Material mat = new Material(shaderSource);
        mat.hideFlags = HideFlags.HideAndDontSave;
        mat.mainTexture = hitTargetTexture;
        Mesh m = new Mesh();
        m.vertices = new Vector3[]
        {
            new Vector3(-hitTargetRadius, -hitTargetRadius, 0.0f),
            new Vector3(hitTargetRadius, -hitTargetRadius, 0.0f),
            new Vector3(hitTargetRadius, hitTargetRadius, 0.0f),
            new Vector3(-hitTargetRadius, hitTargetRadius, 0.0f),
        };
        m.normals = new Vector3[]
        {
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(0.0f, 0.0f, 1.0f),
        };
        float u1 = 0.0f;
        float u2 = 1.0f;
        float v1 = 0.0f;
        float v2 = 1.0f;
        m.uv = new Vector2[]
        {
            new Vector2(u1, v1),
            new Vector2(u2, v1),
            new Vector2(u2, v2),
            new Vector2(u1, v2)
        };
        m.triangles = new int[]
        {
            2, 1, 0,
            3, 2, 0
        };
        MeshFilter filter = hitTarget.AddComponent<MeshFilter>();
        filter.mesh = m;
        MeshRenderer meshRenderer = hitTarget.AddComponent<MeshRenderer>();
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        hitTarget.GetComponent<Renderer>().material = mat;
        hitTarget.SetActive(false);

        holder = new GameObject("Holder");
        holder.transform.parent = this.transform;
        holder.transform.localPosition = Vector3.zero;
        holder.transform.localRotation = Quaternion.identity;

        pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pointer.name = "Pointer";
        pointer.transform.parent = holder.transform;
        pointer.transform.localScale = new Vector3(thickness, thickness, 100f);
        pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
        pointer.transform.localRotation = Quaternion.identity;
        BoxCollider collider = pointer.GetComponent<BoxCollider>();
        if (addRigidBody)
        {
            if (collider)
            {
                collider.isTrigger = true;
            }

            Rigidbody rigidBody = pointer.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
        }
        else
        {
            if (collider)
            {
                Destroy(collider);
            }
        }

        Material newMaterial = new Material(Shader.Find("Unlit/Color"));
        newMaterial.SetColor("_Color", color);
        pointer.GetComponent<MeshRenderer>().material = newMaterial;

        // Make sure only one laser pointer is showing at once
        AllLaserPointers.Add(this);

        LaserShowing = false;
    }

    public virtual void OnPointerIn(PointerEventArgs e)
    {
        IPointerEnterHandler[] pointerEnterHandlers = e.target.GetComponents<IPointerEnterHandler>();
        foreach (var pointerEnterHandler in pointerEnterHandlers)
        {
            pointerEnterHandler?.OnPointerEnter(new PointerEventData(EventSystem.current));
        }

        if (PointerIn != null)
            PointerIn(this, e);
    }

    public virtual void OnPointerClick(PointerEventArgs e)
    {
        IPointerClickHandler[] clickHandlers = e.target.GetComponents<IPointerClickHandler>();
        foreach (var clickHandler in clickHandlers)
        {
            clickHandler?.OnPointerClick(new PointerEventData(EventSystem.current));
        }

        if (PointerClick != null)
            PointerClick(this, e);
    }

    private void OnPointerDown(PointerEventArgs e)
    {
        IPointerDownHandler[] downHandlers = e.target.GetComponents<IPointerDownHandler>();
        foreach (var downHandler in downHandlers)
        {
            downHandler?.OnPointerDown(new PointerEventData(EventSystem.current));
        }
    }

    private void OnPointerUp(PointerEventArgs e)
    {
        IPointerUpHandler[] upHandlers = e.target.GetComponents<IPointerUpHandler>();
        foreach (var upHandler in upHandlers)
        {
            upHandler?.OnPointerUp(new PointerEventData(EventSystem.current));
        }
    }

    public virtual void OnPointerOut(PointerEventArgs e)
    {
        IPointerExitHandler[] pointerExitHandlers = e.target.GetComponents<IPointerExitHandler>();
        foreach (var pointerExitHandler in pointerExitHandlers)
        {
            pointerExitHandler?.OnPointerExit(new PointerEventData(EventSystem.current));
        }

        if (PointerOut != null)
            PointerOut(this, e);
    }


    private void Update()
    {
        if (!pose.isValid)
        {
            if (ActiveLaserPointer == this)
            {
                ActiveLaserPointer = null;
            }

            return;
        }

        if (!isActive)
        {
            isActive = true;
            this.transform.GetChild(0).gameObject.SetActive(true);
        }

        if (ActiveLaserPointer == null)
        {
            ActiveLaserPointer = this;
        }

        LaserShowing = (ActiveLaserPointer == this);
        if (ActiveLaserPointer != this)
        {
            if (interactWithUI.GetStateUp(pose.inputSource))
            {
                ActiveLaserPointer = this;
                LaserShowing = true;
            }

            return;
        }

        float dist = 100f;

        Ray raycast = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        bool bHit = Physics.Raycast(raycast, out hit);

        if (previousContact && (!bHit || previousContact != hit.transform))
        {
            // Default event system.
            PointerEventArgs args = new PointerEventArgs();
            args.fromInputSource = pose.inputSource;
            args.distance = 0f;
            args.flags = 0;
            args.target = previousContact;
            OnPointerOut(args);
        }

        // Target not found for hit testing, so clear state
        if (!bHit)
        {
            previousContact = null;
            hitTarget.SetActive(false);
            // Trigger an air click when user lets go of trigger without hitting anything
            if (interactWithUI.GetStateUp(pose.inputSource))
            {
                OnPointerAirClick?.Invoke();
            }
        }
        else
        {
            if (previousContact != hit.transform)
            {
                // Default event system.
                PointerEventArgs argsIn = new PointerEventArgs();
                argsIn.fromInputSource = pose.inputSource;
                argsIn.distance = hit.distance;
                argsIn.flags = 0;
                argsIn.target = hit.transform;
                OnPointerIn(argsIn);
                previousContact = hit.transform;
            }

            if (hit.distance < 100f)
            {
                dist = hit.distance;

                hitTarget.transform.position = hit.point;
                hitTarget.transform.rotation = Quaternion.LookRotation(-hit.normal);
                hitTarget.SetActive(true);
            }

            if (interactWithUI.GetStateDown(pose.inputSource))
            {
                PointerEventArgs argsClick = new PointerEventArgs();
                argsClick.fromInputSource = pose.inputSource;
                argsClick.distance = hit.distance;
                argsClick.flags = 0;
                argsClick.target = hit.transform;
                OnPointerDown(argsClick);
            }

            if (interactWithUI.GetStateUp(pose.inputSource))
            {
                // Default event system.
                PointerEventArgs argsClick = new PointerEventArgs();
                argsClick.fromInputSource = pose.inputSource;
                argsClick.distance = hit.distance;
                argsClick.flags = 0;
                argsClick.target = hit.transform;
                OnPointerClick(argsClick);
                OnPointerUp(argsClick);
            }
        } // bHit

        if (interactWithUI != null && interactWithUI.GetState(pose.inputSource))
        {
            pointer.transform.localScale = new Vector3(clickThicknesss, clickThicknesss, dist);
            pointer.GetComponent<MeshRenderer>().material.color = clickColor;
        }
        else
        {
            pointer.transform.localScale = new Vector3(thickness, thickness, dist);
            pointer.GetComponent<MeshRenderer>().material.color = color;
        }

        pointer.transform.localPosition = new Vector3(0f, 0f, dist / 2f);
    }
}

public struct PointerEventArgs
{
    public SteamVR_Input_Sources fromInputSource;
    public uint flags;
    public float distance;
    public Transform target;
}

public delegate void PointerEventHandler(object sender, PointerEventArgs e);