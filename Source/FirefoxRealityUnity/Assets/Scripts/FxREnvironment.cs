using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class FxREnvironment : MonoBehaviour
{
    [SerializeField] private Material SkyboxMaterial;
    [SerializeField] private AmbientMode AmbientLightSource;
    [SerializeField]  [ColorUsage(true, true)] private Color AmbientLightColor;

    // Start is called before the first frame update
    void Start()
    {
        RenderSettings.skybox = SkyboxMaterial;
        RenderSettings.ambientMode = AmbientLightSource;
        RenderSettings.ambientLight = AmbientLightColor;
    }

}
