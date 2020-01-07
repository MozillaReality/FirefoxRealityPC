using TMPro;
using UnityEngine;

public class FxRLocalizedStringObserver : MonoBehaviour
{
    [SerializeField] protected string LocalizedStringKey;
    [SerializeField] protected TMP_Text TextField;

    private void OnEnable()
    {
        FxRLocalizedStringsLoader.onLocalizedStringsLoaded += HandleLocalizedStringsLoaded;
        HandleLocalizedStringsLoaded();
    }

    private void HandleLocalizedStringsLoaded()
    {
        TextField.text = FxRLocalizedStringsLoader.GetApplicationString(LocalizedStringKey);
    }
}