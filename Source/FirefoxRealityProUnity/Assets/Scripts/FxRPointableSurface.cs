using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FxRPointableSurface : MonoBehaviour
{
    public FxRPlugin fxr_plugin = null; // Reference to the plugin. Will be set/cleared by FxRController.
    protected Vector2Int videoSize;
    protected int _windowIndex = 0;

    public void PointerEnter()
    {
        //Debug.Log("PointerEnter()");
        fxr_plugin?.fxrWindowPointerEvent(_windowIndex, FxRPlugin.FxRPointerEventID.Enter, -1, -1);
    }

    public void PointerExit()
    {
        //Debug.Log("PointerExit()");
        fxr_plugin?.fxrWindowPointerEvent(_windowIndex, FxRPlugin.FxRPointerEventID.Exit, -1, -1);
    }

    public void PointerOver(Vector2 texCoord)
    {
        int x = (int) (texCoord.x * videoSize.x);
        int y = (int) (texCoord.y * videoSize.y);
        //Debug.Log("PointerOver(" + x + ", " + y + ")");
        fxr_plugin?.fxrWindowPointerEvent(_windowIndex, FxRPlugin.FxRPointerEventID.Over, x, y);
    }

    public void PointerPress(Vector2 texCoord)
    {
        int x = (int) (texCoord.x * videoSize.x);
        int y = (int) (texCoord.y * videoSize.y);
        //Debug.Log("PointerPress(" + x + ", " + y + ")");
        fxr_plugin?.fxrWindowPointerEvent(_windowIndex, FxRPlugin.FxRPointerEventID.Press, x, y);
    }

    public void PointerRelease(Vector2 texCoord)
    {
        int x = (int) (texCoord.x * videoSize.x);
        int y = (int) (texCoord.y * videoSize.y);
        //Debug.Log("PointerRelease(" + x + ", " + y + ")");
        fxr_plugin?.fxrWindowPointerEvent(_windowIndex, FxRPlugin.FxRPointerEventID.Release, x, y);
    }

    public void PointerScrollDiscrete(Vector2 delta)
    {
        int x = (int) (delta.x);
        int y = (int) (delta.y);
        //Debug.Log("PointerScroll(" + x + ", " + y + ")");
        fxr_plugin?.fxrWindowPointerEvent(_windowIndex, FxRPlugin.FxRPointerEventID.ScrollDiscrete, x, y);
    }
}
