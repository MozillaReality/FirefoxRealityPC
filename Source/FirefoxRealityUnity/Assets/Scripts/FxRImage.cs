// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019-2020, Mozilla.

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FxRImage : MonoBehaviour
{
    public FxRColorPalette.COLOR_NAME LogicalColor;

    void OnEnable()
    {
        MyImage.color = FxRConfiguration.Instance.ColorPalette.ColorForName(LogicalColor);
    }

    Image MyImage
    {
        get
        {
            if (myImage == null)
            {
                myImage = GetComponent<Image>();
            }

            return myImage;
        }
    }

    private Image myImage;
}