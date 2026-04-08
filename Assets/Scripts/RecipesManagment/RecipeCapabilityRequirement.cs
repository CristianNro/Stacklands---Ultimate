using System;
using UnityEngine;
using StacklandsLike.Cards;

// ============================================================
// RecipeCapabilityRequirement
// ------------------------------------------------------------
// Typed gameplay requirement for a recipe.
//
// capability:
// - capability required somewhere in the stack
//
// minCount:
// - minimum number of cards with that capability
//
// maxCount:
// - maximum number of cards with that capability
// - 0 means no maximum
//
// ignoreMatchingCardsInIngredientCheck:
// - if true, cards that satisfy this capability can be ignored
//   during exact ingredient comparison
// ============================================================
[Serializable]
public class RecipeCapabilityRequirement
{
    [Header("Capability Requirement")]
    public CardCapabilityType capability = CardCapabilityType.None;

    [Min(1)]
    public int minCount = 1;

    [Min(0)]
    public int maxCount = 0;

    [Header("Matching Behavior")]
    public bool ignoreMatchingCardsInIngredientCheck = true;
}
