using System.Collections.Generic;
using UnityEngine;

// BoardRoot representa la raíz lógica del tablero.
// Registra cartas activas y controla los límites válidos del área jugable.
// IMPORTANTE:
// En este diseño, el área válida real para las cartas es el CardsContainer.
public class BoardRoot : MonoBehaviour
{
    public static BoardRoot Instance { get; private set; }

    [Header("Board References")]
    [SerializeField] private RectTransform cardsContainer;
    [SerializeField] private RectTransform playArea;

    [Header("Board Padding")]
    [SerializeField] private float leftPadding = 16f;
    [SerializeField] private float rightPadding = 16f;
    [SerializeField] private float topPadding = 16f;
    [SerializeField] private float bottomPadding = 16f;

    private readonly List<CardInstance> activeCards = new();

    public RectTransform CardsContainer => cardsContainer;
    public RectTransform PlayArea => playArea;
    public IReadOnlyList<CardInstance> ActiveCards => activeCards;

    private void Awake()
    {
        Instance = this;

        if (cardsContainer == null)
            cardsContainer = GetComponent<RectTransform>();

        if (playArea == null)
            playArea = cardsContainer;
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

    private Vector2 GetCardSizeInContainerSpace(RectTransform cardRect)
    {
        Vector3 scale = cardRect.lossyScale;

        return new Vector2(
            cardRect.rect.width * scale.x,
            cardRect.rect.height * scale.y
        );
    }
}