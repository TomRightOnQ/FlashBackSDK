using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FlackBackSDK MANAGER
/// Manager OUnits and world components
/// </summary>
public class FBBattleSystem : FBGameSystem
{
    // Data
    // OUnit Dict - Record units from the same faction per list
    // example: { Friendly : [ unit1, unit2 ] }
    protected Dictionary<EUnitFaction, List<long>> unitDict = new Dictionary<EUnitFaction, List<long>>();
    public Dictionary<EUnitFaction, List<long>> UnitDict => unitDict;

    // Faction relation manager
    protected CFactionRelationManager factionRelationManager;
    public CFactionRelationManager FactionRelationManager => factionRelationManager;

    public override void OnSystemCreate()
    {
        base.OnSystemCreate();
        factionRelationManager = gameObject.AddComponent<CFactionRelationManager>();
        factionRelationManager.SetUp();
    }

    public override void OnSystemInit() { }

    public override void OnSceneUnloaded()
    {
        // Clear the dictionary 
        foreach (var key in unitDict.Keys)
        {
            unitDict[key].Clear();
        }
        unitDict.Clear();
    }

    public override void OnSceneChange() { }

    public override void OnSceneLoadComplete() { }

    public override void ManualInit() { }

    // Method groups
    /// <summary>
    /// Spawn a unit
    /// </summary>
    /// <param name="unitName"> Prefab Name of the object </param>
    /// <param name="position"> Position </param>
    /// <param name="rotation"> Rotation </param>
    /// <param name="faction"> Faction for this unit </param>
    /// <param name="bActive"> Active by default </param>
    public void SpawnUnit(string unitName, Vector3 position, Quaternion rotation, EUnitFaction faction, bool bActive)
    {
        // Route to FBObjectManager
        GameObject objectReference = FBMainGame.System.ObjectManager.Instantiate(unitName, position, rotation);
        if (objectReference == null)
        {
            return;
        }

        // Get the OUnit and UUID for record
        OUnit unitReference = objectReference.GetComponent<OUnit>();
        if (objectReference == null)
        {
            FBDebug.Instance.FBLogError("Unit spawned, but does not have an OUnit attached", gameObject);
            return;
        }

        long uuid = unitReference.ObjectUUID;
        unitReference.SetUnit(faction);
        recordUnitInfo(faction, uuid);
    }

    /// <summary>
    /// Get the count of the units of a certain faction
    /// </summary>
    /// <param name="faction"> Faction searching for </param>
    public int GetFactionUnitCount(EUnitFaction faction)
    {
        if (!unitDict.ContainsKey(faction))
        {
            return 0;
        }
        else 
        {
            return unitDict[faction].Count;
        }
    }

    /// <summary>
    /// Check if an OUnit fits the target type
    /// </summary>
    /// <param name="targetUnit"> Target to check </param>
    /// <param name="relationsAllowed"> Filter the relations </param>
    /// <param name="factionsAllowed"> Filter by the target's faction </param>
    /// <return> If the target fits </return>
    public bool FilterTarget(OUnit instigator, OUnit targetUnit, EFactionRelation[] relationsAllowed, EUnitFaction[] factionsAllowed)
    {
        if (factionsAllowed != null && Array.IndexOf(factionsAllowed, targetUnit.UnitFaction) == -1)
        {
            return false;
        }
        EFactionRelation relation = factionRelationManager.GetRelation(instigator.UnitFaction, targetUnit.UnitFaction);
        if (relationsAllowed != null && Array.IndexOf(relationsAllowed, relation) == -1)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Implement your own method to set the attr of a unit
    /// </summary>
    public void SetUnitAttr()
    {
    
    }


    /// <summary>
    /// Remove a unit by unit reference, gameobject or uuid
    /// </summary>
    /// //////////////////////////////////////////////////////////////
    public void DestroyUnit(OUnit unit)
    {
        removeUnitInfo(unit.UnitFaction, unit.ObjectUUID);
        FBMainGame.System.ObjectManager.Destroy(unit.gameObject);
    }

    public void DestroyUnit(GameObject targetObject)
    {
        OUnit unitReference = targetObject.GetComponent<OUnit>();
        if (unitReference != null)
        {
            removeUnitInfo(unitReference.UnitFaction, unitReference.ObjectUUID);
            FBMainGame.System.ObjectManager.Destroy(targetObject);
        }
    }

    public void DestroyUnit(long uuid)
    {
        OUnit unitReference = GetUnit(uuid);
        if (unitReference != null)
        {
            removeUnitInfo(unitReference.UnitFaction, uuid);
            FBMainGame.System.ObjectManager.Destroy(uuid);
        }
        else 
        {
            FBDebug.Instance.FBLogWarning("UUID is not referring to an OUnit, destroy as FBObject", gameObject);
        }
    }

    public OUnit GetUnit(long uuid)
    {
        FBObject fBObjectReference = FBMainGame.System.ObjectManager.GetObject(uuid);
        if (fBObjectReference is OUnit)
        {
            return fBObjectReference as OUnit;
        }
        return null;
    }

    /// //////////////////////////////////////////////////////////////

    /// <summary>
    /// Change the faction of unit by unit reference, gameobject or uuid
    /// </summary>
    /// //////////////////////////////////////////////////////////////
    public void ChangeUnitFaction(OUnit unit, EUnitFaction faction)
    {
        unit.ChangeFaction(faction);
        changeUnitInfo(faction, unit.ObjectUUID);
    }

    public void ChangeUnitFaction(GameObject targetObject, EUnitFaction faction)
    {
        OUnit unitReference = targetObject.GetComponent<OUnit>();
        if (unitReference != null)
        {
            ChangeUnitFaction(unitReference, faction);
        }
    }

    public void ChangeUnitFaction(long uuid, EUnitFaction faction)
    {
        // Get the OUnit
        FBObject fBObjectReference = FBMainGame.System.ObjectManager.GetObject(uuid);
        if (fBObjectReference is OUnit)
        {
            ChangeUnitFaction(fBObjectReference as OUnit, faction);
        }
    }
    /// //////////////////////////////////////////////////////////////
    
    /// <summary>
    /// Modify the unit info to the dicti0nary
    /// </summary>
    /// <param name="faction"> Faction of the unit </param>
    /// <param name="uuid"> UUID of the unit </param>
    private void recordUnitInfo(EUnitFaction faction, long uuid)
    {
        if (!unitDict.ContainsKey(faction))
        {
            unitDict[faction] = new List<long>();
        }

        unitDict[faction].Add(uuid);
    }

    private void changeUnitInfo(EUnitFaction faction, long uuid)
    {
        if (!unitDict.ContainsKey(faction))
        {
            unitDict[faction] = new List<long>();
        }

        foreach (var factionKey in unitDict.Keys)
        {
            unitDict[factionKey].Remove(uuid);
        }

        recordUnitInfo(faction, uuid);
    }

    private void removeUnitInfo(EUnitFaction faction, long uuid)
    {
        if (unitDict.ContainsKey(faction))
        {
            unitDict[faction].Remove(uuid);
        }
    }
}
