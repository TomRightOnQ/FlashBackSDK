using System;
using System.Collections;
using UnityEngine;
using static BuffData;

/// <summary>
/// BattleSystem - Buff base class
/// </summary>
public abstract class BuffComponentBase : MonoBehaviour
{
    // Buff Manager for this buff instance
    [SerializeField] public CUnitBuffManager manager;
    // Unit that's carrying this buff
    [SerializeField] protected OUnit unit;
    // Unit that causes this buff
    [SerializeField] protected long instigatorUUID;
    // Buff Data Struct
    [SerializeField] protected BuffData.BuffDataStruct buffData;

    [SerializeField] protected EBuffTag tagName = EBuffTag.SAMPLE_BUFF;
    public EBuffTag TagName => tagName;

    [SerializeField] protected EBuffType typeName = EBuffType.NEUTRAL;
    public EBuffType TypeName => typeName;

    [SerializeField] protected int currentLayer = 0;
    public int CurrentLayer => currentLayer;

    // Buff Parameters
    [SerializeField] protected float BuffTime = 1f;
    [SerializeField] protected float BuffTimeDiff = 0.5f;

    // Buff Flags
    [SerializeField] protected bool bActivated = false;
    public bool IsActivated => bActivated;

    [SerializeField] protected float currentLife = 0f;

    // Buff Methods
    /// <summary>
    /// SetUp the buff
    /// </summary>
    /// <param name="buffID"> ID of the buff </param>
    /// <param name="instigator"> (UUID) Giver of this buff </param>
    /// <param name="carryingUnit"> Carrier of this buff </param>
    /// <param name="carryManager"> Unit manager for this buff </param>
    public virtual void SetBuff(int buffID, long instigator, OUnit carryingUnit, CUnitBuffManager carryManager)
    {
        buffData = BuffData.GetData(buffID);
        instigatorUUID = instigator;
        unit = carryingUnit;
        manager = carryManager;

        BuffTime = buffData.BuffTime;
        BuffTimeDiff = buffData.BuffTimeDiff;
    }

    /// <summary>
    /// Layer Up the buff
    /// If the buff is configured as adding refreshing the time,
    /// then it will refresh regardless the maxlayer limit
    /// </summary>
    public virtual void AddLayer()
    {
        if (currentLayer <= buffData.MaxLayer)
        {
            currentLayer += 1;
        }
        if (buffData.bTimeRefresh)
        {
            currentLife = 0f;
        }
    }

    /// <summary>
    /// Layer Down the buff
    /// </summary>
    public virtual void LayerDown(long remover = -1)
    {
        currentLayer -= 1;
        if (currentLayer <= 0)
        {
            RemoveBuff(remover);
        }
    }

    /// <summary>
    /// Start the buff
    /// This will automatically activate the buff
    /// </summary>
    public virtual void StartBuff()
    {
        if (!bActivated)
        {
            OnBuffAdded();
            ActivateBuff();
        }
        if (!buffData.bOneTimeBuff)
        {
            StartCoroutine(buffTimerCycle());
        }
        else 
        {
            RemoveBuff(-1);
        }
    }

    public virtual void ActivateBuff()
    {
        if (bActivated)
        {
            return;
        }
        bActivated = true;
        OnBuffActivated();
    }

    public virtual void DeactivateBuff()
    {
        if (!bActivated)
        {
            return;
        }
        bActivated = false;
        OnBuffDeactivated();
    }

    /// <summary>
    /// Remove a buff
    /// </summary>
    /// <param name="removerID"> Unit that removes this buff </param>
    public virtual void RemoveBuff(long removerID)
    {
        StopAllCoroutines();
        bActivated = false;
        OnBuffRemoved(removerID);
        manager.DeleteBuff(removerID, buffData.ID);
    }

    private IEnumerator buffTimerCycle()
    {
        while (bActivated && currentLife < BuffTime)
        {
            yield return new WaitForSeconds(BuffTimeDiff);
            currentLife += BuffTimeDiff;
            OnBuffTimer();
        }
        if (buffData.bRemoveAll)
        {
            RemoveBuff(-1);
        }
        else 
        {
            LayerDown();
        }
        
    }

    /// <summary>
    /// Called once when buff is added for the first time
    /// </summary>
    public abstract void OnBuffAdded();

    /// <summary>
    /// Called each time the buff activates
    /// </summary>
    public abstract void OnBuffActivated();

    /// <summary>
    /// Called by the timer
    /// </summary>
    public abstract void OnBuffTimer();

    /// <summary>
    /// Called each time the buff deactivates
    /// </summary>
    public abstract void OnBuffDeactivated();

    /// <summary>
    /// Called when the buff is removed
    /// </summary>
    /// <param name="removerID"> -1 if self-removed </param>
    public abstract void OnBuffRemoved(long removerID);
}
 