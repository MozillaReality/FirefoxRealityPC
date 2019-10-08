using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FxRToggle : FxRButton
{
    [SerializeField] protected Toggle ToggleButton;
    [SerializeField] protected FxRButtonLogicalColorConfig LogicalToggleOnColorConfig;
    [SerializeField] protected Sprite ToggleOnIconSprite;
    [SerializeField] protected Sprite ToggleOffIconSprite;
    public UnityEvent<bool> ToggleValueChanged;

    private FxRButtonLogicalColorConfig LogicalToggleOffColorConfig;

    protected override void OnEnable()
    {
        ToggleButton.onValueChanged.AddListener(HandleToggleValueChanged);

        if (Config != null)
        {
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
        // Invoke any on change listener, if any
        ToggleValueChanged?.Invoke(isOn);
    }

    private void ConfigureStyle(bool isOn)
    {
        Icon.sprite = (isOn) ? ToggleOnIconSprite : ToggleOffIconSprite;
        Config.LogicialColors = (isOn) ? LogicalToggleOnColorConfig : LogicalToggleOffColorConfig;
    }
}