using UnityEngine;

public static class CardDropMarketHandler
{
    public static CardDropResolutionResult TryResolve(CardDropContext context)
    {
        if (context == null || context.targetInfo == null)
            return null;

        MarketSellSlot marketSellSlot = context.targetInfo.marketSellSlot;
        if (marketSellSlot != null && marketSellSlot.TrySellFromDrop(context.draggedCard, context.draggedStack))
            return CardDropResolutionResult.HandledWithoutBoardPlacement();

        MarketPackPurchaseSlot marketPurchaseSlot = context.targetInfo.marketPurchaseSlot;
        if (marketPurchaseSlot == null || !marketPurchaseSlot.TryPurchaseFromDrop(context.draggedCard, context.draggedStack))
            return null;

        bool shouldReturnDraggedObjectToBoard =
            (context.startedAsStackDrag && context.draggedStack != null && context.draggedStack.gameObject != null) ||
            (!context.startedAsStackDrag && context.draggedCard != null && context.draggedCard.gameObject != null);

        if (shouldReturnDraggedObjectToBoard)
            return CardDropResolutionResult.BoardFallback();

        return CardDropResolutionResult.HandledWithoutBoardPlacement();
    }
}
