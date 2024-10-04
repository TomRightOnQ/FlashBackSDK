using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// FlackBackSDK MANAGER
/// Manager all UIs (FBUIBase)
/// </summary>
public class FBUIManager : FBGameSystem
{
    // All Opened UIs
    private Dictionary<string, FBUIBase> openedUIs = new Dictionary<string, FBUIBase>();

    // UICanavses 
    private Dictionary<UILayer, GameObject> uiLayers = new Dictionary<UILayer, GameObject>();

    public override void OnSystemInit()
    {
        // When Initialize the UIs, first collect the references
        // of all canvases for different layers
        FBUICanvas[] uiCanvas = FindObjectsOfType<FBUICanvas>();
        foreach (var canvas in uiCanvas)
        {
            uiLayers[canvas.Layer] = canvas.gameObject;
        }
    }

    public override void OnSceneUnloaded()
    {
        // Remove all UIs marked as NON persistent
        // Create a list to hold the keys of the UIs that are not persistent
        List<string> keysToRemove = new List<string>();

        // Iterate through the dictionary to find non-persistent UIs
        foreach (var kvp in openedUIs)
        {
            if (!kvp.Value.config.IsPersistent)
            {
                RemoveUI(kvp.Key);
                keysToRemove.Add(kvp.Key);
            }
        }

        // Remove the non-persistent UIs from the dictionary
        foreach (var key in keysToRemove)
        {
            openedUIs.Remove(key);
        }

        // Hide all UIs
        HideAllUI(false);
    }

    public override void OnSceneChange() { }

    public override void OnSceneLoadComplete()
    {
        // Open all currently created UIs marked as AutoShow visible
        foreach (var kvp in openedUIs)
        {
            FBUIBase currentUIInstance = kvp.Value;
            if (!currentUIInstance.bShowing && currentUIInstance.config.IsAutoShow)
            {
                ShowUI(kvp.Key);
            }
        }
    }

    public override void ManualInit() { }

    // Public:
    // Life Control
    /// <summary>
    /// Create a FBUIBase type UI
    /// </summary>
    /// <param name="uiName"> UI Name </param>
    public void CreateUI(string uiName)
    {
        if (!openedUIs.ContainsKey(uiName))
        {
            FBDebug.Instance.FBLog(string.Format("Creating UI {0}", uiName), gameObject);

            // Get Path and the reference for the prefab
            string resourcePath = UIConfig.data[uiName];
            GameObject uiObjectReference = FBMainGame.System.ResourceManager.LoadObject(resourcePath);
            if (uiObjectReference == null)
            {
#if UNITY_EDITOR || DEBUG
                FBDebug.Instance.FBLogWarning(string.Format("UI {0} does not exist", uiName), gameObject);
#endif
                return;
            }

            // Instantiate the object
            GameObject uiObjectInstance = FBMainGame.System.ObjectManager.Instantiate(uiObjectReference);
            FBUIBase currentUIInstance = uiObjectInstance.GetComponent<FBUIBase>();
            FBUICreator currentUIConfig = uiObjectInstance.GetComponent<FBUICreator>();
            if (currentUIInstance == null)
            {
#if UNITY_EDITOR || DEBUG
                FBDebug.Instance.FBLogWarning(
                    string.Format("UI {0} does not contains a child of FBUIBase, is it a UI?", uiName), gameObject);
#endif
                return;
            }

            // Assign the uiObject to a parent canvas
            GameObject uiCanvasParent = uiLayers[currentUIConfig.Layer];
            // uiCanvasParent must not be null
            if (uiCanvasParent == null)
            {
#if UNITY_EDITOR || DEBUG
                FBDebug.Instance.FBLogError(
                    string.Format("UILayer Canvas does not exist for {0}!", uiName), gameObject);
#endif
                return;
            }

            uiObjectInstance.transform.parent = uiCanvasParent.transform;
            // Add UI to the UI list
            openedUIs[uiName] = currentUIInstance;
            currentUIInstance.OnCreate();
        }
        else 
        {
#if UNITY_EDITOR || DEBUG
            FBDebug.Instance.FBLogWarning(string.Format("Try to create a CREATED UI {0}", uiName), gameObject);
#endif
            return;
        }
    }

    /// <summary>
    /// Show a FBUIBase type UI
    /// </summary>
    /// <param name="uiName"> UI Name </param>
    public void ShowUI(string uiName)
    {
        if (!openedUIs.ContainsKey(uiName))
        {
#if UNITY_EDITOR || DEBUG
            FBDebug.Instance.FBLogWarning(string.Format("Try to show a NULL UI {0}, creating instead", uiName), gameObject);
#endif
            // Try to create first if not existing
            CreateUI(uiName);
        }
        if (!openedUIs.ContainsKey(uiName))
        {
#if UNITY_EDITOR || DEBUG
            FBDebug.Instance.FBLogWarning(string.Format("Fail to auto create UI {0}", uiName), gameObject);
#endif
            return;
        }
        FBDebug.Instance.FBLog(string.Format("Opening UI {0}", uiName), gameObject);

        FBUIBase currentUIInstance = openedUIs[uiName];
        if (currentUIInstance.bShowing)
        {
#if UNITY_EDITOR || DEBUG
            FBDebug.Instance.FBLogWarning(string.Format("UI {0} is already opened", uiName), gameObject);
#endif
            return;
        }
        if (!currentUIInstance.bHasOpenedOnce)
        {
            currentUIInstance.bHasOpenedOnce = true;
            currentUIInstance.OnOpen();
        }
        currentUIInstance.bShowing = true;
        currentUIInstance.gameObject.SetActive(true);
        currentUIInstance.OnRefresh();
    }

    /// <summary>
    /// Hide a FBUIBase type UI
    /// This will disable the root gameObject of the UI widget
    /// </summary>
    /// <param name="uiName"> UI Name </param>
    public void HideUI(string uiName)
    {
        if (!openedUIs.ContainsKey(uiName))
        {
#if UNITY_EDITOR || DEBUG
            FBDebug.Instance.FBLogWarning(string.Format("Try to hide a NULL UI {0}", uiName), gameObject);
#endif
            return;
        }

        FBUIBase currentUIInstance = openedUIs[uiName];
        if (!currentUIInstance.bShowing)
        {
#if UNITY_EDITOR || DEBUG
            FBDebug.Instance.FBLogWarning(string.Format("Try to hide a hidden UI {0}", uiName), gameObject);
#endif
            return;
        }
        currentUIInstance.bShowing = false;
        currentUIInstance.OnHide();
        currentUIInstance.gameObject.SetActive(false);
    }

    /// <summary>
    /// Remove a FBUIBase type UI
    /// </summary>
    /// <param name="uiName"> UI Name </param>
    public void RemoveUI(string uiName)
    {
        if (!openedUIs.ContainsKey(uiName))
        {
#if UNITY_EDITOR || DEBUG
            FBDebug.Instance.FBLogWarning(string.Format("Try to remove a NULL UI {0}", uiName), gameObject);
#endif
            return;
        }

        FBUIBase currentUIInstance = openedUIs[uiName];
        currentUIInstance.OnRemove();
        // Delete the instance and Dict reference
        openedUIs.Remove(uiName);
        FBMainGame.System.ObjectManager.Destroy(currentUIInstance.gameObject, true);
    }

    // Get method
    /// <summary>
    /// Get the instanfce of a FBUIbase type UI
    /// </summary>
    /// <param name="uiName"> UI Name </param>
    /// <returns> UI Instance </returns>
    public FBUIBase GetUIInstance(string uiName)
    {
        if (openedUIs.ContainsKey(uiName))
        {
#if UNITY_EDITOR || DEBUG
            return openedUIs[uiName];
        }
        FBDebug.Instance.FBLogWarning(string.Format("Try to get a NULL UI {0}", uiName), gameObject);
#endif
        return null;
    }

    // Invoke a function of a UI
    /// <summary>
    /// Invoke a function in a FBUIBase UI
    /// </summary>
    /// <param name="uiName"> Name of the UI </param>
    /// <param name="functionName"> Name of the function </param>
    /// <param name="delay"> Delay, default to 0 </param>
    public void Invoke(string uiName, string functionName, float delay = 0f)
    {
        if (openedUIs.ContainsKey(uiName))
        {
#if UNITY_EDITOR || DEBUG
            openedUIs[uiName].Invoke(functionName, delay);
        }
        FBDebug.Instance.FBLogWarning(string.Format("Try to Invoke a NULL UI {0}", uiName), gameObject);
#endif
    }

    // Check UI States
    /// <summary>
    /// Return whether a FBUIBase UI is created, regardless showing or not
    /// </summary>
    /// <param name="uiName"> UI Name </param>
    /// <returns> Created or not </returns>
    public bool IsCreated(string uiName)
    {
        return openedUIs.ContainsKey(uiName);
    }

    /// <summary>
    /// Return whether a FBUIBase UI is showing or not
    /// </summary>
    /// <param name="uiName"> UI Name </param>
    /// <returns> Showing or not </returns>
    public bool IsShow(string uiName)
    {
        return openedUIs.ContainsKey(uiName) && openedUIs[uiName].bShowing;
    }

    /// <summary>
    /// Hide all UIs
    /// </summary>
    /// <param name="bTriggerOnHide"> Trigger OnHide if marked as true </param>
    public void HideAllUI(bool bTriggerOnHide)
    {
        foreach (var kvp in openedUIs)
        {
            FBUIBase currentUIInstance = kvp.Value;
            if (currentUIInstance.bShowing)
            {
                currentUIInstance.bShowing = false;
                if (bTriggerOnHide)
                {
                    currentUIInstance.OnHide();
                }
                currentUIInstance.gameObject.SetActive(false);
            }
        }
    }
}

public enum UILayer
{
    PopUp = 6,
    Top = 5,
    FullScreen = 4,
    HUDTop = 3,
    HUD = 2,
    OnScreen = 1,
    Bottom = 0,
}