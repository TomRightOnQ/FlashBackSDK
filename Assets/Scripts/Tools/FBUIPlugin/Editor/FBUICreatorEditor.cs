using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor Inspector section for FBUICreator
/// </summary>
[CustomEditor(typeof(FBUICreator))]
public class FBUICreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default fields first
        DrawDefaultInspector();

        // Add a button to open the window and assign the target
        if (GUILayout.Button("Open FBUICreator Window"))
        {
            FBUICreatorWindow.ShowWindow();
            FBUICreatorWindow.Instance.AssignPrefab(target as FBUICreator);
        }
    }
}
