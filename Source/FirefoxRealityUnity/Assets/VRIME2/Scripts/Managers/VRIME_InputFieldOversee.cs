// ========================================================================== //
//  Created: 2019-01-10
//  Remake: 2019-06-25
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

    [AddComponentMenu("VRIME/IMEManager/InputField Oversee")]
	public class VRIME_InputFieldOversee : MonoBehaviour
	{
		public static VRIME_InputFieldOversee Ins {
			get { return instance; }
			set {
				instance = value;
				instance.Init();
			}
		}
		private static VRIME_InputFieldOversee instance;
		#region public/private
		public bool CanInsert {
			get { return checkInsert; }
			private set { checkInsert = value; }
		}
		private bool checkInsert;
		public InputField InputLabel { get { return inputLabel; } }
		private InputField inputLabel;
		public bool PasswordMode {
			get { return passwordStatus; }
			private set { passwordStatus = value; }
		}
		private bool passwordStatus = false;
		#endregion
		#region public Field
		public string InputTemp {
			get { return inputFieldGroup.InputTemp; }
		}
		public Transform RootPath;
		#endregion
		#region private Field
		private VRIME_KeyboardButton deleteAllButton;
		public VRIME_CursorTouchPad countrollerTouchPad;
		private VRIME_NormalStaff inputFieldGroup;
		private VRIME_WCLStaff wclGroup;
		// Recored InputField Selcet Pos for WCL
		private int mWCLFocusPos;
		private int mWCLAnchorPos;
		private bool mWCLCheckInputSelect;// Check Select Change To Match the WCL
		#endregion
		#region const Field
		private const float inputDelayTime = 0.15f;
		#endregion
		#region unity Function
		private void Update() {
			CheckUserSelection();
		}
		#endregion
		#region public Function
		/// <summary>
		/// Insert Word To Inputtemp(and Update InfputField)
		/// </summary>
		/// <param name="iText">insert words</param>
		/// <param name="iDelay">need delay insert(default:false)</param>
		public void Insert(string iText, bool iDelay = false)
		{
			if(CanInsert == false)
				return;
			if(iDelay)
			{
				VRIME_Manager.Ins.StartCoroutine(InsertDelay());
			}
			if(VRIME_Manager.runSystem.wordCandidateShow)
			{
				wclGroup.Insert(iText);
			}
			else
			{
				inputFieldGroup.Insert(iText);
			}
			
		}
		/// <summary>
		/// 輸入先前打好的字到InputTemp
		/// </summary>
		/// <param name="iText"></param>
		public void InsertDefault(string iText)
		{
			if(CanInsert == false)
				return;
			inputFieldGroup.InsertDefault(iText);
		}
		public void InsertByNotMatchWCL(string iText)
		{
			inputFieldGroup.InsertByNotMatchWCL(iText);
		}
		/// <summary>
		/// Empty InputTemp And InputField
		/// </summary>
		public void ClearInput()
		{
			inputFieldGroup.ClearInput();
		}
		/// <summary>
		/// Delete Last Word By Delete Button Click.
		/// If PinYin Language, Delete Will Remove WCL Words First, Then Remove InputTemp Words.
		/// </summary>
		public void DeleteButton()
		{
			if(VRIME_Manager.runSystem.wordCandidateShow)
			{
				if(WCLCheckSelectInput() == false) {
					VRIME_Debugger.LogError("WCL selection error");
            		mWCLCheckInputSelect = false;
				}

				if(wclGroup.DeleteButton())
					return;
			}
			// In WCL Mode, will
			inputFieldGroup.DeleteButton();
		}
		/// <summary>
		/// Delete All Button Disable Collider
		/// and 0.7f sec auto enable
		/// </summary>
		public void DelAllButtonHide()
		{
			deleteAllButton.Event.ColliderStatus(false);
			CancelInvoke("DelAllBtnAutoShow");
			Invoke("DelAllBtnAutoShow", 0.7f);
		}
		public void WCLShowInsert(int iCursorPos)
		{
			VRIME_Debugger.LogWarning("WCL try to insert characters at pos " + iCursorPos);
			int aSelectStartIndex = iCursorPos;
			if(aSelectStartIndex <= inputFieldGroup.InputTemp.Length == false) {
				VRIME_Debugger.LogError("WCL selection error");
				mWCLCheckInputSelect = false;
				return;
			}
			// Set Show Info
			string aShowWords = inputFieldGroup.InputTemp.Insert(aSelectStartIndex, wclGroup.InputTemp);
			mWCLFocusPos = aSelectStartIndex;
			mWCLAnchorPos = aSelectStartIndex + wclGroup.InputTemp.Length;
			mWCLCheckInputSelect = string.IsNullOrEmpty(wclGroup.InputTemp) == false;
			// Reset Text
            inputLabel.text = aShowWords;
			// Move To Anchor
			inputLabel.caretPosition = mWCLAnchorPos;
			inputLabel.ActivateInputField();
            inputLabel.ForceLabelUpdate();
			// Set Back And Select
			inputLabel.caretPosition = iCursorPos;
			inputLabel.selectionFocusPosition = mWCLFocusPos;
			inputLabel.selectionAnchorPosition = mWCLAnchorPos;
			inputLabel.ActivateInputField();
            inputLabel.ForceLabelUpdate();
		}
		#endregion
		#region inputField Control
		/// <summary>
		/// Move InputField Cursor
		/// </summary>
		/// <param name="value"></param>
		public void MoveCaret(int value)
        {
            if (inputLabel != null)
            {
                if (value < 0 && !this.inputLabel.multiLine)
                {
                    //show previous char
                    int caretPos = inputFieldGroup.CaretPosition;

                    inputLabel.caretPosition = caretPos + value - 1;
                    this.inputLabel.ForceLabelUpdate();

                    inputLabel.caretPosition = caretPos;
                }

                inputLabel.caretPosition += value;
                this.inputLabel.ForceLabelUpdate();
            }
        }
		public void PasswordInputfieldSet(string iPlaceholder)
		{
			SetInputPlaceholder(iPlaceholder);
			SetInputContentType(UnityEngine.UI.InputField.ContentType.Password);
		}
		/// <summary>
		/// InputField Object Set Back To Default Values
		/// </summary>
		public void DefaultInputfieldSet()
		{
			SetInputPlaceholder(VRIME_InternationalWord.InputPlaceholderText(VRIME_KeyboardSetting.IMELanguage));
			SetInputContentType(UnityEngine.UI.InputField.ContentType.Standard);
		}
		/// <summary>
		/// Set InputLabel Placeholder Words
		/// </summary>
		/// <param name="iWords"></param>
		public void SetInputPlaceholder(string iWords)
		{
			Text aPlaceHolder = inputLabel.placeholder.GetComponent<Text>();
			aPlaceHolder.text = iWords;
		}
		/// <summary>
		/// Set InputLabel Type
		/// </summary>
		/// <param name="iType"></param>
		public void SetInputContentType(InputField.ContentType iType)
		{
			PasswordMode = iType == InputField.ContentType.Password;
			inputLabel.contentType = iType;
			InputLabel.ForceLabelUpdate();
		}
		/// <summary>
		/// PinYin WCl Use, Insert WCLString To InputTemp
		/// </summary>
        public void CancelCompose(bool iResetAnime = true)
		{
			if (VRIME_Manager.runSystem.wordCandidateShow == false)
                return;
            string wclString = wclGroup.InputTemp;
            int cursorPos = wclGroup.CursorPos;

            VRIME_Debugger.LogWarningFormat("CancelCompose wclstring={0},pos={1},temp={2}", 
                wclString, cursorPos, InputTemp);
			// If String Will Null or Empty, not update InputTemp
			if(string.IsNullOrEmpty(wclString))
				return;
            //Reset WCL
            wclGroup.ResetWCL(iResetAnime);
			// Insert
			inputFieldGroup.InsertByCancelCompose(cursorPos, wclString);
			// update Words
            mWCLCheckInputSelect = false;
            InputLabel.text = InputTemp;
            InputLabel.caretPosition = wclString.Length + cursorPos;
            InputLabel.ForceLabelUpdate();
		}
		/// <summary>
		/// WCL Panel Extend UI Show/Hide
		/// </summary>
		public void WCLClosePanel()
		{
			if(wclGroup == null)
				return;
			
			wclGroup.CloseWCLPanel();
		}
		/// <summary>
		/// Set WCL Words To InputTemp
		/// </summary>
		/// <param name="iButton"></param>
		public void WCLButtonSetWords(VRIME_KeyboardButton iButton)
		{
			if(iButton.pinyinWord == null) {
				VRIME_Debugger.LogError(iButton.gameObject, "PinYin Word Is Null!");
				return;
			}
			mWCLCheckInputSelect = false;
			inputFieldGroup.InsertCandidateWords(iButton.pinyinWord, wclGroup.wclStartIndex);
			wclGroup.RemoveChangeWords(iButton.pinyinWord.Code);
			// Audio Click
			VRIME_KeyAudioOversee.Ins.FunctionSequence(eAudioStatusType.word_candidate_key);
			mWCLCheckInputSelect = true;
		}
		/// <summary>
		/// Call WCL's Function Button
		/// </summary>
		/// <param name="iBtn"></param>
		public void WCLItemFuncCall(VRIME_KeyboardButton iBtn)
		{
			if(iBtn == null)
				return;
			
			wclGroup.DoUIFunction(iBtn);
		}
		/// <summary>
		/// Like Select WCL Word.
		/// </summary>
		/// <param name="iButton"></param>
		public void WCLButtonNearKey(VRIME_KeyboardButton iButton)
		{
			if(iButton.pinyinWord == null) {
				VRIME_Debugger.LogError(iButton.gameObject, "PinYin Word Is Null!");
				return;
			}
			if(WCLCheckSelectInput() == false) {
				VRIME_Debugger.LogError("WCL selection error");
            	mWCLCheckInputSelect = false;
				return;
			}
			mWCLCheckInputSelect = false;
			// Get Words Code
			string aWordCode = iButton.pinyinWord.Code;
			aWordCode = aWordCode.Replace(" ", string.Empty);
			wclGroup.ChangeNearKeyWords(aWordCode);
			// Audio Click
			VRIME_KeyAudioOversee.Ins.FunctionSequence(eAudioStatusType.word_candidate_key);
		}
		/// <summary>
		/// Check WCL PinYin Select State
		/// </summary>
		/// <returns></returns>
		private bool WCLCheckSelectInput()
		{
			if(wclGroup == null)
				return false;
			if(mWCLCheckInputSelect == false)
				return true;
			// Get Input Value
			int aFocusPos = InputLabel.selectionFocusPosition;
			int aAnchorPos = InputLabel.selectionAnchorPosition;
			
			return aFocusPos == mWCLFocusPos && aAnchorPos == mWCLAnchorPos;
		}
		/// <summary>
		/// Voice Input Use
		/// </summary>
		public void VoiceInputCallAction(bool iShow)
		{
			if(iShow)
			{
				// WCL To CancelCompose
				if(string.IsNullOrEmpty(wclGroup.InputTemp) == false)
					CancelCompose(false);
				// Show voice input need close function key event.
				wclGroup.SetFunctionKeyStatus(false);
				
			}
			else
			{
				inputFieldGroup.VoiceInsertContent(inputLabel.text);
				// Close voice input, inputLabel word as default words.
				// Before recored list need save to data tables.
				VRIME_BISender.Ins.SaveAllCharToTable();
				VRIME_BISender.Ins.DefaultInsert(inputLabel.text);
			}
		}
		#endregion
		#region private Function
		/// <summary>
		/// environment Init
		/// </summary>
		private void Init()
		{
			if(RootPath == null){
				VRIME_Debugger.LogError(Ins.name, "RootPath Is Null.");
				return;
			}
			// Input State
			inputLabel = RootPath.GetComponentInChildren<InputField>(true);
			inputLabel.onValueChanged.AddListener(InputLabelChange);
			// Find DeleteAllButton
			deleteAllButton = RootPath.GetComponentInChildren<VRIME_KeyboardButton>(true);
			deleteAllButton.Init();
			deleteAllButton.gameObject.SetActive(false);
			// TouchPad Event
			countrollerTouchPad = inputLabel.GetComponent<VRIME_CursorTouchPad>();
			if(countrollerTouchPad == null)
				countrollerTouchPad = inputLabel.gameObject.AddComponent<VRIME_CursorTouchPad>();
			// InputField Group
			inputFieldGroup = RootPath.GetComponent<VRIME_NormalStaff>();
			if(inputFieldGroup == null)
				inputFieldGroup = RootPath.gameObject.AddComponent<VRIME_NormalStaff>();
			// WCL Group
			Transform mWCLPath = RootPath.Find("Motion/WCLGroup");
			wclGroup = mWCLPath.GetComponent<VRIME_WCLStaff>();
			if(wclGroup == null)
				wclGroup = mWCLPath.gameObject.AddComponent<VRIME_WCLStaff>();
			wclGroup.Init();
			// Can Insert State Reset
			CanInsert = true;
		}
		/// <summary>
		/// On Input Field Change Evnet
		/// </summary>
		/// <param name="iWord"></param>
		private void InputLabelChange(string iWord)
		{
			if(VRIME_Manager.Ins == null)
				return;
			if(VRIME_Manager.Ins.ShowState == false)
				return;
			// Open Delete Button Button
			deleteAllButton.gameObject.SetActive(!string.IsNullOrEmpty(iWord));
			// On Value Change Evnet Send
			if(VRIME_Manager.Ins.onInputValueChange != null)
				VRIME_Manager.Ins.onInputValueChange.Invoke(iWord);
		}
		/// <summary>
		/// Some Input Need Delay.
		/// </summary>
		private IEnumerator InsertDelay()
		{
			CanInsert = false;
			yield return new WaitForSeconds(inputDelayTime);
			CanInsert = true;
		}
		/// <summary>
		/// Collider Enable
		/// </summary>
		private void DelAllBtnAutoShow()
		{
			deleteAllButton.Event.ColliderStatus(true);
		}
		/// <summary>
		/// Check PinYin Status Compose
		/// </summary>
        private void CheckUserSelection()
        {
			if (VRIME_Manager.runSystem == null)
				return;
            if (VRIME_Manager.runSystem.wordCandidateShow == false)
                return;

            if (WCLCheckSelectInput() == false)
            {
                CancelCompose();
            }
        }
		#endregion
	}
}