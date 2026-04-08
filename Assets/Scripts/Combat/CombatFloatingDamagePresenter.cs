using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ============================================================
// CombatFloatingDamagePresenter
// ------------------------------------------------------------
// Escucha resultados de dano y crea numeros flotantes sobre
// la carta objetivo sin mezclar esta UI con la logica de combate.
// ============================================================
public class CombatFloatingDamagePresenter : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CombatEncounterResolver encounterResolver;

    private TMP_FontAsset cachedFont;

    private void Awake()
    {
        if (encounterResolver == null)
            encounterResolver = FindFirstObjectByType<CombatEncounterResolver>();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (encounterResolver != null)
            encounterResolver.OnAttackResolved -= HandleAttackResolved;
    }

    private void TrySubscribe()
    {
        if (encounterResolver == null)
            encounterResolver = FindFirstObjectByType<CombatEncounterResolver>();

        if (encounterResolver == null)
            return;

        encounterResolver.OnAttackResolved -= HandleAttackResolved;
        encounterResolver.OnAttackResolved += HandleAttackResolved;
    }

    private void HandleAttackResolved(CombatEncounter encounter, CardInstance attacker, CardInstance target, CombatDamageResult damageResult)
    {
        if (target == null || target.RectTransform == null || damageResult == null || damageResult.FinalDamage <= 0)
            return;

        RectTransform parent = ResolvePopupParent();
        if (parent == null)
            return;

        GameObject popupGO = new GameObject("CombatFloatingDamage", typeof(RectTransform));
        popupGO.transform.SetParent(parent, false);
        popupGO.transform.SetAsLastSibling();

        RectTransform popupRect = popupGO.GetComponent<RectTransform>();
        popupRect.anchorMin = new Vector2(0.5f, 0.5f);
        popupRect.anchorMax = new Vector2(0.5f, 0.5f);
        popupRect.pivot = new Vector2(0.5f, 0.5f);
        popupRect.sizeDelta = new Vector2(132f, 56f);

        TextMeshProUGUI popupText = popupGO.AddComponent<TextMeshProUGUI>();
        popupText.font = ResolveFont();
        popupText.fontSize = 38f;
        popupText.fontStyle = FontStyles.Bold;
        popupText.alignment = TextAlignmentOptions.Center;
        popupText.raycastTarget = false;
        popupText.textWrappingMode = TextWrappingModes.NoWrap;
        popupText.outlineWidth = 0.22f;
        popupText.outlineColor = new Color(0f, 0f, 0f, 0.95f);

        Shadow shadow = popupGO.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
        shadow.effectDistance = new Vector2(3f, -3f);

        Vector2 popupPosition = ResolvePopupPosition(target.RectTransform, parent);

        CombatFloatingDamagePopup popup = popupGO.AddComponent<CombatFloatingDamagePopup>();
        popup.Initialize(damageResult.FinalDamage.ToString(), popupPosition);
    }

    private RectTransform ResolvePopupParent()
    {
        if (BoardRoot.Instance != null && BoardRoot.Instance.CardsContainer != null)
            return BoardRoot.Instance.CardsContainer;

        return transform as RectTransform;
    }

    private Vector2 ResolvePopupPosition(RectTransform targetRect, RectTransform popupParent)
    {
        if (targetRect == null || popupParent == null)
            return Vector2.zero;

        Vector3 worldPoint = targetRect.TransformPoint(new Vector3(0f, targetRect.rect.height * 0.28f, 0f));
        Vector3 localPoint3 = popupParent.InverseTransformPoint(worldPoint);
        return new Vector2(localPoint3.x, localPoint3.y);
    }

    private TMP_FontAsset ResolveFont()
    {
        if (cachedFont != null)
            return cachedFont;

        if (TMP_Settings.defaultFontAsset != null)
        {
            cachedFont = TMP_Settings.defaultFontAsset;
            return cachedFont;
        }

        cachedFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        return cachedFont;
    }
}
