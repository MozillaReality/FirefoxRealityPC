using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class FxRDialogButton : MonoBehaviour
{
    // TODO: Do we need to support image-only dialog buttons?
    public class ButtonConfig
    {
        public static readonly Color DEFAULT_BACKGROUND_COLOR = Color.gray;
        public static readonly Color DEFAULT_TEXT_COLOR = Color.white;

        public string ButtonLabel;
        public UnityAction ButtonPressedAction;
        public Color BackgroundColor = Color.gray;
        public Color TextColor = Color.white;

        public ButtonConfig(string label, UnityAction pressAction, Color backgroundColor = default,
            Color textColor = default)
        {
            ButtonLabel = label;
            ButtonPressedAction = pressAction;
            BackgroundColor = backgroundColor == default ? DEFAULT_BACKGROUND_COLOR : backgroundColor;
            TextColor = textColor == default ? DEFAULT_TEXT_COLOR : textColor;
        }
    }

    [SerializeField] protected Button Button;
    [SerializeField] protected TMP_Text ButtonLabel;
    [SerializeField] protected Image BackgroundImage;

    public ButtonConfig Config
    {
        set
        {
            Button.onClick.AddListener(value.ButtonPressedAction);
            ButtonLabel.text = value.ButtonLabel;
            ButtonLabel.color = value.TextColor;
            BackgroundImage.color = value.BackgroundColor;
        }
    }

    bool boxColliderSizeSet;
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
}