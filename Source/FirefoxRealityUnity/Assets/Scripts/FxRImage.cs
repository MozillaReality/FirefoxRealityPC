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