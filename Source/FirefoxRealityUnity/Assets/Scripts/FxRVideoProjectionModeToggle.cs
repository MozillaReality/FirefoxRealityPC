// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019, Mozilla.

using UnityEngine;

[RequireComponent(typeof(FxRToggle))]
public class FxRVideoProjectionModeToggle : MonoBehaviour
{
    [SerializeField] protected FxRVideoProjectionMode.PROJECTION_MODE ProjectionMode;
    [SerializeField] protected FxRVideoController VideoController;

    private FxRToggle Toggle
    {
        get
        {
            if (toggle == null)
            {
                toggle = GetComponent<FxRToggle>();
            }

            return toggle;
        }
    }

    private FxRToggle toggle;

    void OnEnable()
    {
        Toggle.ConfigAsToggleConfig.ToggleValueChangedListener += HandleToggleValueChanged;
        FxRVideoController.OnVideoProjectionModeSwitched += HandleVideoProjectionModeSwitched;
        HandleVideoProjectionModeSwitched(VideoController.VideoProjectionMode);
    }

    private void HandleVideoProjectionModeSwitched(FxRVideoProjectionMode.PROJECTION_MODE newMode)
    {
        Toggle.SetIsOnWithoutNotify(newMode == ProjectionMode);
    }

    void OnDisable()
    {
        Toggle.ConfigAsToggleConfig.ToggleValueChangedListener -= HandleToggleValueChanged;
        FxRVideoController.OnVideoProjectionModeSwitched -= HandleVideoProjectionModeSwitched;
    }

    private void HandleToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            VideoController.SwitchProjectionMode(ProjectionMode);
        }
    }
}