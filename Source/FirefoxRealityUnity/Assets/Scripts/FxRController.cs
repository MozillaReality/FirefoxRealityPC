// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019, Mozilla.
//
// FxRController acts in the middle of bootstrapping Firefox Reality with desktop Firefox.

//#define USE_EDITOR_HARDCODED_FIREFOX_PATH // Comment this out to not use a hardcoded path in editor, but instead use StreamingAssets

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Debug = UnityEngine.Debug;
using Hand = Valve.VR.InteractionSystem.Hand;
using static FxRTelemetryService;
using System;
using System.Runtime.InteropServices;
using System.Threading;

public class FxRController : MonoBehaviour
{
#if UNITY_EDITOR
    // Set the following to the location of your local desktop firefox build to use in editor
    private const string HardcodedFirefoxPath = "d:\\patri\\dev\\Mozilla\\gecko_build_release";
#endif

    [SerializeField] private Transform EnvironmentOrigin;
    [SerializeField] private FxREnvironmentSwitcher EnvironmentSwitcher;

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

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
        // Hide the loading indicator
        FindObjectOfType<FxRLoadEnvironment>()?.HideLoadingOverlay();

        // We're all set - launch the overlay browser
        if (LaunchFirefoxDesktop())
        {
            Quit(0);
        }
        else
        {
            // TODO: Show an error/retry dialog?
            Quit(1);
        }
    }

    // Parsing user preferences from `prefs.js` in Firefox user profile folder
    // if the flags are existing in `userPrefs` param.
    private static Dictionary<string, string> ProcessFirefoxProfilePref(string folderPath,
        string[] userPrefs)
    {
        Dictionary<string, string> pref = new Dictionary<string, string>();
        string prefFilePath = folderPath + "/prefs.js";

        if (!File.Exists(prefFilePath))
        {
            return pref;
        }

        // Read each line of the file into a string array. Each element
        // of the array is one line of the file.
        string[] lines = System.IO.File.ReadAllLines(prefFilePath);
        int keyStartOffset = ("" + ('"')).Length;
        int valueStartOffset = (", ").Length;

        foreach (string line in lines)
        {
            if (line.IndexOf("user_pref(") < 0)
            {
                continue;
            }

            int start = line.IndexOf('"') + keyStartOffset;
            int end = line.IndexOf('"', start);
            string key = line.Substring(start, end - start);

            if (!Array.Exists<string>(userPrefs, item => item.Equals(key))) {
                continue;
            }

            start = line.IndexOf(", ") + valueStartOffset;
            end = line.LastIndexOf(");");
            string value = line.Substring(start, end - start);

            pref.Add(key, value);
        }

        return pref;
    }

    public static void Quit(int exitCode)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // TODO: Temporary workaround until Bug 1651340 is resolved.
        // Hide the window when the main thread is going to sleep
        // in order to avoid cluttering the desktop.
        const int SW_HIDE = 0;
        var hwnd = GetActiveWindow();
        ShowWindow(hwnd, SW_HIDE);
        // Sleep for 4000 ms to wait for the telemetry uploader finishes
        // its tasks and avoid Glean.dll is unloaded by Application.Quit().
        Thread.Sleep(4000);
        Application.Quit(exitCode);
#endif
    }

    public static bool LaunchFirefoxDesktop()
    {
        string firefoxInstallPath;
        string firefoxExePath;
        string profileDirectoryPath;
        if (FxRFirefoxDesktopInstallation.FxRDesktopInstallationType ==
            FxRFirefoxDesktopInstallation.InstallationType.EMBEDDED)
        {
            FxRTelemetryServiceInstance.SetInstallFrom(FxRFirefoxDesktopInstallation.InstallationType.EMBEDDED);
            // Embedded Firefox Desktop lives in streaming assets 
            firefoxInstallPath = Application.streamingAssetsPath;
#if (UNITY_EDITOR && USE_EDITOR_HARDCODED_FIREFOX_PATH)
            firefoxInstallPath = HardcodedFirefoxPath;
#endif
            profileDirectoryPath = Path.Combine(firefoxInstallPath, "fxr-profile");

            firefoxExePath = Path.Combine(firefoxInstallPath, "firefox", "firefox.exe");
        }
        else
        {
            FxRTelemetryServiceInstance.SetInstallFrom(FxRFirefoxDesktopInstallation.InstallationType.DOWNLOADED);
            // Downloaded Firefox Desktop lives in the standard installation location
            firefoxInstallPath = FxRFirefoxDesktopVersionChecker.GetFirefoxDesktopInstallationPath();
            firefoxExePath = Path.Combine(firefoxInstallPath, "firefox.exe");
            
            // We put the profile directory in the streaming assets folder, so it gets cleaned up on uninstall
            profileDirectoryPath = Path.Combine(Application.streamingAssetsPath, "fxr-profile");

#if (!UNITY_EDITOR)
            if (string.IsNullOrEmpty(firefoxInstallPath))
            {
                throw new Exception(
                    "Could not determine Firefox installation path!");
            }
#elif (UNITY_EDITOR && USE_EDITOR_HARDCODED_FIREFOX_PATH)
            firefoxExePath = Path.Combine(HardcodedFirefoxPath, "firefox", "firefox.exe");
            profileDirectoryPath = Path.Combine(HardcodedFirefoxPath, "fxr-profile");
#else
            profileDirectoryPath = Path.Combine(HardcodedFirefoxPath, "fxr-profile");
#endif
        }
        // "datareporting.healthreport.uploadEnabled" is the key of Firefox desktop
        // legacy telemetry in preferences.
        string telemetryKey = "datareporting.healthreport.uploadEnabled";
        var pref = ProcessFirefoxProfilePref(profileDirectoryPath, new string[] { telemetryKey });
        // Firefox telemetry is opt-out by default.
        var telemeletryEnabled = true;

        if (pref.ContainsKey(telemetryKey))
        {
            telemeletryEnabled = Convert.ToBoolean(pref["datareporting.healthreport.uploadEnabled"]);
        }
        FxRTelemetryServiceInstance.Initialize(telemeletryEnabled);
        // TODO: we need to change this distribution type while we distribute our app
        // to other channels.
        FxRTelemetryServiceInstance.SetDistributionChannel(DistributionChannelType.HTC);
        FxRTelemetryServiceInstance.SetEntryMethod(EntryMethod.LIBRARY);
        FxRTelemetryServiceInstance.SubmitLaunchPings();

        Process launchProcess = new Process();
        launchProcess.StartInfo.FileName = firefoxExePath;
        launchProcess.StartInfo.Arguments = string.Format("-profile \"{0}\" --fxr", profileDirectoryPath);
        return launchProcess.Start();
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