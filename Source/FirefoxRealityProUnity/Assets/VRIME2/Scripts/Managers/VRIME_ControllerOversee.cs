// ========================================================================== //
//  Created: 2019-01-04
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;

	public enum eControllerType
	{
		Drum,
		Laser
	}
	[AddComponentMenu("VRIME/IMEManager/Controller Oversee")]
	public class VRIME_ControllerOversee : MonoBehaviour
	{
		public static VRIME_ControllerOversee Ins {
			get { return instance; }
			set {
				instance = value;
				instance.Init();
			}
		}
		private static VRIME_ControllerOversee instance;

		
		#region public Field
		public bool switchControllerModelByDevice = false;
		#endregion
		#region private Field
		private List<VRIMEControllerInfo> useControllerList;
		private eControllerType UseControllerType = eControllerType.Drum;
		private VRIMEControllerInfo lastUsingController;
		#endregion
		#region public Function
		/// <summary>
		/// User Controller Change To IME Drumstick
		/// </summary>
		/// <param name="iShow"></param>
		public void CallController(bool iShow)
		{
			// Outside Events
			if(VRIME_Manager.Ins.onControllerShow != null)
				VRIME_Manager.Ins.onControllerShow.Invoke(UseControllerType, iShow);
			// The Other Show Event.
			if(UseControllerType == eControllerType.Drum)
			{
				CallDrumstick(iShow);
			}
		}
		/// <summary>
		/// Check physical collision is drumstick collider.
		/// </summary>
		/// <param name="iObject">Enter collider objects.</param>
		/// <param name="iHeadCheck">Stick have "HeadPosition" and "HandlePostiton" two part.</param>
		/// <returns></returns>
		public bool CheckIsSitck(Collider iObject, bool iHeadCheck = true)
		{
			for(int i = 0; i < useControllerList.Count; i++)
			{
				VRIME_Drumstick aStick = useControllerList[i].IMEDrumstick;
                if(aStick == null)
                    continue;
				Transform aStickPart = aStick.HeadPosition;
				if(iHeadCheck == false)
					aStickPart = aStick.HandlePostiton;
                
				if(aStickPart == iObject.transform) {
                    return true;
				}
			}
            return false;
		}
		/// <summary>
		/// Set Drumstick Transform Position, Rotation, Scale
		/// </summary>
		/// <param name="iHand">left, right, twohand</param>
		/// <param name="iTrans">Use Local Value</param>
		public void SetControllerTransform(eStickType iHand, Transform iTrans)
		{
			if(iHand == eStickType.twohand)
			{
				// Set All
				for(int i = 0; i < useControllerList.Count; i++)
				{
					VRIMEControllerInfo aTmp = useControllerList[i];
					aTmp.Postiton = iTrans.localPosition;
					aTmp.Rotation = iTrans.localRotation.eulerAngles;
					aTmp.Scale = iTrans.localScale;
				}
			}
			else
			{
				// Get Stick
				VRIMEControllerInfo aTempHand = SearchControllerInfo(iHand);
				if(aTempHand == null)
					return;
				// Set Single
				aTempHand.Postiton = iTrans.localPosition;
				aTempHand.Rotation = iTrans.localRotation.eulerAngles;
				aTempHand.Scale = iTrans.localScale;
			}
		}
		/// <summary>
		/// Get All Using Drumstick
		/// </summary>
		/// <returns></returns>
		public VRIMEControllerInfo[] GetUsingControllers()
		{
			return useControllerList.ToArray();
		}
		/// <summary>
		/// Get Drimstick Object, left or right
		/// </summary>
		/// <param name="iType">left or rihgt, not twohand</param>
		/// <returns>IMEController 的GameObject</returns>
		public GameObject GetUsingDrumsticks(eStickType iType)
		{
			GameObject aResult = null;
			for(int i = 0; i < useControllerList.Count; i++)
			{
				if(iType == useControllerList[i].HandType)
				{
					aResult = useControllerList[i].IMEController;
					break;
				}
			}
			return aResult;
		}
		public void SetUseController(GameObject iObj)
		{
			VRIME_Drumstick aStick = iObj.GetComponentInParent<VRIME_Drumstick>();


			for(int i = 0; i < useControllerList.Count; i++)
			{
				VRIME_Drumstick aTemp = useControllerList[i].IMEController.GetComponent<VRIME_Drumstick>();
				if(aStick == aTemp)
				{
					lastUsingController = useControllerList[i];
					break;
				}
			}
		}
		public VRIMEControllerInfo GetLastController() { return lastUsingController; }
		#endregion
		#region private Function
		private void Init()
		{
			useControllerList = new List<VRIMEControllerInfo>();
		}
		/// <summary>
		/// Show And Make To Max Num
		/// </summary>
		/// <param name="iShow"></param>
		private void CallDrumstick(bool iShow)
		{
			// Left Hand Set
			GetControllerToShow(eStickType.left, VRIME_Manager.Ins.controllerModelLeft, "Drumstick_left", iShow);
			// Right Hand Set
			GetControllerToShow(eStickType.right, VRIME_Manager.Ins.controllerModelRight, "Drumstick_right", iShow);
		}
		/// <summary>
		/// refactor get controller to show path function
		/// </summary>
		/// <param name="iType">left or right</param>
		/// <param name="iUserModel">get by vrime_manager user controller models</param>
		/// <param name="iInfoName">also make stick object name</param>
		/// <param name="iShow">controller is show or hide</param>
		private void GetControllerToShow(eStickType iType, GameObject iUserModel, string iInfoName, bool iShow)
		{
			VRIMEControllerInfo aStickInfo = SearchControllerInfo(iType);
			Transform aStickParent = iUserModel.transform.parent;
			if(aStickInfo == null)
			{
				aStickInfo = new VRIMEControllerInfo(iInfoName, switchControllerModelByDevice);
				aStickInfo.HandType = iType;
				useControllerList.Add(aStickInfo);
			}
			aStickInfo.SetOutsideValue(iUserModel, aStickParent);
			aStickInfo.CallController(iShow);
		}

		private VRIMEControllerInfo SearchControllerInfo(eStickType iHand)
		{
			VRIMEControllerInfo aResult = null;
			for(int i = 0; i < useControllerList.Count; i++)
			{
				VRIMEControllerInfo aTemp = useControllerList[i];
				if(aTemp.HandType == iHand) {
					aResult = useControllerList[i];
					break;
				}
			}
			return aResult;
		}
		/// <summary>
		/// Clean Drumstick
		/// </summary>
		private void CleanUsingController()
		{
			if(useControllerList == null)
				return;

			for(int i = 0; i < useControllerList.Count; i++)
			{
				VRIMEControllerInfo aTmpInfo = useControllerList[i];
				aTmpInfo.Name = string.Empty;
				aTmpInfo.UserController = null;
				Destroy(aTmpInfo.IMEController);
				aTmpInfo.ShowControllerParent = null;
				useControllerList.Remove(aTmpInfo);
			}
			useControllerList = new List<VRIMEControllerInfo>();
		}
		#endregion
	}

	public class VRIMEControllerInfo
	{
		public string Name;
		public GameObject UserController;
		public GameObject IMEController;
		public Transform ShowControllerParent;
		public Vector3 Postiton = new Vector3(0, 0f, 0);
		public Vector3 Rotation = new Vector3(0f, 0, 0);
		public Vector3 Scale = Vector3.one;
		public eStickType HandType;
		public bool CheckDevice;
		public VRIME_Drumstick IMEDrumstick;
		private Vector3 handleBoxCenter = new Vector3(0f, 0f, 0.12f);
		private Vector3 handleBoxSize = new Vector3(0.04f, 0.04f, 0.4f);
		public VRIMEControllerInfo(){}
		public VRIMEControllerInfo(string iName, bool iCheck)
		{
			Name = iName;
			CheckDevice = iCheck;
			UserController = null;
			IMEController = GetDrumstickRes();
			ShowControllerParent = null;
		}

		public void SetOutsideValue(GameObject iCtrl, Transform iParent)
		{
			if(UserController != iCtrl) {
				UserController = iCtrl;
			}
			if(ShowControllerParent != iParent) {
				ShowControllerParent = iParent;
			}
		}
		public void CallController(bool iShow)
		{
			if(UserController != null)// If Contrller Not Null, Show/Hide It.
				UserController.SetActive(!iShow);
			if(IMEController == null) {
				VRIME_Debugger.LogError(this.Name, "NOT FOUND IME CONTROLLER!!");
				return;
			}
			// Show Controller And Reset LocalPostiton
			if(ShowControllerParent != null)
			{
				IMEController.transform.parent = iShow ? ShowControllerParent : VRIME_Manager.Ins.BackgroundObjPath;
				IMEController.transform.localPosition = Postiton;
				IMEController.transform.localRotation = Quaternion.Euler(Rotation);
				IMEController.transform.localScale = Scale;
				IMEController.SetActive(iShow);
			}
			else
			{
				IMEController.transform.parent = VRIME_Manager.Ins.BackgroundObjPath;
				IMEController.SetActive(false);
			}
		}
		/// <summary>
		/// get drumstick by unity Resource
		/// </summary>
		/// <param name="iName">修改在場景上的物件名稱</param>
		/// <returns></returns>
		private GameObject GetDrumstickRes()
		{
			// Check Device Type To Load Resource
			string aResPathName = GetResourcePath(CheckDevice);
			// Return Load Done Stcik
			GameObject aResult = VRIME_AssetLoad.GetGameObjectResource(aResPathName);
			aResult.transform.parent = VRIME_Manager.Ins.BackgroundObjPath;
			if(string.IsNullOrEmpty(Name) == false) {
				aResult.name = Name;
			}
			// Chnage Handle BoxCollider Size
			IMEDrumstick = aResult.GetComponent<VRIME_Drumstick>();
			if(IMEDrumstick != null)
			{
				BoxCollider aBoxHandle = IMEDrumstick.HandlePostiton.GetComponent<BoxCollider>();
				if(aBoxHandle != null)
				{
					aBoxHandle.center = handleBoxCenter;
					aBoxHandle.size = handleBoxSize;
				}
			}
			aResult.SetActive(false);
			return aResult;
		}
		private string GetResourcePath(bool iCheckDevice)
		{
			string aBasePath = "Prefabs/Drumstick_";
			string aResPathName = "default";
			if(iCheckDevice)
			{
				string model_name = UnityEngine.XR.XRDevice.model;
				if(string.IsNullOrEmpty(model_name) == false && model_name.ToLower().Contains("vive"))
				{
					aResPathName = "vive";
				}
			}
			return aBasePath + aResPathName;
		}
	}
}