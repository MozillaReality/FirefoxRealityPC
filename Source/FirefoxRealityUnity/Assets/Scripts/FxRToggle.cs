// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019, Mozilla.

﻿using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class FxRToggle : FxRSelectable
{
    [SerializeField] protected FxRButtonLogicalColorConfig LogicalToggleOnColorConfig;
    [SerializeField] protected Sprite ToggleOnIconSprite;
    [SerializeField] protected Sprite ToggleOffIconSprite;

    [Serializable]
    public class ToggleConfig : SelectableConfig
    {
        [HideInInspector] public UnityAction<bool> ToggleValueChangedListener;

        public ToggleConfig(string label, UnityAction<bool> toggleValueChangedListener,
            FxRButtonLogicalColorConfig colorConfig) : base(
            label, colorConfig)
        {
            ToggleValueChangedListener = toggleValueChangedListener;
        }

        public ToggleConfig(SelectableConfig config) : this(config.ButtonLabel, null, config.LogicialColors)
        {
        }
    }

    public ToggleConfig ConfigAsToggleConfig => (ToggleConfig) Config;
    
    public override SelectableConfig Config
    {
        set
        {
            base.Config = value;
            if (ConfigAsToggleConfig.ToggleValueChangedListener != null)
            {
                ToggleButton.onValueChanged.RemoveAllListeners();
                ToggleButton.onValueChanged.AddListener(ConfigAsToggleConfig.ToggleValueChangedListener);
            }

        }
    }

    protected Toggle ToggleButton => (Toggle) Selectable;
    private FxRButtonLogicalColorConfig LogicalToggleOffColorConfig;
    
    protected override void OnEnable()
    {
        ToggleButton.onValueChanged.AddListener(HandleToggleValueChanged);

        if (Config != null)
        {
            Config = new ToggleConfig(Config);
            LogicalToggleOffColorConfig = Config.LogicialColors;
            ConfigureStyle(ToggleButton.isOn);
        }

        base.OnEnable();
    }

    void OnDisable()
    {
        ToggleButton.onValueChanged.RemoveListener(HandleToggleValueChanged);
    }

    private void HandleToggleValueChanged(bool isOn)
    {
        ConfigureStyle(isOn);
        // Reset the config, to re-initialize
        Config = Config;

        // Call the listener, if any
        ConfigAsToggleConfig.ToggleValueChangedListener?.Invoke(isOn);
    }

    private void ConfigureStyle(bool isOn)
    {
        Icon.sprite = (isOn) ? ToggleOnIconSprite : ToggleOffIconSprite;
        Config.LogicialColors = (isOn) ? LogicalToggleOnColorConfig : LogicalToggleOffColorConfig;
    }
}