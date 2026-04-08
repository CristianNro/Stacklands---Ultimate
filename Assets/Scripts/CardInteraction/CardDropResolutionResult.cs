public class CardDropResolutionResult
{
    public bool handled;
    public bool placeDraggedObjectOnBoard;
    public bool hasCustomBoardPlacement;
    public UnityEngine.Vector2 customBoardPlacement;

    public static CardDropResolutionResult BoardFallback()
    {
        return new CardDropResolutionResult
        {
            handled = true,
            placeDraggedObjectOnBoard = true
        };
    }

    public static CardDropResolutionResult BoardFallbackAt(UnityEngine.Vector2 boardPosition)
    {
        return new CardDropResolutionResult
        {
            handled = true,
            placeDraggedObjectOnBoard = true,
            hasCustomBoardPlacement = true,
            customBoardPlacement = boardPosition
        };
    }

    public static CardDropResolutionResult HandledWithoutBoardPlacement()
    {
        return new CardDropResolutionResult
        {
            handled = true,
            placeDraggedObjectOnBoard = false
        };
    }
}
