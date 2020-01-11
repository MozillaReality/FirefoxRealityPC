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
