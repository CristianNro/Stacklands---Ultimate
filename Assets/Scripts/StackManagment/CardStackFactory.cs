using UnityEngine;

// ============================================================
// CardStackFactory
// ------------------------------------------------------------
// Se encarga de crear stacks nuevos de forma consistente.
//
// REGLA:
// los stacks siempre nacen dentro de BoardRoot.CardsContainer.
// ============================================================
public static class CardStackFactory
{
    public static CardStack CreateStack(CardView firstCard, CardView secondCard)
    {
        if (firstCard == null || secondCard == null)
        {
            Debug.LogWarning("No se puede crear stack: una de las cartas es null.");
            return null;
        }

        RectTransform boardContainer = null;

        if (BoardRoot.Instance != null)
            boardContainer = BoardRoot.Instance.CardsContainer;

        if (boardContainer == null)
        {
            Debug.LogError("No existe BoardRoot.CardsContainer para crear stacks.");
            return null;
        }

        // Creamos el objeto stack dentro del contenedor correcto.
        GameObject stackGO = new GameObject("CardStack", typeof(RectTransform));
        RectTransform stackRT = stackGO.GetComponent<RectTransform>();

        stackRT.SetParent(boardContainer, false);
        stackRT.anchorMin = new Vector2(0.5f, 0.5f);
        stackRT.anchorMax = new Vector2(0.5f, 0.5f);
        stackRT.pivot = new Vector2(0.5f, 0.5f);
        stackRT.localScale = Vector3.one;
        stackRT.localRotation = Quaternion.identity;

        // El stack nace en la posición de la primera carta.
        RectTransform firstRT = firstCard.GetComponent<RectTransform>();
        stackRT.anchoredPosition = firstRT.anchoredPosition;

        CardStack stack = stackGO.AddComponent<CardStack>();

        // Agregamos primero la carta destino y luego la arrastrada.
        stack.AddCard(firstCard);
        stack.AddCard(secondCard);

        return stack;
    }
}