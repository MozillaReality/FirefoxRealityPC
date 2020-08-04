// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019-2020, Mozilla.

using System.Collections.Generic;
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