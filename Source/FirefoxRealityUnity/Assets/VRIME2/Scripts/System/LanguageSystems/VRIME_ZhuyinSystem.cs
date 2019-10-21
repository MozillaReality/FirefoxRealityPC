// ========================================================================== //
//  Created: 2019-07-02
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

    public class VRIME_ZhuyinSystem : VRIME_LanguageSys
    {
        #region override Field
        public override bool accentShow { get { return false; } }
        public override bool wordCandidateShow { get { return true; } }
        public override VRIME_Keymaps keymap { get { return VRIME_KeymapsZhuyin.Instance; } }
		protected new int ChineseValueLimit = 7;
		private new int maxNearKeyChars = 3;
		public override int GetStartIndex(string iCheckWords, notMatchString iEvent)
		{
			int aResult = 0;
			string[] aSplitStrs = MakeStringArray(iCheckWords);
			// Check Have " " string handle
			for(int i = 0; i < aSplitStrs.Length; i++)
			{
				bool aIsRight = MatchKeyingZhuyin(aSplitStrs[i]);
				
				if(aIsRight == false)
				{
					if(iEvent != null)
						iEvent(aSplitStrs[i]);
				}
				else if(aSplitStrs[i] == " " || aSplitStrs[i] == VRIME_KeyboardData.cZhuYinOneTone.ToString())
				{
					int aCheckIndex = i - 1;
					if(aCheckIndex < 0)// If index is start do notMatchEvent
					{
						if(iEvent != null)
							iEvent(" ");
					}
					else
					{
						string aCheckWord = aSplitStrs[aCheckIndex];
						aIsRight = MatchKeyingZhuyin(aCheckWord);
						if(aIsRight == false)
						{
							if(iEvent != null)
								iEvent(aSplitStrs[i]);
						}
					}
				}
				else
					break;
			}
			return aResult;
		}
		public override List<Words> GetMaxCombinStrCandidates(string iInputWords, int iStartIndex)
		{
			string[] aNewStrs = GetStrCombination(GetRealSearchWords(iInputWords), iStartIndex);
			if(aNewStrs.Length <= 0)
				return null;
			List<Words> aResult = null;
			for(int i = aNewStrs.Length - 1; i >= 0; i--)
			{
				string aLastCombin = aNewStrs[i];
				if(aLastCombin.Length > ChineseValueLimit * 4)
					continue;
				aResult = keymap.GetCandidates(aNewStrs[i].ToLower());
				if(aResult == null)
					break;
				if(aResult.Count == 0)
					break;
				
				string aValue = aResult[0].Value;
				if(aValue.Length > ChineseValueLimit)
					continue;
				else
					break;
			}
			// Return get candidates result.
			return aResult;
		}

		public override List<string> GetFirstWordNearKeys(string iInputWords)
		{
			string nearKey = iInputWords;
			if(nearKey.Length > maxNearKeyChars) {
				nearKey = nearKey.Substring(0, maxNearKeyChars);
			}

			VRIME_Debugger.Log("GetNearKeys KeyWord :" + nearKey);
			return keymap.GetNearKeys(nearKey.ToLower());
		}
		#endregion
		#region private function
		/// <summary>
		/// Get Strings Combination
		/// ex: abcdefg
		/// restult: a, ab, abc, abcd, abcde, abcdef, abcdefg
		/// </summary>
		/// <param name="iText"></param>
		/// <returns></returns>
		private string[] GetStrCombination(string iText, int iStartIndex)
		{
			string[] aSplitStrs = MakeStringArray(iText);
			List<string> aResult = new List<string>();
			for(int i = iStartIndex; i < aSplitStrs.Length; i++)
			{
				string aTmpStr = string.Empty;
				for(int k = iStartIndex; k <= i; k++)
				{
					bool aIsRight = MatchKeyingZhuyin(aSplitStrs[k]);
					if(aIsRight)
						aTmpStr = aTmpStr.Insert(aTmpStr.Length, aSplitStrs[k]);
					else
					{
						aTmpStr = string.Empty;
						break;
					}
				}
				if(string.IsNullOrEmpty(aTmpStr) == false)
					aResult.Add(aTmpStr);
			}
			return aResult.ToArray();
		}
		private string GetRealSearchWords(string iInputWords)
		{
			return iInputWords.Replace(VRIME_KeyboardData.cZhuYinOneTone, ' ');
		}
		#endregion
    }
}