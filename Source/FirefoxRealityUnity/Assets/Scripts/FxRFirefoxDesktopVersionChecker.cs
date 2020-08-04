// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019-2020, Mozilla.

using System;
using System.Collections;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using UnityEngine;
using UnityEngine.Networking;

public class FxRFirefoxDesktopVersionChecker : Singleton<FxRFirefoxDesktopVersionChecker>
{
    private const int MAJOR_RELEASE_REQUIRED_FALLBACK = 74;
    private const string MINOR_RELEASE_REQUIRED_FALLBACK = "0";
    public static readonly string RELEASE_AND_BETA_REGISTRY_PATH = @"SOFTWARE\Mozilla\Mozilla Firefox";
    public static readonly string NIGHTLY_REGISTRY_PATH = @"SOFTWARE\Mozilla\Nightly";

    private bool FirefoxVersionCheckPerformed;
    private bool FirefoxVersionAvailable;
    private bool FirefoxConfigurationRequired;
    private FirefoxInstallationRequirements InstallationRequirements;

    public static readonly string FXR_CONFIGURATION_DIRECTORY = "firefox";

    public class FirefoxVersions
    {
        public string LATEST_FIREFOX_VERSION;
        public string FIREFOX_NIGHTLY;
    }

    public struct FirefoxInstallationRequirements
    {
        public DOWNLOAD_TYPE DownloadType;
        public INSTALLATION_TYPE_REQUIRED InstallationTypeRequired;
        public INSTALLATION_SCOPE InstallationScope;
    }

    public enum DOWNLOAD_TYPE
    {
        STUB,
        RELEASE
    }

    public enum INSTALLATION_TYPE_REQUIRED
    {
        NONE,
        INSTALL_NEW,
        UPDATE_EXISTING
    }

    public enum INSTALLATION_SCOPE
    {
        LOCAL_MACHINE,
        LOCAL_USER
    }

    public static string GetFirefoxDesktopInstallationPath()
    {
        // TODO: Change these to release once release is ready...
        string nightlyVersion = GetInstalledVersion(Registry.LocalMachine, NIGHTLY_REGISTRY_PATH);
        if (!string.IsNullOrEmpty(nightlyVersion))
        {
            return GetInstallationLocation(Registry.LocalMachine,
                NIGHTLY_REGISTRY_PATH); //RELEASE_AND_BETA_REGISTRY_PATH);
        }

        nightlyVersion = GetInstalledVersion(Registry.CurrentUser, NIGHTLY_REGISTRY_PATH);
        if (!string.IsNullOrEmpty(nightlyVersion))
        {
            return GetInstallationLocation(Registry.CurrentUser,
                NIGHTLY_REGISTRY_PATH); //RELEASE_AND_BETA_REGISTRY_PATH);
        }

        return "";
    }

    public void CheckIfFirefoxInstallationOrConfigurationRequired(
        Action<bool, bool, FirefoxInstallationRequirements> updateRequiredResult)
    {
        if (FirefoxVersionCheckPerformed)
        {
            updateRequiredResult?.Invoke(FirefoxVersionAvailable, FirefoxConfigurationRequired,
                InstallationRequirements);
            return;
        }

        StartCoroutine(RetrieveLatestFirefoxVersion((wasSuccessful, versionString) =>
        {
            FirefoxVersionCheckPerformed = true;
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
                FirefoxVersionAvailable = true;
                FirefoxConfigurationRequired = true;
                InstallationRequirements = new FirefoxInstallationRequirements
                {
                    DownloadType = downloadTypeRequired.Value, InstallationScope = installationScope.Value,
                    InstallationTypeRequired = installTypeRequired.Value
                };
            }
            else
            {
                FirefoxVersionAvailable = false;
                FirefoxConfigurationRequired = CheckIfFirefoxConfigurationRequired();
                InstallationRequirements = new FirefoxInstallationRequirements
                {
                    InstallationTypeRequired = INSTALLATION_TYPE_REQUIRED.NONE
                };
            }

            updateRequiredResult?.Invoke(FirefoxVersionAvailable, FirefoxConfigurationRequired,
                InstallationRequirements);
        }));
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

    // Check if Firefox Desktop is installed
    // Logic for installation:
    // * If user has no Nightly or Release version installed, we download and install the stub installer
    // * If the user has an old Release or Nightly version, we download and install the release or nightly installer into the existing install location
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
            if (!string.IsNullOrEmpty(nightlyVersion))
            {
                installationScope = INSTALLATION_SCOPE.LOCAL_USER;
            }
        }

//        string releaseVersion = GetInstalledVersion(Registry.LocalMachine, RELEASE_AND_BETA_REGISTRY_PATH);
//        if (string.IsNullOrEmpty(releaseVersion))
//        {
//            releaseVersion = GetInstalledVersion(Registry.CurrentUser, RELEASE_AND_BETA_REGISTRY_PATH);
//            if (!string.IsNullOrEmpty(releaseVersion))
//            {
//                installationScope = INSTALLATION_SCOPE.LOCAL_USER;
//            }
//        }

//        bool hasReleaseVersion = !string.IsNullOrEmpty(releaseVersion);
//        if (hasReleaseVersion)
//        {
//            if (InstalledVersionNewEnough(releaseVersion, majorVersionRequired, minorVersionRequired))
//            {
//                installationTypeRequired = INSTALLATION_TYPE_REQUIRED.NONE;
//                return;
//            }
//            else
//            {
//                downloadTypeRequired = DOWNLOAD_TYPE.RELEASE;
//            }
//        }

        bool hasNightlyVersion = !string.IsNullOrEmpty(nightlyVersion);
        if (hasNightlyVersion) //&& !hasReleaseVersion)
        {
            if (InstalledVersionNewEnough(nightlyVersion, majorVersionRequired, minorVersionRequired))
            {
                installationTypeRequired = INSTALLATION_TYPE_REQUIRED.NONE;
                return;
            }
            else
            {
                downloadTypeRequired = DOWNLOAD_TYPE.RELEASE;
            }
        }

        if (hasNightlyVersion)
        {
            var registryKey = installationScope == INSTALLATION_SCOPE.LOCAL_USER
                ? Registry.CurrentUser
                : Registry.LocalMachine;

            // var installPath =
            //     GetInstallationLocation(registryKey, NIGHTLY_REGISTRY_PATH); //RELEASE_AND_BETA_REGISTRY_PATH);

            // TODO: What do we do with installs that came from a distribution? Do we treat them differently?
//            if (!Directory.Exists(Path.Combine(installPath, "distribution")))
//            {
            installationTypeRequired = INSTALLATION_TYPE_REQUIRED.UPDATE_EXISTING;
//            }
//            else
//            {
//                // Since we require a miniumum version level, if the distribution is old, we still would need to update it... 
//                installationTypeRequired = INSTALLATION_TYPE_REQUIRED.NONE;
//            }
        }
        else
        {
            downloadTypeRequired = DOWNLOAD_TYPE.STUB;
            installationTypeRequired = INSTALLATION_TYPE_REQUIRED.INSTALL_NEW;
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

    private static string GetInstalledVersion(RegistryKey scope, string path)
    {
        using (var key = scope.OpenSubKey(path))
        {
            // Grab the value of the (Default) entry which contains the unadorned version number, e.g. 69.0, or 71.0a1
            return key?.GetValue("")?.ToString();
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
            // TODO: Change this to release, once release is ready...
            string latestVersion =
                JsonUtility.FromJson<FirefoxVersions>(jsonResposne).FIREFOX_NIGHTLY; //LATEST_FIREFOX_VERSION;
            successCallback(true, latestVersion);
        }
        else
        {
            successCallback(false, "");
        }
    }

    private bool CheckIfFirefoxConfigurationRequired()
    {
        var configurationSourceDirectory =
            Path.Combine(Application.streamingAssetsPath, FXR_CONFIGURATION_DIRECTORY);
        var firefoxDesktopInstallationPath = GetFirefoxDesktopInstallationPath();
        if (FxRUtilityFunctions.DoAllFilesExist(configurationSourceDirectory, firefoxDesktopInstallationPath))
        {
            // No need to copy anything
            // TODO: Check time/date/size/etc?
            return false;
        }

        return true;
    }
}