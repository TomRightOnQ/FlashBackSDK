/// <summary>
/// FlackBackSDK MANAGER
/// Manage the attachment and other behaviours of buffs on OUnit
/// </summary>
public class FBBuffSystem : FBGameSystem
{
    // Methods:
    /// <summary>
    /// Add a buff to a unit
    /// </summary>
    /// <param name="target"> Unit reference of the target </param>
    /// <param name="instigator"> UUID of the causer of the buff </param>
    /// <param name="buffID"> Buff ID </param>
    public void AddBuff(OUnit target, long instigator, int buffID)
    {
        if (target.BuffManager != null)
        {
            target.BuffManager.AddBuff(instigator, buffID);
        }
    }

    /// <summary>
    /// Remove a buff on a unit
    /// If the buff has multiple layers, this will only remove a layer from it
    /// </summary>
    /// <param name="target"> Unit reference of the target </param>
    /// <param name="remover"> UUID of the object that removes the buff, put -1 if self or none </param>
    /// <param name="buffID"> Buff ID </param>
    public void RemoveBuff(OUnit target, long remover, int buffID)
    {
        if (target.BuffManager != null)
        {
            target.BuffManager.RemoveBuff(remover, buffID);
        }
    }

    /// <summary>
    /// Delete a buff on a unit regardless layers
    /// </summary>
    /// <param name="target"> Unit reference of the target </param>
    /// <param name="remover"> UUID of the object that removes the buff, put -1 if self or none </param>
    /// <param name="buffID"> Buff ID </param>
    public void DeleteBuff(OUnit target, long remover, int buffID)
    {
        if (target.BuffManager != null)
        {
            target.BuffManager.DeleteBuff(remover, buffID);
        }
    }
}
