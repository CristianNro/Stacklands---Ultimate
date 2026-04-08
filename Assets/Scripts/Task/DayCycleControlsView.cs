using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ============================================================
// DayCycleControlsView
// ------------------------------------------------------------
// Vista simple del reloj diario.
//
// Responsabilidades:
// - mostrar el dia actual
// - mostrar el progreso del dia
// - exponer controles de pausa y velocidad
//
// Importante:
// - la autoridad sigue siendo `GameTimeService`
// - esta vista solo refleja estado y emite clicks
// ============================================================
public class DayCycleControlsView : MonoBehaviour
{
    [Header("Data Source")]
    [SerializeField] private GameTimeService gameTimeService;

    [Header("Optional UI References")]
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private TMP_Text dayLabelText;
    [SerializeField] private Image dayProgressFill;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button speedCycleButton;
    [SerializeField] private TMP_Text pauseButtonText;
    [SerializeField] private TMP_Text speedCycleButtonText;

    [Header("Runtime Creation")]
    [SerializeField] private bool autoCreateRuntimePanel = true;

    private bool buttonsBound;

    private void Awake()
    {
        EnsureViewReferences();
        RefreshImmediate();
    }

    private void OnEnable()
    {
        EnsureViewReferences();
        TryBindButtons();
        TrySubscribe();
        RefreshImmediate();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Update()
    {
        RefreshProgressOnly();
    }

    private void TrySubscribe()
    {
        if (gameTimeService == null)
            gameTimeService = GameTimeService.Instance;

        if (gameTimeService == null)
            return;

        gameTimeService.OnDayStarted += HandleDayStarted;
        gameTimeService.OnDayAdvanced += HandleDayAdvanced;
    }

    private void Unsubscribe()
    {
        if (gameTimeService == null)
            return;

        gameTimeService.OnDayStarted -= HandleDayStarted;
        gameTimeService.OnDayAdvanced -= HandleDayAdvanced;
    }

    private void HandleDayStarted(int dayNumber)
    {
        RefreshImmediate();
    }

    private void HandleDayAdvanced(int dayNumber)
    {
        RefreshImmediate();
    }

    private void TryBindButtons()
    {
        if (buttonsBound)
            return;

        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);

        if (speedCycleButton != null)
            speedCycleButton.onClick.AddListener(CycleSpeed);

        buttonsBound = true;
    }

    private void TogglePause()
    {
        if (!TryResolveGameTimeService())
            return;

        if (gameTimeService.PauseTimedSystems)
            gameTimeService.ResumeTime();
        else
            gameTimeService.PauseTime();

        RefreshImmediate();
    }

    private void CycleSpeed()
    {
        if (!TryResolveGameTimeService())
            return;

        gameTimeService.ResumeTime();
        gameTimeService.CycleSpeedMultiplier();
        RefreshImmediate();
    }

    private bool TryResolveGameTimeService()
    {
        if (gameTimeService == null)
            gameTimeService = GameTimeService.Instance;

        return gameTimeService != null;
    }

    private void RefreshImmediate()
    {
        if (!TryResolveGameTimeService())
            return;

        if (dayLabelText != null)
            dayLabelText.text = $"Dia {gameTimeService.CurrentDay}";

        if (pauseButtonText != null)
            pauseButtonText.text = gameTimeService.PauseTimedSystems ? "Play" : "Pause";

        if (speedCycleButtonText != null)
            speedCycleButtonText.text = $"x{Mathf.RoundToInt(Mathf.Max(1f, gameTimeService.TimedSystemsTimeScale))}";

        RefreshProgressOnly();
        RefreshButtonHighlight();
    }

    private void RefreshProgressOnly()
    {
        if (!TryResolveGameTimeService())
            return;

        if (dayProgressFill != null)
            dayProgressFill.fillAmount = gameTimeService.CurrentDayProgress01;
    }

    private void RefreshButtonHighlight()
    {
        if (!TryResolveGameTimeService())
            return;

        HighlightButton(speedCycleButton, !gameTimeService.PauseTimedSystems);
        HighlightButton(pauseButton, gameTimeService.PauseTimedSystems);
    }

    private void HighlightButton(Button button, bool highlighted)
    {
        if (button == null)
            return;

        Image image = button.GetComponent<Image>();
        if (image == null)
            return;

        image.color = highlighted
            ? new Color(0.80f, 0.63f, 0.24f, 0.95f)
            : new Color(0.26f, 0.18f, 0.10f, 0.90f);
    }

    private void EnsureViewReferences()
    {
        if (panelRoot != null && dayLabelText != null && dayProgressFill != null && pauseButton != null && speedCycleButton != null)
            return;

        if (!autoCreateRuntimePanel)
            return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
            return;

        if (panelRoot == null)
            panelRoot = CreateRuntimePanel(canvas.transform as RectTransform);

        if (dayLabelText == null && panelRoot != null)
        {
            Transform dayLabel = panelRoot.Find("DayLabel");
            dayLabelText = dayLabel != null ? dayLabel.GetComponent<TMP_Text>() : null;
        }

        if (dayProgressFill == null && panelRoot != null)
        {
            Transform fill = panelRoot.Find("ProgressBar/Fill");
            dayProgressFill = fill != null ? fill.GetComponent<Image>() : null;
        }

        if (pauseButton == null && panelRoot != null)
        {
            Transform button = panelRoot.Find("Buttons/PauseButton");
            pauseButton = button != null ? button.GetComponent<Button>() : null;
        }

        if (speedCycleButton == null && panelRoot != null)
        {
            Transform button = panelRoot.Find("Buttons/SpeedCycleButton");
            speedCycleButton = button != null ? button.GetComponent<Button>() : null;
        }

        if (pauseButtonText == null && pauseButton != null)
        {
            TMP_Text text = pauseButton.GetComponentInChildren<TMP_Text>();
            if (text != null)
                pauseButtonText = text;
        }

        if (speedCycleButtonText == null && speedCycleButton != null)
        {
            TMP_Text text = speedCycleButton.GetComponentInChildren<TMP_Text>();
            if (text != null)
                speedCycleButtonText = text;
        }
    }

    private RectTransform CreateRuntimePanel(RectTransform canvasRect)
    {
        if (canvasRect == null)
            return null;

        GameObject panel = new GameObject("DayCycleControlsPanel", typeof(RectTransform), typeof(Image));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.SetParent(canvasRect, false);
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = new Vector2(-20f, -20f);
        panelRect.sizeDelta = new Vector2(230f, 140f);

        Image background = panel.GetComponent<Image>();
        background.color = new Color(0.13f, 0.10f, 0.05f, 0.88f);

        CreateLabel(panelRect, "DayLabel", "Dia 1", new Vector2(16f, -12f), new Vector2(-16f, -42f), 28f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        CreateProgressBar(panelRect);
        CreateButtonsRow(panelRect);

        return panelRect;
    }

    private void CreateProgressBar(RectTransform panelRect)
    {
        GameObject barRoot = new GameObject("ProgressBar", typeof(RectTransform), typeof(Image));
        RectTransform barRect = barRoot.GetComponent<RectTransform>();
        barRect.SetParent(panelRect, false);
        barRect.anchorMin = new Vector2(0f, 1f);
        barRect.anchorMax = new Vector2(1f, 1f);
        barRect.pivot = new Vector2(0.5f, 1f);
        barRect.offsetMin = new Vector2(16f, -74f);
        barRect.offsetMax = new Vector2(-16f, -46f);

        Image barBackground = barRoot.GetComponent<Image>();
        barBackground.color = new Color(0.20f, 0.16f, 0.10f, 0.95f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.SetParent(barRect, false);
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.pivot = new Vector2(0.5f, 0.5f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = fill.GetComponent<Image>();
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
        fillImage.fillAmount = 0f;
        fillImage.color = new Color(0.84f, 0.70f, 0.28f, 0.98f);
    }

    private void CreateButtonsRow(RectTransform panelRect)
    {
        GameObject row = new GameObject("Buttons", typeof(RectTransform));
        RectTransform rowRect = row.GetComponent<RectTransform>();
        rowRect.SetParent(panelRect, false);
        rowRect.anchorMin = new Vector2(0f, 0f);
        rowRect.anchorMax = new Vector2(1f, 0f);
        rowRect.pivot = new Vector2(0.5f, 0f);
        rowRect.offsetMin = new Vector2(16f, 16f);
        rowRect.offsetMax = new Vector2(-16f, 56f);

        CreateButton(rowRect, "PauseButton", "Pause", new Vector2(0f, 0f), new Vector2(90f, 0f));
        CreateButton(rowRect, "SpeedCycleButton", "x1", new Vector2(106f, 0f), new Vector2(90f, 0f));
    }

    private Button CreateButton(RectTransform parent, string objectName, string label, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject buttonGo = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = buttonGo.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = buttonGo.GetComponent<Image>();
        image.color = new Color(0.26f, 0.18f, 0.10f, 0.90f);

        Button button = buttonGo.GetComponent<Button>();

        TMP_Text text = CreateLabel(rect, "Label", label, new Vector2(0f, 0f), Vector2.zero, 22f, FontStyles.Bold, TextAlignmentOptions.Center);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    private TMP_Text CreateLabel(
        RectTransform parent,
        string objectName,
        string textValue,
        Vector2 topLeftInset,
        Vector2 bottomRightInset,
        float fontSize,
        FontStyles fontStyle,
        TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = new Vector2(topLeftInset.x, bottomRightInset.y);
        rect.offsetMax = new Vector2(bottomRightInset.x, topLeftInset.y);

        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = new Color(0.96f, 0.94f, 0.86f, 1f);
        text.text = textValue;
        text.raycastTarget = false;

        return text;
    }
}
