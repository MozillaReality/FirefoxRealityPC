// ========================================================================== //
//
//  class OmniToggle
//  -----
//  Purpose: Toggle class based on OmniSelectable
//
//  Usage: Added Animation option to toggle transitions.
//      Also, the onToggleTransition event allows arbitrary components to
//      listen to transition events.
//
//  Based on: https://bitbucket.org/Unity-Technologies/ui/src/2017.3/UnityEngine.UI/UI/Core/Toggle.cs
//
//  Created: 2018-10-25
//  Updated: 2018-10-25
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
    public class OmniToggle : OmniSelectable, IPointerClickHandler, ISubmitHandler, IEventSystemHandler
    {
        [System.Serializable]
        public class ValueChangedEvent : UnityEvent<bool> { };

        [System.Serializable]
        public class ToggleTransitionEvent : UnityEvent<bool, bool> { }

        public enum ToggleTransition
        {
            None = 0,
            SetActive = 1,
            Animation = 2,
            Event = 3
        }

        [SerializeField]
        [Tooltip("Is the toggle currently on or off?")]
        private bool _isOn;
        /// <summary>
        /// Whether the toggle is currently active.
        /// </summary>
        public bool isOn
        {
            get { return _isOn; }
            set { Set(value); }
        }

        // Transition
        [SerializeField]
        [Tooltip("Type of transition to execute")]
        private ToggleTransition _toggleTransition;
        public ToggleTransition toggleTransition { get { return _toggleTransition; } }

        [SerializeField]
        [Tooltip("The GameObject which its active state is to be toggled")]
        private GameObject _toggleGameObject;
        public GameObject toggleGameObject
        {
            get { return _toggleGameObject; }
            set { _toggleGameObject = value; }
        }

        [SerializeField]
        [Tooltip("The bool parameter in the animator controller to be toggled")]
        private Animator _toggleAnimator;
        public Animator toggleAnimator
        {
            get { return _toggleAnimator; }
            set { _toggleAnimator = value; }
        }

        [SerializeField]
        [Tooltip("The bool parameter in the animator controller to be toggled")]
        private string _toggleAnimationParameter = "IsOn";
        public string toggleAnimationParameter
        {
            get { return _toggleAnimationParameter; }
            set { _toggleAnimationParameter = value; }
        }

        [SerializeField]
        private ToggleTransitionEvent onToggleTransitionPersistent;

        // This event is invoked regardless of transition setting
        public event Action<bool, bool> onToggleTransition;

        [SerializeField]
        private ValueChangedEvent _onValueChanged;
        public ValueChangedEvent onValueChanged { get { return _onValueChanged; } }

        // group that this toggle can belong to
        [SerializeField]
        private OmniToggleGroup _group;
        public OmniToggleGroup group
        {
            get { return _group; }
            set
            {
                _group = value;
#if UNITY_EDITOR
                if (Application.isPlaying)
#endif
                {
                    SetToggleGroup(value, true);
                    PlayEffect(true);
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetToggleGroup(group, false);
            PlayEffect(true);
        }

        protected override void OnDisable()
        {
            SetToggleGroup(null, false);
            base.OnDisable();
        }

        private void SetToggleGroup(OmniToggleGroup newGroup, bool setMemberValue)
        {
            OmniToggleGroup oldGroup = group;

            // Sometimes IsActive returns false in OnDisable so don't check for it.
            // Rather remove the toggle too often than too little.
            if (group != null)
                group.UnregisterToggle(this);

            // At runtime the group variable should be set but not when calling this method from OnEnable or OnDisable.
            // That's why we use the setMemberValue parameter.
            if (setMemberValue)
                group = newGroup;

            // Only register to the new group if this Toggle is active.
            if (newGroup != null && isActiveAndEnabled)
                newGroup.RegisterToggle(this);

            // If we are in a new group, and this toggle is on, notify group.
            // Note: Don't refer to group here as it's not guaranteed to have been set.
            if (newGroup != null && newGroup != oldGroup && isOn && isActiveAndEnabled)
                newGroup.NotifyToggleOn(this);
        }

        void Set(bool value)
        {
            Set(value, true);
        }

        void Set(bool value, bool sendCallback)
        {
            if (_isOn == value)
                return;

            // if we are in a group and set to true, do group logic
            _isOn = value;
            if (group != null && isActiveAndEnabled)
            {
                if (_isOn || (!group.AnyTogglesOn() && !group.allowSwitchOff))
                {
                    _isOn = true;
                    group.NotifyToggleOn(this);
                }
            }

            // Always send event when toggle is clicked, even if value didn't change
            // due to already active toggle in a toggle group being clicked.
            // Controls like Dropdown rely on this.
            // It's up to the user to ignore a selection being set to the same value it already was, if desired.
            PlayEffect(toggleTransition == ToggleTransition.None);
            if (sendCallback)
            {
                UISystemProfilerApi.AddMarker("Toggle.value", this);
                onValueChanged.Invoke(_isOn);
            }
        }

        /// <summary>
        /// Play the appropriate effect.
        /// </summary>
        private void PlayEffect(bool instant)
        {
            switch (toggleTransition)
            {
                case ToggleTransition.SetActive:
                    if (toggleGameObject != null)
                        toggleGameObject.SetActive(isOn);
                    break;
                case ToggleTransition.Animation:
                    if (toggleAnimator != null && !string.IsNullOrEmpty(toggleAnimationParameter))
                        toggleAnimator.SetBool(toggleAnimationParameter, isOn);
                    break;
                case ToggleTransition.Event:
                    onToggleTransitionPersistent.Invoke(isOn, false);
                    break;
            }

            if (onToggleTransition != null)
                onToggleTransition(isOn, false);
        }

        /// <summary>
        /// Assume the correct visual state.
        /// </summary>
        protected override void Start()
        {
            PlayEffect(true);
        }

        private void InternalToggle()
        {
            if (!isActiveAndInteractable)
                return;

            isOn = !isOn;
        }

        /// <summary>
        /// React to clicks.
        /// </summary>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            InternalToggle();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            InternalToggle();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            PlayEffect(true);
        }
#endif
    }
}