// ========================================================================== //
//  Created: 2019-06-26
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	public abstract class VRIME_InputFieldStaff : MonoBehaviour
	{
		#region public Field
		public int CaretPosition{
			get{
				int aResult = 0;
				if(inputLabel == null) {
					aResult = InputTemp.Length;
				}
				else {
					aResult = inputLabel.caretPosition;
				}
				
				return aResult;
			}
			protected set {
				if(inputLabel == null)
					return;
				
				inputLabel.caretPosition = value; 
			}
		}
		public int SelAnchorPosition {
			get { return inputLabel.selectionAnchorPosition; }
		}
		public int SelFocusPosition {
			get { return inputLabel.selectionAnchorPosition; }
		}
		public string InputTemp { get; protected set; }
		#endregion
		#region protected Field
		protected InputField inputLabel = VRIME_InputFieldOversee.Ins.InputLabel;
		#endregion
		#region public Function
		virtual public void Insert(string iText) { }
		virtual public void DeleteButton() { }
				/// <summary>
		/// Remove Select in Input Field
		/// </summary>
		public virtual void RemoveSelection()
		{
			if (inputLabel == null || InputTemp.Length < 1)
				return;
			int aAnchorPos = inputLabel.selectionAnchorPosition;
			int aFocusPos = inputLabel.selectionFocusPosition;
			if(aAnchorPos.Equals(aFocusPos))
				return;
			if(aAnchorPos > InputTemp.Length || aFocusPos > InputTemp.Length)
				return;

			int start = Mathf.Min(aAnchorPos, aFocusPos);
            int end = Mathf.Max(aAnchorPos, aFocusPos);

            try
            {
                InputTemp = InputTemp.Remove(start, end - start);
				// BI Logger
				VRIME_BISender.Ins.LabelListDelete(start, end - start);
            }
            catch
            {
                VRIME_Debugger.LogError(inputLabel.name, "Remove Selection Error");
            }
			// Reset Text
            inputLabel.text = InputTemp;
            inputLabel.caretPosition = start;
            inputLabel.ForceLabelUpdate();
		}
		#endregion		
	}
}