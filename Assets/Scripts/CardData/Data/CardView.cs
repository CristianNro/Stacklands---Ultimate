using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Vincula CardData/CardInstance con la representación visual.
public class CardView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CardInstance cardInstance;
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text cardValueText;

    private int lastDisplayedValue = int.MinValue;

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
        if (cardInstance == null) cardInstance = GetComponent<CardInstance>();
    }

    private void Awake()
    {
        if (cardInstance == null) cardInstance = GetComponent<CardInstance>();
        EnsureRuntimeValueLabel();
    }

    private void LateUpdate()
    {
        RefreshValueLabelIfNeeded();
    }

    private void OnValidate()
    {
        Refresh();
    }

    public void Refresh()
    {
        CardData data = Instance != null ? Instance.data : null;
        if (data == null)
        {
            SetValueLabelState(0);
            return;
        }

        if (cardNameText != null)
            cardNameText.text = string.IsNullOrWhiteSpace(data.displayName)
                ? data.cardName
                : data.displayName;

        if (cardImage != null)
            cardImage.sprite = data.cardImage;

        EnsureRuntimeValueLabel();
        RefreshValueLabelIfNeeded(forceRefresh: true);
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

        EnsureRuntimeValueLabel();

        int effectiveValue = Instance.data != null ? Instance.GetEffectiveValue() : 0;
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
        if (TMP_Settings.defaultFontAsset != null)
            return TMP_Settings.defaultFontAsset;

        return Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
    }
}
