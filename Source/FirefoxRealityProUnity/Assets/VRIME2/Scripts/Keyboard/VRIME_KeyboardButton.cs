// ========================================================================== //
//  Created: 2019-01-08
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using UnityEngine.UI;
    using Htc.Omni;
    using TMPro;
    [AddComponentMenu("VRIME/IMEKeyboard/Keyboard Button")]
    public class VRIME_KeyboardButton : MonoBehaviour
    {
        public delegate void KeyPressed(int keyCode);

        public static KeyPressed OnKeyPressed;
        
        #region public Field
        // Button Base Info
        public string Word {
            get { return showingWord; }
            set {
                showingWord = value;
                SetButtonUIWord();
            }
        }
        private string showingWord;
        public eButtonType btnType;
        public eUIType uiType;
        public Transform parent;
        public Transform rootUI;
        public Animator motion;
        public Words pinyinWord;
        public bool stopEventAnime;
        public bool stopEventAudio;
        [SerializeField]
        private VRIME_ButtonEvent buttonEvent;
        public VRIME_ButtonEvent Event { get { return buttonEvent; } }
        public bool longPressStatus { get { return pressTime >= limitPressTime; } }
        #endregion
        #region private Field
        
        [SerializeField]
        // Type of Object
        private Text txtKeyChar;
        private TextMeshPro tmproKeyChar;
        private Image imgKeyChar;
        // Long Press
        private float pressTime;
        private float startPressTime;
        private float limitPressTime = 0.3f;
        private float actionDelayTime = 0.2f;
        
		private const float deleteLongPressTime = 1;
		private const float deleteSlowDelayTime = 0.2f;
		private const float deleteFastDelayTime = 0.05f;
        // State
        private bool canAction = true;
        #endregion
        #region Unity Funciton
        private void Awake() {
            Init();
        }
        #endregion
        #region public Function
        public void Init()
        {
            parent = this.transform.parent;
            // Search UI Root
            rootUI = VRIME_KeyboardData.GetChildInclude(this.transform, "UI");
            // Get Motion Component
            motion = this.GetComponentInChildren<Animator>();
            // Caps Button Set
            if(btnType == eButtonType.Caps)
                VRIME_KeyboardData.CapsButton = this;
			// WCL Extend Button Set
            if(btnType == eButtonType.WCLExtend)
                VRIME_KeyboardData.WCLExtendButton = this;
            // FunctionSet
            GetUIType();
            SetUIEvent();
        }
        /// <summary>
        /// Set button evnet listener active.
        /// </summary>
        /// <param name="iEnable"></param>
        public void SetButtonEvent(bool iEnable)
        {
            if(buttonEvent == null)
                return;
            buttonEvent.onEnter.RemoveListener(ButtonEnter);
            buttonEvent.onStay.RemoveListener(ButtonEnter);
            buttonEvent.onExit.RemoveListener(ButtonExit);
            if(iEnable)
            {
                buttonEvent.onEnter.AddListener(ButtonEnter);
                buttonEvent.onExit.AddListener(ButtonExit);
                buttonEvent.onStay.AddListener(ButtonStay);
            }
            // If enable is false, stop value is true for stop evnet anime/audio.
            stopEventAudio = !iEnable;
            stopEventAnime = !iEnable;
        }
        #endregion
        #region private Function
        private void GetUIType()
        {
            if(rootUI == null)
                return;

            txtKeyChar = rootUI.GetComponentInChildren<Text>();
            if(txtKeyChar != null)
            {
                uiType = eUIType.Text;
                showingWord = txtKeyChar.text;
                return;
            }
            tmproKeyChar = rootUI.GetComponentInChildren<TextMeshPro>();
            if(tmproKeyChar != null)
            {
                uiType = eUIType.TMPro;
                showingWord = tmproKeyChar.text;
                return;
            }
            imgKeyChar = rootUI.GetComponentInChildren<Image>();
            if(imgKeyChar != null)
            {
                uiType = eUIType.Image;
                // Not Think Done Word Well Be
                return;
            }
            uiType = eUIType.None;
        }

        private void SetUIEvent()
        {
            // Add Button Event To Collider
            Collider aTmpPhy = this.GetComponent<Collider>();
            if(aTmpPhy == null) {
                aTmpPhy = this.GetComponentInChildren<Collider>();
            }
            aTmpPhy.isTrigger = true;

            buttonEvent = aTmpPhy.gameObject.GetComponent<VRIME_ButtonEvent>();
            if(buttonEvent == null)
                buttonEvent = aTmpPhy.gameObject.AddComponent<VRIME_ButtonEvent>();
            buttonEvent.ButtonParent = this;
            SetButtonEvent(true);
        }

        private void ButtonEnter(GameObject iObj)
        {
            if(canAction == false)
                return;            
            // Button Event
            DoEnterAction();
            // Save start press time
            startPressTime = Time.time;
        }

        private void ButtonExit(GameObject iObj)
        {
            if(canAction)
            {
                if(longPressStatus)
                    DoLongPressExitAct();
                else
                    DoExitAction();
                // Delay To Block Two Event Run In 1 Sec
                VRIME_KeyboardOversee.Ins.StartCoroutine(DelayAction());
            }
            pressTime = 0f;
        }

        private void ButtonStay(GameObject iObj)
        {
            if(longPressStatus)
                DoLongPress();

            pressTime = Time.time - startPressTime;
        }
        private void DoEnterAction()
        {
            switch(btnType)
            {
                case eButtonType.DeleteAll:
                    VRIME_InputFieldOversee.Ins.ClearInput();
			        VRIME_BISender.Ins.CallActionClick(eActionClickEntrance.clear_text);
                    break;
            }
        }

        private void DoExitAction()
        {
            // Check
            CheckAccentPanelClose();
            // Action
            switch(btnType)
            {
                case eButtonType.Letter:
                case eButtonType.Number:
                    SendInsertWord(showingWord);
                    break;
                case eButtonType.LetterAccent:
                    SendAccentWords(showingWord);
                    break;
                case eButtonType.LetterWCL:
                    VRIME_InputFieldOversee.Ins.WCLButtonSetWords(this);
                    VRIME_BISender.Ins.CallActionClick(eActionClickEntrance.WCL_select);
                    break;
                case eButtonType.Space:
                    SendSpaceWord();
                    break;
                case eButtonType.WCLExtend:
                    stopEventAnime = true;
                    VRIME_InputFieldOversee.Ins.WCLItemFuncCall(this);
                    break;
                case eButtonType.WCLItemFunc:
                    VRIME_InputFieldOversee.Ins.WCLItemFuncCall(this);
                    break;
                case eButtonType.WCLNearKey:
                    VRIME_InputFieldOversee.Ins.WCLButtonNearKey(this);
                    break;
                case eButtonType.Language:
                    VRIME_KeyboardOversee.Ins.LanguageCycleRun();
                    VRIME_BISender.Ins.CallActionClick(eActionClickEntrance.switch_keyboard);
                    break;
                case eButtonType.LanSelect:
                    VRIME_KeyboardOversee.Ins.CallChangeVoiceLang(this);
                    VRIME_BISender.Ins.CallActionClick(eActionClickEntrance.language_wing_select);
                    break;
                case eButtonType.Voice:
                    VoiceAction(false);
                    break;
                case eButtonType.ToolTipPage:
                    VRIME_KeyboardOversee.Ins.StartCoroutine(TooltipAction());
                    break;
                case eButtonType.ResetBoard:
                    VRIME_Manager.Ins.ResetTrackingPos();
                    break;
                case eButtonType.Submit:
                    VRIME_Manager.Ins.SubmitText();
                    break;
                case eButtonType.Delete:
                    DeleteAction(false);
                    break;
                case eButtonType.Caps:
                    CapsAction(false);
                    break;
                case eButtonType.Symbol:
                    SymbolAction();
                    break;
                case eButtonType.Close:
                    VRIME_Manager.Ins.HideIME();
                    VRIME_BISender.Ins.CallActionClick(eActionClickEntrance.keyboard_close);
                    break;
            }
        }

        private void DoLongPress()
        {
            // Check
            CheckAccentPanelClose();
            // Action
            switch(btnType)
            {
                case eButtonType.Letter:
                    AccentPanelAction();                    
                    break;
                case eButtonType.Caps:
                    CapsAction(true);                    
                    break;
                case eButtonType.Delete:
                    DeleteAction(true);
                    break;                
                case eButtonType.Voice:
                    VoiceAction(true);
                    break;
            }
        }
        private void DoLongPressExitAct()
        {
            // Check
            CheckAccentPanelClose();
            // Action
            switch(btnType)
            {
                case eButtonType.Letter:
                case eButtonType.Number:
                    SendInsertWord(showingWord);
                    break;
                case eButtonType.LetterAccent:
                    SendAccentWords(showingWord);
                    break;
                case eButtonType.LetterWCL:
                    VRIME_InputFieldOversee.Ins.WCLButtonSetWords(this);
                    VRIME_BISender.Ins.CallActionClick(eActionClickEntrance.WCL_select);
                    break;
                case eButtonType.Space:
                    SendSpaceWord();
                    break;
                case eButtonType.WCLExtend:
                    stopEventAnime = true;
                    VRIME_InputFieldOversee.Ins.WCLItemFuncCall(this);
                    break;
                case eButtonType.WCLItemFunc:
                    VRIME_InputFieldOversee.Ins.WCLItemFuncCall(this);
                    break;
                case eButtonType.WCLNearKey:
                    VRIME_InputFieldOversee.Ins.WCLButtonNearKey(this);
                    VRIME_BISender.Ins.CallActionClick(eActionClickEntrance.nearKey_select);
                    break;
                case eButtonType.Language:
                    VRIME_KeyboardOversee.Ins.LanguageCycleRun();
                    VRIME_BISender.Ins.CallActionClick(eActionClickEntrance.switch_keyboard);
                    break;
                case eButtonType.LanSelect:
                    VRIME_KeyboardOversee.Ins.CallChangeVoiceLang(this);
                    VRIME_BISender.Ins.CallActionClick(eActionClickEntrance.language_wing_select);
                    break;
                case eButtonType.ToolTipPage:
                    VRIME_KeyboardOversee.Ins.StartCoroutine(TooltipAction());
                    break;
                case eButtonType.ResetBoard:
                    VRIME_Manager.Ins.ResetTrackingPos();
                    break;
                case eButtonType.Submit:
                    VRIME_Manager.Ins.SubmitText();
                    break;
                case eButtonType.Symbol:
                    SymbolAction();
                    break;
                case eButtonType.Close:
                    VRIME_Manager.Ins.HideIME();
                    VRIME_BISender.Ins.CallActionClick(eActionClickEntrance.keyboard_close);
                    break;
            }
        }
        private void SetButtonUIWord()
        {
            switch(uiType)
            {
                case eUIType.TMPro:
                    tmproKeyChar.text = showingWord;
                    break;
                case eUIType.Text:
                    txtKeyChar.text = showingWord;
                    break;
                case eUIType.Image:
                    break;
            }
        }
        private void SendSpaceWord()
        {
            string aSpace = " ";
            if(VRIME_KeyboardSetting.IMELanguage == eLanguage.Zhuyin)
            {
                aSpace = VRIME_KeyboardData.cZhuYinOneTone.ToString();
            }
            // Insert Word Will Close AccentPanel
            VRIME_KeyboardOversee.Ins.CallAccentPanel(false, this);
            // Insert Words                
            SendBaseWords(aSpace, false);
        }
        /// <summary>
        /// I forget space insert is same rule
        /// </summary>
        /// <param name="iWords"></param>
        private void SendInsertWord(string iWords)
        {
            // Check Accent Panel Show Status
            // if AccentShow Word Same To Insert Word
            // Return LongPress Input Word
            if(VRIME_KeyboardOversee.Ins.AccentShow)
            if(VRIME_KeyboardOversee.Ins.AccentShowWord == iWords)
            {
                if(longPressStatus)
                    return;
            }
            // Insert Word Will Close AccentPanel
            VRIME_KeyboardOversee.Ins.CallAccentPanel(false, this);
            // Insert Words                
            SendBaseWords(iWords, false);
        }
        /// <summary>
        /// Change To Input Accent At Can Close
        /// </summary>
        /// <param name="iWords"></param>
        private void SendAccentWords(string iWords)
        {
            VRIME_KeyboardOversee.Ins.CallAccentPanel(false, this);
            SendBaseWords(iWords, true);
            VRIME_BISender.Ins.CallActionClick(eActionClickEntrance.accentPanel_select);
        }
        private void SendBaseWords(string iWords, bool iNeedDelay)
        {
            // TODO: Move this out of this class, but just seeing if it works for now as a quick hack...
            for (int i = 0; i < iWords.Length; i++)
            {
                OnKeyPressed?.Invoke((int)iWords[i]);
            }
            // 2. insert word
            VRIME_InputFieldOversee.Ins.Insert(iWords, iNeedDelay);
            // 3. if caps State == UpperUnLock, set caps = Lower
            if(VRIME_KeyboardOversee.Ins.CapsType == eCapsState.UpperUnLock)
                VRIME_KeyboardOversee.Ins.SetCapsState(eCapsState.Lower);
        }
        private void CapsAction(bool iLongPress)
        {
            if(iLongPress)
            {
                if(VRIME_KeyboardOversee.Ins.CapsType != eCapsState.UpperLock)
                    VRIME_KeyboardOversee.Ins.SetCapsState(eCapsState.UpperLock);
            }
            else
            {
                // Like Get Symbol, but caps have long press event
                int aCapsIndex = (int)VRIME_KeyboardOversee.Ins.CapsType;
                int aNextCap = (aCapsIndex + 1) % 3;

                VRIME_KeyboardOversee.Ins.SetCapsState((eCapsState)aNextCap);
            }
        }
        /// <summary>
        /// Action Delay
        /// </summary>
        /// <returns></returns>
        private IEnumerator DelayAction()
		{
			canAction = false;
			yield return new WaitForSeconds(actionDelayTime);
			canAction = true;
		}
        private IEnumerator TooltipAction()
        {
            VRIME_KeyboardOversee.Ins.TooltipTurnPage();
            yield return new WaitForSeconds(actionDelayTime);
            motion.Play(eAnimBtn.Highlighted.ToString());
        }
        private void DeleteAction(bool iLongPress)
        {
            if(iLongPress) {
                float aIntervalTime = pressTime < deleteLongPressTime ? deleteSlowDelayTime : deleteFastDelayTime;
                Invoke("DeleteLoopAction", aIntervalTime);
            }
            else {
                VRIME_InputFieldOversee.Ins.DeleteButton();
                OnKeyPressed?.Invoke(8);
            }
        }
        private void DeleteLoopAction()
        {
            CancelInvoke("DeleteLoopAction");
            VRIME_InputFieldOversee.Ins.DeleteButton();
            VRIME_KeyAudioOversee.Ins.DeleteLoop();
        }
        private void SymbolAction()
        {
            VRIME_KeyboardOversee.Ins.SetSymbolMode(!VRIME_KeyboardOversee.Ins.SymbolMode);

            VRIME_BISender.Ins.CallActionClick(eActionClickEntrance.switch_symbol);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="iLongPress"></param>
        private void VoiceAction(bool iLongPress)
        {
            VRIME_VoiceOversee.Ins.SetVoiceFunctionKey(this);
            if(iLongPress)
            {
                if(VRIME_BISender.Ins.actionVoiceLanguageOpen == false) {
                    VRIME_BISender.Ins.CallActionClick(eActionClickEntrance.language_wing_open);
                    VRIME_BISender.Ins.actionVoiceLanguageOpen = true;
                }
                VRIME_KeyboardOversee.Ins.CallVoiceLanWing();
                VRIME_VoiceOversee.Ins.UpdateIconState();
                if(VRIME_KeyboardSetting.VoiceInputBlock) {
                    VRIME_BISender.Ins.CallVoiceCancel();
                }
                VRIME_VoiceOversee.Ins.CallVoiceUI(false);
            }
            else
            {
                VRIME_KeyboardOversee.Ins.CallVoiceLanWing(false);   
                if(VRIME_KeyboardSetting.VoiceInputBlock) {
                    VRIME_BISender.Ins.UserActionVoiceKeyClose();
                    VRIME_BISender.Ins.CallVoiceClose();
                } else {
                    VRIME_BISender.Ins.UserActionVoiceKeyOpen();
                    VRIME_BISender.Ins.CallVoiceOpen();
                }
                VRIME_VoiceOversee.Ins.CallVoiceUI();                
            }
        }
        /// <summary>
        /// Other Button Event Close Accent Panel First
        /// Letter or LetterAccent exist, because these have other handle
        /// </summary>
        private void CheckAccentPanelClose()
        {
            bool aNeedClose = VRIME_KeyboardOversee.Ins.AccentShow;
            switch(btnType)
            {
                case eButtonType.Letter:
                case eButtonType.LetterAccent:
                    aNeedClose = false;
                    break;
            }
            // aNeedClose is True as Close Accent Panel
            if(aNeedClose)
                VRIME_KeyboardOversee.Ins.CallAccentPanel(false, this);
        }
        /// <summary>
        /// 
        /// </summary>
        private void AccentPanelAction()
        {
            if(VRIME_KeyboardOversee.Ins.AccentShow) {
                if(VRIME_BISender.Ins.actionAccentShow == false) {
                    VRIME_BISender.Ins.CallActionClick(eActionClickEntrance.accentPanel_open);
                    VRIME_BISender.Ins.actionAccentShow = true;
                }
            }
            else {
                VRIME_BISender.Ins.actionAccentShow = false;
            }
            VRIME_KeyboardOversee.Ins.CallAccentPanel(true, this);
        }
        #endregion
    }
}