using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FxRVideoController : FxRPointableSurface
{
    [SerializeField] protected GameObject VideoControls;
    [SerializeField] protected GameObject ProjectionSelectionMenu;
    [SerializeField] protected GameObject FullScreenVideoMenu;
    [SerializeField] protected Transform FullScreenVideoParent;

    [SerializeField] protected FxRStereoVideoRig Stereo360VideoRigPrefab;
    [SerializeField] protected FxRStereoVideoRig Stereo180VideoRigPrefab;
    [SerializeField] protected GameObject HemispherePrefab;

    [SerializeField] protected int LeftEyeLayer;
    [SerializeField] protected int RightEyeLayer;

    private Texture2D _videoTexture = null; // Texture object with the video image.

    private GameObject _videoProjection;

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
    private const float DEFAULT_FULLSCREEN_VIDEO_WIDTH = 4.0f;

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
        // TODO: If already in mode being requested, just return
        if (_videoProjection != null)
        {
//            var renderers = _videoProjection.GetComponentsInChildren<Renderer>();
//            foreach (var renderer in renderers)
//            {
//                renderer.material = null;
//            }
            DetachVideoTexture();
            Destroy(_videoProjection);
        }

        switch (projectionMode)
        {
            case FxRVideoProjectionMode.PROJECTION_MODE.VIDEO_PROJECTION_2D:
                ProjectVideo2D();
                break;
            case FxRVideoProjectionMode.PROJECTION_MODE.VIDEO_PROJECTION_360:
                ProjectVideo360();
                break;
            case FxRVideoProjectionMode.PROJECTION_MODE.VIDEO_PROJECTION_180:
                ProjectVideo180();
                break;
            case FxRVideoProjectionMode.PROJECTION_MODE.VIDEO_PROJECTION_180LR:
                ProjectVideo180StereoLeftRight();
                break;
            case FxRVideoProjectionMode.PROJECTION_MODE.VIDEO_PROJECTION_180TB:
                ProjectVideo180StereoTopBottom();
                break;
            case FxRVideoProjectionMode.PROJECTION_MODE.VIDEO_PROJECTION_360S:
                ProjectVideo360Stereo();
                break;
            case FxRVideoProjectionMode.PROJECTION_MODE.VIDEO_PROJECTION_3D:
                ProjectVideo3DLeftRight();
                break;
            default:
                Debug.LogError("FxRVideoController::ShowVideo: Received request for unknown projection mode.");
                return;
        }
    }

    private void ProjectVideo3DLeftRight()
    {
        // TODO: Decide on width of full screen video, and put it in a configurable spot
        var width = DEFAULT_FULLSCREEN_VIDEO_WIDTH;
        var height = (width / _videoTexture.width) * _videoTexture.height;

        _videoProjection = new GameObject("3DProjection");
        var leftEye = FxRTextureUtils.Create2DVideoSurface(_videoTexture, 1, 1, width, height, 0, false, true);
        leftEye.layer = LeftEyeLayer;
        var leftEyeMaterial = leftEye.GetComponent<Renderer>().material;
        leftEyeMaterial.SetTextureScale("_MainTex", new Vector2(.5f, 1f));
        leftEyeMaterial.SetTextureOffset("_MainTex", new Vector2(0f, 0f));

        var rightEye = FxRTextureUtils.Create2DVideoSurface(_videoTexture, 1, 1, width, height, 0, false, true);
        rightEye.layer = RightEyeLayer;
        var rightEyeMaterial = rightEye.GetComponent<Renderer>().material;
        rightEyeMaterial.SetTextureScale("_MainTex", new Vector2(.5f, 1f));
        rightEyeMaterial.SetTextureOffset("_MainTex", new Vector2(.5f, 0f));

        leftEye.transform.SetParent(_videoProjection.transform);
        leftEye.transform.localScale = Vector3.one;
        leftEye.transform.localPosition = Vector3.zero;
        leftEye.transform.localRotation = Quaternion.identity;

        rightEye.transform.SetParent(_videoProjection.transform);
        rightEye.transform.localScale = Vector3.one;
        rightEye.transform.localPosition = Vector3.zero;
        rightEye.transform.localRotation = Quaternion.identity;
        
        _videoProjection.transform.SetParent(FullScreenVideoParent);
        _videoProjection.transform.localPosition = Vector3.zero;
        // TODO: Should rotate so it is oriented to direction user is facing when video starts?
        _videoProjection.transform.localRotation = Quaternion.identity;
    }

    private void ProjectVideo180StereoTopBottom()
    {
        FxRStereoVideoRig stereoVideoRig = Instantiate<FxRStereoVideoRig>(Stereo180VideoRigPrefab, Vector3.zero, Quaternion.identity, transform);
        ConfigureProjectionSurface(stereoVideoRig.LeftEyeProjectionSurface, out var leftEyeMaterial);
        leftEyeMaterial.SetTextureScale("_MainTex", new Vector2(2f, .5f));
        leftEyeMaterial.SetTextureOffset("_MainTex", new Vector2(0f, 0f));

        ConfigureProjectionSurface(stereoVideoRig.RightEyeProjectionSurface, out var rightEyeMaterial);
        rightEyeMaterial.SetTextureScale("_MainTex", new Vector2(2f, .5f));
        rightEyeMaterial.SetTextureOffset("_MainTex", new Vector2(0f, .5f));

        _videoProjection = stereoVideoRig.gameObject;
        _videoProjection.transform.localScale = new Vector3(100f, 100f, 100f);
    }

    private void ProjectVideo180StereoLeftRight()
    {
        FxRStereoVideoRig stereoVideoRig = Instantiate<FxRStereoVideoRig>(Stereo180VideoRigPrefab, Vector3.zero, Quaternion.identity, transform);
        ConfigureProjectionSurface(stereoVideoRig.LeftEyeProjectionSurface, out var leftEyeMaterial);
//        leftEyeMaterial.SetTextureScale("_MainTex", new Vector2(.5f, 1f));
        leftEyeMaterial.SetTextureOffset("_MainTex", new Vector2(0f, 0f));

        ConfigureProjectionSurface(stereoVideoRig.RightEyeProjectionSurface, out var rightEyeMaterial);
//        rightEyeMaterial.SetTextureScale("_MainTex", new Vector2(.5f, 1f));
        rightEyeMaterial.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));

        _videoProjection = stereoVideoRig.gameObject;
        _videoProjection.transform.localScale = new Vector3(100f, 100f, 100f);
    }

    private void ProjectVideo360Stereo()
    {
        var stereoVideoRig = Instantiate(Stereo360VideoRigPrefab, Vector3.zero, Quaternion.identity, transform);
        
        ConfigureProjectionSurface(stereoVideoRig.LeftEyeProjectionSurface, out var leftEyeMaterial);
        leftEyeMaterial.SetTextureScale("_MainTex", new Vector2(1f, .5f));
        leftEyeMaterial.SetTextureOffset("_MainTex", new Vector2(0f, .5f));

        ConfigureProjectionSurface(stereoVideoRig.RightEyeProjectionSurface, out var rightEyeMaterial);
        rightEyeMaterial.SetTextureScale("_MainTex", new Vector2(1f, .5f));
        rightEyeMaterial.SetTextureOffset("_MainTex", new Vector2(0f, 0f));

        _videoProjection = stereoVideoRig.gameObject;
        _videoProjection.transform.localScale = new Vector3(100f, 100f, 100f);
    }

    private void ProjectVideo180()
    {
        _videoProjection = Instantiate(HemispherePrefab, Vector3.zero, 
            // Rotate 180degrees to face the user
            Quaternion.Euler(0f, 180f, 0f),
            transform);
        _videoProjection.transform.localScale = new Vector3(100f, 100f, 100f);
        ConfigureProjectionSurface(_videoProjection, out var meshMaterial);

        meshMaterial.SetTextureScale("_MainTex", new Vector2(2f, 1f));
        meshMaterial.SetTextureOffset("_MainTex", new Vector2(0f, 0f));
    }

    private void ProjectVideo2D()
    {
        // TODO: Decide on width of full screen video, and put it in a configurable spot
        var width = DEFAULT_FULLSCREEN_VIDEO_WIDTH;
        var height = (width / _videoTexture.width) * _videoTexture.height;

        _videoProjection = FxRTextureUtils.Create2DVideoSurface(_videoTexture, 1, 1, width, height, 0, false, true);

        _videoProjection.transform.SetParent(FullScreenVideoParent);
        _videoProjection.transform.localPosition = Vector3.zero;
        // TODO: Should rotate so it is oriented to direction user is facing when video starts?
        _videoProjection.transform.localRotation = Quaternion.identity;

        VideoControlsVisible = false;
        FullScreenVideoMenu.SetActive(true);
    }

    public bool ShowVideo(int pixelwidth, int pixelheight, int nativeFormat,
        FxRVideoProjectionMode.PROJECTION_MODE projectionMode, int hackWindowIndex)
    {
        _windowIndex = hackWindowIndex;
        videoSize = new Vector2Int(pixelwidth, pixelheight);
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


    private void ProjectVideo360()
    {
        _videoProjection = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        ConfigureProjectionSurface(_videoProjection, out _);
        
        _videoProjection.transform.SetParent(transform);
        _videoProjection.transform.localPosition = Vector3.zero;
        _videoProjection.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
        _videoProjection.transform.localScale = new Vector3(100f, 100f, 100f);

        VideoControlsVisible = true;
    }

    private void ConfigureProjectionSurface(GameObject projectionSurface, out Material meshMaterial)
    {
        // Flip the mesh uv's so it renders right-side-up
        var mesh = projectionSurface.GetComponent<MeshFilter>().mesh;
        var uvs = mesh.uv;
        List<Vector2> flippedUVs = new List<Vector2>(uvs.Length);
        foreach (var uv in uvs)
        {
            flippedUVs.Add(new Vector2(uv.x, 1 - uv.y));
        }

        mesh.uv = flippedUVs.ToArray();
        // Set up the material
        Shader shaderSource = Shader.Find("Unlit/InsideOut");
        meshMaterial = new Material(shaderSource);
        meshMaterial.hideFlags = HideFlags.HideAndDontSave;
        meshMaterial.mainTexture = _videoTexture;

        // Set up the mesh renderer
        MeshRenderer meshRenderer = projectionSurface.GetComponent<MeshRenderer>();
        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        projectionSurface.GetComponent<Renderer>().material = meshMaterial;
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
        var meshRenderers = _videoProjection.GetComponentsInChildren<MeshRenderer>();
        foreach (var meshRenderer in meshRenderers)
        {
            meshRenderer.material.mainTexture = null;
        }
    }

    void Update()
    {
        if (_windowIndex != 0)
        {
            //Debug.Log("FxRWindow.Update() with _windowIndex == " + _windowIndex);
            fxr_plugin.fxrRequestWindowUpdate(_windowIndex, Time.deltaTime);
        }
        else
        {
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