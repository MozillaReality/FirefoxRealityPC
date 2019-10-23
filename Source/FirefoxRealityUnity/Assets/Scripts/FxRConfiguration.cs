using System.Collections;
using System.Collections.Generic;
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
