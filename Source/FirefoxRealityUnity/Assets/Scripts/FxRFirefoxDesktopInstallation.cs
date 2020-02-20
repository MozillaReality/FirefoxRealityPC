// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019, Mozilla.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

public class FxRFirefoxDesktopInstallation : MonoBehaviour
{
    public delegate void InstallationProcessComplete();

    public static InstallationProcessComplete OnInstallationProcessComplete;

    [SerializeField] protected Sprite FirefoxIcon;

    private const bool FORCE_DESKTOP_BROWSER_CHECK = true;

    private const int MAJOR_RELEASE_REQUIRED_FALLBACK = 71;
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
        FxRFirefoxRealityVersionChecker.Instance.CheckForNewFirefoxRealityPC(
            (newFirefoxRealityPCVersionAvailable, serverVersionInfo) =>
            {
                if (newFirefoxRealityPCVersionAvailable)
                {
                    // Prompt user if new version on server version of JSON file
                    // TODO: i18n and l10n
                    var dialogTitle =
                        FxRLocalizedStringsLoader.GetApplicationString("fxr_update_available_dialog_title");
                    var dialogMessage = string.Format(
                        FxRLocalizedStringsLoader.GetApplicationString("fxr_update_available_dialog_message"),
                        string.IsNullOrEmpty(serverVersionInfo.LATEST_FXR_PC_VERSION)
                            ? ""
                            : serverVersionInfo.LATEST_FXR_PC_VERSION
                        , string.IsNullOrEmpty(serverVersionInfo.LATEST_FXR_PC_RELEASE_NOTE_HIGHLIGHTS) ? "" : serverVersionInfo.LATEST_FXR_PC_RELEASE_NOTE_HIGHLIGHTS);
                    var dialogButtons = new FxRButton.ButtonConfig[2];
                    dialogButtons[0] = new FxRButton.ButtonConfig(
                        FxRLocalizedStringsLoader.GetApplicationString(
                            "fxr_update_available_dialog_update_later_button"),
                        () =>
                        {
                            var updateLaterDialog = FxRDialogController.Instance.CreateDialog();
                            updateLaterDialog.Show(
                                FxRLocalizedStringsLoader.GetApplicationString(
                                    "fxr_update_available_update_later_response_dialog_title"),
                                FxRLocalizedStringsLoader.GetApplicationString(
                                    "fxr_update_available_update_later_response_dialog_message"),
                                FirefoxIcon,
                                new FxRButton.ButtonConfig(
                                    FxRLocalizedStringsLoader.GetApplicationString("ok_button"),
                                    () =>
                                    {
                                        // TODO: Uncomment following, if we want to encourage people to download Desktop
                                        // StartFirefoxDesktopUpdateProcess();
                                        NotifyInstallationComplete();
                                    },
                                    FxRConfiguration.Instance.ColorPalette
                                        .NormalBrowsingSecondaryDialogButtonColors));
                        },
                        FxRConfiguration.Instance.ColorPalette.NormalBrowsingSecondaryDialogButtonColors);
                    dialogButtons[1] = new FxRButton.ButtonConfig(
                        FxRLocalizedStringsLoader.GetApplicationString(
                            "fxr_update_available_dialog_update_now_button"),
                        () =>
                        {
                            // Open up URL to download new version
                            Application.OpenURL(serverVersionInfo.LATEST_FXR_PC_URL);
                            var removeHeadsetPrompt = FxRDialogController.Instance.CreateDialog();
                            removeHeadsetPrompt.Show(
                                FxRLocalizedStringsLoader.GetApplicationString(
                                    "fxr_update_available_update_now_response_dialog_title"),
                                FxRLocalizedStringsLoader.GetApplicationString(
                                    "fxr_update_available_update_now_response_dialog_message"),
                                FirefoxIcon,
                                new FxRButton.ButtonConfig(
                                    FxRLocalizedStringsLoader.GetApplicationString("ok_button"),
                                    () =>
                                    {
                                        FxRController.Quit(0);
                                    },
                                    FxRConfiguration.Instance.ColorPalette
                                        .NormalBrowsingSecondaryDialogButtonColors));
                        },
                        FxRConfiguration.Instance.ColorPalette.NormalBrowsingPrimaryDialogButtonColors);

                    FxRDialogController.Instance.CreateDialog()
                        .Show(dialogTitle, dialogMessage, FirefoxIcon, dialogButtons);
                }
                else
                {
                    NotifyInstallationComplete();
                }
            });
    }

    private void StartFirefoxDesktopUpdateProcess()
    {
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
            else
            {
                NotifyInstallationComplete();
            }
        }));
    }

    private int retryCount = 0;

    private void InitiateDesktopFirefoxInstall(INSTALLATION_TYPE_REQUIRED installationTypeRequired,
        DOWNLOAD_TYPE downloadType, INSTALLATION_SCOPE installationScope)
    {
// TODO: Update copy
// TODO: i18n and l10n
        var dialogTitle = installationTypeRequired == INSTALLATION_TYPE_REQUIRED.INSTALL_NEW
            ? FxRLocalizedStringsLoader.GetApplicationString("desktop_installation_new_install_dialog_title")
            : FxRLocalizedStringsLoader.GetApplicationString("desktop_installation_update_dialog_title");

        var dialogMessage = installationTypeRequired == INSTALLATION_TYPE_REQUIRED.INSTALL_NEW
            ? FxRLocalizedStringsLoader.GetApplicationString("desktop_installation_new_install_dialog_message")
            : FxRLocalizedStringsLoader.GetApplicationString("desktop_installation_update_dialog_message");

        var dialogButtons = new FxRButton.ButtonConfig[2];

        var updateOrInstallLater = (installationTypeRequired == INSTALLATION_TYPE_REQUIRED.INSTALL_NEW)
            ? FxRLocalizedStringsLoader.GetApplicationString("desktop_installation_install_later_button")
            : FxRLocalizedStringsLoader.GetApplicationString("desktop_installation_update_later_button");

        var updateOrInstallNow = (installationTypeRequired == INSTALLATION_TYPE_REQUIRED.INSTALL_NEW)
            ? FxRLocalizedStringsLoader.GetApplicationString("desktop_installation_install_now_button")
            : FxRLocalizedStringsLoader.GetApplicationString("desktop_installation_update_now_button");

        dialogButtons[0] = new FxRButton.ButtonConfig(updateOrInstallLater,
            () => { NotifyInstallationComplete(); },
            FxRConfiguration.Instance.ColorPalette.NormalBrowsingSecondaryDialogButtonColors);
        dialogButtons[1] = new FxRButton.ButtonConfig(updateOrInstallNow,
            () => { ContinueDesktopFirefoxInstall(installationTypeRequired, downloadType, installationScope); },
            FxRConfiguration.Instance.ColorPalette.NormalBrowsingPrimaryDialogButtonColors);
        FxRDialogController.Instance.CreateDialog().Show(dialogTitle, dialogMessage, FirefoxIcon, dialogButtons);
    }

    private void ContinueDesktopFirefoxInstall(INSTALLATION_TYPE_REQUIRED installationTypeRequired,
        DOWNLOAD_TYPE downloadType, INSTALLATION_SCOPE installationScope)
    {
        FxRDialogBox downloadProgressDialog = FxRDialogController.Instance.CreateDialog();

        var dialogTitle = installationTypeRequired == INSTALLATION_TYPE_REQUIRED.INSTALL_NEW
            ? FxRLocalizedStringsLoader.GetApplicationString("desktop_installation_install_started_dialog_title")
            : FxRLocalizedStringsLoader.GetApplicationString("desktop_installation_update_started_dialog_title");

        var dialogMessage = installationTypeRequired == INSTALLATION_TYPE_REQUIRED.INSTALL_NEW
            ? FxRLocalizedStringsLoader.GetApplicationString("desktop_installation_install_started_dialog_message")
            : FxRLocalizedStringsLoader.GetApplicationString("desktop_installation_update_started_dialog_message");

        downloadProgressDialog.Show(dialogTitle, dialogMessage, FirefoxIcon
            , new FxRButton.ButtonConfig(FxRLocalizedStringsLoader.GetApplicationString("ok_button")
                , () => { NotifyInstallationComplete(); }
                , FxRConfiguration.Instance.ColorPalette.NormalBrowsingSecondaryDialogButtonColors));

//                    , new FxRButton.ButtonConfig("Cancel",
//                        () => { downloadCancelled = true; }, FxRConfiguration.Instance.ColorPalette.NormalBrowsingSecondaryDialogButtonColors));
        var progress =
            new Progress<float>(zeroToOne =>
            {
                if (downloadProgressDialog == null) return;
//                        if (!Mathf.Approximately(zeroToOne, 1f))
//                        {
//                            downloadProgressDialog.ShowProgress(zeroToOne);
//                        }
//                        else
//                        {
//                            downloadProgressDialog.Close();
//                            var removeHeadsetPrompt = FxRDialogController.Instance.CreateDialog();
//                            removeHeadsetPrompt.Show("Firefox Desktop Installation Started",
//                                "Please remove your headset to continue the Desktop Firefox install process",
//                                FirefoxIcon,
//                                new FxRButton.ButtonConfig(FxRLocalizedStringsLoader.GetApplicationString("ok_button"), null, FxRConfiguration.Instance.ColorPalette.NormalBrowsingSecondaryDialogButtonColors));
//                        }
                Debug.Log("Download progress: " + zeroToOne.ToString("P1"));
            });

        DownloadAndInstallDesktopFirefox(progress, (wasSuccessful, error, wasCancelled) =>
        {
            if (wasCancelled)
            {
                Debug.Log("Firefox Desktop download cancelled");
                NotifyInstallationComplete();
                return;
            }

            if (wasSuccessful)
            {
                if (downloadProgressDialog != null)
                {
                    var downloadProgressDialogTitle = installationTypeRequired == INSTALLATION_TYPE_REQUIRED.INSTALL_NEW
                        ? FxRLocalizedStringsLoader.GetApplicationString(
                            "desktop_installation_install_finished_dialog_title")
                        : FxRLocalizedStringsLoader.GetApplicationString(
                            "desktop_installation_update_finished_dialog_title");

                    var downloadProgressDialogMessage =
                        installationTypeRequired == INSTALLATION_TYPE_REQUIRED.INSTALL_NEW
                            ? FxRLocalizedStringsLoader.GetApplicationString(
                                "desktop_installation_install_finished_dialog_message")
                            : FxRLocalizedStringsLoader.GetApplicationString(
                                "desktop_installation_update_finished_dialog_message");

                    downloadProgressDialog.UpdateText(downloadProgressDialogTitle, downloadProgressDialogMessage);
                }

                NotifyInstallationComplete();
            }
            else
            {
                if (downloadProgressDialog != null)
                {
                    downloadProgressDialog.Close();
                }

                var installationErrorDialog = FxRDialogController.Instance.CreateDialog();
                string installationErrorTitle =
                    installationTypeRequired == INSTALLATION_TYPE_REQUIRED.INSTALL_NEW
                        ? FxRLocalizedStringsLoader.GetApplicationString(
                            "desktop_installation_install_error_dialog_title")
                        : FxRLocalizedStringsLoader.GetApplicationString(
                            "desktop_installation_update_error_dialog_title");
                var okButton = new FxRButton.ButtonConfig(FxRLocalizedStringsLoader.GetApplicationString("ok_button"),
                    () => { NotifyInstallationComplete(); },
                    FxRConfiguration.Instance.ColorPalette.NormalBrowsingSecondaryDialogButtonColors);
                if (retryCount > 0)
                {
                    installationErrorDialog.Show(installationErrorTitle, error, FirefoxIcon, okButton);
                }
                else
                {
                    var retryButton = new FxRButton.ButtonConfig(
                        FxRLocalizedStringsLoader.GetApplicationString("desktop_installation_retry_button"),
                        () =>
                        {
                            retryCount++;
                            ContinueDesktopFirefoxInstall(installationTypeRequired, downloadType,
                                installationScope);
                        },
                        FxRConfiguration.Instance.ColorPalette.NormalBrowsingSecondaryDialogButtonColors);
                    installationErrorDialog.Show(installationErrorTitle, error, FirefoxIcon, retryButton,
                        okButton);
                }
            }
        }, downloadType, installationScope);
    }

    private static readonly string RELEASE_AND_BETA_REGISTRY_PATH = @"SOFTWARE\Mozilla\Mozilla Firefox";
    private static readonly string NIGHTLY_REGISTRY_PATH = @"SOFTWARE\Mozilla\Nightly";
    private bool downloadCancelled;
    private long lastDownloadResponseCode;

    private static readonly string STUB_INSTALLER_BASE_URL =
        "https://download.mozilla.org/?product=partner-firefox-release-firefoxreality-ffreality-htc-001-stub&os=win64&lang=";

    private static readonly string UPGRADE_INSTALLER_BASE_URL =
        "https://download.mozilla.org/?product=partner-firefox-release-firefoxreality-ffreality-htc-up-001-latest&os=win64&lang=";

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
            var registryKey = installationScope == INSTALLATION_SCOPE.LOCAL_USER
                ? Registry.CurrentUser
                : Registry.LocalMachine;

            var installPath = GetInstallationLocation(registryKey, RELEASE_AND_BETA_REGISTRY_PATH);

            if (!Directory.Exists(Path.Combine(installPath, "distribution")))
            {
                installationTypeRequired = INSTALLATION_TYPE_REQUIRED.UPDATE_EXISTING;
            }
            else
            {
                installationTypeRequired = INSTALLATION_TYPE_REQUIRED.NONE;
            }
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
        try

        {
            releaseMajor = int.Parse(majorMinor[0]);
        }
        catch (FormatException e)
        {
            // Shouldn't happen, but in the event it does, we'll catch the exception and assume we need an update...
            releaseMajor = 0;
        }

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
    private IEnumerator DownloadFirefox(IProgress<float> percentDownloaded,
        Action<bool, string, bool> successCallback // <was successful, error, was cancelled>
        , DOWNLOAD_TYPE downloadType = DOWNLOAD_TYPE.STUB)
    {
        string downloadURL = null;
        switch (downloadType)
        {
            case DOWNLOAD_TYPE.STUB:
                downloadURL = STUB_INSTALLER_BASE_URL + CultureStringTwoSegmentsOnly;
                break;
            case DOWNLOAD_TYPE.RELEASE:
                downloadURL = UPGRADE_INSTALLER_BASE_URL + CultureStringTwoSegmentsOnly;
                break;
//            case DOWNLOAD_TYPE.NIGHTLY:
//                downloadURL = "https://download.mozilla.org/?product=firefox-nightly-latest-l10n-ssl&os=win64&lang=" +
//                             CultureInfo.CurrentCulture.Name;
//                break;
        }

        yield return AttemptDownload(percentDownloaded, successCallback, downloadURL);
        if (!downloadCancelled && lastDownloadResponseCode == 404)
        {
            switch (downloadType)
            {
                case DOWNLOAD_TYPE.STUB:
                    downloadURL = STUB_INSTALLER_BASE_URL + "en-US";
                    break;
                case DOWNLOAD_TYPE.RELEASE:
                    downloadURL = UPGRADE_INSTALLER_BASE_URL + "en-US";
                    break;
            }

            yield return AttemptDownload(percentDownloaded, successCallback, downloadURL);
            // This should never happen for en-US version, but just in case...
            if (lastDownloadResponseCode == 404)
            {
                Debug.LogError("Received a 404 attempting to download.");
                successCallback?.Invoke(false, "Download failed.", false);
            }
        }
    }

    private IEnumerator AttemptDownload(IProgress<float> percentDownloaded, Action<bool, string, bool> successCallback,
        string downloadURL)
    {
        var webRequest = new UnityWebRequest(downloadURL);
        webRequest.downloadHandler = new DownloadHandlerFile(FirefoxInstallerDownloadPath, false);
        downloadCancelled = false;

        var downloadOperation = webRequest.SendWebRequest();
        while (!downloadOperation.isDone && !downloadCancelled)
        {
            yield return new WaitForSeconds(.25f);
            percentDownloaded.Report(downloadOperation.progress);
        }

        lastDownloadResponseCode = downloadOperation.webRequest.responseCode;
        if (downloadCancelled)
        {
            webRequest.Abort();
            successCallback?.Invoke(true, "", true);
        }

        else
        {
            if (lastDownloadResponseCode != 404)
            {
                successCallback?.Invoke(string.IsNullOrEmpty(webRequest.error),
                    !string.IsNullOrEmpty(webRequest.error) ? "Download failed." : "", false);
            }
        }
    }

    private void DownloadAndInstallDesktopFirefox(IProgress<float> percentDownloaded,
        Action<bool, string, bool> successCallback, DOWNLOAD_TYPE downloadType = DOWNLOAD_TYPE.STUB,
        INSTALLATION_SCOPE installationScope = INSTALLATION_SCOPE.LOCAL_MACHINE)
    {
        StartCoroutine(DownloadFirefox(percentDownloaded, (wasSuccessful, error, wasCancelled) =>
        {
            if (!wasCancelled && wasSuccessful)
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

                    StartCoroutine(LaunchInstallationProcess(installProcess, successCallback));
                }
                catch (Exception e)
                {
                    Debug.LogException(e, this);
                    successCallback?.Invoke(false, "Installation failed.", false);
                }
            }
            else if (wasCancelled)
            {
                successCallback?.Invoke(true, "", true);
            }
            else
            {
                successCallback?.Invoke(false, error, false);
            }
        }, downloadType));
    }

    private IEnumerator LaunchInstallationProcess(Process installProcess, Action<bool, string, bool> successCallback)
    {
        installProcess.Start();
        while (!installProcess.HasExited)
        {
            yield return new WaitForEndOfFrame();
        }

        successCallback?.Invoke(installProcess.ExitCode == 0,
            installProcess.ExitCode != 0 ? "Installation failed." : "", false);
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

    private string CultureStringTwoSegmentsOnly
    {
        get
        {
            var cultureSegments = CultureInfo.CurrentCulture.Name.Split('-');
            if (cultureSegments.Length > 1)
            {
                return cultureSegments[0] + "-" + cultureSegments[1];
            }

            return CultureInfo.CurrentCulture.Name;
        }
    }

    private bool installationCompleteNotificationSent;

    private void NotifyInstallationComplete()
    {
        if (!installationCompleteNotificationSent)
        {
            OnInstallationProcessComplete?.Invoke();
        }

        installationCompleteNotificationSent = true;
    }
}