using System.Collections.Generic;
using UnityEngine;

// ============================================================
// MarketPackData
// ------------------------------------------------------------
// Variante aleatoria de pack del Market.
// Usa cardsToRoll + possibleCards con pesos.
// ============================================================
[CreateAssetMenu(fileName = "MarketPack", menuName = "Market/Random Pack Data")]
public class MarketPackData : BaseMarketPackData
{
    [Header("Open Result")]
    // Cantidad de cartas que entrega el sobre cuando se abra.
    [Min(1)]
    public int cardsToRoll = 1;

    // Pool ponderado de cartas posibles dentro del sobre.
    public List<RecipeResultOption> possibleCards = new List<RecipeResultOption>();

    /// <summary>
    /// Devuelve la lista de cartas que salen al abrir este pack.
    /// La implementacion base mantiene el comportamiento aleatorio actual.
    /// </summary>
    public override List<CardData> GetOpenedCards()
    {
        List<CardData> openedCards = new List<CardData>();
        int amountToOpen = Mathf.Max(1, cardsToRoll);

        for (int i = 0; i < amountToOpen; i++)
        {
            CardData rolledCard = RollCard();
            if (rolledCard != null)
                openedCards.Add(rolledCard);
        }

        return openedCards;
    }

    /// <summary>
    /// Devuelve una carta aleatoria ponderada del pool del sobre.
    /// El sistema de apertura reutiliza este helper para packs aleatorios.
    /// </summary>
    public virtual CardData RollCard()
    {
        if (possibleCards == null || possibleCards.Count == 0)
            return null;

        float totalWeight = 0f;

        for (int i = 0; i < possibleCards.Count; i++)
        {
            RecipeResultOption option = possibleCards[i];
            if (option == null || option.result == null || option.weight <= 0f)
                continue;

            totalWeight += option.weight;
        }

        if (totalWeight <= 0f)
            return null;

        float roll = Random.Range(0f, totalWeight);
        float accumulated = 0f;

        for (int i = 0; i < possibleCards.Count; i++)
        {
            RecipeResultOption option = possibleCards[i];
            if (option == null || option.result == null || option.weight <= 0f)
                continue;

            accumulated += option.weight;
            if (roll <= accumulated)
                return option.result;
        }

        return null;
    }
}
