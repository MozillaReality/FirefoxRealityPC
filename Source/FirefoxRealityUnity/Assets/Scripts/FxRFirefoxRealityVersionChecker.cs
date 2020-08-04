// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019-2020, Mozilla.

using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class FxRFirefoxRealityVersionChecker : Singleton<FxRFirefoxRealityVersionChecker>
{
    private bool NewFirefoxRealityPCVersionCheckPerformed;
    private bool NewFirefoxRealityPCVersionAvailable;
    private FirefoxRealityPCVersions RetrievedFirefoxRealityPCVersions;
    public static readonly string FXR_PC_VERSIONS_JSON_FILENAME = "fxrpc_versions.json";

    // Class to represent JSON downloaded from Firefox Reality latest version service
    public class FirefoxRealityPCVersions
    {
        public string LATEST_FXR_PC_BUILD_NUMBER;
        public string LATEST_FXR_PC_VERSION;
        public string LATEST_FXR_PC_RELEASE_NOTE_HIGHLIGHTS;
        public string LATEST_FXR_PC_URL;
    }

    public void CheckForNewFirefoxRealityPC(Action<bool, FirefoxRealityPCVersions> updateRequiredResult)
    {
        if (NewFirefoxRealityPCVersionCheckPerformed)
        {
            updateRequiredResult?.Invoke(NewFirefoxRealityPCVersionAvailable, RetrievedFirefoxRealityPCVersions);
            return;
        }

        StartCoroutine(RetrieveLatestFirefoxRealityPCVersion((fxrPCWasSuccessful, serverVersionInfo) =>
        {
            if (fxrPCWasSuccessful)
            {
                NewFirefoxRealityPCVersionCheckPerformed = true;
                RetrievedFirefoxRealityPCVersions = serverVersionInfo;
                // Retrieve installed JSON versions file from Streaming Assets
                string localJSONPath = Path.Combine(Application.streamingAssetsPath, FXR_PC_VERSIONS_JSON_FILENAME);
                string localJSONVersionInfo = File.ReadAllText(localJSONPath);

                // Compare version number
                var localVersionInfo = JsonUtility.FromJson<FirefoxRealityPCVersions>(localJSONVersionInfo);

                int localBuildNumber;
                int serverBuildNumber;
                if (int.TryParse(localVersionInfo.LATEST_FXR_PC_BUILD_NUMBER, out localBuildNumber)
                    && int.TryParse(serverVersionInfo.LATEST_FXR_PC_BUILD_NUMBER, out serverBuildNumber)
                    && serverBuildNumber > localBuildNumber
                )
                {
                    NewFirefoxRealityPCVersionAvailable = true;
                    updateRequiredResult(true, serverVersionInfo);
                    return;
                }
            }

            updateRequiredResult(false, null);
        }));
    }

    private IEnumerator RetrieveLatestFirefoxRealityPCVersion(Action<bool, FirefoxRealityPCVersions> successCallback)
    {
        string RESTUrl = "https://mixedreality.mozilla.org/FirefoxRealityPC/" + FXR_PC_VERSIONS_JSON_FILENAME;
        var webRequest = new UnityWebRequest(RESTUrl);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        var operation = webRequest.SendWebRequest();
        yield return operation;
        if (string.IsNullOrEmpty(operation.webRequest.error))
        {
            string jsonResposne = webRequest.downloadHandler.text;
            var versionInfo = JsonUtility.FromJson<FirefoxRealityPCVersions>(jsonResposne);
            successCallback(true, versionInfo);
        }
        else
        {
            successCallback(false, null);
        }
    }
}