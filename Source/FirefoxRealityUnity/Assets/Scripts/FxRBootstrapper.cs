﻿// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019-2020, Mozilla.

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class FxRBootstrapper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FxRUnitySystemConsoleRedirector.Redirect();
        FxRFirefoxRealityVersionChecker.Instance.CheckForNewFirefoxRealityPC((newVersionAvailable, serverVersionInfo) =>
        {
            if (newVersionAvailable)
            {
                StartCoroutine(LoadLoadingScene());
            }
            else
            {
                if (FxRFirefoxDesktopInstallation.FxRDesktopInstallationType ==
                    FxRFirefoxDesktopInstallation.InstallationType.EMBEDDED)
                {
                    FxRController.LaunchFirefoxDesktop();
                    FxRController.Quit(0);
                }
                else
                {
                    FxRFirefoxDesktopVersionChecker.Instance.CheckIfFirefoxInstallationOrConfigurationRequired(
                        (installRequired, configurationRequired, firefoxInstallationRequirements) =>
                        {
                            if (installRequired || configurationRequired)
                            {
                                StartCoroutine(LoadLoadingScene());
                            }
                            else
                            {
                                FxRController.LaunchFirefoxDesktop();
                                FxRController.Quit(0);
                            }
                        });
                }
            }
        });
    }

    IEnumerator LoadLoadingScene()
    {
        yield return new WaitForEndOfFrame();
        XRSettings.LoadDeviceByName("OpenVR");
        yield return new WaitForEndOfFrame();
        XRSettings.enabled = true;
        yield return new WaitForEndOfFrame();
        SceneManager.LoadSceneAsync("LoadingScene");
    }
}