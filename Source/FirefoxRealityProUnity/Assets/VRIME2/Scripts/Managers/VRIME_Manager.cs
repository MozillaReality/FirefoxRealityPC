// ========================================================================== //
//  Created: 2019-01-04
// ========================================================================== //
// 2019-01-09(update in HTC Life)
// 追記：
//     如果要做成像AssetStore上使用的話，是不能夠像現在這樣叫你自己看場景的鍵盤物件自己複製貼上去套。
//     Design所謂的「一個Perfab修改，全部都會修改」正是因為場景上的Perfab物件都是「相同來源」才能辦到的事情。
//     之後勢必要把整個鍵盤拉成一個Perfab，這樣外部Developer在使用鍵盤的時候拉一整個Perfab使用才直覺，
//     散一大塊在那邊就不像是打算放上AssetStore供人使用的。
//     這個部分鼓棒由於比較簡單就先這樣實作了，未來勢必得朝這個方向製作(自動載入、組裝鍵盤等等)。
//     就先各種盡力而為吧。
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
    using Htc.Omni;
    using UnityEngine;
	using UnityEngine.Events;	
#if steamvr_v2
	using Valve.VR;
#endif
	
	[RequireComponent(typeof(VRIME_KeyAudioOversee))]
	[RequireComponent(typeof(VRIME_VoiceOversee))]
	[RequireComponent(typeof(VRIME_InputFieldOversee))]
	[RequireComponent(typeof(VRIME_ControllerOversee))]
	[RequireComponent(typeof(VRIME_KeyboardOversee))]
	[AddComponentMenu("VRIME/IMEManager/Manager")]
	public class VRIME_Manager : MonoBehaviour
	{
		public static VRIME_Manager Ins {
			get {
				if(instance == null)
				{
					instance = new VRIME_Manager();
					instance.Init();
				}
				return instance;
			}
		}
		private static VRIME_Manager instance;

		public class submitCallback : UnityEvent<string> { }
		/// <summary>
		/// callback: how to use haptic time
		/// </summary>
		/// <typeparam name="float">delay haptic time</typeparam>
		/// <typeparam name="ushort">haptic duration time</typeparam>
		/// <typeparam name="Transform">do haptic controller Transform</typeparam>
		public class hapticCallback : UnityEvent<float, ushort, Transform> { }
		public class stateChangeCallback : UnityEvent<bool> { }
		public class ctrlShowCallBack : UnityEvent<eControllerType, bool> { }
		public class inputFieldOnChanged : UnityEvent<string> { }
		
		private const string ManagerName = "[VRIME_Manager]";
		private const string RootPathName = "Main";
		private const string StoragePathName = "MakeStorage";

		public static VRIME_LanguageSys runSystem;
		#region Event Field
		[Header("Event")]
		public submitCallback onSubmit = new submitCallback();
		public stateChangeCallback onCallIME = new stateChangeCallback();
		/// <summary>
		/// 替換鼓棒(CallController)發生時，要打的Callback
		/// </summary>
		public ctrlShowCallBack onControllerShow = new ctrlShowCallBack();
		public inputFieldOnChanged onInputValueChange = new inputFieldOnChanged();
		#endregion
		#region Public/Private Field
		public Transform BackgroundObjPath { get { return mInstanTrans; } }
		private Transform mInstanTrans;
		public bool ShowState { get { return mIMEShow; } }
		private bool mIMEShow = false;
		public Transform MainObj{ get { return mObjectRoot; } }
		private Transform mObjectRoot;
		public bool StageMode {
			get { return VRIME_BISender.StageMode; }
			set { VRIME_BISender.StageMode = value; }
		}
		public int LanguagePoolIndex{
			get { return languageCycleIndex; }
			set { languageCycleIndex = value; }
		}
		private int languageCycleIndex;
		#endregion
		#region public field
		
		[Header("Voice Input Engine")]
		public keyboardAudioInput.VoiceEngineType voiceEngineType = keyboardAudioInput.VoiceEngineType.Baidu;

		[Header("Default Voice Input Language")]
		public SupportLanguage defaultVoiceInputLanguage;

		[Header("VoiceInput-Google api requirement")]
		public string googleApiKey;

		[Header("VoiceInput-Baidu api requirement")]
		public string baiduApiKey;
		public string baiduApiSecert;

		[Space(25)]
		[Header("Controller Object")]
		public GameObject controllerModelLeft;
		public GameObject controllerModelRight;

#if steamvr_v2
		public SteamVR_Behaviour_Pose userControllerLeft;
		public SteamVR_Behaviour_Pose userControllerRight;
#else		
		public SteamVR_TrackedObject userControllerLeft;
		public SteamVR_TrackedObject userControllerRight;
#endif		
		[Header("Head Camera")]
		[SerializeField]
		private Transform userHeadCamera;

		[Header("Move InputField Cursor")]
		public bool MoveCursorByTouchpad = true;
		[Space(10)]
		[Header("IME Keyboard Default Language")]
		public eLanguage defaultKeyboardLanguage = eLanguage.English;
		[Header("IME Keyboard Support Language")]
		public bool lanEnglish = true;
		public bool lanPinYin = true;
		public bool lanZhuyin = true;
		public bool lanFrench = true;
		public bool lanItalian = true;
		public bool lanGerman = true;
		public bool lanSpanish = true;
		#endregion

		#region private field
		public VRIME_TrackingObject TrackingObj{
			get { return mTracking; }
			set { mTracking = value; }
		}
		// [Header("Tracking Use")]
		// [SerializeField]
		private VRIME_TrackingObject mTracking;

		private VRIME_MoveAndScale moveAndScale;
		#endregion

		#region Unity Function
		private void Awake() {
			if(instance == null) {
				Init();
				VRIME_KeymapInstance.InitDefaultKeymaps();
			}
			moveAndScale = this.GetComponent<VRIME_MoveAndScale>();
		}
		private void OnApplicationQuit() {
			VRIME_BISender.Ins.FlushData();// Close Application to Flush BI Data
		}
		#endregion
		#region public Function
		public void ShowIME(string iEntry = "")
		{
			VRIME_BISender.Ins.AppEntry = iEntry;
			CallIME(true);
		}
		public void ShowIME(string iDefText, string iEntry = "")
		{
			VRIME_BISender.Ins.AppEntry = iEntry;
			CallIME(true, iDefText);
		}
		public void ShowIME(string iDefText, eKeyBoardLayoutType iKeyboard, string iEntry = "")
		{
			VRIME_BISender.Ins.AppEntry = iEntry;
			CallIME(true, iDefText, iKeyboard);
		}
		public void ShowIME(bool iPassowrdMode, string iDefText, string iHoderText, string iEntry = "")
		{
			VRIME_BISender.Ins.AppEntry = iEntry;
			CallIME(true, iDefText, eKeyBoardLayoutType.English, iPassowrdMode, iHoderText);
		}
		public void ShowIME(bool iPassowrdMode, string iDefText, eKeyBoardLayoutType iKeyboard, string iHoderText, string iEntry = "")
		{
			VRIME_BISender.Ins.AppEntry = iEntry;
			CallIME(true, iDefText, iKeyboard, iPassowrdMode, iHoderText);
		}
		public void HideIME() { CallIME(false); }
		public void ResetTrackingPos()
		{
			if(mTracking.trackedHeadEye == null && userHeadCamera != null) {
				mTracking.trackedHeadEye = userHeadCamera;
			}

			mTracking.UpdatePosition();
			VRIME_KeyboardOversee.Ins.TooltipResetTracking();
		}

		public void ChangeLanguage(eLanguage iLan) { VRIME_KeyboardSetting.IMELanguage = iLan; }
		public void SetHeadCamera(Transform iHead) { userHeadCamera = iHead; }
		public void SubmitText()
		{
			// Start Submit
			string aSubmitString = string.Empty;
			// Call WCL Send To InputTemp
			VRIME_InputFieldOversee.Ins.CancelCompose();
			// Get Input Words
			aSubmitString = VRIME_InputFieldOversee.Ins.InputTemp;
			// After IME Submit Finish, BI Logger Before CleanInput.
			VRIME_BISender.Ins.CallSubmit();
			// Clean And Hide UI
			VRIME_InputFieldOversee.Ins.ClearInput();
			// onSubmit Event Send
			if(onSubmit != null)
				onSubmit.Invoke(aSubmitString);
		}
		/// <summary>
		/// Change IME Language and key pad
		/// </summary>
		/// <param name="iLang"></param>
		public void UpdateKeyboardLanguage(eLanguage iLang)
		{
			VRIME_KeyboardSetting.IMELanguage = iLang;
			VRIME_InputFieldOversee.Ins.CancelCompose();
			VRIME_InputFieldOversee.Ins.WCLClosePanel();
			VRIME_InputFieldOversee.Ins.SetInputPlaceholder(VRIME_InternationalWord.InputPlaceholderText(iLang));
			VRIME_KeyboardOversee.Ins.CallAccentPanel(false, null);
			// Update System
			runSystem = VRIME_LanguageFactory.MakeUsingSystem(iLang);
			
			VRIME_KeyboardOversee.Ins.SetSymbolMode(false);
			VRIME_KeyboardOversee.Ins.SetConfigBySetting();
			// BI Logger Add
			VRIME_BISender.Ins.UpdateEngineInfo();
		}
		/// <summary>
		/// 叫Oversee也是可以，不過讓使用者方便呼叫就這樣做
		/// </summary>
		public void VoiceEngineInit() { VRIME_VoiceOversee.Ins.VoiceEngineInit(); }
		#endregion
		#region  private Function
		private void Init()
		{
			instance = this.GetComponent<VRIME_Manager>();
			if(instance == null)
			{
				VRIME_Debugger.LogError("VRIME_Manager Not In Scenes!!!");
				return;
			}	
			if(!ManagerName.Equals(instance.name)) {
				instance.name = ManagerName;
			}
			// Language Info Set
			GetNewLanguagePool();
			SetDefaultLanguage();
			// Get object root
			mObjectRoot = instance.transform.Find(RootPathName);
			if(mObjectRoot == null)
			{
				// Make obj path
				GameObject aTmpRoot = new GameObject(RootPathName);
				mObjectRoot = aTmpRoot.transform;
				mObjectRoot.parent = instance.transform;
			}
			// Get storage root
			mInstanTrans = mObjectRoot.transform.Find(StoragePathName);
			if(mInstanTrans == null)
			{
				// Make gameObject storage path
				GameObject aTmpStorage = new GameObject(StoragePathName);
				mInstanTrans = aTmpStorage.transform;
				mInstanTrans.parent = mObjectRoot;
			}
			// Add tracking object
			mTracking = instance.GetComponent<VRIME_TrackingObject>();
			if(mTracking == null)
				mTracking = instance.gameObject.AddComponent<VRIME_TrackingObject>();
			// Init language running system.
			runSystem = VRIME_LanguageFactory.MakeUsingSystem(VRIME_KeyboardSetting.IMELanguage);
			// Init Oversee
			VRIME_ControllerOversee.Ins = this.GetComponent<VRIME_ControllerOversee>();
			VRIME_KeyboardOversee.Ins = this.GetComponent<VRIME_KeyboardOversee>();
			VRIME_InputFieldOversee.Ins = this.GetComponent<VRIME_InputFieldOversee>();
			VRIME_VoiceOversee.Ins = this.GetComponent<VRIME_VoiceOversee>();
			VRIME_KeyAudioOversee.Ins = this.GetComponent<VRIME_KeyAudioOversee>();
#if VOICEINPUT_LOGUI
			GameObject aTmpInfoUI = VRIME_AssetLoad.GetGameObjectResource("Prefabs/VoiceInfomation");
			if(aTmpInfoUI != null) {
				aTmpInfoUI.transform.parent = mObjectRoot;
				aTmpInfoUI.transform.localPosition = new Vector3(0.8f, 0.4f);
				aTmpInfoUI.transform.localRotation = Quaternion.Euler(new Vector3(0f, 60f));
				aTmpInfoUI.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
			}
#endif
			// Hide IME
			AnimePlay(eAnimIME.Disabled);			
			// BI setder Init
			VRIME_BISender.Init();
		}
		private void CallIME(bool iShow, string iDefText = "", eKeyBoardLayoutType iKeyboard = eKeyBoardLayoutType.English, bool iPassMode = false, string iHoderText = "")
		{
			// if No Any Controller Call, Don't Show Keyboard.
			if(controllerModelLeft == null &&  controllerModelRight == null)
				return;
			// BI Logger, prevent repeated calls
			if(mIMEShow != iShow)
				VRIME_BISender.Ins.CallIMEStauts(iShow);
			// Start Show/Hide IME
			mIMEShow = iShow;// Set Status
			eLanguage aCheckSystem = VRIME_KeyboardSetting.IMELanguage;
			if(iShow) {
				if(iPassMode) {
					aCheckSystem = eLanguage.English;
					VRIME_InputFieldOversee.Ins.PasswordInputfieldSet(iHoderText);
				}
				else {
					VRIME_InputFieldOversee.Ins.DefaultInputfieldSet();
				}
				runSystem = VRIME_LanguageFactory.MakeUsingSystem(aCheckSystem);
				AnimePlay(eAnimIME.Summon);
				ResetTrackingPos();
				VRIME_KeyboardOversee.Ins.ShowKeyboard(iKeyboard);
				VRIME_InputFieldOversee.Ins.InsertDefault(iDefText);
				Invoke("ShowTooltipCheck", 0.5f);
			}
			else {
				AnimePlay(eAnimIME.Dismiss);
				VRIME_KeyboardOversee.Ins.CallAccentPanel(false, null);
				VRIME_KeyboardOversee.Ins.CallVoiceLanWing(false);
				VRIME_InputFieldOversee.Ins.CancelCompose();
				// BI Logger Data Clear
				VRIME_BISender.Ins.LabelDeleteAll();
			}
			// Change User Controller To IME Controller
			VRIME_ControllerOversee.Ins.CallController(iShow);
			// Save keyboard Show(IME Settings)
			VRIME_KeyboardSetting.UsingKeyboard = iKeyboard;
			// Voice Input
			if(VRIME_VoiceOversee.Ins) {
				VRIME_VoiceOversee.Ins.ReNewKeyboardTip();
				if(VRIME_KeyboardSetting.VoiceInputBlock)
					VRIME_VoiceOversee.Ins.CallVoiceUI(false);
			}
			// Move and Scale Track Obj
			SetMoveAndScaleController(iShow);
			// Audio Play
			VRIME_KeyAudioOversee.Ins.CallIME(iShow);
			// IME Call Back
			if(onCallIME != null)
				onCallIME.Invoke(iShow);
		}
		/// <summary>
		/// Check New User Tool Tips
		/// </summary>
		private void ShowTooltipCheck()
		{
			CancelInvoke("ShowTooltipCheck");
			if(VRIME_KeyboardSetting.TooltipHaveSeen)
				return;
			
			VRIME_KeyboardOversee.Ins.CallTooltip();
		}
		/// <summary>
		/// Move And Sclae Use Steam VR
		/// </summary>
		/// <param name="iShow"></param>
		private void SetMoveAndScaleController(bool iShow)
		{			
			if(iShow) {				
				moveAndScale.enabled = true;
#if steamvr_v2				
				moveAndScale.leftControllerIndex =  userControllerLeft.GetDeviceIndex();
				moveAndScale.rightControllerIndex = userControllerRight.GetDeviceIndex();
#else
				moveAndScale.leftControllerIndex =  (int)userControllerLeft.index;
				moveAndScale.rightControllerIndex = (int)userControllerRight.index;
#endif				
				moveAndScale.leftControllerTransform =  userControllerLeft.transform;
				moveAndScale.rightControllerTransform = userControllerRight.transform;

				moveAndScale.trackedObject = userHeadCamera;
			}
			else {
				moveAndScale.enabled = false;
			}
		}
		/// <summary>
		/// Play Keyboard animation
		/// </summary>
		/// <param name="iState"></param>
		private void AnimePlay(eAnimIME iState)
		{
			if(VRIME_KeyboardSetting.EnableKeyboraButtondAnimation)
            {
				Animator aMotion = mObjectRoot.GetComponent<Animator>();
				aMotion.Play(iState.ToString());
			}
			else
			{
				switch(iState)
				{
					case eAnimIME.Disabled: case eAnimIME.Dismiss:
						mObjectRoot.gameObject.SetActive(false);
						break;
					case eAnimIME.Normal: case eAnimIME.Summon:
						mObjectRoot.gameObject.SetActive(true);
						break;
				}
			}
			
		}
		#endregion
		#region Language Set
		public void GetNewLanguagePool()
		{
			bool[] aSetList = new bool[] {
				lanEnglish,
				lanPinYin,
				lanZhuyin,
				lanFrench,
				lanItalian,
				lanGerman,
				lanSpanish
			};
			// Set lanugage pool
			List<eLanguage> aSettingPool = new List<eLanguage>();
			for(int i = 0; i < aSetList.Length; i++)
			{
				if(aSetList[i])
				{
					aSettingPool.Add((eLanguage) i);
				}
			}
			if(aSettingPool.Count < 1)
			{
				aSettingPool.Add(eLanguage.English);
				lanEnglish = true;
			}
			
			VRIME_KeyboardSetting.LanguagePool = aSettingPool.ToArray();
		}

		private bool SetDefaultLanguage()
		{
			bool aSetSuccess = false;
			// Set default language, and language pool start index
			for(int i = 0; i < VRIME_KeyboardSetting.LanguagePool.Length; i++)
			{
				if(defaultKeyboardLanguage != VRIME_KeyboardSetting.LanguagePool[i])
					continue;
				
				VRIME_KeyboardSetting.IMELanguage = defaultKeyboardLanguage;
				LanguagePoolIndex = i;
				aSetSuccess = true;
				break;
			}
			if(aSetSuccess == false)
			{
				VRIME_KeyboardSetting.IMELanguage = VRIME_KeyboardSetting.LanguagePool[0];
				LanguagePoolIndex = 0;
			}
			return aSetSuccess;
		}
		#endregion

		#if steamvr_v2		
		public void MoveKeyboardHandle(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
			moveAndScale.GripChangeHandler(fromAction, fromSource, newState);
		}

		public void MoveCursorHandle(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
			VRIME_InputFieldOversee.Ins.countrollerTouchPad.TouchChangeHandler(fromAction, fromSource, newState);
		}

		public void MoveCursorPositionHandle(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta) {
			VRIME_InputFieldOversee.Ins.countrollerTouchPad.TouchAxisHandler(fromAction, fromSource, axis, delta);
		}
		#endif
	}
}