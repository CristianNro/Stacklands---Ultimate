using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

// ============================================================
// CardDrag
// ------------------------------------------------------------
// Permite arrastrar:
// - una carta suelta
// - un stack completo
// - un substack si arrastrás desde el medio
//
// Esta versión corrige un bug importante:
// cuando mergeábamos un stack con otro, se recorría una lista
// que se modificaba durante el proceso.
// ============================================================
public class CardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private CardView cardView;

    // Qué rect estamos moviendo realmente.
    // Puede ser la carta sola o el rect del stack.
    private RectTransform draggedRectTransform;

    // Si estamos arrastrando un stack, queda guardado acá.
    private CardStack draggedStack;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        cardView = GetComponent<CardView>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        draggedRectTransform = rectTransform;
        draggedStack = null;

        CardStack parentStack = GetComponentInParent<CardStack>();

        if (parentStack != null)
        {
            // Si estamos dentro de un stack:
            // - raíz => arrastramos el stack entero
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

        // Mientras arrastramos, dejamos pasar raycasts.
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
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

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        GameObject hitObject = eventData.pointerCurrentRaycast.gameObject;

        // Si no tocó nada útil, se deja en el board.
        if (hitObject == null)
        {
            Vector2 boardPoint = GetLocalPointInBoard(eventData);
            PlaceDraggedObjectOnBoard(boardPoint);
            return;
        }

        CardView targetCard = hitObject.GetComponentInParent<CardView>();

        // Si no soltó sobre otra carta, también se deja en el board.
        if (targetCard == null)
        {
            Vector2 boardPoint = GetLocalPointInBoard(eventData);
            PlaceDraggedObjectOnBoard(boardPoint);
            return;
        }

        // Si soltó sobre sí misma, se considera drop en vacío.
        if (targetCard.gameObject == this.gameObject)
        {
            Vector2 boardPoint = GetLocalPointInBoard(eventData);
            PlaceDraggedObjectOnBoard(boardPoint);
            return;
        }

        // =====================================================
        // Caso A: estamos arrastrando un stack
        // =====================================================
        if (draggedStack != null)
        {
            CardStack targetStack = targetCard.GetComponentInParent<CardStack>();

            // Usamos SIEMPRE una copia para no iterar sobre una lista que cambia.
            List<CardView> draggedCardsCopy = new List<CardView>(draggedStack.Cards);

            // A1) Stack sobre otro stack -> merge completo
            if (targetStack != null && targetStack != draggedStack)
            {
                foreach (CardView card in draggedCardsCopy)
                {
                    targetStack.AddCard(card);
                }

                // Si el stack original quedó vacío o inválido, lo destruimos.
                if (draggedStack != null)
                    Destroy(draggedStack.gameObject);

                return;
            }

            // A2) Stack sobre carta suelta -> crear nuevo stack con todo
            if (targetStack == null)
            {
                CardStack newStack = CardStackFactory.CreateStack(targetCard, draggedCardsCopy[0]);

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
        CardStack existingTargetStack = targetCard.GetComponentInParent<CardStack>();

        if (existingTargetStack != null)
        {
            existingTargetStack.AddCard(cardView);
        }
        else
        {
            CardStackFactory.CreateStack(targetCard, cardView);
        }
    }
}