/// <summary>
/// FBBattleSystem - Unit factions
/// </summary>
public enum EUnitFaction
{
    ALL = 1000,
    INVULNERABLE_EXCEPT = 999,
    SPECIAL = 998,
    FRIEND = 0,
    HOSTILE = 1,
    NEUTRAL = 2,
    OBJECT = 3,
}

public enum EFactionRelation
{
    NO_RELATION = 999,
    ALLY = 0,
    ENEMY = 1,
    NEUTRAL = 2,
}