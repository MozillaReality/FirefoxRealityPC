// ========================================================================== //
//  Created: 2019-01-11
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	public class VRIME_KeyboardData
	{
		public const string cLanTipName = "LanTip";
		public const char cZhuYinOneTone = '\u02C9';
		public static VRIME_KeyboardButton CapsButton;
		public static VRIME_KeyboardButton WCLExtendButton;


		private static string[] skipInsertWords = new string[]{".com"};
		public static Transform GetChildInclude(Transform iParent, string iChildName)
		{
			Transform aResult = null;
			Transform[] aChilds = iParent.GetComponentsInChildren<Transform>(true);
			foreach(Transform eChild in aChilds)
			{
				if(eChild.name == iChildName){
					aResult = eChild;
					break;
				}
			}
			return aResult;
		}
		/// <summary>
		/// Search Need Skip Action Words
		/// </summary>
		/// <param name="iWord"></param>
		/// <returns></returns>
		public static bool CheckNeedSkipInsert(string iWord)
		{
			bool aResult = false;
			for(int i = 0; i < skipInsertWords.Length; i++)
			{
				if(iWord == skipInsertWords[i]) {
					aResult = true;
					break;
				}
			}
			return aResult;
		}
		/// <summary>
		/// Get Vive Desing Setting Color
		/// </summary>
		/// <param name="iType"></param>
		/// <returns></returns>
		public static Color GetViveColors(eViveColor iType)
		{
			Color aResult = new Color();

			string aColorCode = "#";
			switch(iType)
			{
				case eViveColor.NormalBlack: aColorCode += "4B596CFF"; break;
				case eViveColor.PressedBlue: aColorCode += "009FCCFF"; break;
			}
			ColorUtility.TryParseHtmlString(aColorCode, out aResult);
			return aResult;
		}
	}
	public enum eButtonType
	{
		None,
		Letter,
		LetterAccent,
		LetterWCL,
		Delete,
		Submit,
		Caps,
		Symbol,
		Voice,
		Setting,
		Language,
		ArrowLeft,
		ArrowRight,
		ArrowUp,
		ArrowDown,
		Close,
		Emoji,
		Space,
		DeleteAll,
		Number,
		ResetBoard,
		WCLExtend,
		LanSelect,
		ToolTipPage,
		WCLItemFunc,
		WCLNearKey,
	}

	public enum eUIType
	{
		None,
		Text,
		Image,
		TMPro
	}

	public enum eAnimBtn
	{
		Normal,
		Highlighted,
		Pressed,
		Released
	}

	public enum eAnimAccent
	{
		Normal,
		Opened,
		Closed,
	}

	public enum eAnimIME
	{
		Normal,
		Summon,
		Dismiss,
		Disabled,
	}

	public enum eAnimWCL
	{
		WCLStates_Normal, // Close Status
		WCLStates_Opened, // open one line status
		WCLStates_Extended, // one line to multiple line
		WCLStates_Closed, // one line close
		WCLStates_AllClosed,// multiple line close
		WCL_ExtendedClosed, // multiple line to one line
		WCLStates_VoiceClosed,// voice input close
		WCLStates_VoiceOpened,// voice input open(include VoiceVisualizer)
		WCLStates_VoiceOpened_Start,// voice input hint text
	}
	public enum eAnimeWCLExtendBtn
	{
		ToggleTransition,
		ToggleTransitionBack,
	}
	public enum eKeyBoardLayoutType
	{
		Number,
		English,
	}

	public enum eKeyboardPadSet
	{
		PanelMesh,
		LanguageWing,
		CharacterAccents,
		Tooltip
	}

	public enum eCapsState
	{
		Lower = 0,
		UpperUnLock,
		UpperLock
	}

	public enum eKeyboardStage
	{
		Normal = 0,
		Symbol,
		Zhuyin
	}
	public enum ePanelMeshNames
	{
		FunctionButtons,
		StandardKeys,
		CustomKeys
	}

	public enum eWCLRows
	{
		WCL_Row_1 = 0,
		WCL_Row_2,
		WCL_Row_3,
		WCL_Row_4,
		WCL_Row_5,
	}

	public enum eWCLFunction
	{
		Extend,
		ItemUp,
		ItemDown,
		ShowPage
	}
	public enum eToolTipPage
	{
		State_1 = 0,
		State_2
	}

	public enum eVoiceEngine
	{
		None,
		google,
		baidu
	}

	public enum eViveColor
	{
		NormalBlack,
		PressedBlue,
	}
	public enum eKeyAudioType
	{
		VRIME_BackspaceLONGPRESS_Single,
		VRIME_BackspaceSINGLE,
		VRIME_CommonKeyDown,
		VRIME_DictationOFF,
		VRIME_DictationON,
		VRIME_PanelBuildIN,
		VRIME_PanelBuildOUT,
		VRIME_SpecialKeyDown,
		VRIME_KeyboardBuildIN,
	}

	public enum eAudioStatusType
	{
		expand_menu_key,
		select_dismiss_key,
		small_character_key,
		spacebar_character_key,
		small_modifier_key,
		large_modifier_key,
		return_enter_key,
		delete_backspace_single,
		delete_backspace_repeat,
		setting_language_key,
		individual_language_key,
		voice_dictation_start,
		voice_dictation_stop,
		dissmiss_exit_key,
		tp_custom_key,// td = third party
		word_candidate_key,
		expand_candidate_key,
		dissmiss_candidate_key,
		appear_enter_key,
	}
	public enum eIMEUIStatus
	{
		enter_show,
		stay_longpress,
		exit_hide,
	}
	public enum eStickType
	{
		left = 0,
		right,
		twohand,
	}
	public enum eConfigFolder
	{
		KeypadLayout,
		KeypadConfig,
		KeypadLayoutData,
	}
}