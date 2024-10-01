using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attached as the persisitent canvases
/// </summary>
public class FBUICanvas : MonoBehaviour
{
    // UILayer of this canvas
    [SerializeField] private UILayer layer = UILayer.Bottom;
    public UILayer Layer => layer;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
}
