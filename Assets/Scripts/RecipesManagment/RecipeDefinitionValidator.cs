using System.Collections.Generic;
using StacklandsLike.Cards;
using UnityEngine;

public static class RecipeDefinitionValidator
{
    public static bool Validate(RecipeData recipe, out string validationError)
    {
        if (recipe == null)
        {
            validationError = "Recipe is null.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(recipe.id))
        {
            validationError = "Recipe id is empty.";
            return false;
        }

        if (recipe.GetCraftTime() <= 0f)
        {
            validationError = "Craft time must be greater than 0.";
            return false;
        }

        if (!HasAnyValidResult(recipe))
        {
            validationError = "Recipe has no valid result (result or weighted options).";
            return false;
        }

        int validIngredientCount = recipe.GetExactIngredientRequirementsSnapshot().Count;
        int validCapabilityRequirementCount = recipe.GetCapabilityRequirementsSnapshot().Count;

        if (recipe.matchMode == RecipeMatchMode.ExactIngredients && validIngredientCount <= 0)
        {
            validationError = "ExactIngredients mode requires at least one valid ingredient.";
            return false;
        }

        if (recipe.matchMode == RecipeMatchMode.CapabilityRequirementsOnly && validCapabilityRequirementCount <= 0)
        {
            validationError = "CapabilityRequirementsOnly mode requires at least one valid capability requirement.";
            return false;
        }

        if (!ValidateIngredientRules(recipe, out validationError))
            return false;

        if (!ValidateCapabilityRequirements(recipe, out validationError))
            return false;

        if (!ValidateDurationModifiers(recipe, out validationError))
            return false;

        validationError = null;
        return true;
    }

    private static bool HasAnyValidResult(RecipeData recipe)
    {
        if (recipe.result != null)
            return true;

        if (recipe.possibleResults == null || recipe.possibleResults.Count == 0)
            return false;

        for (int i = 0; i < recipe.possibleResults.Count; i++)
        {
            RecipeResultOption option = recipe.possibleResults[i];
            if (option == null || option.result == null || option.weight <= 0f)
                continue;

            return true;
        }

        return false;
    }

    private static bool ValidateIngredientRules(RecipeData recipe, out string validationError)
    {
        if (recipe.ingredientRules == null || recipe.ingredientRules.Count == 0)
        {
            validationError = null;
            return true;
        }

        HashSet<string> seenExactIngredientIds = new HashSet<string>();
        HashSet<string> validExactIngredientIds = BuildValidExactIngredientIdSet(recipe);
        int expandableIngredientCount = 0;

        for (int i = 0; i < recipe.ingredientRules.Count; i++)
        {
            RecipeIngredientRule rule = recipe.ingredientRules[i];
            if (rule == null)
                continue;

            string effectiveCardId = rule.GetIngredientCardId();
            if (string.IsNullOrWhiteSpace(effectiveCardId))
            {
                validationError = $"Ingredient rule at index {i} has no CardData reference or valid CardData.id.";
                return false;
            }

            if (rule.requiredCount <= 0)
            {
                validationError = $"Ingredient rule '{effectiveCardId}' must have requiredCount greater than 0.";
                return false;
            }

            if (recipe.matchMode == RecipeMatchMode.ExactIngredients && !validExactIngredientIds.Contains(effectiveCardId))
            {
                validationError = $"Ingredient rule '{effectiveCardId}' does not belong to this exact-ingredient recipe.";
                return false;
            }

            if (!seenExactIngredientIds.Add(effectiveCardId))
            {
                validationError = $"Duplicate exact ingredient rule '{effectiveCardId}' found in recipe.";
                return false;
            }

            if (rule.allowAdditionalCopies)
                expandableIngredientCount++;
        }

        if (recipe.matchMode == RecipeMatchMode.ExactIngredients && expandableIngredientCount > 1)
        {
            validationError = "ExactIngredients recipes currently support only one ingredient with allowAdditionalCopies enabled.";
            return false;
        }

        validationError = null;
        return true;
    }

    private static HashSet<string> BuildValidExactIngredientIdSet(RecipeData recipe)
    {
        HashSet<string> result = new HashSet<string>();
        List<RecipeIngredientRule> exactRequirements = recipe.GetExactIngredientRequirementsSnapshot();

        for (int i = 0; i < exactRequirements.Count; i++)
        {
            RecipeIngredientRule requirement = exactRequirements[i];
            if (requirement == null)
                continue;

            string ingredientId = requirement.GetIngredientCardId();
            if (string.IsNullOrWhiteSpace(ingredientId))
                continue;

            result.Add(ingredientId);
        }

        return result;
    }

    private static bool ValidateCapabilityRequirements(RecipeData recipe, out string validationError)
    {
        if (recipe.capabilityRequirements == null || recipe.capabilityRequirements.Count == 0)
        {
            validationError = null;
            return true;
        }

        for (int i = 0; i < recipe.capabilityRequirements.Count; i++)
        {
            RecipeCapabilityRequirement requirement = recipe.capabilityRequirements[i];
            if (requirement == null || requirement.capability == CardCapabilityType.None)
                continue;

            if (requirement.minCount <= 0)
            {
                validationError = $"Capability requirement '{requirement.capability}' must have minCount greater than 0.";
                return false;
            }

            if (requirement.maxCount > 0 && requirement.maxCount < requirement.minCount)
            {
                validationError = $"Capability requirement '{requirement.capability}' must have maxCount greater than or equal to minCount, or 0 for unlimited.";
                return false;
            }
        }

        validationError = null;
        return true;
    }

    private static bool ValidateDurationModifiers(RecipeData recipe, out string validationError)
    {
        if (recipe.durationCapabilityModifiers == null || recipe.durationCapabilityModifiers.Count == 0)
        {
            validationError = null;
            return true;
        }

        for (int i = 0; i < recipe.durationCapabilityModifiers.Count; i++)
        {
            RecipeDurationCapabilityModifier modifier = recipe.durationCapabilityModifiers[i];
            if (modifier == null)
                continue;

            if (modifier.capability == CardCapabilityType.None)
            {
                validationError = $"Duration modifier at index {i} has no capability.";
                return false;
            }

            if (modifier.multiplier <= 0f)
            {
                validationError = $"Duration modifier '{modifier.capability}' must have multiplier greater than 0.";
                return false;
            }

            if (modifier.maxApplications <= 0)
            {
                validationError = $"Duration modifier '{modifier.capability}' must have maxApplications greater than 0.";
                return false;
            }
        }

        validationError = null;
        return true;
    }
}
