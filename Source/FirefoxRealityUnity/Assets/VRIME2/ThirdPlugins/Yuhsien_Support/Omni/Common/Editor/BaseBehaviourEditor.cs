// ========================================================================== //
//
//  class BaseBehaviourEditor
//  -----
//  Purpose: Custom editor for BaseBehaviour
//
//
//  Created: 2017-07-17
//  Updated: 2017-07-17
//
//  Copyright 2017 HTC America Innovation
// 
// ========================================================================== //
using UnityEditor;
using UnityEngine;

namespace Htc.Omni
{
    [CustomEditor(typeof(BaseBehaviour), true)]
    [CanEditMultipleObjects]
    public class BaseBehaviourEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            var iterator = serializedObject.GetIterator();

            if (iterator.NextVisible(true))
            {
                while (iterator.NextVisible(false))
                    EditorGUILayout.PropertyField(iterator, true, new GUILayoutOption[0]);
            }

            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
        }
    }
}