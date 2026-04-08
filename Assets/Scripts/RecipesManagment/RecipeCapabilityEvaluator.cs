using System.Collections.Generic;
using StacklandsLike.Cards;
using UnityEngine;

public static class RecipeCapabilityEvaluator
{
    public static bool IsCardAllowedByCapabilities(RecipeData recipe, CardData cardData)
    {
        if (recipe == null || cardData == null)
            return false;

        List<RecipeCapabilityRequirement> effectiveRequirements = recipe.GetCapabilityRequirementsSnapshot();
        if (effectiveRequirements.Count == 0)
            return false;

        for (int i = 0; i < effectiveRequirements.Count; i++)
        {
            RecipeCapabilityRequirement requirement = effectiveRequirements[i];
            if (requirement == null)
                continue;

            if (CardSupportsCapability(cardData, requirement.capability))
                return true;
        }

        return false;
    }

    public static bool ValidateCapabilityRequirements(RecipeData recipe, RecipeMatchInput input)
    {
        string failureReason;
        return TryValidateCapabilityRequirements(recipe, input, out failureReason);
    }

    public static bool ShouldIgnoreCardInIngredientMatch(RecipeData recipe, CardData cardData)
    {
        if (recipe == null || cardData == null)
            return false;

        List<RecipeCapabilityRequirement> effectiveRequirements = recipe.GetCapabilityRequirementsSnapshot();
        for (int i = 0; i < effectiveRequirements.Count; i++)
        {
            RecipeCapabilityRequirement requirement = effectiveRequirements[i];
            if (requirement == null)
                continue;

            if (!requirement.ignoreMatchingCardsInIngredientCheck)
                continue;

            if (CardSupportsCapability(cardData, requirement.capability))
                return true;
        }

        return false;
    }

    public static int CountValidCapabilityRequirements(RecipeData recipe)
    {
        if (recipe == null || recipe.capabilityRequirements == null || recipe.capabilityRequirements.Count == 0)
            return 0;

        int count = 0;

        for (int i = 0; i < recipe.capabilityRequirements.Count; i++)
        {
            RecipeCapabilityRequirement requirement = recipe.capabilityRequirements[i];
            if (!IsValidCapabilityRequirement(requirement))
                continue;

            count++;
        }

        return count;
    }

    public static bool TryValidateCapabilityRequirements(RecipeData recipe, RecipeMatchInput input, out string failureReason)
    {
        failureReason = null;

        if (recipe == null)
        {
            failureReason = "Recipe is null.";
            return false;
        }

        if (input == null)
        {
            failureReason = "Recipe match input is null.";
            return false;
        }

        List<RecipeCapabilityRequirement> effectiveRequirements = recipe.GetCapabilityRequirementsSnapshot();
        if (effectiveRequirements.Count == 0)
            return true;

        for (int i = 0; i < effectiveRequirements.Count; i++)
        {
            RecipeCapabilityRequirement requirement = effectiveRequirements[i];
            if (!IsValidCapabilityRequirement(requirement))
                continue;

            int minCount = Mathf.Max(1, requirement.minCount);
            int maxCount = Mathf.Max(0, requirement.maxCount);
            int countInStack = input.CountCardsWithCapability(requirement.capability);
            if (countInStack < minCount)
            {
                failureReason = $"Missing capability requirement '{requirement.capability}'. Expected {minCount}, got {countInStack}.";
                return false;
            }

            if (maxCount > 0 && countInStack > maxCount)
            {
                failureReason = $"Capability requirement '{requirement.capability}' exceeded maximum. Expected at most {maxCount}, got {countInStack}.";
                return false;
            }
        }

        return true;
    }

    private static bool IsValidCapabilityRequirement(RecipeCapabilityRequirement requirement)
    {
        return requirement != null && requirement.capability != CardCapabilityType.None;
    }

    private static bool CardSupportsCapability(CardData cardData, CardCapabilityType capability)
    {
        if (cardData == null || capability == CardCapabilityType.None)
            return false;

        return cardData.capabilities != null && cardData.capabilities.Contains(capability);
    }
}
