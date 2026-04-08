using StacklandsLike.Cards;
using UnityEngine;

public static class RecipeTimingResolver
{
    public static float GetBaseCraftTime(RecipeData recipe)
    {
        if (recipe == null)
            return 0.01f;

        return Mathf.Max(0.01f, recipe.craftTime);
    }

    public static float GetCraftTime(RecipeData recipe, RecipeMatchInput input)
    {
        float resolvedTime = GetBaseCraftTime(recipe);

        if (recipe == null || input == null || recipe.durationCapabilityModifiers == null || recipe.durationCapabilityModifiers.Count == 0)
            return resolvedTime;

        for (int i = 0; i < recipe.durationCapabilityModifiers.Count; i++)
        {
            RecipeDurationCapabilityModifier modifier = recipe.durationCapabilityModifiers[i];
            if (modifier == null || modifier.capability == CardCapabilityType.None)
                continue;

            int applicationCount = Mathf.Min(
                input.CountCardsWithCapability(modifier.capability),
                Mathf.Max(1, modifier.maxApplications));

            if (applicationCount <= 0)
                continue;

            float multiplier = Mathf.Max(0.01f, modifier.multiplier);
            for (int j = 0; j < applicationCount; j++)
                resolvedTime *= multiplier;
        }

        return Mathf.Max(0.01f, resolvedTime);
    }
}
