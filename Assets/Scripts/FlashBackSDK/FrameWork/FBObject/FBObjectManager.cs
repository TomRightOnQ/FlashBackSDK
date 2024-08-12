using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// FlackBackSDK MANAGER
/// Controls the lifecycle of all customized gameobjects (FBObjects)
/// Overloaded Instantiate and Destroy methods
/// </summary>
public class FBObjectManager : FBGameSystem
{
    // Core dictionary to hold all FBObjects
    private Dictionary<long, FBObject> objectDictionary = new Dictionary<long, FBObject>();
    // Sets of object references
    private HashSet<FBObject> objectSet = new HashSet<FBObject>();

    // UUID counter
    [SerializeField, ReadOnly]
    private long currentUUID = 0;

    public override void OnSystemInit()
    {

    }

    public override void ManualInit()
    {

    }

    public override void OnSceneChange()
    {
        base.OnSceneChange();
        // Defer clearing and re-registering of objects until after Awake calls
        StartCoroutine(ClearAndRegisterObjects());
    }

    private IEnumerator ClearAndRegisterObjects()
    {
        yield return null; // Wait one frame to allow all Awake calls to complete

        objectDictionary.Clear();
        objectSet.Clear();
        currentUUID = 0;

        // Register all pre-placed FBObjects in the scene
        FBObject[] allFBObjects = FindObjectsOfType<FBObject>();
        foreach (var FBObject in allFBObjects)
        {
            RegisterObject(FBObject);
        }
    }


    // Instantiate methods group
    /// <summary>
    /// Use the name of the prefab or the reference of the prefab and instantiate with 0
    /// rotation and position by default
    /// IMPORTANT: All pooled object will have NO ACTIVE UUID when not used
    /// </summary>
    /// <param name="prefabName"> Prefab Name </param>
    /// <param name="position"> The start position </param>
    /// <param name="rotation"> The start rotation </param>
    public void Instantiate(string prefabName, Vector3 position, Quaternion rotation)
    {
        /// TODO: Do pooling check here in the future
        /// TODO: Load with the prefab name and get the gameobject reference, then route
        /// to native Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)

    }

    public void Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        /// TODO: Do pooling check here in the future

        GameObject newObject = GameObject.Instantiate(prefab, position, rotation);
        // Check if its an FBObject
        FBObject FBObjectComponent = newObject.GetComponent<FBObject>();
        if (FBObjectComponent != null)
        {
            // Assgin UUID
            RegisterObject(FBObjectComponent);
        }
    }

    public void Instantiate(string prefabName)
    {
        this.Instantiate(prefabName, Vector3.zero, Quaternion.identity);
    }

    public void Instantiate(GameObject prefab)
    {
        this.Instantiate(prefab, Vector3.zero, Quaternion.identity);
    }

    // Get Method
    public FBObject GetObject(long targetUUID)
    {
        if (objectDictionary.TryGetValue(targetUUID, out FBObject objectResult))
        {
            return objectResult;
        }
        return null;
    }

    // Destroy method group
    /// <summary>
    /// Destroy an object by its reference or uuid
    /// </summary>
    /// <param name="targetObject"></param>
    /// <param name="bDelete"> Destroy the object regardless pooling config </param> 
    public void Destroy(GameObject targetObject, bool bDelete = false)
    {
        // Check if its an FBObject
        FBObject FBObjectComponent = targetObject.GetComponent<FBObject>();
        if (FBObjectComponent == null)
        {
            // Native destroy for regular objects
            GameObject.Destroy(targetObject);
            return;
        }

        // Go with the FBObject method
        destroyFBObject(FBObjectComponent);
    }

    public void Destroy(long targetUUID, bool bDelete = false)
    {
        // Track target
        if (!objectDictionary.ContainsKey(targetUUID))
        {
            return;
        }

        // Destroy the object
        FBObject targetFBObject = objectDictionary[targetUUID];
        if (targetFBObject != null)
        {
            destroyFBObject(targetFBObject);
        }
    }

    private void destroyFBObject(FBObject targetFBObject)
    {
        /// TODO: Do pooling check here in the future

        // Remove the reference from both hashset and the dictionary
        if (objectDictionary.ContainsKey(targetFBObject.ObjectUUID))
        {
            objectDictionary.Remove(targetFBObject.ObjectUUID);
            return;
        }
        if (objectSet.Contains(targetFBObject))
        {
            objectSet.Remove(targetFBObject);
        }

        GameObject.Destroy(targetFBObject.gameObject);
    }

    // Dictionary methods
    /// <summary>
    /// Assign a uuid to the object
    /// </summary>
    /// <param name="targetObject"></param>
    public void RegisterObject(FBObject targetObject)
    {
        // Check if the object is already recorded
        if (!objectSet.Contains(targetObject) || targetObject.ObjectUUID >= 0)
        {
            return;
        }

        // Increment the current id and assign it
        currentUUID += 1;
        targetObject.SetFBObjectUUID(currentUUID);
        // Save the references
        objectDictionary.Add(targetObject.ObjectUUID, targetObject);
        objectSet.Add(targetObject);
    }
}
