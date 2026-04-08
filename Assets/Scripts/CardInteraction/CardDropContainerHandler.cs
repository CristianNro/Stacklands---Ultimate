using System.Collections.Generic;

public static class CardDropContainerHandler
{
    public static CardDropResolutionResult TryResolve(CardDropContext context)
    {
        if (context == null || context.targetContainer == null)
            return null;

        if (context.draggedStack != null)
            return TryResolveStackStorage(context);

        if (context.draggedCard != null && context.targetContainer.TryStoreCard(context.draggedCard))
            return CardDropResolutionResult.HandledWithoutBoardPlacement();

        return null;
    }

    private static CardDropResolutionResult TryResolveStackStorage(CardDropContext context)
    {
        List<CardView> draggedCardsCopy = new List<CardView>(context.draggedStack.Cards);
        if (!TryStoreDraggedCardsInContainer(context.targetContainer, draggedCardsCopy))
            return null;

        return CardDropResolutionResult.BoardFallback();
    }

    private static bool TryStoreDraggedCardsInContainer(ContainerRuntime targetContainer, List<CardView> cardsToStore)
    {
        if (targetContainer == null || cardsToStore == null || cardsToStore.Count == 0)
            return false;

        bool storedAny = false;

        for (int i = 0; i < cardsToStore.Count; i++)
        {
            CardView draggedCard = cardsToStore[i];
            if (draggedCard == null)
                continue;

            if (targetContainer.TryStoreCard(draggedCard))
                storedAny = true;
        }

        return storedAny;
    }
}
