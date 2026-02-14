using System.IO;
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine.UI;

/// <summary>
/// Class to create script for the FBUIBase asset
/// </summary>
public static class UIScriptWriter
{
    /// <summary>
    /// Static string lists for default usings
    /// </summary>
    public static string DEFAULT_USING =
        "using UnityEngine;\n" +
        "using UnityEngine.UI;\n" +
        "using System.Collections;\n" +
        "using TMPro;\n";

    /// <summary>
    /// Creates the main UI class script
    /// </summary>
    public static void CreateMainUIClass(string path, string className, string viewClassName)
    {
        string filePath = $"{path}/{className}.cs";

        // Only create if the file doesn't exist
        if (File.Exists(filePath))
        {
            Debug.Log($"Main UI class already exists, preserving: {className}");
            return;
        }

        string scriptContent = $@"{DEFAULT_USING}

/// <summary>
/// Auto-generated UI class for {className}
/// </summary>
public class {className} : FBUIBase
{{
    public override void Init()
    {{
        base.Init();
        // Add your initialization code here
    }}

    public override void OnCreate()
    {{
        base.OnCreate();
        // Called when the UI is created
    }}

    public override void OnOpen()
    {{
        base.OnOpen();
        // Called when the UI is shown for the first time
    }}

    public override void OnRefresh()
    {{
        base.OnRefresh();
        // Called every time the UI is shown after the first time
    }}

    public override void OnHide()
    {{
        base.OnHide();
        // Called when the UI is hidden
    }}

    public override void OnRemove()
    {{
        base.OnRemove();
        // Called when the UI is manually destroyed
    }}
}}";

        WriteScriptFile(filePath, scriptContent);
    }

    /// <summary>
    /// Creates the View class script
    /// </summary>
    public static void CreateViewClass(string path, string viewClassName, GameObject prefab)
    {
        string filePath = $"{path}/{viewClassName}.cs";
        // Generate component references based on the prefab hierarchy
        string componentReferences = GenerateComponentReferences(prefab);
        string scriptContent = $@"{DEFAULT_USING}

/// <summary>
/// Auto-generated View class for UI components
/// </summary>
public class {viewClassName} : FBUIView
{{
    // UI component references will be automatically populated here
    // Example:
    // public Button myButton;
    // public TextMeshProUGUI titleText;
    // public Image backgroundImage;

{componentReferences}
}}";

        WriteScriptFile(filePath, scriptContent);
    }

    /// <summary>
    /// Generates component reference fields based on the prefab hierarchy
    /// </summary>
    private static string GenerateComponentReferences(GameObject prefab)
    {
        var references = new List<string>();
        var processedObjects = new HashSet<GameObject>();

        // Scan the prefab hierarchy
        ScanGameObjectRecursive(prefab.transform, "", references, processedObjects);

        if (references.Count == 0)
        {
            return "    // No UI components found in hierarchy\n";
        }

        return string.Join("\n", references) + "\n";
    }

    /// <summary>
    /// Recursively scans GameObject hierarchy for UI components
    /// </summary>
    private static void ScanGameObjectRecursive(Transform current, string currentPath, List<string> references, HashSet<GameObject> processedObjects)
    {
        if (current == null || processedObjects.Contains(current.gameObject))
            return;

        processedObjects.Add(current.gameObject);

        // Check if this object has FBUIBase (prefab-in-prefab case)
        var childUIBase = current.GetComponent<FBUIBase>();
        if (childUIBase != null && current != current.root) // Don't skip the root itself
        {
            // Record the FBUIBase reference but don't go deeper
            string fieldName = MakeValidFieldName(current.name, childUIBase);
            references.Add($"    [SerializeField] public FBUIBase {fieldName}; // Nested UI: {current.name}");
            return;
        }
        else 
        {
            childUIBase = null;
        }

        // Scan all components on this GameObject for UI elements
        var components = current.GetComponents<Component>();
        foreach (var component in components)
        {
            if (component == null) continue;

            string componentType = component.GetType().Name;
            string fieldName = MakeValidFieldName(current.name, component);
            string fieldType = GetFieldTypeForComponent(component);

            if (!string.IsNullOrEmpty(fieldType))
            {
                string referenceLine = $"    [SerializeField] public {fieldType} {fieldName}; // {currentPath}{current.name}";

                // Avoid duplicates
                if (!references.Any(r => r.EndsWith($"{fieldName};")))
                {
                    references.Add(referenceLine);
                }
            }
        }

        // Recursively scan children (unless we found an FBUIBase)
        if (childUIBase == null)
        {
            for (int i = 0; i < current.childCount; i++)
            {
                Transform child = current.GetChild(i);
                string newPath = string.IsNullOrEmpty(currentPath) ? "" : $"{currentPath}{current.name}/";
                ScanGameObjectRecursive(child, newPath, references, processedObjects);
            }
        }
    }

    /// <summary>
    /// Maps Unity components to appropriate field types
    /// </summary>
    private static string GetFieldTypeForComponent(Component component)
    {
        if (component == null) return null;

        Type componentType = component.GetType();

        // UGUI Components
        if (componentType == typeof(Button)) return "Button";
        if (componentType == typeof(Image)) return "Image";
        if (componentType == typeof(Text)) return "Text";
        if (componentType == typeof(InputField)) return "InputField";
        if (componentType == typeof(Slider)) return "Slider";
        if (componentType == typeof(Scrollbar)) return "Scrollbar";
        if (componentType == typeof(ScrollRect)) return "ScrollRect";
        if (componentType == typeof(Dropdown)) return "Dropdown";
        if (componentType == typeof(Toggle)) return "Toggle";

        // TMPro Components
        if (componentType == typeof(TMPro.TextMeshProUGUI)) return "TMPro.TextMeshProUGUI";
        if (componentType == typeof(TMPro.TMP_InputField)) return "TMPro.TMP_InputField";
        if (componentType == typeof(TMPro.TMP_Dropdown)) return "TMPro.TMP_Dropdown";
        if (componentType == typeof(TMPro.TMP_Text)) return "TMPro.TMP_Text";

        // Other useful UI components
        if (componentType == typeof(Animator)) return "Animator";
        if (componentType == typeof(Animation)) return "Animation";

        // MarkedUIs 
        if (componentType == typeof(FBMarkedUI)) return "GameObject";

        return null; // Skip non-UI components
    }

    /// <summary>
    /// Converts GameObject name to valid C# field name
    /// </summary>
    public static string MakeValidFieldName(string originalName, Component component = null)
    {

        // Start with the original object name
        string baseName = originalName;

        // Remove invalid characters and ensure it starts with a letter
        string validName = Regex.Replace(baseName, @"[^a-zA-Z0-9_]", "");

        // Add component type suffix if provided
        if (component != null)
        {
            string componentSuffix = GetComponentTypeSuffix(component);
            if (!string.IsNullOrEmpty(componentSuffix))
            {
                // Only add suffix if it's different from the base name
                if (!validName.EndsWith(componentSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    validName += "_" + componentSuffix;
                }
            }
        }

        // Ensure it starts with a letter or underscore
        if (string.IsNullOrEmpty(validName) || char.IsDigit(validName[0]))
        {
            validName = "_" + validName;
        }

        // Make first character uppercase for PascalCase (common C# convention)
        if (validName.Length > 0 && char.IsLower(validName[0]))
        {
            validName = char.ToUpper(validName[0]) + validName.Substring(1);
        }

        // Ensure uniqueness by adding number if needed
        int counter = 1;
        string finalName = validName;
        while (IsReservedKeyword(finalName))
        {
            finalName = validName + counter;
            counter++;
        }

        return finalName;
    }

    /// <summary>
    /// Gets the component type name for reflection
    /// </summary>
    public static string GetComponentTypeName(Component component)
    {
        if (component == null) return null;

        Type componentType = component.GetType();

        // Always return type for FBMarkedUI
        if (componentType == typeof(FBMarkedUI))
        {
            return typeof(GameObject).FullName;
        }

        // Check if it's a UI component we care about
        if (GetFieldTypeForComponent(component) != null)
        {
            return componentType.FullName;
        }

        return null;
    }

    /// <summary>
    /// Gets the component type suffix for field naming
    /// </summary>
    private static string GetComponentTypeSuffix(Component component)
    {
        if (component == null) return "";

        Type componentType = component.GetType();

        // UGUI Components
        if (componentType == typeof(Button)) return "Button";
        if (componentType == typeof(Image)) return "Image";
        if (componentType == typeof(Text)) return "Text";
        if (componentType == typeof(InputField)) return "InputField";
        if (componentType == typeof(Slider)) return "Slider";
        if (componentType == typeof(Scrollbar)) return "Scrollbar";
        if (componentType == typeof(ScrollRect)) return "ScrollRect";
        if (componentType == typeof(Dropdown)) return "Dropdown";
        if (componentType == typeof(Toggle)) return "Toggle";
        if (componentType == typeof(Canvas)) return "Canvas";
        if (componentType == typeof(CanvasGroup)) return "CanvasGroup";
        if (componentType == typeof(RectTransform)) return "RectTransform";

        // TMPro Components
        if (componentType == typeof(TMPro.TextMeshProUGUI)) return "Text";
        if (componentType == typeof(TMPro.TMP_InputField)) return "InputField";
        if (componentType == typeof(TMPro.TMP_Dropdown)) return "Dropdown";
        if (componentType == typeof(TMPro.TMP_Text)) return "Text";

        // Other components
        if (componentType == typeof(Animator)) return "Animator";
        if (componentType == typeof(Animation)) return "Animation";
        if (componentType == typeof(FBUIBase)) return "UI";
        // MarkedUIs
        if (componentType == typeof(FBMarkedUI)) return "UINode";

        return "Component"; // Generic fallback
    }

    /// <summary>
    /// Checks if a name is a C# reserved keyword
    /// </summary>
    private static bool IsReservedKeyword(string name)
    {
        string[] reservedKeywords = {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch",
        "char", "checked", "class", "const", "continue", "decimal", "default",
        "delegate", "do", "double", "else", "enum", "event", "explicit",
        "extern", "false", "finally", "fixed", "float", "for", "foreach",
        "goto", "if", "implicit", "in", "int", "interface", "internal",
        "is", "lock", "long", "namespace", "new", "null", "object",
        "operator", "out", "override", "params", "private", "protected",
        "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
        "sizeof", "stacked", "static", "string", "struct", "switch", "this",
        "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
        "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
    };

        return reservedKeywords.Contains(name);
    }

    /// <summary>
    /// Writes a script file to the specified path
    /// </summary>
    private static void WriteScriptFile(string filePath, string content)
    {
        try
        {
            File.WriteAllText(filePath, content);
            Debug.Log($"Successfully created script: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create script at {filePath}: {e.Message}");
        }
    }

    /// <summary>
    /// Ensures the directory exists, creating it if necessary
    /// </summary>
    public static void EnsureDirectoryExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string[] folders = path.Split('/');
            string parentFolder = "Assets";

            for (int i = 1; i < folders.Length; i++)
            {
                string currentFolder = parentFolder + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(currentFolder))
                {
                    AssetDatabase.CreateFolder(parentFolder, folders[i]);
                }
                parentFolder = currentFolder;
            }
        }
    }

    /// <summary>
    /// Extracts the system folder from a prefab path
    /// </summary>
    public static string ExtractSystemFolderFromPath(string prefabPath)
    {
        // Expected path pattern: Resources/Art/UI/Widgets/{systemFolder}/prefabName.prefab
        string[] pathParts = prefabPath.Split('/');

        // Find the "Widgets" folder index
        int widgetsIndex = -1;
        for (int i = 0; i < pathParts.Length; i++)
        {
            if (pathParts[i] == "Widgets")
            {
                widgetsIndex = i;
                break;
            }
        }

        if (widgetsIndex == -1 || widgetsIndex + 1 >= pathParts.Length)
        {
            Debug.LogError("Prefab is not in the expected path structure: Resources/Art/UI/Widgets/{systemFolder}/");
            return null;
        }

        return pathParts[widgetsIndex + 1];
    }
}