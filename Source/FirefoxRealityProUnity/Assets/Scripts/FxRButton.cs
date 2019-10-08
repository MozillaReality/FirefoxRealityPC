using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class FxRButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
    IPointerUpHandler
{
    [Serializable]
    public class ButtonConfig
    {
        public string ButtonLabel;
        [SerializeField] public UnityAction ButtonPressedAction;
        [SerializeField] protected FxRButtonLogicalColorConfig LogicalColorConfig;

        public FxRButtonColorConfig ColorConfig
        {
            get
            {
                if (colorConfig == null)
                {
                    colorConfig = FxRConfiguration.Instance.ColorPalette.CreateButtonColorConfigForLogicalConfig(LogicalColorConfig);
                }

                return colorConfig;
            }
        }

        private FxRButtonColorConfig colorConfig;

        public ButtonConfig(string label, UnityAction pressAction, FxRButtonLogicalColorConfig colorConfig)
        {
            ButtonLabel = label;
            ButtonPressedAction = pressAction;
            LogicalColorConfig = colorConfig;
        }
    }

    [SerializeField] protected Button Button;
    [SerializeField] protected TMP_Text ButtonLabel;
    [SerializeField] protected Image BackgroundImage;
    [SerializeField] protected Image Border;
    [SerializeField] protected Image Icon;

    public ButtonConfig Config
    {
        set
        {
            config = value;
            if (config.ButtonPressedAction != null)
            {
                Button.onClick.AddListener(config.ButtonPressedAction);
            }

            ButtonLabel.text = !string.IsNullOrEmpty(config.ButtonLabel) ? config.ButtonLabel : ButtonLabel.text;
            ButtonLabel.color = config.ColorConfig.NormalTextColor;
            BackgroundImage.color = config.ColorConfig.NormalColor;
            Border.color = config.ColorConfig.BorderColor;
            Border.gameObject.SetActive(config.ColorConfig.HasBorder);
            Icon.color = config.ColorConfig.NormalIconColor;
        }

        private get { return config; }
    }

    [SerializeField] private ButtonConfig config;

    bool boxColliderSizeSet;

    private void OnEnable()
    {
        // Initialize the config, if necessary
        if (config != null)
        {
            Config = config;
        }
    }

    private void Update()
    {
        // Add a box collider to the button so that the laser pointer will work.
        // TODO: Refactor this out into a general class that can add the requisite box colliders to a tree of objects.
        if (boxColliderSizeSet) return;
        var rectTransform = gameObject.GetComponent<RectTransform>();
        if (rectTransform.sizeDelta.Equals(Vector2.zero)) return;
        var boxCOllider = gameObject.AddComponent<BoxCollider>();
        boxCOllider.size = new Vector3(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y, 1f);
        boxColliderSizeSet = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        BackgroundImage.color = Config.ColorConfig.HoverColor;
        ButtonLabel.color = Config.ColorConfig.HoverTextColor;
        Icon.color = config.ColorConfig.HoverIconColor;
        Border.gameObject.SetActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        BackgroundImage.color = Config.ColorConfig.NormalColor;
        ButtonLabel.color = Config.ColorConfig.NormalTextColor;
        Icon.color = config.ColorConfig.NormalIconColor;
        Border.gameObject.SetActive(config.ColorConfig.HasBorder);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        BackgroundImage.color = Config.ColorConfig.PressedColor;
        ButtonLabel.color = Config.ColorConfig.PressedTextColor;
        Icon.color = config.ColorConfig.PressedIconColor;
        Border.gameObject.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        BackgroundImage.color = Config.ColorConfig.NormalColor;
        ButtonLabel.color = Config.ColorConfig.NormalTextColor;
        Icon.color = config.ColorConfig.NormalIconColor;
        Border.gameObject.SetActive(config.ColorConfig.HasBorder);
    }
}