# SingletonShortcuts
SingletonShortcut is a utility for simplifying the workflow of managing Unity's MonoBehaviour and ScriptableObject in the Unity Editor. It provides helper methods for tasks such as:

- Opening a floating property editor window to quickly edit singleton-like objects, such as game settings or utility assets.
- Selecting MonoBehaviour or ScriptableObject instances in the editor.
- Find single instances of specific objects across scenes or in the project.

These methods are beneficial for single-instance objects, such as:
- Game Settings ScriptableObject
- Utility ScriptableObject
- Singleton MonoBehaviour

# Examples

## 1. Using PropertyWindowHelper with a ScriptableObject
Here’s an example demonstrating how to integrate PropertyWindowHelper with a ScriptableObject called SampleSO.

> [!NOTE]
> Make sure that you contain the **UnityEditor.MenuItem** usage inside **#if UNITY_EDITOR** or put the script inside an **Editor** folder. Since [MenuItem] attribute only works in Editor.

```csharp
using UnityEngine;
using PhEngine.Utils;

public class SampleSO : ScriptableObject
{
    [Header("Gameplay Settings")]
    [SerializeField] float playerSpeed = 5.0f;
    [SerializeField] int maxHealth = 100;
    [SerializeField] float enemySpawnRate = 1.0f;

    [Header("UI Settings")]
    [SerializeField] Color uiColor = Color.white;
    [SerializeField] int fontSize = 14;

#if UNITY_EDITOR
    [UnityEditor.MenuItem("CustomMenu/SampleSO23")]
    static void ShowWindow()
    {
        PropertyWindowHelper.OpenScriptableObjectWindow<SampleSO>();
    }
#endif
}
```
## 2. Shortcut with [SingletonShortcut] Attribute
If you don’t want to manually write the MenuItem attribute logic in **Option 1**, you can use the custom [SingletonShortcut] attribute. Simply add it to the class you want to create a shortcut for. 

> [!NOTE]
> However, using this attribute will require a manual Menu Item rebuild from you.
> And the class that uses the attribute needs to be public, non-abstract, non-generic, and derived from either MonoBehaviour or ScriptableObject.

### Steps:
- Add [SingletonShortcut] to the desired classes.
- Wait for the code to compile.
- Open the menu Singleton Shortcuts > Rebuild All.
- The Editor Script will automatically generate MenuItem entries for all classes tagged with [SingletonShortcut].

```csharp
using UnityEngine;
using PhEngine.Utils;

[SingletonShortcut]
public class SampleSO : ScriptableObject
{
    [Header("Gameplay Settings")]
    [SerializeField] float playerSpeed = 5.0f;
    [SerializeField] int maxHealth = 100;
    [SerializeField] float enemySpawnRate = 1.0f;

    [Header("UI Settings")]
    [SerializeField] Color uiColor = Color.white;
    [SerializeField] int fontSize = 14;

#if UNITY_EDITOR
    [UnityEditor.MenuItem("CustomMenu/SampleSO23")]
    static void ShowWindow()
    {
        PropertyWindowHelper.OpenScriptableObjectWindow<SampleSO>();
    }
#endif
}
```

# Additional Notes
- For those who already have a MonoBehaviour implemented as a singleton in the scene and want to quickly open its property editor window, you can also use **OpenSingletonWindow<T>** or **SelectMonoBehaviour<T>** from the **PropertyWindowHelper** class. Usage of [SingletonShortcut] Attribute is also supported for MonoBehaviour.

- Here's a summary of [SingletonShortcut] attribute parameters that you can also use :

| Property Name              | Description                                                         | Default Value                                 |
|----------------------------|---------------------------------------------------------------------|-----------------------------------------------|
| **ShortcutType Type**       | Specifies the type of shortcut to create.                           | `ShortcutType.FloatingWindow`                 |
| **bool IsTryCreateIfMissing**| Indicates whether to attempt creating the object if it's missing.  | `false`                                       |
| **string CustomPath**       | Allows specifying a custom menu path for the generated shortcut.   | `null` (uses a default menu structure)        |
| **int Priority**            | Sets the priority of the menu item in the Unity Editor.            | `0`                                           |

