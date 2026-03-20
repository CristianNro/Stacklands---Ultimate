using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using StacklandsLike.Cards;

// ============================================================
// CardStack
// ------------------------------------------------------------
// Stack lógico + visual de cartas.
//
// REGLA ACTUAL:
// - 1 carta  = carta suelta
// - 2+ cartas = stack real
//
// En esta etapa el stack YA NO decide solo cuándo revisar recetas.
// Ahora solo:
// - mantiene cartas
// - mantiene layout
// - avisa cambios
// - ejecuta crafting si otro sistema se lo pide
//
// El que decide si hay receta o no ahora va a ser RecipeSystem.
// ============================================================
public class CardStack : MonoBehaviour
{
    [Header("Stack Data")]
    [SerializeField] private List<CardView> cards = new List<CardView>();

    [Header("Visual Layout")]
    [SerializeField] private Vector2 stackOffset = new Vector2(0f, -28f);

    // Evento para que otros sistemas (como RecipeSystem) reaccionen
    // cuando cambia el contenido del stack.
    public event Action<CardStack> OnStackChanged;

    // =========================================================
    // Estado de crafting (todavía transicional)
    // ---------------------------------------------------------
    // El stack todavía EJECUTA el crafting, pero ya no decide
    // por sí mismo cuándo buscar recetas.
    // =========================================================
    private GameObject progressBarRoot;
    private Image progressFillImage;
    private RectTransform progressFillRect;

    public IReadOnlyList<CardView> Cards => cards;

    // Expone la receta actual solo para consulta.
    public RecipeData ActiveRecipe => activeRecipe;
    public bool IsCrafting => isCrafting;

    // =========================================================
    // Estado visual de crafting (transicional)
    // ---------------------------------------------------------
    // En esta etapa el tiempo real lo maneja TaskSystem.
    // El stack solo mantiene la UI y sabe ejecutar el resultado.
    // =========================================================
    private RecipeData activeRecipe;
    private bool isCrafting = false;

    // =========================================================
    // Helpers
    // =========================================================

    private CardInstance GetCardInstance(CardView card)
    {
        if (card == null) return null;
        return card.GetComponent<CardInstance>();
    }

    private CardData GetCardData(CardView card)
    {
        CardInstance instance = GetCardInstance(card);
        return instance != null ? instance.data : null;
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
    /// Reparenta una carta al contenedor principal del board,
    /// conservando la posición visual.
    /// </summary>
    private void MoveCardToBoardKeepingVisualPosition(CardView card)
    {
        if (card == null) return;

        RectTransform cardRT = card.GetComponent<RectTransform>();
        RectTransform boardRT = GetBoardContainer();

        if (cardRT == null || boardRT == null)
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

        if (BoardRoot.Instance != null)
            BoardRoot.Instance.ClampCardToBoard(cardRT);
    }

    /// <summary>
    /// Limpia estados triviales:
    /// - 0 cartas -> destruir stack
    /// - 1 carta  -> volver a carta suelta
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

            if (instance != null && instance.currentStack == this)
                instance.currentStack = null;

            MoveCardToBoardKeepingVisualPosition(onlyCard);

            cards.Clear();
            StopCraftingVisuals();
            Destroy(gameObject);
        }
    }

    // =========================================================
    // API pública principal
    // =========================================================

    public void AddCard(CardView card)
    {
        if (card == null) return;
        if (cards.Contains(card)) return;

        CardInstance instance = GetCardInstance(card);
        CardStack previousStack = instance != null ? instance.currentStack : null;

        if (previousStack != null && previousStack != this)
        {
            previousStack.RemoveCard(card);
        }

        cards.Add(card);
        card.transform.SetParent(transform, worldPositionStays: false);

        if (instance != null)
            instance.currentStack = this;

        NotifyStackChanged();
    }

    public void AddCards(List<CardView> newCards)
    {
        if (newCards == null || newCards.Count == 0) return;

        bool changed = false;

        foreach (CardView card in newCards)
        {
            if (card == null) continue;
            if (cards.Contains(card)) continue;

            CardInstance instance = GetCardInstance(card);
            CardStack previousStack = instance != null ? instance.currentStack : null;

            if (previousStack != null && previousStack != this)
            {
                previousStack.RemoveCard(card);
            }

            cards.Add(card);
            card.transform.SetParent(transform, worldPositionStays: false);

            if (instance != null)
                instance.currentStack = this;

            changed = true;
        }

        if (changed)
            NotifyStackChanged();
    }

    public void RemoveCard(CardView card)
    {
        if (card == null) return;
        if (!cards.Contains(card)) return;

        cards.Remove(card);

        CardInstance instance = GetCardInstance(card);
        if (instance != null && instance.currentStack == this)
            instance.currentStack = null;

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
            CardDropTarget dropTarget = cards[0].GetComponent<CardDropTarget>();
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
    }

    public CardStack SplitFrom(CardView card)
    {
        if (card == null) return null;

        int index = cards.IndexOf(card);
        if (index == -1) return null;

        if (index == 0)
            return this;

        RectTransform boardRT = GetBoardContainer();
        if (boardRT == null)
        {
            Debug.LogWarning("No se pudo partir el stack: no existe board container.");
            return this;
        }

        GameObject newStackGO = new GameObject("CardStack", typeof(RectTransform));
        RectTransform newStackRT = newStackGO.GetComponent<RectTransform>();

        RectTransform draggedCardRT = card.GetComponent<RectTransform>();
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, draggedCardRT.position);
        Vector2 boardLocalPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            boardRT,
            screenPoint,
            null,
            out boardLocalPoint
        );

        newStackRT.SetParent(boardRT, false);
        newStackRT.anchorMin = new Vector2(0.5f, 0.5f);
        newStackRT.anchorMax = new Vector2(0.5f, 0.5f);
        newStackRT.pivot = new Vector2(0.5f, 0.5f);
        newStackRT.localScale = Vector3.one;
        newStackRT.localRotation = Quaternion.identity;
        newStackRT.anchoredPosition = boardLocalPoint;

        CardStack newStack = newStackGO.AddComponent<CardStack>();
        newStack.stackOffset = this.stackOffset;

        List<CardView> movedCards = new List<CardView>();

        for (int i = index; i < cards.Count; i++)
            movedCards.Add(cards[i]);

        foreach (CardView movedCard in movedCards)
        {
            cards.Remove(movedCard);

            CardInstance movedInstance = GetCardInstance(movedCard);
            if (movedInstance != null && movedInstance.currentStack == this)
                movedInstance.currentStack = null;
        }

        newStack.AddCards(movedCards);

        CleanupTrivialState();

        if (this != null && gameObject != null && cards.Count >= 2)
            NotifyStackChanged();

        return newStack;
    }

    // =========================================================
    // Consultas públicas
    // =========================================================

    public int GetCardIndex(CardView card)
    {
        return cards.IndexOf(card);
    }

    public bool Contains(CardView card)
    {
        return cards.Contains(card);
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

    /// <summary>
    /// Cuenta cuántas cartas del stack tienen un tag dado.
    /// </summary>
    public int CountCardsWithTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return 0;

        int count = 0;

        for (int i = 0; i < cards.Count; i++)
        {
            CardView card = cards[i];
            if (card == null) continue;

            CardInstance instance = card.GetComponent<CardInstance>();
            if (instance == null) continue;

            if (instance.HasTag(tag))
                count++;
        }

        return count;
    }

    /// <summary>
    /// Devuelve true si alguna carta del stack tiene el tag indicado.
    /// Usa CardInstance.HasTag(...) para consultar la data runtime.
    /// </summary>
    public bool ContainsCardWithTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return false;

        for (int i = 0; i < cards.Count; i++)
        {
            CardView card = cards[i];
            if (card == null) continue;

            CardInstance instance = card.GetComponent<CardInstance>();
            if (instance == null) continue;

            if (instance.HasTag(tag))
                return true;
        }

        return false;
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

    /// <summary>
    /// Devuelve el tamaño total visual del stack.
    /// </summary>
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

        CardDropTarget dropTarget = cards[0].GetComponent<CardDropTarget>();
        if (dropTarget != null)
            effectiveOffset = dropTarget.GetStackOffset();

        if (cards.Count == 1)
            return new Vector2(cardWidth, cardHeight);

        float totalExtraX = Mathf.Abs(effectiveOffset.x) * (cards.Count - 1);
        float totalExtraY = Mathf.Abs(effectiveOffset.y) * (cards.Count - 1);

        return new Vector2(cardWidth + totalExtraX, cardHeight + totalExtraY);
    }

    /// <summary>
    /// Devuelve cuánto se extiende visualmente el stack hacia cada lado
    /// respecto del root.
    /// </summary>
    public void GetVisualExtents(out float left, out float right, out float bottom, out float top)
    {
        left = 0f;
        right = 0f;
        bottom = 0f;
        top = 0f;

        if (cards == null || cards.Count == 0)
            return;

        RectTransform rootCardRT = cards[0].GetComponent<RectTransform>();
        if (rootCardRT == null)
            return;

        float cardWidth = rootCardRT.rect.width * rootCardRT.lossyScale.x;
        float cardHeight = rootCardRT.rect.height * rootCardRT.lossyScale.y;

        Vector2 effectiveOffset = stackOffset;

        CardDropTarget dropTarget = cards[0].GetComponent<CardDropTarget>();
        if (dropTarget != null)
            effectiveOffset = dropTarget.GetStackOffset();

        float minX = 0f;
        float maxX = 0f;
        float minY = 0f;
        float maxY = 0f;

        for (int i = 0; i < cards.Count; i++)
        {
            Vector2 pos = effectiveOffset * i;

            float cardMinX = pos.x - (cardWidth * 0.5f);
            float cardMaxX = pos.x + (cardWidth * 0.5f);
            float cardMinY = pos.y - (cardHeight * 0.5f);
            float cardMaxY = pos.y + (cardHeight * 0.5f);

            if (i == 0)
            {
                minX = cardMinX;
                maxX = cardMaxX;
                minY = cardMinY;
                maxY = cardMaxY;
            }
            else
            {
                minX = Mathf.Min(minX, cardMinX);
                maxX = Mathf.Max(maxX, cardMaxX);
                minY = Mathf.Min(minY, cardMinY);
                maxY = Mathf.Max(maxY, cardMaxY);
            }
        }

        left = -minX;
        right = maxX;
        bottom = -minY;
        top = maxY;
    }

    // =========================================================
    // Notificación central
    // =========================================================

    /// <summary>
    /// Avisa que cambió el contenido del stack.
    ///
    /// IMPORTANTE:
    /// En esta etapa ya NO revisa recetas acá adentro.
    /// Solo:
    /// - refresca layout
    /// - notifica evento
    /// </summary>
    private void NotifyStackChanged()
    {
        RefreshLayout();
        OnStackChanged?.Invoke(this);
    }

    // =========================================================
    // Crafting controlado por RecipeSystem
    // =========================================================

    /// <summary>
    /// Prepara y muestra la UI visual cuando TaskSystem le dice que hay
    /// una tarea corriendo.
    /// </summary>
    public void StartCraftingVisuals(RecipeData recipe)
    {
        if (recipe == null) return;

        // Si ya está mostrando la misma receta, no hace falta reiniciar.
        if (isCrafting && activeRecipe == recipe)
            return;

        activeRecipe = recipe;
        isCrafting = true;

        CreateProgressBar();
        SetCraftingProgress(0f);

    }
    

    /// <summary>
    /// Detiene la UI visual del crafting actual.
    /// El tiempo real ya no lo maneja el stack.
    /// </summary>
    public void StopCraftingVisuals()
    {
        activeRecipe = null;
        isCrafting = false;

        DestroyProgressBar();
    }

    /// <summary>
    /// Actualiza visualmente el progreso del crafting.
    /// progress01 debe venir normalizado entre 0 y 1.
    /// </summary>
    public void SetCraftingProgress(float progress01)
    {
        if (!isCrafting) return;
        if (progressFillRect == null) return;

        float progress = Mathf.Clamp01(progress01);
        float fullWidth = 100f;

        progressFillRect.sizeDelta = new Vector2(fullWidth * progress, 0f);
    }

    /// <summary>
    /// Completa una receta cuando TaskSystem informa que terminó la tarea.
    /// Por ahora el stack sigue ejecutando el resultado final.
    /// Más adelante esto también se va a separar.
    /// </summary>
    public void CompleteRecipeFromTask(RecipeData recipe)
    {
        if (recipe == null)
        {
            StopCraftingVisuals();
            return;
        }

        CardData rolledResult = recipe.RollResult();

        if (rolledResult == null)
        {
            StopCraftingVisuals();
            Debug.LogWarning($"[{name}] La receta no devolvió ningún resultado válido.");
            return;
        }

        Debug.Log($"[{name}] Receta completada. Resultado sorteado: {rolledResult.cardName}");

        Vector2 spawnPos = GetStackPosition();
        List<CardView> cardsToProcess = new List<CardView>(cards);

        foreach (CardView card in cardsToProcess)
        {
            if (card == null) continue;

            CardInstance instance = card.GetComponent<CardInstance>();
            if (instance == null || instance.data == null) continue;

            CardData cardData = instance.data;

            // --------------------------------------------------------
            // 1. Intentamos usar regla explícita de la receta
            // --------------------------------------------------------
            RecipeIngredientConsumeMode? explicitMode = recipe.GetConsumeModeForCard(cardData);

            if (explicitMode.HasValue)
            {
                switch (explicitMode.Value)
                {
                    case RecipeIngredientConsumeMode.None:
                        // No pasa nada, la carta queda intacta.
                        break;

                    case RecipeIngredientConsumeMode.ConsumeOneUse:
                        // Consume un uso; si se queda sin usos, se destruye.
                        if (instance.ConsumeUseIfNeeded())
                        {
                            cards.Remove(card);

                            if (instance.currentStack == this)
                                instance.currentStack = null;

                            Destroy(card.gameObject);
                        }
                        break;

                    case RecipeIngredientConsumeMode.ConsumeEntireCard:
                        // Se destruye directamente, sin mirar usos.
                        cards.Remove(card);

                        if (instance.currentStack == this)
                            instance.currentStack = null;

                        Destroy(card.gameObject);
                        break;
                }

                // Si hubo regla explícita, no aplicamos fallback.
                continue;
            }

            // --------------------------------------------------------
            // 2. Fallback al comportamiento viejo
            // --------------------------------------------------------
            // Si la receta no define regla para esta carta, conservamos
            // el comportamiento anterior para no romper recetas existentes.
            if (instance.ConsumeUseIfNeeded())
            {
                cards.Remove(card);

                if (instance.currentStack == this)
                    instance.currentStack = null;

                Destroy(card.gameObject);
            }
        }

        if (CardSpawner.Instance != null)
            CardSpawner.Instance.SpawnAnimated(rolledResult, spawnPos);

        StopCraftingVisuals();

        CleanupTrivialState();

        if (this != null && gameObject != null && cards.Count >= 2)
            NotifyStackChanged();
    }

    // =========================================================
    // UI de progreso (transicional)
    // =========================================================

    private void CreateProgressBar()
    {
        if (progressBarRoot != null) return;

        progressBarRoot = new GameObject("CraftProgressBar", typeof(RectTransform));
        progressBarRoot.transform.SetParent(transform, false);

        RectTransform rootRT = progressBarRoot.GetComponent<RectTransform>();
        rootRT.anchoredPosition = new Vector2(0f, 65f);
        rootRT.sizeDelta = new Vector2(100f, 12f);
        rootRT.anchorMin = new Vector2(0.5f, 0.5f);
        rootRT.anchorMax = new Vector2(0.5f, 0.5f);
        rootRT.pivot = new Vector2(0.5f, 0.5f);

        GameObject backgroundGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        backgroundGO.transform.SetParent(progressBarRoot.transform, false);

        RectTransform bgRT = backgroundGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        Image bgImage = backgroundGO.GetComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.6f);

        GameObject fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGO.transform.SetParent(backgroundGO.transform, false);

        progressFillRect = fillGO.GetComponent<RectTransform>();
        progressFillRect.anchorMin = new Vector2(0f, 0f);
        progressFillRect.anchorMax = new Vector2(0f, 1f);
        progressFillRect.pivot = new Vector2(0f, 0.5f);
        progressFillRect.anchoredPosition = Vector2.zero;
        progressFillRect.sizeDelta = new Vector2(0f, 0f);

        progressFillImage = fillGO.GetComponent<Image>();
        progressFillImage.color = new Color(0.2f, 0.9f, 0.2f, 1f);
    }

    private void DestroyProgressBar()
    {
        if (progressBarRoot != null)
        {
            Destroy(progressBarRoot);
            progressBarRoot = null;
            progressFillImage = null;
            progressFillRect = null;
        }
    }
}