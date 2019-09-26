using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

public class FxRFirefoxDesktopInstaller : MonoBehaviour
{
    private const bool FORCE_DESKTOP_BROWSER_CHECK = true;

    private const int MAJOR_RELEASE_REQUIRED_FALLBACK = 69;
    private const string MINOR_RELEASE_REQUIRED_FALLBACK = "0";

    private const string NUMBER_OF_TIMES_CHECKED_BROWSER_PREF_KEY = "NUMBER_OF_TIMES_CHECKED_BROWSER_PREF_KEY";
    private const string FXR_VERSION_LAST_CHECKED_BROWSER_PREF_KEY = "FXR_VERSION_LAST_CHECKED_BROWSER_PREF_KEY";

    int NUMBER_OF_TIMES_TO_CHECK_BROWSER = 1;

    // Class to represent JSON downloaded from Firefox latest version service
    private class FirefoxVersions
    {
        public string LATEST_FIREFOX_VERSION;
    }

    private enum DOWNLOAD_TYPE
    {
        STUB,
        RELEASE

        //        , NIGHTLY
    }

    private enum INSTALLATION_TYPE_REQUIRED
    {
        NONE,
        INSTALL_NEW,
        UPDATE_EXISTING
    }

    private enum INSTALLATION_SCOPE
    {
        LOCAL_MACHINE,
        LOCAL_USER
    }

    void Start()
    {
        // TODO: Keep track of whether we have already asked to install

        if (!FORCE_DESKTOP_BROWSER_CHECK)
        {
            int browserChecks = PlayerPrefs.GetInt(NUMBER_OF_TIMES_CHECKED_BROWSER_PREF_KEY, 0);
            string lastBrowserCheckFxRVersion =
                PlayerPrefs.GetString(FXR_VERSION_LAST_CHECKED_BROWSER_PREF_KEY, Application.version);
            PlayerPrefs.SetString(FXR_VERSION_LAST_CHECKED_BROWSER_PREF_KEY, Application.version);

            if (CompareVersions(lastBrowserCheckFxRVersion, Application.version) < 0)
            {
                // User upgraded since we last checked desktop browser, so reset our checks
                PlayerPrefs.SetInt(NUMBER_OF_TIMES_CHECKED_BROWSER_PREF_KEY, 0);
                browserChecks = 0;
            }
            else if (browserChecks >= NUMBER_OF_TIMES_TO_CHECK_BROWSER)
            {
                return;
            }

            PlayerPrefs.SetInt(NUMBER_OF_TIMES_CHECKED_BROWSER_PREF_KEY, browserChecks + 1);
            PlayerPrefs.SetString(FXR_VERSION_LAST_CHECKED_BROWSER_PREF_KEY, Application.version);
            PlayerPrefs.Save();
        }

        StartCoroutine(RetrieveLatestFirefoxVersion((wasSuccessful, versionString) =>
        {
            int releaseMajor = MAJOR_RELEASE_REQUIRED_FALLBACK;
            string releaseMinor = MINOR_RELEASE_REQUIRED_FALLBACK;
            if (wasSuccessful)
            {
                ParseBrowserVersion(versionString, out releaseMajor, out releaseMinor);
            }

            INSTALLATION_TYPE_REQUIRED? installTypeRequired;
            DOWNLOAD_TYPE? downloadTypeRequired;
            INSTALLATION_SCOPE? installationScope;
            DetermineFirefoxDesktopInstallationRequirements(releaseMajor, releaseMinor
                , out installTypeRequired
                , out downloadTypeRequired
                , out installationScope);
            if (installTypeRequired != null && installTypeRequired != INSTALLATION_TYPE_REQUIRED.NONE &&
                downloadTypeRequired != null &&
                installationScope != null)
            {
                InitiateDesktopFirefoxInstall(installTypeRequired.Value, downloadTypeRequired.Value,
                    installationScope.Value);
            }
        }));
    }

    private void InitiateDesktopFirefoxInstall(INSTALLATION_TYPE_REQUIRED installationTypeRequired,
        DOWNLOAD_TYPE downloadType, INSTALLATION_SCOPE installationScope)
    {
        // TODO: Update copy
        // TODO: i18n and l10n
        var dialogTitle = installationTypeRequired == INSTALLATION_TYPE_REQUIRED.INSTALL_NEW
            ? "It appears you don't have Firefox Desktop installed."
            : "There is an update to Firefox Desktop available.";
        var dialogMessage = installationTypeRequired == INSTALLATION_TYPE_REQUIRED.INSTALL_NEW
            ? "Would you like to install Firefox Desktop?"
            : "Would you like to update Firefox Desktop?";
        var dialogButtons = new FxRDialogButton.ButtonConfig[2];
        var updateOrInstall =
            (installationTypeRequired == INSTALLATION_TYPE_REQUIRED.INSTALL_NEW) ? "Install" : "Update";
        dialogButtons[0] = new FxRDialogButton.ButtonConfig(updateOrInstall + " Later", null, Color.gray);
        dialogButtons[1] = new FxRDialogButton.ButtonConfig(updateOrInstall + " Now", () =>
        {
            var removeHeadsetPrompt = FxRDialogController.Instance.CreateDialog();
            removeHeadsetPrompt.Show("Firefox Desktop Installation Started",
                "Please remove your headset to continue the Desktop Firefox install process",
                new FxRDialogButton.ButtonConfig("OK", null));

            // TODO: Put a progress bar in dialog once we allow for full desktop installation
            var progress =
                new Progress<float>(percent => { Debug.Log("Download progress: " + percent.ToString("P1")); });
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
            }, downloadType, installationScope);
        }, Color.blue);

        FxRDialogController.Instance.CreateDialog().Show(dialogTitle, dialogMessage, dialogButtons);
    }

    private static readonly string RELEASE_AND_BETA_REGISTRY_PATH = @"SOFTWARE\Mozilla\Mozilla Firefox";
    private static readonly string NIGHTLY_REGISTRY_PATH = @"SOFTWARE\Mozilla\Nightly";

    // Check if Firefox Desktop is installed
    // Logic for installation:
    // * If user has no Nightly or Release version installed, we download and install the stub installer
    // * If the user has an old Release version, we download and install the release installer into the existing release location
    // * If the user has an up-to-date Release version, or don't have a Release version but have any Nightly version, we don't prompt them to install
    // We'll compare minor version #'s ordinally, as they can contain letters
    private void DetermineFirefoxDesktopInstallationRequirements(int majorVersionRequired,
        string minorVersionRequired, out INSTALLATION_TYPE_REQUIRED? installationTypeRequired,
        out DOWNLOAD_TYPE? downloadTypeRequired, out INSTALLATION_SCOPE? installationScope
    )

    {
        downloadTypeRequired = null;
        installationScope = INSTALLATION_SCOPE.LOCAL_MACHINE;

        string nightlyVersion = GetInstalledVersion(Registry.LocalMachine, NIGHTLY_REGISTRY_PATH);
        if (string.IsNullOrEmpty(nightlyVersion))
        {
            nightlyVersion = GetInstalledVersion(Registry.CurrentUser, NIGHTLY_REGISTRY_PATH);
//            if (!string.IsNullOrEmpty(nightlyVersion))
//            {
//                installationScope = INSTALLATION_SCOPE.LOCAL_USER;
//            }
        }

        string releaseVersion = GetInstalledVersion(Registry.LocalMachine, RELEASE_AND_BETA_REGISTRY_PATH);
        if (string.IsNullOrEmpty(releaseVersion))
        {
            releaseVersion = GetInstalledVersion(Registry.CurrentUser, RELEASE_AND_BETA_REGISTRY_PATH);
            if (!string.IsNullOrEmpty(releaseVersion))
            {
                installationScope = INSTALLATION_SCOPE.LOCAL_USER;
            }
        }

        bool hasReleaseVersion = !string.IsNullOrEmpty(releaseVersion);
        if (hasReleaseVersion)
        {
            if (InstalledVersionNewEnough(releaseVersion, majorVersionRequired, minorVersionRequired))
            {
                installationTypeRequired = INSTALLATION_TYPE_REQUIRED.NONE;
                return;
            }
            else
            {
                downloadTypeRequired = DOWNLOAD_TYPE.RELEASE;
            }
        }

        bool hasNightlyVersion = !string.IsNullOrEmpty(nightlyVersion);
        // If user has Nightly installed, and don't have Release installed, we won't prompt them to download and install
        if (hasNightlyVersion && !hasReleaseVersion)
        {
            installationTypeRequired = INSTALLATION_TYPE_REQUIRED.NONE;
            return;
//            if (InstalledVersionNewEnough(nightlyVersion, majorVersionRequired, minorVersionRequired))
//            {
//                return INSTALLATION_TYPE_REQUIRED.NONE;
//            }
//            else if (!hasReleaseVersion)
//            {
//                downloadTypeRequired = DOWNLOAD_TYPE.NIGHTLY;
//            }
        }

        if (hasReleaseVersion) // || hasNightlyVersion)
        {
            installationTypeRequired = INSTALLATION_TYPE_REQUIRED.UPDATE_EXISTING;
        }
        else
        {
            downloadTypeRequired = DOWNLOAD_TYPE.STUB;
            installationTypeRequired = INSTALLATION_TYPE_REQUIRED.INSTALL_NEW;
        }
    }

    private bool InstalledVersionNewEnough(string installedVersion, int majorVersionRequired,
        string minorVersionRequired)
    {
        ParseBrowserVersion(installedVersion, out var releaseMajor, out var releaseMinor);

        if (releaseMajor > majorVersionRequired
            || (releaseMajor == majorVersionRequired
                && string.CompareOrdinal(minorVersionRequired, releaseMinor) <= 0)
        )
        {
            return true;
        }

        return false;
    }

    private int CompareVersions(string versionA, string versionB)
    {
        if (versionA.Equals(versionB))
        {
            return 0;
        }

        ParseBrowserVersion(versionA, out var majorA, out var minorA);
        ParseBrowserVersion(versionB, out var majorB, out var minorB);

        if (majorA < majorB)
        {
            return -1;
        }
        else if (majorA > majorB)
        {
            return 1;
        }
        else
        {
            return string.CompareOrdinal(versionA, versionB);
        }
    }

    private static void ParseBrowserVersion(string versionString, out int releaseMajor, out string releaseMinor)
    {
        string[] majorMinor = versionString.Split(new char[] {'.'});
        releaseMajor = int.Parse(majorMinor[0]);
        releaseMinor = string.Join(".", majorMinor.Skip(1).Take(majorMinor.Length - 1).ToArray());
    }

    private IEnumerator RetrieveLatestFirefoxVersion(Action<bool, string> successCallback)
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
    private IEnumerator DownloadFirefox(IProgress<float> percentDownloaded, Action<bool, string> successCallback,
        DOWNLOAD_TYPE downloadType = DOWNLOAD_TYPE.STUB)
    {
        string downloadURL = null;

        switch (downloadType)
        {
            case DOWNLOAD_TYPE.STUB:
                downloadURL = "https://download.mozilla.org/?product=firefox-stub&os=win&lang=" +
                              CultureInfo.CurrentCulture.Name;
                break;
            case DOWNLOAD_TYPE.RELEASE:
                downloadURL = "https://download.mozilla.org/?product=firefox-latest-ssl&os=win64&lang=" +
                              CultureInfo.CurrentCulture.Name;
                break;
//            case DOWNLOAD_TYPE.NIGHTLY:
//                downloadURL = "https://download.mozilla.org/?product=firefox-nightly-latest-l10n-ssl&os=win64&lang=" +
//                             CultureInfo.CurrentCulture.Name;
//                break;
        }

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

    private void DownloadAndInstallDesktopFirefox(IProgress<float> percentDownloaded,
        Action<bool, string> successCallback, DOWNLOAD_TYPE downloadType = DOWNLOAD_TYPE.STUB,
        INSTALLATION_SCOPE installationScope = INSTALLATION_SCOPE.LOCAL_MACHINE)
    {
        StartCoroutine(DownloadFirefox(percentDownloaded, (wasSuccessful, error) =>
        {
            if (wasSuccessful)
            {
                try
                {
                    Process installProcess = new Process();
                    installProcess.StartInfo.FileName = FirefoxInstallerDownloadPath;

                    var registryKey = installationScope == INSTALLATION_SCOPE.LOCAL_USER
                        ? Registry.CurrentUser
                        : Registry.LocalMachine;
                    string installPath = "";
//                    if (downloadType == DOWNLOAD_TYPE.NIGHTLY)
//                    {
//                        installPath = GetInstallationLocation(
//                            registryKey, NIGHTLY_REGISTRY_PATH);
//                    }
//                    else 
                    if (downloadType == DOWNLOAD_TYPE.RELEASE)
                    {
                        installPath = GetInstallationLocation(
                            registryKey, RELEASE_AND_BETA_REGISTRY_PATH);
                    }

                    // Run with admin privileges
                    installProcess.StartInfo.Verb = "runas";
                    if (!string.IsNullOrEmpty(installPath))
                    {
                        installProcess.StartInfo.Arguments = "/InstallDirectoryPath=" + installPath;
                    }

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
        }, downloadType));
    }

    private static string GetInstalledVersion(RegistryKey scope, string path)
    {
        using (var key = scope.OpenSubKey(path))
        {
            // Grab the value of the (Default) entry which contains the unadorned version number, e.g. 69.0, or 71.0a1
            return key?.GetValue("")?.ToString();
        }
    }

    private static string GetInstallationLocation(RegistryKey scope, string path)
    {
        using (var key = scope.OpenSubKey(path))
        {
            string versionString = key?.GetValue("CurrentVersion")?.ToString();
            if (!string.IsNullOrEmpty(versionString))
            {
                using (var versionKey = scope.OpenSubKey(path + "\\" + versionString + "\\Main"))
                {
                    return versionKey?.GetValue("Install Directory")?.ToString();
                }
            }
        }

        return null;
    }

    private string FirefoxInstallerDownloadPath =>
        Path.Combine(Application.persistentDataPath, "Firefox Installer.exe");
}