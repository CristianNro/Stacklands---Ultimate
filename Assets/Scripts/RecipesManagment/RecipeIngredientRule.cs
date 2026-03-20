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
    public string cardId;

    [Header("Consume Behavior")]
    public RecipeIngredientConsumeMode consumeMode = RecipeIngredientConsumeMode.None;
}