using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Win32;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

public class FxRFirefoxDesktopInstaller : MonoBehaviour
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
        // TODO: Keep track of whether we have already asked to install
        StartCoroutine(RetrieveLatestFirefoxVersion((wasSuccessful, versionString) =>
        {
            if (wasSuccessful)
            {
                string[] majorMinor = versionString.Split('.');
                int releaseMajor = int.Parse(majorMinor[0]);
                string releaseMinor = "";
                for (int i = 1; i < majorMinor.Length; i++)
                {
                    releaseMinor += majorMinor[i];
                }

                var installTypeRequired = IsFirefoxDesktopInstallationRequired(releaseMajor, releaseMinor);
                if (installTypeRequired == INSTALLATION_TYPE_REQUIRED.INSTALL_NEW
                    // TODO: Handle update differently from new install...
                    || installTypeRequired == INSTALLATION_TYPE_REQUIRED.UPDATE_EXISTING)
                {
                    InitiateDesktopFirefoxInstall(installTypeRequired);
                }
            }
            else
            {
                // Fallback - couldn't retrieve latest from REST call
                // TODO: DO we want a fallback, or just fail silently and try again another time?
                var installTypeRequired = IsFirefoxDesktopInstallationRequired(69, "0");
                if (installTypeRequired == INSTALLATION_TYPE_REQUIRED.INSTALL_NEW
                    // TODO: Handle update differently from new install...
                    || installTypeRequired == INSTALLATION_TYPE_REQUIRED.UPDATE_EXISTING)
                {
                    InitiateDesktopFirefoxInstall(installTypeRequired);
                }
            }
        }));
    }

    private void InitiateDesktopFirefoxInstall(INSTALLATION_TYPE_REQUIRED installationTypeRequired)
    {
        // TODO: Update copy
        var dialogTitle = installationTypeRequired == INSTALLATION_TYPE_REQUIRED.INSTALL_NEW
            ? "It appears you don't have Firefox Desktop installed."
            : "There is an update to Firefox Desktop available.";
        var dialogMessage = installationTypeRequired == INSTALLATION_TYPE_REQUIRED.INSTALL_NEW
            ? "Would you like to install Firefox Desktop?"
            : "Would you like to update Firefox Desktop?";
        var dialogButtons = new FxRDialogButton.ButtonConfig[2];
        var updateOrInstall = (installationTypeRequired == INSTALLATION_TYPE_REQUIRED.INSTALL_NEW) ? "Install" : "Update";
        dialogButtons[0] = new FxRDialogButton.ButtonConfig(updateOrInstall + " Later", null, Color.gray);
        dialogButtons[1] = new FxRDialogButton.ButtonConfig(updateOrInstall + " Now", () =>
        {
            var removeHeadsetPrompt = FxRDialogController.Instance.CreateDialog();
            removeHeadsetPrompt.Show("Firefox Desktop Installation Started", "Please remove your headset to continue the Desktop Firefox install process", new FxRDialogButton.ButtonConfig("OK", null));
            
            // TODO: Put a progress bar in dialog once we allow for full desktop installation
            var progress = new Progress<float>(percent => { Debug.Log("Download progress: " + percent.ToString("P1")); });
            DownloadAndInstallDesktopFirefox(progress, (wasSuccessful, error) =>
            { 
                if (wasSuccessful)
                {
                    Debug.Log("Firefox Desktop installation successfully launched");
                }
                else
                {
                    Debug.Log("Firefox Desktop was not successfully installed. Error: " + error);
                }
            });
        }, Color.blue);
        
        FxRDialogController.Instance.CreateDialog().Show(dialogTitle, dialogMessage, dialogButtons);
    }

    public enum INSTALLATION_TYPE_REQUIRED
    {
        NONE,
        INSTALL_NEW,
        UPDATE_EXISTING
    }

    // Check if Firefox Desktop is installed
    // We'll compare minor version #'s ordinally, as they can contain letters
    public INSTALLATION_TYPE_REQUIRED IsFirefoxDesktopInstallationRequired(int majorVersionRequired,
        string minorVersionRequired)
    {
        string releaseAndBetaPath = @"SOFTWARE\Mozilla\Mozilla Firefox";
        string releaseVersion = GetInstalledVersion(Registry.LocalMachine, releaseAndBetaPath);
        releaseVersion = string.IsNullOrEmpty(releaseVersion)
            ? GetInstalledVersion(Registry.CurrentUser, releaseAndBetaPath)
            : releaseVersion;

        string nightlyPath = @"SOFTWARE\Mozilla\Nightly";
        string nightlyVersion = GetInstalledVersion(Registry.LocalMachine, nightlyPath);
        nightlyVersion = string.IsNullOrEmpty(nightlyVersion)
            ? GetInstalledVersion(Registry.CurrentUser, nightlyPath)
            : nightlyVersion;

        bool hasReleaseVersion = !string.IsNullOrEmpty(releaseVersion);
        if (hasReleaseVersion)
        {
            if (InstalledVersionNewEnough(releaseVersion, majorVersionRequired, minorVersionRequired))
                return INSTALLATION_TYPE_REQUIRED.NONE;
        }

        bool hasNightlyVersion = !string.IsNullOrEmpty(nightlyVersion);
        if (hasNightlyVersion)
        {
            if (InstalledVersionNewEnough(nightlyVersion, majorVersionRequired, minorVersionRequired))
                return INSTALLATION_TYPE_REQUIRED.NONE;
        }

        return (hasReleaseVersion || hasNightlyVersion)
            ? INSTALLATION_TYPE_REQUIRED.UPDATE_EXISTING
            : INSTALLATION_TYPE_REQUIRED.INSTALL_NEW;
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
                    // TODO: Do we want to have this process run in a co-routine so we can wait for it to exit? i.e. Do we care about the exit status?
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
            // Grab the value of the (Default) entry which contains the unadorned version number, e.g. 69.0, or 71.0a1
            return key?.GetValue("")?.ToString();
            ;
        }
    }

    private string FirefoxInstallerDownloadPath =>
        Path.Combine(Application.persistentDataPath, "Firefox Installer.exe");
}