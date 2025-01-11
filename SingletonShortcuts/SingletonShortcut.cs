using System;
using UnityEngine;

namespace PhEngine.Utils
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SingletonShortcut : Attribute
    {
        public ShortcutType Type;
        public int Priority;
        public bool IsTryCreateIfMissing;
        public string CustomPath;
        public SingletonShortcut(ShortcutType type = ShortcutType.FloatingWindow, bool isTryCreateIfMissing = false, string customPath = null, int priority = 0)
        {
            Type = type;
            CustomPath = customPath;
            IsTryCreateIfMissing = isTryCreateIfMissing;
            Priority = 0;
        }
        
        public static bool IsValid(Type type)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(type) && !typeof(ScriptableObject).IsAssignableFrom(type))
            {
                Debug.LogError($"The class {type.Name} must inherit from MonoBehaviour or ScriptableObject to use the SingletonShortcut attribute.");
                return false;
            }

            if (!type.IsPublic)
            {
                Debug.LogError($"The class {type.Name} must be public to use the SingletonShortcut attribute.");
                return false;
            }
            
            if (type.IsGenericType)
            {
                Debug.LogError($"The class {type.Name} cannot be a generic type to use the SingletonShortcut attribute.");
                return false;
            }

            if (type.IsAbstract)
            {
                Debug.LogError($"The class {type.Name} cannot be abstract to use the SingletonShortcut attribute.");
                return false;
            }
            
            return true;
        }
    }

    public enum ShortcutType
    {
        FloatingWindow, Selection
    }
}