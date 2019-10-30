// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019, Mozilla.

﻿using System.Collections.Generic;
using UnityEngine;

public class FxRBrowserModeVisibilityManager : MonoBehaviour
{
    public List<FxRController.FXR_BROWSING_MODE> ApplicableBrowserModes;
    public List<GameObject> GameObjectsToEnable;

    private void OnEnable()
    {
        HandleBrowserModeChanged(FxRController.CurrentBrowsingMode);
        FxRController.OnBrowsingModeChanged += HandleBrowserModeChanged;
    }

    private void OnDisable()
    {
        FxRController.OnBrowsingModeChanged -= HandleBrowserModeChanged;
    }

    private void HandleBrowserModeChanged(FxRController.FXR_BROWSING_MODE browsingMode)
    {
        bool shouldBeActive = ApplicableBrowserModes.Contains(browsingMode);
        foreach (var go in GameObjectsToEnable)
        {
            go.SetActive(shouldBeActive);
        }
    }
}