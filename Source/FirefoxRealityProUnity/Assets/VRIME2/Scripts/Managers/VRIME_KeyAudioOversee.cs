// ========================================================================== //
//  Created: 2019-03-11
// ========================================================================== //
namespace VRIME2
{
    using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using Valve.VR;

    [AddComponentMenu("VRIME/IMEManager/KeyAudio Oversee")]
	public class VRIME_KeyAudioOversee : MonoBehaviour
	{
		public static VRIME_KeyAudioOversee Ins {
			get { return instance; }
			set {
				instance = value;
				instance.Init();
			}
		}
		private static VRIME_KeyAudioOversee instance;
		#region const Field
		private const float defVolume = 0.8f;
		private const string audioObjName = "KeyAudioTool";
		#endregion
		#region public Field
		#endregion
		#region private Field
		private Transform sourcePath;
		private AudioSource sourceTool;
		private VRIMEKeyAudioBoardInfo audioInfoData;
		[SerializeField]
		private Dictionary<eKeyAudioType, AudioClip> imeAudioClipList;
		private float pressTime;
		private VRIME_KeyboardButton previousButton;
		#endregion
		#region unity Function
		private void Awake() {
			DefaultAudioSetting();
		}
		#endregion
		#region public Function
		/// <summary>
		/// Most Audio Will By Button Click
		/// </summary>
		/// <param name="iStatus">Trigger Status</param>
		/// <param name="iBtnType">Button Type</param>
		public void ButtonEventStatus(eIMEUIStatus iStatus, VRIME_KeyboardButton iButton)
		{
			previousButton = iButton;
			switch(iStatus)
			{
				case eIMEUIStatus.enter_show: 
				case eIMEUIStatus.exit_hide:
					AudioEventButtonInOut(iStatus, iButton.btnType);
					break;
				case eIMEUIStatus.stay_longpress: AudioEventButtonStay(iButton.btnType); break;
			}
		}
		/// <summary>
		/// Voice Input Use Audio
		/// </summary>
		public void VoiceInputStatus(eIMEUIStatus iStatus)
		{
			VRIMEKeyAudioInfo aTemp = null;
			ushort aHapticDura = 0;

			if(iStatus == eIMEUIStatus.enter_show)
			{
				aTemp = audioInfoData.GetInfo(eAudioStatusType.voice_dictation_start);
				aHapticDura = aTemp.hapticEnter;
			}
			else if(iStatus == eIMEUIStatus.exit_hide)
			{
				aTemp = audioInfoData.GetInfo(eAudioStatusType.voice_dictation_stop);
				aHapticDura = aTemp.hapticExit;
			}
			
			if(aTemp == null)
				return;

			PlayAudioEvent(sourceTool, aTemp.keyAudioType, aTemp.audioPitch);
			CallHapticEvent(aTemp.delayHaptic, aHapticDura);
		}
		public void LanguageWing(bool iShow)
		{
			VRIMEKeyAudioInfo aTemp = null;
			ushort aHapticDura = 0;
			if(iShow)
			{
				aTemp = audioInfoData.GetInfo(eAudioStatusType.expand_menu_key);
				aHapticDura = aTemp.hapticEnter;
			}
			if(aTemp == null)
				return;

			PlayAudioEvent(sourceTool, aTemp.keyAudioType, aTemp.audioPitch);
			CallHapticEvent(aTemp.delayHaptic, aHapticDura);
		}
		/// <summary>
		/// Many Button Function Use Audio not on click, It maybe Finish Other Time at after Click
		/// </summary>
		/// <param name="iType"></param>
		/// <param name="iSecondSource"></param>
		public void FunctionSequence(eAudioStatusType iType)
		{
			VRIMEKeyAudioInfo aTemp = audioInfoData.GetInfo(iType);
			if(aTemp == null)
				return;
			
			// Play Evnet
			PlayAudioEvent(sourceTool, aTemp.keyAudioType, aTemp.audioPitch);
			CallHapticSquence(aTemp.sequenceInfo);
		}
		/// <summary>
		/// For IME Show Use
		/// </summary>
		/// <param name="iShow"></param>
		public void CallIME(bool iShow)
		{
			sourceTool.Stop();
			eAudioStatusType aType = eAudioStatusType.appear_enter_key;
			if(iShow == false)
				aType = eAudioStatusType.dissmiss_exit_key;
			// Play Audio
			FunctionSequence(aType);
		}
		public void DeleteLoop()
		{
			VRIMEKeyAudioInfo aLoopData = audioInfoData.GetInfo(eAudioStatusType.delete_backspace_repeat);
			PlayAudioEvent(aLoopData.keyAudioType, aLoopData.audioPitch);
		}
		#endregion
		#region private Function
		private void Init()
		{
			// Load Clip Resource
			LoadAllAudioData();
			// Make Audio Control Object(First AudioSource)
			sourcePath = VRIME_Manager.Ins.MainObj.Find(audioObjName);
			if(sourcePath == null)
			{
				GameObject aTmpSource = new GameObject(audioObjName);
				sourcePath = aTmpSource.transform;
				sourcePath.parent = VRIME_Manager.Ins.MainObj;
			}
			sourceTool = sourcePath.GetComponent<AudioSource>();
			if(sourceTool == null)
				sourceTool = sourcePath.gameObject.AddComponent<AudioSource>();
			// Set Default AudioSource Set
			DefaultAudioSetting();
		}
		/// <summary>
		/// Load Audio Data and Resource to Oversee Keep.
		/// </summary>
		private void LoadAllAudioData()
		{
			audioInfoData = new VRIMEKeyAudioBoardInfo();
			imeAudioClipList = new Dictionary<eKeyAudioType, AudioClip>();

            var aTypeArray = System.Enum.GetValues(typeof(eKeyAudioType));
			for(int i = 0; i < aTypeArray.Length; i++)
			{
				eKeyAudioType aType = (eKeyAudioType)aTypeArray.GetValue(i);
				AudioClip aLoadClip = VRIME_AssetLoad.GetAudioClipResource("SoundEffect/"+ aType.ToString());
				if(aLoadClip == null)
					continue;
				imeAudioClipList.Add(aType, aLoadClip);
			}
        }
		/// <summary>
		/// Send Haptic Event By Data Value
		/// </summary>
		/// <param name="iDelay">Delay Time Value, float</param>
		/// <param name="iDuration">Haptic Duration Time, ushort</param>
		private void CallHapticEvent(float iDelay, ushort iDuration)
		{
			if(iDuration <= 0)
				return;
			// Get Controller
			VRIMEControllerInfo aController = VRIME_ControllerOversee.Ins.GetLastController();
			if(aController == null)
				return;
			// Get SteamVR_TrackedObject
#if steamvr_v2
			SteamVR_Behaviour_Pose aTrackedObj = null;
#else
			SteamVR_TrackedObject aTrackedObj = null;

#endif			
			switch(aController.HandType)
			{
				case eStickType.left: aTrackedObj = VRIME_Manager.Ins.userControllerLeft; break;
				case eStickType.right: aTrackedObj = VRIME_Manager.Ins.userControllerRight; break;
			}			
			
			if(aTrackedObj == null)
				return;

#if steamvr_v2
			int iSteamVRCtrlIndex = aTrackedObj.GetDeviceIndex();
#else
			int iSteamVRCtrlIndex = (int)aTrackedObj.index;
#endif			
			VRIME_Manager.Ins.StartCoroutine(StartHaptic(iDelay, iDuration, iSteamVRCtrlIndex));
		}
		/// <summary>
		/// Send Many HapticEvent 
		/// </summary>
		/// <param name="iSquenceArray">Array Data</param>
		private void CallHapticSquence(VRIMEHapticSquence[] iSquenceArray)
		{
			if(iSquenceArray == null)
				return;
		    if(VRIME_KeyboardSetting.EnableControllerVibration == false)
                return;
			
			for(int i = 0; i < iSquenceArray.Length; i++)
			{
				VRIMEHapticSquence aData = iSquenceArray[i];
				CallHapticEvent(aData.delayTime, aData.durationTime);
			}
		}
		/// <summary>
		/// StartCorourtine To Hpatic Controller
		/// </summary>
		/// <param name="iDelayTime"></param>
		/// <param name="iDuration"></param>
		/// <param name="iControllerIndex"></param>
		/// <returns></returns>
		private IEnumerator StartHaptic(float iDelayTime, ushort iDuration, int iControllerIndex)
		{
			yield return new WaitForSeconds(iDelayTime);// Delay Time
			//SteamVR_Controller.Input(iControllerIndex).TriggerHapticPulse(iDuration, EVRButtonId.k_EButton_Max);
			var axisId = (uint)EVRButtonId.k_EButton_SteamVR_Touchpad - (uint)EVRButtonId.k_EButton_Axis0;
			OpenVR.System.TriggerHapticPulse((uint)iControllerIndex, axisId, (char)iDuration);

		}
		/// <summary>
		/// play Audio by Keyboard Audio Type, it is already loaded Audio Clips in Disctionary.
		/// </summary>
		/// <param name="iSource">Use Audio Source</param>
		/// <param name="iType">Keyboard Audio Type</param>
		/// <param name="iAudioPitch">Pitch Value Set</param>
		/// <param name="iIsLoop">Audio loop default is false, set true will loop play</param>
		private void PlayAudioEvent(AudioSource iSource, eKeyAudioType iType, float iAudioPitch, bool iIsLoop = false)
		{
			AudioClip aPlayTemp = imeAudioClipList[iType];
			iSource.loop = iIsLoop;
			iSource.pitch = iAudioPitch;
			iSource.clip = aPlayTemp;
			iSource.Play();
		}
		/// <summary>
		/// play Audio by Keyboard Audio Type, it is already loaded Audio Clips in Disctionary.
		/// </summary>
		/// <param name="iType">Keyboard Audio Type</param>
		/// <param name="iAudioPitch">Pitch Value Set</param>
		/// <param name="iIsLoop">Audio loop default is false, set true will loop play</param>
		private void PlayAudioEvent(eKeyAudioType iType, float iAudioPitch, bool iIsLoop = false)
		{
			PlayAudioEvent(sourceTool, iType, iAudioPitch, iIsLoop);
		}
		/// <summary>
		/// set Default AudioSource Value to Init ro Unity Awake.
		/// </summary>
		private void DefaultAudioSetting()
		{
			if(sourceTool == null)
				return;

			sourceTool.playOnAwake = false;
			sourceTool.loop = false;
			sourceTool.volume = defVolume;
			sourceTool.pitch = 1;
			sourceTool.Stop();
		}
		/// <summary>
		/// Stop Playing Audio And Reset About Long-Press Values.
		/// </summary>
		private void StopAudioAction()
		{
			if(sourceTool == null)
				return;
			
			sourceTool.loop = false;
			pressTime = 0f;
		}
		#endregion
		#region Button Event Status
		/// <summary>
		/// Button's enter/exit trigger event audio. Play by button type.
		/// </summary>
		/// <param name="iStatus"></param>
		/// <param name="iBtnType"></param>
		private void AudioEventButtonInOut(eIMEUIStatus iStatus, eButtonType iBtnType)
		{
			VRIMEKeyAudioInfo aTemp = null;
			// Get Data By Button Type
			switch(iBtnType)
			{
				case eButtonType.Letter:
					aTemp = CheckAccentShow();
					break;
				case eButtonType.Number:
				case eButtonType.LetterAccent:
				case eButtonType.WCLItemFunc:
					aTemp = audioInfoData.GetInfo(eAudioStatusType.small_character_key);
					break;
				case eButtonType.Space:
					aTemp = audioInfoData.GetInfo(eAudioStatusType.spacebar_character_key);
					break;
				case eButtonType.Symbol:
					aTemp = audioInfoData.GetInfo(eAudioStatusType.small_modifier_key);
					break;
				case eButtonType.Caps:
				case eButtonType.ToolTipPage:
					aTemp = audioInfoData.GetInfo(eAudioStatusType.large_modifier_key);
					break;
				case eButtonType.Submit:
					aTemp = audioInfoData.GetInfo(eAudioStatusType.return_enter_key);
					break;
				case eButtonType.Delete:
				case eButtonType.DeleteAll:
					aTemp = audioInfoData.GetInfo(eAudioStatusType.delete_backspace_single);
					break;
				case eButtonType.Language:
					aTemp = audioInfoData.GetInfo(eAudioStatusType.setting_language_key);
					break;
			}
			if(aTemp == null)
				return;
			
			ushort aHapticDura = 0;
			// Audio And Set Haptic-Duration
			if(iStatus == eIMEUIStatus.enter_show)
			{
				aHapticDura = aTemp.hapticEnter;
				PlayAudioEvent(aTemp.keyAudioType, aTemp.audioPitch);
			}
			else if(iStatus == eIMEUIStatus.exit_hide)
			{
				aHapticDura = aTemp.hapticExit;
				// Exit Stop Audio and BackDefault
				StopAudioAction();
			}
			// Haptic Event
			CallHapticEvent(aTemp.delayHaptic, aHapticDura);
		}
		/// <summary>
		/// Button Trigger Stay Event paly audio.(Loop Audio)
		/// </summary>
		/// <param name="iBtnType"></param>
		private void AudioEventButtonStay(eButtonType iBtnType)
		{
			// Get Data By Button Type
			switch(iBtnType)
			{
				case eButtonType.Delete:
					// DeleteLoop();
					break;
				default:
					break;

			}
			pressTime += Time.deltaTime;
		}
		/// <summary>
		/// When AccentPanel Show, Check ShowWord. If same to previous Play Audio Button, don't play audio again.
		/// </summary>
		/// <returns></returns>
		private VRIMEKeyAudioInfo CheckAccentShow()
		{
			VRIMEKeyAudioInfo aResult = audioInfoData.GetInfo(eAudioStatusType.small_character_key);
			// Like Button Check, match Audio Play.
			if(VRIME_KeyboardOversee.Ins.AccentShow)
			if(VRIME_KeyboardOversee.Ins.AccentShowWord == previousButton.Word)
			{
				if(previousButton.longPressStatus)
					aResult = null;
			}
			return aResult;
		}
		#endregion
	}
}