using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the tool class of FBUIBase
/// The script contains functions to automatically scan and add the references
/// </summary>
public class FBUICreator : MonoBehaviour
{
    // Path of the script
    [ReadOnly] public string ScriptPath = "Assets/Scripts/";
    [ReadOnly] public UILayer Layer = UILayer.Bottom;
}
