// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019-2020, Mozilla.

using UnityEngine;

public class FxRConfiguration : Singleton<FxRConfiguration>
{
    [SerializeField]
    protected FxRColorPalette ColorPalettePrefab;

    [HideInInspector]
    public FxRColorPalette ColorPalette
    {
        get
        {
            if (colorPalette == null)
            {
                colorPalette = Instantiate(ColorPalettePrefab, transform);
            }

            return colorPalette;
        }
    }

    private FxRColorPalette colorPalette;
}
