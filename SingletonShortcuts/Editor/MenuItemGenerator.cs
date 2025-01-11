using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace PhEngine.Utils
{
    public static class MenuItemGenerator
    {
        private const string OutputPath = "Assets/GeneratedMenuItems/Editor/";
        private const string GeneratedFileName = "SingletonShortcutMenuItems.cs";
        const string CachedTypesKeys = "CachedTypes";
        
        const string OpenScriptableObjectWindow = "OpenScriptableObjectWindow";
        const string OpenSingletonWindow = "OpenSingletonWindow";
        
        const string SelectScriptableObject = "SelectScriptableObject";
        const string SelectMonoBehaviour = "SelectMonoBehaviour";

        const string DefaultMenuName = "Singleton Shortcuts";

        [MenuItem("Singleton Shortcuts/Rebuild All", priority = -100)]
        static void ManuallyGenerateMenuItems()
        {
            TryGenerate();
            AssetDatabase.Refresh();
        }

        static void TryGenerate()
        {
            var typesWithAttribute = GetTypesWithSingletonShortcut();
            if (typesWithAttribute.Any())
            {
                GenerateScriptFile(typesWithAttribute);
            }
            else
                DeleteGeneratedFile();
            
        }

        static HashSet<Type> GetTypesWithSingletonShortcut()
        {
            var typesWithAttribute = new HashSet<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly.GetName().Name != "PhEngine.Utils" && assembly.GetReferencedAssemblies().All(referenced => referenced.Name != "PhEngine.Utils")) 
                    continue;

                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    //TODO: Add option to consider only Included namespaces.
                    if (type.GetCustomAttribute<SingletonShortcut>() != default && SingletonShortcut.IsValid(type))
                        typesWithAttribute.Add(type);
                }
            }
            return typesWithAttribute;
        }

        static void GenerateScriptFile(HashSet<Type> types)
        {
            if (!Directory.Exists(OutputPath))
                Directory.CreateDirectory(OutputPath);

            string scriptContent = GenerateScriptContent(types);
            
            // Check if the file exists and compare content
            string filePath = Path.Combine(OutputPath, GeneratedFileName);
            if (File.Exists(filePath) && EditorPrefs.HasKey(CachedTypesKeys) && EditorPrefs.GetString(CachedTypesKeys) == scriptContent)
            {
                Debug.Log($"[Singleton Shortcuts] No changes detected");
                return;
            }
            
            File.WriteAllText(filePath, scriptContent);
            EditorPrefs.SetString(CachedTypesKeys, scriptContent);
            Debug.Log($"[Singleton Shortcuts] Generated script file: {filePath}");
        }
        
        static void DeleteGeneratedFile()
        {
            string filePath = Path.Combine(OutputPath, GeneratedFileName);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        static string GenerateScriptContent(HashSet<Type> types)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(
                @"using PhEngine.Utils;
using UnityEditor;
using System.Reflection;

public class SingletonShortcutMenuItems
{");
            foreach (var type in types)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append(GenerateMenuItem(type));
            }

            stringBuilder.AppendLine("}");
            return stringBuilder.ToString();
        }

        static StringBuilder GenerateMenuItem(Type type)
        {
            var attribute = type.GetCustomAttribute<SingletonShortcut>();
            string menuPath = string.IsNullOrEmpty(attribute.CustomPath) ? 
                $"{DefaultMenuName}/{type.Name}" : 
                attribute.CustomPath;

            if (!menuPath.Contains("/"))
                menuPath = $"{DefaultMenuName}/{menuPath}";

            bool isCreateIfMissing = attribute.IsTryCreateIfMissing;
            var shortcutType = attribute.Type;
            var function = shortcutType == ShortcutType.FloatingWindow ?
                GetOpenFloatingWindowFunction(type) : 
                GetSelectFunction(type);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($@"    [MenuItem(""{menuPath}"", priority = {attribute.Priority})]
    public static void Show{type.Name}()
    {{
        var type = Assembly.Load(""{type.Assembly.FullName}"").GetType(""{type.FullName}"");
        typeof(PropertyWindowHelper).GetMethod(""{function}"")?
            .MakeGenericMethod(type).Invoke(null, new object[] {{ {isCreateIfMissing.ToString().ToLower()} }});
    }}");
            return stringBuilder;
        }
        
        static string GetOpenFloatingWindowFunction(Type type)
        {
            return type.IsSubclassOf(typeof(ScriptableObject)) ? 
                OpenScriptableObjectWindow :
                OpenSingletonWindow;
        }
        
        static string GetSelectFunction(Type type)
        {
            return type.IsSubclassOf(typeof(ScriptableObject))
                ? SelectScriptableObject
                : SelectMonoBehaviour;
        }
    }
}
