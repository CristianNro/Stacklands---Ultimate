using System;
using UnityEngine;

// ============================================================
// RecipeTagRequirement
// ------------------------------------------------------------
// Requisito genérico por tag para una receta.
//
// tag:
// - tag requerido, por ejemplo "villager", "tool", "fire-source"
//
// minCount:
// - cuántas cartas con ese tag se necesitan como mínimo
//
// ignoreMatchingCardsInIngredientCheck:
// - si es true, las cartas que tengan este tag se ignoran
//   cuando se comparan los ingredientes de la receta
//
// Ejemplo:
// receta: Tree -> Wood
// requisito: villager (ignore=true)
// stack: Villager + Tree
// para ingredientes se ignora Villager y queda solo Tree
// ============================================================
[Serializable]
public class RecipeTagRequirement
{
    [Header("Tag Requirement")]
    public string tag;

    [Min(1)]
    public int minCount = 1;

    [Header("Matching Behavior")]
    public bool ignoreMatchingCardsInIngredientCheck = true;
}