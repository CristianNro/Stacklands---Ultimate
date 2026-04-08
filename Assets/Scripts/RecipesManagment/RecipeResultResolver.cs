using UnityEngine;

public static class RecipeResultResolver
{
    public static bool HasMultipleResults(RecipeData recipe)
    {
        return recipe != null && recipe.possibleResults != null && recipe.possibleResults.Count > 0;
    }

    public static CardData RollResult(RecipeData recipe)
    {
        if (recipe == null)
            return null;

        if (recipe.possibleResults != null && recipe.possibleResults.Count > 0)
        {
            float totalWeight = 0f;

            for (int i = 0; i < recipe.possibleResults.Count; i++)
            {
                RecipeResultOption option = recipe.possibleResults[i];
                if (option == null || option.result == null || option.weight <= 0f)
                    continue;

                totalWeight += option.weight;
            }

            if (totalWeight > 0f)
            {
                float roll = Random.Range(0f, totalWeight);
                float accumulated = 0f;

                for (int i = 0; i < recipe.possibleResults.Count; i++)
                {
                    RecipeResultOption option = recipe.possibleResults[i];
                    if (option == null || option.result == null || option.weight <= 0f)
                        continue;

                    accumulated += option.weight;

                    if (roll <= accumulated)
                        return option.result;
                }
            }
        }

        return recipe.result;
    }
}
