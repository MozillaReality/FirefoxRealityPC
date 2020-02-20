// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019, Mozilla.

﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FxRDialogBox : MonoBehaviour
{
    [SerializeField] protected FxRButton DialogButtonPrefab;
    [SerializeField] protected Transform ButtonContainer;
    [SerializeField] protected Image Icon;
    [SerializeField] protected TMP_Text TitleText;
    [SerializeField] protected TMP_Text BodyText;
    [SerializeField] protected Image ProgressBar;
    [SerializeField] protected Image BackgroundImage;
    
    public delegate void DialogBoxOpen();
    public static DialogBoxOpen OnDialogBoxOpen;

    public delegate void DialogBoxClosed();
    public static DialogBoxClosed OnAllDialogBoxesCLosed;
    
    private static int DialogsOpenCount = 0;

    // TODO: Do dialogs need to have an option to close them without pressing a button?
    public void Show(string title, string message, Sprite icon, params FxRButton.ButtonConfig[] buttonConfigs)
    {
        foreach (var buttonConfig in buttonConfigs)
        {
            FxRButton button = Instantiate<FxRButton>(DialogButtonPrefab, ButtonContainer.position,
                ButtonContainer.rotation, ButtonContainer);
            button.transform.localScale = Vector3.one;

            var action = buttonConfig.ButtonPressedAction;
            buttonConfig.ButtonPressedAction = () =>
            {
                action?.Invoke();
                Close();
            };
            button.Config = buttonConfig;
        }

        TitleText.text = title;
        BodyText.text = message;
        gameObject.SetActive(true);
        ProgressBar.gameObject.SetActive(false);
        Icon.sprite = icon;
        Icon.gameObject.SetActive(icon != null);
        BackgroundImage.color = FxRConfiguration.Instance.ColorPalette.NormalBrowsingDialogBackgroundColor;
        TitleText.color = FxRConfiguration.Instance.ColorPalette.DialogTitleTextColor;
        BodyText.color = FxRConfiguration.Instance.ColorPalette.DialogBodyTextColor;
        DialogsOpenCount++;
        OnDialogBoxOpen?.Invoke();
    }

    public void ShowProgress(float zeroToOne)
    {
        ProgressBar.gameObject.SetActive(true);
        ProgressBar.fillAmount = zeroToOne;
    }

    public void Close()
    {
        DialogsOpenCount--;
        if (DialogsOpenCount == 0)
        {
            OnAllDialogBoxesCLosed?.Invoke();
        }
        // TODO: Animation to have dialog go away?
        Destroy(gameObject);
    }

    public void UpdateText(string title, string message)
    {
        TitleText.text = title;
        BodyText.text = message;
    }
}