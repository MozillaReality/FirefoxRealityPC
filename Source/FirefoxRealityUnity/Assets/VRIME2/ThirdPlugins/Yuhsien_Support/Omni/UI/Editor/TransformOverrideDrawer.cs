// ========================================================================== //
//
//  class TransformOverrideDrawer
//  -----
//  Purpose: Custom drawer for TransformOverride
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
    [CustomPropertyDrawer(typeof(TransformOverride))]
    public class TransformOverrideDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var active = property.FindPropertyRelative("active").boolValue;
            return active ? EditorGUIUtility.singleLineHeight * 4 : EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            var rect = position;
            rect.height = EditorGUIUtility.singleLineHeight;

            var activeProp = property.FindPropertyRelative("active");

            activeProp.boolValue = EditorGUI.ToggleLeft(rect, label, activeProp.boolValue);

            if (activeProp.boolValue)
            {
                EditorGUI.indentLevel++;

                rect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("position"));
                rect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("rotation"));
                rect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("scale"));

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
}