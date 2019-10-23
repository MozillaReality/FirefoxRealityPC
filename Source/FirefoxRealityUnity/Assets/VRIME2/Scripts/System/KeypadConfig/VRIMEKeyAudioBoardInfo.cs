// ========================================================================== //
//  Created: 2019-07-02
// ========================================================================== //
namespace VRIME2
{
	using System.Collections.Generic;
	
	public class VRIMEKeyAudioBoardInfo
	{
		public VRIMEKeyAudioBoardInfo() { Init(); }
		public VRIMEKeyAudioInfo[] AllAudioInfo { get { return mKeyboard.ToArray(); } }

		public VRIMEKeyAudioInfo GetInfo(eAudioStatusType iStatus)
		{
			VRIMEKeyAudioInfo aResult = null;
			for(int i = 0; i < mKeyboard.Count; i++)
			{
				if(mKeyboard[i].statusType == iStatus)
				{
					aResult = mKeyboard[i];
					break;
				}
			}
			return aResult;
		}
		protected List<VRIMEKeyAudioInfo> mKeyboard = new List<VRIMEKeyAudioInfo>();

		protected void Init()
		{
			globalSet();
			characterKeys();
			modifierKeys();
			actionAndDeleteKeys();
			functionKeys();
			customKeys();
			wordCandidateListKeys();
		}

		private void globalSet()
		{
			// Long press, expand menu
			VRIMEKeyAudioInfo aDataTemp = new VRIMEKeyAudioInfo(eAudioStatusType.expand_menu_key, eKeyAudioType.VRIME_SpecialKeyDown);
			aDataTemp.delayHaptic = 0f;
			aDataTemp.audioPitch = 1.4f;
			aDataTemp.hapticEnter = 2500;
			aDataTemp.hapticExit = 0;
			aDataTemp.isLongPress = true;
			mKeyboard.Add(aDataTemp);
			// Long press, select and dismiss
			aDataTemp = new VRIMEKeyAudioInfo(eAudioStatusType.select_dismiss_key, eKeyAudioType.VRIME_CommonKeyDown);
			aDataTemp.delayHaptic = 0.14f;
			aDataTemp.audioPitch = 1.7f;
			aDataTemp.hapticEnter = 0;
			aDataTemp.hapticExit = 550;
			aDataTemp.isLongPress = true;
			mKeyboard.Add(aDataTemp);
		}

		private void characterKeys()
		{
			VRIMEKeyAudioInfo aDataTemp = new VRIMEKeyAudioInfo(eAudioStatusType.small_character_key, eKeyAudioType.VRIME_CommonKeyDown);
			aDataTemp.delayHaptic = 0.14f;
			aDataTemp.audioPitch = 1.7f;
			aDataTemp.hapticEnter = 2500;
			aDataTemp.hapticExit = 550;
			aDataTemp.isLongPress = false;
			mKeyboard.Add(aDataTemp);

			aDataTemp = new VRIMEKeyAudioInfo(eAudioStatusType.spacebar_character_key, eKeyAudioType.VRIME_CommonKeyDown);
			aDataTemp.delayHaptic = 0.14f;
			aDataTemp.audioPitch = 1.4f;
			aDataTemp.hapticEnter = 2500;
			aDataTemp.hapticExit = 550;
			aDataTemp.isLongPress = false;
			mKeyboard.Add(aDataTemp);
		}

		private void modifierKeys()
		{
			// like symbol, or emoji key
			VRIMEKeyAudioInfo aDataTemp = new VRIMEKeyAudioInfo(eAudioStatusType.small_modifier_key, eKeyAudioType.VRIME_SpecialKeyDown);
			aDataTemp.delayHaptic = 0f;
			aDataTemp.audioPitch = 1.4f;
			aDataTemp.hapticEnter = 2500;
			aDataTemp.hapticExit = 550;
			aDataTemp.isLongPress = false;
			mKeyboard.Add(aDataTemp);
			// shift key, like caps
			aDataTemp = new VRIMEKeyAudioInfo(eAudioStatusType.large_modifier_key, eKeyAudioType.VRIME_CommonKeyDown);
			aDataTemp.delayHaptic = 0f;
			aDataTemp.audioPitch = 1.4f;
			aDataTemp.hapticEnter = 2500;
			aDataTemp.hapticExit = 550;
			aDataTemp.isLongPress = false;
			mKeyboard.Add(aDataTemp);
		}

		private void actionAndDeleteKeys()
		{
			VRIMEKeyAudioInfo aDataTemp = new VRIMEKeyAudioInfo(eAudioStatusType.return_enter_key, eKeyAudioType.VRIME_CommonKeyDown);
			aDataTemp.delayHaptic = 0.14f;
			aDataTemp.audioPitch = 1.4f;
			aDataTemp.hapticEnter = 2500;
			aDataTemp.hapticExit = 550;
			aDataTemp.isLongPress = false;
			mKeyboard.Add(aDataTemp);

			aDataTemp = new VRIMEKeyAudioInfo(eAudioStatusType.delete_backspace_single, eKeyAudioType.VRIME_BackspaceSINGLE);
			aDataTemp.delayHaptic = 0.14f;
			aDataTemp.audioPitch = 2f;
			aDataTemp.hapticEnter = 2500;
			aDataTemp.hapticExit = 550;
			aDataTemp.isLongPress = false;
			mKeyboard.Add(aDataTemp);

			aDataTemp = new VRIMEKeyAudioInfo(eAudioStatusType.delete_backspace_repeat, eKeyAudioType.VRIME_BackspaceLONGPRESS_Single);
			aDataTemp.delayHaptic = 0f;// N/A
			aDataTemp.audioPitch = 2f;// N/A, so this Value same to delete_backspace_single
			aDataTemp.hapticEnter = 0;// N/A
			aDataTemp.hapticExit = 0;// N/A
			aDataTemp.isLongPress = true;
			mKeyboard.Add(aDataTemp);
		}

		private void functionKeys()
		{
			// Language Function Button
			VRIMEKeyAudioInfo aDataTemp = new VRIMEKeyAudioInfo(eAudioStatusType.setting_language_key, eKeyAudioType.VRIME_CommonKeyDown);
			aDataTemp.delayHaptic = 0.14f;
			aDataTemp.audioPitch = 0.8f;
			aDataTemp.hapticEnter = 2500;
			aDataTemp.hapticExit = 550;
			aDataTemp.isLongPress = false;
			mKeyboard.Add(aDataTemp);
			// maybe language wing button 
			aDataTemp = new VRIMEKeyAudioInfo(eAudioStatusType.individual_language_key, eKeyAudioType.VRIME_CommonKeyDown);
			aDataTemp.delayHaptic = 0.14f;
			aDataTemp.audioPitch = 1.7f;
			aDataTemp.hapticEnter = 2500;
			aDataTemp.hapticExit = 550;
			aDataTemp.isLongPress = false;
			mKeyboard.Add(aDataTemp);
			// Voice Input
			aDataTemp = new VRIMEKeyAudioInfo(eAudioStatusType.voice_dictation_start, eKeyAudioType.VRIME_DictationON);
			aDataTemp.delayHaptic = 0.14f;
			aDataTemp.audioPitch = 1f;
			aDataTemp.hapticEnter = 2500;
			aDataTemp.hapticExit = 0;// N/A (key stays down)
			aDataTemp.isLongPress = false;
			mKeyboard.Add(aDataTemp);

			aDataTemp = new VRIMEKeyAudioInfo(eAudioStatusType.voice_dictation_stop, eKeyAudioType.VRIME_DictationOFF);
			aDataTemp.delayHaptic = 0f;
			aDataTemp.audioPitch = 1f;
			aDataTemp.hapticEnter = 0;// N/A (key goes up on auto)
			aDataTemp.hapticExit = 550;
			aDataTemp.isLongPress = false;
			mKeyboard.Add(aDataTemp);

			aDataTemp = new VRIMEKeyAudioInfo(eAudioStatusType.dissmiss_exit_key, eKeyAudioType.VRIME_PanelBuildOUT);
			aDataTemp.delayHaptic = 0f;
			aDataTemp.audioPitch = 1f;
			aDataTemp.hapticEnter = 0;// N/A
			aDataTemp.hapticExit = 0;// N/A
			aDataTemp.isLongPress = false;
			aDataTemp.sequenceInfo = new VRIMEHapticSquence[] {
				new VRIMEHapticSquence(0f, 2500),
				new VRIMEHapticSquence(0.05f, 500),
				new VRIMEHapticSquence(0.05f, 350)
			};
			mKeyboard.Add(aDataTemp);

			aDataTemp = new VRIMEKeyAudioInfo(eAudioStatusType.appear_enter_key, eKeyAudioType.VRIME_KeyboardBuildIN);
			aDataTemp.delayHaptic = 0f;
			aDataTemp.audioPitch = 1f;
			aDataTemp.hapticEnter = 0;// N/A
			aDataTemp.hapticExit = 0;// N/A
			aDataTemp.isLongPress = false;
			mKeyboard.Add(aDataTemp);
		}

		private void customKeys()
		{
			// tp = third-party
			VRIMEKeyAudioInfo aDataTemp = new VRIMEKeyAudioInfo(eAudioStatusType.tp_custom_key, eKeyAudioType.VRIME_SpecialKeyDown);
			aDataTemp.delayHaptic = 0f;
			aDataTemp.audioPitch = 1f;
			aDataTemp.hapticEnter = 4500;
			aDataTemp.hapticExit = 550;
			aDataTemp.isLongPress = false;
			mKeyboard.Add(aDataTemp);
		}

		private void wordCandidateListKeys()
		{
			// Word candidate (predicted word)
			VRIMEKeyAudioInfo aDataTemp = new VRIMEKeyAudioInfo(eAudioStatusType.word_candidate_key, eKeyAudioType.VRIME_SpecialKeyDown);
			aDataTemp.delayHaptic = 0f;
			aDataTemp.audioPitch = 1f;
			aDataTemp.hapticEnter = 0;// N/A
			aDataTemp.hapticExit = 0;// N/A
			aDataTemp.isLongPress = false;
			aDataTemp.sequenceInfo = new VRIMEHapticSquence[] {
				new VRIMEHapticSquence(0f, 350),
				new VRIMEHapticSquence(0.1f, 2500)
			};
			mKeyboard.Add(aDataTemp);
			// Expand candidate list
			aDataTemp = new VRIMEKeyAudioInfo(eAudioStatusType.expand_candidate_key, eKeyAudioType.VRIME_PanelBuildIN);
			aDataTemp.delayHaptic = 0f;
			aDataTemp.audioPitch = 1f;
			aDataTemp.hapticEnter = 0;// N/A
			aDataTemp.hapticExit = 0;// N/A
			aDataTemp.isLongPress = false;
			aDataTemp.sequenceInfo = new VRIMEHapticSquence[] {
				new VRIMEHapticSquence(0f, 350),
				new VRIMEHapticSquence(0.1f, 2500)
			};
			mKeyboard.Add(aDataTemp);
			// Dismiss candidate list
			aDataTemp = new VRIMEKeyAudioInfo(eAudioStatusType.dissmiss_candidate_key, eKeyAudioType.VRIME_PanelBuildOUT);
			aDataTemp.delayHaptic = 0f;
			aDataTemp.audioPitch = 1f;
			aDataTemp.hapticEnter = 0;// N/A
			aDataTemp.hapticExit = 0;// N/A
			aDataTemp.isLongPress = false;
			aDataTemp.sequenceInfo = new VRIMEHapticSquence[] {
				new VRIMEHapticSquence(0f, 350),
				new VRIMEHapticSquence(0.1f, 2500)
			};
			mKeyboard.Add(aDataTemp);
		}
	}
	public class VRIMEKeyAudioInfo
	{
		// microsecond = 0.000001
		public VRIMEKeyAudioInfo(eAudioStatusType iStatus, eKeyAudioType iAudio)
		{
			statusType = iStatus;
			keyAudioType = iAudio;
		}
		public eAudioStatusType statusType;
		public eKeyAudioType keyAudioType;
		public float delayHaptic;// between sound and haptic response, microsecond
		public float audioPitch;
		public ushort hapticEnter;// 0 = No Haptic, microsecond
		public ushort hapticExit;// 0 = No Haptic, microsecond
		/// <summary>
		/// it Will delay > duration combo
		/// </summary>
		public VRIMEHapticSquence[] sequenceInfo;// float, microsecond
		public bool isLongPress;
	}
	/// <summary>
	/// VRIMEKeyAudioInfo Use
	/// </summary>
	public struct VRIMEHapticSquence
	{
		public float delayTime;
		public ushort durationTime;
		public VRIMEHapticSquence (float iDelay, ushort iDuration)
		{
			delayTime = iDelay;
			durationTime = iDuration;
		}
	}
}