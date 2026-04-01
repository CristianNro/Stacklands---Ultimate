using System.Collections.Generic;
using UnityEngine;
using StacklandsLike.Cards;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Stacklands/Recipe")]
public class RecipeData : ScriptableObject
{
    [Header("Identity")]
    // Identificador estable de la receta para referencias internas.
    public string id;
    // Nombre legible para mostrar en inspector o UI.
    public string displayName;

    [Header("Behavior")]
    // Define si la receta matchea por ingredientes exactos o solo por tags.
    public RecipeMatchMode matchMode = RecipeMatchMode.ExactIngredients;
    // Define si corre una vez o si repite ciclos mientras el stack siga siendo valido.
    public RecipeExecutionMode executionMode = RecipeExecutionMode.Single;

    [Header("Ingredients")]
    // Lista exacta de cartas requeridas cuando la receta usa ExactIngredients.
    public List<CardData> ingredients = new List<CardData>();

    [Header("Tag Requirements")]
    // Requisitos por tag que el stack debe cumplir para activar la receta.
    public List<RecipeTagRequirement> tagRequirements = new List<RecipeTagRequirement>();

    [Header("Results")]
    // Resultado clasico de la receta cuando no se usan resultados ponderados.
    public CardData result;
    // Pool de resultados posibles con peso relativo para sorteos.
    public List<RecipeResultOption> possibleResults = new List<RecipeResultOption>();

    [Header("Timing")]
    // Tiempo base de cada ejecucion o de cada ciclo repetible.
    public float craftTime = 1f;

    [Header("Ingredient Consumption Rules")]
    // Reglas explicitas de consumo para cartas concretas al completar la receta.
    public List<RecipeIngredientRule> ingredientRules = new List<RecipeIngredientRule>();

    /// <summary>
    /// Punto central de matching.
    /// La receta decide sola como validar el stack segun su modo.
    /// </summary>
    public virtual bool MatchesStack(CardStack stack)
    {
        if (stack == null)
            return false;

        switch (matchMode)
        {
            case RecipeMatchMode.TagRequirementsOnly:
                return MatchesByTagRequirements(stack);

            case RecipeMatchMode.ExactIngredients:
            default:
                return MatchesByExactIngredients(stack);
        }
    }

    public bool IsRepeatable()
    {
        return executionMode == RecipeExecutionMode.RepeatWhileValid;
    }

    public virtual float GetCraftTime()
    {
        return Mathf.Max(0.01f, craftTime);
    }

    /// <summary>
    /// En recetas guiadas por tags, todo el stack debe pertenecer
    /// a algun tag declarado en la receta.
    /// </summary>
    public bool IsCardAllowedByTags(CardData cardData)
    {
        if (cardData == null || cardData.tags == null || tagRequirements == null || tagRequirements.Count == 0)
            return false;

        for (int i = 0; i < tagRequirements.Count; i++)
        {
            RecipeTagRequirement requirement = tagRequirements[i];
            if (requirement == null) continue;
            if (string.IsNullOrWhiteSpace(requirement.tag)) continue;

            if (cardData.tags.Contains(requirement.tag))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Verifica solo las cantidades minimas por tag.
    /// Se usa como base tanto para recetas exactas como tag-driven.
    /// </summary>
    public bool ValidateTagRequirements(CardStack stack)
    {
        if (stack == null)
            return false;

        if (tagRequirements == null || tagRequirements.Count == 0)
            return true;

        for (int i = 0; i < tagRequirements.Count; i++)
        {
            RecipeTagRequirement requirement = tagRequirements[i];
            if (requirement == null) continue;
            if (string.IsNullOrWhiteSpace(requirement.tag)) continue;

            int countInStack = stack.CountCardsWithTag(requirement.tag);
            if (countInStack < requirement.minCount)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Score simple de especificidad para desempatar recetas.
    /// Exactas ganan sobre las guiadas solo por tags.
    /// Luego se prioriza la que tenga mas requisitos concretos.
    /// </summary>
    public int GetSpecificityScore()
    {
        int score = 0;

        if (matchMode == RecipeMatchMode.ExactIngredients)
            score += 10000;

        score += ingredients != null ? ingredients.Count * 100 : 0;
        score += tagRequirements != null ? tagRequirements.Count * 10 : 0;

        if (IsRepeatable())
            score += 1;

        return score;
    }

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

    public bool HasMultipleResults()
    {
        return possibleResults != null && possibleResults.Count > 0;
    }

    public CardData RollResult()
    {
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

        return result;
    }

    public RecipeIngredientConsumeMode? GetConsumeModeForCard(CardData cardData)
    {
        if (cardData == null) return null;

        RecipeIngredientRule rule = GetRuleForCardId(cardData.id);
        if (rule == null) return null;

        return rule.consumeMode;
    }

    public bool HasTagRequirements()
    {
        return tagRequirements != null && tagRequirements.Count > 0;
    }

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

    private bool MatchesByExactIngredients(CardStack stack)
    {
        if (stack == null)
            return false;

        List<CardData> stackData = stack.GetCardDataList();
        if (stackData == null)
            return false;

        List<CardData> filteredStackData = new List<CardData>();

        for (int i = 0; i < stackData.Count; i++)
        {
            CardData card = stackData[i];
            if (card == null) continue;

            if (ShouldIgnoreCardInIngredientMatch(card))
                continue;

            filteredStackData.Add(card);
        }

        if (filteredStackData.Count != ingredients.Count)
            return false;

        List<string> stackIds = new List<string>();
        List<string> recipeIds = new List<string>();

        for (int i = 0; i < filteredStackData.Count; i++)
        {
            if (filteredStackData[i] != null)
                stackIds.Add(filteredStackData[i].id);
        }

        for (int i = 0; i < ingredients.Count; i++)
        {
            if (ingredients[i] != null)
                recipeIds.Add(ingredients[i].id);
        }

        stackIds.Sort();
        recipeIds.Sort();

        if (stackIds.Count != recipeIds.Count)
            return false;

        for (int i = 0; i < stackIds.Count; i++)
        {
            if (stackIds[i] != recipeIds[i])
                return false;
        }

        return ValidateTagRequirements(stack);
    }

    private bool MatchesByTagRequirements(CardStack stack)
    {
        if (stack == null)
            return false;

        List<CardData> cards = stack.GetCardDataList();
        if (cards == null || cards.Count == 0)
            return false;

        for (int i = 0; i < cards.Count; i++)
        {
            if (!IsCardAllowedByTags(cards[i]))
                return false;
        }

        if (!ValidateTagRequirements(stack))
            return false;

        return true;
    }

    public override string ToString()
    {
        return "Receta para " + result + ": " + string.Join(", ", ingredients);
    }
}
