using UnityEngine;

public class FxRDialogController : Singleton<FxRDialogController>
{
    [SerializeField] protected FxRDialogBox DialogBoxPrefab;
    [SerializeField] protected Transform DialogParent;

    private FxRDialogBox currentlyOpenedDialog;
    /*
     * Instantiate a dialog box at the proper location
     */
    // TODO: Allow for multiple types of dialogs that are pinned to their proper spot depending upon type
    // TODO: Only allow for a single instance of any given type of dialog? 
    public FxRDialogBox CreateDialog()
    {
        var dialogBox = Instantiate<FxRDialogBox>(DialogBoxPrefab,DialogParent.transform.position, DialogParent.transform.rotation, DialogParent);
        dialogBox.transform.localScale = Vector3.one;
        currentlyOpenedDialog = dialogBox;
        return dialogBox;
    }
}
