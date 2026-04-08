using System.Collections.Generic;
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
    public static bool CanCreateStack(CardView firstCard, IReadOnlyList<CardView> additionalCards)
    {
        CardInstance firstInstance = firstCard != null ? firstCard.GetComponent<CardInstance>() : null;
        if (firstInstance == null || additionalCards == null || additionalCards.Count == 0)
            return false;

        List<CardInstance> additionalInstances = new List<CardInstance>();

        for (int i = 0; i < additionalCards.Count; i++)
        {
            CardInstance additionalInstance = additionalCards[i] != null ? additionalCards[i].GetComponent<CardInstance>() : null;
            if (additionalInstance == null)
                return false;

            additionalInstances.Add(additionalInstance);
        }

        return StackRules.CanCardsExistInSameStack(
            existingCards: null,
            incomingCards: BuildIncomingList(firstInstance, additionalInstances),
            out _
        );
    }

    public static CardStack CreateStack(CardView firstCard, CardView secondCard)
    {
        if (firstCard == null || secondCard == null)
        {
            Debug.LogWarning("No se puede crear stack: una de las cartas es null.");
            return null;
        }

        if (!CanCreateStack(firstCard, new List<CardView> { secondCard }))
            return null;

        if (BoardRoot.Instance == null)
        {
            Debug.LogError("No existe BoardRoot.CardsContainer para crear stacks.");
            return null;
        }

        RectTransform firstRT = firstCard.GetComponent<RectTransform>();
        if (firstRT == null)
            return null;

        RectTransform stackRT = BoardRoot.Instance.CreateBoardRectTransform("CardStack", firstRT.anchoredPosition, clampToBoard: true);
        if (stackRT == null)
            return null;

        CardStack stack = stackRT.gameObject.AddComponent<CardStack>();

        // Agregamos primero la carta destino y luego la arrastrada.
        if (!stack.TryAddCard(firstCard) || !stack.TryAddCard(secondCard))
        {
            Object.Destroy(stackRT.gameObject);
            return null;
        }

        return stack;
    }

    private static List<CardInstance> BuildIncomingList(CardInstance firstInstance, List<CardInstance> additionalInstances)
    {
        List<CardInstance> instances = new List<CardInstance>();

        if (firstInstance != null)
            instances.Add(firstInstance);

        if (additionalInstances != null)
            instances.AddRange(additionalInstances);

        return instances;
    }
}
