using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FxRVideoController : MonoBehaviour
{
    public FxRPlugin fxr_plugin = null; // Reference to the plugin. Will be set/cleared by FxRController.

    [SerializeField] protected GameObject VideoControls;
    [SerializeField] protected GameObject ProjectionSelectionMenu;
    [SerializeField] protected GameObject FullScreenVideoMenu;
    [SerializeField] protected Transform FullScreenVideoParent;
    

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
            if (_videoControlsVisible)
            {
                FullScreenVideoMenu.SetActive(false);    
            }
            
        }
    }
    private bool _videoControlsVisible = true;
    
    private bool ProjectionSelectionMenuVisible
    {
        get { return _projectionSelectionMenuVisible; }
        set
        {
            _projectionSelectionMenuVisible = value;
            ProjectionSelectionMenu.SetActive(_projectionSelectionMenuVisible);
        }
    }
    private bool _projectionSelectionMenuVisible = true;

    public void ToggleProjectionSelectionMenuVisible()
    {
        ProjectionSelectionMenuVisible = !ProjectionSelectionMenuVisible;
    }
    
    public void SwitchProjectionMode(FxRVideoProjectionMode projectionMode)
    {
        SwitchProjectionMode(projectionMode.Projection);
        ProjectionSelectionMenu.SetActive(false);
    }
    
    public void SwitchProjectionMode(FxRVideoProjectionMode.PROJECTION_MODE projectionMode)
    {
        // TODO: Support multiple video projection modes: 360 Video, 180 left/right, 180 top/bottom, etc
        // TODO: If already in mode being requested, just return
        if (_videoProjection != null)
        {
            _videoProjection.GetComponent<Renderer>().material = null;
            DetachVideoTexture();
            Destroy(_videoProjection);
        }

        switch (projectionMode)
        {
            case FxRVideoProjectionMode.PROJECTION_MODE.VIDEO_PROJECTION_2D:
                ProjectVideo2D();
                break;
            case FxRVideoProjectionMode.PROJECTION_MODE.VIDEO_PROJECTION_360:
                ProjectVideo360(_videoTexture);
                break;
            default:
                Debug.LogError("FxRVideoController::ShowVideo: Received request for unknown projection mode.");
                return;
        }
    }

    private void ProjectVideo2D()
    {
        // TODO: Decide on width of full screen video, and put it in a configurable spot
        var width = 4.0f;
        var height = (width / _videoTexture.width) * _videoTexture.height;

        _videoProjection = FxRTextureUtils.Create2DVideoSurface(_videoTexture, 1, 1, width, height, 0, false, true);
        
        _videoProjection.transform.SetParent(FullScreenVideoParent);
        _videoProjection.transform.localPosition = Vector3.zero;
        // TODO: Should rotate so it is oriented to direction user is facing when video starts?
        _videoProjection.transform.localRotation = Quaternion.identity;

        VideoControlsVisible = false;
        FullScreenVideoMenu.SetActive(true);
    }

    public bool ShowVideo(int pixelwidth, int pixelheight, int nativeFormat, FxRVideoProjectionMode.PROJECTION_MODE projectionMode, int hackWindowIndex)
    {
        _windowIndex = hackWindowIndex;
        TextureFormat format = fxr_plugin.NativeFormatToTextureFormat(nativeFormat);
        if (format == (TextureFormat) 0)
        {
            Debug.LogError("FxRVideoController::ShowVideo: Received request for unknown texture format.");
            return false;
        }
        _videoTexture = CreateVideoTexture(pixelwidth, pixelheight, format);
        SwitchProjectionMode(projectionMode);

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
        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        _videoProjection.GetComponent<Renderer>().material = vm;

        MeshCollider vmc = _videoProjection.AddComponent<MeshCollider>();

        _videoProjection.transform.parent = transform;
        _videoProjection.transform.localPosition = Vector3.zero;
        // TODO: Should rotate so it is oriented to direction user is facing when video starts?
        _videoProjection.transform.localRotation = Quaternion.identity;

        VideoControlsVisible = true;
    }

    public void ExitVideo()
    {
        if (_videoProjection != null)
        {
            fxr_plugin?.fxrSetWindowUnityTextureID(_windowIndex, IntPtr.Zero);

             DetachVideoTexture();
            Destroy(_videoProjection);
            Destroy(_videoTexture);
            _videoProjection = null;
        }

        VideoControlsVisible = false;
        
        _windowIndex = 0;
    }

    private void DetachVideoTexture()
    {
        _videoProjection.GetComponentInChildren<MeshRenderer>().material.mainTexture = null;
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
        FullScreenVideoMenu.SetActive(false);
        ProjectionSelectionMenuVisible = false;
        
        FxRController.OnBrowsingModeChanged += HandleBrowsingModeChanged;
    }

    private void HandleBrowsingModeChanged(FxRController.FXR_BROWSING_MODE browsingMode)
    {
        if (browsingMode == FxRController.FXR_BROWSING_MODE.FXR_BROWSER_MODE_WEB_BROWSING)
        {
            FullScreenVideoMenu.SetActive(false);
        }
    }
}