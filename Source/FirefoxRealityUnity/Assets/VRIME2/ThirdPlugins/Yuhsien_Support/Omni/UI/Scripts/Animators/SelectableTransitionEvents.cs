// ========================================================================== //
//
//  class SelectableTransitionEvents
//  -----
//  Purpose: Exposes OmniSelectable transition events
//
//
//  Created: 2018-10-25
//  Updated: 2018-11-13
//
//  Copyright 2018 HTC America Innovation
// 
// ========================================================================== //
using UnityEngine;
using UnityEngine.Events;

namespace Htc.Omni
{
    [RequireComponent(typeof(OmniSelectable))]
    public class SelectableTransitionEvents : SelectableTransitionHandler
    {
        [System.Serializable]
        public class TransitionEvent : UnityEvent<bool> { };

        [SerializeField]
        private TransitionEvent _onNormal;
        public TransitionEvent onNormal { get { return _onNormal; } }

        [SerializeField]
        private TransitionEvent _onHighlight;
        public TransitionEvent onHighlight { get { return _onHighlight; } }

        [SerializeField]
        private TransitionEvent _onPress;
        public TransitionEvent onPress { get { return _onPress; } }

        [SerializeField]
        private TransitionEvent _onDisable;
        public TransitionEvent onDisable { get { return _onDisable; } }

        private void OnTransition(OmniSelectable.SelectionState state, bool instant)
        {
            switch (state)
            {
                case OmniSelectable.SelectionState.Normal:
                    if (onNormal != null)
                        onNormal.Invoke(instant);
                    break;
                case OmniSelectable.SelectionState.Highlighted:
                    if (onHighlight != null)
                        onHighlight.Invoke(instant);
                    break;
                case OmniSelectable.SelectionState.Pressed:
                    if (onPress != null)
                        onPress.Invoke(instant);
                    break;
                case OmniSelectable.SelectionState.Disabled:
                    if (onDisable != null)
                        onDisable.Invoke(instant);
                    break;
            }
        }

        protected override void OnSelectionStateTransition(OmniSelectable.SelectionState state, bool instant)
        {
        }
    }
}