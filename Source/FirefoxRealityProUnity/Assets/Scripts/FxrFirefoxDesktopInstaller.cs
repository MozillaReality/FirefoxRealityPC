using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Win32;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

public class FxrFirefoxDesktopInstaller : MonoBehaviour
{
    private class FirefoxVersions
    {
        public string LATEST_FIREFOX_VERSION;
    }

    // Test method that kicks of the download, spits out download progress to the log, and then launches the downloaded installer 
    public void TestDesktopFirefoxInstall()
    {
        var progress = new Progress<float>(percent => { Debug.Log("Download progress: " + percent.ToString("P1")); });
        DownloadAndInstallDesktopFirefox(progress, (wasSuccessful, error) =>
        {
            if (wasSuccessful)
            {
                Debug.Log("Firefox Desktop successfully installed");
            }
            else
            {
                Debug.Log("Firefox Desktop was not successfully installed. Error: " + error);
            }
        });
    }

    void Start()
    {
        StartCoroutine(RetrieveLatestFirefoxVersion((wasSuccessful, versionString) =>
        {
            if (wasSuccessful)
            {
                string[] majorMinor = versionString.Split(new char[] {'.'});
                int releaseMajor = int.Parse(majorMinor[0]);
                string releaseMinor = "";
                for (int i = 1; i < majorMinor.Length; i++)
                {
                    releaseMinor += majorMinor[i];
                }

                if (IsFirefoxDesktopInstallationRequired(releaseMajor, releaseMinor))
                {
                    TestDesktopFirefoxInstall();
                }
            }
            else
            {
                // Fallback - couldn't retrieve latest from REST call
                // TODO: DO we want a fallback, or just fail silently and try again another time?
                if (IsFirefoxDesktopInstallationRequired(69, "0"))
                {
                    TestDesktopFirefoxInstall();
                }
            }
        }));
    }

    // Check if Firefox Desktop is installed
    // We'll compare minor version #'s ordinally, as they can contain letters
    public bool IsFirefoxDesktopInstallationRequired(int majorVersionRequired, string minorVersionRequired)
    {
        string releaseAndBetaPath = @"SOFTWARE\Mozilla\Mozilla Firefox";
        string releaseVersion = GetInstalledVersion(Registry.LocalMachine, releaseAndBetaPath);
        releaseVersion = string.IsNullOrEmpty(releaseVersion)
            ? GetInstalledVersion(Registry.CurrentUser, releaseAndBetaPath)
            : releaseVersion;

        string nightlyPath = @"SOFTWARE\mozilla.org\Mozilla";
        string nightlyVersion = GetInstalledVersion(Registry.LocalMachine, nightlyPath);
        nightlyVersion = string.IsNullOrEmpty(nightlyVersion)
            ? GetInstalledVersion(Registry.CurrentUser, nightlyPath)
            : nightlyVersion;

        if (!string.IsNullOrEmpty(releaseVersion))
        {
            if (InstalledVersionNewEnough(releaseVersion, majorVersionRequired, minorVersionRequired))
                return false;
        }

        if (!string.IsNullOrEmpty(nightlyVersion))
        {
            if (InstalledVersionNewEnough(nightlyVersion, majorVersionRequired, minorVersionRequired))
                return false;
        }

        return true;
    }

    private bool InstalledVersionNewEnough(string installedVersion, int majorVersionRequired,
        string minorVersionRequired)
    {
        string[] majorMinor = installedVersion.Split(new char[] {'.'});
        int releaseMajor = int.Parse(majorMinor[0]);
        string releaseMinor = "";
        for (int i = 1; i < majorMinor.Length; i++)
        {
            releaseMinor += majorMinor[i];
        }

        if (releaseMajor > majorVersionRequired
            || (releaseMajor == majorVersionRequired
                && string.CompareOrdinal(minorVersionRequired, releaseMinor) <= 0)
        )
        {
            return true;
        }

        return false;
    }

    public IEnumerator RetrieveLatestFirefoxVersion(Action<bool, string> successCallback)
    {
        string RESTUrl = "https://product-details.mozilla.org/1.0/firefox_versions.json";
        var webRequest = new UnityWebRequest(RESTUrl);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        var operation = webRequest.SendWebRequest();
        yield return operation;
        if (string.IsNullOrEmpty(operation.webRequest.error))
        {
            string jsonResposne = webRequest.downloadHandler.text;
            string latestVersion = JsonUtility.FromJson<FirefoxVersions>(jsonResposne).LATEST_FIREFOX_VERSION;
            successCallback(true, latestVersion);
        }
        else
        {
            successCallback(false, "");
        }
    }

    // Download the Firefox stub installer
    public IEnumerator DownloadFirefox(IProgress<float> percentDownloaded, Action<bool, string> successCallback)
    {
        // TODO: Currently assumes English. Should we grab user's locale?
        string downloadURL = "https://download.mozilla.org/?product=firefox-stub&os=win&lang=" +
                             CultureInfo.CurrentCulture.Name;
        var webRequest = new UnityWebRequest(downloadURL);
        webRequest.downloadHandler = new DownloadHandlerFile(FirefoxInstallerDownloadPath, false);
        var downloadOperation = webRequest.SendWebRequest();
        while (!downloadOperation.isDone)
        {
            yield return new WaitForSeconds(.25f);
            percentDownloaded.Report(downloadOperation.progress);
        }

        successCallback?.Invoke(string.IsNullOrEmpty(webRequest.error), webRequest.error);
    }

    public void DownloadAndInstallDesktopFirefox(IProgress<float> percentDownloaded,
        Action<bool, string> successCallback)
    {
        StartCoroutine(DownloadFirefox(percentDownloaded, (wasSuccessful, error) =>
        {
            if (wasSuccessful)
            {
                try
                {
                    Process installProcess = new Process();
                    installProcess.StartInfo.FileName = FirefoxInstallerDownloadPath;
                    // Run with admin priviliges
                    installProcess.StartInfo.Verb = "runas";
                    installProcess.Start();
                    successCallback?.Invoke(true, "");
                }
                catch (Exception e)
                {
                    Debug.LogException(e, this);
                    successCallback?.Invoke(false, e.Message);
                }
            }
            else
            {
                successCallback?.Invoke(false, error);
            }
        }));
    }

    private static string GetInstalledVersion(RegistryKey scope, string path)
    {
        using (var key = scope.OpenSubKey(path))
        {
            // First check the value of the (Default) entry, which is non-null for release/debug, but is empty for nightly
            string versionString = key?.GetValue("")?.ToString();
            // Grab the "CurrentVersion" if (Default) entry is null or empty string, which is the case for nightly
            versionString = string.IsNullOrEmpty(versionString)
                ? key?.GetValue("CurrentVersion")?.ToString()
                : versionString;
            return versionString;
        }
    }

    private string FirefoxInstallerDownloadPath =>
        Path.Combine(Application.persistentDataPath, "Firefox Installer.exe");
}