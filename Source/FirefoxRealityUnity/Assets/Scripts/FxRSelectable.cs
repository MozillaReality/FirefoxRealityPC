// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019-2020, Mozilla.

using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class FxRSelectable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
    IPointerUpHandler
{
    protected Selectable Selectable
    {
        get
        {
            if (selectable == null)
            {
                selectable = GetComponent<Selectable>();
            }

            return selectable;
        }
    }

    private Selectable selectable;

    [SerializeField] protected TMP_Text ButtonLabel;
    [SerializeField] protected Image BackgroundImage;
    [SerializeField] protected Image Border;
    [SerializeField] protected Image Icon;
    [SerializeField] private SelectableConfig config;
    private bool boxColliderSizeSet;

    [Serializable]
    public class SelectableConfig
    {
        public string ButtonLabel;
        [SerializeField] protected FxRButtonLogicalColorConfig LogicalColorConfig;

        public FxRButtonLogicalColorConfig LogicialColors
        {
            get => LogicalColorConfig;
            set
            {
                LogicalColorConfig = value;
                // Reset color config, so it gets re-created next time it is needed
                colorConfig = null;
            }
        }

        public FxRButtonColorConfig ColorConfig
        {
            get
            {
                if (colorConfig == null)
                {
                    colorConfig =
                        FxRConfiguration.Instance.ColorPalette.CreateButtonColorConfigForLogicalConfig(
                            LogicalColorConfig);
                }

                return colorConfig;
            }
        }

        private FxRButtonColorConfig colorConfig;

        public SelectableConfig(string label, FxRButtonLogicalColorConfig colorConfig)
        {
            ButtonLabel = label;
            LogicalColorConfig = colorConfig;
        }
    }

    public virtual SelectableConfig Config
    {
        set
        {
            config = value;

            ButtonLabel.text = !string.IsNullOrEmpty(config.ButtonLabel) ? config.ButtonLabel : ButtonLabel.text;
            ButtonLabel.color = config.ColorConfig.NormalTextColor;
            BackgroundImage.color = config.ColorConfig.NormalColor;
            Border.color = config.ColorConfig.BorderColor;
            Border.gameObject.SetActive(config.ColorConfig.HasBorder);
            Icon.color = config.ColorConfig.NormalIconColor;
        }

        protected get { return config; }
    }

    protected virtual void OnEnable()
    {
        // Initialize the config, if necessary
        if (config != null)
        {
            Config = config;
        }
    }

    private void Update()
    {
        // Add a box collider to the button so that the laser pointer will work.
        // TODO: Refactor this out into a general class that can add the requisite box colliders to a tree of objects.
        if (boxColliderSizeSet) return;
        var rectTransform = gameObject.GetComponent<RectTransform>();
        if (rectTransform.sizeDelta.Equals(Vector2.zero)) return;
        var boxCOllider = gameObject.AddComponent<BoxCollider>();
        boxCOllider.size = new Vector3(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y, 1f);
        boxColliderSizeSet = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        BackgroundImage.color = Config.ColorConfig.HoverColor;
        ButtonLabel.color = Config.ColorConfig.HoverTextColor;
        Icon.color = config.ColorConfig.HoverIconColor;
        Border.gameObject.SetActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        BackgroundImage.color = Config.ColorConfig.NormalColor;
        ButtonLabel.color = Config.ColorConfig.NormalTextColor;
        Icon.color = config.ColorConfig.NormalIconColor;
        Border.gameObject.SetActive(config.ColorConfig.HasBorder);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        BackgroundImage.color = Config.ColorConfig.PressedColor;
        ButtonLabel.color = Config.ColorConfig.PressedTextColor;
        Icon.color = config.ColorConfig.PressedIconColor;
        Border.gameObject.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        BackgroundImage.color = Config.ColorConfig.NormalColor;
        ButtonLabel.color = Config.ColorConfig.NormalTextColor;
        Icon.color = config.ColorConfig.NormalIconColor;
        Border.gameObject.SetActive(config.ColorConfig.HasBorder);
    }
}