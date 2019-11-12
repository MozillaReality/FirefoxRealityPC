// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019, Mozilla.

using System;
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

    public ToggleConfig ConfigAsToggleConfig
    {
        get
        {
            if (Config != null && (Config as ToggleConfig) == null)
            {
                Config = new ToggleConfig(Config);
                LogicalToggleOffColorConfig = new FxRButtonLogicalColorConfig(Config.LogicialColors);
                ConfigureStyle(ToggleButton.isOn);
            }

            return Config as ToggleConfig;
        }
    }

    public override SelectableConfig Config
    {
        set
        {
            base.Config = value;
            var initToggleConfig = ConfigAsToggleConfig;
        }
    }

    public void SetIsOnWithoutNotify(bool isOn)
    {
        if (ToggleButton.isOn != isOn)
        {
            ToggleButton.SetIsOnWithoutNotify(isOn);
            ConfigureStyle(isOn);
            // Reset the config, to re-initialize
            Config = Config;
        }
    }
    
    protected Toggle ToggleButton => (Toggle) Selectable;
    private FxRButtonLogicalColorConfig LogicalToggleOffColorConfig;

    protected override void OnEnable()
    {
        base.OnEnable();
        ToggleButton.onValueChanged.AddListener(HandleToggleValueChanged);
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