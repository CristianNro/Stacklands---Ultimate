using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// BoardRoot representa la raíz lógica del tablero.
// Registra cartas activas y controla los límites válidos del área jugable.
// IMPORTANTE:
// En este diseño, el área válida real para las cartas es el CardsContainer.
public class BoardRoot : MonoBehaviour
{
    public static BoardRoot Instance { get; private set; }
    public static event System.Action<BoardRoot> OnBoardRootAvailable;
    public event System.Action<CardInstance> OnCardRegistered;
    public event System.Action<CardInstance> OnCardUnregistered;
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
    public float LeftPadding => leftPadding;
    public float RightPadding => rightPadding;
    public float TopPadding => topPadding;
    public float BottomPadding => bottomPadding;

    private void Awake()
    {
        Instance = this;

        if (cardsContainer == null)
            cardsContainer = GetComponent<RectTransform>();

        if (playArea == null)
            playArea = cardsContainer;

        RegisterExistingCards();
        RegisterExistingStacks();
        OnBoardRootAvailable?.Invoke(this);
    }

    // =========================================================
    // Registro de cartas
    // =========================================================

    public void RegisterCard(CardInstance card)
    {
        if (card == null) return;
        if (activeCards.Contains(card)) return;

        activeCards.Add(card);
        OnCardRegistered?.Invoke(card);
    }

    public void UnregisterCard(CardInstance card)
    {
        if (card == null) return;
        if (!activeCards.Remove(card)) return;

        OnCardUnregistered?.Invoke(card);
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
        return GetClampedPositionWithinRect(cardsContainer != null ? cardsContainer.rect : new Rect(), anchoredPosition, cardRect);
    }

    public Vector2 GetClampedPositionInPlayArea(Vector2 anchoredPosition, RectTransform cardRect)
    {
        Rect constraintRect = GetConstraintRectInContainerSpace(usePlayArea: true);
        return GetClampedPositionWithinRect(constraintRect, anchoredPosition, cardRect);
    }

    private Vector2 GetClampedPositionWithinRect(Rect rect, Vector2 anchoredPosition, RectTransform cardRect)
    {
        if (cardsContainer == null || cardRect == null)
            return anchoredPosition;

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

        CardStack stack = cardRect.GetComponent<CardStack>();
        if (stack != null)
            ClampStackVisualBoundsToBoard(stack);
    }

    public void ClampCardToPlayArea(RectTransform cardRect)
    {
        if (cardRect == null) return;

        cardRect.anchoredPosition = GetClampedPositionInPlayArea(cardRect.anchoredPosition, cardRect);

        CardStack stack = cardRect.GetComponent<CardStack>();
        if (stack != null)
            ClampStackVisualBoundsToPlayArea(stack);
    }

    public void ClampStackVisualBoundsToBoard(CardStack stack)
    {
        ClampStackVisualBoundsToConstraintRect(stack, GetConstraintRectInContainerSpace(usePlayArea: false));
    }

    public void ClampStackVisualBoundsToPlayArea(CardStack stack)
    {
        ClampStackVisualBoundsToConstraintRect(stack, GetConstraintRectInContainerSpace(usePlayArea: true));
    }

    private void ClampStackVisualBoundsToConstraintRect(CardStack stack, Rect constraintRect)
    {
        if (stack == null || cardsContainer == null)
            return;

        RectTransform stackRect = stack.GetComponent<RectTransform>();
        if (stackRect == null)
            return;

        Rect? visualBounds = GetCurrentStackBoundsInContainerSpace(stack);
        if (!visualBounds.HasValue)
        {
            stackRect.anchoredPosition = GetClampedPositionWithinRect(constraintRect, stackRect.anchoredPosition, stackRect);
            return;
        }

        Rect bounds = visualBounds.Value;

        float minX = constraintRect.xMin + leftPadding;
        float maxX = constraintRect.xMax - rightPadding;
        float minY = constraintRect.yMin + bottomPadding;
        float maxY = constraintRect.yMax - topPadding;

        Vector2 delta = Vector2.zero;

        if (bounds.xMin < minX)
            delta.x += minX - bounds.xMin;
        else if (bounds.xMax > maxX)
            delta.x -= bounds.xMax - maxX;

        if (bounds.yMin < minY)
            delta.y += minY - bounds.yMin;
        else if (bounds.yMax > maxY)
            delta.y -= bounds.yMax - maxY;

        if (delta != Vector2.zero)
            stackRect.anchoredPosition += delta;
    }

    public bool TryGetBoardPointFromScreenPosition(Vector2 screenPosition, Camera eventCamera, out Vector2 boardPoint)
    {
        boardPoint = Vector2.zero;

        if (cardsContainer == null)
            return false;

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cardsContainer,
            screenPosition,
            eventCamera,
            out boardPoint
        );
    }

    public Vector2 GetBoardPointFromScreenPosition(Vector2 screenPosition, Camera eventCamera)
    {
        return TryGetBoardPointFromScreenPosition(screenPosition, eventCamera, out Vector2 boardPoint)
            ? boardPoint
            : Vector2.zero;
    }

    public Vector2 GetBoardPointFromWorldPosition(Vector3 worldPosition, Camera worldCamera = null, Camera eventCamera = null)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPosition);
        return GetBoardPointFromScreenPosition(screenPoint, eventCamera);
    }

    public bool TryPlaceRectOnBoard(RectTransform rectTransform, Vector2 desiredAnchoredPosition, bool clampToBoard = true)
    {
        if (cardsContainer == null || rectTransform == null)
            return false;

        rectTransform.SetParent(cardsContainer, false);

        if (!clampToBoard)
        {
            rectTransform.anchoredPosition = desiredAnchoredPosition;
            return true;
        }

        rectTransform.anchoredPosition = desiredAnchoredPosition;
        ClampCardToBoard(rectTransform);

        return true;
    }

    public bool TryMoveRectToBoardKeepingVisualPosition(RectTransform rectTransform, Camera worldCamera = null, Camera eventCamera = null, bool clampToBoard = true)
    {
        if (rectTransform == null)
            return false;

        Vector2 boardPoint = GetBoardPointFromWorldPosition(rectTransform.position, worldCamera, eventCamera);
        return TryPlaceRectOnBoard(rectTransform, boardPoint, clampToBoard);
    }

    public RectTransform CreateBoardRectTransform(string objectName, Vector2 desiredAnchoredPosition, bool clampToBoard = true)
    {
        if (cardsContainer == null)
            return null;

        GameObject go = new GameObject(objectName, typeof(RectTransform));
        RectTransform rectTransform = go.GetComponent<RectTransform>();

        rectTransform.SetParent(cardsContainer, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.anchoredPosition = clampToBoard
            ? GetClampedPosition(desiredAnchoredPosition, rectTransform)
            : desiredAnchoredPosition;

        return rectTransform;
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

    public CardView FindBestCoveredCardTarget(RectTransform movingRect, float minimumTargetCoverage)
    {
        if (movingRect == null || minimumTargetCoverage <= 0f)
            return null;

        Rect movingBounds = GetCurrentBoundsInContainerSpace(movingRect);
        if (movingBounds.width <= 0f || movingBounds.height <= 0f)
            return null;

        float movingArea = movingBounds.width * movingBounds.height;
        if (movingArea <= 0f)
            return null;

        CardView bestCard = null;
        float bestCoverage = minimumTargetCoverage;

        for (int i = 0; i < activeCards.Count; i++)
        {
            CardInstance instance = activeCards[i];
            if (instance == null || instance.RectTransform == null || instance.View == null)
                continue;

            if (instance.RectTransform == movingRect)
                continue;

            if (instance.RectTransform.IsChildOf(movingRect))
                continue;

            Rect targetBounds = GetCurrentBoundsInContainerSpace(instance.RectTransform);
            if (targetBounds.width <= 0f || targetBounds.height <= 0f)
                continue;

            Rect overlap = GetIntersection(movingBounds, targetBounds);
            if (overlap.width <= 0f || overlap.height <= 0f)
                continue;

            float targetArea = targetBounds.width * targetBounds.height;
            if (targetArea <= 0f)
                continue;

            float overlapArea = overlap.width * overlap.height;
            float targetCoverage = overlapArea / targetArea;
            float movingCoverage = overlapArea / movingArea;
            float coverage = Mathf.Max(targetCoverage, movingCoverage);

            if (coverage < bestCoverage)
                continue;

            bestCoverage = coverage;
            bestCard = instance.View;
        }

        return bestCard;
    }

    private void RegisterExistingStacks()
    {
        CardStack[] allStacks = FindObjectsByType<CardStack>(FindObjectsSortMode.None);

        for (int i = 0; i < allStacks.Length; i++)
            RegisterStack(allStacks[i]);
    }

    private void RegisterExistingCards()
    {
        CardInstance[] allCards = FindObjectsByType<CardInstance>(FindObjectsSortMode.None);

        for (int i = 0; i < allCards.Length; i++)
            RegisterCard(allCards[i]);
    }

    private bool IsPositionFreeForRect(Vector2 position, RectTransform movingRect, float minimumVisibleFraction, RectTransform ignoreRect = null)
    {
        Rect movingBounds = GetProjectedBoundsForRectAtPosition(movingRect, position);
        if (movingBounds.width <= 0f || movingBounds.height <= 0f)
            return true;

        for (int i = 0; i < activeCards.Count; i++)
        {
            CardInstance instance = activeCards[i];
            if (instance == null || instance.RectTransform == null)
                continue;

            if (ignoreRect != null && instance.RectTransform == ignoreRect)
                continue;

            if (movingRect != null && instance.RectTransform.IsChildOf(movingRect))
                continue;

            Rect otherBounds = GetCurrentBoundsInContainerSpace(instance.RectTransform);
            if (otherBounds.width <= 0f || otherBounds.height <= 0f)
                continue;

            Rect overlap = GetIntersection(movingBounds, otherBounds);
            if (overlap.width <= 0f || overlap.height <= 0f)
                continue;

            float movingWidthThreshold = movingBounds.width * minimumVisibleFraction;
            float movingHeightThreshold = movingBounds.height * minimumVisibleFraction;
            float otherWidthThreshold = otherBounds.width * minimumVisibleFraction;
            float otherHeightThreshold = otherBounds.height * minimumVisibleFraction;

            bool blocksMovingVisibility = overlap.width >= movingWidthThreshold && overlap.height >= movingHeightThreshold;
            bool blocksOtherVisibility = overlap.width >= otherWidthThreshold && overlap.height >= otherHeightThreshold;

            if (blocksMovingVisibility || blocksOtherVisibility)
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

            Rect bounds = GetCurrentBoundsInContainerSpace(instance.RectTransform);
            Vector2 center = bounds.center;

            float distance = Vector2.Distance(position, center);
            if (distance < occupiedRadius)
                return false;
        }

        return true;
    }

    private Rect GetProjectedBoundsForRectAtPosition(RectTransform rectTransform, Vector2 anchoredPosition)
    {
        if (rectTransform == null)
            return new Rect();

        CardStack stack = rectTransform.GetComponent<CardStack>();
        if (stack != null)
        {
            stack.GetVisualExtents(out float left, out float right, out float bottom, out float top);
            return Rect.MinMaxRect(
                anchoredPosition.x - left,
                anchoredPosition.y - bottom,
                anchoredPosition.x + right,
                anchoredPosition.y + top
            );
        }

        Vector2 size = GetCardSizeInContainerSpace(rectTransform);
        if (size == Vector2.zero)
            return new Rect();

        Vector2 halfSize = size * 0.5f;
        return Rect.MinMaxRect(
            anchoredPosition.x - halfSize.x,
            anchoredPosition.y - halfSize.y,
            anchoredPosition.x + halfSize.x,
            anchoredPosition.y + halfSize.y
        );
    }

    private Rect GetCurrentBoundsInContainerSpace(RectTransform rectTransform)
    {
        if (cardsContainer == null || rectTransform == null)
            return new Rect();

        Vector3[] worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);

        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

        for (int i = 0; i < worldCorners.Length; i++)
        {
            Vector3 localCorner3 = cardsContainer.InverseTransformPoint(worldCorners[i]);
            Vector2 localCorner = new Vector2(localCorner3.x, localCorner3.y);

            min = Vector2.Min(min, localCorner);
            max = Vector2.Max(max, localCorner);
        }

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private Rect? GetCurrentStackBoundsInContainerSpace(CardStack stack)
    {
        if (stack == null)
            return null;

        IReadOnlyList<CardView> stackCards = stack.Cards;
        if (stackCards == null || stackCards.Count == 0)
            return null;

        bool hasAnyBounds = false;
        Rect combinedBounds = new Rect();

        for (int i = 0; i < stackCards.Count; i++)
        {
            CardView card = stackCards[i];
            if (card == null)
                continue;

            RectTransform cardRect = card.GetComponent<RectTransform>();
            if (cardRect == null)
                continue;

            Rect cardBounds = GetCurrentBoundsInContainerSpace(cardRect);
            if (cardBounds.width <= 0f || cardBounds.height <= 0f)
                continue;

            if (!hasAnyBounds)
            {
                combinedBounds = cardBounds;
                hasAnyBounds = true;
            }
            else
            {
                combinedBounds = Rect.MinMaxRect(
                    Mathf.Min(combinedBounds.xMin, cardBounds.xMin),
                    Mathf.Min(combinedBounds.yMin, cardBounds.yMin),
                    Mathf.Max(combinedBounds.xMax, cardBounds.xMax),
                    Mathf.Max(combinedBounds.yMax, cardBounds.yMax)
                );
            }
        }

        return hasAnyBounds ? combinedBounds : null;
    }

    private Rect GetIntersection(Rect a, Rect b)
    {
        float xMin = Mathf.Max(a.xMin, b.xMin);
        float yMin = Mathf.Max(a.yMin, b.yMin);
        float xMax = Mathf.Min(a.xMax, b.xMax);
        float yMax = Mathf.Min(a.yMax, b.yMax);

        if (xMax <= xMin || yMax <= yMin)
            return new Rect();

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    private Rect GetConstraintRectInContainerSpace(bool usePlayArea)
    {
        if (cardsContainer == null)
            return new Rect();

        if (!usePlayArea || playArea == null || playArea == cardsContainer)
            return cardsContainer.rect;

        Vector3[] worldCorners = new Vector3[4];
        playArea.GetWorldCorners(worldCorners);

        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

        for (int i = 0; i < worldCorners.Length; i++)
        {
            Vector3 localCorner3 = cardsContainer.InverseTransformPoint(worldCorners[i]);
            Vector2 localCorner = new Vector2(localCorner3.x, localCorner3.y);

            min = Vector2.Min(min, localCorner);
            max = Vector2.Max(max, localCorner);
        }

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        InfrastructureValidationUtility.ValidateAndLogBoardRoot(this);
    }
#endif
}
