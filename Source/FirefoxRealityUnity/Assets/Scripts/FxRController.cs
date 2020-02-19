// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019, Mozilla.
//
// FxRController acts in the middle of bootstrapping Firefox
// Reality with desktop Firefox.

#define USE_EDITOR_HARDCODED_FIREFOX_PATH // Comment this out to not use a hardcoded path in editor, but instead use StreamingAssets

using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Hand = Valve.VR.InteractionSystem.Hand;

public class FxRController : MonoBehaviour
{
#if UNITY_EDITOR
    // Set the following to the location of your local desktop firefox build to use in editor
    private const string HardcodedFirefoxPath = "d:\\patri\\dev\\Mozilla\\gecko_build_release";
#endif

    [SerializeField] private Transform EnvironmentOrigin;
    [SerializeField] private FxREnvironmentSwitcher EnvironmentSwitcher;

    public enum FXR_BROWSING_MODE
    {
        FXR_BROWSER_MODE_DESKTOP_INSTALL,
        FXR_BROWSER_MODE_WEB_BROWSING
    }

    public delegate void BrowsingModeChanged(FXR_BROWSING_MODE browsingMode);

    public static BrowsingModeChanged OnBrowsingModeChanged;

    public static FXR_BROWSING_MODE CurrentBrowsingMode
    {
        get => currentBrowsingMode;
        private set
        {
            if (currentBrowsingMode != value)
            {
                OnBrowsingModeChanged?.Invoke(value);
            }

            currentBrowsingMode = value;
        }
    }

    private static FXR_BROWSING_MODE currentBrowsingMode = FXR_BROWSING_MODE.FXR_BROWSER_MODE_DESKTOP_INSTALL;

    private HashSet<FxRLaserPointer> LaserPointers
    {
        get
        {
            if (laserPointers == null)
            {
                laserPointers = new HashSet<FxRLaserPointer>();
            }

            // The laser pointers are inactive at startup in order that they don't show during the loading sequence,
            // since they end up being jittery and distracting.
            // 
            // Since FindObjectsOfType<> only finds active objects, we make sure that they are lazily found by FindObjectsOfType<>
            // as soon as they become active.
            if (laserPointers.Count < 2)
            {
                laserPointers.UnionWith(FindObjectsOfType<FxRLaserPointer>());
            }

            return laserPointers;
        }
    }

    private HashSet<FxRLaserPointer> laserPointers;

    private HashSet<Hand> Hands
    {
        get
        {
            if (hands == null)
            {
                hands = new HashSet<Hand>();
            }

            // The controllers are inactive at startup in order that they don't show during the loading sequence,
            // since they end up being jittery and distracting.
            // 
            // Since FindObjectsOfType<> only finds active objects, we make sure that they are lazily found by FindObjectsOfType<>
            // as soon as they become active.
            if (hands.Count < 2)
            {
                hands.UnionWith(FindObjectsOfType<Hand>());
            }

            return hands;
        }
    }

    private HashSet<Hand> hands;

    private int _hackKeepWindowIndex;

    //
    // MonoBehavior methods.
    //

    void Awake()
    {
        Debug.Log("FxRController.Awake())");
    }

    private Vector3 initialBodyDirection;
    private bool bodyDirectionInitialzed;
    private int bodyDirectionChecks;

    void OnEnable()
    {
        initialBodyDirection = Player.instance.bodyDirectionGuess;

        Debug.Log("FxRController.OnEnable()");

        Application.runInBackground = true;

        FxRFirefoxDesktopInstallation.OnInstallationProcessComplete += HandleInstallationProcessComplete;
    }

    private void HandleInstallationProcessComplete()
    {
        CurrentBrowsingMode = FXR_BROWSING_MODE.FXR_BROWSER_MODE_WEB_BROWSING;
        // Hide the loading indicator
        FindObjectOfType<FxRLoadEnvironment>()?.HideLoadingOverlay();

        // Give the plugin a place to look for resources.
        string resourcesPath = Application.streamingAssetsPath;

#if (UNITY_EDITOR && USE_EDITOR_HARDCODED_FIREFOX_PATH)
        resourcesPath = HardcodedFirefoxPath;
#endif

        // TODO: Launch the browser, then exit...
    }

    void OnDisable()
    {
        Debug.Log("FxRController.OnDisable()");
        FxRFirefoxDesktopInstallation.OnInstallationProcessComplete -= HandleInstallationProcessComplete;
    }

    void Update()
    {
        if (!bodyDirectionInitialzed
            && !initialBodyDirection.Equals(Player.instance.bodyDirectionGuess))
        {
            bodyDirectionChecks++;
            if (bodyDirectionChecks > 3)
            {
                // Orient the environment so that the user is facing the browser window, and activate the environment
                EnvironmentOrigin.forward = Player.instance.bodyDirectionGuess;
                EnvironmentOrigin.transform.position = Player.instance.feetPositionGuess;
                // Initialize the environment
                EnvironmentSwitcher.SwitchEnvironment(0);
                bodyDirectionInitialzed = true;
            }
        }
    }
}