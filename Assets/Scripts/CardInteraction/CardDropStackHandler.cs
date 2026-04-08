using System.Collections.Generic;
using UnityEngine;

public static class CardDropStackHandler
{
    public static CardDropResolutionResult TryResolve(CardDropContext context)
    {
        if (context == null || (context.targetCard == null && context.targetStack == null))
            return null;

        return context.draggedStack != null
            ? TryResolveStackDrop(context)
            : TryResolveSingleCardDrop(context);
    }

    private static CardDropResolutionResult TryResolveStackDrop(CardDropContext context)
    {
        if (CombatEncounterDropTrigger.TryStartCombat(context))
            return CardDropResolutionResult.HandledWithoutBoardPlacement();

        CardStack targetStack = context.targetStack;
        List<CardView> draggedCardsCopy = new List<CardView>(context.draggedStack.Cards);

        if (targetStack != null && targetStack != context.draggedStack)
        {
            if (!targetStack.CanAcceptCards(draggedCardsCopy))
                return CardDropResolutionResult.BoardFallback();

            for (int i = 0; i < draggedCardsCopy.Count; i++)
                targetStack.AddCard(draggedCardsCopy[i]);

            Object.Destroy(context.draggedStack.gameObject);
            return CardDropResolutionResult.HandledWithoutBoardPlacement();
        }

        if (targetStack == null)
        {
            if (!CardStackFactory.CanCreateStack(context.targetCard, draggedCardsCopy))
                return CardDropResolutionResult.BoardFallback();

            CardStack newStack = CardStackFactory.CreateStack(context.targetCard, draggedCardsCopy[0]);
            if (newStack == null)
                return CardDropResolutionResult.BoardFallback();

            for (int i = 1; i < draggedCardsCopy.Count; i++)
                newStack.AddCard(draggedCardsCopy[i]);

            Object.Destroy(context.draggedStack.gameObject);
            return CardDropResolutionResult.HandledWithoutBoardPlacement();
        }

        return TryResolveSingleCardDrop(context);
    }

    private static CardDropResolutionResult TryResolveSingleCardDrop(CardDropContext context)
    {
        if (CombatEncounterDropTrigger.TryStartCombat(context))
            return CardDropResolutionResult.HandledWithoutBoardPlacement();

        CardStack existingTargetStack = context.targetStack;
        if (existingTargetStack != null)
        {
            if (!existingTargetStack.CanAcceptCard(context.draggedCard, out string rejectionReason))
            {
                Debug.Log(
                    $"[CardDropStackHandler] Rejected add to existing stack '{existingTargetStack.name}'. Dragged='{context.draggedCard?.name ?? "null"}'. Reason: {rejectionReason}",
                    existingTargetStack
                );
                return CardDropResolutionResult.BoardFallback();
            }

            existingTargetStack.AddCard(context.draggedCard);
            return CardDropResolutionResult.HandledWithoutBoardPlacement();
        }

        if (!CardStackFactory.CanCreateStack(context.targetCard, new List<CardView> { context.draggedCard }))
        {
            Debug.Log(
                $"[CardDropStackHandler] Rejected new stack creation. Target='{context.targetCard?.name ?? "null"}', dragged='{context.draggedCard?.name ?? "null"}'.",
                context.targetCard
            );
            return CardDropResolutionResult.BoardFallback();
        }

        CardStack newStack = CardStackFactory.CreateStack(context.targetCard, context.draggedCard);
        if (newStack == null)
            return CardDropResolutionResult.BoardFallback();

        return CardDropResolutionResult.HandledWithoutBoardPlacement();
    }
}
