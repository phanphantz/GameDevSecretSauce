#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace PhEngine.Utils
{
    public static class PropertyWindowHelper
    {
        public static void SelectMonoBehaviour<T>(bool isCreateIfNotExist = false) where T : MonoBehaviour
        {
#if UNITY_EDITOR
            var obj = TryFind<T>(isCreateIfNotExist);
            if (obj)
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
            else
            {
                Debug.LogError("Cannot find any " + typeof(T).Name + " from the opened scenes.");
            }
#endif
        }
        
        public static void SelectScriptableObject<T>(bool isCreateIfNotExist = false) where T : ScriptableObject
        {
#if UNITY_EDITOR
            if (ScriptableObjectHelper.TryFindInEditor<T>(isCreateIfNotExist, out var asset))
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
#endif
        }
        
        /// <summary>
        /// Opens a floating Property Editor window of the target ScriptableObject.
        /// First ScriptableObject that is found under the project folder will be used. 
        /// This function only works in Editor.
        /// </summary>
        /// <param name="isCreateIfNotExist">Create a new ScriptableObject under Resources folder if there isn't any.</param>
        /// <typeparam name="T"></typeparam>
        public static void OpenScriptableObjectWindow<T>(bool isCreateIfNotExist = false) where T : ScriptableObject
        {
#if UNITY_EDITOR
            if (ScriptableObjectHelper.TryFindInEditor<T>(isCreateIfNotExist, out var asset))
                EditorUtility.OpenPropertyEditor(asset);
#endif
        }

        /// <summary>
        /// Opens a floating Property Editor window of first MonoBehaviour with specified type that is found from opened scenes.
        /// </summary>
        /// <param name="isCreateIfNotExist">Create a new game object with a specified component under Resources folder if there isn't any.</param>
        /// <typeparam name="T"></typeparam>
        public static void OpenSingletonWindow<T>(bool isCreateIfNotExist = false) where T : MonoBehaviour
        {
#if UNITY_EDITOR
            var target = TryFind<T>(isCreateIfNotExist);
            if (target)
                EditorUtility.OpenPropertyEditor(target);
            else
                Debug.LogError("Cannot find any " + typeof(T).Name + " from the opened scenes.");
#endif
        }

        static T TryFind<T>(bool isCreateIfNotExist) where T : MonoBehaviour
        {
            var target = Object.FindObjectOfType<T>();
            if (target == null && isCreateIfNotExist)
            {
                var gameObject = new GameObject(typeof(T).Name + "_(Generated)");
                target = gameObject.AddComponent<T>();
            }

            return target;
        }
    }
}