using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using StacklandsLike.Cards;

// ============================================================
// CardStack
// ------------------------------------------------------------
// Stack logico + visual de cartas.
//
// REGLA ACTUAL:
// - 1 carta  = carta suelta
// - 2+ cartas = stack real
//
// En esta etapa el stack ya no decide cuando buscar recetas.
// Ahora solo:
// - mantiene cartas
// - mantiene layout
// - avisa cambios
// - ejecuta crafting si otro sistema se lo pide
// ============================================================
public class CardStack : MonoBehaviour, ICardDropTargetSource
{
    [Header("Stack Data")]
    [SerializeField] private List<CardView> cards = new List<CardView>();

    [Header("Visual Layout")]
    [SerializeField] private Vector2 stackOffset = new Vector2(0f, -28f);

    // Evento para que otros sistemas reaccionen cuando cambia el stack.
    public event Action<CardStack> OnStackChanged;

    private CardStackCraftingVisuals craftingVisuals;
    private bool lifecycleRegistered;
    private RectTransform dropSurfaceRect;
    private Image dropSurfaceImage;

    public IReadOnlyList<CardView> Cards => cards;
    public RecipeData ActiveRecipe => craftingVisuals != null ? craftingVisuals.ActiveRecipe : null;
    public bool IsCrafting => craftingVisuals != null && craftingVisuals.IsCrafting;

    private CardInstance GetCardInstance(CardView card)
    {
        return card != null ? card.Instance : null;
    }

    private CardData GetCardData(CardView card)
    {
        CardInstance instance = GetCardInstance(card);
        return instance != null ? instance.data : null;
    }

    private CardStackCraftingVisuals GetOrCreateCraftingVisuals()
    {
        if (craftingVisuals == null)
            craftingVisuals = GetComponent<CardStackCraftingVisuals>();

        if (craftingVisuals == null)
            craftingVisuals = gameObject.AddComponent<CardStackCraftingVisuals>();

        return craftingVisuals;
    }

    private void OnEnable()
    {
        if (lifecycleRegistered)
            return;

        lifecycleRegistered = true;

        if (BoardRoot.Instance != null)
            BoardRoot.Instance.RegisterStack(this);

        EnsureDropSurface();
    }

    private void OnDisable()
    {
        UnregisterLifecycleIfNeeded();
    }

    private void OnDestroy()
    {
        UnregisterLifecycleIfNeeded();
    }

    private void UnregisterLifecycleIfNeeded()
    {
        if (!lifecycleRegistered)
            return;

        lifecycleRegistered = false;

        if (BoardRoot.Instance != null)
            BoardRoot.Instance.UnregisterStack(this);
    }

    private RectTransform GetBoardContainer()
    {
        if (BoardRoot.Instance != null && BoardRoot.Instance.CardsContainer != null)
            return BoardRoot.Instance.CardsContainer;

        if (BoardRoot.Instance != null)
            return BoardRoot.Instance.GetComponent<RectTransform>();

        return null;
    }

    /// <summary>
    /// Reparenta una carta al board conservando la posicion visual.
    /// </summary>
    private void MoveCardToBoardKeepingVisualPosition(CardView card)
    {
        if (card == null) return;

        RectTransform cardRT = card.GetComponent<RectTransform>();
        if (cardRT == null)
        {
            return;
        }

        if (BoardRoot.Instance != null)
        {
            BoardRoot.Instance.TryMoveRectToBoardKeepingVisualPosition(cardRT, worldCamera: null, eventCamera: null, clampToBoard: true);
            return;
        }

        RectTransform boardRT = GetBoardContainer();
        if (boardRT == null)
        {
            card.transform.SetParent(null, true);
            return;
        }

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, cardRT.position);
        Vector2 boardLocalPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            boardRT,
            screenPoint,
            null,
            out boardLocalPoint
        );

        cardRT.SetParent(boardRT, false);
        cardRT.anchoredPosition = boardLocalPoint;
    }

    /// <summary>
    /// Limpia los estados triviales del stack.
    /// </summary>
    private void CleanupTrivialState()
    {
        if (cards.Count == 0)
        {
            StopCraftingVisuals();
            Destroy(gameObject);
            return;
        }

        if (cards.Count == 1)
        {
            CardView onlyCard = cards[0];
            CardInstance instance = GetCardInstance(onlyCard);

            if (instance != null && instance.CurrentStack == this)
                instance.ClearCurrentStack(this);

            MoveCardToBoardKeepingVisualPosition(onlyCard);

            cards.Clear();
            StopCraftingVisuals();
            Destroy(gameObject);
        }
    }

    public void AddCard(CardView card)
    {
        TryAddCard(card);
    }

    public bool TryAddCard(CardView card, bool logFailure = true)
    {
        if (card == null)
            return false;

        if (cards.Contains(card))
            return false;

        if (!CanAcceptCard(card, out string rejectionReason))
        {
            if (logFailure)
                Debug.LogWarning($"[{name}] No se pudo agregar la carta al stack: {rejectionReason}");

            return false;
        }

        CardInstance instance = GetCardInstance(card);
        CardStack previousStack = instance != null ? instance.CurrentStack : null;

        if (previousStack != null && previousStack != this)
            previousStack.RemoveCard(card);

        cards.Add(card);
        card.transform.SetParent(transform, worldPositionStays: false);

        if (instance != null)
            instance.SetCurrentStack(this);

        NotifyStackChanged();
        return true;
    }

    public void AddCards(List<CardView> newCards)
    {
        TryAddCards(newCards);
    }

    public bool TryAddCards(IReadOnlyList<CardView> newCards, bool logFailure = true)
    {
        if (newCards == null || newCards.Count == 0)
            return false;

        if (!CanAcceptCards(newCards, out string rejectionReason))
        {
            if (logFailure)
                Debug.LogWarning($"[{name}] No se pudieron agregar cartas al stack: {rejectionReason}");

            return false;
        }

        bool changed = false;

        for (int i = 0; i < newCards.Count; i++)
        {
            CardView card = newCards[i];
            if (card == null) continue;
            if (cards.Contains(card)) continue;

            CardInstance instance = GetCardInstance(card);
            CardStack previousStack = instance != null ? instance.CurrentStack : null;

            if (previousStack != null && previousStack != this)
                previousStack.RemoveCard(card);

            cards.Add(card);
            card.transform.SetParent(transform, worldPositionStays: false);

            if (instance != null)
                instance.SetCurrentStack(this);

            changed = true;
        }

        if (changed)
            NotifyStackChanged();

        return changed;
    }

    public void RemoveCard(CardView card)
    {
        if (card == null) return;
        if (!cards.Contains(card)) return;

        cards.Remove(card);

        CardInstance instance = GetCardInstance(card);
        if (instance != null && instance.CurrentStack == this)
            instance.ClearCurrentStack(this);

        MoveCardToBoardKeepingVisualPosition(card);

        CleanupTrivialState();

        if (this != null && gameObject != null && cards.Count >= 2)
            NotifyStackChanged();
    }

    public void RefreshLayout()
    {
        Vector2 effectiveOffset = stackOffset;

        if (cards.Count > 0 && cards[0] != null)
        {
            CardStackLayoutAnchor dropTarget = cards[0].GetComponent<CardStackLayoutAnchor>();
            if (dropTarget != null)
                effectiveOffset = dropTarget.GetStackOffset();
        }

        for (int i = 0; i < cards.Count; i++)
        {
            CardView card = cards[i];
            if (card == null) continue;

            RectTransform rt = card.GetComponent<RectTransform>();
            if (rt == null) continue;

            rt.anchoredPosition = effectiveOffset * i;
            card.transform.SetSiblingIndex(i);
        }

        RefreshDropBounds();

        if (dropSurfaceRect != null)
            dropSurfaceRect.SetAsFirstSibling();
    }

    public CardStack SplitFrom(CardView card)
    {
        if (card == null) return null;

        int index = cards.IndexOf(card);
        if (index == -1) return null;

        if (index == 0)
            return this;

        int movedCardCount = cards.Count - index;
        if (movedCardCount == 1)
        {
            cards.Remove(card);

            CardInstance instance = GetCardInstance(card);
            if (instance != null && instance.CurrentStack == this)
                instance.ClearCurrentStack(this);

            MoveCardToBoardKeepingVisualPosition(card);

            CleanupTrivialState();

            if (this != null && gameObject != null && cards.Count >= 2)
                NotifyStackChanged();

            return null;
        }

        RectTransform boardRT = GetBoardContainer();
        if (boardRT == null)
        {
            Debug.LogWarning("No se pudo partir el stack: no existe board container.");
            return this;
        }

        RectTransform draggedCardRT = card.GetComponent<RectTransform>();
        Vector2 boardLocalPoint = BoardRoot.Instance != null
            ? BoardRoot.Instance.GetBoardPointFromWorldPosition(draggedCardRT.position)
            : Vector2.zero;

        if (BoardRoot.Instance == null)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, draggedCardRT.position);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                boardRT,
                screenPoint,
                null,
                out boardLocalPoint
            );
        }

        RectTransform newStackRT = BoardRoot.Instance != null
            ? BoardRoot.Instance.CreateBoardRectTransform("CardStack", boardLocalPoint, clampToBoard: true)
            : null;

        if (newStackRT == null)
        {
            GameObject fallbackStackGO = new GameObject("CardStack", typeof(RectTransform));
            newStackRT = fallbackStackGO.GetComponent<RectTransform>();
            newStackRT.SetParent(boardRT, false);
            newStackRT.anchorMin = new Vector2(0.5f, 0.5f);
            newStackRT.anchorMax = new Vector2(0.5f, 0.5f);
            newStackRT.pivot = new Vector2(0.5f, 0.5f);
            newStackRT.localScale = Vector3.one;
            newStackRT.localRotation = Quaternion.identity;
            newStackRT.anchoredPosition = boardLocalPoint;
        }

        CardStack newStack = newStackRT.gameObject.AddComponent<CardStack>();
        newStack.stackOffset = this.stackOffset;
        List<CardView> movedCards = new List<CardView>();

        for (int i = index; i < cards.Count; i++)
            movedCards.Add(cards[i]);

        foreach (CardView movedCard in movedCards)
        {
            cards.Remove(movedCard);

            CardInstance movedInstance = GetCardInstance(movedCard);
            if (movedInstance != null && movedInstance.CurrentStack == this)
                movedInstance.ClearCurrentStack(this);
        }

        newStack.AddCards(movedCards);

        CleanupTrivialState();

        if (this != null && gameObject != null && cards.Count >= 2)
            NotifyStackChanged();

        return newStack;
    }

    public int GetCardIndex(CardView card)
    {
        return cards.IndexOf(card);
    }

    public bool Contains(CardView card)
    {
        return cards.Contains(card);
    }

    public float GetCurrentTotalWeight()
    {
        float totalWeight = 0f;

        for (int i = 0; i < cards.Count; i++)
        {
            CardInstance instance = GetCardInstance(cards[i]);
            if (instance == null)
                continue;

            totalWeight += instance.GetWeight();
        }

        return totalWeight;
    }

    public bool IsPlayerMovable()
    {
        if (cards.Count == 0)
            return false;

        for (int i = 0; i < cards.Count; i++)
        {
            CardInstance instance = GetCardInstance(cards[i]);
            if (instance == null || !instance.IsMovable())
                return false;
        }

        return true;
    }

    public bool CanDragFrom(CardView card)
    {
        if (card == null)
            return false;

        int index = cards.IndexOf(card);
        if (index == -1)
            return false;

        for (int i = index; i < cards.Count; i++)
        {
            CardInstance instance = GetCardInstance(cards[i]);
            if (instance == null || !instance.IsMovable())
                return false;
        }

        return true;
    }

    public bool CanAcceptCard(CardView card)
    {
        return CanAcceptCard(card, out _);
    }

    public bool CanAcceptCard(CardView card, out string rejectionReason)
    {
        rejectionReason = null;

        if (card == null)
        {
            rejectionReason = "la carta es null";
            return false;
        }

        CardInstance incomingInstance = GetCardInstance(card);
        if (incomingInstance == null)
        {
            rejectionReason = "la carta entrante no tiene CardInstance";
            return false;
        }

        List<CardInstance> incomingInstances = new List<CardInstance> { incomingInstance };
        return StackRules.CanCardsExistInSameStack(GetCardInstancesSnapshot(), incomingInstances, out rejectionReason);
    }

    public bool CanAcceptCards(IReadOnlyList<CardView> incomingCards)
    {
        return CanAcceptCards(incomingCards, out _);
    }

    public bool CanAcceptCards(IReadOnlyList<CardView> incomingCards, out string rejectionReason)
    {
        rejectionReason = null;

        if (incomingCards == null || incomingCards.Count == 0)
        {
            rejectionReason = "no hay cartas entrantes";
            return false;
        }

        List<CardInstance> incomingInstances = new List<CardInstance>();

        for (int i = 0; i < incomingCards.Count; i++)
        {
            CardInstance incomingInstance = GetCardInstance(incomingCards[i]);
            if (incomingInstance == null)
            {
                rejectionReason = "una de las cartas entrantes no tiene CardInstance";
                return false;
            }

            incomingInstances.Add(incomingInstance);
        }

        return StackRules.CanCardsExistInSameStack(GetCardInstancesSnapshot(), incomingInstances, out rejectionReason);
    }

    private List<CardInstance> GetCardInstancesSnapshot()
    {
        List<CardInstance> instances = new List<CardInstance>();

        for (int i = 0; i < cards.Count; i++)
        {
            CardInstance instance = GetCardInstance(cards[i]);
            if (instance != null)
                instances.Add(instance);
        }

        return instances;
    }

    public List<CardData> GetCardDataList()
    {
        List<CardData> result = new List<CardData>();

        foreach (CardView card in cards)
        {
            CardData data = GetCardData(card);
            if (data != null)
                result.Add(data);
        }

        return result;
    }

    public int CountCardsWithCapability(CardCapabilityType capability)
    {
        if (capability == CardCapabilityType.None)
            return 0;

        int count = 0;

        for (int i = 0; i < cards.Count; i++)
        {
            CardView card = cards[i];
            if (card == null) continue;

            CardInstance instance = GetCardInstance(card);
            if (instance == null) continue;

            if (instance.HasCapability(capability))
                count++;
        }

        return count;
    }

    public CardView GetRootCard()
    {
        if (cards.Count == 0) return null;
        return cards[0];
    }

    public Vector2 GetStackPosition()
    {
        RectTransform rt = GetComponent<RectTransform>();
        return rt != null ? rt.anchoredPosition : Vector2.zero;
    }

    public bool IsEmpty()
    {
        return cards.Count == 0;
    }

    public bool HasOnlyOneCard()
    {
        return cards.Count == 1;
    }

    public Vector2 GetVisualSize()
    {
        if (cards == null || cards.Count == 0)
            return Vector2.zero;

        RectTransform rootCardRT = cards[0].GetComponent<RectTransform>();
        if (rootCardRT == null)
            return Vector2.zero;

        float cardWidth = rootCardRT.rect.width * rootCardRT.lossyScale.x;
        float cardHeight = rootCardRT.rect.height * rootCardRT.lossyScale.y;

        Vector2 effectiveOffset = stackOffset;

        CardStackLayoutAnchor dropTarget = cards[0].GetComponent<CardStackLayoutAnchor>();
        if (dropTarget != null)
            effectiveOffset = dropTarget.GetStackOffset();

        if (cards.Count == 1)
            return new Vector2(cardWidth, cardHeight);

        float totalExtraX = Mathf.Abs(effectiveOffset.x) * (cards.Count - 1);
        float totalExtraY = Mathf.Abs(effectiveOffset.y) * (cards.Count - 1);

        return new Vector2(cardWidth + totalExtraX, cardHeight + totalExtraY);
    }

    public void GetVisualExtents(out float left, out float right, out float bottom, out float top)
    {
        left = 0f;
        right = 0f;
        bottom = 0f;
        top = 0f;

        if (cards == null || cards.Count == 0)
            return;

        bool hasBounds = false;
        float minX = 0f;
        float maxX = 0f;
        float minY = 0f;
        float maxY = 0f;

        for (int i = 0; i < cards.Count; i++)
        {
            RectTransform cardRect = cards[i] != null ? cards[i].GetComponent<RectTransform>() : null;
            if (cardRect == null)
                continue;

            Vector3[] worldCorners = new Vector3[4];
            cardRect.GetWorldCorners(worldCorners);

            for (int cornerIndex = 0; cornerIndex < worldCorners.Length; cornerIndex++)
            {
                Vector3 localCorner3 = transform.InverseTransformPoint(worldCorners[cornerIndex]);
                float x = localCorner3.x;
                float y = localCorner3.y;

                if (!hasBounds)
                {
                    minX = maxX = x;
                    minY = maxY = y;
                    hasBounds = true;
                    continue;
                }

                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x);
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y);
            }
        }

        if (!hasBounds)
            return;

        left = -minX;
        right = maxX;
        bottom = -minY;
        top = maxY;
    }

    private void NotifyStackChanged()
    {
        RefreshLayout();

        if (BoardRoot.Instance != null)
            BoardRoot.Instance.ClampStackVisualBoundsToBoard(this);

        OnStackChanged?.Invoke(this);
    }

    private void RefreshDropBounds()
    {
        EnsureDropSurface();

        if (dropSurfaceRect == null)
            return;

        GetVisualExtents(out float left, out float right, out float bottom, out float top);
        float width = left + right;
        float height = bottom + top;

        if (width <= 0f || height <= 0f)
            return;

        dropSurfaceRect.anchoredPosition = new Vector2(
            (right - left) * 0.5f,
            (top - bottom) * 0.5f);
        dropSurfaceRect.sizeDelta = new Vector2(width, height);
    }

    private void EnsureDropSurface()
    {
        if (dropSurfaceRect != null && dropSurfaceImage != null)
            return;

        Transform existing = transform.Find("DropSurface");
        if (existing != null)
        {
            dropSurfaceRect = existing as RectTransform;
            dropSurfaceImage = existing.GetComponent<Image>();
        }

        if (dropSurfaceRect == null)
        {
            GameObject surfaceGO = new GameObject("DropSurface", typeof(RectTransform), typeof(Image));
            surfaceGO.transform.SetParent(transform, false);
            surfaceGO.transform.SetAsFirstSibling();

            dropSurfaceRect = surfaceGO.GetComponent<RectTransform>();
            dropSurfaceRect.anchorMin = new Vector2(0.5f, 0.5f);
            dropSurfaceRect.anchorMax = new Vector2(0.5f, 0.5f);
            dropSurfaceRect.pivot = new Vector2(0.5f, 0.5f);

            dropSurfaceImage = surfaceGO.GetComponent<Image>();
        }

        if (dropSurfaceImage != null)
        {
            dropSurfaceImage.color = new Color(0f, 0f, 0f, 0f);
            dropSurfaceImage.raycastTarget = true;
        }
    }

    public void PopulateDropTargetInfo(CardDropTargetInfo targetInfo)
    {
        if (targetInfo == null)
            return;

        targetInfo.targetStack = this;

        if (targetInfo.primaryType == CardDropTargetType.None)
            targetInfo.primaryType = CardDropTargetType.Stack;
    }

    public void SetDropSurfaceRaycastEnabled(bool enabled)
    {
        EnsureDropSurface();

        if (dropSurfaceImage != null)
            dropSurfaceImage.raycastTarget = enabled;
    }

    public void StartCraftingVisuals(RecipeData recipe)
    {
        GetOrCreateCraftingVisuals().StartVisuals(recipe);
    }

    public void StopCraftingVisuals()
    {
        if (craftingVisuals != null)
            craftingVisuals.StopVisuals();
    }

    public void SetCraftingProgress(float progress01)
    {
        if (craftingVisuals != null)
            craftingVisuals.SetProgress(progress01);
    }

    public void DestroyCardForSystem(CardView card)
    {
        if (card == null)
            return;

        CardInstance instance = GetCardInstance(card);
        cards.Remove(card);

        if (instance != null && instance.CurrentStack == this)
            instance.ClearCurrentStack(this);

        Destroy(card.gameObject);
    }

    public void FinalizeCraftingMutation()
    {
        CleanupTrivialState();

        if (this != null && gameObject != null && cards.Count >= 2)
            NotifyStackChanged();
    }

}
