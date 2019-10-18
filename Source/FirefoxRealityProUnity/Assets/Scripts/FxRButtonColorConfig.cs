using System;
using UnityEngine;

[Serializable]
public class FxRButtonColorConfig
{
    public Color NormalColor;
    public Color HoverColor;
    public Color PressedColor;
    public Color NormalTextColor;
    public Color HoverTextColor;
    public Color PressedTextColor;
    public Color NormalIconColor;
    public Color HoverIconColor;
    public Color PressedIconColor;

    public bool HasBorder = false;
    public Color BorderColor;
}
