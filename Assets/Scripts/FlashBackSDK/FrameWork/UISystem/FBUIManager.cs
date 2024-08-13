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

    // Root for UI resources
    private static string UI_PREFAB_ROOT = "Art/UI";

    public override void OnSystemInit()
    {

    }

    public override void OnSceneChange()
    {
        // Clear all UIs after scene changed
        openedUIs.Clear();

        // This will find and potentially register already-open UIs at start, if needed
        FBUIBase[] allUIBases = FindObjectsByType<FBUIBase>(FindObjectsSortMode.None);
        foreach (FBUIBase ui in allUIBases)
        {
            openedUIs[ui.name] = ui;
            // UIData uiData = UIConfig.Instance.GetUIData(ui.name);
        }
    }

    public override void ManualInit()
    {

    }

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
            string resourcePath = Path.Combine(UI_PREFAB_ROOT, UIConfig.GetPath(uiName));
            GameObject uiObjectReference = FBMainGame.Game.ResourceManager.LoadObject(resourcePath);
            if (uiObjectReference == null)
            {
                FBDebug.Instance.FBLogWarning(string.Format("UI {0} does not exist", uiName), gameObject);
                return;
            }

            // Instantiate the object
            GameObject uiObjectInstance = FBMainGame.Game.ObjectManager.Instantiate(uiObjectReference);
            FBUIBase currentUIInstance = uiObjectInstance.GetComponent<FBUIBase>();
            if (currentUIInstance == null)
            {
                FBDebug.Instance.FBLogWarning(
                    string.Format("UI {0} does not contains a child of FBUIBase, is it a UI?", uiName), gameObject);
                return;
            }

            // Add UI to the UI list
            openedUIs[uiName] = currentUIInstance;
            currentUIInstance.OnCreate();
        }
        else 
        {
            FBDebug.Instance.FBLogWarning(string.Format("Try to create a CREATED UI {0}", uiName), gameObject);
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
            FBDebug.Instance.FBLogWarning(string.Format("Try to show a NULL UI {0}, creating instead", uiName), gameObject);
            // Try to create first if not existing
            CreateUI(uiName);
        }
        if (!openedUIs.ContainsKey(uiName))
        {
            FBDebug.Instance.FBLogWarning(string.Format("Fail to auto create UI {0}", uiName), gameObject);
            return;
        }
        FBDebug.Instance.FBLog(string.Format("Opening UI {0}", uiName), gameObject);

        FBUIBase currentUIInstance = openedUIs[uiName];
        if (currentUIInstance.bShowing)
        {
            FBDebug.Instance.FBLogWarning(string.Format("UI {0} is already opened", uiName), gameObject);
            return;
        }
        if (!currentUIInstance.bHasOpenedOnce)
        {
            currentUIInstance.bHasOpenedOnce = true;
            currentUIInstance.OnOpen();
        }
        currentUIInstance.bShowing = true;
        currentUIInstance.OnRefresh();
    }

    /// <summary>
    /// Hide a FBUIBase type UI
    /// </summary>
    /// <param name="uiName"> UI Name </param>
    public void HideUI(string uiName)
    {
        if (!openedUIs.ContainsKey(uiName))
        {
            FBDebug.Instance.FBLogWarning(string.Format("Try to hide a NULL UI {0}", uiName), gameObject);
            return;
        }

        FBUIBase currentUIInstance = openedUIs[uiName];
        if (!currentUIInstance.bShowing)
        {
            FBDebug.Instance.FBLogWarning(string.Format("Try to hide a hidden UI {0}", uiName), gameObject);
            return;
        }
        currentUIInstance.bShowing = false;
        currentUIInstance.OnHide();
    }

    /// <summary>
    /// Remove a FBUIBase type UI
    /// </summary>
    /// <param name="uiName"> UI Name </param>
    public void RemoveUI(string uiName)
    {
        if (!openedUIs.ContainsKey(uiName))
        {
            FBDebug.Instance.FBLogWarning(string.Format("Try to remove a NULL UI {0}", uiName), gameObject);
            return;
        }

        FBUIBase currentUIInstance = openedUIs[uiName];
        currentUIInstance.OnRemove();
        // Delete the instance and Dict reference
        openedUIs.Remove(uiName);
        FBMainGame.Game.ObjectManager.Destroy(currentUIInstance.gameObject, true);
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
            return openedUIs[uiName];
        }
        FBDebug.Instance.FBLogWarning(string.Format("Try to get a NULL UI {0}", uiName), gameObject);
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
            openedUIs[uiName].Invoke(functionName, delay);
        }
        FBDebug.Instance.FBLogWarning(string.Format("Try to Invoke a NULL UI {0}", uiName), gameObject);
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
}
