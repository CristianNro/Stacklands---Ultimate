using UnityEngine;

public static class CardDropTargetResolver
{
    public static CardDropTargetInfo Resolve(GameObject hitObject)
    {
        CardDropTargetInfo targetInfo = new CardDropTargetInfo
        {
            primaryType = CardDropTargetType.None,
            hitObject = hitObject
        };

        if (hitObject == null)
            return targetInfo;

        if (TryPopulateFromExplicitSources(hitObject, targetInfo))
            return targetInfo;

        targetInfo.marketSellSlot = hitObject.GetComponentInParent<MarketSellSlot>();
        if (targetInfo.marketSellSlot != null)
            targetInfo.primaryType = CardDropTargetType.MarketSell;

        targetInfo.marketPurchaseSlot = hitObject.GetComponentInParent<MarketPackPurchaseSlot>();
        if (targetInfo.primaryType == CardDropTargetType.None && targetInfo.marketPurchaseSlot != null)
            targetInfo.primaryType = CardDropTargetType.MarketPurchase;

        targetInfo.targetCard = hitObject.GetComponentInParent<CardView>();
        targetInfo.targetInstance = targetInfo.targetCard != null
            ? targetInfo.targetCard.Instance
            : null;
        targetInfo.targetContainer = targetInfo.targetInstance != null && targetInfo.targetInstance.HasActiveContainerRuntime()
            ? targetInfo.targetInstance.ContainerRuntime
            : null;
        targetInfo.targetStack = targetInfo.targetCard != null
            ? targetInfo.targetCard.GetComponentInParent<CardStack>()
            : null;

        if (targetInfo.primaryType == CardDropTargetType.None)
        {
            if (targetInfo.targetContainer != null)
                targetInfo.primaryType = CardDropTargetType.Container;
            else if (targetInfo.targetStack != null)
                targetInfo.primaryType = CardDropTargetType.Stack;
            else if (targetInfo.targetCard != null)
                targetInfo.primaryType = CardDropTargetType.Card;
        }

        return targetInfo;
    }

    private static bool TryPopulateFromExplicitSources(GameObject hitObject, CardDropTargetInfo targetInfo)
    {
        MonoBehaviour[] behaviours = hitObject.GetComponentsInParent<MonoBehaviour>(includeInactive: true);
        bool populatedAny = false;

        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (!(behaviour is ICardDropTargetSource source))
                continue;

            source.PopulateDropTargetInfo(targetInfo);
            populatedAny = true;
        }

        return populatedAny && targetInfo.primaryType != CardDropTargetType.None;
    }
}
