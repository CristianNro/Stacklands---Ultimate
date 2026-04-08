using UnityEngine;
using UnityEngine.EventSystems;

// ============================================================
// CardDrag
// ------------------------------------------------------------
// Permite arrastrar:
// - una carta suelta
// - un stack completo
// - un substack si arrastras desde el medio
//
// Tambien soporta soltar cartas sobre un contenedor para guardarlas.
// ============================================================
public class CardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drop Matching")]
    [SerializeField, Range(0.1f, 1f)] private float overlapStackTargetThreshold = 0.5f;
    [SerializeField] private bool debugDropResolution;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private CardView cardView;
    private CardInstance cardInstance;

    // Que rect estamos moviendo realmente.
    // Puede ser la carta sola o el rect del stack.
    private RectTransform draggedRectTransform;

    // Si estamos arrastrando un stack, queda guardado aca.
    private CardStack draggedStack;
    private bool dragStarted;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        cardView = GetComponent<CardView>();
        cardInstance = GetComponent<CardInstance>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragStarted = false;
        draggedRectTransform = rectTransform;
        draggedStack = null;
        cardInstance?.SetDragging(false);

        if (cardInstance == null || !cardInstance.IsMovable())
            return;

        CardStack parentStack = GetComponentInParent<CardStack>();

        if (parentStack != null)
        {
            if (!parentStack.CanDragFrom(cardView))
                return;

            // Si estamos dentro de un stack:
            // - raiz => arrastramos el stack entero
            // - medio => se parte y arrastramos el substack nuevo
            draggedStack = parentStack.SplitFrom(cardView);

            if (draggedStack != null)
            {
                draggedRectTransform = draggedStack.GetComponent<RectTransform>();
                draggedStack.transform.SetAsLastSibling();
                draggedStack.SetDropSurfaceRaycastEnabled(false);
            }
        }
        else
        {
            transform.SetAsLastSibling();
        }

        dragStarted = true;
        cardInstance.SetDragging(true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragStarted) return;
        if (draggedRectTransform == null || canvas == null) return;

        draggedRectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        if (BoardRoot.Instance != null)
            BoardRoot.Instance.ClampCardToPlayArea(draggedRectTransform);
    }

    private RectTransform GetBoardContainer()
    {
        if (BoardRoot.Instance != null && BoardRoot.Instance.CardsContainer != null)
            return BoardRoot.Instance.CardsContainer;

        return rectTransform.parent as RectTransform;
    }

    private Vector2 GetLocalPointInBoard(PointerEventData eventData)
    {
        if (BoardRoot.Instance != null)
            return BoardRoot.Instance.GetBoardPointFromScreenPosition(eventData.position, eventData.pressEventCamera);

        RectTransform boardRect = GetBoardContainer();
        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            boardRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        return localPoint;
    }

    private void PlaceDraggedObjectOnBoard(Vector2 desiredAnchoredPosition)
    {
        if (draggedRectTransform == null)
            return;

        if (BoardRoot.Instance != null)
        {
            BoardRoot.Instance.TryPlaceRectOnBoard(draggedRectTransform, desiredAnchoredPosition, clampToBoard: true);
            return;
        }

        RectTransform boardRect = GetBoardContainer();
        if (boardRect == null)
            return;

        draggedRectTransform.SetParent(boardRect, false);
        draggedRectTransform.anchoredPosition = desiredAnchoredPosition;
    }

    private Vector2 GetCurrentDraggedBoardPosition()
    {
        if (draggedRectTransform == null)
            return Vector2.zero;

        if (BoardRoot.Instance != null)
            return draggedRectTransform.anchoredPosition;

        RectTransform boardRect = GetBoardContainer();
        if (boardRect == null)
            return draggedRectTransform.anchoredPosition;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, draggedRectTransform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            boardRect,
            screenPoint,
            null,
            out Vector2 boardPoint
        );

        return boardPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragStarted)
            return;

        dragStarted = false;
        cardInstance?.SetDragging(false);
        canvasGroup.blocksRaycasts = true;

        if (draggedStack != null)
            draggedStack.SetDropSurfaceRaycastEnabled(true);

        GameObject hitObject = eventData.pointerCurrentRaycast.gameObject;
        Vector2 releasedBoardPosition = GetCurrentDraggedBoardPosition();
        Vector2 boardPoint = GetLocalPointInBoard(eventData);
        bool startedAsStackDrag = draggedStack != null;
        CardDropTargetInfo targetInfo = CardDropTargetResolver.Resolve(hitObject);

        bool shouldTryOverlapTarget =
            targetInfo == null ||
            targetInfo.primaryType == CardDropTargetType.None ||
            (targetInfo.targetCard != null && targetInfo.targetCard.gameObject == gameObject);

        if (shouldTryOverlapTarget &&
            BoardRoot.Instance != null &&
            draggedRectTransform != null)
        {
            CardView overlappedCard = BoardRoot.Instance.FindBestCoveredCardTarget(draggedRectTransform, overlapStackTargetThreshold);
            if (overlappedCard != null && overlappedCard.gameObject != gameObject)
            {
                targetInfo = new CardDropTargetInfo
                {
                    primaryType = CardDropTargetType.None,
                    hitObject = hitObject
                };

                overlappedCard.PopulateDropTargetInfo(targetInfo);
            }
        }

        bool shouldTryEncounterTarget =
            targetInfo == null ||
            targetInfo.primaryType == CardDropTargetType.None ||
            (targetInfo.targetCard != null && targetInfo.targetCard.gameObject == gameObject);

        if (shouldTryEncounterTarget)
        {
            CombatEncounter overlappedEncounter = CombatEncounter.FindOverlapping(draggedRectTransform)
                ?? CombatEncounter.FindAtBoardPoint(boardPoint);

            if (overlappedEncounter != null)
            {
                targetInfo = new CardDropTargetInfo
                {
                    primaryType = CardDropTargetType.None,
                    hitObject = hitObject
                };

                overlappedEncounter.PopulateDropTargetInfo(targetInfo);
            }
        }

        if (debugDropResolution)
        {
            string initialTarget = hitObject != null ? hitObject.name : "null";
            string resolvedTarget = targetInfo != null && targetInfo.targetCard != null ? targetInfo.targetCard.name : "null";
            string resolvedType = targetInfo != null ? targetInfo.primaryType.ToString() : "null";
            Debug.Log(
                $"[CardDrag] Drop '{name}' -> hit='{initialTarget}', type='{resolvedType}', targetCard='{resolvedTarget}', overlapThreshold={overlapStackTargetThreshold:0.##}, startedAsStack={startedAsStackDrag}",
                this
            );
        }

        CardDropContext dropContext = new CardDropContext
        {
            draggedCard = cardView,
            draggedInstance = cardInstance,
            draggedStack = draggedStack,
            startedAsStackDrag = startedAsStackDrag,
            hitObject = hitObject,
            targetInfo = targetInfo,
            boardPoint = boardPoint
        };

        CardDropResolutionResult resolution = CardDropResolver.Resolve(dropContext);
        if (debugDropResolution)
        {
            Debug.Log(
                $"[CardDrag] Resolution for '{name}' -> handled={(resolution != null && resolution.handled)}, returnToBoard={(resolution != null && resolution.placeDraggedObjectOnBoard)}",
                this
            );
        }

        if (resolution == null || !resolution.handled)
        {
            PlaceDraggedObjectOnBoard(releasedBoardPosition);
            return;
        }

        if (resolution.placeDraggedObjectOnBoard)
        {
            Vector2 placement = resolution.hasCustomBoardPlacement
                ? resolution.customBoardPlacement
                : releasedBoardPosition;

            PlaceDraggedObjectOnBoard(placement);
        }
    }

    private void OnDisable()
    {
        if (cardInstance != null)
            cardInstance.SetDragging(false);
    }
}
