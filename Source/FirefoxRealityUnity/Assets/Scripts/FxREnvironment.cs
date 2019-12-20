using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FxREnvironment : MonoBehaviour
{
    [SerializeField] private Material SkyboxMaterial;
    [SerializeField] private AmbientMode AmbientLightSource;
    [SerializeField] [ColorUsage(true, true)] private Color AmbientLightColor;
    [SerializeField] private List<ReflectionProbe> ReflectionProbs;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        RenderSettings.skybox = SkyboxMaterial;
        RenderSettings.ambientMode = AmbientLightSource;
        RenderSettings.ambientLight = AmbientLightColor;
        yield return new WaitForSeconds(.5f);
        foreach (var reflectionProbe in ReflectionProbs)
        {
            reflectionProbe.RenderProbe();
        }
        
    }

}