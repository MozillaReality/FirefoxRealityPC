using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class FxRDialogButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
    IPointerUpHandler
{
    // TODO: Do we need to support image-only dialog buttons?

    public static ButtonColorConfig SecondaryButtonColorConfig = new ButtonColorConfig()
    {
        // TODO: Pull out these specific colors into a configurable set
        NormalColor = Color.white,
        HoverColor = new Color(.0f, 179f/255f, 227f/255f),
        PressedColor = new Color(9f/255f, 104f/255f, 182f/255f),
        NormalTextColor = new Color(44f/255f,58f/255f,80f/255f),
        HoverTextColor = new Color(44f/255f,58f/255f,80f/255f),
        PressedTextColor = Color.white,
        HasBorder =  true,
        BorderColor = new Color(.0f, 179f/255f, 227f/255f)
    };

    public static ButtonColorConfig PrimaryButtonColorConfig = new ButtonColorConfig()
    {
        // TODO: Pull out these specific colors into a configurable set
        NormalColor = new Color(86f/255f, 217f/255f, 246f/255f),
        HoverColor = new Color(.0f, 179f/255f, 227f/255f),
        PressedColor = new Color(9f/255f, 104f/255f, 182f/255f),
        NormalTextColor = new Color(44f/255f,58f/255f,80f/255f),
        HoverTextColor = new Color(44f/255f,58f/255f,80f/255f),
        PressedTextColor = Color.white
    };

    [Serializable]
    public class ButtonColorConfig
    {
        public Color NormalColor;
        public Color HoverColor;
        public Color PressedColor;
        public Color TextColor;
        public Color NormalTextColor;
        public Color HoverTextColor;
        public Color PressedTextColor;
        public bool HasBorder = false;
        public Color BorderColor;
    }

    [Serializable]
    public class ButtonConfig
    {
        public static readonly Color DEFAULT_BACKGROUND_COLOR = Color.gray;
        public static readonly Color DEFAULT_TEXT_COLOR = Color.white;

        public string ButtonLabel;
        [SerializeField] public UnityAction ButtonPressedAction;
        [SerializeField] public ButtonColorConfig ColorConfig;

        public ButtonConfig(string label, UnityAction pressAction, ButtonColorConfig colorConfig = null)
        {
            ButtonLabel = label;
            ButtonPressedAction = pressAction;
            ColorConfig = (colorConfig == null) ? SecondaryButtonColorConfig : colorConfig;
        }
    }

    [SerializeField] protected Button Button;
    [SerializeField] protected TMP_Text ButtonLabel;
    [SerializeField] protected Image BackgroundImage;
    [SerializeField] protected Image Border;

    public ButtonConfig Config
    {
        set
        {
            config = value;
            if (config.ButtonPressedAction != null)
            {
                Button.onClick.AddListener(config.ButtonPressedAction);
            }
            ButtonLabel.text = config.ButtonLabel;
            ButtonLabel.color = config.ColorConfig.NormalTextColor;
            BackgroundImage.color = config.ColorConfig.NormalColor;
            Border.color = config.ColorConfig.BorderColor;
            Border.gameObject.SetActive(config.ColorConfig.HasBorder);
        }

        private get { return config; }
    }

    [SerializeField]
    private ButtonConfig config;

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
        Border.gameObject.SetActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        BackgroundImage.color = Config.ColorConfig.NormalColor;
        ButtonLabel.color = Config.ColorConfig.NormalTextColor;
        Border.gameObject.SetActive(config.ColorConfig.HasBorder);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        BackgroundImage.color = Config.ColorConfig.PressedColor;
        ButtonLabel.color = Config.ColorConfig.PressedTextColor;
        Border.gameObject.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        BackgroundImage.color = Config.ColorConfig.NormalColor;
        ButtonLabel.color = Config.ColorConfig.NormalTextColor;
        Border.gameObject.SetActive(config.ColorConfig.HasBorder);
    }
}