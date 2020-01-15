// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2020, Mozilla.

/*
 * This class can be attached to a TextMesh Pro text field, to keep its text contents updated based upon
 * the current language translation for a specified localized string key. This class populates the string
 * when it is enabled, and also listens for the "OnLocalizedStringsLoaded" event from the FxRLocalizedStringsLoader.
 */
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
        FxRLocalizedStringsLoader.OnLocalizedStringsLoaded += HandleLocalizedStringsLoaded;
        HandleLocalizedStringsLoaded();
    }

    private void OnDisable()
    {
        FxRLocalizedStringsLoader.OnLocalizedStringsLoaded -= HandleLocalizedStringsLoaded;
    }

    private void HandleLocalizedStringsLoaded()
    {
        TextField.text = FxRLocalizedStringsLoader.GetApplicationString(LocalizedStringKey);
    }
}