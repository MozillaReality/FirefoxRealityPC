// ========================================================================== //
//
//  class OmniSliderEditor
//  -----
//  Purpose: Custom editor for OmniSlider
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

namespace Htc.Omni
{
    [CustomEditor(typeof(OmniSlider))]
    [CanEditMultipleObjects]
    public class OmniSliderEditor : OmniSelectableEditor
    {
        private SerializedProperty length;
        private SerializedProperty minValue;
        private SerializedProperty maxValue;
        private SerializedProperty wholeNumbers;
        private SerializedProperty value;
        private SerializedProperty onValueChanged;
        private SerializedProperty fillBox;
        private SerializedProperty background;
        private SerializedProperty fill;
        private SerializedProperty handle;
        private SerializedProperty direction;

        protected override void OnEnable()
        {
            base.OnEnable();
            length = serializedObject.FindProperty("_length");
            minValue = serializedObject.FindProperty("_minValue");
            maxValue = serializedObject.FindProperty("_maxValue");
            wholeNumbers = serializedObject.FindProperty("_wholeNumbers");
            value = serializedObject.FindProperty("_value");
            onValueChanged = serializedObject.FindProperty("_onValueChanged");
            fillBox = serializedObject.FindProperty("_fillBox");
            background = serializedObject.FindProperty("_background");
            fill = serializedObject.FindProperty("_fill");
            handle = serializedObject.FindProperty("_handle");
            direction = serializedObject.FindProperty("_direction");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            EditorGUILayout.PropertyField(fillBox);
            EditorGUILayout.PropertyField(background);
            EditorGUILayout.PropertyField(fill);
            EditorGUILayout.PropertyField(handle);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(length);
            EditorGUILayout.PropertyField(direction);
            EditorGUILayout.PropertyField(minValue);
            EditorGUILayout.PropertyField(maxValue);
            EditorGUILayout.PropertyField(wholeNumbers);

            var val = EditorGUILayout.Slider(value.displayName, value.floatValue, minValue.floatValue, maxValue.floatValue);
            value.floatValue = wholeNumbers.boolValue ? Mathf.RoundToInt(val) : val;

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(onValueChanged);

            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
        }
    }
}