using UnityEngine;
using UnityEngine.EventSystems;
using StacklandsLike.Cards;
using System.Collections.Generic;

// ============================================================
// ContainerRuntime
// ------------------------------------------------------------
// Maneja la interaccion de doble click y el almacenamiento de
// cartas dentro de una carta contenedor.
// ============================================================
public class ContainerRuntime : MonoBehaviour, IPointerClickHandler
{
    [Header("Runtime")]
    [SerializeField] private string containerId;
    [SerializeField] private float doubleClickWindow = 0.35f;

    private ContainerCardData containerData;
    private CardInstance cardInstance;
    private float lastClickTime = -10f;

    public string ContainerId => containerId;
    public ContainerCardData ContainerData => containerData;
    public CardInstance OwnerInstance => cardInstance;

    private void Awake()
    {
        cardInstance = GetComponent<CardInstance>();

        if (string.IsNullOrWhiteSpace(containerId))
            containerId = System.Guid.NewGuid().ToString();

        ContainerStorageService.GetOrCreate();
    }

    public void Initialize(ContainerCardData data)
    {
        // Cada contenedor instanciado necesita su propio id de almacenamiento.
        // Si copiara el id del asset/template, varios cofres compartirian
        // el mismo contenido y por eso mostrarian el mismo valor.
        containerId = System.Guid.NewGuid().ToString();
        containerData = data;
        RefreshRuntimeValueFromContents();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (containerData == null)
            return;

        float now = Time.unscaledTime;
        bool isDoubleClick = (now - lastClickTime) <= doubleClickWindow;
        lastClickTime = now;

        if (!isDoubleClick)
            return;

        OpenContainer();
    }

    public bool TryStoreCard(CardView card)
    {
        if (card == null || containerData == null)
            return false;

        CardInstance instance = card.GetComponent<CardInstance>();
        if (instance == null || instance == cardInstance)
            return false;

        if (instance.IsBusy || instance.IsInCombat())
            return false;

        if (!CanStoreCardData(instance.data))
            return false;

        ContainerStorageService storage = ContainerStorageService.GetOrCreate();
        if (!storage.StoreCard(containerId, containerData, instance))
            return false;

        CardStack previousStack = instance.CurrentStack;
        if (previousStack != null)
            previousStack.RemoveCard(card);

        RefreshRuntimeValueFromContents();
        Destroy(card.gameObject);
        return true;
    }

    private bool CanStoreCardData(CardData data)
    {
        if (data == null || containerData == null)
            return false;

        if (data is ContainerCardData)
            return false;

        if (data.isCurrency && data.currencyType != CurrencyType.None)
        {
            if (!ActsAsCurrencyContainer())
                return false;

            return CanStoreCurrencyCard(data);
        }

        if (ActsAsCurrencyContainer())
            return CanStoreCurrencyCard(data);

        bool isListed = IsListedCardType(data.cardType);
        bool passesCardTypeFilter;

        switch (containerData.listMode)
        {
            case ContainerListMode.AllowOnlyListed:
                // Si la lista esta vacia en modo allow-only, no entra ninguna carta.
                passesCardTypeFilter = isListed;
                break;

            case ContainerListMode.BlockListed:
            default:
                // Si la lista esta vacia en modo block-list, entra todo.
                passesCardTypeFilter = !isListed;
                break;
        }

        if (!passesCardTypeFilter)
            return false;

        return PassesSubtypeFilter(data);
    }

    private bool CanStoreCurrencyCard(CardData data)
    {
        if (data == null)
            return false;

        if (!data.isCurrency || data.currencyType == CurrencyType.None)
            return false;

        return data.currencyType == containerData.currencyType;
    }

    private void OpenContainer()
    {
        ContainerStorageService storage = ContainerStorageService.GetOrCreate();
        Vector2 releaseCenter = cardInstance != null && cardInstance.RectTransform != null
            ? cardInstance.RectTransform.anchoredPosition
            : Vector2.zero;

        storage.ReleaseContents(
            containerId,
            releaseCenter,
            containerData.releaseRadius,
            containerData.maxCardsReleasedPerOpen);
        RefreshRuntimeValueFromContents();
    }

    /// <summary>
    /// Si este contenedor se comporta como currency, su valor runtime
    /// refleja la suma del value de todas las cartas almacenadas.
    /// </summary>
    public void RefreshRuntimeValueFromContents()
    {
        if (cardInstance == null)
            return;

        if (!ActsAsCurrencyContainer())
        {
            cardInstance.ClearRuntimeValueOverride();
            return;
        }

        ContainerStorageService storage = ContainerStorageService.GetOrCreate();
        int totalStoredValue = storage.GetStoredTotalValue(containerId);
        cardInstance.SetRuntimeValueOverride(totalStoredValue);
    }

    public int GetStoredTotalValue()
    {
        ContainerStorageService storage = ContainerStorageService.GetOrCreate();
        return storage.GetStoredTotalValue(containerId);
    }

    private bool ActsAsCurrencyContainer()
    {
        return containerData != null && containerData.isCurrency && containerData.currencyType != CurrencyType.None;
    }

    private bool IsListedCardType(CardType cardType)
    {
        List<CardType> listedCardTypes = containerData != null ? containerData.listedCardTypes : null;
        if (listedCardTypes == null)
            return false;

        for (int i = 0; i < listedCardTypes.Count; i++)
        {
            if (listedCardTypes[i] == cardType)
                return true;
        }

        return false;
    }

    private bool PassesSubtypeFilter(CardData data)
    {
        if (data is ResourceCardData resourceData)
            return PassesResourceTypeFilter(resourceData);

        return true;
    }

    private bool PassesResourceTypeFilter(ResourceCardData resourceData)
    {
        if (resourceData == null || containerData == null || !containerData.useResourceTypeFilter)
            return true;

        bool isListed = IsListedResourceType(resourceData.resourceType);

        switch (containerData.resourceListMode)
        {
            case ContainerListMode.BlockListed:
                return !isListed;

            case ContainerListMode.AllowOnlyListed:
            default:
                return isListed;
        }
    }

    private bool IsListedResourceType(ResourceType resourceType)
    {
        List<ResourceType> listedResourceTypes = containerData != null ? containerData.listedResourceTypes : null;
        if (listedResourceTypes == null)
            return false;

        for (int i = 0; i < listedResourceTypes.Count; i++)
        {
            if (listedResourceTypes[i] == resourceType)
                return true;
        }

        return false;
    }
}
