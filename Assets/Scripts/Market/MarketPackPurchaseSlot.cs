using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using StacklandsLike.Cards;
#if UNITY_EDITOR
using UnityEditor;
#endif

// ============================================================
// MarketPackPurchaseSlot
// ------------------------------------------------------------
// Slot de compra de sobres dentro del Market.
//
// La compra ocurre cuando el jugador arrastra una forma de pago
// valida sobre este slot:
// - carta suelta marcada como currency y aceptada por este slot
// - stack cuyas cartas tengan tipos de currency aceptados
// - contenedor monetario con contenido currency aceptado
//
// El slot:
// - valida si el pago alcanza
// - consume primero los valores mas bajos
// - devuelve cambio en monedas de valor 1
// - spawnea el pack debajo del slot
// ============================================================
public class MarketPackPurchaseSlot : MonoBehaviour, IDropHandler, ICardDropTargetSource
{
    [Header("Pack")]
    // Sobre que vende este slot.
    [SerializeField] private BaseMarketPackData packData;

    [Header("Accepted Currency")]
    [SerializeField] private CurrencyFilterMode acceptedCurrencyFilterMode = CurrencyFilterMode.AllowOnlyListed;
    [SerializeField] private List<CurrencyType> acceptedCurrencyTypes = new List<CurrencyType> { CurrencyType.Normal };

    [Header("Change")]
    // Cartas permitidas para devolver cambio.
    // El sistema elegira la combinacion exacta con menos cartas posibles.
    [SerializeField] private List<CardData> changeCurrencyCards = new List<CardData>();
    // Distancia entre monedas devueltas al generar el cambio.
    [SerializeField] private float changeSpacing = 26f;

    [Header("Spawn")]
    // Distancia vertical desde el slot hasta la posicion preferida de spawn.
    [SerializeField] private float spawnOffsetY = 140f;
    // Distancia entre posiciones candidatas al buscar espacio libre.
    [SerializeField] private float searchStep = 90f;
    // Cantidad maxima de anillos de busqueda alrededor del punto ideal.
    [SerializeField] private int maxSearchRings = 4;
    // Distancia minima para considerar que una posicion ya esta ocupada.
    [SerializeField] private float occupiedRadius = 70f;

    public BaseMarketPackData PackData => packData;
    public CurrencyFilterMode AcceptedCurrencyFilterMode => acceptedCurrencyFilterMode;
    public IReadOnlyList<CurrencyType> AcceptedCurrencyTypes => acceptedCurrencyTypes;
    public IReadOnlyList<CardData> ChangeCurrencyCards => changeCurrencyCards;
    public float ChangeSpacing => changeSpacing;

    public void OnDrop(PointerEventData eventData)
    {
        // El flujo real de compra lo resuelve CardDrag.OnEndDrag porque ahi
        // sabemos si el jugador arrastra una carta suelta o un stack.
    }

    public bool TryPurchaseFromDrop(CardView draggedCard, CardStack draggedStack)
    {
        return MarketTransactionCoordinator.TryPurchase(this, draggedCard, draggedStack);
    }

    public void PopulateDropTargetInfo(CardDropTargetInfo targetInfo)
    {
        if (targetInfo == null)
            return;

        targetInfo.marketPurchaseSlot = this;
        if (targetInfo.primaryType == CardDropTargetType.None)
            targetInfo.primaryType = CardDropTargetType.MarketPurchase;
    }

    /// <summary>
    /// Spawnea la carta fisica del sobre en el tablero cerca de la posicion
    /// ideal debajo del slot. La compra real llamara este metodo cuando el
    /// pago haya sido validado y consumido.
    /// </summary>
    public GameObject SpawnPurchasedPack()
    {
        if (packData == null || packData.packCard == null)
        {
            Debug.LogWarning($"[{name}] El pack no tiene packCard asignada.");
            return null;
        }

        if (CardSpawner.Instance == null)
        {
            Debug.LogWarning($"[{name}] No existe CardSpawner.Instance.");
            return null;
        }

        Vector2 spawnPosition = FindNearestFreeSpawnPosition(GetPreferredSpawnPosition());
        GameObject spawnedPack = CardSpawner.Instance.Spawn(packData.packCard, spawnPosition);

        CardInstance cardInstance = spawnedPack != null ? spawnedPack.GetComponent<CardInstance>() : null;
        if (cardInstance != null)
        {
            MarketPackRegistry.GetOrCreate().Register(cardInstance, packData);
            cardInstance.EnableMarketPackRuntime();
        }

        return spawnedPack;
    }

    /// <summary>
    /// Convierte la posicion del slot UI a una posicion del tablero y aplica
    /// un offset hacia abajo para que el sobre aparezca justo debajo del Market.
    /// </summary>
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
            RectTransform boardRect = null;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, slotRect.position);

            boardRect = BoardRoot.Instance != null ? BoardRoot.Instance.CardsContainer : null;
            if (boardRect == null)
                return Vector2.zero;

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

    /// <summary>
    /// Busca una posicion libre cercana al punto ideal.
    /// Se expande en anillos simples para evitar apilar sobres encima de cartas.
    /// </summary>
    public Vector2 FindNearestFreeSpawnPosition(Vector2 preferredPosition)
    {
        if (BoardRoot.Instance == null)
            return preferredPosition;

        return BoardRoot.Instance.FindNearestFreePoint(preferredPosition, occupiedRadius, searchStep, maxSearchRings);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        MarketValidationUtility.ValidateAndLogPurchaseSlot(this);
    }
#endif
}
