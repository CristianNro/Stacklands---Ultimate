using System.Collections.Generic;
using UnityEngine;
using StacklandsLike.Cards;

public static class CardDropCombatHandler
{
    public static CardDropResolutionResult TryResolve(CardDropContext context)
    {
        if (context == null || context.targetCombatEncounter == null)
            return null;

        if (TryAddUnitsToEncounter(context))
            return CardDropResolutionResult.HandledWithoutBoardPlacement();

        if (ContainsAnyUnit(context))
        {
            Vector2 blockedUnitPosition = ResolveSafeBoardPosition(context);
            return CardDropResolutionResult.BoardFallbackAt(blockedUnitPosition);
        }

        if (IsCombatEncounterCompatible(context))
            return CardDropResolutionResult.BoardFallback();

        Vector2 safeBoardPosition = ResolveSafeBoardPosition(context);
        return CardDropResolutionResult.BoardFallbackAt(safeBoardPosition);
    }

    private static bool TryAddUnitsToEncounter(CardDropContext context)
    {
        CombatEncounter encounter = context != null ? context.targetCombatEncounter : null;
        CombatEncounterFactory factory = CombatEncounterFactory.Instance;
        if (encounter == null || factory == null)
            return false;

        List<CardInstance> draggedUnits = CollectDraggedUnits(context);
        if (draggedUnits.Count == 0)
            return false;

        if (!TryResolveEncounterFactions(encounter, out FactionType friendlyFaction, out FactionType enemyFaction))
            return false;

        List<CardInstance> friendlyReinforcements = new List<CardInstance>();
        List<CardInstance> enemyReinforcements = new List<CardInstance>();

        for (int i = 0; i < draggedUnits.Count; i++)
        {
            CardInstance instance = draggedUnits[i];
            if (!CombatFactionUtility.TryGetFaction(instance, out FactionType faction))
                return false;

            if (faction == friendlyFaction)
            {
                friendlyReinforcements.Add(instance);
                continue;
            }

            if (faction == enemyFaction)
            {
                enemyReinforcements.Add(instance);
                continue;
            }

            return false;
        }

        bool addedAny = false;

        if (friendlyReinforcements.Count > 0)
            addedAny |= factory.TryAddParticipantsToEncounter(encounter, friendlyReinforcements, CombatTeam.Friendly);

        if (enemyReinforcements.Count > 0)
            addedAny |= factory.TryAddParticipantsToEncounter(encounter, enemyReinforcements, CombatTeam.Enemy);

        return addedAny;
    }

    private static bool IsCombatEncounterCompatible(CardDropContext context)
    {
        if (context.draggedStack != null)
        {
            IReadOnlyList<CardView> stackCards = context.draggedStack.Cards;
            for (int i = 0; i < stackCards.Count; i++)
            {
                CardInstance instance = stackCards[i] != null ? stackCards[i].Instance : null;
                if (!IsCombatEncounterCompatible(instance))
                    return false;
            }

            return stackCards.Count > 0;
        }

        return IsCombatEncounterCompatible(context.draggedInstance);
    }

    private static bool IsCombatEncounterCompatible(CardInstance instance)
    {
        if (instance == null || instance.data == null)
            return false;

        CardType cardType = instance.data.cardType;
        return cardType == CardType.Unit || cardType == CardType.Enemy || cardType == CardType.Item;
    }

    private static bool ContainsAnyUnit(CardDropContext context)
    {
        List<CardInstance> draggedInstances = CollectDraggedInstances(context);
        for (int i = 0; i < draggedInstances.Count; i++)
        {
            CardInstance instance = draggedInstances[i];
            if (instance != null && instance.data is CombatantCardData)
                return true;
        }

        return false;
    }

    private static List<CardInstance> CollectDraggedUnits(CardDropContext context)
    {
        List<CardInstance> result = new List<CardInstance>();
        List<CardInstance> draggedInstances = CollectDraggedInstances(context);

        for (int i = 0; i < draggedInstances.Count; i++)
        {
            CardInstance instance = draggedInstances[i];
            if (!IsJoinableUnit(instance))
                continue;

            result.Add(instance);
        }

        return result;
    }

    private static List<CardInstance> CollectDraggedInstances(CardDropContext context)
    {
        List<CardInstance> result = new List<CardInstance>();
        if (context == null)
            return result;

        if (context.draggedStack != null)
        {
            IReadOnlyList<CardView> stackCards = context.draggedStack.Cards;
            for (int i = 0; i < stackCards.Count; i++)
            {
                CardInstance instance = stackCards[i] != null ? stackCards[i].Instance : null;
                if (instance != null)
                    result.Add(instance);
            }

            return result;
        }

        if (context.draggedInstance != null)
            result.Add(context.draggedInstance);

        return result;
    }

    private static bool IsJoinableUnit(CardInstance instance)
    {
        if (instance == null || !(instance.data is CombatantCardData))
            return false;

        CombatParticipantRuntime runtime = instance.CombatParticipantRuntime;
        if (runtime == null || !runtime.isActiveAndEnabled || runtime.CombatantData == null)
            return false;

        if (runtime.IsInCombat || runtime.IsDead())
            return false;

        return true;
    }

    private static bool TryResolveEncounterFactions(CombatEncounter encounter, out FactionType friendlyFaction, out FactionType enemyFaction)
    {
        friendlyFaction = FactionType.Neutral;
        enemyFaction = FactionType.Neutral;

        return TryGetTeamFaction(encounter, CombatTeam.Friendly, out friendlyFaction)
            && TryGetTeamFaction(encounter, CombatTeam.Enemy, out enemyFaction);
    }

    private static bool TryGetTeamFaction(CombatEncounter encounter, CombatTeam team, out FactionType faction)
    {
        faction = FactionType.Neutral;
        if (encounter == null)
            return false;

        IReadOnlyList<CardInstance> participants = encounter.GetParticipants(team);
        if (participants == null)
            return false;

        for (int i = 0; i < participants.Count; i++)
        {
            CardInstance instance = participants[i];
            if (instance == null)
                continue;

            if (!CombatFactionUtility.TryGetFaction(instance, out FactionType participantFaction))
                continue;

            faction = participantFaction;
            return true;
        }

        return false;
    }

    private static Vector2 ResolveSafeBoardPosition(CardDropContext context)
    {
        Vector2 preferredPosition = context.boardPoint;
        RectTransform draggedRect = GetDraggedRect(context);

        if (BoardRoot.Instance == null || draggedRect == null)
            return preferredPosition;

        Rect encounterBounds = context.targetCombatEncounter != null
            ? context.targetCombatEncounter.GetBoundsInBoardSpace()
            : new Rect();

        Vector2 clampedPreferred = BoardRoot.Instance.GetClampedPosition(preferredPosition, draggedRect);
        Vector2 bestCandidate = FindNearestPositionOutsideEncounter(clampedPreferred, draggedRect, encounterBounds);

        return BoardRoot.Instance.FindNearestFreePositionForRect(
            bestCandidate,
            draggedRect,
            minimumVisibleFraction: 0.55f,
            searchStep: 60f,
            maxSearchRings: 12,
            ignoreRect: draggedRect);
    }

    private static RectTransform GetDraggedRect(CardDropContext context)
    {
        if (context == null)
            return null;

        if (context.draggedStack != null)
            return context.draggedStack.GetComponent<RectTransform>();

        return context.draggedCard != null ? context.draggedCard.GetComponent<RectTransform>() : null;
    }

    private static Vector2 FindNearestPositionOutsideEncounter(Vector2 preferredPosition, RectTransform draggedRect, Rect encounterBounds)
    {
        if (draggedRect == null || encounterBounds.width <= 0f || encounterBounds.height <= 0f || BoardRoot.Instance == null)
            return preferredPosition;

        if (!IntersectsEncounter(draggedRect, preferredPosition, encounterBounds))
            return preferredPosition;

        Vector2 size = GetDraggedSizeInBoardSpace(draggedRect);
        float halfWidth = size.x * 0.5f;
        float halfHeight = size.y * 0.5f;
        float margin = 12f;

        Vector2[] baseCandidates =
        {
            new Vector2(encounterBounds.xMin - halfWidth - margin, preferredPosition.y),
            new Vector2(encounterBounds.xMax + halfWidth + margin, preferredPosition.y),
            new Vector2(preferredPosition.x, encounterBounds.yMax + halfHeight + margin),
            new Vector2(preferredPosition.x, encounterBounds.yMin - halfHeight - margin)
        };

        Vector2 best = preferredPosition;
        float bestDistance = float.PositiveInfinity;

        for (int i = 0; i < baseCandidates.Length; i++)
        {
            Vector2 candidate = BoardRoot.Instance.GetClampedPosition(baseCandidates[i], draggedRect);
            if (IntersectsEncounter(draggedRect, candidate, encounterBounds))
                continue;

            float distance = (candidate - preferredPosition).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }

        if (bestDistance < float.PositiveInfinity)
            return best;

        return preferredPosition;
    }

    private static bool IntersectsEncounter(RectTransform draggedRect, Vector2 anchoredPosition, Rect encounterBounds)
    {
        Rect draggedBounds = GetDraggedBoundsInBoardSpace(draggedRect, anchoredPosition);
        return draggedBounds.width > 0f
            && draggedBounds.height > 0f
            && encounterBounds.Overlaps(draggedBounds);
    }

    private static Rect GetDraggedBoundsInBoardSpace(RectTransform draggedRect, Vector2 anchoredPosition)
    {
        if (draggedRect == null)
            return new Rect();

        CardStack stack = draggedRect.GetComponent<CardStack>();
        if (stack != null)
        {
            stack.GetVisualExtents(out float left, out float right, out float bottom, out float top);
            return Rect.MinMaxRect(
                anchoredPosition.x - left,
                anchoredPosition.y - bottom,
                anchoredPosition.x + right,
                anchoredPosition.y + top);
        }

        Vector2 size = GetDraggedSizeInBoardSpace(draggedRect);
        Vector2 halfSize = size * 0.5f;
        return Rect.MinMaxRect(
            anchoredPosition.x - halfSize.x,
            anchoredPosition.y - halfSize.y,
            anchoredPosition.x + halfSize.x,
            anchoredPosition.y + halfSize.y);
    }

    private static Vector2 GetDraggedSizeInBoardSpace(RectTransform draggedRect)
    {
        Vector3 scale = draggedRect.lossyScale;
        return new Vector2(
            draggedRect.rect.width * scale.x,
            draggedRect.rect.height * scale.y);
    }
}
