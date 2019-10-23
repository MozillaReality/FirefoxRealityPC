using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FxRColorPalette : MonoBehaviour
{
    public enum COLOR_NAME
    {
        COLOR_UNDEFINED,
        COLOR_01,
        COLOR_02,
        COLOR_03,
        COLOR_04,
        COLOR_05,
        COLOR_06,
        COLOR_07,
        COLOR_08,
        COLOR_09,
        COLOR_10,
        COLOR_11,
        COLOR_12,
        COLOR_13,
        COLOR_14,
        COLOR_15,
        COLOR_16,
        COLOR_17,
        COLOR_18
    }

    [Serializable]
    public class NamedColor
    {
        public COLOR_NAME Name;
        public Color Color;
    }

    [SerializeField] protected List<NamedColor> NamedColors;
    
    public readonly FxRButtonLogicalColorConfig NormalBrowsingPrimaryDialogButtonColors =
        new FxRButtonLogicalColorConfig()
        {
            NormalColor = COLOR_NAME.COLOR_08,
            HoverColor = COLOR_NAME.COLOR_07,
            PressedColor = COLOR_NAME.COLOR_09,
            NormalTextColor = COLOR_NAME.COLOR_13,
            HoverTextColor = COLOR_NAME.COLOR_13,
            PressedTextColor = COLOR_NAME.COLOR_06,
            HasBorder = false,
            BorderColor = COLOR_NAME.COLOR_UNDEFINED
        };

    public readonly FxRButtonLogicalColorConfig NormalBrowsingSecondaryDialogButtonColors =
        new FxRButtonLogicalColorConfig()
        {
            NormalColor = COLOR_NAME.COLOR_06,
            HoverColor = COLOR_NAME.COLOR_07,
            PressedColor = COLOR_NAME.COLOR_09,
            NormalTextColor = COLOR_NAME.COLOR_13,
            HoverTextColor = COLOR_NAME.COLOR_13,
            PressedTextColor = COLOR_NAME.COLOR_06,
            HasBorder = true,
            BorderColor = COLOR_NAME.COLOR_08
        };

    public Color NormalBrowsingDialogBackgroundColor => ColorForName(COLOR_NAME.COLOR_06);
    public Color DialogTitleTextColor => ColorForName(COLOR_NAME.COLOR_13);
    public Color DialogBodyTextColor => ColorForName(COLOR_NAME.COLOR_14);

    public FxRButtonColorConfig CreateButtonColorConfigForLogicalConfig(FxRButtonLogicalColorConfig logicalColorConfig)
    {
        FxRButtonColorConfig colorConfig = new FxRButtonColorConfig()
        {
            BorderColor = ColorForName(logicalColorConfig.BorderColor),
            HasBorder = logicalColorConfig.HasBorder,
            HoverColor = ColorForName(logicalColorConfig.HoverColor),
            NormalColor = ColorForName(logicalColorConfig.NormalColor),
            PressedColor = ColorForName(logicalColorConfig.PressedColor),
            HoverTextColor = ColorForName(logicalColorConfig.HoverTextColor),
            NormalTextColor = ColorForName(logicalColorConfig.NormalTextColor),
            PressedTextColor = ColorForName(logicalColorConfig.PressedTextColor),
            HoverIconColor = ColorForName(logicalColorConfig.HoverIconColor),
            NormalIconColor = ColorForName(logicalColorConfig.NormalIconColor),
            PressedIconColor = ColorForName(logicalColorConfig.PressedIconColor)
        };
        return colorConfig;
    }

    private Color ColorForName(COLOR_NAME colorName)
    {
        foreach (var namedColor in NamedColors)
        {
            if (namedColor.Name.Equals(colorName))
            {
                return namedColor.Color;
            }
        }

        Debug.LogError("No color configured for color: " + Enum.GetName(typeof(COLOR_NAME), colorName));
        return default;
    }
}