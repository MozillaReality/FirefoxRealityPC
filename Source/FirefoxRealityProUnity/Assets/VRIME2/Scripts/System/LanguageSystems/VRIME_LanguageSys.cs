// ========================================================================== //
//  Created: 2019-06-26
// ========================================================================== //
namespace VRIME2
{
	using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public delegate void notMatchString(string iStr);
	public interface ILanguageSystem
	{
		/// <summary>
		/// Get input words where start
		/// </summary>
		/// <param name="iCheckWords"></param>
		/// <param name="iEvent"></param>
		/// <returns></returns>
		int GetStartIndex(string iCheckWords, notMatchString iEvent);
		/// <summary>
		/// Search and try to get longest candidate words.
		/// </summary>
		/// <returns></returns>
		List<Words> GetMaxCombinStrCandidates(string iInputWords, int iStartIndex);
		/// <summary>
		/// Use first key to search possible near key.
		/// </summary>
		/// <returns></returns>
		List<string> GetFirstWordNearKeys(string iInputWords);
	}

	public abstract class VRIME_LanguageSys : ILanguageSystem
	{
		#region Inheritable Method
		abstract public bool accentShow { get; }
		abstract public bool wordCandidateShow { get; }
		abstract public VRIME_Keymaps keymap { get; }
        public virtual int GetStartIndex(string iCheckWords, notMatchString iEvent)
		{
			throw new System.NotImplementedException();
		}
		public virtual List<Words> GetMaxCombinStrCandidates(string iInputWords, int iStartIndex)
		{
			throw new System.NotImplementedException();
		}
		public virtual List<string> GetFirstWordNearKeys(string iInputWords)
        {
            throw new System.NotImplementedException();
        }
		#endregion
		#region Value Field
		protected int ChineseValueLimit;
		protected int maxNearKeyChars;
		#endregion
		#region Other Method
		protected bool MatchKeyingEnglish(string iText) { return Regex.IsMatch(iText, @"^[A-Za-z]+$"); }
		// Unicode usually common chinese.
		protected bool MatchKeyingChinese(string iText) { return Regex.IsMatch(iText, @"^[\u4e00-\u9fa5]+$"); }
		// Unicode zhuyin and four tone, one space
		protected bool MatchKeyingZhuyin(string iText) { return Regex.IsMatch(iText, @"^[\u3105-\u3129\u02CA\u02C7\u02CB\u02D9\u0020\u02C9]+$"); }
		protected string[] MakeStringArray(string iInsert)
		{
			List<string> aMakeList = new List<string>();
			for(int i = 0; i < iInsert.Length; i++)
			{
				aMakeList.Add(iInsert.Substring(i, 1));
			}

			return aMakeList.ToArray();
		}
        #endregion
		#region public Function
		/// <summary>
		/// Get layout class by setting running language.
		/// </summary>
		/// <param name="iSymbolMode"></param>
		/// <returns></returns>
		public VRIME_KeypadLayout GetLayoutData(eLanguage iUseLanugage, bool iSymbolMode = false)
		{
			string aLayoutName = string.Empty;
			if(iSymbolMode)
			{
				aLayoutName = "symbol";
			}
			else
			{
				switch(iUseLanugage)
				{
					case eLanguage.Zhuyin: aLayoutName = "zhuyin"; break;
					default: aLayoutName = "qwerty"; break;
				}
			}
			string aLayoutJson = VRIME_AssetLoad.LoadKeypadConfig(aLayoutName, eConfigFolder.KeypadLayout);
			VRIME_KeypadLayout aResult = JsonUtility.FromJson<VRIME_KeypadLayout>(aLayoutJson);

			return aResult;
		}
		/// <summary>
		/// Get config class by setting running language.
		/// </summary>
		/// <param name="iSymbolMode"></param>
		/// <returns></returns>
		public VRIME_KeypadConfig GetConfigData(eLanguage iUseLanugage, bool iSymbolMode = false)
		{
			string aConfigName = string.Empty;
			
			if(iSymbolMode)
			{
				switch(iUseLanugage)
				{
					case eLanguage.Zhuyin: aConfigName = "symbol_zhuyin"; break;
					default: aConfigName = "symbol"; break;
				}
			}
			else
			{
				switch(iUseLanugage)
				{
					case eLanguage.PinYin: aConfigName = "english"; break;
					default: aConfigName = iUseLanugage.ToString().ToLower(); break;
				}
			}
			string aConfigJson = VRIME_AssetLoad.LoadKeypadConfig(aConfigName, eConfigFolder.KeypadConfig);
			VRIME_KeypadConfig aResult = JsonUtility.FromJson<VRIME_KeypadConfig>(aConfigJson);

			return aResult;
		}
		#endregion
    }
}