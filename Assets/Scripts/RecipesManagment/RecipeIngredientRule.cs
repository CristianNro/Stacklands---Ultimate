using System;
using UnityEngine;
using StacklandsLike.Cards;

// ============================================================
// RecipeIngredientRule
// ------------------------------------------------------------
// Regla de consumo para un ingrediente dentro de una receta.
//
// cardId:
// - debería coincidir con CardData.id
//
// consumeMode:
// - define qué pasa con ese ingrediente al completar la receta
// ============================================================
[Serializable]
public class RecipeIngredientRule
{
    [Header("Ingredient Match")]
    public CardData card;

    [Header("Exact Ingredient Matching")]
    [Min(1)]
    public int requiredCount = 1;
    public bool allowAdditionalCopies = false;

    [Header("Consume Behavior")]
    public RecipeIngredientConsumeMode consumeMode = RecipeIngredientConsumeMode.None;

    public bool Matches(CardData cardData)
    {
        if (cardData == null || card == null)
            return false;

        return card == cardData;
    }

    public string GetIngredientCardId()
    {
        return card != null ? card.id : string.Empty;
    }

    public int GetNormalizedRequiredCount()
    {
        return Mathf.Max(1, requiredCount);
    }
}
