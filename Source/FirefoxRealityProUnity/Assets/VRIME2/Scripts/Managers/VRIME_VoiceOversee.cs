// ========================================================================== //
//  Created: 2019-02-13
// ========================================================================== //

namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Htc.Omni;
	
	[AddComponentMenu("VRIME/IMEManager/Voice Oversee")]
	public class VRIME_VoiceOversee : MonoBehaviour {
		public static VRIME_VoiceOversee Ins {
			get { return instance; }
			set {
				instance = value;
				instance.Init();
			}
		}
		private static VRIME_VoiceOversee instance;

		#region public Field
		public Transform RootPath;
		public bool inputStatus { get { return voiceShowState; } }
		private bool voiceShowState = false;
		public eVoiceEngine inputEngine { get { return usingEngine; } }
		private eVoiceEngine usingEngine;
		public SupportLanguage InputLanguage { get { return usingLanguage; } }
		private SupportLanguage usingLanguage;
		#endregion
		#region private Field
		// [SerializeField]
		private GameObject voiceRoot;
		private keyboardAudioInput keyboardAudio;
		private VRIME_KeyboardButton functionKey;
		private Animator inputMotion;// Animation
		#endregion
		#region public Fucntion
		/// <summary>
		/// Call keyboardAudioInput UI Open/Close Recycle
		/// </summary>
		public void CallVoiceUI() { CallVoiceUI(!voiceShowState); }
		/// <summary>
		/// Call keyboardAudioInput UI by Value
		/// </summary>
		/// <param name="iShow"></param>
		public void CallVoiceUI(bool iShow)
		{
			if(voiceRoot == null)
				return;
			if(voiceShowState == iShow)
				return;

			VRIME_KeyboardSetting.VoiceInputBlock = iShow;
			// InputField Controll
			if(VRIME_InputFieldOversee.Ins != null) {
				VRIME_InputFieldOversee.Ins.VoiceInputCallAction(iShow);
			}
			// UI Show
			AnimeShow(iShow);
			voiceRoot.SetActive(iShow);
			voiceShowState = iShow;
			UpdateIconState();
			// BI Logger Add
			VRIME_BISender.Ins.UpdateEngineInfo();
		}
		/// <summary>
		/// Update Icon State To Know Voice UI Open State
		/// </summary>
		public void UpdateIconState()
		{
			CancelInvoke("SetFunctionKeyState");
			Invoke("SetFunctionKeyState", 0.1f);
		}
		/// <summary>
		/// Cahnge keyboardAudioInput Using Language
		/// </summary>
		/// <param name="iLanguage"></param>
		public void SetSpeechLanguage(SupportLanguage iLanguage)
		{
			// if(keyboardAudio == null)
			// 	return;
			keyboardAudioInput.SpeechLanguage = iLanguage;
			usingLanguage = iLanguage;
			VRIME_KeyboardOversee.Ins.SetVoiceTip(iLanguage);
		}
		/// <summary>
		/// Keep CallVoiceUI's Button and Extra Set CallVoice State
		/// </summary>
		/// <param name="iBtn"></param>
		public void SetVoiceFunctionKey(VRIME_KeyboardButton iBtn)
		{
			if(iBtn == null || functionKey == iBtn)
				return;
			if(iBtn.Event == null)
				iBtn.Init();
			// Remove
			iBtn.Event.onEnter.RemoveListener(ButtonEnter);
			iBtn.Event.onStay.RemoveListener(ButtonEnter);
			iBtn.Event.onExit.RemoveListener(ButtonExit);
			// Add
			iBtn.Event.onEnter.AddListener(ButtonEnter);
			iBtn.Event.onStay.AddListener(ButtonEnter);
			iBtn.Event.onExit.AddListener(ButtonExit);
			functionKey = iBtn;
		}
		/// <summary>
		/// Update FunctionKey's Language Tip
		/// </summary>
		public void ReNewKeyboardTip()
		{
			VRIME_KeyboardOversee.Ins.SetVoiceTip(usingLanguage);
		}
		/// <summary>
		/// OpenStart 到Opened的狀態
		/// </summary>
		public void AnimeToVoiceOpend()
		{
			// if(WCLVisState == eAnimWCL.WCLStates_VoiceClosed)
			// 	return;
			
			AnimePlay(eAnimWCL.WCLStates_VoiceOpened);
		}
		/// <summary>
		/// 供外部初始化用
		/// </summary>
		public void VoiceEngineInit()
		{
			keyboardAudio = RootPath.GetComponentInChildren<keyboardAudioInput>(true);
			keyboardAudio.InitVoiceEngine(VoiceEngineCallback, this);
		}
		#endregion
		#region private Function
		private void Init()
		{
			if(RootPath == null){
				VRIME_Debugger.LogError(Ins.name, "RootPath Is Null.");
				return;
			}

			voiceRoot = RootPath.gameObject;
			// Voice Engine Init
			VoiceEngineInit();
			// Animation init
			if(VRIME_InputFieldOversee.Ins != null)
			{
				Transform aInputMotion = VRIME_InputFieldOversee.Ins.RootPath.Find("Motion");
				inputMotion = aInputMotion.GetComponent<Animator>();
				VRIME_Debugger.Log("Animtor Get.");
			}
		}
		/// <summary>
		/// keyboardAudioInput's CallBack
		/// </summary>
		/// <param name="engineBase"></param>
		/// <param name="language"></param>
		private void VoiceEngineCallback(VoiceEngineBase engineBase, SupportLanguage language)
		{
			// 原先是要取VoiceEngineBase.VoiceEngineName知道是從哪個Engine來的
			// 但實際上這個值進來的時候是null的狀態
			// 所以直接GetType().Name取Class名稱作為辨識
			VRIME_Debugger.Log("Engine :" + engineBase.GetType().Name + ", Language :" + language.ToString());
			usingEngine = eVoiceEngine.None;

			string aEngineTypeName = engineBase.GetType().Name;
			switch(aEngineTypeName)
			{
				case "GoogleVoiceEngineBase" : usingEngine = eVoiceEngine.google; break; 
				case "BaiduVoiceEngineBase" : usingEngine = eVoiceEngine.baidu; break;
			}
			usingLanguage = language;

			VRIME_KeyboardOversee.Ins.SetVoiceLanWingButton(usingEngine);
			VRIME_KeyboardOversee.Ins.SetVoiceTip(usingLanguage);
		}
		/// <summary>
		/// use invoke change voice icon state
		/// 增加其他狀況關閉VoiceInput UI，所以需要增加回復狀態
		/// </summary>
		private void SetFunctionKeyState()
		{
			if(functionKey == null)
				return;
			Transform aExtraWord = functionKey.rootUI.Find(VRIME_KeyboardData.cLanTipName);
			TMPro.TextMeshPro aExtraPro = aExtraWord.GetComponent<TMPro.TextMeshPro>();
			
			if(voiceShowState) {
				functionKey.motion.Play(eAnimBtn.Pressed.ToString());
				aExtraPro.color = VRIME_KeyboardData.GetViveColors(eViveColor.PressedBlue);
			}
			else {
				functionKey.motion.Play(eAnimBtn.Normal.ToString());
				aExtraPro.color = VRIME_KeyboardData.GetViveColors(eViveColor.NormalBlack);
			}
		}
		/// <summary>
		/// Function Key Extra Event
		/// </summary>
		/// <param name="iObj"></param>
		private void ButtonEnter(GameObject iObj)
		{
			Transform aExtraWord = functionKey.rootUI.Find(VRIME_KeyboardData.cLanTipName);
			TMPro.TextMeshPro aExtraPro = aExtraWord.GetComponent<TMPro.TextMeshPro>();
			aExtraPro.color = VRIME_KeyboardData.GetViveColors(eViveColor.PressedBlue);
		}
		/// <summary>
		/// Function Key Extra Event
		/// </summary>
		/// <param name="iObj"></param>
		private void ButtonExit(GameObject iObj)
		{
			Transform aExtraWord = functionKey.rootUI.Find(VRIME_KeyboardData.cLanTipName);
			TMPro.TextMeshPro aExtraPro = aExtraWord.GetComponent<TMPro.TextMeshPro>();
			aExtraPro.color = VRIME_KeyboardData.GetViveColors(eViveColor.NormalBlack);
		}
		/// <summary>
		/// 打開Voice時候的動畫
		/// </summary>
		/// <param name="iShow"></param>
		private void AnimeShow(bool iShow)
		{
			if(iShow)
			{
				AnimePlay(eAnimWCL.WCLStates_VoiceOpened_Start);
				VRIME_KeyAudioOversee.Ins.VoiceInputStatus(eIMEUIStatus.enter_show);
			}
			else
			{
				AnimePlay(eAnimWCL.WCLStates_VoiceClosed);
				VRIME_KeyAudioOversee.Ins.VoiceInputStatus(eIMEUIStatus.exit_hide);
			}
		}
		/// <summary>
		/// InputOversee's Animator
		/// </summary>
		/// <param name="iState"></param>
		private void AnimePlay(eAnimWCL iState)
		{
			if(inputMotion == null)
				return;

			inputMotion.Play(iState.ToString());
		}
		#endregion

	}
}