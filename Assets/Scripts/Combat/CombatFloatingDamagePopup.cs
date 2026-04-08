using TMPro;
using UnityEngine;

// ============================================================
// CombatFloatingDamagePopup
// ------------------------------------------------------------
// Presentacion ligera para un numero de dano flotante.
// Vive poco tiempo y se destruye solo.
// ============================================================
public class CombatFloatingDamagePopup : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private float lifetime = 0.85f;
    [SerializeField] private float riseDistance = 58f;
    [SerializeField] private float horizontalDrift = 14f;
    [SerializeField] private Color startColor = new Color(1f, 0.95f, 0.35f, 1f);

    private RectTransform rectTransform;
    private Vector2 startPosition;
    private Vector2 endPosition;
    private float elapsedTime;
    private Vector3 startScale = new Vector3(0.72f, 0.72f, 1f);
    private Vector3 peakScale = new Vector3(1.12f, 1.12f, 1f);

    public void Initialize(string text, Vector2 anchoredPosition)
    {
        rectTransform = transform as RectTransform;
        if (rectTransform != null)
            rectTransform.anchoredPosition = anchoredPosition;

        startPosition = anchoredPosition;
        endPosition = anchoredPosition + new Vector2(Random.Range(-horizontalDrift, horizontalDrift), riseDistance);
        elapsedTime = 0f;

        if (damageText != null)
        {
            damageText.text = text;
            damageText.color = startColor;
        }

        transform.localScale = startScale;
    }

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        if (damageText == null)
            damageText = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (damageText == null || rectTransform == null)
            return;

        elapsedTime += Time.unscaledDeltaTime;
        float progress = lifetime > 0f ? Mathf.Clamp01(elapsedTime / lifetime) : 1f;
        float eased = 1f - Mathf.Pow(1f - progress, 2f);

        rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, eased);
        transform.localScale = Vector3.Lerp(
            progress < 0.2f ? startScale : peakScale,
            Vector3.one,
            progress < 0.2f ? progress / 0.2f : (progress - 0.2f) / 0.8f);

        Color color = startColor;
        color.a = 1f - progress;
        damageText.color = color;

        if (progress >= 1f)
            Destroy(gameObject);
    }
}
