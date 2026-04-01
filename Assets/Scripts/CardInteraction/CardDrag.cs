using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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
    }

    private RectTransform GetBoardContainer()
    {
        if (BoardRoot.Instance != null && BoardRoot.Instance.CardsContainer != null)
            return BoardRoot.Instance.CardsContainer;

        return rectTransform.parent as RectTransform;
    }

    private Vector2 GetLocalPointInBoard(PointerEventData eventData)
    {
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
        RectTransform boardRect = GetBoardContainer();
        if (boardRect == null || draggedRectTransform == null)
            return;

        draggedRectTransform.SetParent(boardRect, false);

        Vector2 finalPosition = desiredAnchoredPosition;

        if (BoardRoot.Instance != null)
            finalPosition = BoardRoot.Instance.GetClampedPosition(desiredAnchoredPosition, draggedRectTransform);

        draggedRectTransform.anchoredPosition = finalPosition;
    }

    private bool TryStoreDraggedCardsInContainer(ContainerRuntime targetContainer, List<CardView> cardsToStore)
    {
        if (targetContainer == null || cardsToStore == null || cardsToStore.Count == 0)
            return false;

        bool storedAny = false;

        for (int i = 0; i < cardsToStore.Count; i++)
        {
            CardView draggedCard = cardsToStore[i];
            if (draggedCard == null) continue;

            if (targetContainer.TryStoreCard(draggedCard))
                storedAny = true;
        }

        return storedAny;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragStarted)
            return;

        dragStarted = false;
        cardInstance?.SetDragging(false);
        canvasGroup.blocksRaycasts = true;

        GameObject hitObject = eventData.pointerCurrentRaycast.gameObject;

        if (hitObject == null)
        {
            Vector2 boardPoint = GetLocalPointInBoard(eventData);
            PlaceDraggedObjectOnBoard(boardPoint);
            return;
        }

        bool startedAsStackDrag = draggedStack != null;

        MarketSellSlot marketSellSlot = hitObject != null
            ? hitObject.GetComponentInParent<MarketSellSlot>()
            : null;

        if (marketSellSlot != null && marketSellSlot.TrySellFromDrop(cardView, draggedStack))
            return;

        MarketPackPurchaseSlot marketSlot = hitObject != null
            ? hitObject.GetComponentInParent<MarketPackPurchaseSlot>()
            : null;

        if (marketSlot != null && marketSlot.TryPurchaseFromDrop(cardView, draggedStack))
        {
            Vector2 boardPoint = GetLocalPointInBoard(eventData);

            // Si el objeto usado para pagar sigue existiendo despues de la compra
            // (por ejemplo un cofre con contenido restante), lo devolvemos al board.
            if (startedAsStackDrag)
            {
                if (draggedStack != null && draggedStack.gameObject != null)
                    PlaceDraggedObjectOnBoard(boardPoint);

                return;
            }

            if (cardView != null && cardView.gameObject != null)
                PlaceDraggedObjectOnBoard(boardPoint);

            return;
        }

        CardView targetCard = hitObject.GetComponentInParent<CardView>();

        if (targetCard == null)
        {
            Vector2 boardPoint = GetLocalPointInBoard(eventData);
            PlaceDraggedObjectOnBoard(boardPoint);
            return;
        }

        if (targetCard.gameObject == this.gameObject)
        {
            Vector2 boardPoint = GetLocalPointInBoard(eventData);
            PlaceDraggedObjectOnBoard(boardPoint);
            return;
        }

        CardInstance targetInstance = targetCard.GetComponent<CardInstance>();
        ContainerRuntime targetContainer = targetInstance != null && targetInstance.HasActiveContainerRuntime()
            ? targetInstance.ContainerRuntime
            : null;

        // =====================================================
        // Caso A: estamos arrastrando un stack
        // =====================================================
        if (draggedStack != null)
        {
            CardStack targetStack = targetCard.GetComponentInParent<CardStack>();
            List<CardView> draggedCardsCopy = new List<CardView>(draggedStack.Cards);

            // A0) Stack sobre contenedor -> guardar todas las cartas posibles.
            // Si el contenedor se llena a mitad de camino, el resto del stack
            // debe quedar en el tablero; no hay que destruirlo entero.
            if (targetContainer != null && TryStoreDraggedCardsInContainer(targetContainer, draggedCardsCopy))
            {
                if (draggedStack != null && draggedStack.gameObject != null)
                {
                    Vector2 boardPoint = GetLocalPointInBoard(eventData);
                    PlaceDraggedObjectOnBoard(boardPoint);
                }

                return;
            }

            // A1) Stack sobre otro stack -> merge completo.
            if (targetStack != null && targetStack != draggedStack)
            {
                if (!targetStack.CanAcceptCards(draggedCardsCopy))
                {
                    Vector2 boardPoint = GetLocalPointInBoard(eventData);
                    PlaceDraggedObjectOnBoard(boardPoint);
                    return;
                }

                foreach (CardView card in draggedCardsCopy)
                    targetStack.AddCard(card);

                if (draggedStack != null)
                    Destroy(draggedStack.gameObject);

                return;
            }

            // A2) Stack sobre carta suelta -> crear nuevo stack con todo.
            if (targetStack == null)
            {
                if (!CardStackFactory.CanCreateStack(targetCard, draggedCardsCopy))
                {
                    Vector2 boardPoint = GetLocalPointInBoard(eventData);
                    PlaceDraggedObjectOnBoard(boardPoint);
                    return;
                }

                CardStack newStack = CardStackFactory.CreateStack(targetCard, draggedCardsCopy[0]);
                if (newStack == null)
                {
                    Vector2 boardPoint = GetLocalPointInBoard(eventData);
                    PlaceDraggedObjectOnBoard(boardPoint);
                    return;
                }

                for (int i = 1; i < draggedCardsCopy.Count; i++)
                {
                    newStack.AddCard(draggedCardsCopy[i]);
                }

                if (draggedStack != null)
                    Destroy(draggedStack.gameObject);

                return;
            }
        }

        // =====================================================
        // Caso B: arrastramos una carta suelta
        // =====================================================
        if (targetContainer != null && targetContainer.TryStoreCard(cardView))
            return;

        CardStack existingTargetStack = targetCard.GetComponentInParent<CardStack>();

        if (existingTargetStack != null)
        {
            if (existingTargetStack.CanAcceptCard(cardView))
            {
                existingTargetStack.AddCard(cardView);
            }
            else
            {
                Vector2 boardPoint = GetLocalPointInBoard(eventData);
                PlaceDraggedObjectOnBoard(boardPoint);
            }
        }
        else
        {
            CardStack newStack = CardStackFactory.CreateStack(targetCard, cardView);
            if (newStack == null)
            {
                Vector2 boardPoint = GetLocalPointInBoard(eventData);
                PlaceDraggedObjectOnBoard(boardPoint);
            }
        }
    }

    private void OnDisable()
    {
        if (cardInstance != null)
            cardInstance.SetDragging(false);
    }
}
