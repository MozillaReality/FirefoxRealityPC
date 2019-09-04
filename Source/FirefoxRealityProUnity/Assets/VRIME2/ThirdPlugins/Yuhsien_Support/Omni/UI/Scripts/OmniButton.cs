// ========================================================================== //
//
//  class OmniButton
//  -----
//  Purpose: Button class based on OmniSelectable
//
//  Based on: https://bitbucket.org/Unity-Technologies/ui/src/2017.3/UnityEngine.UI/UI/Core/Button.cs
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
using System.Collections;

namespace Htc.Omni
{
    public class OmniButton : OmniSelectable, IPointerClickHandler, ISubmitHandler, IEventSystemHandler
    {
        [SerializeField]
        private UnityEvent _onClick;
        public UnityEvent onClick { get { return _onClick; } }

        private void Press()
        {
            if (!isActiveAndInteractable)
                return;

            UISystemProfilerApi.AddMarker("OmniButton.onClick", this);
            onClick.Invoke();
        }

        // Trigger all registered callbacks.
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            Press();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            Press();

            // if we get set disabled during the press
            // don't run the coroutine.
            if (!isActiveAndInteractable)
                return;

            DoStateTransition(SelectionState.Pressed, false);
            StartCoroutine(OnFinishSubmit());
        }

        private IEnumerator OnFinishSubmit()
        {
            yield return new WaitForSecondsRealtime(0.2f);
            DoStateTransition(selectionState, false);
        }
    }
}