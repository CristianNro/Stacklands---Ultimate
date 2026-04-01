using UnityEngine;
using StacklandsLike.Cards;

public class CardInstance : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string runtimeId;

    [Header("Definition")]
    public CardData data;

    [Header("Runtime State")]
    [SerializeField] private bool isDragging;
    [SerializeField] private bool isSelected;
    [SerializeField] private bool isBusy;
    public int usesRemaining;

    [Header("Runtime Value")]
    [SerializeField] private bool hasRuntimeValueOverride;
    [SerializeField] private int runtimeValueOverride;

    [Header("Stack")]
    [SerializeField] private CardStack currentStack;

    [Header("Cached References")]
    public RectTransform RectTransform { get; private set; }
    public CardView View { get; private set; }

    [Header("Specialized Runtime")]
    [SerializeField] private UnitRuntime unitRuntime;
    [SerializeField] private BuildingRuntime buildingRuntime;
    [SerializeField] private ContainerRuntime containerRuntime;
    [SerializeField] private MarketPackRuntime marketPackRuntime;

    private void Awake()
    {
        AutoAssignRuntimeReferences();

        if (string.IsNullOrWhiteSpace(runtimeId))
            runtimeId = System.Guid.NewGuid().ToString();
    }

    public string RuntimeId => runtimeId;
    public bool IsDragging => isDragging;
    public bool IsSelected => isSelected;
    public bool IsBusy => isBusy;
    public CardStack CurrentStack => currentStack;
    public UnitRuntime UnitRuntime => unitRuntime;
    public BuildingRuntime BuildingRuntime => buildingRuntime;
    public ContainerRuntime ContainerRuntime => containerRuntime;
    public MarketPackRuntime MarketPackRuntime => marketPackRuntime;
    public bool HasRuntimeValueOverride => hasRuntimeValueOverride;
    public int RuntimeValueOverride => runtimeValueOverride;

    public void AutoAssignRuntimeReferences()
    {
        RectTransform = GetComponent<RectTransform>();
        View = GetComponent<CardView>();

        if (unitRuntime == null) unitRuntime = GetComponent<UnitRuntime>();
        if (buildingRuntime == null) buildingRuntime = GetComponent<BuildingRuntime>();
        if (containerRuntime == null) containerRuntime = GetComponent<ContainerRuntime>();
        if (marketPackRuntime == null) marketPackRuntime = GetComponent<MarketPackRuntime>();
    }

    public void Initialize(CardData data)
    {
        AutoAssignRuntimeReferences();

        // El "prefab base" de cartas vive en escena, asi que las copias
        // nuevas no deben heredar el runtimeId serializado del template.
        runtimeId = System.Guid.NewGuid().ToString();
        this.data = data;
        isDragging = false;
        isSelected = false;
        isBusy = false;
        currentStack = null;
        usesRemaining = data != null ? data.maxUses : 0;
        hasRuntimeValueOverride = false;
        runtimeValueOverride = 0;

        ConfigureSpecializedRuntimes();
    }

    public void ConfigureSpecializedRuntimes()
    {
        AutoAssignRuntimeReferences();
        DisableAllSpecializedRuntimes();

        if (data is UnitCardData unitData && unitRuntime != null)
        {
            unitRuntime.enabled = true;
            unitRuntime.Initialize(unitData);
        }

        if (data is BuildingCardData buildingData && buildingRuntime != null)
        {
            buildingRuntime.enabled = true;
            buildingRuntime.Initialize(buildingData);
        }

        if (data is ContainerCardData containerData && containerRuntime != null)
        {
            containerRuntime.enabled = true;
            containerRuntime.Initialize(containerData);
        }
    }

    public void DisableAllSpecializedRuntimes()
    {
        if (unitRuntime != null)
            unitRuntime.enabled = false;

        if (buildingRuntime != null)
            buildingRuntime.enabled = false;

        if (containerRuntime != null)
            containerRuntime.enabled = false;

        if (marketPackRuntime != null)
            marketPackRuntime.enabled = false;
    }

    public bool HasActiveContainerRuntime()
    {
        return containerRuntime != null && containerRuntime.isActiveAndEnabled && containerRuntime.ContainerData != null;
    }

    public bool HasActiveMarketPackRuntime()
    {
        return marketPackRuntime != null && marketPackRuntime.isActiveAndEnabled;
    }

    public void EnableMarketPackRuntime()
    {
        if (marketPackRuntime != null)
            marketPackRuntime.enabled = true;
    }

    public void DisableMarketPackRuntime()
    {
        if (marketPackRuntime != null)
            marketPackRuntime.enabled = false;
    }

    public void SetCurrentStack(CardStack stack)
    {
        currentStack = stack;
    }

    public void ClearCurrentStack(CardStack expectedStack = null)
    {
        if (expectedStack != null && currentStack != expectedStack)
            return;

        currentStack = null;
    }

    public void SetDragging(bool value)
    {
        isDragging = value;
    }

    public void SetSelected(bool value)
    {
        isSelected = value;
    }

    public void SetBusy(bool value)
    {
        isBusy = value;
    }

    /// <summary>
    /// Devuelve el valor util de esta instancia.
    /// Si existe un override runtime, ese valor tiene prioridad
    /// sobre el value configurado en el asset.
    /// </summary>
    public int GetEffectiveValue()
    {
        if (hasRuntimeValueOverride)
            return runtimeValueOverride;

        return data != null ? data.value : 0;
    }

    public void SetRuntimeValueOverride(int value)
    {
        hasRuntimeValueOverride = true;
        runtimeValueOverride = Mathf.Max(0, value);
        View?.Refresh();
    }

    public void ClearRuntimeValueOverride()
    {
        hasRuntimeValueOverride = false;
        runtimeValueOverride = 0;
        View?.Refresh();
    }

    public bool HasTag(string tag)
    {
        return data != null && data.tags.Contains(tag);
    }

    public bool IsMovable()
    {
        return data != null && data.isMovable;
    }

    public bool IsStackable()
    {
        return data != null && data.stackable;
    }

    public bool IsCurrency()
    {
        return data != null && data.isCurrency && data.currencyType != CurrencyType.None;
    }

    public CurrencyType GetCurrencyType()
    {
        if (!IsCurrency())
            return CurrencyType.None;

        return data.currencyType;
    }

    public float GetWeight()
    {
        return data != null ? Mathf.Max(0f, data.weight) : 0f;
    }

    public bool HasLimitedUses() => usesRemaining > 0;

    public bool ConsumeUseIfNeeded()
    {
        if (usesRemaining == 0) return false;

        usesRemaining--;
        return usesRemaining <= 0;
    }

    public ContainerStorageService.StoredCardSnapshot CreateStoredSnapshot()
    {
        if (data == null)
            return null;

        return new ContainerStorageService.StoredCardSnapshot
        {
            definition = data,
            runtime = new ContainerStorageService.StoredCardRuntimeSnapshot
            {
                usesRemaining = usesRemaining,
                anchoredPosition = RectTransform != null ? RectTransform.anchoredPosition : Vector2.zero,
                hasRuntimeValueOverride = hasRuntimeValueOverride,
                runtimeValueOverride = runtimeValueOverride
            }
        };
    }

    public void ApplyStoredSnapshot(ContainerStorageService.StoredCardSnapshot snapshot)
    {
        if (snapshot == null)
            return;

        ContainerStorageService.StoredCardRuntimeSnapshot runtime = snapshot.runtime;
        if (runtime == null)
            return;

        usesRemaining = runtime.usesRemaining;

        if (runtime.hasRuntimeValueOverride)
            SetRuntimeValueOverride(runtime.runtimeValueOverride);
        else
            ClearRuntimeValueOverride();
    }

    private void OnDestroy()
    {
        if (BoardRoot.Instance != null)
            BoardRoot.Instance.UnregisterCard(this);
    }
}
