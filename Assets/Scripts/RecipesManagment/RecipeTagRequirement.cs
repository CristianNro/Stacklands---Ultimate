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
    // Tag que la receta espera encontrar en el stack.
    public string tag;

    [Min(1)]
    // Cantidad minima de cartas con este tag para que la receta sea valida.
    public int minCount = 1;

    [Header("Matching Behavior")]
    // Si esta activo, estas cartas no cuentan como ingredientes exactos al comparar.
    public bool ignoreMatchingCardsInIngredientCheck = true;
}
