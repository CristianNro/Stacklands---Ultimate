using System.Collections.Generic;
using StacklandsLike.Cards;
using UnityEngine;

// ============================================================
// MarketTransactionCoordinator
// ------------------------------------------------------------
// Orquesta flujos completos de compra y venta sobre slots de
// market usando los servicios economicos existentes.
// Separa validacion/consumo/entrega de resultados del script UI
// del slot sin introducir todavia un framework mayor.
// ============================================================
public static class MarketTransactionCoordinator
{
    public static bool TryPurchase(MarketPackPurchaseSlot slot, CardView draggedCard, CardStack draggedStack)
    {
        if (slot == null || slot.PackData == null)
        {
            if (slot != null)
                Debug.LogWarning($"[{slot.name}] No tiene un MarketPackData asignado.");

            return false;
        }

        if (ContainsBusyCombatCards(draggedCard, draggedStack))
            return false;

        MarketTransactionService.PaymentContext paymentContext = MarketTransactionService.BuildPaymentContext(
            draggedCard,
            draggedStack,
            slot.AcceptedCurrencyFilterMode,
            slot.AcceptedCurrencyTypes);
        if (paymentContext == null)
            return false;

        List<MarketTransactionService.PaymentUnit> selectedUnits = MarketTransactionService.SelectUnitsToConsume(
            paymentContext.availableUnits,
            slot.PackData.price,
            out int totalPaidValue);
        if (selectedUnits == null || selectedUnits.Count == 0 || totalPaidValue < slot.PackData.price)
        {
            Debug.Log($"[{slot.name}] El pago no alcanza para comprar '{slot.PackData.displayName}'.");
            return false;
        }

        MarketTransactionService.ConsumePaymentUnits(paymentContext, selectedUnits);

        GameObject spawnedPack = slot.SpawnPurchasedPack();
        int changeValue = totalPaidValue - slot.PackData.price;

        if (changeValue > 0)
            MarketDeliveryService.DeliverPurchaseChange(slot, changeValue, paymentContext, spawnedPack);

        return true;
    }

    public static bool TrySell(MarketSellSlot slot, CardView draggedCard, CardStack draggedStack)
    {
        if (slot == null)
            return false;

        if (ContainsBusyCombatCards(draggedCard, draggedStack))
            return false;

        List<MarketTransactionService.SellableUnit> sellableUnits = MarketTransactionService.BuildSellableUnits(draggedCard, draggedStack);
        if (sellableUnits == null || sellableUnits.Count == 0)
            return false;

        int totalValue = MarketTransactionService.GetTotalValue(sellableUnits);
        if (totalValue <= 0)
            return false;

        List<CardData> rewardCards = MarketEconomyService.BuildBestValueCombination(
            slot.RewardCurrencyCards,
            totalValue,
            slot.AcceptedCurrencyFilterMode,
            slot.AcceptedCurrencyTypes);
        if (rewardCards == null || rewardCards.Count == 0)
        {
            Debug.LogWarning($"[{slot.name}] No existe una combinacion de reward valida para vender por {totalValue}.");
            return false;
        }

        MarketTransactionService.ConsumeSoldUnits(sellableUnits);
        MarketDeliveryService.DeliverSaleReward(slot, rewardCards);
        return true;
    }

    private static bool ContainsBusyCombatCards(CardView draggedCard, CardStack draggedStack)
    {
        if (draggedStack != null)
        {
            IReadOnlyList<CardView> cards = draggedStack.Cards;
            for (int i = 0; i < cards.Count; i++)
            {
                CardInstance instance = cards[i] != null ? cards[i].Instance : null;
                if (instance != null && (instance.IsBusy || instance.IsInCombat()))
                    return true;
            }

            return false;
        }

        CardInstance draggedInstance = draggedCard != null ? draggedCard.Instance : null;
        return draggedInstance != null && (draggedInstance.IsBusy || draggedInstance.IsInCombat());
    }
}
