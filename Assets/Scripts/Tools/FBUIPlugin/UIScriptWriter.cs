using System.IO;
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

/// <summary>
/// Class to create script for the FBUIBase asset
/// </summary>
public static class UIScriptWriter
{
    ///Static string lists for default usings
    public static string DEFAULT_USING = 
        "using UnityEngine;\n" +
        "using UnityEngine.UI;\n" +
        "using System.Collections;\n" +
        "using TMPro;\n";

    /// <summary>
    /// Create script
    /// </summary>
    /// <param name="name"> Name of the prefab, so is the class </param>
    /// <param name="scriptPath"> Desired script path </param>
    public static void CreateScript(string name, string scriptPath)
    {
        // Construct the full path to the script file
        string scriptFileName = $"{name}.cs";
        string fullScriptPath = Path.Combine(scriptPath, scriptFileName);

        // Check if the script file exists
        if (!File.Exists(fullScriptPath))
        {
            // Check if a class with the same name already exists
            if (!ClassExists(name))
            {
                // Create the script file
                CreateScriptFile(fullScriptPath, name);
            }
            else
            {
                Debug.LogWarning($"A class named '{name}' already exists.");
            }
        }
        else
        {
            Debug.Log($"Script '{scriptFileName}' already exists at '{scriptPath}'.");
        }
    }

    public static bool ClassExists(string className)
    {
        return (null != Type.GetType(className)); ;
    }

    public static void CreateScriptFile(string path, string className)
    {
        string scriptContent = $"{DEFAULT_USING}\npublic class {className} : FBUIBase\n{{\n" +
            $"\t// Called once when create regardless shown or not\n" +
            $"\tpublic override void OnCreate() \n\t{{\n\t}}\n\n" +
            $"\t// Called once when opened for the first time, before OnRefresh\n" +
            $"\tpublic override void OnOpen() \n\t{{\n\t}}\n\n" +
            $"\t// Called once when showed\n" +
            $"\tpublic override void OnRefresh() \n\t{{\n\t}}\n\n" +
            $"\t// Called once when closed\n" +
            $"\tpublic override void OnHide() \n\t{{\n\t}}\n\n" +
            $"\t// Called once when destroyed\n" +
            $"\tpublic override void OnRemove() \n\t{{\n\t}}\n" +
            $"\n}}";

        File.WriteAllText(path, scriptContent);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// Scan and write all references
    /// </summary>
    /// <param name="name"> Name of the prefab, so is the class </param>
    /// <param name="scriptPath"> Script path </param>
    /// <param name="selectedObject"> UI prefab </param>
    public static void WriteReferences(string name, string scriptPath, GameObject selectedObject)
    {
        // Scan all child objects
        List<GameObject> childObjects = new List<GameObject>();
        CollectChildren(selectedObject.transform, childObjects);

        // Find the position of insertion
        // right after the line of the class definition
        string scriptFileName = $"{name}.cs";
        string fullScriptPath = Path.Combine(scriptPath, scriptFileName);
        string scriptContent = File.ReadAllText(fullScriptPath);
        string classPattern = $@"public class {name} : FBUIBase(, .+)?\s*{{";

        // Use Regex to find the class definition
        Regex regex = new Regex(classPattern, RegexOptions.Singleline);
        Match match = regex.Match(scriptContent);

        if (match.Success)
        {
            int insertionIndex = scriptContent.IndexOf('{', match.Index) + 1;
            Dictionary<string, Type> widgetNameToComponentType = UIConfig.widgetName;
            // String for new reference entries
            string newReferences = "";

            foreach (GameObject child in childObjects)
            {
                foreach (var widgetNameEntry in widgetNameToComponentType)
                {
                    if (child.name.Contains(widgetNameEntry.Key))
                    {
                        // Get the component type from the dictionary
                        Type componentType = widgetNameEntry.Value;
                        // Construct the serialized field declaration
                        string componentName = child.name;
                        string serializedField = $"\t[SerializeField] private {componentType.Name} {componentName};\n";

                        // Check if the reference already exists in the script content
                        string referencePattern = $@"\b(public|private|protected)\s+{componentType.Name}\s+{componentName}\b";
                        Regex referenceRegex = new Regex(referencePattern, RegexOptions.Singleline);
                        if (!referenceRegex.IsMatch(scriptContent))
                        {
                            newReferences += serializedField;
                        }
                    }
                }
            }

            // If we have new references to add, insert them into the script content
            if (!string.IsNullOrEmpty(newReferences))
            {
                newReferences = "\n\t// Auto generated UI widgets references\n" + newReferences;
                scriptContent = scriptContent.Insert(insertionIndex, newReferences);
                File.WriteAllText(fullScriptPath, scriptContent);
                AssetDatabase.Refresh();
                AssetDatabase.ImportAsset(fullScriptPath);
            }

            string newCallbacks = "";
            foreach (GameObject child in childObjects)
            {
                string componentName = child.name;
                // Go through the widgetNameToComponentType dictionary
                foreach (var pair in widgetNameToComponentType)
                {
                    // Check if the object name contains the key from the dictionary
                    if (componentName.Contains(pair.Key))
                    {
                        // Check if there is a callback prefix for this component type
                        if (UIConfig.callBack.TryGetValue(pair.Value, out string callbackPrefix))
                        {
                            string callbackMethodName = $"{callbackPrefix}{componentName}";
                            string callbackMethod = $"\n\tpublic void {callbackMethodName}()\n\t{{\n\t\t// TODO: Implement callback logic for {componentName}\n\t}}\n";

                            // Check if the callback method already exists in the script content
                            string callbackMethodPattern = $@"\bpublic\s+void\s+{callbackMethodName}\b";
                            Regex callbackMethodRegex = new Regex(callbackMethodPattern, RegexOptions.Singleline);
                            if (!callbackMethodRegex.IsMatch(scriptContent))
                            {
                                newCallbacks += callbackMethod;
                            }
                        }
                    }
                }
            }


            // If we have new callbacks to add, insert them before the last } of the script
            if (!string.IsNullOrEmpty(newCallbacks))
            {
                newCallbacks = "\n\t// Auto generated UI callback functions\n" + newCallbacks;
                int lastBraceIndex = scriptContent.LastIndexOf('}');
                scriptContent = scriptContent.Insert(lastBraceIndex, newCallbacks);
            }

            File.WriteAllText(fullScriptPath, scriptContent);
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(fullScriptPath);

            // Attach the script to the prefab if it's not already attached
            MonoScript scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(fullScriptPath);
            if (scriptAsset != null && !selectedObject.GetComponent(name))
            {
                Undo.AddComponent(selectedObject, scriptAsset.GetClass());
                EditorUtility.SetDirty(selectedObject);
            }
            AssignComponentsToFields(selectedObject, name);
        }
    }

    private static void CollectChildren(Transform parent, List<GameObject> childObjects)
    {
        childObjects.Add(parent.gameObject);
        for (int i = 0; i < parent.childCount; i++)
        {
            CollectChildren(parent.GetChild(i), childObjects);
        }
    }

    /// <summary>
    /// Assign reference field automatically
    /// </summary>
    /// <param name="selectedObject"> UI prefab </param>
    /// <param name="scriptName"> Script path </param>
    private static void AssignComponentsToFields(GameObject selectedObject, string scriptName)
    {
        Component scriptComponent = selectedObject.GetComponent(scriptName);
        System.Type scriptType = scriptComponent.GetType();
        // Collect all fields within the compoennt
        System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic 
            | System.Reflection.BindingFlags.Public 
            | System.Reflection.BindingFlags.Instance;
        System.Reflection.FieldInfo[] fields = scriptType.GetFields(flags);

        List<GameObject> childObjects = new List<GameObject>();
        CollectChildren(selectedObject.transform, childObjects);

        // Assign fields
        foreach (System.Reflection.FieldInfo field in fields)
        {
            // Check if the field has the SerializeField attribute
            object[] attributes = field.GetCustomAttributes(typeof(SerializeField), true);
            if (attributes.Length > 0)
            {
                // Check if the field type is a component type
                if (typeof(Component).IsAssignableFrom(field.FieldType) || typeof(GameObject).IsAssignableFrom(field.FieldType))
                {
                    // For the FBUICreator field, assign its creator
                    if (field.FieldType == typeof(FBUICreator))
                    {
                        Component component = selectedObject.GetComponent(typeof(FBUICreator));
                        field.SetValue(scriptComponent, component);
                        EditorUtility.SetDirty(scriptComponent);
                        continue;
                    }

                    foreach (GameObject child in childObjects)
                    {
                        var widgetNamePair = UIConfig.widgetName.FirstOrDefault(kvp => kvp.Value == field.FieldType);
                        if (widgetNamePair.Key != null && child.name.Contains(widgetNamePair.Key))
                        {
                            // if we are assigning a gameobject, then do not use component
                            if (field.FieldType == typeof(GameObject))
                            {
                                field.SetValue(scriptComponent, child);
                                EditorUtility.SetDirty(scriptComponent);
                                break;
                            }
                            Component component = child.GetComponent(field.FieldType);
                            if (component != null)
                            {
                                field.SetValue(scriptComponent, component);
                                EditorUtility.SetDirty(scriptComponent);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}

