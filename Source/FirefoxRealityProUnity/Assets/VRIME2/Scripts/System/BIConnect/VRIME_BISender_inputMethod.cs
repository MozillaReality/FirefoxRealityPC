// ========================================================================== //
//  Created: 2019-04-25
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public partial class VRIME_BISender
	{
		#region Const
		private const string CATEGORY_INPUT_METHOD = "input_method_usage";
		private const string INPUT_NONE = "none";
		private const string INPUT_EV_SUBMIT = "submit";
		private const string INPUT_USING_ENGINE = "htc";
		private const string INPUT_METHOD_VOICE = "voice";
		private const string INPUT_METHOD_TYPING = "typing";
		private const string INPUT_PINYIN_CHANGE = "pinyin_change";
		#endregion
		#region Input Mtehod Info
		// Input
		private List<VRIME_BIInputChar> mLabelCharList;
		private List<VRIME_BIInputChar> mPinYinCharList;
		private List<VRIME_BIInputChar> mDeleteCharList;
		// Send Result
		private List<VIRME_BIInputMethodTable> mInputMethodList;
		private string mUsingSuatus; // Type / Voice
		private string mUsingEngine;// Keyboard / Voice_[engine name]
		private string mUsingLanguage;// engine use language
		private System.DateTime mLastInputTime;
		#endregion
		#region public Function
		/// <summary>
		/// Normal input function, include voice input
		/// </summary>
		/// <param name="iText"></param>
		public void LabelInsert(string iText, int iInsertIndex)
		{
			char[] aWords = iText.ToCharArray();
			InsertToCharList(aWords, iInsertIndex, ref mLabelCharList);
			// VRIME_Debugger.Log("mLabelCharList Count :" + mLabelCharList.Count);
		}
		/// <summary>
		/// PinYin use input list.
		/// </summary>
		/// <param name="iText"></param>
		public void PinYinInsert(string iText, int iInsertIndex)
		{
			char[] aWords = iText.ToCharArray();
			InsertToCharList(aWords, iInsertIndex, ref mPinYinCharList);
		}
		/// <summary>
		/// Already in inputfield words, there not be count.
		/// But need add list to match inputfield status.
		/// </summary>
		/// <param name="iText"></param>
		public void DefaultInsert(string iText)
		{
			string aStatus = mUsingSuatus;
			string aEngine = mUsingEngine;
			string aLanguage = mUsingLanguage;
			mUsingEngine = INPUT_NONE;
			mUsingLanguage = INPUT_NONE;
			mUsingSuatus = INPUT_NONE;
			LabelInsert(iText, 0);
			mUsingEngine = aEngine;
			mUsingLanguage = aLanguage;
			mUsingSuatus = aStatus;
		}
		/// <summary>
		/// Cancel compose update lable list and pinyin list work.
		/// </summary>
		/// <param name="iSaveString"></param>
		public void CancelCompose(int iInsertIndex, string iSaveString)
		{
			// Update label list
			LabelInsert(iSaveString, iInsertIndex);
			// Re Add Already Keying duration.
			char[] aWords = iSaveString.ToCharArray();
			for(int i =0; i < aWords.Length; i++)
			{
				VRIME_BIInputChar aInsertChar = mLabelCharList[i + iInsertIndex];
				if(i < mPinYinCharList.Count)
				{
					VRIME_BIInputChar aChar = mPinYinCharList[i];
					if(aChar.inputWord == aInsertChar.inputWord)
					{
						aInsertChar.duration += aChar.duration;
						mLabelCharList[i + iInsertIndex] = aInsertChar;
					}
				}
			}
			// Clean PinYin List
			CleanInputCharList(ref mPinYinCharList);
		}
		/// <summary>
		/// Delete all button, count all delete word and clean list.
		/// </summary>
		public void LabelDeleteAll()
		{
			LabelListDelete(0, mLabelCharList.Count);
			CleanInputCharList(ref mLabelCharList);
			PinYinListDelete(0, mPinYinCharList.Count);
			CleanInputCharList(ref mPinYinCharList);
		}
		/// <summary>
		/// Inputfield list delete count.
		/// </summary>
		/// <param name="iStartIndex"></param>
		/// <param name="iCount"></param>
		public void LabelListDelete(int iStartIndex, int iCount)
		{
			DeleteToCharList(iStartIndex, iCount, ref mLabelCharList);
		}
		/// <summary>
		/// PinYin list delete count.
		/// </summary>
		/// <param name="iStartIndex"></param>
		/// <param name="iCount"></param>
		public void PinYinListDelete(int iStartIndex, int iCount)
		{
			DeleteToCharList(iStartIndex, iCount, ref mPinYinCharList);
		}
		/// <summary>
		/// PinYin Word Change Simplified Chinese Word Recored
		/// </summary>
		/// <param name="iStartIndex"></param>
		/// <param name="iCount"></param>
		/// <param name="iChangeWord"></param>
		public void PinYinChangeWord(int iStartIndex, int iCount, string iChangeWord, int iInsertIndex)
		{
			// Recored Duration
			for(int i = iStartIndex; i < iCount; i++)
			{
				if(i < mPinYinCharList.Count == false)
					break;
				
				VRIME_BIInputChar aChar = mPinYinCharList[i];
				SaveCahrToTable(aChar, true);
			}			
			// Insert ChangeWord To mLabelCharList
			char[] aChWords = iChangeWord.ToCharArray();
			InsertToCharList(aChWords, iInsertIndex, ref mLabelCharList);
			// set Change Words Is None Engine && language type, skip count use.
			for(int i = 0; i < aChWords.Length; i++)
			{
				if(i + iInsertIndex < mLabelCharList.Count == false)
					break;
				
				int aIndex = i + iInsertIndex;
				VRIME_BIInputChar aFirstChar = mLabelCharList[aIndex];
				aFirstChar.language = INPUT_PINYIN_CHANGE;
				mLabelCharList[aIndex] = aFirstChar;
			}
			// Remove Word List
			mPinYinCharList.RemoveRange(iStartIndex, iCount);
		}
		/// <summary>
		/// When WCL can't find the matching text, it will directly input the text into InputField(InputTemp).
		/// In BILogger is mPinYinCharList to mLabelCharList.
		/// </summary>
		/// <param name="iStartIndex"></param>
		/// <param name="iWords"></param>
		public void PinYinInsertNotMatchWords(int iStartIndex, string iWords)
		{
			char[] aChWords = iWords.ToCharArray();
			for(int i = 0; i < aChWords.Length; i++)
			{
				VRIME_BIInputChar aChar = mPinYinCharList[i];
				mLabelCharList.Insert(iStartIndex + i, aChar);
			}
			mPinYinCharList.RemoveRange(0, aChWords.Length);
		}
		/// <summary>
		/// PinYin click near-key to change cahr duration.
		/// </summary>
		/// <param name="iStartIndex"></param>
		/// <param name="iNewChar"></param>
		public void PinYinNearKeyChange(int iStartIndex, string iNewChar)
		{
			System.DateTime aTime = System.DateTime.Now;
			System.TimeSpan aSaveDuration =  aTime - mLastInputTime;
			// Start Change
			char[] aCharArray = iNewChar.ToCharArray();
			for(int i = 0; i < aCharArray.Length; i++)
			{
				int aIndex = iStartIndex + i;
				if(aIndex >= mPinYinCharList.Count)
					break;

				VRIME_BIInputChar aTemp = mPinYinCharList[aIndex];
				aTemp.inputWord = aCharArray[i];
				if(i == 0)// Because duration Is Hapen to First Word
					aTemp.duration = aSaveDuration.TotalMilliseconds;
				
				mPinYinCharList[aIndex] = aTemp;
			}
			// New Last Input Millisecond
			mLastInputTime = aTime;
		}
		/// <summary>
		/// Count all recored data
		/// </summary>
		public void SaveAllCharToTable()
		{
			// Input Label Count
			foreach(VRIME_BIInputChar eChar in mLabelCharList)
			{
				SaveCahrToTable(eChar, true);
			}
			// Clean Data
			CleanInputCharList(ref mLabelCharList);
			// Delete Label Count
			foreach(VRIME_BIInputChar eChar in mDeleteCharList)
			{
				SaveCahrToTable(eChar, false);
			}
			// Clean Data
			CleanInputCharList(ref mDeleteCharList);
			// ReCount PinYin
			CountPinYinInputFinalCount();
		}
		#endregion
		#region private Function
		/// <summary>
		/// Init Function
		/// </summary>
		private void InputMethodInit()
		{
			mLabelCharList = new List<VRIME_BIInputChar>();
			mPinYinCharList = new List<VRIME_BIInputChar>();
			mDeleteCharList = new List<VRIME_BIInputChar>();
			mInputMethodList = new List<VIRME_BIInputMethodTable>();
		}
		/// <summary>
		/// Keybaord show/hide working.
		/// show: set using engine and first duration start time.
		/// hide: count and send recored data.
		/// </summary>
		/// <param name="iShow"></param>
		private void KeybaordDefaultInputData(bool iShow)
		{
			if(iShow)
			{
				UpdateEngineInfo();
			}
			else
			{
				SendInputMethod();
			}
		}
		/// <summary>
		/// Update keyboard using data.
		/// </summary>
		public void UpdateEngineInfo()
		{
			// Check
			if(VRIME_VoiceOversee.Ins.inputStatus)
			{
				mUsingSuatus = INPUT_METHOD_VOICE;
				mUsingEngine = VRIME_VoiceOversee.Ins.inputEngine.ToString();
				mUsingLanguage = VRIME_VoiceOversee.Ins.InputLanguage.ToString();
			}
			else
			{
				mUsingSuatus = INPUT_METHOD_TYPING;
				mUsingEngine = INPUT_USING_ENGINE;
				mUsingLanguage = GetIMELanguageText(VRIME_KeyboardSetting.IMELanguage);
			}
			// First Millisecond
			mLastInputTime = System.DateTime.Now;
		}
		/// <summary>
		/// Insert words to VRIME_BIInputChar list.
		/// </summary>
		/// <param name="iWords"></param>
		/// <param name="iUpdateList"></param>
		private void InsertToCharList(char[] iWords, int iStartIndex, ref List<VRIME_BIInputChar> iUpdateList)
		{
			System.DateTime aTime = System.DateTime.Now;
			System.TimeSpan aSaveDuration =  aTime - mLastInputTime;
			// Save to List
			for(int i = 0; i < iWords.Length; i++)
			{
				char aRecWord = iWords[i];
				VRIME_BIInputChar aBIChar = new VRIME_BIInputChar();
				aBIChar.insertType = mUsingSuatus;
				aBIChar.biEngine = mUsingEngine;
				aBIChar.language = mUsingLanguage;
				aBIChar.inputWord = aRecWord;
				if(i == 0)// Because duration Is Hapen to First Word
					aBIChar.duration = aSaveDuration.TotalMilliseconds;
				iUpdateList.Insert(iStartIndex + i, aBIChar);
			}
			// New Last Input Millisecond
			mLastInputTime = aTime;
		}
		/// <summary>
		/// Delete data recored.
		/// </summary>
		/// <param name="iStartIndex"></param>
		/// <param name="iCount"></param>
		/// <param name="iDeleteList"></param>
		private void DeleteToCharList(int iStartIndex, int iCount, ref List<VRIME_BIInputChar> iDeleteList)
		{
			System.DateTime aTime = System.DateTime.Now;
			System.TimeSpan aSaveDuration =  aTime - mLastInputTime;
			// Save Data To Delete Info
			for(int i = 0; i < iCount; i++)
			{
				int aIndex = iStartIndex + i;
				if(aIndex >= iDeleteList.Count)
					break;

				VRIME_BIInputChar aBIChar = iDeleteList[aIndex];
				aBIChar.index = aIndex;
				if(i == 0)// Because duration Is Hapen to First Word
					aBIChar.duration = aSaveDuration.TotalMilliseconds;
				else
					aBIChar.duration = 0;
				mDeleteCharList.Add(aBIChar);
			}
			// Remove Data By Range
			iDeleteList.RemoveRange(iStartIndex, iCount);
			// New Last Input Millisecond
			mLastInputTime = aTime;
		}		
		/// <summary>
		/// PinYin result word not count in table, but delete count and delete duration will be count.
		/// Although this will increase the difficulty of verification, it is in line with the rules of PM.
		/// </summary>
		private void CountPinYinInputFinalCount()
		{
			VIRME_BIInputMethodTable aTempChangeWord = GetMethodTable(INPUT_USING_ENGINE, INPUT_PINYIN_CHANGE);
			VIRME_BIInputMethodTable aTempPinYin = GetMethodTable(INPUT_USING_ENGINE, eLanguage.PinYin.ToString());
			aTempPinYin.deleteCount += aTempChangeWord.deleteCount;
			aTempPinYin.allDeleteDuration += aTempChangeWord.allDeleteDuration;

			SetMethodTable(aTempPinYin);
			mInputMethodList.Remove(aTempChangeWord);
		}
		/// <summary>
		/// Count single VRIME_BIInputChar to VIRME_BIInputMethodTable.
		/// For list foreach loop use.
		/// </summary>
		/// <param name="iChar"></param>
		/// <param name="iIsInput"></param>
		private void SaveCahrToTable(VRIME_BIInputChar iChar, bool iIsInput)
		{
			if(iChar.insertType == INPUT_METHOD_VOICE)// Voice Skip ' '
			{
				if(iChar.inputWord == ' ')
					return;
			}
			VIRME_BIInputMethodTable aTemp = GetMethodTable(iChar.biEngine, iChar.language);
			aTemp.insertType = iChar.insertType;
			if(iIsInput)
			{
				double aInputD = System.Math.Round(iChar.duration);
				aTemp.inputCount += 1;
				aTemp.allInputDuration += aInputD;
			}
			else
			{
				double aDeleteD = System.Math.Round(iChar.duration);
				aTemp.deleteCount += 1;
				aTemp.allDeleteDuration += aDeleteD;
			}
			SetMethodTable(aTemp);
		}
		/// <summary>
		/// Get VIRME_BIInputMethodTable data.
		/// </summary>
		/// <param name="iEngine"></param>
		/// <param name="iLanguage"></param>
		/// <returns></returns>
		private VIRME_BIInputMethodTable GetMethodTable(string iEngine, string iLanguage)
		{
			VIRME_BIInputMethodTable aResult = new VIRME_BIInputMethodTable();
			foreach(VIRME_BIInputMethodTable eTable in mInputMethodList)
			{
				// Check Engine
				if(iEngine != eTable.biEngine)
					continue;
				// Check use Language
				if(iLanguage != eTable.language)
					continue;
				// Set Result
				aResult = eTable;
			}
			// If Not Fount Table, Make One
			if(string.IsNullOrEmpty(aResult.biEngine) && string.IsNullOrEmpty(aResult.language))
			{
				aResult.biEngine = iEngine;
				aResult.language = iLanguage;
				mInputMethodList.Add(aResult);
			}
			return aResult;
		}
		/// <summary>
		/// Make new VIRME_BIInputMethodTable data.
		/// </summary>
		/// <param name="iTable"></param>
		private void SetMethodTable(VIRME_BIInputMethodTable iTable)
		{
			for(int i = 0; i < mInputMethodList.Count; i++)
			{
				// Check Engine
				if(iTable.biEngine != mInputMethodList[i].biEngine)
					continue;
				// Check use Language
				if(iTable.language != mInputMethodList[i].language)
					continue;
				mInputMethodList[i] = iTable;
			}
		}
		/// <summary>
		/// Clean list.
		/// </summary>
		/// <param name="iList"></param>
		private void CleanInputCharList(ref List<VRIME_BIInputChar> iList)
		{
			iList.RemoveRange(0, iList.Count);
			iList = new List<VRIME_BIInputChar>();
		}
		#endregion
		#region Button Hit Position Calculate
		/// <summary>
		/// Get Hit Position By Quadrant
		/// </summary>
		/// <param name="iBtn"></param>
		/// <param name="iHit"></param>
		public void ButtonHitPosition(VRIME_KeyboardButton iBtn, Collider iHit)
		{
			BoxCollider aBtnBox = iBtn.Event.GetComponent<BoxCollider>();
			// Return to Zero Pos
			float aReturnZeroX = iHit.transform.position.x - aBtnBox.transform.position.x;
			float aReturnZeroY = iHit.transform.position.y - aBtnBox.transform.position.y;
			// Quadrant
			int aXYQuad = GetQuadrantInt(aReturnZeroX, aReturnZeroY);

			VRIME_Debugger.Log("Get Position Info:\n ToZero X :" + aReturnZeroX + ", Y :" + aReturnZeroY
				+ "\n Hit X :" + iHit.transform.position.x + ", Y :" + iHit.transform.position.y
				+ "\n Center X :" + aBtnBox.transform.position.x + ", Y :" + aBtnBox.transform.position.y
				+ "\n Quadrant :" + aXYQuad);
		}
		/// <summary>
		/// 取象限
		/// </summary>
		/// <param name="iX"></param>
		/// <param name="iY"></param>
		/// <returns></returns>
		private int GetQuadrantInt(float iX, float iY)
		{
			if(iX > 0 && iY >= 0) { return 1; }
			else if(iX <= 0 && iY > 0) { return 2; }
			else if(iX < 0 && iY <= 0) { return 3; }
			else if(iX >= 0 && iY < 0) { return 4; }
			else { return 0; }
		}
		#endregion
		#region Input Method Send
		/// <summary>
		/// Send Recred Table Data
		/// </summary>
		private void SendInputMethod()
		{
			SaveAllCharToTable();
			BILogInit();
			foreach(VIRME_BIInputMethodTable eTable in mInputMethodList)
			{
				if(CheckTableNotSend(eTable))
					continue;

				using(new BILogScope(CATEGORY_INPUT_METHOD)) {
					BILogScope.BI.AddData(KEY_EVENT, INPUT_EV_SUBMIT);
					BILogScope.BI.AddData(KEY_A0, eTable.insertType);
					BILogScope.BI.AddData(KEY_A1, eTable.biEngine);
					BILogScope.BI.AddData(KEY_A2, eTable.language);
					BILogScope.BI.AddData(KEY_A3, eTable.inputCount.ToString());
					BILogScope.BI.AddData(KEY_A4, eTable.deleteCount.ToString());
					BILogScope.BI.AddData(KEY_A5, eTable.allInputDuration.ToString());
					BILogScope.BI.AddData(KEY_A6, eTable.allDeleteDuration.ToString());
					AddDataCommon();
				}
				eTable.PrintLog();
			}
			VRIME_Debugger.Log("Send Done, mInputMethodList Count :" + mInputMethodList.Count);
			// Clean Input Method
			mInputMethodList.RemoveRange(0, mInputMethodList.Count);
			mInputMethodList = new List<VIRME_BIInputMethodTable>();
		}
		/// <summary>
		/// if Any value is null, empty or "None", don't send table
		/// </summary>
		/// <param name="iTable"></param>
		/// <returns></returns>
		private bool CheckTableNotSend(VIRME_BIInputMethodTable iTable)
		{
			bool aResult = false;

			aResult = string.IsNullOrEmpty(iTable.biEngine)
				|| string.IsNullOrEmpty(iTable.language)
				|| string.IsNullOrEmpty(iTable.insertType)
				|| iTable.biEngine == INPUT_NONE
				|| iTable.language == INPUT_NONE;

			if(aResult == true)
			{
#if UNITY_EDITOR
				VRIME_Debugger.Log("Have a None Value..............");
				iTable.PrintLog();
#endif
			}
			return aResult;
		}
		#endregion
	}
	/// <summary>
	/// Recored Every Char
	/// </summary>
	public struct VRIME_BIInputChar
	{
		public string insertType;
		public string biEngine;
		public string language;
		public char inputWord;
		public int index;
		public double duration;

		public void PrintLog()
		{
			VRIME_Debugger.Log("Char :" + inputWord
			+ "\n Engine :" + biEngine
			+ "\n Language :" + language
			+ "\n Insert Type :" + insertType
			+ "\n duration :" + duration
			+ "\n index :" + index);
		}
	}
	/// <summary>
	/// Output to BILogger Table
	/// </summary>
	public struct VIRME_BIInputMethodTable
	{
		public string insertType;
		public string biEngine;
		public string language;
		public int inputCount;
		public int deleteCount;
		public double allInputDuration;
		public double allDeleteDuration;

		public void PrintLog()
		{
			VRIME_Debugger.Log("Engine :" + biEngine
			+ "\n Language :" + language
			+ "\n Insert Type :" + insertType
			+ "\n all input D-Time :" + allInputDuration
			+ "\n all delete D-Time :" + allDeleteDuration
			+ "\n inputCount :" + inputCount
			+ "\n deleteCount :" + deleteCount);
		}
	}
}