using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using StacklandsLike.Cards;

// ============================================================
// MarketSellSlot
// ------------------------------------------------------------
// Slot de venta dentro del Market.
//
// El jugador arrastra una carta o un stack al slot y recibe una
// combinacion exacta de cartas currency que minimiza la cantidad
// total de cartas entregadas como pago.
// ============================================================
public class MarketSellSlot : MonoBehaviour, IDropHandler
{
    [Header("Reward")]
    // Cartas currency permitidas como recompensa de venta.
    // El sistema elegira la combinacion exacta con menos cartas posibles.
    [SerializeField] private List<CardData> rewardCurrencyCards = new List<CardData>();
    [SerializeField] private float rewardSpacing = 26f;

    [Header("Accepted Currency")]
    [SerializeField] private CurrencyFilterMode acceptedCurrencyFilterMode = CurrencyFilterMode.AllowOnlyListed;
    [SerializeField] private List<CurrencyType> acceptedCurrencyTypes = new List<CurrencyType> { CurrencyType.Normal };

    [Header("Spawn")]
    // Offset vertical desde el slot hasta el punto donde aparecen las recompensas.
    [SerializeField] private float spawnOffsetY = 140f;

    public void OnDrop(PointerEventData eventData)
    {
        // El flujo real lo resuelve CardDrag.OnEndDrag para poder distinguir
        // bien entre carta suelta y stack arrastrado.
    }

    public bool TrySellFromDrop(CardView draggedCard, CardStack draggedStack)
    {
        List<MarketTransactionService.SellableUnit> sellableUnits = MarketTransactionService.BuildSellableUnits(draggedCard, draggedStack);
        if (sellableUnits == null || sellableUnits.Count == 0)
            return false;

        int totalValue = MarketTransactionService.GetTotalValue(sellableUnits);
        if (totalValue <= 0)
            return false;

        List<CardData> rewardCards = BuildBestRewardCombination(totalValue);
        if (rewardCards == null || rewardCards.Count == 0)
        {
            Debug.LogWarning($"[{name}] No existe una combinacion de reward valida para vender por {totalValue}.");
            return false;
        }

        MarketTransactionService.ConsumeSoldUnits(sellableUnits);
        SpawnRewardCards(rewardCards);
        return true;
    }

    private void SpawnRewardCards(List<CardData> rewardCards)
    {
        if (rewardCards == null || rewardCards.Count == 0 || CardSpawner.Instance == null)
            return;

        Vector2 basePosition = GetPreferredSpawnPosition();
        float startOffsetX = -((rewardCards.Count - 1) * rewardSpacing) * 0.5f;

        for (int i = 0; i < rewardCards.Count; i++)
        {
            CardData cardData = rewardCards[i];
            if (cardData == null)
                continue;

            Vector2 preferredPosition = basePosition + new Vector2(startOffsetX + (i * rewardSpacing), 0f);
            Vector2 spawnPosition = CardSpawner.Instance.FindNearestFreeSpawnPosition(preferredPosition);
            CardSpawner.Instance.Spawn(cardData, spawnPosition);
        }
    }

    private List<CardData> BuildBestRewardCombination(int totalValue)
    {
        return MarketEconomyService.BuildBestValueCombination(
            rewardCurrencyCards,
            totalValue,
            acceptedCurrencyFilterMode,
            acceptedCurrencyTypes);
    }

    private Vector2 GetPreferredSpawnPosition()
    {
        RectTransform slotRect = transform as RectTransform;
        RectTransform boardRect = BoardRoot.Instance != null ? BoardRoot.Instance.CardsContainer : null;

        if (slotRect == null || boardRect == null)
            return Vector2.zero;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, slotRect.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            boardRect,
            screenPoint,
            null,
            out Vector2 boardPoint
        );

        boardPoint.y -= spawnOffsetY;
        return boardPoint;
    }
}
