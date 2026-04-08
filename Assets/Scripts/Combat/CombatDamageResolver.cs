using StacklandsLike.Cards;
using UnityEngine;

// ============================================================
// CombatDamageResolver
// ------------------------------------------------------------
// Resuelve la matematica de dano V2 fuera del resolver general
// del encounter.
//
// Responsabilidades:
// - leer dano base del atacante
// - decidir armadura fisica o magica
// - aplicar modificadores por DamageType
// - devolver un resultado final reutilizable
// ============================================================
public class CombatDamageResolver : MonoBehaviour
{
    public CombatDamageResult Resolve(CardInstance attacker, CardInstance target)
    {
        CombatParticipantRuntime attackerRuntime = attacker != null ? attacker.CombatParticipantRuntime : null;
        CombatParticipantRuntime targetRuntime = target != null ? target.CombatParticipantRuntime : null;
        CombatantCardData attackerData = attackerRuntime != null ? attackerRuntime.CombatantData : null;
        CombatantCardData targetData = targetRuntime != null ? targetRuntime.CombatantData : null;

        int baseDamage = attackerData != null ? Mathf.Max(0, attackerData.attackDamage) : 0;
        CombatDefenseChannel defenseChannel = attackerData != null
            ? attackerData.attackDefenseChannel
            : CombatDefenseChannel.Physical;

        int appliedArmor = ResolveArmor(targetData, defenseChannel);
        float damageAfterArmor = Mathf.Max(0f, baseDamage - appliedArmor);
        float totalPercentModifier = ResolveTotalPercentModifier(attackerData, targetData);
        float finalDamageFloat = damageAfterArmor * Mathf.Max(0f, 1f + totalPercentModifier);
        int finalDamage = Mathf.Max(0, Mathf.RoundToInt(finalDamageFloat));

        return new CombatDamageResult(
            baseDamage,
            finalDamage,
            appliedArmor,
            totalPercentModifier,
            defenseChannel,
            attackerData != null ? attackerData.attackDamageTypes : null);
    }

    private int ResolveArmor(CombatantCardData targetData, CombatDefenseChannel defenseChannel)
    {
        if (targetData == null)
            return 0;

        return defenseChannel == CombatDefenseChannel.Magical
            ? Mathf.Max(0, targetData.baseMagicalArmor)
            : Mathf.Max(0, targetData.basePhysicalArmor);
    }

    private float ResolveTotalPercentModifier(CombatantCardData attackerData, CombatantCardData targetData)
    {
        if (attackerData == null || attackerData.attackDamageTypes == null || attackerData.attackDamageTypes.Count == 0)
            return 0f;

        if (targetData == null || targetData.receivedDamageModifiers == null || targetData.receivedDamageModifiers.Count == 0)
            return 0f;

        float totalModifier = 0f;

        for (int i = 0; i < attackerData.attackDamageTypes.Count; i++)
        {
            DamageType attackDamageType = attackerData.attackDamageTypes[i];
            for (int j = 0; j < targetData.receivedDamageModifiers.Count; j++)
            {
                DamageTypeModifierEntry modifierEntry = targetData.receivedDamageModifiers[j];
                if (modifierEntry == null || modifierEntry.damageType != attackDamageType)
                    continue;

                totalModifier += modifierEntry.percentModifier;
            }
        }

        return totalModifier;
    }
}
