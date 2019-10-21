// ========================================================================== //
//
//  class OmniToggleEditor
//  -----
//  Purpose: Custom editor for OmniToggle
//
//
//  Created: 2018-10-25
//  Updated: 2018-10-25
//
//  Copyright 2018 HTC America Innovation
// 
// ========================================================================== //
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;

namespace Htc.Omni
{
    [CustomEditor(typeof(OmniToggle))]
    [CanEditMultipleObjects]
    public class OmniToggleEditor : OmniSelectableEditor
    {
        private SerializedProperty isOn;
        private SerializedProperty toggleTransition;
        private SerializedProperty onValueChanged;

        private SerializedProperty toggleGameObject;
        private SerializedProperty toggleAnimator;
        private SerializedProperty toggleAnimationParameter;
        private SerializedProperty onToggleTransition;

        private SerializedProperty group;

        private AnimBool showToggleActiveTransition = new AnimBool();
        private AnimBool showToggleAnimTransition = new AnimBool();
        private AnimBool showToggleEventTransition = new AnimBool();

        protected override void OnEnable()
        {
            base.OnEnable();
            isOn = serializedObject.FindProperty("_isOn");
            toggleTransition = serializedObject.FindProperty("_toggleTransition");
            onValueChanged = serializedObject.FindProperty("_onValueChanged");

            toggleGameObject = serializedObject.FindProperty("_toggleGameObject");
            toggleAnimator = serializedObject.FindProperty("_toggleAnimator");
            toggleAnimationParameter = serializedObject.FindProperty("_toggleAnimationParameter");
            onToggleTransition = serializedObject.FindProperty("onToggleTransitionPersistent");

            group = serializedObject.FindProperty("_group");

            var trans = (OmniToggle.ToggleTransition)toggleTransition.enumValueIndex;
            showToggleActiveTransition.value = (!toggleTransition.hasMultipleDifferentValues && trans == OmniToggle.ToggleTransition.SetActive);
            showToggleAnimTransition.value = (!toggleTransition.hasMultipleDifferentValues && trans == OmniToggle.ToggleTransition.Animation);
            showToggleEventTransition.value = (!toggleTransition.hasMultipleDifferentValues && trans == OmniToggle.ToggleTransition.Event);
        }

        public override bool RequiresConstantRepaint()
        {
            return base.RequiresConstantRepaint()
                || showToggleActiveTransition.isAnimating
                || showToggleAnimTransition.isAnimating
                || showToggleEventTransition.isAnimating;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            EditorGUILayout.PropertyField(isOn);
            EditorGUILayout.PropertyField(toggleTransition);

            var trans = (OmniToggle.ToggleTransition)toggleTransition.enumValueIndex;
            showToggleActiveTransition.target = (!toggleTransition.hasMultipleDifferentValues && trans == OmniToggle.ToggleTransition.SetActive);
            showToggleAnimTransition.target = (!toggleTransition.hasMultipleDifferentValues && trans == OmniToggle.ToggleTransition.Animation);
            showToggleEventTransition.target = (!toggleTransition.hasMultipleDifferentValues && trans == OmniToggle.ToggleTransition.Event);

            EditorGUI.indentLevel++;

            if (EditorGUILayout.BeginFadeGroup(showToggleActiveTransition.faded))
            {
                EditorGUILayout.PropertyField(toggleGameObject, new GUIContent("Game Object", toggleGameObject.tooltip));
            }
            EditorGUILayout.EndFadeGroup();

            if (EditorGUILayout.BeginFadeGroup(showToggleAnimTransition.faded))
            {
                EditorGUILayout.PropertyField(toggleAnimator, new GUIContent("Animator", toggleAnimator.tooltip));
                EditorGUILayout.PropertyField(toggleAnimationParameter, new GUIContent("Parameter", toggleAnimationParameter.tooltip));

                var anim = toggleAnimator.objectReferenceValue as Animator;

                if (anim != null && anim.runtimeAnimatorController == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(" ");

                    if (GUILayout.Button("Auto Generate Animation", EditorStyles.miniButton))
                    {
                        // Validate animation triggers
                        var parameterName = toggleAnimationParameter.stringValue;

                        // triggerNames[0] = toggleAnimationTriggers.FindPropertyRelative("offTrigger").stringValue;
                        // triggerNames[1] = toggleAnimationTriggers.FindPropertyRelative("onTrigger").stringValue;

                        if (!string.IsNullOrEmpty(parameterName))
                        {
                            var controller = AnimatorControllerFactory.CreateBinaryController(parameterName, "Off", "On", target.name);

                            if (controller != null)
                                anim.runtimeAnimatorController = controller;
                        }
                        else
                        {
                            Debug.LogError("Toggle transition animation parameter name cannot be blank");
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
                else if (anim == null)
                {
                    EditorGUILayout.HelpBox("Assign an animator to auto generate animation", MessageType.Info);
                }
            }
            EditorGUILayout.EndFadeGroup();

            if (EditorGUILayout.BeginFadeGroup(showToggleEventTransition.faded))
            {
                EditorGUILayout.PropertyField(onToggleTransition, new GUIContent("On Toggle Transition", onToggleTransition.tooltip));
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(group);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(onValueChanged);
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
        }
    }
}