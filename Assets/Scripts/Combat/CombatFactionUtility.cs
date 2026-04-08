using System.Collections.Generic;
using StacklandsLike.Cards;

// ============================================================
// CombatFactionUtility
// ------------------------------------------------------------
// Helpers chicos para decidir si dos cartas pueden entrar en
// combate y para agrupar participantes por faccion.
//
// Regla V1:
// - misma faccion   => no combaten
// - distinta faccion => pueden combatir
// ============================================================
public static class CombatFactionUtility
{
    public static bool TryGetFaction(CardInstance instance, out FactionType faction)
    {
        faction = FactionType.Neutral;

        CombatParticipantRuntime runtime = instance != null ? instance.CombatParticipantRuntime : null;
        if (runtime == null || !runtime.isActiveAndEnabled || runtime.CombatantData == null)
            return false;

        faction = runtime.CombatantData.faction;
        return true;
    }

    public static bool AreHostile(CardInstance first, CardInstance second)
    {
        if (!TryGetFaction(first, out FactionType firstFaction))
            return false;

        if (!TryGetFaction(second, out FactionType secondFaction))
            return false;

        return firstFaction != secondFaction;
    }

    public static List<CardInstance> CollectParticipantsForFaction(IReadOnlyList<CardInstance> source, FactionType faction)
    {
        List<CardInstance> result = new List<CardInstance>();
        if (source == null)
            return result;

        for (int i = 0; i < source.Count; i++)
        {
            CardInstance instance = source[i];
            if (!TryGetFaction(instance, out FactionType instanceFaction))
                continue;

            if (instanceFaction != faction)
                continue;

            result.Add(instance);
        }

        return result;
    }
}
