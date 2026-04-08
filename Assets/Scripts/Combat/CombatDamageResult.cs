using System.Collections.Generic;
using StacklandsLike.Cards;

public class CombatDamageResult
{
    private readonly List<DamageType> attackDamageTypes = new List<DamageType>();

    public int BaseDamage { get; }
    public int FinalDamage { get; }
    public int AppliedArmor { get; }
    public float TotalPercentModifier { get; }
    public CombatDefenseChannel DefenseChannel { get; }
    public IReadOnlyList<DamageType> AttackDamageTypes => attackDamageTypes;

    public CombatDamageResult(
        int baseDamage,
        int finalDamage,
        int appliedArmor,
        float totalPercentModifier,
        CombatDefenseChannel defenseChannel,
        IReadOnlyList<DamageType> attackDamageTypesSource)
    {
        BaseDamage = baseDamage;
        FinalDamage = finalDamage;
        AppliedArmor = appliedArmor;
        TotalPercentModifier = totalPercentModifier;
        DefenseChannel = defenseChannel;

        if (attackDamageTypesSource == null)
            return;

        for (int i = 0; i < attackDamageTypesSource.Count; i++)
            attackDamageTypes.Add(attackDamageTypesSource[i]);
    }
}
