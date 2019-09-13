using System.Collections.Generic;
using UnityEngine;

public class FxRBrowserModeVisibilityManager : MonoBehaviour
{
    public List<FxRController.FXR_BROWSING_MODE> ApplicableBrowserModes;
    public List<GameObject> GameObjectsToEnable;

    private void OnEnable()
    {
        HandleBrowserModeChanged(FxRController.CurrentBrowsingMode);
        FxRController.OnBrowsingModeChanged += HandleBrowserModeChanged;
    }

    private void OnDisable()
    {
        FxRController.OnBrowsingModeChanged -= HandleBrowserModeChanged;
    }

    private void HandleBrowserModeChanged(FxRController.FXR_BROWSING_MODE browsingMode)
    {
        bool shouldBeActive = ApplicableBrowserModes.Contains(browsingMode);
        foreach (var go in GameObjectsToEnable)
        {
            go.SetActive(shouldBeActive);
        }
    }
}