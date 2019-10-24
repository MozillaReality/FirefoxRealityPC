using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using VRIME2;

public class FxRWindow : FxRPointableSurface
{
    public static Vector2Int DefaultSizeToRequest = new Vector2Int(1920, 1080);
    public bool flipX = false;
    public bool flipY = false;
    private static float DefaultWidth = 3.0f;
    private float Width = DefaultWidth;
    private float Height;
    private float textureScaleU;
    private float textureScaleV;
    private bool pollForVREvents = true;

    private GameObject
        _videoMeshGO = null; // The GameObject which holds the MeshFilter and MeshRenderer for the video. 

    private Texture2D _videoTexture = null; // Texture object with the video image.

    private TextureFormat _textureFormat;

    public Vector2Int PixelSize
    {
        get => videoSize;
        private set { videoSize = value; }
    }

    public TextureFormat TextureFormat
    {
        get => _textureFormat;
        private set { _textureFormat = value; }
    }

    public static FxRWindow FindWindowWithUID(int uid)
    {
        Debug.Log("FxRWindow.FindWindowWithUID(uid:" + uid + ")");
        if (uid != 0)
        {
            FxRWindow[] windows = GameObject.FindObjectsOfType<FxRWindow>();
            foreach (FxRWindow window in windows)
            {
                if (window.GetInstanceID() == uid) return window;
            }
        }

        return null;
    }

    public void ShowVideo()
    {
        // TODO: Stopgap until video textures are supported in plugin
        fxr_plugin?.fxrSetWindowUnityTextureID(_windowIndex, IntPtr.Zero);
        _videoMeshGO.GetComponent<Renderer>().material.mainTexture = null;
        Destroy(_videoTexture);
        _videoTexture = null;

        fxr_plugin?.fxrTriggerFullScreenBeginEvent();
    }

    // TODO: This is only necessary in the current state of affairs where we are sharing a video texture id between video and windows...
    public void RecreateVideoTexture()
    {
        _videoTexture = CreateWindowTexture(videoSize.x, videoSize.y, _textureFormat, out textureScaleU,
            out textureScaleV);
        _videoMeshGO.GetComponent<Renderer>().material.mainTexture = _videoTexture;
    }

    public static FxRWindow CreateNewInParent(GameObject parent)
    {
        Debug.Log("FxRWindow.CreateNewInParent(parent:" + parent + ")");
        FxRWindow window = parent.AddComponent<FxRWindow>();
        return window;
    }

    private void OnEnable()
    {
        VRIME_KeyboardButton.OnKeyPressed += HandleKeyPressed;
    }

    private void OnDisable()
    {
        VRIME_KeyboardButton.OnKeyPressed -= HandleKeyPressed;
        pollForVREvents = false;
    }

    private void HandleKeyPressed(int keycode)
    {
        // TODO: All windows with respond to all keyboard presses. Since we only ever have one at the moment...
        fxr_plugin.fxrKeyEvent(_windowIndex, keycode);
    }

    void Start()
    {
        Debug.Log("FxRWindow.Start()");

        if (_windowIndex == 0)
            fxr_plugin?.fxrRequestNewWindow(GetInstanceID(), DefaultSizeToRequest.x, DefaultSizeToRequest.y);
    }

    public void Close()
    {
        Debug.Log("FxRWindow.OnApplicationQuit()");
        if (_windowIndex != 0)
        {
            fxr_plugin?.fxrCloseWindow(_windowIndex);
            _windowIndex = 0;
        }
    }

    public void RequestSizeMultiple(float sizeMultiple)
    {
        Width = DefaultWidth * sizeMultiple;
        Resize(Mathf.FloorToInt(DefaultSizeToRequest.x * sizeMultiple),
            Mathf.FloorToInt(DefaultSizeToRequest.y * sizeMultiple));
    }

    public bool Resize(int widthPixels, int heightPixels)
    {
        return fxr_plugin.fxrRequestWindowSizeChange(_windowIndex, widthPixels, heightPixels);
    }

    public void WasCreated(int windowIndex, int widthPixels, int heightPixels, TextureFormat format)
    {
        Debug.Log("FxRWindow.WasCreated(windowIndex:" + windowIndex + ", widthPixels:" + widthPixels +
                  ", heightPixels:" + heightPixels + ", format:" + format + ")");
        _windowIndex = windowIndex;
        Height = (Width / widthPixels) * heightPixels;
        videoSize = new Vector2Int(widthPixels, heightPixels);
        _textureFormat = format;
        _videoTexture =
            CreateWindowTexture(videoSize.x, videoSize.y, _textureFormat, out textureScaleU, out textureScaleV);

        _videoMeshGO = FxRTextureUtils.Create2DVideoSurface(_videoTexture, textureScaleU, textureScaleV, Width, Height,
            0, flipX, flipY);
        _videoMeshGO.transform.parent = this.gameObject.transform;
        _videoMeshGO.transform.localPosition = Vector3.zero;
        _videoMeshGO.transform.localRotation = Quaternion.identity;
    }

    public void WasResized(int widthPixels, int heightPixels)
    {
        Height = (Width / widthPixels) * heightPixels;
        videoSize = new Vector2Int(widthPixels, heightPixels);
        var oldTexture = _videoTexture;
        _videoTexture =
            CreateWindowTexture(videoSize.x, videoSize.y, _textureFormat, out textureScaleU, out textureScaleV);
        Destroy(oldTexture);

        FxRTextureUtils.Configure2DVideoSurface(_videoMeshGO, _videoTexture, textureScaleU, textureScaleV, Width,
            Height, flipX, flipY);
    }

    // Update is called once per frame
    void Update()
    {
        if (_windowIndex != 0)
        {
            //Debug.Log("FxRWindow.Update() with _windowIndex == " + _windowIndex);
            fxr_plugin?.fxrRequestWindowUpdate(_windowIndex, Time.deltaTime);
        }

        else
        {
            //Debug.Log("FxRWindow.Update() with _windowIndex == 0");
        }
    }

    // Pointer events from FxRLaserPointer.

//
    private Texture2D CreateWindowTexture(int videoWidth, int videoHeight, TextureFormat format,
        out float textureScaleU, out float textureScaleV)
    {
// Check parameters.
        var vt = FxRTextureUtils.CreateTexture(videoWidth, videoHeight, format);
        if (vt == null)
        {
            textureScaleU = 0;
            textureScaleV = 0;
        }

        else
        {
            textureScaleU = 1;
            textureScaleV = 1;
        }

// Now pass the ID to the native side.
        IntPtr nativeTexPtr = vt.GetNativeTexturePtr();

//        Debug.Log("Calling fxrSetWindowUnityTextureID(windowIndex:" + _windowIndex + ", nativeTexPtr:" +
//                  nativeTexPtr.ToString("X") + ")");
        fxr_plugin?.fxrSetWindowUnityTextureID(_windowIndex, nativeTexPtr);
        return vt;
    }

    private void DestroyWindow()
    {
        bool ed = Application.isEditor;
        if (_videoTexture != null)
        {
            if (ed) DestroyImmediate(_videoTexture);
            else Destroy(_videoTexture);
            _videoTexture = null;
        }

        if (_videoMeshGO != null)
        {
            if (ed) DestroyImmediate(_videoMeshGO);
            else Destroy(_videoMeshGO);
            _videoMeshGO = null;
        }

        Resources.UnloadUnusedAssets();
    }
}