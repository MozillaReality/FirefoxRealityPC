// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2020, Mozilla.

/*
 * This class is used to ensure that all of the video projection choices in the video projection menu are
 * wide enough to accommodate the strings for every potential language, by auto-sizing the width of the grid
 * containing them to the width of the largest text field preferred size.
 */
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class FxRAutoSizeVideoProjectionMenu : MonoBehaviour
{
    public float additionalPadding = 58f;
    public float minWidthOfTextFields = 130f;
    private void OnEnable()
    {
        StartCoroutine(AdjustGridWidth());
    }

    private IEnumerator AdjustGridWidth()
    {
        yield return new WaitForEndOfFrame();
        float maxWidth = minWidthOfTextFields;
        foreach (var label in SelectionToggleTextLabels)
        {
            if (label.preferredWidth > maxWidth)
            {
                maxWidth = label.preferredWidth;
            }
        }
        MyGridLayoutGroupGroup.cellSize = new Vector2(additionalPadding + maxWidth, MyGridLayoutGroupGroup.cellSize.y);
    }

    private TMP_Text[] SelectionToggleTextLabels
    {
        get
        {
            if (selectionToggleTextLabels == null)
            {
                selectionToggleTextLabels = GetComponentsInChildren<TMP_Text>();
            }

            return selectionToggleTextLabels;
        }
    }

    private TMP_Text[] selectionToggleTextLabels;

    private GridLayoutGroup MyGridLayoutGroupGroup
    {
        get
        {
            if (myGridLayoutGroup == null)
            {
                myGridLayoutGroup = GetComponent<GridLayoutGroup>();
            }

            return myGridLayoutGroup;
        }
    }

    private GridLayoutGroup myGridLayoutGroup;
}
