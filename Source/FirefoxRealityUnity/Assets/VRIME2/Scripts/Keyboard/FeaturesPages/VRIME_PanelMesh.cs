// ========================================================================== //
//  Created: 2019-06-28
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
    using Htc.Omni;
    using TMPro;
    using UnityEngine;

    public class VRIME_PanelMesh : VRIME_FeaturesPage
    {
		public MeshRenderer panelMeshRender{
			get{
				return this.GetComponent<MeshRenderer>();
			}
		}
		#region private Field
		private VRIME_PanelCategory[] panelCategorys;
		private VRIME_KeyboardButton cahngeLanBtn;
		private LinearLayout meshLayout;
		private VRIME_PanelCatchLayout catchKeyboardLayout;
		#endregion
		#region save Using
		private VRIME_KeypadLayout saveUsingLayout;
		private eLanguage saveUsingLanguage;
		private bool saveUsingSymbol;
		#endregion
		#region override Function
        public override void Init(VRIME_KeyboardStaff iParent)
        {
			parentFunc = iParent;
			meshLayout = this.GetComponent<LinearLayout>();
			// Set Panel Category
			InitPanelCategory();
			// Make Language Tip
			ChangeLanguageButtonTip(VRIME_KeyboardSetting.IMELanguage);
			// Base Keyboard Layout
			InitSetKeypadLayout();
			// Check voice button need show.
			CheckVoiceFunctionButton();
        }
		#endregion
		#region Init Function
		private void InitPanelCategory()
		{
			int aPanelCount = this.transform.childCount;
			panelCategorys = new VRIME_PanelCategory[aPanelCount];
			for(int i = 0; i < aPanelCount; i ++)
			{
				LinearLayout aTmpLine = this.transform.GetChild(i).GetComponentInChildren<LinearLayout>();
				VRIME_PanelCategory aTmpCat = aTmpLine.GetComponent<VRIME_PanelCategory>();
				if(aTmpCat == null)
					aTmpCat = aTmpLine.gameObject.AddComponent<VRIME_PanelCategory>();
				// Save Category
				aTmpCat.root = aTmpLine;
				aTmpCat.UpdateRows();
				panelCategorys[i] = aTmpCat;
			}
		}
		/// <summary>
		/// Init default layout stage and save state. 
		/// </summary>
		private void InitSetKeypadLayout()
		{
			if(parentFunc.staffType != eKeyBoardLayoutType.English)
				return;
			// 1. Make all catch objects.
			MakeCatchObjects();
			// 2. Set default layout.
			VRIME_KeypadLayout aLayout = VRIME_Manager.runSystem.GetLayoutData(VRIME_KeyboardSetting.IMELanguage);
			DoChangeLayout(aLayout, VRIME_KeyboardSetting.IMELanguage, VRIME_KeyboardOversee.Ins.SymbolMode);
		}
		/// <summary>
		/// 2019/07/25
		/// Default language button in row 4, voice button in row 5.
		/// If Google and Baidu voice key both null, voice button will hide.
		/// And language button need move one down row.
		/// </summary>
		private void CheckVoiceFunctionButton()
		{
			VRIME_KeyboardButton aVoiceButton = GetFunctionButton(eButtonType.Voice);
			if(aVoiceButton == null)
				return;
			aVoiceButton.gameObject.SetActive(true);
			// Check ApiKey existence
			bool aGoogleKeyNull = string.IsNullOrEmpty(VRIME_Manager.Ins.googleApiKey);
			// Baidu ApiKey&ApiSecert need have data
            bool aBaiduKeyNull = string.IsNullOrEmpty(VRIME_Manager.Ins.baiduApiKey) || string.IsNullOrEmpty(VRIME_Manager.Ins.baiduApiSecert);
            if(aGoogleKeyNull || aBaiduKeyNull == false)
				return;
			// google and baidu apikey all null, hide voice button and move down oter button(except close button).
			VRIME_PanelCategory aFunctionCat = GetPanelCategory(ePanelMeshNames.FunctionButtons);
			if(aFunctionCat == null)
				return;
			// Row 1 is colse button, not need move.
			ButtonMoveOneRow(true, aFunctionCat, 1);
			// Hide VoiceButton
			aVoiceButton.gameObject.SetActive(false);
		}
		#endregion
		#region public Function
		public VRIME_KeyboardButton GetFunctionButton(eButtonType iType)
		{
			VRIME_PanelCategory aFunctionCat = GetPanelCategory(ePanelMeshNames.FunctionButtons);
			if(aFunctionCat == null)
				return null;

			VRIME_KeyboardButton aResult = null;
			for(int i = 0; i < aFunctionCat.rows.Length; i++)
			{
				Transform aSingleRow = aFunctionCat.rows[i];
				for(int k = 0; k < aSingleRow.childCount; k++)
				{
					VRIME_KeyboardButton aChild = aSingleRow.GetChild(k).GetComponent<VRIME_KeyboardButton>();
					if(aChild == null)
						continue;
					if(aChild.btnType == iType)
					{
						aResult = aChild;
						return aResult;
					}
				}
			}
			return aResult;
		}
		/// <summary>
		/// Make a User Can Understand Language Tip
		/// </summary>
		public void ChangeLanguageButtonTip(eLanguage iIconWordLanguage)
		{
			VRIME_KeyboardButton aLanButton = GetFunctionButton(eButtonType.Language);
			if(aLanButton == null)
				return;
			
			SetChangeLanButton(aLanButton);
			TextMeshPro aBaseText = aLanButton.rootUI.GetComponentInChildren<TextMeshPro>();
			Transform aExtraWord = aLanButton.rootUI.Find(VRIME_KeyboardData.cLanTipName);
			
			if(aExtraWord == null)
			{
				if(VRIME_KeyboardOversee.Ins.LabelBtnOriginal == null)
					return;
				TextMeshPro aCopyLabel = VRIME_KeyboardOversee.Ins.LabelBtnOriginal.rootUI.GetComponentInChildren<TextMeshPro>();
				GameObject aTempObj = GameObject.Instantiate(aCopyLabel.gameObject, aLanButton.rootUI);
				aTempObj.name = VRIME_KeyboardData.cLanTipName;
				aTempObj.transform.localPosition = aBaseText.transform.localPosition + new Vector3(0f, -0.023f, 0.001f);
				aExtraWord = aTempObj.transform;
			}
			TextMeshPro aExtraPro = aExtraWord.GetComponent<TextMeshPro>();
			// Set Language Icon Word To Know What is Language Now.
			aExtraPro.text = VRIME_InternationalWord.IMEIconWord(iIconWordLanguage);
			aExtraPro.color = VRIME_KeyboardData.GetViveColors(eViveColor.NormalBlack);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="iVLang"></param>
		public void ChangeVoiceLanguageTip(SupportLanguage iVLang)
		{
			VRIME_KeyboardButton aLanButton = GetFunctionButton(eButtonType.Voice);
			// poshaughnessey - we don't support voice in current version
//			if(aLanButton == null)
				return;
			
			if(VRIME_VoiceOversee.Ins != null)
				VRIME_VoiceOversee.Ins.SetVoiceFunctionKey(aLanButton);
			TextMeshPro aBaseText = aLanButton.rootUI.GetComponentInChildren<TextMeshPro>();
			Transform aExtraWord = aLanButton.rootUI.Find(VRIME_KeyboardData.cLanTipName);
			
			if(aExtraWord == null)
			{
				if(VRIME_KeyboardOversee.Ins.LabelBtnOriginal == null)
					return;
				TextMeshPro aCopyLabel = VRIME_KeyboardOversee.Ins.LabelBtnOriginal.rootUI.GetComponentInChildren<TextMeshPro>();
				GameObject aTempObj = GameObject.Instantiate(aCopyLabel.gameObject, aLanButton.rootUI);
				aTempObj.name = VRIME_KeyboardData.cLanTipName;
				aTempObj.transform.localPosition = aBaseText.transform.localPosition + new Vector3(0f, -0.023f, 0.001f);
				aExtraWord = aTempObj.transform;
			}
			TextMeshPro aExtraPro = aExtraWord.GetComponent<TextMeshPro>();
			// Set Language Icon Word To Know What is Language Now.
			aExtraPro.text = VRIME_InternationalWord.VoiceIconWord(iVLang);
			aExtraPro.color = VRIME_KeyboardData.GetViveColors(eViveColor.NormalBlack);
		}
		public void FunctionKeySetByPasswordMode(bool iIsOn)
		{
			if(parentFunc.staffType != eKeyBoardLayoutType.English)
				return;
			VRIME_PanelCategory aFunctionCat = GetPanelCategory(ePanelMeshNames.FunctionButtons);
			if(aFunctionCat == null)
				return;
			// 1. Reset FunctionKey
			VRIME_KeyboardButton aLanButton = GetFunctionButton(eButtonType.Language);
			if(aLanButton != null)
			{
				aLanButton.transform.parent = aFunctionCat.rows[3];
				aLanButton.transform.localPosition = Vector3.zero;
				aLanButton.transform.localRotation = Quaternion.Euler(Vector3.zero);
				aLanButton.transform.localScale = Vector3.one;
				aLanButton.gameObject.SetActive(true);
			}
			VRIME_KeyboardButton aVoiceButton = GetFunctionButton(eButtonType.Voice);
			if(aVoiceButton != null)
			{
				aVoiceButton.transform.parent = aFunctionCat.rows[4];
				aVoiceButton.transform.localPosition = Vector3.zero;
				aVoiceButton.transform.localRotation = Quaternion.Euler(Vector3.zero);
				aVoiceButton.transform.localScale = Vector3.one;
				aVoiceButton.gameObject.SetActive(true);
			}			
			// 2. Set voice button key on/off.
			if(iIsOn) {
				ButtonMoveOneRow(true, aFunctionCat, 1);
				aVoiceButton.gameObject.SetActive(false);
			}
			else {
				CheckVoiceFunctionButton();
			}
		}
		#endregion
		#region private Function
		/// <summary>
		/// All row child move row parent, one row by one row.
		/// </summary>
		/// <param name="iMoveDown"></param>
		/// <param name="iCategory"></param>
		/// <param name="iStartCheckRow"></param>
		private void ButtonMoveOneRow(bool iMoveDown, VRIME_PanelCategory iCategory, int iStartCheckRow)
		{
			if(iCategory == null)
				return;
			
			for(int i = iStartCheckRow; i < iCategory.rows.Length - 1; i++)// Row 6 not open
			{
				Transform aRow = iCategory.rows[i];
				for(int k = 0; k < aRow.childCount; k++)
				{
					bool aSuccess = false;
					int aNextIndex = 0;
					// Check Move row is success.
					if(iMoveDown) {
						aNextIndex = i + 1;
						aSuccess = aNextIndex < iCategory.rows.Length;
					}
					else {
						aNextIndex = i - 1;
						aSuccess = aNextIndex >= 0;
					}
					// Move
					if(aSuccess)
					{
						Transform aNextParent = iCategory.rows[aNextIndex];
						Transform aChild = aRow.GetChild(k);
						aChild.parent = aNextParent;
						aChild.localPosition = Vector3.zero;
						aChild.localRotation = Quaternion.Euler(Vector3.zero);
						aChild.localScale = Vector3.one;
					}
				}
			}
		}
		/// <summary>
		/// Get row button array.
		/// </summary>
		/// <param name="iCat"></param>
		/// <param name="iRow"></param>
		/// <returns></returns>
		private VRIME_KeyboardButton[] GetRowButtons(VRIME_PanelCategory iCat, eOmniKeyRows iRow)
		{
			VRIME_KeyboardButton[] aResult = null;
			if(iCat == null)
				return aResult;
			
			Transform aRow = null;
			for(int i = 0; i < iCat.rows.Length; i++)
			{
				if(iCat.rows[i].name == iRow.ToString()) {
					aRow = iCat.rows[i];
					break;
				}
			}
			if(aRow == null)
				return aResult;

			for(int i = 0; i < aRow.childCount; i++) {
				Transform aChild = aRow.GetChild(i);
				aChild.gameObject.SetActive(true);
			}
			
			aResult = aRow.GetComponentsInChildren<VRIME_KeyboardButton>(false);
			return aResult;
		}
		/// <summary>
		/// Get VRIME_PanelCategory by child names.
		/// </summary>
		/// <param name="iType"></param>
		/// <returns></returns>
		private VRIME_PanelCategory GetPanelCategory(ePanelMeshNames iType)
		{
			VRIME_PanelCategory aResult = null;
			for(int i = 0; i < panelCategorys.Length; i++)
			{
				if(panelCategorys[i].parent.name == iType.ToString())
				{
					aResult = panelCategorys[i];
					break;
				}
			}
			return aResult;
		}
		#endregion
		#region language button
		/// <summary>
		/// Icon Tip Color Change Use
		/// </summary>
		/// <param name="iBtn"></param>
		private void SetChangeLanButton(VRIME_KeyboardButton iBtn)
		{
			if(cahngeLanBtn == iBtn)
				return;
			if(iBtn.Event == null)
				iBtn.Init();
			if(iBtn != null)
			{
				iBtn.Event.onEnter.RemoveListener(ButtonEnter);
				iBtn.Event.onStay.RemoveListener(ButtonEnter);
				iBtn.Event.onExit.RemoveListener(ButtonExit);
			}
			iBtn.Event.onEnter.AddListener(ButtonEnter);
			iBtn.Event.onStay.AddListener(ButtonEnter);
			iBtn.Event.onExit.AddListener(ButtonExit);
			cahngeLanBtn = iBtn;
		}
		/// <summary>
		/// Extra Button Event: Chnage Tip Color
		/// </summary>
		/// <param name="iObj"></param>
		private void ButtonEnter(GameObject iObj)
		{
			Transform aExtraWord = cahngeLanBtn.rootUI.Find(VRIME_KeyboardData.cLanTipName);
			TMPro.TextMeshPro aExtraPro = aExtraWord.GetComponent<TMPro.TextMeshPro>();
			aExtraPro.color = VRIME_KeyboardData.GetViveColors(eViveColor.PressedBlue);
		}
		/// <summary>
		/// Extra Button Event: Chnage Tip Color
		/// </summary>
		/// <param name="iObj"></param>
		private void ButtonExit(GameObject iObj)
		{
			Transform aExtraWord = cahngeLanBtn.rootUI.Find(VRIME_KeyboardData.cLanTipName);
			TMPro.TextMeshPro aExtraPro = aExtraWord.GetComponent<TMPro.TextMeshPro>();
			aExtraPro.color = VRIME_KeyboardData.GetViveColors(eViveColor.NormalBlack);
		}
		#endregion
		#region File Set Keyboard
		public void ChangeKeypadByJsonFile(eLanguage iLanugage, bool iSymbolMode)
		{
			if(parentFunc.staffType != eKeyBoardLayoutType.English)
				return;
			// 1. Get panel category object
			VRIME_PanelCategory aStandardCat = GetPanelCategory(ePanelMeshNames.StandardKeys);
			if(aStandardCat == null)
				return;
			// 2. Get Keyboard Layout
			VRIME_KeypadLayout aLayout = VRIME_Manager.runSystem.GetLayoutData(iLanugage, iSymbolMode);
			if(aLayout.layoutType != saveUsingLayout.layoutType)// Same Stage Not Update Layout andconfig
			{
				DoChangeLayout(aLayout, iLanugage, iSymbolMode);
			}
			// 3. Delay refresh layout, because if no delay time, refresh will not working.
			CancelInvoke("SetConfigData");
			Invoke("SetConfigData", 0.1f);
		}		
		/// <summary>
		/// 
		/// </summary>
		public void SetConfigData()
		{
			VRIME_PanelCategory aStandardCat = GetPanelCategory(ePanelMeshNames.StandardKeys);
			if(aStandardCat == null)
				return;
			// Get config and make config data array.
			VRIME_KeypadConfig aConfig = VRIME_Manager.runSystem.GetConfigData(saveUsingLanguage, saveUsingSymbol);
			if(aConfig == null)
				return;
			for(int i = 0; i < aConfig.rowDatas.Length; i++)
			{
				VRIME_KeyboardButton[] aButtons = GetRowButtons(aStandardCat, (eOmniKeyRows)i);
				VRIME_KeypadConfigData[] aConfigDatas = aConfig.rowDatas[i].objDatas;
				// Set data to button
				SetConfigDataToButtonsbyRow(aButtons, aConfigDatas, aConfig.layoutType);
			}
		}
		
		/// <summary>
		/// Because MakeCatchObjects() done will hapen next frame.
		/// so use invoke to do change layout in next fream.
		/// </summary>
		private void SetLayout()
		{
			catchKeyboardLayout.SetCategoryLayout(saveUsingLayout);
		}
		/// <summary>
		/// Make all type catch objects.
		/// </summary>
		private void MakeCatchObjects()
		{
			if (catchKeyboardLayout != null)
				return;
			// Make a catch object
			GameObject aCatchRoot = new GameObject("CatchRoot");
			aCatchRoot.SetActive(false);
			aCatchRoot.transform.parent = this.transform;
			aCatchRoot.transform.localPosition = Vector3.zero;
			aCatchRoot.transform.localRotation = Quaternion.Euler(Vector3.zero);
			// add VRIME_PanelCatchLayout in aCatchRoot.
			catchKeyboardLayout = aCatchRoot.AddComponent<VRIME_PanelCatchLayout>();
			catchKeyboardLayout.MakeCatchObjects(GetPanelCategory(ePanelMeshNames.StandardKeys));
		}
		/// <summary>
		/// Row by row set config data to keyboard buttons.
		/// </summary>
		/// <param name="iButtons">row keyboard button</param>
		/// <param name="iDatas">row config datas</param>
		/// <param name="iConfigStage">keyboard layout</param>
		private void SetConfigDataToButtonsbyRow(VRIME_KeyboardButton[] iButtons, VRIME_KeypadConfigData[] iDatas, eKeyboardStage iConfigStage)
		{
			if(iButtons.Length != iDatas.Length)
			{
				VRIME_Debugger.LogError("PanelMesh", "Button and data num not match.");
				return;
			}
			// 1.Get caps state
			eCapsState aState = VRIME_KeyboardOversee.Ins.CapsType;
			bool aIsUpper = aState != eCapsState.Lower;
			if(iConfigStage != eKeyboardStage.Normal)
				aIsUpper = false;
			// 2. Set data to button.
			for(int k = 0; k < iButtons.Length; k++)
			{
				VRIME_KeyboardButton aButton = iButtons[k];
				VRIME_KeypadConfigData aData = iDatas[k];
				aButton.btnType = aData.keyFuncType;
				// Set
				string aWord = "";
				switch(aButton.btnType)
				{
					case eButtonType.Letter:
						aWord = aIsUpper ? aData.word.ToUpper() : aData.word.ToLower();
						break;
					case eButtonType.Caps:
						VRIME_KeyboardData.CapsButton = aButton;
						aWord = VRIME_KeyboardOversee.Ins.GetCapsButtonWord(aState);
						break;
					default:
						aWord = aData.word;
						break;
				}
				// Set Word	
				aButton.Word = aWord;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="iLayout"></param>
		/// <param name="iLang"></param>
		/// <param name="iSymbol"></param>
		private void DoChangeLayout(VRIME_KeypadLayout iLayout, eLanguage iLang, bool iSymbol)
		{
			saveUsingSymbol = iSymbol;
			saveUsingLanguage = iLang;
			saveUsingLayout = iLayout;
			CancelInvoke("SetLayout");
			Invoke("SetLayout", 0f);
		}
		#endregion
    }
}