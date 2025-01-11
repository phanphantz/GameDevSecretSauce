using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PhEngine.Utils
{
    public static class ScriptableObjectHelper
    {
        /// <summary>
        /// Try to find and return the first ScriptableObject if it exists in the project folder.
        /// This function only works in Editor.
        /// </summary>
        /// <param name="isCreateIfNotExist">Create a new ScriptableObject under specified directory path if there isn't any.</param>
        /// <param name="result"></param>
        /// <param name="fallbackDirectoryForCreation">Directory path where the ScriptableObject will be created if not found. if NULL is passed, the object will be created under the Resources folder</param>
        /// <typeparam name="T">Type of the ScriptableObject to create</typeparam>
        /// <returns></returns>
        public static bool TryFindInEditor<T>(bool isCreateIfNotExist, out T result, string fallbackDirectoryForCreation = null) where T : ScriptableObject
        {
            result = null;
#if UNITY_EDITOR
            var typeName = typeof(T).Name;
            var guids = AssetDatabase.FindAssets("t:" + typeName);
            var resultList = new List<T>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var match = AssetDatabase.LoadAssetAtPath<T>(path);
                if (match != null)
                    resultList.Add(match);
            }

            var resultCount = resultList.Count;
            if (resultCount != 0)
            {
                if (resultCount > 1)
                    Debug.LogWarning("Multiple ScriptableObjects of type: " + typeName +
                                     " were found. The first one will be used.");

                result = resultList.FirstOrDefault();
                return true;
            }

            if (!isCreateIfNotExist)
            {
                Debug.LogError("The ScriptableObject of type: " + typeName + " were not found");
                return false;
            }

            //No ScriptableObjects found, Let's create one!
            var createPath = $"Assets/Resources/" + typeName;
            if (!string.IsNullOrEmpty(fallbackDirectoryForCreation))
                createPath = Path.Combine(fallbackDirectoryForCreation, typeName);

            return TryCreateInEditor(createPath, out result);
#else
        Debug.LogWarning("You tried to find a ScriptableObject inside a build. This is not permitted.");
        return false;
#endif
        }

        public static bool TryCreateInEditor<T>(string pathWithoutExtension, out T result) where T : ScriptableObject
        {
            result = null;
#if UNITY_EDITOR
            var scriptableObject = ScriptableObject.CreateInstance<T>();
            var finalPath = pathWithoutExtension + ".asset";
            AssetDatabase.CreateAsset(scriptableObject, finalPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            result = AssetDatabase.LoadAssetAtPath<T>(finalPath);
            return true;
#else
        Debug.LogWarning("You tried to create a ScriptableObject inside a build. This is not permitted.");
        return false;
#endif
        }
    }
}