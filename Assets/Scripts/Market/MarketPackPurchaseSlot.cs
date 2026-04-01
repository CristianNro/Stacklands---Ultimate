using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using StacklandsLike.Cards;

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
public class MarketPackPurchaseSlot : MonoBehaviour, IDropHandler
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

    public void OnDrop(PointerEventData eventData)
    {
        // El flujo real de compra lo resuelve CardDrag.OnEndDrag porque ahi
        // sabemos si el jugador arrastra una carta suelta o un stack.
    }

    public bool TryPurchaseFromDrop(CardView draggedCard, CardStack draggedStack)
    {
        if (packData == null)
        {
            Debug.LogWarning($"[{name}] No tiene un MarketPackData asignado.");
            return false;
        }

        MarketTransactionService.PaymentContext paymentContext = MarketTransactionService.BuildPaymentContext(
            draggedCard,
            draggedStack,
            acceptedCurrencyFilterMode,
            acceptedCurrencyTypes);
        if (paymentContext == null)
            return false;

        List<MarketTransactionService.PaymentUnit> selectedUnits = MarketTransactionService.SelectUnitsToConsume(paymentContext.availableUnits, packData.price, out int totalPaidValue);
        if (selectedUnits == null || selectedUnits.Count == 0 || totalPaidValue < packData.price)
        {
            Debug.Log($"[{name}] El pago no alcanza para comprar '{packData.displayName}'.");
            return false;
        }

        MarketTransactionService.ConsumePaymentUnits(paymentContext, selectedUnits);

        GameObject spawnedPack = SpawnPurchasedPack();
        int changeValue = totalPaidValue - packData.price;

        if (changeValue > 0)
            ReturnChange(changeValue, paymentContext, spawnedPack);

        return true;
    }

    private void ReturnChange(int changeValue, MarketTransactionService.PaymentContext paymentContext, GameObject spawnedPack)
    {
        if (changeValue <= 0)
            return;

        // Si el pago vino de un cofre individual, el cambio vuelve al mismo
        // contenedor en lugar de aparecer en el tablero.
        if (paymentContext != null && paymentContext.draggedContainer != null && paymentContext.draggedStack == null)
        {
            if (StoreChangeInContainer(changeValue, paymentContext.draggedContainer))
                return;
        }

        SpawnChangeOnBoard(changeValue, spawnedPack);
    }

    private bool StoreChangeInContainer(int changeValue, ContainerRuntime containerRuntime)
    {
        if (containerRuntime == null || changeValue <= 0)
            return false;

        List<CardData> changeCardsToStore = MarketEconomyService.BuildBestValueCombination(
            changeCurrencyCards,
            changeValue,
            acceptedCurrencyFilterMode,
            acceptedCurrencyTypes);
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

    private void SpawnChangeOnBoard(int changeValue, GameObject spawnedPack)
    {
        if (changeValue <= 0)
            return;

        if (CardSpawner.Instance == null)
        {
            Debug.LogWarning($"[{name}] No se puede devolver cambio porque falta CardSpawner.Instance.");
            return;
        }

        List<CardData> changeCardsToSpawn = MarketEconomyService.BuildBestValueCombination(
            changeCurrencyCards,
            changeValue,
            acceptedCurrencyFilterMode,
            acceptedCurrencyTypes);
        if (changeCardsToSpawn == null || changeCardsToSpawn.Count == 0)
        {
            Debug.LogWarning($"[{name}] No existe una combinacion de cambio valida para devolver {changeValue}.");
            return;
        }

        Vector2 basePosition = GetPreferredSpawnPosition();

        if (spawnedPack != null)
        {
            RectTransform packRect = spawnedPack.GetComponent<RectTransform>();
            if (packRect != null)
                basePosition = packRect.anchoredPosition + new Vector2(0f, -28f);
        }

        float startOffsetX = -((changeCardsToSpawn.Count - 1) * changeSpacing) * 0.5f;

        for (int i = 0; i < changeCardsToSpawn.Count; i++)
        {
            Vector2 preferredPosition = basePosition + new Vector2(startOffsetX + (i * changeSpacing), 0f);
            Vector2 spawnPosition = FindNearestFreeSpawnPosition(preferredPosition);
            CardSpawner.Instance.Spawn(changeCardsToSpawn[i], spawnPosition);
        }
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
}
