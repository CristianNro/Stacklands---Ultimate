using System;
using UnityEngine;

// ============================================================
// RecipeResultOption
// ------------------------------------------------------------
// Representa un posible resultado de una receta con un peso
// relativo para el sorteo.
//
// Ejemplo:
// - Wood, weight 70
// - Rare Wood, weight 20
// - Seed, weight 10
//
// No hace falta que sume 100.
// ============================================================
[Serializable]
public class RecipeResultOption
{
    [Header("Result")]
    public CardData result;

    [Min(0f)]
    public float weight = 1f;
}