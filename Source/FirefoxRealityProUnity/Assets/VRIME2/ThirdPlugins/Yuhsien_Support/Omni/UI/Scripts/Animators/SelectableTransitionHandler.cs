// ========================================================================== //
//
//  class SelectableTransitionHandler
//  -----
//  Purpose: Base class that handles OmniSelectable state transition events
//
//
//  Created: 2018-11-14
//  Updated: 2018-11-14
//
//  Copyright 2018 HTC America Innovation
// 
// ========================================================================== //
using UnityEngine;

namespace Htc.Omni
{
    [RequireComponent(typeof(OmniSelectable))]
    public abstract class SelectableTransitionHandler : BaseBehaviour
    {
        // Required Component getters
        private OmniSelectable _selectable;
        private OmniSelectable selectable { get { if (_selectable == null) _selectable = GetComponent<OmniSelectable>(); return _selectable; } }

        // Empty Start() to make the enabled checkbox visible
        protected virtual void Start() { }

        protected virtual void OnEnable()
        {
            selectable.onSelectionStateTransition += OnSelectionStateTransition;
        }

        protected virtual void OnDisable()
        {
            selectable.onSelectionStateTransition -= OnSelectionStateTransition;
        }

        protected abstract void OnSelectionStateTransition(OmniSelectable.SelectionState state, bool instant);
    }
}