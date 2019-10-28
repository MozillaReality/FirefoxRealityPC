// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019, Mozilla.

﻿using System;
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
