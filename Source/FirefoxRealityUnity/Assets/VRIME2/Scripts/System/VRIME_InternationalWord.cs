// ========================================================================== //
//  Created: 2019-05-15
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Htc.Omni;

	public class VRIME_InternationalWord
	{
		/// <summary>
		/// Function Key : Language Button
		/// Icon Using words
		/// </summary>
		/// <param name="iLanguage"></param>
		/// <returns></returns>
		public static string IMEIconWord(eLanguage iLanguage)
		{
			string aResult = string.Empty;
			switch(iLanguage)
			{
				case eLanguage.English: aResult = "EN"; break;
				case eLanguage.PinYin: aResult = "拼"; break;
				case eLanguage.Zhuyin: aResult = "注"; break;
				case eLanguage.French: aResult = "FR"; break;
				case eLanguage.Italian: aResult = "IT"; break;
				case eLanguage.German: aResult = "DE"; break;
				case eLanguage.Spanish: aResult = "ES"; break;
			}
			return aResult;
		}
		/// <summary>
		/// Function Key : Voice Input Button
		/// Icon Using words
		/// </summary>
		/// <param name="iVoiceLanguage"></param>
		/// <returns></returns>
		public static string VoiceIconWord(SupportLanguage iVoiceLanguage)
		{
			string aResult = string.Empty;
			switch(iVoiceLanguage)
			{
				case SupportLanguage.SimplifiedChinese:
					aResult = "简";
					break;
				case SupportLanguage.TraditionalChinese:
					aResult = "繁";
					break;            
				case SupportLanguage.English:
				default:
					aResult = "EN";
					break;
			}
			return aResult;
		}
		/// <summary>
		/// Language Wing Show IME Select Text
		/// </summary>
		/// <param name="iLang"></param>
		/// <returns></returns>
		public static string LanguageWingShowText(eLanguage iLang)
		{
			string aResult = string.Empty;
			switch(iLang)
			{
				case eLanguage.English:
					aResult = "English\n(US)";
					break;
				case eLanguage.PinYin:
					aResult = "中文\n(简体)";
					break;
				case eLanguage.Zhuyin:
					aResult = "中文\n(注音)";
					break;
			}
			return aResult;
		}
		/// <summary>
		/// Language Wing Show Voice Input Select Text
		/// </summary>
		/// <param name="iLang"></param>
		/// <returns></returns>
		public static string VoiceLangWingShowText(SupportLanguage iLang)
		{
			string aResult = string.Empty;
			switch(iLang)
			{
				case SupportLanguage.SimplifiedChinese:
					aResult = "普通話\n(中國)";
					break;
				case SupportLanguage.TraditionalChinese:
					aResult = "國語\n(台灣)";
					break;            
				case SupportLanguage.English:
				default:
					aResult = "English\n(US)";
					break;
			}
			return aResult;
		}
		/// <summary>
		/// Voice Input Hint Text
		/// </summary>
		/// <param name="iSupLanguage"></param>
		/// <returns></returns>
		public static string VoiceInpupHintText(SupportLanguage iSupLanguage)
		{
			string aResult = string.Empty;
			switch(iSupLanguage)
			{
				case SupportLanguage.SimplifiedChinese:
					aResult = "请说点什么...";
					break;
				case SupportLanguage.TraditionalChinese:
					aResult = "請說點什麼...";
					break;            
				case SupportLanguage.English:
				default:
					aResult = "Please say something...";
					break;
			}
			return aResult;
		}

		public static string InputPlaceholderText(eLanguage iLanguage)
		{
			switch(iLanguage)
			{
				case eLanguage.PinYin: return "输入文字...";
				case eLanguage.Zhuyin: return "輸入文字...";
				case eLanguage.French: return "Entrez du texte...";
				case eLanguage.Italian: return "Inserire il testo...";
				case eLanguage.German: return "Text eingeben...";
				case eLanguage.Spanish: return "Ingrese texto...";
				default: return "Enter text...";
			}
		}
	}
	public enum eLanguage
	{
		English = 0,
		PinYin,
		Zhuyin,
		French,
		Italian,
		German,
		Spanish,
	}
}

namespace Htc.Omni
{
	public enum SupportLanguage { English, TraditionalChinese, SimplifiedChinese }
}