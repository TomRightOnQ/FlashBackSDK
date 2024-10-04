using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component attached manager - Unit Buff
/// Manage and Control the bufflist of an OUnit type
/// </summary>
public class CUnitBuffManager : MonoBehaviour
{
    // OUnit on this manager
    [SerializeField] private OUnit parentUnit;

    // BuffDict
    [SerializeField] private Dictionary<int, BuffComponentBase> buffDict = new Dictionary<int, BuffComponentBase>();
    public Dictionary<int, BuffComponentBase> BuffDict => buffDict;

    // BuffNum
    private Dictionary<EBuffTag, int> buffCountDict = new Dictionary<EBuffTag, int>();

    public void SetReference(OUnit parent)
    {
        parentUnit = parent;
    }

    /// <summary>
    /// Get the total count of buffs on the unit
    /// </summary>
    /// <returns> Count </returns>
    public int GetBuffCount()
    {
        int outCount = 0;
        foreach (KeyValuePair<EBuffTag, int> kvp in buffCountDict)
        {
            outCount += kvp.Value;
        }
        return outCount;
    }

    /// <summary>
    /// Get the total count of buffs for one type
    /// </summary>
    /// <param name="targetTag"> Tag seraching for </param>
    /// <returns> Count </returns>
    public int GetBuffCountByTag(EBuffTag targetTag)
    {
        if (buffCountDict.ContainsKey(targetTag))
        {
            return buffCountDict[targetTag];
        }
        return 0;
    }

    // Call by BuffSystem only
    public void AddBuff(long instigator, int buffID)
    {
        // Check if the buff is already there
        BuffData.BuffDataStruct buffData = BuffData.GetData(buffID);
        if (buffDict.ContainsKey(buffID) && buffDict[buffID] != null)
        {
            buffDict[buffID].AddLayer();
            return;
        }

        string buffClass = buffData.BuffClass;
        Type type = Type.GetType(buffClass);

        if (type == null)
        {
#if UNITY_EDITOR || DEBUG
            FBDebug.Instance.FBLogError($"Buff class '{buffClass}' not found.", gameObject);
#endif
            return;
        }

        if (!typeof(BuffComponentBase).IsAssignableFrom(type))
        {
#if UNITY_EDITOR || DEBUG
            FBDebug.Instance.FBLogError($"Buff class '{buffClass}' does not inherit from BuffComponentBase.", gameObject);
#endif
            return;
        }

        BuffComponentBase buffComponent = (BuffComponentBase)Activator.CreateInstance(type);
        buffComponent.SetBuff(buffID, instigator, parentUnit, this);

        // Write buff data to dicts
        buffDict[buffID] = buffComponent;

        if (!buffCountDict.ContainsKey(buffData.BuffTag))
        {
            buffCountDict[buffData.BuffTag] = 0;
        }
        buffCountDict[buffData.BuffTag] += 1;

        buffComponent.StartBuff();
    }

    // Call by BuffSystem only
    public void RemoveBuff(long remover, int buffID)
    {
        // Check if the buff is already there
        if (!buffDict.ContainsKey(buffID))
        {
            return;
        }
        // If this layerdown triggers deletion of the buff
        // Deletebuff will handle it
        buffDict[buffID].LayerDown(remover);
    }

    /// <summary>
    /// Delete the buff component
    /// </summary>
    /// <param name="remover"> Remover's UUID </param>
    /// <param name="buffID"> Buff ID </param>
    public void DeleteBuff(long remover, int buffID)
    {
        if (!buffDict.TryGetValue(buffID, out BuffComponentBase buffComponent) || buffComponent == null)
        {
            return;
        }

        BuffData.BuffDataStruct buffData = BuffData.GetData(buffID);
        UnityEngine.Object.Destroy(buffDict[buffID]);
        buffDict.Remove(buffID);
        buffCountDict[buffData.BuffTag] = Mathf.Max(0, buffCountDict[buffData.BuffTag] - 1);
        if (buffCountDict[buffData.BuffTag] == 0)
        {
            buffCountDict.Remove(buffData.BuffTag);
        }
    }
}
