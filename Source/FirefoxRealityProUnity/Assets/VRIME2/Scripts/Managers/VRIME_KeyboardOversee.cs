// ========================================================================== //
//  Created: 2019-01-04
// ========================================================================== //
namespace VRIME2
{
    using System;
    using System.Collections;
	using System.Collections.Generic;
    using Htc.Omni;
    using UnityEngine;

	[AddComponentMenu("VRIME/IMEManager/Keyboard Oversee")]
	public class VRIME_KeyboardOversee : MonoBehaviour
	{
		public static VRIME_KeyboardOversee Ins {
			get { return instance; }
			set {
				instance = value;
				instance.Init();
			}
		}
		private static VRIME_KeyboardOversee instance;
		#region readonly field
		private readonly string omniIconLower = "9";
		private readonly string omniIconUpperUnLock = "8";
		private readonly string omniIconUpperLock = "~";
		#endregion
		#region public/private Field		
		public eCapsState CapsType {
			get { return capsState;}
		}
		private eCapsState capsState = eCapsState.Lower;
		public MeshRenderer UsingPanelMeshRender {
			get {
				if(showKeyboardLayout == null)
					return null;
				return showKeyboardLayout.panelMeshRender;
			}
		}
		private VRIME_KeyboardStaff showKeyboardLayout;
		public bool SymbolMode {
			get { return symbolStage; }
		}
		private bool symbolStage;
		#endregion
		#region public Field
		public Transform RootPath;
		public bool AccentShow {
			get {
				if(showKeyboardLayout == null)
					return false;
					
				return showKeyboardLayout.Accent.IsShow;
			}
		}
		public string AccentShowWord {
			get {
				if(showKeyboardLayout == null)
					return string.Empty;
					
				return showKeyboardLayout.Accent.ShowWord;
			}
		}
		[HideInInspector]
		public VRIME_KeyboardButton LabelBtnOriginal;
		#endregion
		#region private Field
		// [SerializeField]
		private VRIME_KeyboardStaff[] allKeyboards;
		private Dictionary<eOmniButtonType, GameObject> buttonTypeObjects = new Dictionary<eOmniButtonType, GameObject>();
		#endregion
		#region Unity Function
		#endregion
		#region public Function
		/// <summary>
		/// Call Show keyboard By Type(Normal/Number)
		/// </summary>
		/// <param name="iType"></param>
		public void ShowKeyboard(eKeyBoardLayoutType iType)
		{
			foreach(var eKB in allKeyboards) {
				bool iShow = false;
				if(eKB.staffType == iType) {
					showKeyboardLayout = eKB;
					ShowKeyboardByLanguage();
					iShow = true;
				}
				eKB.gameObject.SetActive(iShow);
			}
		}
		/// <summary>
		/// Set KeyboardManager(RootPath) Local Rotation
		/// </summary>
		/// <param name="iRot">localRotation(Vector3)</param>
		public void SetKeyboardRotation(Vector3 iRot)
		{
			if(RootPath == null)
				return;
			
			RootPath.localRotation = Quaternion.Euler(iRot);
		}
		/// <summary>
		/// Set Keyboard in Caps Button Click Change Board Button Words.
		/// </summary>
		/// <param name="iState"></param>
		public void SetCapsState(eCapsState iState)
		{
			capsState = iState;
			showKeyboardLayout.ChangeLanguagebySetting();
			showKeyboardLayout.Accent.CapsChangeShow(VRIME_KeyboardData.CapsButton);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="iModeOn"></param>
		public void SetSymbolMode(bool iModeOn)
		{
			symbolStage = iModeOn;
			showKeyboardLayout.SetSymbolConfig();
		}
		public void SetConfigBySetting()
		{
			showKeyboardLayout.ChangeLanguagebySetting();
		}

		public void SetPasswordMode(bool iModeOn)
		{
			symbolStage = false;
			showKeyboardLayout.SetPassowrdKeyboard(iModeOn);
			showKeyboardLayout.Accent.CapsChangeShow(VRIME_KeyboardData.CapsButton);
		}
		/// <summary>
		/// Europ Keyboard Use, Word Accent UI
		/// </summary>
		/// <param name="iShow"></param>
		/// <param name="iBtn"></param>
		public void CallAccentPanel(bool iShow, VRIME_KeyboardButton iBtn)
		{
			if(showKeyboardLayout.Accent != null)
				showKeyboardLayout.Accent.CallButtons(iShow, iBtn);
		}
		/// <summary>
		/// 打開LanguageWing改成Voice功能
		/// </summary>
		public void CallLanguageWing()
		{
			showKeyboardLayout.callLanWing(!showKeyboardLayout.lanWingShowState);
		}
		/// <summary>
		/// Change Language Setting By Language Wing UI
		/// </summary>
		/// <param name="iBtn"></param>
		public void CallChangeLanguage(VRIME_KeyboardButton iBtn)
		{
			showKeyboardLayout.ChangeLanguage(iBtn);
		}
		/// <summary>
		/// Voice Langugage, Same to CallChangeLanguage()
		/// </summary>
		/// <param name="iShow"></param>
		public void CallVoiceLanWing(bool iShow = true)
		{
			if(showKeyboardLayout.lanWingShowState != iShow)
				showKeyboardLayout.callLanWing(iShow);
		}
		/// <summary>
		/// Change Voice UI Using Language By Language Wing UI
		/// </summary>
		/// <param name="iBtn"></param>
		public void CallChangeVoiceLang(VRIME_KeyboardButton iBtn)
		{
			showKeyboardLayout.ChangeVoiceLanguage(iBtn);
		}
		/// <summary>
		/// Set Language Words by Voice Engine
		/// </summary>
		/// <param name="iEngine">Now using Language</param>
		public void SetVoiceLanWingButton(eVoiceEngine iEngine)
		{
			showKeyboardLayout.SetVoiceLanWingBtn(iEngine);
		}
		/// <summary>
		/// Set Button Tip Words.
		/// </summary>
		/// <param name="iVLang"></param>
		public void SetVoiceTip(SupportLanguage iVLang)
		{
			showKeyboardLayout.ChangeVoiceLanguageTip(iVLang);
		}
		/// <summary>
		/// cycle change language
		/// </summary>
		public void LanguageCycleRun()
		{
			showKeyboardLayout.ChangeLanguageCycle();
		}
		/// <summary>
		/// Show ToolTip
		/// </summary>
		public void CallTooltip()
		{
			if(VRIME_KeyboardSetting.TooltipHaveSeen)
				return;
			showKeyboardLayout.callTooltip(true);
		}
		/// <summary>
		/// ToolTip Turn Next Page
		/// </summary>
		public void TooltipTurnPage()
		{
			if(VRIME_KeyboardSetting.TooltipHaveSeen)
				return;

			string[] aPages =  Enum.GetNames(typeof(eToolTipPage));
			int aPageIndex = (int)showKeyboardLayout.toolTipNowPage;
			if(aPageIndex >= (aPages.Length - 2))
			{
				showKeyboardLayout.callTooltip(false);
			}
			else
			{
				aPageIndex++;
				showKeyboardLayout.SetTooltipPage((eToolTipPage)aPageIndex);
			}
		}
		/// <summary>
		/// Do ResetTracking To Turn Next Page
		/// </summary>
		public void TooltipResetTracking()
		{
			if(showKeyboardLayout.toolTipNowPage != eToolTipPage.State_2)
				return;
			
			TooltipTurnPage();
		}
		/// <summary>
		/// Move Keyboard's ToolTip
		/// </summary>
		public void TooltipMove()
		{
			if(showKeyboardLayout.toolTipNowPage != eToolTipPage.State_1)
				return;

			TooltipTurnPage();
		}
		/// <summary>
		/// Change Word
		/// </summary>
		/// <param name="iPage"></param>
		/// <param name="iWords"></param>
		public void SetTooltipPageInfo(eToolTipPage iPage, VRIME_TipWords iWords)
		{
			showKeyboardLayout.SetTooltipPagesWords(iPage, iWords);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="iType"></param>
		/// <returns></returns>
		public GameObject GetKeybuttonByType(eOmniButtonType iType)
		{
			GameObject aTypeObject;
			GameObject aResult = null;
			bool aSucess = buttonTypeObjects.TryGetValue(iType, out aTypeObject);
			if(aSucess)
			{
				aResult = GameObject.Instantiate(aTypeObject);
				aResult.name = aTypeObject.name;
			}
			return aResult;
		}
		#endregion
		#region private Function
		private void Init()
		{
			if(RootPath == null){
				VRIME_Debugger.LogError(Ins.name, "RootPath Is Null.");
				return;
			}
			// Make Backup key Button
			InitLabelButton();
			// Make Original Button Keys
			InitKeypadObject();
			// Function Set
			GetChildKeyboards();
		}
		/// <summary>
		/// Get Child at KeyboardManager have Keyboards, and set Staff on it.
		/// </summary>
		private void GetChildKeyboards()
		{
			string[] aKBNames =  Enum.GetNames(typeof(eKeyBoardLayoutType));
			allKeyboards = new VRIME_KeyboardStaff[RootPath.childCount];
			for(int i = 0; i < aKBNames.Length; i++)
			{
				SetKeyboardStaff(i, aKBNames[i]);
			}
		}
		/// <summary>
		/// WCL Label Buttons
		/// </summary>
		private void InitLabelButton()
		{
			GameObject aTmpLabel = VRIME_AssetLoad.GetGameObjectResource("Prefabs/LabelButton_CJKtc");
			aTmpLabel.SetActive(false);
			aTmpLabel.transform.parent = VRIME_Manager.Ins.BackgroundObjPath;
			LabelBtnOriginal = aTmpLabel.GetComponent<VRIME_KeyboardButton>();
			if(LabelBtnOriginal == null)
				LabelBtnOriginal = aTmpLabel.AddComponent<VRIME_KeyboardButton>();
			LabelBtnOriginal.Init();
		}
		/// <summary>
		/// Keypad Buttons
		/// </summary>
		private void InitKeypadObject()
		{
			string[] aButtonKeyType =  System.Enum.GetNames(typeof(eOmniButtonType));
			for(int i = 0; i < aButtonKeyType.Length; i++)
			{
				GameObject aTempKey = VRIME_AssetLoad.GetGameObjectResource("Prefabs/" + aButtonKeyType[i]);
				if(aTempKey == null)
					continue;
				
				aTempKey.SetActive(false);
				aTempKey.name = aButtonKeyType[i];
				aTempKey.transform.parent = VRIME_Manager.Ins.BackgroundObjPath;
				aTempKey.AddComponent<VRIME_KeyboardButton>();
				
				buttonTypeObjects.Add((eOmniButtonType)i, aTempKey);
			}
		}
		/// <summary>
		/// Set KeyboardStaff On Every Type Keyboard(Auto Get)
		/// </summary>
		/// <param name="iIndex"></param>
		/// <param name="iKBName"></param>
		private void SetKeyboardStaff(int iIndex, string iKBName)
		{
			for(int k = 0; k < allKeyboards.Length; k++)
			{
				Transform aTmp = RootPath.GetChild(k);
				if(aTmp.name == iKBName + "Keyboard")
				{
					VRIME_KeyboardStaff aTmpStaff = aTmp.GetComponent<VRIME_KeyboardStaff>();
					if(aTmpStaff == null)
						aTmpStaff = aTmp.gameObject.AddComponent<VRIME_KeyboardStaff>();

					aTmpStaff.staffType = (eKeyBoardLayoutType)iIndex;
					aTmpStaff.Init();

					allKeyboards[iIndex] = aTmpStaff;
					if(aTmpStaff.staffType == eKeyBoardLayoutType.English)
						showKeyboardLayout = allKeyboards[iIndex];
					break;
				}
			}
		}
		/// <summary>
		/// get Staff by Type
		/// </summary>
		/// <param name="iType"></param>
		/// <returns></returns>
		private VRIME_KeyboardStaff GetKeyboardStaff(eKeyBoardLayoutType iType)
		{
			VRIME_KeyboardStaff aResult = null;
			for(int i = 0; i < allKeyboards.Length; i++)
			{
				if(allKeyboards[i].staffType == iType){
					aResult = allKeyboards[i];
					break;
				}

			}
			return aResult;
		}
		/// <summary>
		/// Show keyboard auto select keypad config
		/// </summary>
		private void ShowKeyboardByLanguage()
		{
			bool aPassowrdMode = VRIME_InputFieldOversee.Ins.PasswordMode;
			SetPasswordMode(aPassowrdMode);
			if(aPassowrdMode == false)
			{
				SetCapsState(eCapsState.Lower);
			}
			// Close language fucntion key active.
			bool aCloseFunction = VRIME_KeyboardSetting.LanguagePool.Length <=1;
			if(aPassowrdMode)
				aCloseFunction = true;
			
			SetLanguageFunctionActive(!aCloseFunction);
		}
		public string GetCapsButtonWord(eCapsState iState)
		{
			string aResult = string.Empty;
			switch(iState)
			{
				case eCapsState.Lower:
					aResult = omniIconLower;
					break;
				case eCapsState.UpperUnLock:
					aResult = omniIconUpperUnLock;
					break;
				case eCapsState.UpperLock:
					aResult = omniIconUpperLock;
					break;
			}
			return aResult;
		}
		public void SetLanguageFunctionActive(bool iActive)
		{
			VRIME_KeyboardButton aLanBtn = showKeyboardLayout.GetFunctionButton(eButtonType.Language);
			aLanBtn.Event.gameObject.SetActive(iActive);
		}
		#endregion
	}
}