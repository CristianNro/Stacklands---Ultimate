using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using StacklandsLike.Cards;
#if UNITY_EDITOR
using UnityEditor;
#endif

// ============================================================
// MarketSellSlot
// ------------------------------------------------------------
// Slot de venta dentro del Market.
//
// El jugador arrastra una carta o un stack al slot y recibe una
// combinacion exacta de cartas currency que minimiza la cantidad
// total de cartas entregadas como pago.
// ============================================================
public class MarketSellSlot : MonoBehaviour, IDropHandler, ICardDropTargetSource
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

    public IReadOnlyList<CardData> RewardCurrencyCards => rewardCurrencyCards;
    public float RewardSpacing => rewardSpacing;
    public CurrencyFilterMode AcceptedCurrencyFilterMode => acceptedCurrencyFilterMode;
    public IReadOnlyList<CurrencyType> AcceptedCurrencyTypes => acceptedCurrencyTypes;

    public void OnDrop(PointerEventData eventData)
    {
        // El flujo real lo resuelve CardDrag.OnEndDrag para poder distinguir
        // bien entre carta suelta y stack arrastrado.
    }

    public bool TrySellFromDrop(CardView draggedCard, CardStack draggedStack)
    {
        return MarketTransactionCoordinator.TrySell(this, draggedCard, draggedStack);
    }

    public void PopulateDropTargetInfo(CardDropTargetInfo targetInfo)
    {
        if (targetInfo == null)
            return;

        targetInfo.marketSellSlot = this;
        if (targetInfo.primaryType == CardDropTargetType.None)
            targetInfo.primaryType = CardDropTargetType.MarketSell;
    }

    public Vector2 GetPreferredSpawnPosition()
    {
        RectTransform slotRect = transform as RectTransform;
        if (slotRect == null)
            return Vector2.zero;

        Vector2 boardPoint;

        if (BoardRoot.Instance != null)
        {
            boardPoint = BoardRoot.Instance.GetBoardPointFromWorldPosition(slotRect.position);
        }
        else
        {
            RectTransform boardRect = BoardRoot.Instance != null ? BoardRoot.Instance.CardsContainer : null;
            if (boardRect == null)
                return Vector2.zero;

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, slotRect.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                boardRect,
                screenPoint,
                null,
                out boardPoint
            );
        }

        boardPoint.y -= spawnOffsetY;
        return boardPoint;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        MarketValidationUtility.ValidateAndLogSellSlot(this);
    }
#endif
}
