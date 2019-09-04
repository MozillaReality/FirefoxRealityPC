// ========================================================================== //
//  Created: 2019-01-17
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public class VRIME_KeyboardSetting
	{
		public const string SDKVersion = "2.3.0b";
        public static bool EnableControllerVibration = true;
        public static bool EnableKeyboraButtondAnimation = true;
		public static bool EnableKeyboardButtonAudio = true;
        public static bool BlockDrumstick = false;
		public static bool TooltipHaveSeen {
			get {
				int aTipSave = PlayerPrefs.GetInt("Tooltip", 0);
				// Debug.Log("aTipSave = " + aTipSave);
				return aTipSave == 1;
				// return true;
			}
			set {
				bool aTmp = value;
				int aTipSave = aTmp ? 1 : 0;
				PlayerPrefs.SetInt("Tooltip", aTipSave);
				PlayerPrefs.Save();
				// Debug.Log("aTipSave = " + aTipSave);
			}
		}
		// New Settings
		public static bool VoiceInputBlock = false;// Handle Other Key In Voice Input Work
		public static eKeyBoardLayoutType UsingKeyboard = eKeyBoardLayoutType.English;
		#region Language Settings
		public static eLanguage IMELanguage = eLanguage.English;
		public static eLanguage[] LanguagePool;
		#endregion
	}
}