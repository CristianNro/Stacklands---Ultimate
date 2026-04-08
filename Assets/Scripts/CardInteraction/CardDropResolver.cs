public static class CardDropResolver
{
    private delegate CardDropResolutionResult DropHandler(CardDropContext context);

    private static readonly DropHandler[] OrderedHandlers =
    {
        CardDropMarketHandler.TryResolve,
        CardDropContainerHandler.TryResolve,
        CardDropCombatHandler.TryResolve,
        CardDropStackHandler.TryResolve
    };

    public static CardDropResolutionResult Resolve(CardDropContext context)
    {
        if (context == null)
            return new CardDropResolutionResult();

        if (context.targetInfo == null || context.targetInfo.primaryType == CardDropTargetType.None)
            return CardDropResolutionResult.BoardFallback();

        if (context.targetCard != null && context.draggedCard != null && context.targetCard.gameObject == context.draggedCard.gameObject)
            return CardDropResolutionResult.BoardFallback();

        for (int i = 0; i < OrderedHandlers.Length; i++)
        {
            DropHandler handler = OrderedHandlers[i];
            if (handler == null)
                continue;

            CardDropResolutionResult result = handler(context);
            if (result != null)
                return result;
        }

        return CardDropResolutionResult.BoardFallback();
    }
}
