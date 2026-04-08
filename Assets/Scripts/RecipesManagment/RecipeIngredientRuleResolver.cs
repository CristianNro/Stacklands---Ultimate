using StacklandsLike.Cards;

public static class RecipeIngredientRuleResolver
{
    public static RecipeIngredientConsumeMode? GetConsumeModeForCard(RecipeData recipe, CardData cardData)
    {
        if (cardData == null)
            return null;

        RecipeIngredientRule rule = GetRuleForCard(recipe, cardData);
        if (rule == null)
            return null;

        return rule.consumeMode;
    }

    public static RecipeIngredientRule GetRuleForCard(RecipeData recipe, CardData cardData)
    {
        if (recipe == null || recipe.ingredientRules == null || cardData == null)
            return null;

        for (int i = 0; i < recipe.ingredientRules.Count; i++)
        {
            RecipeIngredientRule rule = recipe.ingredientRules[i];
            if (rule == null)
                continue;

            if (rule.Matches(cardData))
                return rule;
        }

        return null;
    }
}
