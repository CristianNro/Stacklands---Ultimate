using System.Collections.Generic;
using UnityEngine;
using StacklandsLike.Cards;

public abstract class CombatantCardData : CardData
{
    [Header("Combatant Identity")]
    public FactionType faction = FactionType.Player;

    [Header("Health")]
    public int maxHealth = 10;

    [Header("Combat")]
    [Min(0)] public int attackDamage = 1;
    [Min(0.01f)] public float attackInterval = 1f;
    public CombatLineRole combatLineRole = CombatLineRole.Melee;
    [Min(0)] public int basePhysicalArmor = 0;
    [Min(0)] public int baseMagicalArmor = 0;
    public CombatDefenseChannel attackDefenseChannel = CombatDefenseChannel.Physical;
    public List<DamageType> attackDamageTypes = new();
    public List<DamageTypeModifierEntry> receivedDamageModifiers = new();
}
