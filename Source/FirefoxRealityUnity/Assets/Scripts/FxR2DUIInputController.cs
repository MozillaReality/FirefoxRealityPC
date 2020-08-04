// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019-2020, Mozilla.

using UnityEngine;
using UnityEngine.EventSystems;

public class FxR2DUIInputController : MonoBehaviour
{
    [SerializeField] protected Camera UICamera;
    Transform previousContact = null;

    private void Update()
    {
        var ray = UICamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool bHit = Physics.Raycast(ray, out hit);
        if (previousContact && (!bHit || previousContact != hit.transform))
        {
            IPointerExitHandler[] pointerExitHandlers = previousContact.GetComponents<IPointerExitHandler>();
            foreach (var pointerExitHandler in pointerExitHandlers)
            {
                pointerExitHandler?.OnPointerExit(new PointerEventData(EventSystem.current));
            }
        }

        if (!bHit)
        {
            previousContact = null;
        }
        else
        {
            if (previousContact != hit.transform)
            {
                IPointerEnterHandler[] pointerEnterHandlers = hit.transform.GetComponents<IPointerEnterHandler>();
                foreach (var pointerEnterHandler in pointerEnterHandlers)
                {
                    pointerEnterHandler?.OnPointerEnter(new PointerEventData(EventSystem.current));
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                IPointerDownHandler[] downHandlers = hit.transform.GetComponents<IPointerDownHandler>();

                foreach (var downHandler in downHandlers)
                {
                    downHandler?.OnPointerDown(new PointerEventData(EventSystem.current));
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                IPointerUpHandler[] upHandlers = hit.transform.GetComponents<IPointerUpHandler>();

                foreach (var downHandler in upHandlers)
                {
                    downHandler?.OnPointerUp(new PointerEventData(EventSystem.current));
                }

                IPointerClickHandler[] clickHandlers = hit.transform.GetComponents<IPointerClickHandler>();
                foreach (var clickHandler in clickHandlers)
                {
                    clickHandler?.OnPointerClick(new PointerEventData(EventSystem.current));
                }
            }

            previousContact = hit.transform;
        }
    }
}