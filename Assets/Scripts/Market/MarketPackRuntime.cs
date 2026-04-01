using UnityEngine;
using UnityEngine.EventSystems;

// ============================================================
// MarketPackRuntime
// ------------------------------------------------------------
// Permite abrir con doble click un sobre comprado desde el Market.
// La informacion del pack real se consulta en MarketPackRegistry.
// ============================================================
public class MarketPackRuntime : MonoBehaviour, IPointerClickHandler
{
    [Header("Open")]
    [SerializeField] private float doubleClickWindow = 0.35f;
    [SerializeField] private float releaseRadius = 120f;
    [SerializeField] private float releaseDuration = 0.6f;

    private CardInstance cardInstance;
    private float lastClickTime = -10f;

    private void Awake()
    {
        cardInstance = GetComponent<CardInstance>();
        MarketPackRegistry.GetOrCreate();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        float now = Time.unscaledTime;
        bool isDoubleClick = (now - lastClickTime) <= doubleClickWindow;
        lastClickTime = now;

        if (!isDoubleClick)
            return;

        OpenPack();
    }

    /// <summary>
    /// Abre el sobre, hace salir las cartas desde el centro del pack
    /// y las reparte en circulo alrededor de su posicion original.
    /// </summary>
    public void OpenPack()
    {
        if (cardInstance == null)
            return;

        BaseMarketPackData packData = ResolvePackData();

        if (packData == null)
            return;

        if (CardSpawner.Instance == null)
        {
            Debug.LogWarning($"[{name}] No existe CardSpawner.Instance para abrir el pack.");
            return;
        }

        Vector2 centerPosition = cardInstance.RectTransform != null
            ? cardInstance.RectTransform.anchoredPosition
            : Vector2.zero;

        var openedCards = packData.GetOpenedCards();
        if (openedCards == null || openedCards.Count == 0)
            return;

        int cardsToRelease = openedCards.Count;

        for (int i = 0; i < cardsToRelease; i++)
        {
            CardData rolledCard = openedCards[i];
            if (rolledCard == null)
                continue;

            float angle = cardsToRelease == 1 ? 0f : (Mathf.PI * 2f * i) / cardsToRelease;
            Vector2 preferredOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * releaseRadius;
            Vector2 targetPosition = centerPosition + preferredOffset;

            CardSpawner.Instance.SpawnAnimatedToPosition(
                rolledCard,
                centerPosition,
                targetPosition,
                releaseDuration
            );
        }

        if (MarketPackRegistry.Instance != null)
            MarketPackRegistry.Instance.Unregister(cardInstance);
        Destroy(gameObject);
    }

    /// <summary>
    /// Prioriza la definicion embebida en la carta-pack.
    /// Si la carta es un pack generico comprado desde el Market,
    /// usa el registro runtime como fallback.
    /// </summary>
    private BaseMarketPackData ResolvePackData()
    {
        if (cardInstance == null || cardInstance.data == null)
            return null;

        if (cardInstance.data is PackCardData packCardData && packCardData.embeddedPackData != null)
            return packCardData.embeddedPackData;

        MarketPackRegistry registry = MarketPackRegistry.GetOrCreate();
        return registry.GetPackData(cardInstance);
    }

    private void OnDestroy()
    {
        if (cardInstance != null && MarketPackRegistry.Instance != null)
            MarketPackRegistry.Instance.Unregister(cardInstance);
    }
}
