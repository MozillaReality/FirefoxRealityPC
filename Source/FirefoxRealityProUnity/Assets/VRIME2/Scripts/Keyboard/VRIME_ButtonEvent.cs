// ========================================================================== //
//  Created: 2019-01-09
// ========================================================================== //
namespace VRIME2
{
    using System;
    using System.Collections;
    using UnityEngine;

    public class VRIME_ButtonEvent : VRIME_BasePhysicsEvent
    {
        #region private field
        #endregion
        #region public/private Field
        public VRIME_KeyboardButton ButtonParent
        {
            get { return kbButton; }
            set { kbButton = value; }
        }
        private  VRIME_KeyboardButton kbButton;
        #endregion
        #region private Field
        private bool blockEnter = false;
        #endregion
        #region unity function
        private void OnDisable() {
          string[] aAnimName =  Enum.GetNames(typeof(eAnimBtn));
          for(int i = 0; i < aAnimName.Length; i ++) {
              kbButton.motion.ResetTrigger(aAnimName[i]);
          }
        }
        #endregion

		#region Override Function
        protected override void ButtonEnter(Collider iController)
        {
            // Sample Block Controller direction
            blockEnter = transform.InverseTransformPoint(iController.transform.position).z > 0f;
            if(blockEnter)
                return;

            if(VRIME_ControllerOversee.Ins.CheckIsSitck(iController) == false)
                return;
            if(VRIME_KeyboardSetting.BlockDrumstick)
                return;
            if(CheckVoiceInputActive())
                return;
            if(onEnter != null)
				onEnter.Invoke(iController.gameObject);
            // Play Animation
            PlayAnimator(eAnimBtn.Highlighted);
            // Set Enter Controller
            VRIME_ControllerOversee.Ins.SetUseController(iController.gameObject);
            // Sound Event
            PlayAudio(eIMEUIStatus.enter_show);
        }
        protected override void ButtonExit(Collider iController)
        {
            if(blockEnter)
                return;

            if(VRIME_ControllerOversee.Ins.CheckIsSitck(iController) == false)
                return;
            if(VRIME_KeyboardSetting.BlockDrumstick)
                return;
            if(CheckVoiceInputActive())
                return;
            if(onExit != null)
				onExit.Invoke(iController.gameObject);
            // Play Animation
            PlayAnimator(eAnimBtn.Released);
            PlayAnimator(eAnimBtn.Normal);
            // Sound Event
            PlayAudio(eIMEUIStatus.exit_hide);
        }
        protected override void ButtonStay(Collider iController)
        {
            if(blockEnter)
                return;

            if(VRIME_ControllerOversee.Ins.CheckIsSitck(iController) == false)
                return;
            if(VRIME_KeyboardSetting.BlockDrumstick)
                return;
            if(CheckVoiceInputActive())
                return;
            if(onStay != null)
				onStay.Invoke(iController.gameObject);
            // Play Animation
            PlayAnimator(eAnimBtn.Pressed);
            // Sound Event
            PlayAudio(eIMEUIStatus.stay_longpress);
        }
		#endregion
        #region public function
        public void ColliderStatus(bool iEnable)
        {
            Collider aCol = this.GetComponent<Collider>();
            aCol.enabled = iEnable;
        }
        
        #endregion
        #region private function        
        /// <summary>
        /// Play Button Anime;
        /// </summary>
        /// <param name="iState"></param>
        private void PlayAnimator(eAnimBtn iState)
        {
            if(VRIME_KeyboardSetting.EnableKeyboraButtondAnimation == false)
                return;
            if(kbButton.stopEventAnime)
                return;

            kbButton.motion.Play(iState.ToString());
        }
        /// <summary>
        /// Stop Button Anime
        /// </summary>
        /// <param name="iState"></param>
        private void StopAnimator(eAnimBtn iState)
        {
            kbButton.motion.ResetTrigger(iState.ToString());
        }
        /// <summary>
        /// Play Normal Anime after anime play done
        /// </summary>
        /// <returns></returns>
        private IEnumerator GetWaitAnimator()
        {
            yield return new WaitForSeconds(kbButton.motion.GetCurrentAnimatorStateInfo(0).length + kbButton.motion.GetCurrentAnimatorStateInfo(0).normalizedTime);
            kbButton.motion.Play(eAnimBtn.Normal.ToString());
        }
        /// <summary>
        /// Play Key Audio
        /// </summary>
        /// <param name="iType"></param>
        private void PlayAudio(eIMEUIStatus iType)
        {
            if(VRIME_KeyboardSetting.EnableKeyboardButtonAudio == false)
                return;
            if(kbButton.stopEventAudio)
                return;
            
            VRIME_KeyAudioOversee.Ins.ButtonEventStatus(iType, kbButton);
        }
        private bool CheckVoiceInputActive()
        {
            bool aBlock = false;
            switch(kbButton.btnType)
            {
                case eButtonType.Letter:
                case eButtonType.Caps:
                case eButtonType.Symbol:
                case eButtonType.Space:
                case eButtonType.Number:
                case eButtonType.Delete:
                case eButtonType.Language:
                case eButtonType.ResetBoard:
                case eButtonType.Submit:
                    // Block Input Change To Cancel Voice Input
                    if(VRIME_VoiceOversee.Ins != null) {
                        if(VRIME_KeyboardSetting.VoiceInputBlock) {
                            VRIME_BISender.Ins.CallVoiceCancel();
                            VRIME_VoiceOversee.Ins.CallVoiceUI(false);                            
                        }
                    }
                    break;
            }
            return aBlock;
        }
        #endregion

    }
}