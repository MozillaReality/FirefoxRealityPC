// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019-2020, Mozilla.

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
        // Set up the lighting and skybox
        RenderSettings.skybox = Instantiate(SkyboxMaterial);
        RenderSettings.ambientMode = AmbientLightSource;
        RenderSettings.ambientLight = AmbientLightColor;
        // Let the environment render before we render the reflections
        yield return new WaitForEndOfFrame();
        // Snapshot the reflections
        // TODO: this could probably be baked 
        foreach (var reflectionProbe in ReflectionProbs)
        {
            reflectionProbe.RenderProbe();
        }

        // Rotate the skybox opposite the direction the environment is rotated, so it properly aligns 
        if (RenderSettings.skybox.HasProperty("_Rotation"))
        {
            var baseRotation = SkyboxMaterial.GetFloat("_Rotation");
            var adjustedRotation = baseRotation - transform.rotation.eulerAngles.y;
            RenderSettings.skybox.SetFloat("_Rotation", adjustedRotation);
        }
    }

}