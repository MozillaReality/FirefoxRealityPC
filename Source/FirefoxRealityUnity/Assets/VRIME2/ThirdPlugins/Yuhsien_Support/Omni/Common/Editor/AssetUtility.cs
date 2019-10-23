// ========================================================================== //
//
//  class AssetUtility
//  -----
//  Purpose: Utilities for creating and loading assets
//
//
//  Created: 2017-04-09
//  Updated: 2017-04-09
//
//  Copyright 2017 Yu-hsien Chang
// 
// ========================================================================== //
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Htc.Omni
{
    public static class AssetUtility
    {
        private const string AssetFolder = "Assets/Resources/ViveToolkit/";

        public static T Load<T>(string assetName, bool create = false) where T : UnityEngine.ScriptableObject
        {
            var assetPath = AssetFolder + assetName + ".asset";
            // Load data from asset
            var data = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            // Create asset if no data is available
            if (data == null && create)
            {
                data = UnityEngine.ScriptableObject.CreateInstance<T>();
                CreateAssetFolder();
                AssetDatabase.CreateAsset(data, assetPath);
                AssetDatabase.SaveAssets();
            }

            return data;
        }

        public static void CreateAssetFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            if (!AssetDatabase.IsValidFolder("Assets/Resources/ViveToolkit"))
                AssetDatabase.CreateFolder("Assets/Resources", "ViveToolkit");
        }

        /// <summary>
        //	This makes it easy to create, name and place unique new ScriptableObject asset files.
        /// </summary>
        public static void CreateAsset<T>(T asset = null) where T : ScriptableObject
        {
            if (asset == null)
                asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
                path = "Assets";
            else if (!string.IsNullOrEmpty(Path.GetExtension(path)))
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), string.Empty);

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).Name + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}