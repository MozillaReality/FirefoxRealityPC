// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019-2020, Mozilla.

using System;
using static Mozilla.Glean.Glean;
using Mozilla.Glean.Private;
using Mozilla.Glean;
using static FxRInternalMetricsOuter;
using UnityEngine;


// This type is based on which distribution channel
// is FxR downloaded.
public enum DistributionChannelType
{
    HTC
}

public enum EntryMethod
{
    SYSTEM_BUTTON,
    LIBRARY,
    GAZE
}

// A wrapper of using Mozilla Glean telemetry library.
// In the current usage of Firefox Reality PC, we only
// define pings that are collected at the launch time.
// Then, submit pings before this application is quit.
public sealed class FxRTelemetryService
{
    // Initialize the singleton using the `Lazy` facilities.
    private static readonly Lazy<FxRTelemetryService>
      lazy = new Lazy<FxRTelemetryService>(() => new FxRTelemetryService());
    public static FxRTelemetryService FxRTelemetryServiceInstance => lazy.Value;

    public void Initialize(bool aUploadEnabled)
    {
        string mode;
#if DEBUG
        mode = "debug";
#else
        mode = "release";
#endif
        // Application.identifier can't be used as `applicationId`.
        // It is empty in Windows standalone applications, and it causes
        // failed ping uploads.
        GleanInstance.Initialize(applicationId: "org.mozilla.firefoxreality",
                                 Application.version,
                                 aUploadEnabled,
                                 new Configuration(
                                 channel: mode,
                                 buildId: String.IsNullOrEmpty(Application.buildGUID) ? null : Application.buildGUID),
                                 "data");
    }

    public void SetEnabled(bool aEnabled)
    {
        GleanInstance.SetUploadEnabled(aEnabled);
    }

    public void SetDistributionChannel(DistributionChannelType aChannel)
    {
        FxRInternalMetrics.distributionChannel.Set(aChannel.ToString().ToLower());
    }

    public void SetInstallFrom(FxRFirefoxDesktopInstallation.InstallationType aInstallFrom)
    {
        FxRInternalMetrics.installFrom.Set(aInstallFrom.ToString().ToLower());
    }

    public void SetEntryMethod(EntryMethod aEntryMethod)
    {
        FxRInternalMetrics.entryMethod.Set(aEntryMethod.ToString().ToLower());
    }

    public void SubmitLaunchPings()
    {
        FxRInternalMetrics.launchPing.Submit();
    }
}
