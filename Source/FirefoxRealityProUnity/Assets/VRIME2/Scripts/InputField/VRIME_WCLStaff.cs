// ========================================================================== //
//  Created: 2019-01-17
//  Remake: 2019-06-26
// ========================================================================== //
namespace VRIME2
{
    using System;
    using System.Collections;
	using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Htc.Omni;
    using UnityEngine;

    public class VRIME_WCLStaff : VRIME_InputFieldStaff
    {
		#region public/private Field
		public int wclStartIndex { get { return mStartIndex; } }
		private int mStartIndex = 0;
		public int CursorPos { get { return mOldCaretPos; } }
		private int mOldCaretPos = 0;
		#endregion
		#region public Field
		public eAnimWCL WCLState = eAnimWCL.WCLStates_Normal;
		#endregion
		#region private Field
		private Animator motion;
		private Transform itemCanvas;
		private bool firstLineOpened = false;
		private List<Words> mUseWordsList;
		private VRIME_WCLItemController wclItemCtrl;
		#endregion
		#region override Function
		public new bool DeleteButton()
		{
			if(string.IsNullOrEmpty(InputTemp))
				return false;

            string aAfterWords = InputTemp.Remove(InputTemp.Length - 1, 1);
			VRIME_BISender.Ins.PinYinListDelete(InputTemp.Length - 1, 1);
			// Update After Words
			InputTemp = aAfterWords;
			UpdateCandidateItem();
			return true;
		}
        public override void Insert(string iText)
		{
			//Get cusor current position
			mOldCaretPos = CaretPosition;
			if(SkipInsertAction(iText)) {
				return;
			}
			// BI Logger
			VRIME_BISender.Ins.PinYinInsert(iText, InputTemp.Length);
			// Get Start Using Words
			InputTemp = InputTemp.Insert(InputTemp.Length, iText);
			mStartIndex = VRIME_Manager.runSystem.GetStartIndex(InputTemp, StartIndexNoMatchWords);
			if(string.IsNullOrEmpty(InputTemp)){
				mUseWordsList = new List<Words>();
            }			
			UpdateCandidateItem();
		}
		#endregion
		#region public Fucntion
		public void Init()
		{
			InputTemp = string.Empty;
			motion = this.transform.parent.GetComponent<Animator>();
			VRIME_Manager.Ins.onInputValueChange.AddListener(InputValueCheck);
			itemCanvas = this.transform.Find("Canvas");
			wclItemCtrl = itemCanvas.GetComponent<VRIME_WCLItemController>();
			if(wclItemCtrl == null) {
				wclItemCtrl = itemCanvas.gameObject.AddComponent<VRIME_WCLItemController>();
			}
			wclItemCtrl.Init();
		}
		public void CloseWCLPanel()
		{
			CleanUseingWords();
			wclItemCtrl.CloseItems();
		}
		/// <summary>
		/// Clean Word And Return First String To InputField
		/// </summary>
		/// <returns></returns>
		public string CleanUseingWords()
		{
			// Return WordsList First Value
			string aResult = string.Empty;
			if(mUseWordsList == null)
				return aResult;
			if(mUseWordsList.Count >= 1)
				aResult = mUseWordsList[0].Value;
            ResetWCL();
            return aResult;
		}
		/// <summary>
		/// Because voice input don't need do close animation, so only reset status.
		/// </summary>
		/// <param name="iCloseAnime"></param>
		public void ResetWCL(bool iCloseAnime = true)
        {
            // Clean Values
            InputTemp = string.Empty;
            mUseWordsList = null;
			wclItemCtrl.ResetExtendPage();
            mStartIndex = 0;
			mOldCaretPos = 0;
			firstLineOpened = false;
            //Close animation
			if(iCloseAnime)
            	UIAnimeShow(true);
			else
				WCLState = eAnimWCL.WCLStates_Normal;
        }
		/// <summary>
		/// Call function active.
		/// </summary>
		/// <param name="iEnbale"></param>
		public void SetFunctionKeyStatus(bool iEnbale)
		{
			wclItemCtrl.SetFunctionEventStatus(iEnbale);
		}
		#endregion
		#region private Function
		private void InputValueCheck(string iText)
		{
			if(string.IsNullOrEmpty(iText)) {
				CloseWCLPanel();
			}
		}
		private bool SkipInsertAction(string iText)
		{
			bool aResult = VRIME_KeyboardData.CheckNeedSkipInsert(iText.ToLower());
			if(aResult)
			{
                VRIME_Debugger.Log("SkipInsertAction ShowText =" + InputTemp + ", iText =" + iText);
				InputTemp = string.Concat(InputTemp, iText);
                VRIME_InputFieldOversee.Ins.CancelCompose();
            }
			return aResult;
		}
		/// <summary>
		/// Not Match Word Handle
		/// </summary>		
		private void StartIndexNoMatchWords(string iWords)
		{
			VRIME_InputFieldOversee.Ins.InsertByNotMatchWCL(iWords);
			InputTemp = InputTemp.Remove(0, 1);// RemoveWords
			mOldCaretPos++; // Cursor move right 1
		}
		#endregion
		#region String Make
		private void WordListLog(List<Words> iWordList)
		{
			string aLog = "Words Log------------------------------------\n";
			foreach(Words eWord in iWordList)
			{
				aLog +=  "Code :" + eWord.Code + ",  Value :" + eWord.Value + "\n";
			}
			VRIME_Debugger.Log(aLog);
		}
		public void RemoveChangeWords(string iCode)
		{
			if(string.IsNullOrEmpty(InputTemp)){
				CleanUseingWords();
				return;
			}
			// Get After Words
			string aAfterWords = InputTemp;
			aAfterWords = aAfterWords.Remove(mStartIndex, iCode.Length);
			// Close Extend
			if(WCLState == eAnimWCL.WCLStates_Extended)
				callWCLExtend();
			// Update Insert Words
			InputTemp = aAfterWords;
			mOldCaretPos = CaretPosition;
			mStartIndex = VRIME_Manager.runSystem.GetStartIndex(InputTemp, StartIndexNoMatchWords);
			UpdateCandidateItem();
		}
		/// <summary>
		/// Near key mechanism just replaces input word, and it doesn't send word to input field
		/// </summary>
		/// <param name="iCode"></param>
		public void ChangeNearKeyWords(string iCode)
		{
			if(string.IsNullOrEmpty(InputTemp)){
				CleanUseingWords();
				return;
			}
			// Get After Words
			string aAfterWords = InputTemp;
			aAfterWords = aAfterWords.Remove(mStartIndex, iCode.Length);
			aAfterWords = aAfterWords.Insert(mStartIndex, iCode);
			// BI Logger
			VRIME_BISender.Ins.PinYinNearKeyChange(mStartIndex, iCode);
			// Update Insert Words
			InputTemp = aAfterWords;
			UpdateCandidateItem();
			VRIME_Debugger.Log("ChangeNearKeyAfter = " + InputTemp);
		}
		#endregion
		#region Anime Function
		private void CheckAnimShow()
		{
			bool aIsClose = mUseWordsList == null;
			if(aIsClose == false)
				aIsClose = mUseWordsList.Count == 0;
			
			UIAnimeShow(aIsClose);
			if(aIsClose)
			{
				// Set PageNum
				VRIME_WCLFunctionButton aPageBtn = wclItemCtrl.GetFunctionButton(eWCLFunction.ShowPage);
				aPageBtn.button.gameObject.SetActive(false);
			}			
			// Show Row 1 Item
			wclItemCtrl.SetBodyObjectShow(false);
		}
		private void UIAnimeShow(bool iIsClose)
		{
			eAnimWCL aPlaying = WCLState;
			bool aFunctionEnable = true;
			if(iIsClose)
			{				
				switch(aPlaying)
				{
					case eAnimWCL.WCLStates_Opened:
					case eAnimWCL.WCL_ExtendedClosed:
						aPlaying = eAnimWCL.WCLStates_Closed;
						break;
					case eAnimWCL.WCLStates_Extended:
						aPlaying = eAnimWCL.WCLStates_AllClosed;
						break;
				}
				// When WCL is closed, function need set disable.
				aFunctionEnable = false;
				// Set Close Time In 0.3f Sec
				CancelInvoke("CloseWCLToNromal");
				Invoke("CloseWCLToNromal", 0.3f);
			}
			else
			{
				switch(aPlaying)
				{
					case eAnimWCL.WCLStates_Normal:
					case eAnimWCL.WCLStates_Closed:
					case eAnimWCL.WCLStates_AllClosed:
						aPlaying = eAnimWCL.WCLStates_Opened;
						break;
				}
			}
			// Set listener event.
			SetFunctionKeyStatus(aFunctionEnable);
			// Final Play Anime
			AnimePlay(aPlaying);
			if(firstLineOpened)
				return;
			// Extend Check and Play Anime 
			CheckExtendArror();
			firstLineOpened = true;
		}
		private void CloseWCLToNromal()
		{
			AnimePlay(eAnimWCL.WCLStates_Normal);
			firstLineOpened = false;
		}
		private void CheckExtendArror()
		{
			bool aExtendClose = true;
			switch(WCLState)
			{
				case eAnimWCL.WCLStates_Normal:
					return;
				case eAnimWCL.WCLStates_Opened:
				case eAnimWCL.WCL_ExtendedClosed:
				case eAnimWCL.WCLStates_AllClosed:
					aExtendClose = false;
					break;
			}
			// Extend Arrow Anime
			extendAnimePlay(aExtendClose);
		}
		private void extendAnimePlay(bool iState)
		{
			if(VRIME_KeyboardData.WCLExtendButton == null)
				return;
			if(iState) {
				VRIME_KeyboardData.WCLExtendButton.motion.Play(eAnimeWCLExtendBtn.ToggleTransition.ToString());
			}
			else {
				VRIME_KeyboardData.WCLExtendButton.motion.Play(eAnimeWCLExtendBtn.ToggleTransitionBack.ToString());
			}			
			// Hide Delete All Button
			VRIME_InputFieldOversee.Ins.DelAllButtonHide();
		}
		private void ExtendAudioPlay(bool iShow)
		{
			eAudioStatusType aAudioType = eAudioStatusType.expand_candidate_key;
			if(iShow == false) {
				aAudioType = eAudioStatusType.dissmiss_candidate_key;
			}
			VRIME_KeyAudioOversee.Ins.FunctionSequence(aAudioType);
		}
		/// <summary>
		/// Show WCL Extend UI By Now Animation State
		/// </summary>
		public void callWCLExtend()
		{
			// Show Extend Anime
			bool aExtendStatus = WCLState != eAnimWCL.WCLStates_Extended;
			if(aExtendStatus) {
				AnimePlay(eAnimWCL.WCLStates_Extended);
				VRIME_BISender.Ins.CallActionClick(eActionClickEntrance.WCL_open);
			}
			else {
				AnimePlay(eAnimWCL.WCL_ExtendedClosed);
			}
			// Set PageNum show
			VRIME_WCLFunctionButton aPageBtn = wclItemCtrl.GetFunctionButton(eWCLFunction.ShowPage);
			aPageBtn.button.gameObject.SetActive(aExtendStatus);
			// Set Cat Body			
			wclItemCtrl.SetBodyObjectShow(aExtendStatus);
			wclItemCtrl.UpdatePageItems(mUseWordsList);
			// Button Anime Play
			extendAnimePlay(aExtendStatus);
			// Play Audio
			ExtendAudioPlay(aExtendStatus);
		}
		private void AnimePlay(eAnimWCL iState)
		{
			if(motion == null)
				return;

			WCLState = iState;
			motion.Play(iState.ToString());
		}
		#endregion
		#region Set Extend UI
		private void UpdateCandidateItem()
		{
			// Show WCL Words
			wclItemCtrl.ResetWordsIndex();
			mUseWordsList = VRIME_Manager.runSystem.GetMaxCombinStrCandidates(InputTemp, mStartIndex);
			// Animation Play
			CheckAnimShow();
			VRIME_InputFieldOversee.Ins.WCLShowInsert(mOldCaretPos);
			if(mUseWordsList == null)
				return;

			wclItemCtrl.SetRowBodyItem(mUseWordsList, WCLState);
			wclItemCtrl.SetRowHeadShow(InputTemp, WCLState);
			wclItemCtrl.SetPageButtonShow(WCLState);
			WordListLog(mUseWordsList);
		}
		#endregion
		#region Function Keys
		public void DoUIFunction(VRIME_KeyboardButton iBtn)
		{
			VRIME_WCLFunctionButton aDoButton = wclItemCtrl.GetFucntionKey(iBtn);
			if(aDoButton == null)
				return;
			switch(aDoButton.type)
			{
				case eWCLFunction.Extend:
					callWCLExtend();
					break;
				case eWCLFunction.ItemUp:
					wclItemCtrl.ChangeWordPageUp(mUseWordsList, WCLState);
					break;
				case eWCLFunction.ItemDown:
					wclItemCtrl.ChangeWordPageDown(mUseWordsList, WCLState);
					break;
			}
		}		
		#endregion
    }
	public class VRIME_WCLFunctionButton
	{
		public VRIME_KeyboardButton button;
		public eWCLFunction type;
	}
}