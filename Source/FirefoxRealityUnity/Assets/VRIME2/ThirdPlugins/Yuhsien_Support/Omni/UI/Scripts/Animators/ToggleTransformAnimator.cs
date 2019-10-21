// ========================================================================== //
//
//  class ToggleTransformAnimator
//  -----
//  Purpose: Manipulates target transform w.r.t. toggle transition events
//
//
//  Created: 2018-11-16
//  Updated: 2018-11-16
//
//  Copyright 2018 HTC America Innovation
// 
// ========================================================================== //
using UnityEngine;
using System.Collections;

namespace Htc.Omni
{
    public class ToggleTransformAnimator : ToggleTransitionHandler
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
        TransformOverride offTransform;

        [SerializeField]
        TransformOverride onTransform;

        private IEnumerator transitionCoroutine;

        protected override void DoToggleTransition(bool isOn, bool instant)
        {
            if (isOn && onTransform != null && onTransform.active)
                DoTransition(onTransform, instant);
            else if (!isOn && offTransform != null && offTransform.active)
                DoTransition(offTransform, instant);
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