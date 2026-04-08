using System.Collections.Generic;
using UnityEngine;
using StacklandsLike.Cards;

// ============================================================
// CombatEncounterDropTrigger
// ------------------------------------------------------------
// Decide si un intento de apilar cartas debe convertirse en
// combate en vez de usar el flujo normal de stacks.
//
// Ownership:
// - lee el contexto de drop
// - clasifica participantes por faccion
// - delega la creacion real a CombatEncounterFactory
//
// No:
// - resuelve dano
// - resuelve layout visual
// ============================================================
public static class CombatEncounterDropTrigger
{
    public static bool TryStartCombat(CardDropContext context)
    {
        if (context == null || context.draggedInstance == null)
            return false;

        if (!CombatFactionUtility.TryGetFaction(context.draggedInstance, out FactionType friendlyFaction))
            return false;

        List<CardInstance> sourceCandidates = CollectSourceCandidates(context);
        List<CardInstance> targetCandidates = CollectTargetCandidates(context);
        CardInstance hostileTarget = FindFirstHostileTarget(context.draggedInstance, targetCandidates);
        if (hostileTarget == null)
            return false;

        if (!CombatFactionUtility.TryGetFaction(hostileTarget, out FactionType enemyFaction))
            return false;

        List<CardInstance> friendlyParticipants = CombatFactionUtility.CollectParticipantsForFaction(sourceCandidates, friendlyFaction);
        List<CardInstance> enemyParticipants = CombatFactionUtility.CollectParticipantsForFaction(targetCandidates, enemyFaction);

        if (friendlyParticipants.Count == 0 || enemyParticipants.Count == 0)
            return false;

        CombatEncounterFactory factory = CombatEncounterFactory.Instance;
        if (factory == null)
        {
            Debug.LogWarning("[CombatEncounterDropTrigger] No existe CombatEncounterFactory activa en escena.");
            return false;
        }

        CombatEncounter encounter = factory.CreateEncounter(
            friendlyParticipants,
            enemyParticipants,
            explicitAnchorPosition: context.boardPoint);

        if (encounter == null)
            return false;

        DetachParticipantsFromStacks(friendlyParticipants);
        DetachParticipantsFromStacks(enemyParticipants);
        return true;
    }

    private static List<CardInstance> CollectSourceCandidates(CardDropContext context)
    {
        List<CardInstance> result = new List<CardInstance>();

        if (context.draggedStack != null)
        {
            IReadOnlyList<CardView> cards = context.draggedStack.Cards;
            for (int i = 0; i < cards.Count; i++)
            {
                CardInstance instance = cards[i] != null ? cards[i].Instance : null;
                if (instance != null)
                    result.Add(instance);
            }

            return result;
        }

        result.Add(context.draggedInstance);
        return result;
    }

    private static List<CardInstance> CollectTargetCandidates(CardDropContext context)
    {
        List<CardInstance> result = new List<CardInstance>();

        if (context.targetStack != null)
        {
            IReadOnlyList<CardView> cards = context.targetStack.Cards;
            for (int i = 0; i < cards.Count; i++)
            {
                CardInstance instance = cards[i] != null ? cards[i].Instance : null;
                if (instance != null)
                    result.Add(instance);
            }

            return result;
        }

        result.Add(context.targetInstance);
        return result;
    }

    private static void DetachParticipantsFromStacks(IReadOnlyList<CardInstance> participants)
    {
        if (participants == null)
            return;

        for (int i = 0; i < participants.Count; i++)
        {
            CardInstance instance = participants[i];
            if (instance == null || instance.View == null)
                continue;

            CardStack currentStack = instance.CurrentStack;
            if (currentStack == null)
                continue;

            currentStack.RemoveCard(instance.View);
        }
    }

    private static CardInstance FindFirstHostileTarget(CardInstance source, IReadOnlyList<CardInstance> targetCandidates)
    {
        if (source == null || targetCandidates == null)
            return null;

        for (int i = 0; i < targetCandidates.Count; i++)
        {
            CardInstance candidate = targetCandidates[i];
            if (CombatFactionUtility.AreHostile(source, candidate))
                return candidate;
        }

        return null;
    }
}
