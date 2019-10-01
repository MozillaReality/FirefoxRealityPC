using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FxRDialogBox : MonoBehaviour
{
    [SerializeField] protected FxRDialogButton DialogButtonPrefab;
    [SerializeField] protected Transform ButtonContainer;
    [SerializeField] protected Image Icon;
    [SerializeField] protected TMP_Text HeaderText;
    [SerializeField] protected TMP_Text MessageText;
    [SerializeField] protected Image ProgressBar;

    // TODO: Do dialogs need to have an option to close them without pressing a button?
    public void Show(string title, string message, Sprite icon, params FxRDialogButton.ButtonConfig[] buttonConfigs)
    {
        foreach (var buttonConfig in buttonConfigs)
        {
            FxRDialogButton button = Instantiate<FxRDialogButton>(DialogButtonPrefab, ButtonContainer.position,
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

        HeaderText.text = title;
        MessageText.text = message;
        gameObject.SetActive(true);
        ProgressBar.gameObject.SetActive(false);
        Icon.sprite = icon;
        Icon.gameObject.SetActive(icon != null);
    }

    public void ShowProgress(float zeroToOne)
    {
        ProgressBar.gameObject.SetActive(true);
        ProgressBar.fillAmount = zeroToOne;
    }
    
    public void Close()
    {
        // TODO: Animation to have dialog go away?
        Destroy(gameObject);
    }
}