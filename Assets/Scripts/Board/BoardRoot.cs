using System.Collections.Generic;
using UnityEngine;

// BoardRoot representa la raíz lógica del tablero.
// Registra cartas activas y controla los límites válidos del área jugable.
// IMPORTANTE:
// En este diseño, el área válida real para las cartas es el CardsContainer.
public class BoardRoot : MonoBehaviour
{
    public static BoardRoot Instance { get; private set; }
    public event System.Action<CardStack> OnStackRegistered;
    public event System.Action<CardStack> OnStackUnregistered;

    [Header("Board References")]
    [SerializeField] private RectTransform cardsContainer;
    [SerializeField] private RectTransform playArea;

    [Header("Board Padding")]
    [SerializeField] private float leftPadding = 16f;
    [SerializeField] private float rightPadding = 16f;
    [SerializeField] private float topPadding = 16f;
    [SerializeField] private float bottomPadding = 16f;

    private readonly List<CardInstance> activeCards = new();
    private readonly List<CardStack> activeStacks = new();

    public RectTransform CardsContainer => cardsContainer;
    public RectTransform PlayArea => playArea;
    public IReadOnlyList<CardInstance> ActiveCards => activeCards;
    public IReadOnlyList<CardStack> ActiveStacks => activeStacks;

    private void Awake()
    {
        Instance = this;

        if (cardsContainer == null)
            cardsContainer = GetComponent<RectTransform>();

        if (playArea == null)
            playArea = cardsContainer;

        RegisterExistingStacks();
    }

    // =========================================================
    // Registro de cartas
    // =========================================================

    public void RegisterCard(CardInstance card)
    {
        if (card == null) return;
        if (activeCards.Contains(card)) return;

        activeCards.Add(card);
    }

    public void UnregisterCard(CardInstance card)
    {
        if (card == null) return;
        activeCards.Remove(card);
    }

    public void RegisterStack(CardStack stack)
    {
        if (stack == null) return;
        if (activeStacks.Contains(stack)) return;

        activeStacks.Add(stack);
        OnStackRegistered?.Invoke(stack);
    }

    public void UnregisterStack(CardStack stack)
    {
        if (stack == null) return;
        if (!activeStacks.Remove(stack)) return;

        OnStackUnregistered?.Invoke(stack);
    }

    // =========================================================
    // Posiciones válidas dentro del tablero
    // =========================================================

    public bool IsInsideBoard(Vector2 anchoredPosition, RectTransform cardRect)
    {
        Vector2 clamped = GetClampedPosition(anchoredPosition, cardRect);
        return Vector2.Distance(clamped, anchoredPosition) < 0.01f;
    }

        public Vector2 GetClampedPosition(Vector2 anchoredPosition, RectTransform cardRect)
    {
        if (cardsContainer == null || cardRect == null)
            return anchoredPosition;

        Rect rect = cardsContainer.rect;

        float leftExtent;
        float rightExtent;
        float bottomExtent;
        float topExtent;

        // -----------------------------------------------------
        // Caso 1: estamos moviendo un stack
        // -----------------------------------------------------
        // En ese caso, el rect del objeto CardStack NO representa
        // el tamaño real ocupado por todas sus cartas hijas.
        // Entonces le pedimos al stack sus extensiones reales.
        // -----------------------------------------------------
        CardStack stack = cardRect.GetComponent<CardStack>();
        if (stack != null)
        {
            stack.GetVisualExtents(
                out leftExtent,
                out rightExtent,
                out bottomExtent,
                out topExtent
            );
        }
        else
        {
            // -------------------------------------------------
            // Caso 2: estamos moviendo una carta suelta
            // -------------------------------------------------
            Vector2 size = GetCardSizeInContainerSpace(cardRect);

            leftExtent = size.x * 0.5f;
            rightExtent = size.x * 0.5f;
            bottomExtent = size.y * 0.5f;
            topExtent = size.y * 0.5f;
        }

        float minX = rect.xMin + leftPadding + leftExtent;
        float maxX = rect.xMax - rightPadding - rightExtent;

        float minY = rect.yMin + bottomPadding + bottomExtent;
        float maxY = rect.yMax - topPadding - topExtent;

        // Si por algún motivo el contenido es más grande que el área disponible,
        // lo forzamos al centro para evitar NaN o clamps inválidos.
        if (minX > maxX)
        {
            float centerX = (rect.xMin + rect.xMax) * 0.5f;
            minX = centerX;
            maxX = centerX;
        }

        if (minY > maxY)
        {
            float centerY = (rect.yMin + rect.yMax) * 0.5f;
            minY = centerY;
            maxY = centerY;
        }

        return new Vector2(
            Mathf.Clamp(anchoredPosition.x, minX, maxX),
            Mathf.Clamp(anchoredPosition.y, minY, maxY)
        );
    }

    public void ClampCardToBoard(RectTransform cardRect)
    {
        if (cardRect == null) return;

        cardRect.anchoredPosition = GetClampedPosition(cardRect.anchoredPosition, cardRect);
    }

    // =========================================================
    // Helpers
    // =========================================================

    public Vector2 FindNearestFreePositionForRect(
        Vector2 preferredPosition,
        RectTransform movingRect,
        float minimumVisibleFraction,
        float searchStep,
        int maxSearchRings,
        RectTransform ignoreRect = null)
    {
        if (movingRect == null)
            return preferredPosition;

        Vector2 clampedPreferred = GetClampedPosition(preferredPosition, movingRect);

        if (IsPositionFreeForRect(clampedPreferred, movingRect, minimumVisibleFraction, ignoreRect))
            return clampedPreferred;

        for (int ring = 1; ring <= maxSearchRings; ring++)
        {
            int samples = ring * 8;

            for (int i = 0; i < samples; i++)
            {
                float angle = (Mathf.PI * 2f * i) / samples;
                Vector2 candidate = clampedPreferred + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (searchStep * ring);
                candidate = GetClampedPosition(candidate, movingRect);

                if (IsPositionFreeForRect(candidate, movingRect, minimumVisibleFraction, ignoreRect))
                    return candidate;
            }
        }

        return clampedPreferred;
    }

    public Vector2 FindNearestFreePoint(
        Vector2 preferredPosition,
        float occupiedRadius,
        float searchStep,
        int maxSearchRings)
    {
        RectTransform boardRect = cardsContainer;
        if (boardRect == null)
            return preferredPosition;

        Vector2 clampedPreferred = GetClampedPosition(preferredPosition, boardRect);

        if (IsPointFree(clampedPreferred, occupiedRadius))
            return clampedPreferred;

        for (int ring = 1; ring <= maxSearchRings; ring++)
        {
            int samples = ring * 8;

            for (int i = 0; i < samples; i++)
            {
                float angle = (Mathf.PI * 2f * i) / samples;
                Vector2 candidate = clampedPreferred + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (searchStep * ring);
                candidate = GetClampedPosition(candidate, boardRect);

                if (IsPointFree(candidate, occupiedRadius))
                    return candidate;
            }
        }

        return clampedPreferred;
    }

    private void RegisterExistingStacks()
    {
        CardStack[] allStacks = FindObjectsByType<CardStack>(FindObjectsSortMode.None);

        for (int i = 0; i < allStacks.Length; i++)
            RegisterStack(allStacks[i]);
    }

    private bool IsPositionFreeForRect(Vector2 position, RectTransform movingRect, float minimumVisibleFraction, RectTransform ignoreRect = null)
    {
        Vector2 movingSize = GetCardSizeInContainerSpace(movingRect);
        if (movingSize == Vector2.zero)
            return true;

        for (int i = 0; i < activeCards.Count; i++)
        {
            CardInstance instance = activeCards[i];
            if (instance == null || instance.RectTransform == null)
                continue;

            if (ignoreRect != null && instance.RectTransform == ignoreRect)
                continue;

            Vector2 otherPosition = instance.RectTransform.anchoredPosition;
            float minSeparationX = movingSize.x * minimumVisibleFraction;
            float minSeparationY = movingSize.y * minimumVisibleFraction;

            bool overlapsX = Mathf.Abs(position.x - otherPosition.x) < minSeparationX;
            bool overlapsY = Mathf.Abs(position.y - otherPosition.y) < minSeparationY;

            if (overlapsX && overlapsY)
                return false;
        }

        return true;
    }

    private bool IsPointFree(Vector2 position, float occupiedRadius)
    {
        for (int i = 0; i < activeCards.Count; i++)
        {
            CardInstance instance = activeCards[i];
            if (instance == null || instance.RectTransform == null)
                continue;

            float distance = Vector2.Distance(position, instance.RectTransform.anchoredPosition);
            if (distance < occupiedRadius)
                return false;
        }

        return true;
    }

    private Vector2 GetCardSizeInContainerSpace(RectTransform cardRect)
    {
        Vector3 scale = cardRect.lossyScale;

        return new Vector2(
            cardRect.rect.width * scale.x,
            cardRect.rect.height * scale.y
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
