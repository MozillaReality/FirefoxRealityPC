using UnityEngine;
using System.Collections;

public class FxRWindow : MonoBehaviour
{
    public Vector2 InitialSize = new Vector2(4.0f, 2.25f);
    public Vector2Int  InitialVideoSize = new Vector2Int(1920, 1080);
    private Vector2 size;
    private Vector2Int videoSize;
    private GameObject _videoMeshGO = null; // The GameObject which holds the MeshFilter and MeshRenderer for the video. 
    private Texture2D _videoTexture = null;  // Texture object with the video image.
    private Material _videoMaterial  = null;  // Material which uses our "VideoPlaneNoLight" shader, and paints itself with _videoTexture.

    // Use this for initialization
    void Start()
    {
        size = InitialSize;
        videoSize = InitialVideoSize;

        _videoMeshGO = CreateVideoMesh(size.x, size.y, videoSize.x, videoSize.y, 0, out _videoTexture, out _videoMaterial);
        _videoMeshGO.transform.parent = this.gameObject.transform;
        _videoMeshGO.transform.localPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Creates a GameObject in layer 'layer' which renders a mesh displaying the video stream.
    // Places references to the Color array (as required), the texture and the material into the out parameters.
    private GameObject CreateVideoMesh(float width, float height, int videoWidth, int videoHeight, int layer, out Texture2D vt, out Material vm)
    {
        // Check parameters.
        if (videoWidth <= 0 || videoHeight <= 0)
        {
            Debug.LogError("Error: Cannot configure video texture with invalid video size: " + videoWidth + "x" + videoHeight);
            vt = null; vm = null;
            return null;
        }

        // Create new GameObject to hold mesh.
        GameObject vmgo = new GameObject("Video source");
        if (vmgo == null) {
            Debug.LogError("Error: CreateVideoMesh cannot create GameObject.");
            vt = null; vm = null;
            return null;
        }
        vmgo.layer = layer;

        // Work out size of required texture.
        int textureWidth;
        int textureHeight;
        /*if (dontUseNPOT) {*/
        textureWidth = videoWidth;
        textureHeight = videoHeight;
        /*} else {
            textureWidth = Mathf.ClosestPowerOfTwo(videoWidth);
            if (textureWidth < videoWidth) textureWidth *= 2;
            textureHeight = Mathf.ClosestPowerOfTwo(videoHeight);
            if (textureHeight < videoHeight) textureHeight *= 2;
        }*/
        Debug.Log("Video size " + videoWidth + "x" + videoHeight + " will use texture size " + textureWidth + "x" + textureHeight + ".");

        float textureScaleU = (float)videoWidth / (float)textureWidth;
        float textureScaleV = (float)videoHeight / (float)textureHeight;
        //Debug.Log("Video texture coordinate scaling: " + textureScaleU + ", " + textureScaleV);

        vt = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        //vt = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
        vt.hideFlags = HideFlags.HideAndDontSave;
        vt.filterMode = FilterMode.Bilinear;
        vt.wrapMode = TextureWrapMode.Clamp;
        vt.anisoLevel = 0;

        // Initialise the video texture to black.
        Color32[] arr = new Color32[textureWidth * textureHeight];
        Color32 blackOpaque = new Color32(0, 0, 0, 255);
        for (int i = 0; i < arr.Length; i++) arr[i] = blackOpaque;
        vt.SetPixels32(arr);
        vt.Apply(); // Pushes all SetPixels*() ops to texture.
        arr = null;

        // Create a material tied to the texture.
        Shader shaderSource = Shader.Find("VideoPlaneNoLight");
        vm = new Material(shaderSource); //fxrUnity.Properties.Resources.VideoPlaneShader;
        vm.hideFlags = HideFlags.HideAndDontSave;
        vm.mainTexture = vt;
        //Debug.Log("Created video material");

        // Now create a mesh appropriate for displaying the video, a mesh filter to instantiate that mesh,
        // and a mesh renderer to render the material on the instantiated mesh.
        bool flipX = false;
        bool flipY = false;
        Mesh m = new Mesh();
        m.Clear();
        m.vertices = new Vector3[] {
                new Vector3(-width*0.5f, 0.0f, 0.0f),
                new Vector3(width*0.5f, 0.0f, 0.0f),
                new Vector3(width*0.5f,  height, 0.0f),
                new Vector3(-width*0.5f,  height, 0.0f),
            };
        m.normals = new Vector3[] {
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
            };
        float u1 = flipX ? textureScaleU : 0.0f;
        float u2 = flipX ? 0.0f : textureScaleU;
        float v1 = flipY ? textureScaleV : 0.0f;
        float v2 = flipY ? 0.0f : textureScaleV;
        m.uv = new Vector2[] {
                new Vector2(u1, v1),
                new Vector2(u2, v1),
                new Vector2(u2, v2),
                new Vector2(u1, v2)
            };
        m.triangles = new int[] {
                2, 1, 0,
                3, 2, 0
            };

        MeshFilter filter = vmgo.AddComponent<MeshFilter>();
        filter.mesh = m;

        MeshRenderer meshRenderer = vmgo.AddComponent<MeshRenderer>();
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        vmgo.GetComponent<Renderer>().material = vm;

        return vmgo;
    }

    private void DestroyVideoMesh()
    {
        bool ed = Application.isEditor;

        if (_videoMaterial != null)
        {
            if (ed) DestroyImmediate(_videoMaterial);
            else Destroy(_videoMaterial);
            _videoMaterial = null;
        }
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
