using UnityEngine;
using Valve.VR;
using VRIME2;

public class FxRVRIMEInitializer : Singleton<FxRVRIMEInitializer>
{
    public Transform VRIMEKeyboardParent;
    // References needed to satisfy the VRIME_Keyboard
    [SerializeField] private GameObject ControllerModelLeft;
    [SerializeField] private GameObject ControllerModelRight;

    [SerializeField] private SteamVR_Behaviour_Pose UserControllerLeft;
    [SerializeField] private SteamVR_Behaviour_Pose UserControllerRight;
    [SerializeField] private Transform UserHeadCamera;
    
    public void InitializeVRIMEKeyboard(VRIME_Manager vrimeManager)
    {
        if (VRIMEKeyboardParent == null || ControllerModelLeft == null)
        {
            return;
        }
        vrimeManager.transform.SetParent(VRIMEKeyboardParent, false);
        vrimeManager.transform.position = Vector3.zero;
        vrimeManager.transform.localScale = Vector3.one;
        vrimeManager.transform.rotation = Quaternion.identity;

        vrimeManager.controllerModelLeft = ControllerModelLeft;
        vrimeManager.controllerModelRight = ControllerModelRight;
        vrimeManager.userControllerLeft = UserControllerLeft;
        vrimeManager.userControllerRight = UserControllerRight;
        vrimeManager.userHeadCamera = UserHeadCamera;
    }
}
