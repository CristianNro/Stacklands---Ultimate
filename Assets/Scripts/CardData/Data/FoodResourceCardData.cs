using UnityEngine;
using StacklandsLike.Cards;

// ============================================================
// FoodResourceCardData
// ------------------------------------------------------------
// Subtipo explicito para recursos comestibles.
//
// Sigue siendo un `ResourceCardData`, pero agrega el primer
// contrato propio de comida que hoy necesita el ciclo diario:
// cuanto alimento total aporta esta carta al consumirse.
// ============================================================
[CreateAssetMenu(fileName = "FoodResourceCard", menuName = "Cards/Food Resource Card")]
public class FoodResourceCardData : ResourceCardData
{
    [Header("Food")]
    [Min(1)] public int foodValue = 1;
    [Min(0f)] public float spoilAfterSeconds = 0f;

    private void Reset()
    {
        cardType = CardType.Food;
        resourceType = ResourceType.Food;
    }
}
