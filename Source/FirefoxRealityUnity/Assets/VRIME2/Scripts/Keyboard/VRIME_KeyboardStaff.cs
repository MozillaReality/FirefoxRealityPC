// ========================================================================== //
//  Created: 2019-01-11
// ========================================================================== //
namespace VRIME2
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Htc.Omni;
	using TMPro;

	public class VRIME_KeyboardStaff : MonoBehaviour
	{
		#region public field
		public eKeyBoardLayoutType staffType;
		#endregion
		#region serialize field
		[Header("Panel Mesh")]
		[SerializeField]
		private VRIME_PanelMesh panelMesh;
		public MeshRenderer panelMeshRender { get {
				if(panelMesh == null)
					return null;

				return panelMesh.panelMeshRender;
			}
		}
		[Header("Language Wing")]
		[SerializeField]
		private VRIME_LanguageWing languageWing;
		public bool lanWingShowState { get {
				if(languageWing == null)
					return false;
				
				return languageWing.lanWingShowState;
			}
		}
		[Header("Character Accents")]
		[SerializeField]
		private VRIME_CharacterAccents characterAccents;
		public VRIME_KeyboardAccent Accent { get {
				if(characterAccents == null)
					return null;
				
				return characterAccents.Accent;
			}
		}
		[Header("Tool Tip")]
		[SerializeField]
		private VRIME_ToolTip toolTip;
		public eToolTipPage toolTipNowPage { get {
				if(toolTip == null)
					return eToolTipPage.State_1;
				
				return toolTip.toolTipNowPage;
			}
		}
		#endregion
		#region public Function
		public void Init()
		{
			SetPanelMesh();
			SetLanguageWing();
			SetCharacterAccents();
			SetToolTip();
		}
		#endregion
		#region PanelMesh
		public VRIME_KeyboardButton GetFunctionButton(eButtonType iType)
		{
			if(panelMesh == null)
				return null;
			
			return panelMesh.GetFunctionButton(iType);
		}
		/// <summary>
		/// Voice button language tip.
		/// </summary>
		/// <param name="iVLang"></param>
		public void ChangeVoiceLanguageTip(SupportLanguage iVLang)
		{
			if(panelMesh == null)
				return;
				
			panelMesh.ChangeVoiceLanguageTip(iVLang);
		}
		/// <summary>
		/// Set keyboard layout and set data, and change language button language tip.
		/// </summary>
		public void ChangeLanguagebySetting()
		{
			if(panelMesh == null)
				return;
			panelMesh.ChangeKeypadByJsonFile(VRIME_KeyboardSetting.IMELanguage, VRIME_KeyboardOversee.Ins.SymbolMode);
			panelMesh.ChangeLanguageButtonTip(VRIME_KeyboardSetting.IMELanguage);
		}
		/// <summary>
		/// Start Symbol mode and check password mode
		/// </summary>
		public void SetSymbolConfig()
		{
			if(panelMesh == null)
				return;
			bool aModeCheck = VRIME_InputFieldOversee.Ins.PasswordMode;
			eLanguage aSetLanguage = VRIME_KeyboardSetting.IMELanguage;
			if(aModeCheck)
				aSetLanguage = eLanguage.English;

			panelMesh.ChangeKeypadByJsonFile(aSetLanguage, VRIME_KeyboardOversee.Ins.SymbolMode);
		}
		/// <summary>
		/// set password function
		/// </summary>
		/// <param name="iModeOn"></param>
		public void SetPassowrdKeyboard(bool iModeOn)
		{
			if(panelMesh == null)
				return;
			panelMesh.FunctionKeySetByPasswordMode(iModeOn);
			panelMesh.ChangeKeypadByJsonFile(eLanguage.English, VRIME_KeyboardOversee.Ins.SymbolMode);
			panelMesh.ChangeLanguageButtonTip(eLanguage.English);
		}
		/// <summary>
		/// Init
		/// </summary>
		private void SetPanelMesh()
		{
			Transform aPathRoot = this.transform.Find("PanelMesh");
			if(aPathRoot == null)
				return;
			panelMesh = aPathRoot.GetComponent<VRIME_PanelMesh>();
			if(panelMesh == null)
				panelMesh = aPathRoot.gameObject.AddComponent<VRIME_PanelMesh>();

			panelMesh.Init(this);
		}
		#endregion
		#region LanguageWing
		/// <summary>
		/// Init
		/// </summary>
		private void SetLanguageWing()
		{
			Transform aPathRoot = this.transform.Find("LanguageWing");
			if(aPathRoot == null)
				return;
			languageWing = aPathRoot.GetComponent<VRIME_LanguageWing>();
			if(languageWing == null)
				languageWing = aPathRoot.gameObject.AddComponent<VRIME_LanguageWing>();
			
			languageWing.Init(this);
		}
		public void callLanWing(bool iShow)
		{
			if(languageWing == null)
				return;

			languageWing.callLanWing(iShow);
		}
		/// <summary>
		/// Language Button At Wings
		/// </summary>
		/// <param name="iBtn"></param>
		public void ChangeLanguage(VRIME_KeyboardButton iBtn)
		{
			if(languageWing == null)
				return;

			languageWing.ChangeLanguage(iBtn);
		}
		/// <summary>
		/// Voice Language Button At Wings
		/// </summary>
		/// <param name="iBtn"></param>
		public void ChangeVoiceLanguage(VRIME_KeyboardButton iBtn)
		{
			if(languageWing == null)
				return;

			languageWing.ChangeVoiceLanguage(iBtn);
		}
		public void ChangeLanguageCycle()
		{
			if(languageWing == null)
				return;

			languageWing.ChangeLanguageCycle();
		}
		public void SetVoiceLanWingBtn(eVoiceEngine iEngine)
		{
			if(languageWing == null)
				return;

			languageWing.SetVoiceLanWingBtn(iEngine);
		}
		#endregion
		#region CharacterAccents
		/// <summary>
		/// Init
		/// </summary>
		private void SetCharacterAccents()
		{
			Transform aPathRoot = this.transform.Find("CharacterAccents");
			if(aPathRoot == null)
				return;
			characterAccents = aPathRoot.GetComponent<VRIME_CharacterAccents>();
			if(characterAccents == null)
				characterAccents = aPathRoot.gameObject.AddComponent<VRIME_CharacterAccents>();

			characterAccents.Init(this);
		}
		#endregion
		#region Tooltip
		public void callTooltip(bool iShow)
		{
			if(toolTip == null)
				return;

			toolTip.callTooltip(iShow);
		}
		public void SetTooltipPage(eToolTipPage iPage)
		{
			if(toolTip == null)
				return;

			toolTip.SetTooltipPage(iPage);
		}
		public void SetTooltipPagesWords(eToolTipPage iPage, VRIME_TipWords iWords)
		{
			if(toolTip == null)
				return;
				
			toolTip.SetTooltipPagesWords(iPage, iWords);
		}
		/// <summary>
		/// Init
		/// </summary>
		private void SetToolTip()
		{
			Transform aPathRoot = this.transform.Find("Tooltip");
			if(aPathRoot == null)
				return;
			toolTip = aPathRoot.GetComponent<VRIME_ToolTip>();
			if(toolTip == null)
				toolTip = aPathRoot.gameObject.AddComponent<VRIME_ToolTip>();
			
			toolTip.Init(this);			
		}
		#endregion
	}
}