// ========================================================================== //
//  Created: 2019-04-29
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public partial class VRIME_BISender
	{

		private const string CATEGORY_USER_ACTION = "user_action";
		private const string ACTION_EV_CLICK = "click";
		private const string ACTION_EV_EVENT = "event";
		private const string ACTION_CLICK_OPEN = "open";
		private const string ACTION_CLICK_CLOSE = "close";
		
		#region public felid
		public bool actionAccentShow = false;
        public bool actionVoiceLanguageOpen = false;
		#endregion

		#region public function
		/// <summary>
		/// 
		/// </summary>
		/// <param name="iClickEV"></param>
		public void CallActionClick(eActionClickEntrance iClickEV)
		{
			if(iClickEV == eActionClickEntrance.voice_key)
				return;
			
			BILogInit();
			using(new BILogScope(CATEGORY_USER_ACTION)) {
				BILogScope.BI.AddData(KEY_EVENT, ACTION_EV_CLICK);
				BILogScope.BI.AddData(KEY_A0, iClickEV.ToString());
				string aLanguageText = string.Empty;
				switch(iClickEV)
				{
					case eActionClickEntrance.language_wing_open:
					case eActionClickEntrance.language_wing_select:
						aLanguageText = keyboardAudioInput.SpeechLanguage.ToString();
						break;
					default:
						aLanguageText = GetIMELanguageText(VRIME_KeyboardSetting.IMELanguage);
						break;
				}
				BILogScope.BI.AddData(KEY_A1, aLanguageText);
				AddDataCommon();
			}
			VRIME_Debugger.Log("Action Click :" + iClickEV);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="iClickEV"></param>
		public void CallActionEvent(eActionEventEntrance iClickEV)
		{
			BILogInit();
			using(new BILogScope(CATEGORY_USER_ACTION)) {
				BILogScope.BI.AddData(KEY_EVENT, ACTION_EV_EVENT);
				BILogScope.BI.AddData(KEY_A0, iClickEV.ToString());
				BILogScope.BI.AddData(KEY_A1, GetIMELanguageText(VRIME_KeyboardSetting.IMELanguage));
				
				AddDataCommon();
			}
			VRIME_Debugger.Log("Action Click :" + iClickEV);
		}

		public void UserActionVoiceKeyOpen() { ActionVoiceKeyClick(true); }
		public void UserActionVoiceKeyClose() { ActionVoiceKeyClick(false); }
		#endregion
		#region private Function
		private void ActionVoiceKeyClick(bool iIsOpen)
		{
			BILogInit();
			using(new BILogScope(CATEGORY_USER_ACTION)) {
				BILogScope.BI.AddData(KEY_EVENT, ACTION_EV_CLICK);
				BILogScope.BI.AddData(KEY_A0, eActionClickEntrance.voice_key.ToString());
				string aA1Text = string.Empty;
				if(iIsOpen) {
					aA1Text = ACTION_CLICK_OPEN;
				}
				else {
					aA1Text = ACTION_CLICK_CLOSE;
				}
				BILogScope.BI.AddData(KEY_A1, aA1Text);
				AddDataCommon();
			}
			VRIME_Debugger.Log("Action Voice Key Click :" + iIsOpen);
		}
		#endregion
	}
	public enum eActionClickEntrance {
			submit,
			switch_keyboard,
			voice_key,
			clear_text,
			accentPanel_open,
			accentPanel_select,
			language_wing_open,
			language_wing_select,
			switch_symbol,
			nearKey_select,
			WCL_open,
			WCL_select,
			WCL_page_up,
			WCL_page_down,
			keyboard_close,
	}
	public enum eActionEventEntrance
	{
		language_wing_auto_close,
	}
}