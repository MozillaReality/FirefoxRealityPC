using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using VRIME2;

public class FxRWindow : MonoBehaviour
{
    public static Vector2Int DefaultSizeToRequest = new Vector2Int(1920, 1080);
    public bool flipX = false;
    public bool flipY = false;
    private static float DefaultWidth = 3.0f;
    private float Width = DefaultWidth;
    private float Height;
    private Vector2Int videoSize;
    private float textureScaleU;
    private float textureScaleV;
    private bool pollForVREvents = true;

    private GameObject
        _videoMeshGO = null; // The GameObject which holds the MeshFilter and MeshRenderer for the video. 

    private Texture2D _videoTexture = null; // Texture object with the video image.
    public FxRPlugin fxr_plugin = null; // Reference to the plugin. Will be set/cleared by FxRController.

    private int _windowIndex = 0;
    private TextureFormat _textureFormat;

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
        _videoMeshGO = CreateWindowGameObject(_videoTexture, textureScaleU, textureScaleV, Width, Height, 0);
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
        ConfigureWindow(_videoMeshGO, _videoTexture, textureScaleU, textureScaleV, Width, Height);
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

// Creates a GameObject in layer 'layer' which renders a mesh displaying the video stream.
    private GameObject CreateWindowGameObject(Texture2D vt, float textureScaleU, float textureScaleV, float width,
        float height, int layer)
    {
// Check parameters.
        if (!vt)
        {
            Debug.LogError("Error: CreateWindowMesh null Texture2D");
            return null;
        }

// Create new GameObject to hold mesh.
        GameObject vmgo = new GameObject("Video source");
        if (vmgo == null)
        {
            Debug.LogError("Error: CreateWindowMesh cannot create GameObject.");
            return null;
        }

        vmgo.layer = layer;

// Create a material which uses our "VideoPlaneNoLight" shader, and paints itself with the texture.
        Shader shaderSource = Shader.Find("TextureNoLight");
        Material vm = new Material(shaderSource); //fxrUnity.Properties.Resources.VideoPlaneShader;
        vm.hideFlags = HideFlags.HideAndDontSave;
//Debug.Log("Created video material");

        MeshFilter filter = vmgo.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = vmgo.AddComponent<MeshRenderer>();
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        vmgo.GetComponent<Renderer>().material = vm;
        vmgo.AddComponent<MeshCollider>();

        ConfigureWindow(vmgo, vt, textureScaleU, textureScaleV, width, height);
        return vmgo;
    }

    private void ConfigureWindow(GameObject vmgo, Texture2D vt, float textureScaleU, float textureScaleV,
        float width, float height)
    {
// Check parameters.
        if (!vt)
        {
            Debug.LogError("Error: CreateWindowMesh null Texture2D");
            return;
        }

// Create the mesh
        var m = CreateVideoMesh(textureScaleU, textureScaleV, width, height);

// Assign the texture to the window's material
        Material vm = vmgo.GetComponent<Renderer>().material;
        vm.mainTexture = vt;

// Assign the mesh to the mesh filter
        MeshFilter filter = vmgo.GetComponent<MeshFilter>();
        filter.mesh = m;

// Update the mesh collider mesh
        MeshCollider vmc = vmgo.GetComponent<MeshCollider>();
        ;

        vmc.sharedMesh = filter.sharedMesh;
    }

    private Mesh CreateVideoMesh(float textureScaleU, float textureScaleV, float width, float height)
    {
// Now create a mesh appropriate for displaying the video, a mesh filter to instantiate that mesh,
// and a mesh renderer to render the material on the instantiated mesh.
        Mesh m = new Mesh();
        m.Clear();
        m.vertices = new Vector3[]
        {
            new Vector3(-width * 0.5f, 0.0f, 0.0f),
            new Vector3(width * 0.5f, 0.0f, 0.0f),
            new Vector3(width * 0.5f, height, 0.0f),
            new Vector3(-width * 0.5f, height, 0.0f),
        };
        m.normals = new Vector3[]
        {
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(0.0f, 0.0f, 1.0f),
        };
        float u1 = flipX ? textureScaleU : 0.0f;
        float u2 = flipX ? 0.0f : textureScaleU;
        float v1 = flipY ? textureScaleV : 0.0f;
        float v2 = flipY ? 0.0f : textureScaleV;
        m.uv = new Vector2[]
        {
            new Vector2(u1, v1),
            new Vector2(u2, v1),
            new Vector2(u2, v2),
            new Vector2(u1, v2)
        };
        m.triangles = new int[]
        {
            2, 1, 0,
            3, 2, 0
        };
        return m;
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