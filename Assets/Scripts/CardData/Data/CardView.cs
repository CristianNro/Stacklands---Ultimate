using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Vincula CardData/CardInstance con la representación visual.
public class CardView : MonoBehaviour, ICardDropTargetSource
{
    [Header("References")]
    [SerializeField] private CardInstance cardInstance;
    [SerializeField] private Image cardImage;
    [SerializeField] private Image cardIcon;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text cardValueText;
    [SerializeField] private RectTransform cardHealthRoot;
    [SerializeField] private TMP_Text cardHealthText;
    [SerializeField] private RectTransform transformationProgressRoot;
    [SerializeField] private Image transformationProgressBackground;
    [SerializeField] private Image transformationProgressFill;
    [SerializeField] private RectTransform transformationProgressFillRect;
    [Header("Debug")]
    [SerializeField] private bool debugTransformationBar;

    private static TMP_FontAsset cachedValueFont;
    private static bool valueFontResolved;

    private Vector3 baseScale = Vector3.one;
    private Color baseCardColor = Color.white;
    private float attackFeedbackTimer;
    private float hitFeedbackTimer;
    private float deathFeedbackTimer = -1f;

    private int lastDisplayedValue = int.MinValue;
    private int lastDisplayedHealth = int.MinValue;
    private bool lastDisplayedHealthVisible;
    private float lastDisplayedTransformationProgress = -1f;
    private bool lastTransformationBarVisible;
    private bool lastTransformationBarPaused;
    private bool lastLoggedTransformationBarVisible;
    private bool lastLoggedTransformationBarPaused;
    private float lastLoggedTransformationProgress = -1f;

    public CardInstance Instance
    {
        get
        {
            if (cardInstance == null)
                cardInstance = GetComponent<CardInstance>();

            return cardInstance;
        }
    }

    private void Reset()
    {
        AutoAssignVisualReferences();
    }

    private void Awake()
    {
        AutoAssignVisualReferences();
        baseScale = transform.localScale;
        if (cardImage != null)
            baseCardColor = cardImage.color;
    }

    private void LateUpdate()
    {
        RefreshCombatFeedback();
        RefreshValueLabelIfNeeded();
        RefreshCombatHealthIfNeeded();
        RefreshTransformationBarIfNeeded();
    }

    private void OnValidate()
    {
        AutoAssignVisualReferences();
        Refresh();
    }

    private void AutoAssignVisualReferences()
    {
        if (cardInstance == null)
            cardInstance = GetComponent<CardInstance>();

        if (cardImage == null)
            cardImage = GetComponent<Image>();

        if (cardNameText == null)
            cardNameText = FindTextByObjectName("DisplayName");

        if (cardValueText == null)
            cardValueText = FindTextByObjectName("Value");

        if (cardHealthRoot == null)
            cardHealthRoot = FindRectByObjectName("Health");

        if (cardHealthText == null && cardHealthRoot != null)
            cardHealthText = cardHealthRoot.GetComponentInChildren<TMP_Text>(true);
    }

    private TMP_Text FindTextByObjectName(string objectName)
    {
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text != null && text.gameObject.name == objectName)
                return text;
        }

        return null;
    }

    private RectTransform FindRectByObjectName(string objectName)
    {
        RectTransform[] rects = GetComponentsInChildren<RectTransform>(true);
        for (int i = 0; i < rects.Length; i++)
        {
            RectTransform rect = rects[i];
            if (rect != null && rect.gameObject.name == objectName)
                return rect;
        }

        return null;
    }

    public void Refresh()
    {
        CardData data = Instance != null ? Instance.data : null;
        if (data == null)
        {
            SetValueLabelState(0);
            SetHealthLabelState(0, false);
            return;
        }

        if (cardNameText != null)
            cardNameText.text = string.IsNullOrWhiteSpace(data.displayName)
                ? data.cardName
                : data.displayName;

        if (cardImage != null)
            cardImage.sprite = data.cardImage;

        if (cardIcon != null)
        {
            cardIcon.sprite = data.cardIcon;
            cardIcon.enabled = data.cardIcon != null;
        }

        EnsureRuntimeValueLabel();
        RefreshValueLabelIfNeeded(forceRefresh: true);
        EnsureRuntimeCombatHealthLabel();
        RefreshCombatHealthIfNeeded(forceRefresh: true);
        EnsureRuntimeTransformationBar();
        RefreshTransformationBarIfNeeded(forceRefresh: true);
    }

    public void PlayCombatAttackFeedback()
    {
        attackFeedbackTimer = 0.12f;
    }

    public void PlayCombatHitFeedback()
    {
        hitFeedbackTimer = 0.16f;
    }

    public void PlayCombatDeathFeedback()
    {
        deathFeedbackTimer = 0.18f;
    }

    private void RefreshCombatFeedback()
    {
        float deltaTime = Time.unscaledDeltaTime;

        if (attackFeedbackTimer > 0f)
            attackFeedbackTimer = Mathf.Max(0f, attackFeedbackTimer - deltaTime);

        if (hitFeedbackTimer > 0f)
            hitFeedbackTimer = Mathf.Max(0f, hitFeedbackTimer - deltaTime);

        if (deathFeedbackTimer > 0f)
            deathFeedbackTimer = Mathf.Max(0f, deathFeedbackTimer - deltaTime);

        float attackProgress = attackFeedbackTimer > 0f ? attackFeedbackTimer / 0.12f : 0f;
        float hitProgress = hitFeedbackTimer > 0f ? hitFeedbackTimer / 0.16f : 0f;
        float deathProgress = deathFeedbackTimer > 0f ? deathFeedbackTimer / 0.18f : 0f;

        float attackScaleBoost = Mathf.Sin(attackProgress * Mathf.PI) * 0.08f;
        float hitScaleDrop = Mathf.Sin(hitProgress * Mathf.PI) * 0.06f;
        float deathScaleDrop = Mathf.Sin(deathProgress * Mathf.PI * 0.5f) * 0.2f;

        transform.localScale = baseScale * (1f + attackScaleBoost - hitScaleDrop - deathScaleDrop);

        if (cardImage == null)
            return;

        Color feedbackColor = baseCardColor;

        if (hitProgress > 0f)
            feedbackColor = Color.Lerp(baseCardColor, new Color(1f, 0.45f, 0.45f, 1f), Mathf.Sin(hitProgress * Mathf.PI));

        if (deathProgress > 0f)
            feedbackColor = Color.Lerp(feedbackColor, new Color(0.35f, 0.1f, 0.1f, 1f), Mathf.Sin(deathProgress * Mathf.PI * 0.5f));

        cardImage.color = feedbackColor;
    }

    /// <summary>
    /// Si el prefab no trae un label para el valor, lo creamos en runtime
    /// para poder mostrar el valor efectivo de la carta sin depender de
    /// cambios manuales de UI en esta etapa.
    /// </summary>
    private void EnsureRuntimeValueLabel()
    {
        if (cardValueText != null)
            return;

        Transform labelParent = cardImage != null ? cardImage.transform : transform;
        RectTransform parentRect = labelParent as RectTransform;
        if (parentRect == null)
            return;

        GameObject valueGO = new GameObject("CardValueText", typeof(RectTransform));
        valueGO.transform.SetParent(labelParent, false);
        valueGO.transform.SetAsLastSibling();

        RectTransform valueRect = valueGO.GetComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(1f, 0f);
        valueRect.anchorMax = new Vector2(1f, 0f);
        valueRect.pivot = new Vector2(1f, 0f);
        valueRect.anchoredPosition = new Vector2(-6f, 6f);
        valueRect.sizeDelta = new Vector2(42f, 24f);

        TextMeshProUGUI valueText = valueGO.AddComponent<TextMeshProUGUI>();
        TMP_FontAsset fontAsset = ResolveValueFont();
        if (fontAsset != null)
            valueText.font = fontAsset;
        valueText.fontSize = 18f;
        valueText.enableAutoSizing = true;
        valueText.fontSizeMin = 12f;
        valueText.fontSizeMax = 18f;
        valueText.alignment = TextAlignmentOptions.BottomRight;
        valueText.color = Color.black;
        valueText.raycastTarget = false;
        valueText.text = string.Empty;

        cardValueText = valueText;
    }

    /// <summary>
    /// El valor runtime puede cambiar sin que se recree la carta,
    /// por ejemplo cuando un contenedor guarda o libera contenido.
    /// Este helper mantiene el texto sincronizado sin rehacer toda la vista.
    /// </summary>
    private void RefreshValueLabelIfNeeded(bool forceRefresh = false)
    {
        if (Instance == null)
            return;

        int effectiveValue = Instance.data != null ? Instance.GetEffectiveValue() : 0;
        if (cardValueText == null && effectiveValue > 0)
            EnsureRuntimeValueLabel();

        if (!forceRefresh && effectiveValue == lastDisplayedValue)
            return;

        SetValueLabelState(effectiveValue);
    }

    private void SetValueLabelState(int effectiveValue)
    {
        lastDisplayedValue = effectiveValue;

        if (cardValueText == null)
            return;

        bool showValue = effectiveValue > 0;
        cardValueText.text = showValue ? effectiveValue.ToString() : string.Empty;
        cardValueText.gameObject.SetActive(showValue);
    }

    private TMP_FontAsset ResolveValueFont()
    {
        if (valueFontResolved)
            return cachedValueFont;

        valueFontResolved = true;

        if (TMP_Settings.defaultFontAsset != null)
        {
            cachedValueFont = TMP_Settings.defaultFontAsset;
            return cachedValueFont;
        }

        cachedValueFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        return cachedValueFont;
    }

    public static void PrewarmSharedResources()
    {
        if (valueFontResolved)
            return;

        if (TMP_Settings.defaultFontAsset != null)
        {
            cachedValueFont = TMP_Settings.defaultFontAsset;
            valueFontResolved = true;
            return;
        }

        cachedValueFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        valueFontResolved = true;
    }

    /// <summary>
    /// Si el prefab ya trae un bloque `Health`, lo reutilizamos.
    /// Si no existe todavia, generamos un fallback runtime.
    /// </summary>
    private void EnsureRuntimeCombatHealthLabel()
    {
        if (cardHealthRoot != null && cardHealthText != null)
            return;

        if (cardValueText == null)
            EnsureRuntimeValueLabel();

        RectTransform templateRect = cardValueText != null ? cardValueText.transform.parent as RectTransform : null;
        if (templateRect == null)
            return;

        GameObject healthRootGO = Instantiate(templateRect.gameObject);
        healthRootGO.name = "Health";
        healthRootGO.transform.SetParent(templateRect.parent, false);
        healthRootGO.transform.SetAsLastSibling();

        RectTransform healthRootRect = healthRootGO.GetComponent<RectTransform>();
        healthRootRect.anchorMin = new Vector2(0.5f, 0.5f);
        healthRootRect.anchorMax = new Vector2(0.5f, 0.5f);
        healthRootRect.pivot = new Vector2(0.5f, 0.5f);
        healthRootRect.anchoredPosition = new Vector2(-32.15851f, -45.259277f);

        TMP_Text healthText = healthRootGO.GetComponentInChildren<TMP_Text>(true);
        if (healthText == null)
            return;

        healthText.alignment = TextAlignmentOptions.BottomLeft;
        healthText.color = new Color(0.68f, 0.10f, 0.10f, 1f);
        healthText.raycastTarget = false;
        healthText.text = string.Empty;

        cardHealthRoot = healthRootRect;
        cardHealthText = healthText;
    }

    private void RefreshCombatHealthIfNeeded(bool forceRefresh = false)
    {
        CombatParticipantRuntime combatRuntime = Instance != null ? Instance.CombatParticipantRuntime : null;
        bool showHealth = combatRuntime != null && combatRuntime.isActiveAndEnabled && combatRuntime.CombatantData != null && !combatRuntime.IsDead();
        int health = showHealth ? combatRuntime.CurrentHealth : 0;

        if (cardHealthText == null && showHealth)
            EnsureRuntimeCombatHealthLabel();

        if (!forceRefresh
            && health == lastDisplayedHealth
            && showHealth == lastDisplayedHealthVisible)
        {
            return;
        }

        lastDisplayedHealth = health;
        lastDisplayedHealthVisible = showHealth;

        SetHealthLabelState(health, showHealth);
    }

    private void SetHealthLabelState(int health, bool showHealth)
    {
        if (cardHealthText != null)
            cardHealthText.text = showHealth ? health.ToString() : string.Empty;

        if (cardHealthRoot != null)
        {
            cardHealthRoot.gameObject.SetActive(showHealth);
            return;
        }

        if (cardHealthText != null)
            cardHealthText.gameObject.SetActive(showHealth);
    }

    /// <summary>
    /// La barra de transformacion se crea en runtime para no forzar cambios
    /// manuales de prefab mientras terminamos de estabilizar el sistema.
    /// </summary>
    private void EnsureRuntimeTransformationBar()
    {
        if (transformationProgressRoot != null && transformationProgressBackground != null && transformationProgressFill != null && transformationProgressFillRect != null)
            return;

        RectTransform cardRect = transform as RectTransform;
        if (cardRect == null)
            return;

        GameObject rootGO = new GameObject("TransformationProgressBar", typeof(RectTransform));
        rootGO.transform.SetParent(transform, false);
        rootGO.transform.SetAsLastSibling();

        RectTransform rootRect = rootGO.GetComponent<RectTransform>();
        rootRect.anchoredPosition = new Vector2(0f, 65f);
        rootRect.sizeDelta = new Vector2(100f, 12f);
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);

        GameObject backgroundGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        backgroundGO.transform.SetParent(rootGO.transform, false);

        RectTransform backgroundRect = backgroundGO.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        Image backgroundImage = backgroundGO.GetComponent<Image>();
        backgroundImage.color = new Color(0f, 0f, 0f, 0.6f);
        backgroundImage.raycastTarget = false;

        GameObject fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGO.transform.SetParent(backgroundGO.transform, false);

        RectTransform fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = new Vector2(0f, 0f);

        Image fillImage = fillGO.GetComponent<Image>();
        fillImage.color = new Color(0.36f, 0.77f, 0.25f, 1f);
        fillImage.raycastTarget = false;

        transformationProgressRoot = rootRect;
        transformationProgressBackground = backgroundImage;
        transformationProgressFill = fillImage;
        transformationProgressFillRect = fillRect;
    }

    /// <summary>
    /// La barra visual lee el progreso del runtime, pero no lo decide.
    /// Si la carta no esta transformando activamente, la barra se oculta.
    /// </summary>
    private void RefreshTransformationBarIfNeeded(bool forceRefresh = false)
    {
        CardTransformationRuntime transformationRuntime = Instance != null ? Instance.TransformationRuntime : null;
        CardTransformationRule activeRule = transformationRuntime != null ? transformationRuntime.ActiveRule : null;
        bool isRunning = transformationRuntime != null && transformationRuntime.IsRunning;
        bool isPaused = transformationRuntime != null && transformationRuntime.IsPaused;
        float progress = transformationRuntime != null ? Mathf.Clamp01(transformationRuntime.Progress01) : 0f;
        bool showBar = transformationRuntime != null
            && transformationRuntime.isActiveAndEnabled
            && activeRule != null
            && activeRule.showProgressBar
            && !transformationRuntime.IsComplete
            && ((isRunning && !isPaused) || (isPaused && progress > 0f));

        if (transformationProgressRoot == null || transformationProgressFill == null || transformationProgressBackground == null || transformationProgressFillRect == null)
        {
            if (!showBar)
                return;

            EnsureRuntimeTransformationBar();
            if (transformationProgressRoot == null || transformationProgressFill == null || transformationProgressBackground == null || transformationProgressFillRect == null)
                return;
        }

        if (!forceRefresh
            && Mathf.Approximately(progress, lastDisplayedTransformationProgress)
            && showBar == lastTransformationBarVisible)
        {
            if (showBar && isPaused != lastTransformationBarPaused)
            {
                // aunque el progreso no cambie, el color debe reflejar
                // si la transformacion paso a estado pausado o activo.
            }
            else
            {
                return;
            }
        }

        lastDisplayedTransformationProgress = progress;
        lastTransformationBarVisible = showBar;
        lastTransformationBarPaused = isPaused;

        transformationProgressFillRect.sizeDelta = new Vector2(100f * progress, 0f);
        transformationProgressFill.color = isPaused
            ? new Color(0.92f, 0.77f, 0.20f, 1f)
            : new Color(0.36f, 0.77f, 0.25f, 1f);
        transformationProgressRoot.gameObject.SetActive(showBar);

        LogTransformationBarStateIfNeeded(transformationRuntime, activeRule, showBar, isPaused, progress);
    }

    private void LogTransformationBarStateIfNeeded(
        CardTransformationRuntime runtime,
        CardTransformationRule rule,
        bool showBar,
        bool isPaused,
        float progress)
    {
        if (!debugTransformationBar)
            return;

        bool shouldLog = showBar != lastLoggedTransformationBarVisible
            || isPaused != lastLoggedTransformationBarPaused
            || !Mathf.Approximately(progress, lastLoggedTransformationProgress);

        if (!shouldLog)
            return;

        lastLoggedTransformationBarVisible = showBar;
        lastLoggedTransformationBarPaused = isPaused;
        lastLoggedTransformationProgress = progress;

        string cardName = Instance != null && Instance.data != null
            ? (string.IsNullOrWhiteSpace(Instance.data.displayName) ? Instance.data.cardName : Instance.data.displayName)
            : "Unknown";

        Debug.Log(
            $"[CardView] Transformation bar '{cardName}' => visible:{showBar}, paused:{isPaused}, running:{(runtime != null && runtime.IsRunning)}, progress:{progress:0.000}, showProgressBar:{(rule != null && rule.showProgressBar)}, hasRule:{(rule != null)}",
            this);
    }

    public void PopulateDropTargetInfo(CardDropTargetInfo targetInfo)
    {
        if (targetInfo == null)
            return;

        targetInfo.targetCard = this;
        targetInfo.targetInstance = Instance;
        CombatEncounter combatEncounter = ResolveOwningCombatEncounter();
        if (combatEncounter != null)
            targetInfo.targetCombatEncounter = combatEncounter;

        targetInfo.targetContainer = targetInfo.targetInstance != null && targetInfo.targetInstance.HasActiveContainerRuntime()
            ? targetInfo.targetInstance.ContainerRuntime
            : null;
        targetInfo.targetStack = GetComponentInParent<CardStack>();

        if (targetInfo.primaryType != CardDropTargetType.None)
            return;

        if (targetInfo.targetCombatEncounter != null)
            targetInfo.primaryType = CardDropTargetType.CombatEncounter;
        else if (targetInfo.targetContainer != null)
            targetInfo.primaryType = CardDropTargetType.Container;
        else if (targetInfo.targetStack != null)
            targetInfo.primaryType = CardDropTargetType.Stack;
        else
            targetInfo.primaryType = CardDropTargetType.Card;
    }

    private CombatEncounter ResolveOwningCombatEncounter()
    {
        CombatParticipantRuntime combatRuntime = Instance != null ? Instance.CombatParticipantRuntime : null;
        if (combatRuntime == null || !combatRuntime.IsInCombat)
            return null;

        return CombatEncounter.FindById(combatRuntime.EncounterId);
    }
}
