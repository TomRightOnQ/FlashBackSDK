using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attached as the persisitent canvases
/// </summary>
public class FBUICanvas : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
}
