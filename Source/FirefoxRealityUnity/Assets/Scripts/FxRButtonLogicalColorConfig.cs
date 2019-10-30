// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019, Mozilla.

﻿using System;

[Serializable]
public class FxRButtonLogicalColorConfig
{
    public FxRColorPalette.COLOR_NAME NormalColor;
    public FxRColorPalette.COLOR_NAME HoverColor;
    public FxRColorPalette.COLOR_NAME PressedColor;
    public FxRColorPalette.COLOR_NAME NormalTextColor;
    public FxRColorPalette.COLOR_NAME HoverTextColor;
    public FxRColorPalette.COLOR_NAME PressedTextColor;
    public FxRColorPalette.COLOR_NAME NormalIconColor;
    public FxRColorPalette.COLOR_NAME HoverIconColor;
    public FxRColorPalette.COLOR_NAME PressedIconColor;
    public bool HasBorder;
    public FxRColorPalette.COLOR_NAME BorderColor;
}
