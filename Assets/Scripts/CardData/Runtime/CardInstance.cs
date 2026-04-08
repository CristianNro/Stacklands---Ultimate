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
    [SerializeField] private FoodRuntime foodRuntime;
    [SerializeField] private CombatParticipantRuntime combatParticipantRuntime;
    [SerializeField] private CardTransformationRuntime transformationRuntime;

    private BoardRoot registeredBoardRoot;

    private void Awake()
    {
        AutoAssignRuntimeReferences();

        if (string.IsNullOrWhiteSpace(runtimeId))
            runtimeId = System.Guid.NewGuid().ToString();
    }

    private void OnEnable()
    {
        BoardRoot.OnBoardRootAvailable += HandleBoardRootAvailable;
        TryRegisterWithBoard();
    }

    private void OnDisable()
    {
        BoardRoot.OnBoardRootAvailable -= HandleBoardRootAvailable;
        UnregisterFromBoardIfNeeded();
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
    public FoodRuntime FoodRuntime => foodRuntime;
    public CombatParticipantRuntime CombatParticipantRuntime => combatParticipantRuntime;
    public CardTransformationRuntime TransformationRuntime => transformationRuntime;
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
        if (foodRuntime == null) foodRuntime = GetComponent<FoodRuntime>();
        if (combatParticipantRuntime == null) combatParticipantRuntime = GetComponent<CombatParticipantRuntime>();
        if (transformationRuntime == null) transformationRuntime = GetComponent<CardTransformationRuntime>();
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

        if (data is UnitCardData unitData)
        {
            if (unitRuntime == null)
                unitRuntime = gameObject.AddComponent<UnitRuntime>();

            unitRuntime.enabled = true;
            unitRuntime.Initialize(unitData);
        }

        if (data is CombatantCardData combatantData)
        {

            if (combatParticipantRuntime == null)
                combatParticipantRuntime = gameObject.AddComponent<CombatParticipantRuntime>();

            combatParticipantRuntime.enabled = true;
            combatParticipantRuntime.Initialize(combatantData);
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

        if (data is FoodResourceCardData foodCardData)
        {
            if (foodRuntime == null)
                foodRuntime = gameObject.AddComponent<FoodRuntime>();

            foodRuntime.enabled = true;
            foodRuntime.Initialize(foodCardData);
        }

        if (data != null && data.transformationRule != null)
        {
            CardTransformationRule rule = data.transformationRule;
            if (rule.sourceCard != data)
            {
                Debug.LogWarning(
                    $"[CardInstance] '{data.name}' references transformation rule '{rule.name}' but that rule expects '{(rule.sourceCard != null ? rule.sourceCard.name : "null")}'. Transformation runtime was not started.",
                    this);
                return;
            }

            if (transformationRuntime == null)
                transformationRuntime = gameObject.AddComponent<CardTransformationRuntime>();

            transformationRuntime.enabled = true;
            transformationRuntime.Initialize(rule);
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

        if (foodRuntime != null)
            foodRuntime.enabled = false;

        if (combatParticipantRuntime != null)
            combatParticipantRuntime.enabled = false;

        if (transformationRuntime != null)
            transformationRuntime.enabled = false;
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

    public bool HasCapability(CardCapabilityType capability)
    {
        if (data == null || capability == CardCapabilityType.None)
            return false;

        return data.capabilities != null && data.capabilities.Contains(capability);
    }

    public bool IsMovable()
    {
        return data != null && data.isMovable && !isBusy;
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

    public bool IsInCombat()
    {
        return combatParticipantRuntime != null
            && combatParticipantRuntime.isActiveAndEnabled
            && combatParticipantRuntime.IsInCombat;
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
                runtimeValueOverride = runtimeValueOverride,
                hasRemainingFoodValue = foodRuntime != null && foodRuntime.isActiveAndEnabled,
                remainingFoodValue = foodRuntime != null && foodRuntime.isActiveAndEnabled ? foodRuntime.RemainingFoodValue : 0,
                hasTransformationProgress = transformationRuntime != null && transformationRuntime.isActiveAndEnabled && transformationRuntime.ActiveRule != null,
                transformationElapsedTime = transformationRuntime != null && transformationRuntime.isActiveAndEnabled ? transformationRuntime.ElapsedTime : 0f
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

        if (foodRuntime != null && foodRuntime.isActiveAndEnabled && runtime.hasRemainingFoodValue)
            foodRuntime.SetRemainingFoodValue(runtime.remainingFoodValue);

        if (transformationRuntime != null && transformationRuntime.isActiveAndEnabled && runtime.hasTransformationProgress)
            transformationRuntime.SetProgress(runtime.transformationElapsedTime);
    }

    private void HandleBoardRootAvailable(BoardRoot boardRoot)
    {
        TryRegisterWithBoard();
    }

    private void TryRegisterWithBoard()
    {
        BoardRoot boardRoot = BoardRoot.Instance;
        if (boardRoot == null)
            return;

        if (registeredBoardRoot == boardRoot)
            return;

        UnregisterFromBoardIfNeeded();
        registeredBoardRoot = boardRoot;
        registeredBoardRoot.RegisterCard(this);
    }

    private void UnregisterFromBoardIfNeeded()
    {
        if (registeredBoardRoot == null)
            return;

        registeredBoardRoot.UnregisterCard(this);
        registeredBoardRoot = null;
    }

    private void OnDestroy()
    {
        UnregisterFromBoardIfNeeded();
    }
}
