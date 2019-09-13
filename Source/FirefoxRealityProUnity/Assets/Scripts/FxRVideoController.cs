using System;
using System.Collections.Generic;
using UnityEngine;

public class FxRVideoController : MonoBehaviour
{
    public enum FXR_VIDEO_PROJECTION_MODE
    {
        VIDEO_PROJECTION_2D = 0,
        VIDEO_PROJECTION_360 = 1,
        VIDEO_PROJECTION_360S = 2, // 360 stereo
        VIDEO_PROJECTION_180 = 3,
        VIDEO_PROJECTION_180LR = 4, // 180 left to right
        VIDEO_PROJECTION_180TB = 5, // 180 top to bottom
        VIDEO_PROJECTION_3D = 6 // 3D side by side

    }
    public delegate void ImmersiveScreenVideoClosed(int windowIndex);

   public FxRPlugin fxr_plugin = null; // Reference to the plugin. Will be set/cleared by FxRController.

    [SerializeField] protected GameObject VideoControls;

    private Texture2D _videoTexture = null; // Texture object with the video image.

    private GameObject _videoProjection;
    // TODO: Remove _windowIndex once full screen video uses its own texture mechanism
    private int _windowIndex = 0;

    private bool VideoControlsVisible
    {
        get { return _videoControlsVisible; }
        set
        {
            _videoControlsVisible = value;
            VideoControls.SetActive(_videoControlsVisible);
        }
    }

    private bool _videoControlsVisible = true;

    public bool ShowVideo(int pixelwidth, int pixelheight, int nativeFormat, FXR_VIDEO_PROJECTION_MODE projectionMode)
    {
        _windowIndex = 1;
        TextureFormat format = fxr_plugin.NativeFormatToTextureFormat(nativeFormat);
        if (format == (TextureFormat) 0)
        {
            Debug.LogError("FxRVideoController::ShowVideo: Received request for unknown texture format.");
            return false;
        }
        var videoTexture = CreateVideoTexture(pixelwidth, pixelheight, format);

        // TODO: Support multiple video projection modes: 360 Video, 180 left/right, 180 top/bottom, etc
        switch (projectionMode)
        {
            case FXR_VIDEO_PROJECTION_MODE.VIDEO_PROJECTION_360:
                ProjectVideo360(videoTexture);
                break;
            default:
                Debug.LogError("FxRVideoController::ShowVideo: Received request for unknown projection mode.");
                return false;
        }


        return true;
    }
    
    private Texture2D CreateVideoTexture(int videoWidth, int videoHeight, TextureFormat format)
    {
        // Check parameters.
        var vt = FxRTextureUtils.CreateTexture(videoWidth, videoHeight, format);

        // Now pass the ID to the native side.
        IntPtr nativeTexPtr = vt.GetNativeTexturePtr();
        Debug.Log("Calling fxrSetWindowUnityTextureID(windowIndex:" + _windowIndex + ", nativeTexPtr:" +
                  nativeTexPtr.ToString("X") + ")");
        fxr_plugin?.fxrSetWindowUnityTextureID(_windowIndex, nativeTexPtr);

        return vt;
    }

    
    private void ProjectVideo360(Texture2D videoTexture)
    {
        _videoProjection = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _videoProjection.layer = 0;

        // Flip the mesh uv's so it renders right-side-up
        var mesh = _videoProjection.GetComponent<MeshFilter>().mesh;
        var uvs = mesh.uv;
        List<Vector2> flippedUVs = new List<Vector2>(uvs.Length);
        foreach (var uv in uvs)
        {
            flippedUVs.Add(new Vector2(uv.x, 1-uv.y));
        }

        mesh.uv = flippedUVs.ToArray();

        _videoProjection.transform.localScale = new Vector3(100f, 100f, 100f);

        // Set up the material
        Shader shaderSource = Shader.Find("Unlit/InsideOut");
        Material vm = new Material(shaderSource);
        vm.hideFlags = HideFlags.HideAndDontSave;
        vm.mainTexture = videoTexture;

        // Set up the mesh renderer
        MeshRenderer meshRenderer = _videoProjection.GetComponent<MeshRenderer>();
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        _videoProjection.GetComponent<Renderer>().material = vm;

        MeshCollider vmc = _videoProjection.AddComponent<MeshCollider>();

        _videoProjection.transform.parent = transform;
        _videoProjection.transform.localPosition = Vector3.zero;
        // TODO: Should rotate so it is oriented to direction user is facing when video starts?
        _videoProjection.transform.localRotation = Quaternion.identity;

        VideoControlsVisible = true;
    }

    public void ToggleVideoControlsVisibility()
    {
        VideoControlsVisible = !VideoControlsVisible;
    }

    public void ExitVideo()
    {
        if (_videoProjection != null)
        {
            fxr_plugin?.fxrSetWindowUnityTextureID(_windowIndex, IntPtr.Zero);

            var videoTexture = _videoProjection.GetComponentInChildren<MeshRenderer>().material.mainTexture;
            _videoProjection.GetComponentInChildren<MeshRenderer>().material.mainTexture = null;
            Destroy(_videoProjection);
            Destroy(videoTexture);
            _videoProjection = null;
        }

        VideoControlsVisible = false;
        
        _windowIndex = 0;
    }

    void Update()
    {
        if (_windowIndex != 0) {
            //Debug.Log("FxRWindow.Update() with _windowIndex == " + _windowIndex);
            fxr_plugin.fxrRequestWindowUpdate(_windowIndex, Time.deltaTime);
        } else {
            //Debug.Log("FxRWindow.Update() with _windowIndex == 0");
        }
    }

    private void OnEnable()
    {
        VideoControlsVisible = false;
    }
}