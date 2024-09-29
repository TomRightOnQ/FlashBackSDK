using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Allows user to right click and create an empty object with canvas renderer and FBUICreator
/// Then we can use the FBUICreator to config the code and other details
/// </summary>
public class CreateTemplateUIRoot : Editor
{
    [MenuItem("Assets/Create/FlashBackSDK/UI/Template UI Root", false, 10)]
    static void CreateObjectInProject()
    {
        // This method will only be executed when the project is not playing
        if (Application.isPlaying)
            return;

        // Get the current path where the user right-clicked in the Project window
        string currentPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(currentPath) || !Directory.Exists(currentPath))
        {
            currentPath = "Assets"; // Default to the Assets folder if the current path is not valid
        }

        // Load the template prefab from the Assets folder
        GameObject templatePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Art/UI/Widgets/Template/UI_NewWidget.prefab");
        if (templatePrefab == null)
        {
            Debug.LogError("Template prefab not found at specified path.");
            return;
        }

        // Create a unique path for the new prefab
        string newPrefabPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(currentPath, "NewTemplateUIRoot.prefab"));

        GameObject newObject = PrefabUtility.InstantiatePrefab(templatePrefab) as GameObject;
        if (newObject == null)
        {
            Debug.LogError("Failed to instantiate prefab.");
            return;
        }

        // Unpack the prefab instance in the scene to make it independent
        PrefabUtility.UnpackPrefabInstance(newObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

        // Save the newObject as a new independent prefab asset
        PrefabUtility.SaveAsPrefabAsset(newObject, newPrefabPath);

        DestroyImmediate(newObject);
        AssetDatabase.Refresh();
    }
}
