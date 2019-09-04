// ========================================================================== //
//  Created: 2019-06-27
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

    public class VRIME_PinYinSystem : VRIME_LanguageSys
    {
		#region override Field
        public override bool accentShow { get { return false; } }
        public override bool wordCandidateShow { get { return true; } }
        public override VRIME_Keymaps keymap { get { return VRIME_KeymapsPinyin.Instance; } }
		protected new int ChineseValueLimit = 7;
		private new int maxNearKeyChars = 6;

		public override int GetStartIndex(string iCheckWords, notMatchString iEvent)
		{
			int aResult = 0;
			string[] aSplitStrs = MakeStringArray(iCheckWords);
			for(int i = aResult; i < aSplitStrs.Length; i++)
			{
				bool aMatch = MatchKeyingEnglish(aSplitStrs[i]);
				if(aMatch){
					List<Words> aCheckList = CleanWordListEnglish(keymap.GetCandidates(aSplitStrs[i].ToLower()));
					if(aCheckList == null){
						aResult++;
						continue;
					}
					if(aCheckList.Count == 0){
						aResult++;
						continue;
					}
					break;
				}
				else {
					if(iEvent != null)
						iEvent(aSplitStrs[i]);
				}
			}
			return aResult;
		}

		public override List<Words> GetMaxCombinStrCandidates(string iInputWords, int iStartIndex)
		{
			string[] aNewStrs = GetStrCombination(iInputWords, iStartIndex);
			List<Words> aCheckList = null;
			List<Words> aResult = null;
			for(int i = 0; i < aNewStrs.Length; i++)
			{
				aCheckList = keymap.GetCandidates(aNewStrs[i].ToLower());
				if(aCheckList == null)
					break;
				if(aCheckList.Count == 0)
					break;
				if(i == 0)
					aResult = aCheckList;

				string aCheck = aCheckList[0].Value;
				bool aIsChinese = MatchKeyingChinese(aCheck);
				if(aIsChinese) {
					if(aCheck.Length > ChineseValueLimit)// Max Check Words by value limit
						break;
					aResult = aCheckList;
				}
				else {
					continue;
				}
			}
			if(aNewStrs.Length <= 0)
				return null;
			// Remove English Words
			return CleanWordListEnglish(aResult);
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
		#region private Function
		private List<Words> CleanWordListEnglish(List<Words> iWords)
		{
			List<Words> aResult = new List<Words>();
			for(int i = 0; i< iWords.Count; i++)
			{
				if(MatchKeyingEnglish(iWords[i].Value) == false)
				{
					Words aTemp = iWords[i];
					aTemp.Code = aTemp.Code.Replace(" ", string.Empty);
					aResult.Add(aTemp);
				}
					
			}
			return aResult;
		}
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
					bool aIsEnglish = MatchKeyingEnglish(aSplitStrs[k]);
					if(aIsEnglish)
						aTmpStr = aTmpStr.Insert(aTmpStr.Length, aSplitStrs[k]);
					else// Not English in Combo will error
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
		#endregion

    }
}