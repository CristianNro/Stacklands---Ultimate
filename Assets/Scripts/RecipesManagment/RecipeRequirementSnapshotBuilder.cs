using System;
using System.Collections.Generic;
using StacklandsLike.Cards;
using UnityEngine;

public static class RecipeRequirementSnapshotBuilder
{
    public static List<RecipeCapabilityRequirement> BuildCapabilityRequirementsSnapshot(RecipeData recipe)
    {
        List<RecipeCapabilityRequirement> result = new List<RecipeCapabilityRequirement>();

        if (recipe == null || recipe.capabilityRequirements == null || recipe.capabilityRequirements.Count == 0)
            return result;

        for (int i = 0; i < recipe.capabilityRequirements.Count; i++)
        {
            RecipeCapabilityRequirement requirement = recipe.capabilityRequirements[i];
            if (requirement == null || requirement.capability == CardCapabilityType.None)
                continue;

            result.Add(new RecipeCapabilityRequirement
            {
                capability = requirement.capability,
                minCount = Mathf.Max(1, requirement.minCount),
                maxCount = Mathf.Max(0, requirement.maxCount),
                ignoreMatchingCardsInIngredientCheck = requirement.ignoreMatchingCardsInIngredientCheck
            });
        }

        return result;
    }

    public static List<RecipeIngredientRule> BuildExactIngredientRequirementsSnapshot(RecipeData recipe)
    {
        List<RecipeIngredientRule> result = new List<RecipeIngredientRule>();
        Dictionary<string, RecipeIngredientRule> byId = new Dictionary<string, RecipeIngredientRule>();

        if (recipe == null)
            return result;

        if (recipe.ingredients != null && recipe.ingredients.Count > 0)
        {
            Dictionary<CardData, int> ingredientCounts = new Dictionary<CardData, int>();

            for (int i = 0; i < recipe.ingredients.Count; i++)
            {
                CardData ingredient = recipe.ingredients[i];
                if (ingredient == null || string.IsNullOrWhiteSpace(ingredient.id))
                    continue;

                if (ingredientCounts.ContainsKey(ingredient))
                    ingredientCounts[ingredient]++;
                else
                    ingredientCounts[ingredient] = 1;
            }

            foreach (KeyValuePair<CardData, int> pair in ingredientCounts)
            {
                byId[pair.Key.id] = new RecipeIngredientRule
                {
                    card = pair.Key,
                    requiredCount = Mathf.Max(1, pair.Value),
                    allowAdditionalCopies = false,
                    consumeMode = RecipeIngredientConsumeMode.None
                };
            }
        }

        if (recipe.ingredientRules != null && recipe.ingredientRules.Count > 0)
        {
            for (int i = 0; i < recipe.ingredientRules.Count; i++)
            {
                RecipeIngredientRule rule = recipe.ingredientRules[i];
                if (rule == null)
                    continue;

                string ingredientId = rule.GetIngredientCardId();
                if (string.IsNullOrWhiteSpace(ingredientId))
                    continue;

                if (byId.TryGetValue(ingredientId, out RecipeIngredientRule existing))
                {
                    existing.card = rule.card != null ? rule.card : existing.card;
                    existing.requiredCount = rule.GetNormalizedRequiredCount();
                    existing.allowAdditionalCopies = rule.allowAdditionalCopies;
                    existing.consumeMode = rule.consumeMode;
                    continue;
                }

                if (byId.Count == 0)
                {
                    byId[ingredientId] = new RecipeIngredientRule
                    {
                        card = rule.card,
                        requiredCount = rule.GetNormalizedRequiredCount(),
                        allowAdditionalCopies = rule.allowAdditionalCopies,
                        consumeMode = rule.consumeMode
                    };
                }
            }
        }

        foreach (KeyValuePair<string, RecipeIngredientRule> pair in byId)
            result.Add(pair.Value);

        return result;
    }
}
