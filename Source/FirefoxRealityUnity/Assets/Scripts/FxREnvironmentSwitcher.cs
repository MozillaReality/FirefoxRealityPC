﻿using System.Collections.Generic;
using UnityEngine;

public class FxREnvironmentSwitcher : MonoBehaviour
{
    [SerializeField] private Transform EnvironmentParent;
    [SerializeField] private List<FxREnvironment> EnvironmentPrefabs;

    private FxREnvironment CurrentEnvironment;

    // Update is called once per frame
    public void SwitchEnvironment(int environmentIndex)
    {
        if (CurrentEnvironment != null)
        {
            Destroy(CurrentEnvironment.gameObject);
        }
        CurrentEnvironment = Instantiate(EnvironmentPrefabs[environmentIndex]
            , EnvironmentParent.transform.position
            , EnvironmentParent.transform.rotation
            , EnvironmentParent);
    }
}