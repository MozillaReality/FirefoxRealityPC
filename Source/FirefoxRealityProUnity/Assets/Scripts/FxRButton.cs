using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class FxRButton : FxRSelectable
{
    [SerializeField] public UnityAction ButtonPressedAction;

    [Serializable]
    public class ButtonConfig : SelectableConfig
    {
        [HideInInspector] public UnityAction ButtonPressedAction;

        public ButtonConfig(string label, UnityAction pressAction, FxRButtonLogicalColorConfig colorConfig) : base(
            label, colorConfig)
        {
            ButtonPressedAction = pressAction;
        }

        public ButtonConfig(SelectableConfig config) : this(config.ButtonLabel, null, config.LogicialColors)
        {
        }
    }

    public override SelectableConfig Config
    {
        set
        {
            base.Config = value;
            if (ConfigAsButtonConfig.ButtonPressedAction != null)
            {
                Button.onClick.RemoveAllListeners();
                Button.onClick.AddListener(ConfigAsButtonConfig.ButtonPressedAction);
            }
        }
    }

    public ButtonConfig ConfigAsButtonConfig => (ButtonConfig) Config;

    protected Button Button => (Button) Selectable;

    protected override void OnEnable()
    {
        if (Config != null)
        {
            Config = new ButtonConfig(Config);
        }

        base.OnEnable();
    }
}