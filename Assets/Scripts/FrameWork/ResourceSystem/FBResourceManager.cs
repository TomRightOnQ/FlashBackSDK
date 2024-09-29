using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FlackBackSDK MANAGER
/// Load and manager resources
/// </summary>
public class FBResourceManager : FBGameSystem
{
    public override void OnSystemInit()
    {

    }

    public override void OnSceneChange()
    {

    }

    public override void ManualInit()
    {

    }

    // Public:
    /// <summary>
    /// Load a Prefab's reference
    /// </summary>
    /// <param name="resourcePath"> Resource full path (beginning after Resources folder) </param>
    /// <returns> Prefab reference </returns>
    public GameObject LoadObject(string resourcePath)
    {
        if (resourcePath == "None")
        {
            return null;
        }
        GameObject OutObjectRef = Resources.Load<GameObject>(resourcePath);
        if (OutObjectRef != null)
        {
            return OutObjectRef;
        }
        else
        {
            FBDebug.Instance.FBLogError(string.Format("Unable to get GameObject at {0}", resourcePath), gameObject);
            return null;
        }
    }

    /// <summary>
    /// Load an image as a sprite
    /// </summary>
    /// <param name="resourcePath"> Resource full path (beginning after Resources folder) </param>
    /// <returns> Prefab reference </returns>
    public Sprite LoadImage(string resourcePath)
    {
        resourcePath = resourcePath.Replace(".png", "").Replace(".jpg", "");
        Sprite OutImage = Resources.Load<Sprite>(resourcePath);
        if (OutImage != null)
        {
            return OutImage;
        }
        else
        {
            FBDebug.Instance.FBLogError(string.Format("Unable to get Image at {0}", resourcePath), gameObject);
            return null;
        }
    }
}
