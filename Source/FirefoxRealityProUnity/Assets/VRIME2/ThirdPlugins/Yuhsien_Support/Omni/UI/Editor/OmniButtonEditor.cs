// ========================================================================== //
//
//  class OmniButtonEditor
//  -----
//  Purpose: Custom editor for OmniButton
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
    [CustomEditor(typeof(OmniButton))]
    [CanEditMultipleObjects]
    public class OmniButtonEditor : OmniSelectableEditor
    {
        private SerializedProperty onClick;

        protected override void OnEnable()
        {
            base.OnEnable();
            onClick = serializedObject.FindProperty("_onClick");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            EditorGUILayout.PropertyField(onClick);
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
        }
    }
}