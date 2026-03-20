using UnityEngine;
using StacklandsLike.Cards;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Stacklands/Recipe")]
public class RecipeData : ScriptableObject
{
    // Ingredientes necesarios para esta receta
    public List<CardData> ingredients;

    // Resultado de la receta
    public CardData result;

    [Header("Possible Results")]
    public List<RecipeResultOption> possibleResults = new List<RecipeResultOption>();

    // Tiempo que tarda en completarse la receta, en segundos
    public float craftTime = 1f;

    [Header("Tag Requirements")]
    public List<RecipeTagRequirement> tagRequirements = new List<RecipeTagRequirement>();

    [Header("Ingredient Consumption Rules")]
    public List<RecipeIngredientRule> ingredientRules = new List<RecipeIngredientRule>();

    // Verifica si una lista de cartas coincide con esta receta
    public bool Matches(List<CardData> cards)
    {
        // Si la cantidad no coincide, no puede ser esta receta
        if (cards.Count != ingredients.Count)
            return false;

        // Hacemos una copia para ir removiendo coincidencias
        List<CardData> remaining = new List<CardData>(cards);

        foreach (var ingredient in ingredients)
        {
            // Si falta un ingrediente, falla
            if (!remaining.Contains(ingredient))
                return false;

            // Lo removemos para evitar coincidencias duplicadas falsas
            remaining.Remove(ingredient);
        }

        return true;
    }
    

    // =========================================================
    // Helpers
    // =========================================================
    /// <summary>
    /// Devuelve la regla de consumo configurada para un cardId.
    /// Si no existe una regla explícita, devuelve null.
    /// </summary>
    public RecipeIngredientRule GetRuleForCardId(string cardId)
    {
        if (ingredientRules == null || string.IsNullOrWhiteSpace(cardId))
            return null;

        for (int i = 0; i < ingredientRules.Count; i++)
        {
            RecipeIngredientRule rule = ingredientRules[i];
            if (rule == null) continue;

            if (rule.cardId == cardId)
                return rule;
        }

        return null;
    }

    /// <summary>
    /// Devuelve true si la receta tiene múltiples resultados configurados.
    /// </summary>
    public bool HasMultipleResults()
    {
        return possibleResults != null && possibleResults.Count > 0;
    }

    /// <summary>
    /// Elige un resultado aleatorio ponderado.
    /// Si no hay resultados múltiples configurados, usa el result clásico.
    /// </summary>
    public CardData RollResult()
    {
        // --------------------------------------------------------
        // Caso 1: receta nueva con múltiples resultados
        // --------------------------------------------------------
        if (possibleResults != null && possibleResults.Count > 0)
        {
            float totalWeight = 0f;

            for (int i = 0; i < possibleResults.Count; i++)
            {
                RecipeResultOption option = possibleResults[i];
                if (option == null) continue;
                if (option.result == null) continue;
                if (option.weight <= 0f) continue;

                totalWeight += option.weight;
            }

            // Si por algún motivo todos los pesos son inválidos,
            // hacemos fallback al result clásico.
            if (totalWeight > 0f)
            {
                float roll = UnityEngine.Random.Range(0f, totalWeight);
                float accumulated = 0f;

                for (int i = 0; i < possibleResults.Count; i++)
                {
                    RecipeResultOption option = possibleResults[i];
                    if (option == null) continue;
                    if (option.result == null) continue;
                    if (option.weight <= 0f) continue;

                    accumulated += option.weight;

                    if (roll <= accumulated)
                        return option.result;
                }
            }
        }

        // --------------------------------------------------------
        // Caso 2: fallback a receta vieja
        // --------------------------------------------------------
        return result;
    }

    /// <summary>
    /// Devuelve el modo de consumo configurado para una carta.
    /// Si no existe una regla explícita, devuelve null para que el sistema
    /// pueda usar el fallback anterior.
    /// </summary>
    public RecipeIngredientConsumeMode? GetConsumeModeForCard(CardData cardData)
    {
        if (cardData == null) return null;

        RecipeIngredientRule rule = GetRuleForCardId(cardData.id);
        if (rule == null) return null;

        return rule.consumeMode;
    }

    /// <summary>
    /// Devuelve true si la receta tiene requisitos por tags configurados.
    /// </summary>
    public bool HasTagRequirements()
    {
        return tagRequirements != null && tagRequirements.Count > 0;
    }

    /// <summary>
    /// Devuelve true si una carta con estos tags debe ser ignorada
    /// durante el matching de ingredientes para esta receta.
    /// </summary>
    public bool ShouldIgnoreCardInIngredientMatch(CardData cardData)
    {
        if (cardData == null || cardData.tags == null || tagRequirements == null)
            return false;

        for (int i = 0; i < tagRequirements.Count; i++)
        {
            RecipeTagRequirement requirement = tagRequirements[i];
            if (requirement == null) continue;
            if (!requirement.ignoreMatchingCardsInIngredientCheck) continue;
            if (string.IsNullOrWhiteSpace(requirement.tag)) continue;

            if (cardData.tags.Contains(requirement.tag))
                return true;
        }

        return false;
    }

    // Override del método ToString() para debugging.
    // Cuando veas logs, verás algo como "Receta para Lodo: Agua, Tierra" en lugar de "RecipeData (clone)".
    public override string ToString()
    {
        return "Receta para " + result + ": " + string.Join(", ", ingredients);
    }
}