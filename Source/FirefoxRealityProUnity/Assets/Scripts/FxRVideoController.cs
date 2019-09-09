using System;
using System.Collections.Generic;
using UnityEngine;

public class FxRVideoController : Singleton<FxRVideoController>
{
    public delegate void ImmersiveScreenVideoClosed(int windowIndex);

    public static ImmersiveScreenVideoClosed OnImmersiveVideoClosed;
    public FxRPlugin fxr_plugin = null; // Reference to the plugin. Will be set/cleared by FxRController.

    [SerializeField] protected List<GameObject> ObjectsToHide;
    [SerializeField] protected GameObject VideoControls;

    private List<GameObject> _objectsHidden = new List<GameObject>();
    private Texture2D _videoTexture = null; // Texture object with the video image.

    private GameObject _videoSphere;
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

    // TODO: Support multiple video modes: 360 Video, 180 left/right, 180 top/bottom, etc
    public void ShowVideo(Texture2D videoTexture, int windowIndex)
    {
        _windowIndex = windowIndex;
        _videoSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _videoSphere.layer = 0;

        // Flip the mesh uv's so it renders right-side-up
        var mesh = _videoSphere.GetComponent<MeshFilter>().mesh;
        var uvs = mesh.uv;
        List<Vector2> flippedUVs = new List<Vector2>(uvs.Length);
        foreach (var uv in uvs)
        {
            flippedUVs.Add(new Vector2(uv.x, 1-uv.y));
        }

        mesh.uv = flippedUVs.ToArray();

        _videoSphere.transform.localScale = new Vector3(100f, 100f, 100f);

        // Set up the material
        Shader shaderSource = Shader.Find("Unlit/InsideOut");
        Material vm = new Material(shaderSource);
        vm.hideFlags = HideFlags.HideAndDontSave;
        vm.mainTexture = videoTexture;

        // Set up the mesh renderer
        MeshRenderer meshRenderer = _videoSphere.GetComponent<MeshRenderer>();
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        _videoSphere.GetComponent<Renderer>().material = vm;

        MeshCollider vmc = _videoSphere.AddComponent<MeshCollider>();
       
        _videoSphere.transform.parent = transform;
        _videoSphere.transform.localPosition = Vector3.zero;
        // TODO: Should rotate so it is oriented to direction user is facing when video starts?
        _videoSphere.transform.localRotation = Quaternion.identity;

        foreach (var objectToHide in ObjectsToHide)
        {
            if (objectToHide.activeSelf)
            {
                objectToHide.SetActive(false);
                _objectsHidden.Add(objectToHide);
            }
        }

        VideoControlsVisible = true;
    }

    public void ToggleVideoControlsVisibility()
    {
        VideoControlsVisible = !VideoControlsVisible;
    }

    public void ExitVideo()
    {
        if (_videoSphere != null)
        {
            var videoTexture = _videoSphere.GetComponentInChildren<MeshRenderer>().material.mainTexture;
            _videoSphere.GetComponentInChildren<MeshRenderer>().material.mainTexture = null;
            Destroy(_videoSphere);
            Destroy(videoTexture);
            _videoSphere = null;
        }

        foreach (var objectHidden in _objectsHidden)
        {
            objectHidden.SetActive(true);
        }

        _objectsHidden.Clear();
        VideoControlsVisible = false;
        
        OnImmersiveVideoClosed?.Invoke(_windowIndex);        
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