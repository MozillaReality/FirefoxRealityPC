// ========================================================================== //
//  Created: 2019-01-17
//  Remake: 2019-06-26
// ========================================================================== //
namespace VRIME2
{
    using System;
    using System.Collections;
	using System.Collections.Generic;
    using Htc.Omni;
    using UnityEngine;

	public class VRIME_WCLItemController : MonoBehaviour
	{
		#region const Field
		private const int mBaseRowItems = 6;
		#endregion
		#region private Field
		private GameObject originalDivider;
		private VRIME_KeyboardButton originalWclItem;
		private VRIME_WCLCategory[] wclCategorys;
		private VRIME_WCLFunctionButton[] wclFunctionKeys;
		private int mFirstRowLimit;
		private int mWordsIndex = 0;
		private int ExtendPage = 1;
		private List<int> mPageStartIndex;
		#endregion
		#region status Control
		public void ResetWordsIndex()
		{
			mWordsIndex = 0;
		}
		public void ResetExtendPage()
		{
			ExtendPage = 1;
		}
		public void CloseItems()
		{
			foreach (VRIME_WCLCategory item in wclCategorys)
			{
				item.HeadObjectsSet(false);
				item.BodyObjectsSet(false);
			}
		}
		#endregion
		#region public Function
		public void Init()
		{
			GetOriginalBtns();
			SetCategory();
			SetFunctionKey();
			SetFunctionEventStatus(false);
		}
		/// <summary>
		/// Set function button event enable/disable.
		/// </summary>
		/// <param name="iActive"></param>
		public void SetFunctionEventStatus(bool iActive)
		{
			for(int i = 0; i < wclFunctionKeys.Length; i++)
			{
				VRIME_WCLFunctionButton aTemp = wclFunctionKeys[i];
				aTemp.button.SetButtonEvent(iActive);
			}
		}
		public VRIME_WCLFunctionButton GetFunctionButton(eWCLFunction iType)
		{
			VRIME_WCLFunctionButton aResult = null;
			for(int i = 0; i < wclFunctionKeys.Length; i++)
			{
				VRIME_WCLFunctionButton aTemp = wclFunctionKeys[i];
				if(aTemp.type == iType)
				{
					aResult = aTemp;
					break;
				}
			}
			return aResult;
		}
		public void SetBodyObjectShow(bool iRowOther)
		{
			if(wclCategorys == null)
				return;
			for(int i = 0; i< wclCategorys.Length; i++)
			{
				bool aShow = false;

				if(wclCategorys[i].atRows == eWCLRows.WCL_Row_1){
					continue;
				}
				else {
					aShow = iRowOther;
				}
				
				wclCategorys[i].BodyObjectsSet(aShow);
				wclCategorys[i].HeadObjectsSet(aShow);
				wclCategorys[i].Layout.UpdateLayout();
			}
		}
		/// <summary>
		/// Set Extend UI
		/// </summary>
		/// <param name="iWords"></param>
		public void SetRowBodyItem(List<Words> iWords, eAnimWCL iNowState)
		{
			int aPageNum = 0;// One Page Have ItemNum
			for(int i = 0; i < wclCategorys.Length; i++)
			{
				VRIME_WCLCategory aSetItem = wclCategorys[i];
				aSetItem.BodyObjectsSet(false);
				int aSetItemLimit = mBaseRowItems;
				if(aSetItem.atRows == eWCLRows.WCL_Row_1) {
					aSetItemLimit = mBaseRowItems + 1;
					mFirstRowLimit = aSetItemLimit;
				}
				int aDivLimit = aSetItemLimit - 1;

				for(int k = 0; k < aSetItemLimit; k++)
				{
					// Get Setting Words
					Words aWordItem = GetCanUseWords(iWords, mWordsIndex);
					if(aWordItem == null)
						break;
					// Get Writing Button
					VRIME_KeyboardButton aButton = GetCategoryButton(aSetItem, k, aDivLimit);
					aButton.Word = aWordItem.Value;
					aButton.pinyinWord = aWordItem;
					mWordsIndex++;
					aPageNum++;
					// Button Space Check
					bool aNeedSpace = AddButtonExtendSpace(aButton);
					// Show Button
					if(aSetItem.atRows == eWCLRows.WCL_Row_1 || iNowState == eAnimWCL.WCLStates_Extended) {
						if(aNeedSpace) {
							mFirstRowLimit--;
						}
						aButton.gameObject.SetActive(true);
						if(k < aSetItem.BodyTextDivs.Count)
						{
							aSetItem.BodyTextDivs[k].SetActive(true);
						}
					}
					if(aNeedSpace) {
						aSetItemLimit--;
					}
				}
				aSetItem.RowLimit = aSetItemLimit;
				aSetItem.Layout.UpdateLayout();
			}
			// Page Count
			mPageStartIndex = new List<int>();
			mPageStartIndex.Add(0);// Base Num
			int aExtendPageNum = aPageNum - mFirstRowLimit;
			int aCountNum = mFirstRowLimit;
			while(aCountNum < iWords.Count)
			{
				mPageStartIndex.Add(aCountNum);
				aCountNum += aExtendPageNum;
			}
			// Set Page Num
			ExtendPage = 1;// Reset StartPage
			SetPageWord();
		}
		/// <summary>
		/// Near Key Word UI
		/// </summary>
		public void SetRowHeadShow(string iInputWords, eAnimWCL iNowState)
		{
			List<string> aNearKeys = VRIME_Manager.runSystem.GetFirstWordNearKeys(iInputWords);
			if(aNearKeys == null)
				return;
			
			int aNearKeyIndex = 0;
			// Get/Set HeadButtons
			for(int i = 0; i < wclCategorys.Length; i++)
			{
				VRIME_WCLCategory aSetItem = wclCategorys[i];
				aSetItem.HeadObjectsSet(false);
				if(aSetItem.atRows == eWCLRows.WCL_Row_1)
					continue;
				VRIME_KeyboardButton aButton = null;
				if(aSetItem.Heads.Count < 1)
				{
					GameObject aTmpObj = GameObject.Instantiate(originalWclItem.gameObject, aSetItem.RowHead.transform);
					aTmpObj.name = "WCLItem" + i;
					aButton = aTmpObj.GetComponent<VRIME_KeyboardButton>();
					if(aButton == null){
						aButton = aTmpObj.AddComponent<VRIME_KeyboardButton>();
					}
					aButton.btnType = eButtonType.WCLNearKey;
					aButton.Init();
					aSetItem.Heads.Add(aTmpObj);
				}
				else
				{
					aButton = aSetItem.Heads[0].GetComponent<VRIME_KeyboardButton>();
				}

				string aNearKey = string.Empty;
				if(aNearKeyIndex < aNearKeys.Count) {
					aNearKey = aNearKeys[aNearKeyIndex];
					while(string.IsNullOrEmpty(aNearKey))
					{
						aNearKeyIndex++;
						aNearKey = aNearKeys[aNearKeyIndex];
					}
					aNearKeyIndex++;
				}
				bool aShowButton = string.IsNullOrEmpty(aNearKey) == false;

				aButton.Word = aNearKey;
				Words aTmp = new Words();
				aTmp.Code = aNearKey;
				aTmp.Value = aNearKey;
				aButton.pinyinWord = aTmp;
				if(iNowState == eAnimWCL.WCLStates_Extended)
					aButton.gameObject.SetActive(aShowButton);
			}
		}
		public void SetPageButtonShow(eAnimWCL iNowState)
		{
			if(mPageStartIndex == null)
				return;

			if(wclFunctionKeys == null)
				return;

			for(int i = 0; i < wclFunctionKeys.Length; i++)
			{
				if(wclFunctionKeys[i].type == eWCLFunction.Extend)
					continue;
				eWCLFunction aCheckType = wclFunctionKeys[i].type;
				bool aBtnShow = false;
				bool aEventAction = false;
				
				switch(aCheckType)
				{
					case eWCLFunction.ItemUp:
						aBtnShow = ExtendPage > 1;
						aEventAction = aBtnShow;
						break;
					case eWCLFunction.ItemDown:
						aBtnShow = ExtendPage < mPageStartIndex.Count - 1;
						aEventAction = aBtnShow;
						break;
					case eWCLFunction.ShowPage:
						aBtnShow = iNowState == eAnimWCL.WCLStates_Extended;
						break;
				}
				wclFunctionKeys[i].button.rootUI.gameObject.SetActive(aBtnShow);
				wclFunctionKeys[i].button.SetButtonEvent(aEventAction);
			}
			// And Set Page Words
			SetPageWord();
		}
		public VRIME_WCLFunctionButton GetFucntionKey(VRIME_KeyboardButton iBtn)
		{
			if(wclFunctionKeys == null)
				return null;
			
			VRIME_WCLFunctionButton aDoButton = null;
			for(int i = 0; i < wclFunctionKeys.Length; i++)
			{
				if(wclFunctionKeys[i].button.Equals(iBtn)) {
					aDoButton = wclFunctionKeys[i];
					break;
				}
			}
			return aDoButton;
		}
		public void ChangeWordPageUp(List<Words> iWords, eAnimWCL iNowState)
		{
			if(mPageStartIndex == null)
				return;
			if(mPageStartIndex.Count == 0)
				return;
			

			ExtendPage--;
			if(ExtendPage > 0) {
				UpdatePageItems(iWords);
			}
			else
				ExtendPage = 1;
			// Check Button Show
			SetPageButtonShow(iNowState);
			VRIME_BISender.Ins.CallActionClick(eActionClickEntrance.WCL_page_up);
		}
		public void ChangeWordPageDown(List<Words> iWords, eAnimWCL iNowState)
		{
			if(mPageStartIndex == null)
				return;
			if(mPageStartIndex.Count == 0)
				return;
			

			ExtendPage++;
			if(ExtendPage < mPageStartIndex.Count) {
				UpdatePageItems(iWords);
			}
			else
				ExtendPage = mPageStartIndex.Count - 1;
			// Check Button Show
			SetPageButtonShow(iNowState);
			VRIME_BISender.Ins.CallActionClick(eActionClickEntrance.WCL_page_down);
		}
		#endregion
		#region Init Fucntion
		private void GetOriginalBtns()
		{
			GameObject aTempObj = VRIME_AssetLoad.GetGameObjectResource("Prefabs/LabelButton_WCL");
			if(aTempObj != null)
			{
				aTempObj.SetActive(false);
				aTempObj.transform.parent = VRIME_Manager.Ins.BackgroundObjPath;
				originalWclItem = aTempObj.AddComponent<VRIME_KeyboardButton>();
				originalWclItem.Init();
				originalWclItem.btnType = eButtonType.LetterWCL;

			}
			aTempObj = VRIME_AssetLoad.GetGameObjectResource("Prefabs/TextDivider");
			if(aTempObj != null)
			{
				aTempObj.SetActive(false);
				aTempObj.transform.parent = VRIME_Manager.Ins.BackgroundObjPath;
				originalDivider = aTempObj;
			}
		}
		private void SetCategory()
		{
			string[] aWCLRowNames =  Enum.GetNames(typeof(eWCLRows));
			wclCategorys = new VRIME_WCLCategory[aWCLRowNames.Length];
			for(int i = 0; i < wclCategorys.Length; i++)
			{
				if(i == 0)
				{
					Transform aFirstBody = this.transform.Find("WordSuggestion/WCL_Row_" + (i + 1));
					VRIME_WCLCategory aFirstCat = aFirstBody.GetComponent<VRIME_WCLCategory>();
					if(aFirstCat == null)
						aFirstCat = aFirstBody.gameObject.AddComponent<VRIME_WCLCategory>();
					
					aFirstCat.Init((eWCLRows)i, aFirstBody);
					wclCategorys[i] = aFirstCat;
				}
				else
				{
					Transform aBody = this.transform.Find("WCL/WCL_Row_" + (i + 1));
					Transform aHead = this.transform.Find("WCL_Categories/WCL_Row_" + (i + 1));
					VRIME_WCLCategory aOthCat = aBody.GetComponent<VRIME_WCLCategory>();
					if(aOthCat == null)
						aOthCat = aBody.gameObject.AddComponent<VRIME_WCLCategory>();
					
					aOthCat.Init((eWCLRows)i, aBody, aHead);
					wclCategorys[i] = aOthCat;
				}
			}
		}
		private void SetFunctionKey()
		{
			Transform aKeyRoot = this.transform.Find("WCL_UI/Buttons");
			if(aKeyRoot == null)
				return;
			// wclFunctionKeys
			VRIME_KeyboardButton[] aFunKeys = aKeyRoot.GetComponentsInChildren<VRIME_KeyboardButton>(true);
			wclFunctionKeys = new VRIME_WCLFunctionButton[aFunKeys.Length];
			for(int i = 0; i < aFunKeys.Length; i++)
			{
				VRIME_WCLFunctionButton aTemp = new VRIME_WCLFunctionButton();
				aTemp.button = aFunKeys[i];
				aTemp.button.Init();
				string aButtonRowName = aTemp.button.transform.parent.name;
				switch(aButtonRowName)
				{
					case "WCL_Row_1":
						aTemp.type = eWCLFunction.Extend;
						break;
					case "WCL_Row_4":
						aTemp.type = eWCLFunction.ItemUp;
						break;
					case "WCL_Row_3":
						aTemp.type = eWCLFunction.ShowPage;
						aTemp.button.GetComponentInChildren<Collider>().enabled = false;
						break;
					case "WCL_Row_5":
						aTemp.type = eWCLFunction.ItemDown;
						break;
				}
				wclFunctionKeys[i] = aTemp;
			}
		}
		#endregion
		#region private Function
		/// <summary>
		/// Get mWordsIndex Select Words
		/// </summary>
		/// <param name="iWords"></param>
		/// <returns></returns>
		private Words GetCanUseWords(List<Words> iWords, int iIndex)
		{
			Words aResult = null;
			if(iIndex >= 0 && iIndex < iWords.Count) {
				aResult = iWords[iIndex];
			}
			return aResult;
		}
		/// <summary>
		/// Get And Set Button Item
		/// </summary>
		/// <param name="iCat"></param>
		/// <param name="iIndex"></param>
		/// <param name="iDivLimit"></param>
		/// <returns></returns>
		private VRIME_KeyboardButton GetCategoryButton(VRIME_WCLCategory iCat, int iIndex, int iDivLimit)
		{
			VRIME_KeyboardButton aResult = null;
			if(iIndex < iCat.Bodys.Count)
			{
				aResult = iCat.Bodys[iIndex].GetComponent<VRIME_KeyboardButton>();
			}
			else
			{
				GameObject aTmpObj = GameObject.Instantiate(originalWclItem.gameObject, iCat.RowBody.transform);
				aTmpObj.name = "WCLItem" + iIndex;
				aResult = aTmpObj.GetComponent<VRIME_KeyboardButton>();
				if(aResult == null){
					aResult = aTmpObj.AddComponent<VRIME_KeyboardButton>();
				}
				aResult.btnType = eButtonType.LetterWCL;
				aResult.Init();
				iCat.Bodys.Add(aTmpObj);
				// TextDirve
				if(iIndex < iDivLimit)
				{
					aTmpObj = GameObject.Instantiate(originalDivider, iCat.RowBody.transform);
					aTmpObj.name = "TextDivider" + iIndex;
					iCat.BodyTextDivs.Add(aTmpObj);
				}
			}
			return aResult;
		}
		private bool AddButtonExtendSpace(VRIME_KeyboardButton iBtn)
		{
			bool aResult = false;
			if(iBtn == null)
				return aResult;

			int aWordLen = iBtn.Word.Length;
			
			LinearLayoutElement aLayout = iBtn.GetComponent<LinearLayoutElement>();
			BoxCollider aBoxCol = iBtn.GetComponentInChildren<BoxCollider>(true);
			if(aWordLen >= 3)// Space Need Back
			{
				// Box Collider
				if(aBoxCol != null)
				{
					float aSizeX = 20f + ((aWordLen - 2) * 30f);
					Vector3 aBoxColSize = aBoxCol.size;
					aBoxColSize.x = aSizeX;
					aBoxCol.size = aBoxColSize;
				}
				// Layout
				if(aLayout == null)
					aLayout = iBtn.gameObject.AddComponent<LinearLayoutElement>();
					
				float aExXSize = 30f;
				if(aWordLen > 4)
				{
					aExXSize = (aWordLen - 2) * 18f;
					aResult = true;
				}

				aLayout.size = new Vector3(aExXSize, 1f, 1f);
			}
			else
			{
				// Box Collider
				if(aBoxCol != null)
				{
					Vector3 aBoxColSize = aBoxCol.size;
					aBoxColSize.x = 40f;
					aBoxCol.size = aBoxColSize;
				}
				// Layout
				if(aLayout == null)
					aLayout = iBtn.gameObject.AddComponent<LinearLayoutElement>();
				
				aLayout.size = new Vector3(0f, 1f, 1f);
			}
			return aResult;
		}
		private void SetPageWord()
		{
			VRIME_WCLFunctionButton aPageBtn = GetFunctionButton(eWCLFunction.ShowPage);
			if(aPageBtn == null)
				return;

			string aPageString = string.Empty;
			bool aShow = true;
			if(mPageStartIndex != null)
			{
				int aMaxPage = mPageStartIndex.Count - 1;
				if(aMaxPage <= 0) {
					aShow = false;
				}
				aPageString = ExtendPage + "/" + aMaxPage;
			}
			
			aPageBtn.button.Word = aPageString;
			aPageBtn.button.rootUI.gameObject.SetActive(aShow);
		}
		public void UpdatePageItems(List<Words> iWords)
		{
			if(wclCategorys == null)
				return;
			// Set Start Index
			if(ExtendPage < mPageStartIndex.Count)
				mWordsIndex = mPageStartIndex[ExtendPage];
				
			for(int i = 0; i < wclCategorys.Length; i++)
			{
				VRIME_WCLCategory aSetItem = wclCategorys[i];
				int aSetItemLimit = mBaseRowItems;
				if(aSetItem.atRows == eWCLRows.WCL_Row_1) {
					continue;
				}
				// Start Update Item
				int aDivLimit = aSetItemLimit - 1;
				for(int k = 0; k < aSetItemLimit; k++)
				{					
					// Get Values
					Words aWordItem = null;
					VRIME_KeyboardButton aButton = GetCategoryButton(aSetItem, k, aDivLimit);
					if(mWordsIndex < iWords.Count) {
						aWordItem = GetCanUseWords(iWords, mWordsIndex);
					}
					bool aItemIsNull = aWordItem == null;
					// Set Values
					if(aItemIsNull == false) {
						aButton.Word = aWordItem.Value;
						aButton.pinyinWord = aWordItem;
						mWordsIndex++;
					}
					aButton.gameObject.SetActive(!aItemIsNull);
					// Set TextDivider
					if(k < aSetItem.BodyTextDivs.Count) {
						aSetItem.BodyTextDivs[k].SetActive(!aItemIsNull);
					}
				}
			}
		}
		#endregion
	}
}