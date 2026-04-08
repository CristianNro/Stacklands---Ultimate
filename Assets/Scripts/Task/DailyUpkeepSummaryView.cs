using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ============================================================
// DailyUpkeepSummaryView
// ------------------------------------------------------------
// Capa visual simple para mostrar el resultado del upkeep diario.
//
// Objetivos:
// - escuchar `DailyUpkeepSystem`
// - armar un resumen legible del cierre del dia
// - no convertirse en autoridad del gameplay
//
// Si no se le asignan referencias de UI, crea un panel basico
// en runtime dentro del primer Canvas encontrado para que el
// sistema se pueda probar sin prefabs extra.
// ============================================================
public class DailyUpkeepSummaryView : MonoBehaviour
{
    [Header("Data Source")]
    [SerializeField] private DailyUpkeepSystem dailyUpkeepSystem;

    [Header("Optional UI References")]
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    [Header("Display")]
    [SerializeField, Min(0.5f)] private float visibleDurationSeconds = 4f;
    [SerializeField] private bool autoCreateRuntimePanel = true;

    private float visibleTimer;

    private void Awake()
    {
        EnsureViewReferences();
        HideImmediate();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Update()
    {
        if (canvasGroup == null || canvasGroup.alpha <= 0f)
            return;

        visibleTimer -= Time.unscaledDeltaTime;
        if (visibleTimer <= 0f)
            HideImmediate();
    }

    private void TrySubscribe()
    {
        if (dailyUpkeepSystem == null)
            dailyUpkeepSystem = FindFirstObjectByType<DailyUpkeepSystem>();

        if (dailyUpkeepSystem != null)
            dailyUpkeepSystem.OnDailyUpkeepProcessed += HandleDailyUpkeepProcessed;
    }

    private void Unsubscribe()
    {
        if (dailyUpkeepSystem != null)
            dailyUpkeepSystem.OnDailyUpkeepProcessed -= HandleDailyUpkeepProcessed;
    }

    private void HandleDailyUpkeepProcessed(DailyUpkeepResult result)
    {
        if (result == null)
            return;

        EnsureViewReferences();

        if (titleText != null)
            titleText.text = $"Fin del dia {result.dayNumber}";

        if (bodyText != null)
            bodyText.text = BuildSummaryText(result);

        ShowForDuration();
    }

    private string BuildSummaryText(DailyUpkeepResult result)
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine($"Comida consumida: {result.consumedFood}/{result.requiredFood}");
        builder.AppendLine($"Unidades alimentadas: {result.fedUnits.Count}");

        if (result.deadUnits.Count > 0)
        {
            builder.AppendLine($"Muertes por hambre: {result.deadUnits.Count}");
            builder.AppendLine(BuildDeadUnitsLine(result));
        }
        else
        {
            builder.Append("Sin muertes por hambre");
        }

        return builder.ToString();
    }

    private string BuildDeadUnitsLine(DailyUpkeepResult result)
    {
        if (result == null || result.deadUnits.Count == 0)
            return string.Empty;

        const int maxNamesToShow = 4;
        StringBuilder builder = new StringBuilder("Bajas: ");

        int shown = Mathf.Min(maxNamesToShow, result.deadUnits.Count);
        for (int i = 0; i < shown; i++)
        {
            DailyUpkeepResult.UnitFeedingRecord unit = result.deadUnits[i];
            if (unit == null)
                continue;

            if (i > 0)
                builder.Append(", ");

            builder.Append(unit.displayName);
        }

        if (result.deadUnits.Count > maxNamesToShow)
            builder.Append($" y {result.deadUnits.Count - maxNamesToShow} mas");

        return builder.ToString();
    }

    private void ShowForDuration()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (panelRoot != null)
            panelRoot.gameObject.SetActive(true);

        visibleTimer = Mathf.Max(0.5f, visibleDurationSeconds);
    }

    private void HideImmediate()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (panelRoot != null)
            panelRoot.gameObject.SetActive(false);

        visibleTimer = 0f;
    }

    private void EnsureViewReferences()
    {
        if (panelRoot != null && canvasGroup != null && titleText != null && bodyText != null)
            return;

        if (!autoCreateRuntimePanel)
            return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
            return;

        if (panelRoot == null)
            panelRoot = CreateRuntimePanel(canvas.transform as RectTransform);

        if (canvasGroup == null && panelRoot != null)
            canvasGroup = panelRoot.GetComponent<CanvasGroup>();

        if (titleText == null && panelRoot != null)
        {
            Transform title = panelRoot.Find("Title");
            titleText = title != null ? title.GetComponent<TMP_Text>() : null;
        }

        if (bodyText == null && panelRoot != null)
        {
            Transform body = panelRoot.Find("Body");
            bodyText = body != null ? body.GetComponent<TMP_Text>() : null;
        }
    }

    private RectTransform CreateRuntimePanel(RectTransform canvasRect)
    {
        if (canvasRect == null)
            return null;

        GameObject panel = new GameObject("DailyUpkeepSummaryPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.SetParent(canvasRect, false);
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0f, -24f);
        panelRect.sizeDelta = new Vector2(420f, 120f);

        Image background = panel.GetComponent<Image>();
        background.color = new Color(0.12f, 0.09f, 0.04f, 0.88f);

        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        CreateTextElement(panelRect, "Title", new Vector2(16f, -14f), new Vector2(-16f, -42f), 26f, FontStyles.Bold);
        CreateTextElement(panelRect, "Body", new Vector2(16f, -48f), new Vector2(-16f, -14f), 20f, FontStyles.Normal);

        return panelRect;
    }

    private TMP_Text CreateTextElement(RectTransform parent, string objectName, Vector2 topLeftInset, Vector2 bottomRightInset, float fontSize, FontStyles fontStyle)
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
        text.color = new Color(0.96f, 0.94f, 0.86f, 1f);
        text.alignment = TextAlignmentOptions.TopLeft;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        text.text = string.Empty;

        return text;
    }
}
