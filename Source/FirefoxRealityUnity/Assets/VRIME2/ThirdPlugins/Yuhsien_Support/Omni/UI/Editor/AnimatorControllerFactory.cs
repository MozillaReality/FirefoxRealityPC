// ========================================================================== //
//
//  class AnimationControllerFactory
//  -----
//  Purpose: Factory for creating AnimatorControllers with embedded AnimationClips
//
//
//  Created: 2017-07-17
//  Updated: 2017-07-17
//
//  Copyright 2018 HTC America Innovation
// 
// ========================================================================== //
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections;

namespace Htc.Omni
{
    public static class AnimatorControllerFactory
    {
        public static AnimatorController CreateTriggerableController(ICollection triggers, string defaultControllerName = null)
        {
            string saveControllerPath = GetSaveControllerPath(defaultControllerName);

            if (!string.IsNullOrEmpty(saveControllerPath))
            {
                var controller = AnimatorController.CreateAnimatorControllerAtPath(saveControllerPath);

                var stateMachine = controller.layers[0].stateMachine;

                foreach (string trigger in triggers)
                {
                    controller.AddParameter(trigger, AnimatorControllerParameterType.Trigger);

                    var state = AddStateWithClip(controller, trigger);
                    var transition = stateMachine.AddAnyStateTransition(state);
                    transition.AddCondition(AnimatorConditionMode.If, 0f, trigger);
                }

                AssetDatabase.ImportAsset(saveControllerPath);

                return controller;
            }
            else
            {
                return null;
            }
        }

        public static AnimatorController CreateBinaryController(string parameterName, string offStateName = "Off", string onStateName = "On", string defaultControllerName = null)
        {
            string saveControllerPath = GetSaveControllerPath(defaultControllerName);

            if (!string.IsNullOrEmpty(saveControllerPath))
            {
                var controller = AnimatorController.CreateAnimatorControllerAtPath(saveControllerPath);
                controller.AddParameter(parameterName, AnimatorControllerParameterType.Bool);

                var offState = AddStateWithClip(controller, offStateName);
                var onState = AddStateWithClip(controller, onStateName);

                var stateMachine = controller.layers[0].stateMachine;
                var onTransition = offState.AddTransition(onState);
                onTransition.AddCondition(AnimatorConditionMode.If, 0, parameterName);

                var offTransition = onState.AddTransition(offState);
                offTransition.AddCondition(AnimatorConditionMode.IfNot, 0, parameterName);

                AssetDatabase.ImportAsset(saveControllerPath);

                return controller;
            }
            else
            {
                return null;
            }
        }

        private static string GetSaveControllerPath(string defaultName = null)
        {
            string name = string.IsNullOrEmpty(defaultName) ? "Animation" : defaultName;
            string message = string.Format("Create a new animator for the game object '{0}':", name);
            return EditorUtility.SaveFilePanelInProject("New Animation Contoller", name, "controller", message);
        }

        private static AnimatorState AddStateWithClip(AnimatorController controller, string name)
        {
            AnimationClip animationClip = AnimatorController.AllocateAnimatorClip(name);
            AssetDatabase.AddObjectToAsset(animationClip, controller);
            AnimatorState state = controller.AddMotion(animationClip);
            return state;
        }
    }
}