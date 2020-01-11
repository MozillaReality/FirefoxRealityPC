// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2020, Mozilla.

using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class FxRLocalizedStringObserver : MonoBehaviour
{
    [SerializeField] protected string LocalizedStringKey;

    private TMP_Text TextField
    {
        get
        {
            if (textField == null)
            {
                textField = GetComponent<TMP_Text>();
            }

            return textField;
        }
    }

    private TMP_Text textField;

    private void OnEnable()
    {
        FxRLocalizedStringsLoader.onLocalizedStringsLoaded += HandleLocalizedStringsLoaded;
        HandleLocalizedStringsLoaded();
    }

    private void OnDisable()
    {
        FxRLocalizedStringsLoader.onLocalizedStringsLoaded -= HandleLocalizedStringsLoaded;
    }

    private void HandleLocalizedStringsLoaded()
    {
        TextField.text = FxRLocalizedStringsLoader.GetApplicationString(LocalizedStringKey);
        Debug.LogWarningFormat("Preferred width: {0:F2}", TextField.preferredWidth);
        
    }
}