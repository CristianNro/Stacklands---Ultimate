using UnityEngine;
using StacklandsLike.Cards;

public abstract class SurvivorUnitCardData : CombatantCardData
{
    [Header("Unit Identity")]
    public UnitRole unitRole;

    [Header("Needs")]
    public float maxHunger = 100f;
    [Min(1)] public int dailyFoodConsumption = 1;
}
