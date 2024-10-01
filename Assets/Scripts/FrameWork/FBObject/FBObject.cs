using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The base object type of the project
/// Derived from monobehaviours
/// UUID ready
/// Only one FBObject is allowed per gameobject; feel free to attach other types of components
/// </summary>
public class FBObject : MonoBehaviour
{
    // UUID
    [SerializeField, ReadOnly]
    private long objectUUID = -1;
    public long ObjectUUID => objectUUID;

    /// <summary>
    /// Constructor of the object
    /// </summary>
    /// <param name="newID"></param>
    public FBObject(long newID = -1)
    {
        objectUUID = newID;
    }

    // Register the object at its awake
    // Do not override
    private void Awake()
    {
        // Assign a uuid if the default id is not assigned
        if (objectUUID < 0)
        {
            // Call manager to record the object
            FBMainGame.System.Get<FBObjectManager>().RegisterObject(this);
        }
    }

    // Ban Start from being overriden
    private void Start(){ }

    // Overriden awake manually controlled
    protected virtual void C_Awake()
    {
    
    }

    /// <summary>
    /// Methods for setting the uuid
    /// </summary>
    /// <param name="newID"></param>
    public void SetFBObjectUUID(long newID)
    {
        objectUUID = newID;
    }
}
