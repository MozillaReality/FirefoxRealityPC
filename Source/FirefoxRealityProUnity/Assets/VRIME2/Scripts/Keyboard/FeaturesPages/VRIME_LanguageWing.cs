// ========================================================================== //
//  Created: 2019-06-28
// ========================================================================== //
namespace VRIME2
{
    using System;
    using System.Collections;
	using System.Collections.Generic;
    using Htc.Omni;
    using UnityEngine;

    public class VRIME_LanguageWing : VRIME_FeaturesPage
    {
		private LinearLayout itemsRoot;
		private Animator anime;
		private VRIME_LanguageWingButton[] lanWingButtons;
		public bool lanWingShowState = false;
		#region override Function
        public override void Init(VRIME_KeyboardStaff iParent)
        {
			parentFunc = iParent;
            itemsRoot = this.transform.Find("LanguageMesh/FunctionButtons/Transform").GetComponent<LinearLayout>();
			anime = this.transform.GetComponent<Animator>();
			// Init Buttons 
			InitLanguageWingButtons();
			// Set Language
			SetLanguageWingButtons();
        }
		#endregion
		#region public Function
		public void callLanWing(bool iShow)
		{
			lanWingShowState = iShow;
			eAnimAccent aType = eAnimAccent.Normal;
			if(iShow)
				aType = eAnimAccent.Opened;
			else
				aType = eAnimAccent.Closed;
			// Animation
			PlayAnimeLanWing(aType);
			// Audio
			if(iShow)
				VRIME_KeyAudioOversee.Ins.LanguageWing(true);
			// Set Auto Close
			CancelInvoke("AutoCloseLanWing");
			Invoke("AutoCloseLanWing", 2f);
		}
		/// <summary>
		/// Language Button At Wings
		/// </summary>
		/// <param name="iBtn"></param>
		public void ChangeLanguage(VRIME_KeyboardButton iBtn)
		{
			VRIME_LanguageWingButton aTempWing = null;
			for(int i = 0; i < lanWingButtons.Length; i++)
			{
				if(lanWingButtons[i].button.Equals(iBtn)){
					aTempWing = lanWingButtons[i];
					break;
				}
			}
			if(aTempWing == null)
				return;
			// Reset KeyBoard
			VRIME_Manager.Ins.UpdateKeyboardLanguage(aTempWing.language);
			// Close Wing
			callLanWing(false);
		}
		/// <summary>
		/// Voice Language Button At Wings
		/// </summary>
		/// <param name="iBtn"></param>
		public void ChangeVoiceLanguage(VRIME_KeyboardButton iBtn)
		{
			VRIME_LanguageWingButton aTempWing = null;
			for(int i = 0; i < lanWingButtons.Length; i++)
			{
				if(lanWingButtons[i].button.Equals(iBtn)){
					aTempWing = lanWingButtons[i];
					break;
				}
			}
			if(aTempWing == null)
				return;
			if(VRIME_VoiceOversee.Ins != null) {
				VRIME_VoiceOversee.Ins.SetSpeechLanguage(aTempWing.voiceLangeage);
				VRIME_VoiceOversee.Ins.CallVoiceUI(true);
			}
			// Close Wing
			callLanWing(false);
			// BI Logger Add
			VRIME_BISender.Ins.UpdateEngineInfo();
		}
		public void ChangeLanguageCycle()
		{
			// string[] aLanguages =  Enum.GetNames(typeof(eLanguage));
			// int aLangIndex = (int)VRIME_KeyboardSetting.IMELanguage;
			// int aNextIndex = (aLangIndex + 1) % aLanguages.Length;
			
			// eLanguage aSet = (eLanguage)aNextIndex;
			// VRIME_Manager.Ins.UpdateKeyboardLanguage(aSet);
			// Get next lanugage by language pool.
			int aLastIndex = VRIME_Manager.Ins.LanguagePoolIndex;
			int aNextIndex = (aLastIndex + 1) % VRIME_KeyboardSetting.LanguagePool.Length;

			VRIME_Manager.Ins.UpdateKeyboardLanguage(VRIME_KeyboardSetting.LanguagePool[aNextIndex]);
			VRIME_Manager.Ins.LanguagePoolIndex = aNextIndex;
		}
		public void SetVoiceLanWingBtn(eVoiceEngine iEngine)
		{
			if(lanWingButtons == null) {
				InitLanguageWingButtons();
			}
			int aLangNum = Enum.GetNames(typeof(SupportLanguage)).Length;
			for(int i = 0; i < lanWingButtons.Length; i++)
			{
				VRIME_LanguageWingButton aTempWing = lanWingButtons[i];
				Collider aCol = aTempWing.button.GetComponentInChildren<Collider>(true);
				aCol.enabled = true;
				string aShowWords = string.Empty;
				if(i < aLangNum){
					aTempWing.voiceLangeage = (SupportLanguage)i;
					aShowWords = VRIME_InternationalWord.VoiceLangWingShowText(aTempWing.voiceLangeage);
				}
				else{
					aCol.enabled = false;
				}

				if(iEngine == eVoiceEngine.baidu && aTempWing.voiceLangeage == SupportLanguage.TraditionalChinese)
				{
					aShowWords = string.Empty;
					aCol.enabled = false;
				}

				aTempWing.button.Word = aShowWords;
				// Temp to Array
				lanWingButtons[i] = aTempWing;
			}
		}
		#endregion
		#region private Function
		private void InitLanguageWingButtons()
		{
			VRIME_KeyboardButton[] aButtons = itemsRoot.GetComponentsInChildren<VRIME_KeyboardButton>(true);
			lanWingButtons = new VRIME_LanguageWingButton[aButtons.Length];
			for(int i = 0; i < lanWingButtons.Length; i++)
			{
				VRIME_LanguageWingButton aTempWing = new VRIME_LanguageWingButton();
				aTempWing.button = aButtons[i];
				aTempWing.button.Init();
				lanWingButtons[i] = aTempWing;
			}
		}
		private void SetLanguageWingButtons()
		{
			int aLangNum = Enum.GetNames(typeof(eLanguage)).Length;
			for(int i = 0; i < lanWingButtons.Length; i++)
			{
				VRIME_LanguageWingButton aTempWing = lanWingButtons[i];
				Collider aCol = aTempWing.button.GetComponentInChildren<Collider>(true);
				aCol.enabled = true;
				string aShowWords = string.Empty;
				if(i < aLangNum){
					aTempWing.language = (eLanguage)i;
					aShowWords = VRIME_InternationalWord.LanguageWingShowText(aTempWing.language);
				}
				else{
					aCol.enabled = false;
				}
				aTempWing.button.Word = aShowWords;
				// Temp to Array
				lanWingButtons[i] = aTempWing;
			}
		}
		/// <summary>
		/// Set Invoke To Function Auto Act
		/// </summary>
		private void AutoCloseLanWing()
		{
			if(lanWingShowState == false)
				return;
			
			callLanWing(false);
			VRIME_BISender.Ins.CallActionEvent(eActionEventEntrance.language_wing_auto_close);
			VRIME_BISender.Ins.actionVoiceLanguageOpen = false;
		}
		private void PlayAnimeLanWing(eAnimAccent iType) { PlayAnimator(anime, iType.ToString()); }
		#endregion
    }
}