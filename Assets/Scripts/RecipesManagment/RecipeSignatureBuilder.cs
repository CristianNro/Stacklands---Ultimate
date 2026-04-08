using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class RecipeSignatureBuilder
{
    public static string BuildUniquenessSignature(RecipeData recipe)
    {
        if (recipe == null)
            return "recipe:<null>";

        StringBuilder signature = new StringBuilder();

        signature.Append("mode:");
        signature.Append(recipe.matchMode);
        signature.Append("|ingredients:");
        signature.Append(BuildIngredientsSignature(recipe));
        signature.Append("|capabilities:");
        signature.Append(BuildCapabilityRequirementsSignature(recipe));

        return signature.ToString();
    }

    public static string BuildCapabilityRequirementsSignature(RecipeData recipe)
    {
        if (recipe == null)
            return "<none>";

        List<RecipeCapabilityRequirement> effectiveRequirements = recipe.GetCapabilityRequirementsSnapshot();
        if (effectiveRequirements.Count == 0)
            return "<none>";

        List<string> parts = new List<string>();

        for (int i = 0; i < effectiveRequirements.Count; i++)
        {
            RecipeCapabilityRequirement requirement = effectiveRequirements[i];
            if (requirement == null || requirement.capability == StacklandsLike.Cards.CardCapabilityType.None)
                continue;

            int minCount = Mathf.Max(1, requirement.minCount);
            int maxCount = Mathf.Max(0, requirement.maxCount);
            string part = requirement.capability + ":" + minCount + ":" + maxCount + ":" + (requirement.ignoreMatchingCardsInIngredientCheck ? "1" : "0");
            parts.Add(part);
        }

        if (parts.Count == 0)
            return "<none>";

        parts.Sort();
        return string.Join(",", parts);
    }

    private static string BuildIngredientsSignature(RecipeData recipe)
    {
        if (recipe == null)
            return "<none>";

        List<RecipeIngredientRule> exactRequirements = recipe.GetExactIngredientRequirementsSnapshot();
        if (exactRequirements.Count == 0)
            return "<none>";

        List<string> ingredientParts = new List<string>();

        for (int i = 0; i < exactRequirements.Count; i++)
        {
            RecipeIngredientRule requirement = exactRequirements[i];
            if (requirement == null)
                continue;

            string ingredientId = requirement.GetIngredientCardId();
            if (string.IsNullOrWhiteSpace(ingredientId))
                continue;

            ingredientParts.Add(
                ingredientId + ":" +
                requirement.GetNormalizedRequiredCount() + ":" +
                (requirement.allowAdditionalCopies ? "1" : "0"));
        }

        if (ingredientParts.Count == 0)
            return "<none>";

        ingredientParts.Sort();
        return string.Join(",", ingredientParts);
    }
}
