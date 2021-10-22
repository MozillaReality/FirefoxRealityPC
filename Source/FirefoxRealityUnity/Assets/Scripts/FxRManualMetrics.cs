// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019-2020, Mozilla.

using Mozilla.Glean.Private;
using System;

public sealed class FxRInternalMetricsOuter
{
    // Initialize the singleton using the `Lazy` facilities.
    private static readonly Lazy<FxRInternalMetricsOuter>
      lazy = new Lazy<FxRInternalMetricsOuter>(() => new FxRInternalMetricsOuter());
    public static FxRInternalMetricsOuter FxRInternalMetrics => lazy.Value;

    // Private constructor to disallow instantiation from external callers.
    private FxRInternalMetricsOuter() { }

    private readonly Lazy<PingType<NoReasonCodes>> launchPingLazy = new Lazy<PingType<NoReasonCodes>>(() => new PingType<NoReasonCodes>(
            name: "launch",
            includeClientId: true,
            sendIfEmpty: false,
            reasonCodes: null
        ));
    public PingType<NoReasonCodes> launchPing => launchPingLazy.Value;
}
