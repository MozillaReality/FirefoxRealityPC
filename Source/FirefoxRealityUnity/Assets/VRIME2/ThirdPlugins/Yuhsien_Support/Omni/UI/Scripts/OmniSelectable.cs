// ========================================================================== //
//
//  class OmniSelectable
//  -----
//  Purpose: Base class of all Omni Selectables
//
//  Usage: Enables SelectionState transitions similar to Ui.Selectable
//      The biggest addition is the onSelectionStateTransition event
//      which allows arbitrary components to listen to transition events
//
//      Derive from SelectableTransitionHandler to create animation components
//      that are designer-friendly.
//
//
//  Created: 2018-10-25
//  Updated: 2018-11-26
//
//  Copyright 2018 HTC America Innovation
// 
// ========================================================================== //
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

namespace Htc.Omni
{
    [DisallowMultipleComponent]
    [SelectionBase]
    public class OmniSelectable : UIBehaviour, IMoveHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IEventSystemHandler
    {
        [System.Serializable]
        public class StateTransitionEvent : UnityEvent<SelectionState, bool> { }

        public enum Transition
        {
            None = 0,
            Animation = 1,
            Event = 2
        }

        public enum SelectionState
        {
            Normal = 0,
            Highlighted = 1,
            Pressed = 2,
            Disabled = 3
        }

        [SerializeField]
        private bool _interactable = true;
        public bool interactable
        {
            get { return _interactable; }
            set
            {
                if (_interactable != value)
                {
                    _interactable = value;
                    pressed = false;
                    highlighted = false;
                    var state = value ? SelectionState.Normal : SelectionState.Disabled;
                    DoStateTransition(state, false);
                }
            }
        }

        [SerializeField]
        private Transition _transition;
        public Transition transition { get { return _transition; } }

        [SerializeField]
        private UnityEngine.UI.AnimationTriggers _animationTriggers;
        public UnityEngine.UI.AnimationTriggers animationTriggers { get { return _animationTriggers; } }

        private Animator _animator;
        public Animator animator { get { if (_animator == null) _animator = GetComponent<Animator>(); return _animator; } }

        [SerializeField]
        private StateTransitionEvent onSelectionStateTransitionPersistent;

        // This event is invoked regardless of transition setting
        public event Action<SelectionState, bool> onSelectionStateTransition;

        public SelectionState selectionState { get; private set; }
        protected bool highlighted { get; private set; }
        protected bool pressed { get; private set; }

        protected bool isActiveAndInteractable { get { return isActiveAndEnabled && interactable; } }

        public virtual void OnMove(AxisEventData eventData)
        {
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (interactable)
            {
                pressed = true;
                selectionState = SelectionState.Pressed;
                DoStateTransition(selectionState, !Application.isPlaying);
            }
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (interactable)
            {
                pressed = false;
                selectionState = highlighted ? SelectionState.Highlighted : SelectionState.Normal;
                DoStateTransition(selectionState, !Application.isPlaying);
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (interactable)
            {
                highlighted = true;
                selectionState = pressed ? SelectionState.Pressed : SelectionState.Highlighted;
                DoStateTransition(selectionState, !Application.isPlaying);
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (interactable)
            {
                highlighted = false;
                selectionState = SelectionState.Normal;
                DoStateTransition(selectionState, !Application.isPlaying);
            }
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
        }

        protected virtual void DoStateTransition(SelectionState state, bool instant)
        {
            switch (transition)
            {
                case Transition.Animation:
                    if (animator != null)
                    {
                        SetOrResetTrigger(animationTriggers.normalTrigger, state == SelectionState.Normal);
                        SetOrResetTrigger(animationTriggers.highlightedTrigger, state == SelectionState.Highlighted);
                        SetOrResetTrigger(animationTriggers.pressedTrigger, state == SelectionState.Pressed);
                        SetOrResetTrigger(animationTriggers.disabledTrigger, state == SelectionState.Disabled);
                    }

                    break;
                case Transition.Event:
                    onSelectionStateTransitionPersistent.Invoke(state, instant);
                    break;
            }

            if (onSelectionStateTransition != null)
                onSelectionStateTransition.Invoke(state, instant);
        }

        private void SetOrResetTrigger(string trigger, bool set)
        {
            if (!string.IsNullOrEmpty(trigger))
            {
                if (set)
                    animator.SetTrigger(trigger);
                else
                    animator.ResetTrigger(trigger);
            }
        }
    }
}