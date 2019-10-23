// ========================================================================== //
//
//  class OmniSelectableEditor
//  -----
//  Purpose: Custom editor for OmniSelectable
//
//
//  Created: 2018-10-25
//  Updated: 2018-10-25
//
//  Copyright 2018 HTC America Innovation
// 
// ========================================================================== //
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.AnimatedValues;
using UnityEngine;
using System.Collections.Generic;

namespace Htc.Omni
{
    [CustomEditor(typeof(OmniSelectable))]
    //[CanEditMultipleObjects]
    public class OmniSelectableEditor : Editor
    {
        private SerializedProperty interactable;
        private SerializedProperty transition;

        private SerializedProperty animationTriggers;
        private SerializedProperty onSelectionStateTransition;

        private AnimBool showAnimTransition = new AnimBool();
        private AnimBool showEventTransition = new AnimBool();

        protected virtual void OnEnable()
        {
            interactable = serializedObject.FindProperty("_interactable");
            transition = serializedObject.FindProperty("_transition");
            animationTriggers = serializedObject.FindProperty("_animationTriggers");
            onSelectionStateTransition = serializedObject.FindProperty("onSelectionStateTransitionPersistent");

            var trans = (OmniSelectable.Transition)transition.enumValueIndex;
            showAnimTransition.value = (!transition.hasMultipleDifferentValues && trans == OmniSelectable.Transition.Animation);
            showEventTransition.value = (!transition.hasMultipleDifferentValues && trans == OmniSelectable.Transition.Event);
        }

        public override bool RequiresConstantRepaint()
        {
            return base.RequiresConstantRepaint() || showAnimTransition.isAnimating || showEventTransition.isAnimating;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            EditorGUILayout.PropertyField(interactable);
            EditorGUILayout.PropertyField(transition);


            var trans = (OmniSelectable.Transition)transition.enumValueIndex;
            showAnimTransition.target = (!transition.hasMultipleDifferentValues && trans == OmniSelectable.Transition.Animation);
            showEventTransition.target = (!transition.hasMultipleDifferentValues && trans == OmniSelectable.Transition.Event);

            EditorGUI.indentLevel++;

            if (EditorGUILayout.BeginFadeGroup(showAnimTransition.faded))
            {
                EditorGUILayout.PropertyField(animationTriggers);

                var anim = (target as MonoBehaviour).GetComponent<Animator>();

                if (anim == null || anim.runtimeAnimatorController == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(" ");

                    if (GUILayout.Button("Auto Generate Animation", EditorStyles.miniButton))
                    {
                        // Validate animation triggers
                        var triggerNames = new string[4];

                        triggerNames[0] = animationTriggers.FindPropertyRelative("m_NormalTrigger").stringValue;
                        triggerNames[1] = animationTriggers.FindPropertyRelative("m_HighlightedTrigger").stringValue;
                        triggerNames[2] = animationTriggers.FindPropertyRelative("m_PressedTrigger").stringValue;
                        triggerNames[3] = animationTriggers.FindPropertyRelative("m_DisabledTrigger").stringValue;

                        var hasInvalidName = false;
                        if (string.IsNullOrEmpty(triggerNames[0]))
                        {
                            Debug.LogError("Normal Trigger name cannot be blank");
                            hasInvalidName = true;
                        }
                        else if (string.IsNullOrEmpty(triggerNames[1]))
                        {
                            Debug.LogError("Highlighted Trigger name cannot be blank");
                            hasInvalidName = true;
                        }
                        else if (string.IsNullOrEmpty(triggerNames[2]))
                        {
                            Debug.LogError("Pressed Trigger name cannot be blank");
                            hasInvalidName = true;
                        }
                        else if (string.IsNullOrEmpty(triggerNames[3]))
                        {
                            Debug.LogError("Disabled Trigger name cannot be blank");
                            hasInvalidName = true;
                        }

                        if (!hasInvalidName)
                        {
                            var controller = AnimatorControllerFactory.CreateTriggerableController(triggerNames, target.name);

                            if (controller != null)
                            {
                                if (anim == null)
                                    anim = (target as MonoBehaviour).gameObject.AddComponent<Animator>();

                                anim.runtimeAnimatorController = controller;
                            }
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFadeGroup();

            if (EditorGUILayout.BeginFadeGroup(showEventTransition.faded))
            {
                EditorGUILayout.PropertyField(onSelectionStateTransition, new GUIContent("On Transition", onSelectionStateTransition.tooltip));
            }
            EditorGUILayout.EndFadeGroup();


            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
        }

        private void ValidateAnimationTriggers()
        {
            var triggers = animationTriggers.Copy();
            var strs = new List<string>();
            if (triggers.NextVisible(true))
            {
                while (triggers.NextVisible(false))
                    strs.Add(triggers.stringValue);
            }
        }
    }
}