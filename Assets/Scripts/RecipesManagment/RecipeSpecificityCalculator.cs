using System.Collections.Generic;
using StacklandsLike.Cards;

public static class RecipeSpecificityCalculator
{
    public static int GetSpecificityScore(RecipeData recipe)
    {
        if (recipe == null)
            return 0;

        int score = 0;

        if (recipe.matchMode == RecipeMatchMode.ExactIngredients)
            score += 10000;

        score += GetMinimumExactIngredientCount(recipe) * 100;
        score += RecipeCapabilityEvaluator.CountValidCapabilityRequirements(recipe) * 10;

        if (recipe.IsRepeatable())
            score += 1;

        return score;
    }

    private static int GetMinimumExactIngredientCount(RecipeData recipe)
    {
        if (recipe == null)
            return 0;

        List<RecipeIngredientRule> exactRequirements = recipe.GetExactIngredientRequirementsSnapshot();
        int count = 0;

        for (int i = 0; i < exactRequirements.Count; i++)
        {
            RecipeIngredientRule requirement = exactRequirements[i];
            if (requirement == null)
                continue;

            count += requirement.GetNormalizedRequiredCount();
        }

        return count;
    }
}
