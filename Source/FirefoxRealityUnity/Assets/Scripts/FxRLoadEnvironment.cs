// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019, Mozilla.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;

public class FxRLoadEnvironment : MonoBehaviour
{
    private AsyncOperation LoadingOperation;

    [SerializeField] protected SteamVR_Overlay LoadingOverlay;
    [SerializeField] protected Transform LoadingSpinner;
    [SerializeField] private List<GameObject> Hands;
    [SerializeField] private float SpinnerRotationSpeed = 1f;

    private bool loadingOverlayHidden;
    private bool spinnerActive;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        LoadingOperation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        foreach (var hand in Hands)
        {
            hand.SetActive(false);
        }

        LoadingSpinner.gameObject.SetActive(false);
    }

    public void HideLoadingOverlay()
    {
        LoadingOverlay.gameObject.SetActive(false);
        loadingOverlayHidden = true;
        spinnerActive = false;
    }

    private void FixedUpdate()
    {
        if (spinnerActive)
        {
            // Rotate the spinner clockwise 
            var rotation = (Quaternion.Euler(Vector3.back * SpinnerRotationSpeed)) * LoadingSpinner.localRotation;
            LoadingSpinner.localRotation = Quaternion.Slerp(LoadingSpinner.localRotation, rotation,
                Time.fixedDeltaTime * SpinnerRotationSpeed);
        }
    }

    private void Update()
    {
        if (LoadingOperation != null && LoadingOperation.isDone)
        {
            if (loadingOverlayHidden)
            {
                // Safe to show the hands now
                foreach (var hand in Hands)
                {
                    hand.SetActive(true);
                }

                // Our work here is done...
                Destroy(LoadingOverlay.gameObject);
                Destroy(gameObject);
            }
            else
            {
                // Scene is loaded - show spinner until we are told to hide
                LoadingSpinner.gameObject.SetActive(true);
                spinnerActive = true;
            }
        }
    }
}