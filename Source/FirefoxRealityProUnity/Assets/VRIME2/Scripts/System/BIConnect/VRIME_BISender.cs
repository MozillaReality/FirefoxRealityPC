// ========================================================================== //
//  Created: 2019-04-16
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using UnityEngine.Networking;
#if steamvr_v2
	using Valve.VR;
#endif

    public partial class VRIME_BISender
	{
		public static VRIME_BISender Ins {
			get {
				if(instance == null) {
					Init();
				}
				return instance;
			}
		}
		private static VRIME_BISender instance;
		#region Keys
		private const string KEY_EVENT = "ev";
        private const string KEY_A0 = "a0";
        private const string KEY_A1 = "a1";
        private const string KEY_A2 = "a2";
        private const string KEY_A3 = "a3";
        private const string KEY_A4 = "a4";
        private const string KEY_A5 = "a5";
        private const string KEY_A6 = "a6";
        private const string KEY_A7 = "a7";
        private const string KEY_A8 = "a8";
        private const string KEY_A9 = "a9";
        private const string KEY_A10 = "a10";
		private const string KEY_SESSION_ID = "session_id";
		private const string KEY_SERIAL_NUM = "serial_num";
		private const string KEY_HMD_TYPE = "hmd_type";
        private const string KEY_HMD_SERIAL = "hmd_serial";
		private const string KEY_COUNTRY = "country";
		private const string KEY_BI_VERSION = "bi_data_point_version";
		#endregion
		#region Values
		private const string BI_VERSION = "1.1";
		private const string RUNTIME_STEAM_VR = "SteamVR";
		#endregion
		#region Init Check
		public static bool StageMode {
			get { return mStageStatus; }
			set {
				mStageStatus = value;
				BILogInit(true);
			}
		}
		private static bool mStageStatus = false;
		private static bool mBILogInit = false;
		#endregion
		#region private Field
		private string mHmdType;
        private string mHmdSerial;
		private string mSessionId;
		private string mViveCountryCode;
		private bool mStopSend = true;
		private Coroutine mRunningCoroutine;
		private string mSerialNum;
		#endregion
		#region public Function
		public static void Init()
		{
			instance = new VRIME_BISender();
			// Input Method
			instance.InputMethodInit();
			// Other Data Init
			instance.mSerialNum = instance.GetDeviceSerialNumber();
			if(VRIME_Manager.Ins)
			{
				VRIME_Manager.Ins.StartCoroutine(instance.getHMDInfo());
				VRIME_Manager.Ins.StartCoroutine(instance.getEngineTypeByNetwork());
			}
			
			BILogInit();
			VRIME_Debugger.Log("BI Sender Init Done.");
		}
		/// <summary>
		/// Call BI Flush Function
		/// </summary>
		/// <param name="iIsAll"></param>
		public void FlushData(bool iIsAll = true)
		{
#if !UNITY_EDITOR
			if(iIsAll)
			{
				BILogScope.BI.FlushAll();
			}
			else
			{
				BILogScope.BI.Flush();
			}
#endif
		}		
		#endregion
		#region private Function
		/// <summary>
		/// BI Data Point Init
		/// </summary>
		/// <param name="iForce"></param>
		private static void BILogInit(bool iForce = false)
		{
			if(iForce == false)
			{
				if(mBILogInit)
					return;
			}

			mBILogInit = BILogScope.Init(mStageStatus);
			VRIME_Debugger.Log("BILog Init Result :" + mBILogInit);
		}
		/// <summary>
		/// Auto Flush by VRIME_Manager Invoke
		/// </summary>
		private IEnumerator AutoFlushData()
		{
			yield return new WaitForSeconds(3f);
			if(mStopSend)
				yield break;
			
			FlushData();
			mRunningCoroutine = null;
			VRIME_Debugger.Log("BI Sender Flush-All Done.");
		}
		/// <summary>
		/// Get Country Code By ViveStore, StartCoroutine by VRIME_Manager
		/// code copy form keyboardAudioInput.cs
		/// </summary>
		/// <returns></returns>
		private IEnumerator getEngineTypeByNetwork()
		{
			mViveCountryCode = string.Empty;
			UnityWebRequest request = UnityWebRequest.Get("https://store.viveport.com/api/whichcountry/v1/plain");
			yield return request.SendWebRequest();

			if (request.isNetworkError)
			{
				VRIME_Debugger.Log(request.error);
				yield break;
			}
			// Save Result
			mViveCountryCode = request.downloadHandler.text;
			VRIME_Debugger.Log("ViveCountryCode :"+ mViveCountryCode);
		}
		/// <summary>
		/// Get Instance By StartCoroutine
		/// </summary>
		/// <returns></returns>
		private IEnumerator getHMDInfo()
		{
			yield return new WaitForSeconds(0f);
			var instance = SteamVR.instance;
			// Save Result
            if (instance != null)
            {
                mHmdType = instance.hmd_ModelNumber;
                mHmdSerial = instance.hmd_SerialNumber;
            }
			VRIME_Debugger.Log("HMD Type :" + mHmdType + ", HMD Serial :" + mHmdSerial);
		}
		/// <summary>
		/// Because Only Use Vector3 will do rounding, but real Vector
		/// </summary>
		/// <param name="iValue"></param>
		/// <returns></returns>
		private string Vector3ToString(Vector3 iValue)
		{
			float aX = iValue.x;
			float aY = iValue.y;
			float aZ = iValue.z;

			return aX + "," + aY + "," + aZ;
		}
		/// <summary>
		/// IME Language Get Check
		/// </summary>
		/// <param name="iLan"></param>
		/// <returns></returns>
		private string GetIMELanguageText(eLanguage iLan)
		{
			string aResult = string.Empty;
			switch(iLan)
			{
				case eLanguage.English:
					aResult = eLanguage.English.ToString();
					break;
				default:
					aResult = iLan.ToString();
					break;
			}
			return aResult;
		}
		/// <summary>
		/// Default get device serial number is SystemInfo.deviceUniqueIdentifier.
		/// If device is SystemInfo.unsupportedIdentifier, return we set it up words.
		/// </summary>
		/// <returns></returns>
		private string GetDeviceSerialNumber()
		{
			if (SystemInfo.unsupportedIdentifier != SystemInfo.deviceUniqueIdentifier)
			{
				return SystemInfo.deviceUniqueIdentifier;
			}
			else
			{
				return "N/A";
			}
		}
		#endregion
		#region CommonData		
		/// <summary>
		/// I think mostly data will be use this iformation
		/// </summary>
		private void AddDataCommon()
		{
			BILogScope.BI.AddData(KEY_SESSION_ID, mSessionId);
			BILogScope.BI.AddData(KEY_SERIAL_NUM, mSerialNum);
			BILogScope.BI.AddData(KEY_HMD_TYPE, mHmdType); // ex: Vive. MV
			BILogScope.BI.AddData(KEY_HMD_SERIAL, mHmdSerial);
			BILogScope.BI.AddData(KEY_COUNTRY, mViveCountryCode);
			BILogScope.BI.AddData(KEY_BI_VERSION, BI_VERSION);
		}
		#endregion
	}
}
/*
VR IME SDK v1.1 data points (https://htcsense.jira.com/wiki/spaces/BI/pages/606700239/REVIEWED+VR+IME+SDK+v1.1+data+points)
examele:
category {
	dataType key : Value(s) // annotation
}
=========

activity_and_usage {
	string ev : "show" || "done" || "close"
	string a0 : "SteamVR" || "WaveVR" || ... // Lyon: Current version only supports SteamVR.
	string a1 : "Windows 7 64bit" || "Windows 10 64bit" || ...
	string a2 : "ViveVideo" || "VPNextApp" || ...
	string a3 : "155,55,80" // Keyboard grobal position
	string a4 : "10,0,0" // Keyboard rotation
	string a5 : "1.0" // Keyboard scale
	string a6 : "Vive" || "Rift" || ... // Controller model ID
	string a7 : "2.0.0" || "2.0.1" || ... // VR IME SDK version code
	string a8 : "search" || "log_in" || ... // This value is assigned by developer, so probably it is only useful for internal developer.
Call method like ShowIME("search")
	stirng session_id : Unique identifier
	string serial_num : "270e0594db9ee0e2e5ab339565b3" // Serial number of PC computer or all-in-one device
	string hmd_type : "Vive" || "Vive Pro" || "Oculus Rift" || ... 
	string hmd_serial : "FA63ZJJ01539" // Serial number of HMD (Head-mounted Display)
	string country : "CN" || "TW" || "US" || ...
	long tms : 1514539617946 // Timestamp in milliseconds
	long tz : 28800000 (+8 hours, CST) // Timezone offset in milliseconds
	string bi_data_point_version : "1.0" // BI data point version
}
*/