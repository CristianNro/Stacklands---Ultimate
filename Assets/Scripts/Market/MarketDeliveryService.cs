using System.Collections.Generic;
using StacklandsLike.Cards;
using UnityEngine;

// ============================================================
// MarketDeliveryService
// ------------------------------------------------------------
// Centraliza la politica de entrega de recompensas y cambio
// para el market. Separa la orquestacion economica de la forma
// en que los resultados vuelven al board o a contenedores.
// ============================================================
public static class MarketDeliveryService
{
    public static void DeliverPurchaseChange(
        MarketPackPurchaseSlot slot,
        int changeValue,
        MarketTransactionService.PaymentContext paymentContext,
        GameObject spawnedPack)
    {
        if (slot == null || changeValue <= 0)
            return;

        if (paymentContext != null && paymentContext.draggedContainer != null && paymentContext.draggedStack == null)
        {
            if (TryStoreChangeInContainer(slot, changeValue, paymentContext.draggedContainer))
                return;
        }

        SpawnChangeOnBoard(slot, changeValue, spawnedPack);
    }

    public static void DeliverSaleReward(MarketSellSlot slot, List<CardData> rewardCards)
    {
        if (slot == null || rewardCards == null || rewardCards.Count == 0 || CardSpawner.Instance == null)
            return;

        Vector2 basePosition = slot.GetPreferredSpawnPosition();
        float startOffsetX = -((rewardCards.Count - 1) * slot.RewardSpacing) * 0.5f;

        for (int i = 0; i < rewardCards.Count; i++)
        {
            CardData cardData = rewardCards[i];
            if (cardData == null)
                continue;

            Vector2 preferredPosition = basePosition + new Vector2(startOffsetX + (i * slot.RewardSpacing), 0f);
            Vector2 spawnPosition = CardSpawner.Instance.FindNearestFreeSpawnPosition(preferredPosition);
            CardSpawner.Instance.Spawn(cardData, spawnPosition);
        }
    }

    private static bool TryStoreChangeInContainer(MarketPackPurchaseSlot slot, int changeValue, ContainerRuntime containerRuntime)
    {
        if (slot == null || containerRuntime == null || changeValue <= 0)
            return false;

        List<CardData> changeCardsToStore = MarketEconomyService.BuildBestValueCombination(
            slot.ChangeCurrencyCards,
            changeValue,
            slot.AcceptedCurrencyFilterMode,
            slot.AcceptedCurrencyTypes);
        if (changeCardsToStore == null || changeCardsToStore.Count == 0)
            return false;

        List<ContainerStorageService.StoredCardSnapshot> snapshotsToAdd = new List<ContainerStorageService.StoredCardSnapshot>();

        for (int i = 0; i < changeCardsToStore.Count; i++)
        {
            CardData cardData = changeCardsToStore[i];
            if (cardData == null)
                continue;

            ContainerStorageService.StoredCardSnapshot snapshot =
                ContainerStorageService.CreateSnapshotFromCardData(cardData, cardData.maxUses, Vector2.zero);

            if (snapshot != null)
                snapshotsToAdd.Add(snapshot);
        }

        if (snapshotsToAdd.Count == 0)
            return false;

        ContainerStorageService storage = ContainerStorageService.GetOrCreate();
        storage.AddStoredSnapshots(containerRuntime.ContainerId, snapshotsToAdd);
        containerRuntime.RefreshRuntimeValueFromContents();
        return true;
    }

    private static void SpawnChangeOnBoard(MarketPackPurchaseSlot slot, int changeValue, GameObject spawnedPack)
    {
        if (slot == null || changeValue <= 0)
            return;

        if (CardSpawner.Instance == null)
        {
            Debug.LogWarning($"[{slot.name}] No se puede devolver cambio porque falta CardSpawner.Instance.");
            return;
        }

        List<CardData> changeCardsToSpawn = MarketEconomyService.BuildBestValueCombination(
            slot.ChangeCurrencyCards,
            changeValue,
            slot.AcceptedCurrencyFilterMode,
            slot.AcceptedCurrencyTypes);
        if (changeCardsToSpawn == null || changeCardsToSpawn.Count == 0)
        {
            Debug.LogWarning($"[{slot.name}] No existe una combinacion de cambio valida para devolver {changeValue}.");
            return;
        }

        Vector2 basePosition = slot.GetPreferredSpawnPosition();

        if (spawnedPack != null)
        {
            RectTransform packRect = spawnedPack.GetComponent<RectTransform>();
            if (packRect != null)
                basePosition = packRect.anchoredPosition + new Vector2(0f, -28f);
        }

        float startOffsetX = -((changeCardsToSpawn.Count - 1) * slot.ChangeSpacing) * 0.5f;

        for (int i = 0; i < changeCardsToSpawn.Count; i++)
        {
            Vector2 preferredPosition = basePosition + new Vector2(startOffsetX + (i * slot.ChangeSpacing), 0f);
            Vector2 spawnPosition = slot.FindNearestFreeSpawnPosition(preferredPosition);
            CardSpawner.Instance.Spawn(changeCardsToSpawn[i], spawnPosition);
        }
    }
}
