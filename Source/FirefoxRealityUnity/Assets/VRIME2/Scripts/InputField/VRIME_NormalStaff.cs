// ========================================================================== //
//  Created: 2019-06-26
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	public class VRIME_NormalStaff : VRIME_InputFieldStaff
	{
		#region override Function
		public override void Insert(string iText)
		{
			// Remove Selection
			if(SelAnchorPosition != SelFocusPosition)
				RemoveSelection();
			
			int biCaretPos = CaretPosition;
			UpdateInputField(iText);
			// BI Logger
			VRIME_BISender.Ins.LabelInsert(iText, biCaretPos);
		}
		/// <summary>
		/// Delete Last Word By Delete Button Click.
		/// If PinYin Language, Delete Will Remove WCL Words First, Then Remove InputTemp Words.
		/// </summary>
		public override void DeleteButton()
		{
			if(inputLabel == null || InputTemp.Length < 1)
				return;
			
			if((SelAnchorPosition + SelFocusPosition) < 1)
				return;
			if(SelAnchorPosition.Equals(SelFocusPosition) == false) {
				RemoveSelection();
				return;
			}

			int aCaretPos = CaretPosition;
			if(inputLabel.multiLine == false)
			{
				//show previous char
                CaretPosition = aCaretPos - 2;
                inputLabel.ForceLabelUpdate();
                CaretPosition = aCaretPos;
			}
			// Remove Last Word
			InputTemp = InputTemp.Remove(aCaretPos - 1, 1);
			CaretPosition--;
			inputLabel.text = InputTemp;
			// Update
            inputLabel.ForceLabelUpdate();
			// BI Logger
			VRIME_BISender.Ins.LabelListDelete(aCaretPos - 1, 1);
		}
		#endregion
		#region public Function
		public void InsertCandidateWords(Words iWord, int iStartIndex)
		{
			// Get Words Code
			string aChWords = iWord.Value;
			string aWordCode = iWord.Code;
			aWordCode = aWordCode.Replace(" ", string.Empty);
			// Replace Words
			int aBIInsertPos = CaretPosition; // Fix Insert Pos
			InputTemp = InputTemp.Insert(CaretPosition, aChWords);
			CaretPosition = CaretPosition + aChWords.Length;
			VRIME_BISender.Ins.PinYinChangeWord(iStartIndex, aWordCode.Length, aChWords, aBIInsertPos);// BILogger
		}
		public void InsertByNotMatchWCL(string iText)
		{
			int aBIInsertPos = CaretPosition;
			UpdateInputField(iText);
			VRIME_BISender.Ins.PinYinInsertNotMatchWords(aBIInsertPos, iText);// BILogger
		}
		public void InsertByCancelCompose(int iCursorPos, string iText)
		{
			//Submit WCL string directly
            InputTemp = InputTemp.Insert(iCursorPos, iText);
			// BI Logger
			VRIME_BISender.Ins.CancelCompose(iCursorPos, iText);
		}
		/// <summary>
		/// 輸入先前打好的字到InputTemp
		/// </summary>
		/// <param name="iText"></param>
		public void InsertDefault(string iText)
		{
			// Save Default caretPosition
			int aCursorPos = CaretPosition;
			// Re Input Default Text(Clear InputTemp First)
			ClearInput();
			UpdateInputField(iText);
			// Reset caretPosition
			CaretPosition = aCursorPos;
			// BI Logger
			VRIME_BISender.Ins.DefaultInsert(iText);
		}
		/// <summary>
		/// Empty InputTemp And InputField
		/// </summary>
		public void ClearInput()
		{
			InputTemp = string.Empty;
			inputLabel.text = InputTemp;
			CaretPosition = 0;
			inputLabel.ForceLabelUpdate();
			// BI Logger
			VRIME_BISender.Ins.LabelDeleteAll();
		}
		public void VoiceInsertContent(string iText)
		{
			InputTemp = iText;
		}
		#endregion
		#region private Function
		/// <summary>
		/// 
		/// </summary>
		/// <param name="iText"></param>
		private void UpdateInputField(string iText)
		{
			InputTemp = InputTemp.Insert(CaretPosition, iText);
			// Update InputField
			inputLabel.text = InputTemp;
			CaretPosition = CaretPosition + iText.Length;
			// Update Status
			inputLabel.ForceLabelUpdate();
			inputLabel.Select();
			inputLabel.ActivateInputField();
		}
		#endregion	
	}
}