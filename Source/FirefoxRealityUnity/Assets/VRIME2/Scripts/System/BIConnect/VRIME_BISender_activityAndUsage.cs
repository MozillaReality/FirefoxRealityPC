// ========================================================================== //
//  Created: 2019-04-25
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public partial class VRIME_BISender
	{
		#region Const
		private const string CATEGORY_ACTIVITY_AND_USAGE = "activity_and_usage";
		private const string USAGE_EV_SHOW = "show";
		private const string USAGE_EV_CLOSE = "close";
		private const string USAGE_EV_DONE = "done";

		#endregion
		#region public Field
		public string AppEntry {
			get { return mAppEntryName; }
			set { mAppEntryName = value; }
		}
		private string mAppEntryName;
		#endregion
		#region public Function
		/// <summary>
		/// activity_and_usage, ev = IME show/hide(close)
		/// </summary>
		/// <param name="iShow"></param>
		public void CallIMEStauts(bool iShow)
		{
			BILogInit();
			
			if(iShow)// If Show IME, Give a new Session ID.
			{
				mSessionId = System.Guid.NewGuid().ToString();
				mStopSend = true;
			}
			else
			{
				mStopSend = false;
				if(mRunningCoroutine != null)
					VRIME_Manager.Ins.StopCoroutine(mRunningCoroutine);
				mRunningCoroutine = VRIME_Manager.Ins.StartCoroutine(AutoFlushData());
			}
			// Add Log Data
			using(new BILogScope(CATEGORY_ACTIVITY_AND_USAGE)) {
				BILogScope.BI.AddData(KEY_EVENT, iShow ? USAGE_EV_SHOW : USAGE_EV_CLOSE);
				AddDataActUsageCommon();
				AddDataCommon();
			}
			// Input Method
			KeybaordDefaultInputData(iShow);
			VRIME_Debugger.Log("BI CallIMEStauts Logged. SessionID :" + mSessionId);
		}
		/// <summary>
		/// activity_and_usage, ev = "done"
		/// </summary>
		public void CallSubmit()
		{
			BILogInit();
			using(new BILogScope(CATEGORY_ACTIVITY_AND_USAGE)) {
				BILogScope.BI.AddData(KEY_EVENT, USAGE_EV_DONE);
				AddDataActUsageCommon();
				AddDataCommon();
			}
			// Input Method
			SendInputMethod();
			// User Action
			CallActionClick(eActionClickEntrance.submit);
			// VRIME_Debugger.Log("BI CallSubmit Logged. SessionID :" + mSessionId);
		}
		#endregion
		#region private Function
		/// <summary>
		/// Show-Hide-Submit(Done) use same Key-A(N)
		/// </summary>
		private void AddDataActUsageCommon()
		{
			Transform aManagerTrans = VRIME_Manager.Ins.transform;
			Transform aKeyBoardTrans = VRIME_KeyboardOversee.Ins.RootPath;

			BILogScope.BI.AddData(KEY_A0, RUNTIME_STEAM_VR);
			BILogScope.BI.AddData(KEY_A1, SystemInfo.operatingSystem);
			BILogScope.BI.AddData(KEY_A2, Application.productName);
			BILogScope.BI.AddData(KEY_A3, Vector3ToString(aManagerTrans.position));
			BILogScope.BI.AddData(KEY_A4, Vector3ToString(aKeyBoardTrans.localRotation.eulerAngles));
			BILogScope.BI.AddData(KEY_A5, Vector3ToString(aManagerTrans.localScale));
			BILogScope.BI.AddData(KEY_A6, UnityEngine.XR.XRDevice.model);
			BILogScope.BI.AddData(KEY_A7, VRIME_KeyboardSetting.SDKVersion);
			BILogScope.BI.AddData(KEY_A8, AppEntry);
		}
		#endregion
	}
}