using UnityEngine;

public class FxRTextureUtils : MonoBehaviour
{
    public static Texture2D CreateTexture(int width, int height, TextureFormat format)
    {
        // Check parameters.
        if (width <= 0 || height <= 0)
        {
            Debug.LogError("Error: Cannot configure video texture with invalid size: " + width + "x" + height);
            return null;
        }
        
        Texture2D vt = new Texture2D(width, height, format, false);
        vt.hideFlags = HideFlags.HideAndDontSave;
        vt.filterMode = FilterMode.Bilinear;
        vt.wrapMode = TextureWrapMode.Clamp;
        vt.anisoLevel = 0;

        // Initialise the video texture to black.
        Color32[] arr = new Color32[width * height];
        Color32 blackOpaque = new Color32(0, 0, 0, 255);
        for (int i = 0; i < arr.Length; i++) arr[i] = blackOpaque;
        vt.SetPixels32(arr);
        vt.Apply(); // Pushes all SetPixels*() ops to texture.
        arr = null;

        return vt;
    }
}