// ========================================================================== //
//
//  class SelectableTransitionTransformAnimator
//  -----
//  Purpose: Manipulates target transform w.r.t. OmniSelectable transition events
//
//
//  Created: 2018-11-14
//  Updated: 2018-11-14
//
//  Copyright 2018 HTC America Innovation
// 
// ========================================================================== //
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace Htc.Omni
{
    [RequireComponent(typeof(OmniSelectable))]
    public class SelectableTransitionTransformAnimator : SelectableTransitionHandler
    {
        [SerializeField]
        private Transform target;

        [Space]
        [SerializeField]
        [Range(0, 1)]
        private float transitionDuration = 0.2f;

        [SerializeField]
        private AnimationCurve transitionCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Space]
        [SerializeField]
        private TransformOverride normalTransform;

        [SerializeField]
        private TransformOverride highlightedTransform;

        [SerializeField]
        private TransformOverride pressedTransform;

        [SerializeField]
        private TransformOverride disabledTransform;

        private IEnumerator transitionCoroutine;

        protected override void OnSelectionStateTransition(OmniSelectable.SelectionState state, bool instant)
        {
            switch (state)
            {
                case OmniSelectable.SelectionState.Normal:
                    DoTransition(normalTransform, instant);
                    break;
                case OmniSelectable.SelectionState.Highlighted:
                    DoTransition(highlightedTransform, instant);
                    break;
                case OmniSelectable.SelectionState.Pressed:
                    DoTransition(pressedTransform, instant);
                    break;
                case OmniSelectable.SelectionState.Disabled:
                    DoTransition(disabledTransform, instant);
                    break;
            }
        }

        private void DoTransition(TransformOverride transformOverride, bool instant)
        {
            if (target == null)
                return;

            if (instant)
            {
                target.localPosition = transformOverride.position;
                target.localEulerAngles = transformOverride.rotation;
                target.localScale = transformOverride.scale;
            }
            else
            {
                if (transitionCoroutine != null)
                    StopCoroutine(transitionCoroutine);

                transitionCoroutine = DoTransitionAnimation(transformOverride);
                StartCoroutine(transitionCoroutine);
            }
        }

        private IEnumerator DoTransitionAnimation(TransformOverride transformOverride)
        {
            var time = 0f;
            var startPosition = target.localPosition;
            var startRotation = target.localRotation;
            var startScale = target.localScale;

            var targetPosition = transformOverride.position;
            var targetRotation = Quaternion.Euler(transformOverride.rotation);
            var targetScale = transformOverride.scale;

            while (time < transitionDuration)
            {
                var t = transitionCurve.Evaluate(time / transitionDuration);
                target.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
                target.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);
                target.localScale = Vector3.Lerp(startScale, targetScale, t);

                time += Time.deltaTime;
                yield return null;
            }

            target.localPosition = targetPosition;
            target.localRotation = targetRotation;
            target.localScale = targetScale;

            transitionCoroutine = null;
        }
    }
}