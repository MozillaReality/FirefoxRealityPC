// ========================================================================== //
//
//  class ToggleTransitionHandler
//  -----
//  Purpose: Base class that handles OmniToggle toggle transition events
//
//
//  Created: 2018-11-16
//  Updated: 2018-11-16
//
//  Copyright 2018 HTC America Innovation
// 
// ========================================================================== //
using UnityEngine;

namespace Htc.Omni
{
    [ExecuteInEditMode]
    public abstract class ToggleTransitionHandler : BaseBehaviour
    {
        [SerializeField]
        private bool _isOn;
        public bool isOn
        {
            get { return _isOn; }
            set { OnToggleTransition(value, false); }
        }

        private OmniToggle toggle;

        private void Awake()
        {
            if (toggle == null)
                toggle = GetComponent<OmniToggle>();
        }

        protected virtual void OnEnable()
        {
            if (toggle != null)
            {
                _isOn = toggle.isOn;
                toggle.onToggleTransition += OnToggleTransition;
            }

            DoToggleTransition(isOn, true);
        }

        protected virtual void OnDisable()
        {
            if (toggle != null)
                toggle.onToggleTransition -= OnToggleTransition;
        }

        public void OnToggleTransition(bool isOn, bool instant)
        {
            if (_isOn != isOn)
            {
                _isOn = isOn;
                if (isActiveAndEnabled)
                {
#if UNITY_EDITOR
                    DoToggleTransition(isOn, Application.isPlaying ? instant : true);
#else
                    DoToggleTransition(isOn, instant);
#endif
                }
            }
        }

        protected abstract void DoToggleTransition(bool isOn, bool instant);

        void OnValidate()
        {
            if (isActiveAndEnabled)
                DoToggleTransition(isOn, true);
        }
    }
}