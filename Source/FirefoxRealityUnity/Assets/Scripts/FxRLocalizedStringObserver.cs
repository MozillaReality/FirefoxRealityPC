using System;
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
    }
}