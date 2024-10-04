using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component attached manager - faction relation
/// Manage the relationships among factions
/// Attached to BattleSystem
/// </summary>
public class CFactionRelationManager : MonoBehaviour
{
    private Dictionary<EUnitFaction, Dictionary<EUnitFaction, EFactionRelation>> relations;

    public void SetUp()
    {
        relations = new Dictionary<EUnitFaction, Dictionary<EUnitFaction, EFactionRelation>>();
        foreach (EUnitFaction faction in Enum.GetValues(typeof(EUnitFaction)))
        {
            relations[faction] = new Dictionary<EUnitFaction, EFactionRelation>();
            foreach (EUnitFaction otherFaction in Enum.GetValues(typeof(EUnitFaction)))
            {
                // Initialize all factions as neutral towards each other
                relations[faction][otherFaction] = EFactionRelation.NEUTRAL;
            }
        }
    }

    /// <summary>
    /// Set the realtion between two factions
    /// </summary>
    /// <param name="faction"> Faction A </param>
    /// <param name="otherFaction"> Faction B </param>
    /// <param name="relation"> Relation enum </param>
    public void SetRelation(EUnitFaction faction, EUnitFaction otherFaction, EFactionRelation relation)
    {
        if (relations.ContainsKey(faction) && relations[faction].ContainsKey(otherFaction))
        {
            relations[faction][otherFaction] = relation;
        }
        else
        {
            FBDebug.Instance.FBLogWarning("Faction SET aborted due to invalid parameters", gameObject);
        }
    }

    /// <summary>
    /// Get the relation between two factions
    /// </summary>
    /// <param name="faction"> Faction A </param>
    /// <param name="otherFaction"> Faction B </param>
    /// <returns> Relation enum </returns>
    public EFactionRelation GetRelation(EUnitFaction faction, EUnitFaction otherFaction)
    {
        if (relations.ContainsKey(faction) && relations[faction].ContainsKey(otherFaction))
        {
            return relations[faction][otherFaction];
        }
        else
        {
            FBDebug.Instance.FBLogWarning("Faction returns NO_RELATION due to invalid parameters", gameObject);
        }
        return EFactionRelation.NO_RELATION;
    }

    /// <summary>
    /// Reset all relations among factions
    /// </summary>
    public void ResetAllRelations()
    {
        foreach (var faction in relations.Keys)
        {
            foreach (var otherFaction in relations[faction].Keys)
            {
                relations[faction][otherFaction] = EFactionRelation.NEUTRAL;
            }
        }
    }
}
