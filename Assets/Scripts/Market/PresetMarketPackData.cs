using System.Collections.Generic;
using UnityEngine;

// ============================================================
// PresetMarketPackData
// ------------------------------------------------------------
// Variante de pack que entrega siempre la misma lista exacta de
// cartas al abrirse, sin usar pesos aleatorios.
// ============================================================
[CreateAssetMenu(fileName = "PresetMarketPack", menuName = "Market/Preset Pack Data")]
public class PresetMarketPackData : BaseMarketPackData
{
    [Header("Preset Contents")]
    // Cartas exactas que entrega este pack al abrirse.
    public List<CardData> fixedCards = new List<CardData>();

    public override List<CardData> GetOpenedCards()
    {
        List<CardData> openedCards = new List<CardData>();

        for (int i = 0; i < fixedCards.Count; i++)
        {
            CardData cardData = fixedCards[i];
            if (cardData != null)
                openedCards.Add(cardData);
        }

        return openedCards;
    }

}
