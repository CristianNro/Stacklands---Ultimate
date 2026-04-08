using System;
using System.Collections.Generic;
using StacklandsLike.Cards;

public static class RecipeMatcher
{
    public static RecipeMatchResult Evaluate(RecipeData recipe, RecipeMatchInput input)
    {
        CardStack stack = input != null ? input.sourceStack : null;

        if (recipe == null)
            return RecipeMatchResult.NotMatched(null, stack, "Recipe is null.");

        if (input == null)
            return RecipeMatchResult.NotMatched(recipe, stack, "Recipe match input is null.");

        switch (recipe.matchMode)
        {
            case RecipeMatchMode.CapabilityRequirementsOnly:
                return EvaluateCapabilityRequirementMatch(recipe, input);

            case RecipeMatchMode.ExactIngredients:
            default:
                return EvaluateExactIngredientMatch(recipe, input);
        }
    }

    private static RecipeMatchResult EvaluateExactIngredientMatch(RecipeData recipe, RecipeMatchInput input)
    {
        CardStack stack = input != null ? input.sourceStack : null;

        if (input == null)
            return RecipeMatchResult.NotMatched(recipe, stack, "Recipe match input is null.");

        List<CardData> stackData = input.GetCardDataList();
        if (stackData == null)
            return RecipeMatchResult.NotMatched(recipe, stack, "Stack data is null.");

        List<CardData> filteredStackData = new List<CardData>();

        for (int i = 0; i < stackData.Count; i++)
        {
            CardData card = stackData[i];
            if (card == null)
                continue;

            if (recipe.ShouldIgnoreCardInIngredientMatch(card))
                continue;

            filteredStackData.Add(card);
        }

        List<RecipeIngredientRule> exactRequirements = recipe.GetExactIngredientRequirementsSnapshot();
        if (exactRequirements.Count == 0)
            return RecipeMatchResult.NotMatched(recipe, stack, "Recipe has no valid exact ingredient requirements.");

        Dictionary<string, int> actualCounts = new Dictionary<string, int>();
        List<string> stackIds = new List<string>();

        for (int i = 0; i < filteredStackData.Count; i++)
        {
            CardData card = filteredStackData[i];
            if (card == null || string.IsNullOrWhiteSpace(card.id))
                continue;

            stackIds.Add(card.id);

            if (actualCounts.ContainsKey(card.id))
                actualCounts[card.id]++;
            else
                actualCounts[card.id] = 1;
        }

        Dictionary<string, int> expectedCounts = new Dictionary<string, int>();
        HashSet<string> expandableIngredientIds = new HashSet<string>();
        int expectedMinimumCardCount = 0;

        for (int i = 0; i < exactRequirements.Count; i++)
        {
            RecipeIngredientRule requirement = exactRequirements[i];
            if (requirement == null)
                continue;

            string ingredientId = requirement.GetIngredientCardId();
            if (string.IsNullOrWhiteSpace(ingredientId))
                continue;

            int requiredCount = requirement.GetNormalizedRequiredCount();
            expectedCounts[ingredientId] = requiredCount;
            expectedMinimumCardCount += requiredCount;

            if (requirement.allowAdditionalCopies)
                expandableIngredientIds.Add(ingredientId);
        }

        if (filteredStackData.Count < expectedMinimumCardCount)
        {
            return RecipeMatchResult.NotMatched(
                recipe,
                stack,
                $"Exact ingredient count mismatch. Expected at least {expectedMinimumCardCount}, got {filteredStackData.Count}.");
        }

        foreach (KeyValuePair<string, int> pair in expectedCounts)
        {
            int actualCount = actualCounts.TryGetValue(pair.Key, out int foundCount) ? foundCount : 0;
            if (actualCount < pair.Value)
            {
                return RecipeMatchResult.NotMatched(
                    recipe,
                    stack,
                    $"Missing exact ingredient '{pair.Key}'. Expected at least {pair.Value}, got {actualCount}.");
            }
        }

        foreach (KeyValuePair<string, int> pair in actualCounts)
        {
            if (!expectedCounts.TryGetValue(pair.Key, out int expectedCount))
            {
                return RecipeMatchResult.NotMatched(
                    recipe,
                    stack,
                    $"Unexpected exact ingredient '{pair.Key}' in stack.");
            }

            if (pair.Value > expectedCount && !expandableIngredientIds.Contains(pair.Key))
            {
                return RecipeMatchResult.NotMatched(
                    recipe,
                    stack,
                    $"Ingredient '{pair.Key}' exceeded exact requirement. Expected {expectedCount}, got {pair.Value}.");
            }
        }

        stackIds.Sort();

        string missingCapabilityReason;
        if (!recipe.TryValidateCapabilityRequirements(input, out missingCapabilityReason))
            return RecipeMatchResult.NotMatched(recipe, stack, missingCapabilityReason);

        return RecipeMatchResult.Matched(
            recipe,
            stack,
            BuildExactMatchSignature(recipe, stackIds),
            "Exact ingredients and capability requirements satisfied.");
    }

    private static RecipeMatchResult EvaluateCapabilityRequirementMatch(RecipeData recipe, RecipeMatchInput input)
    {
        CardStack stack = input != null ? input.sourceStack : null;

        if (input == null)
            return RecipeMatchResult.NotMatched(recipe, stack, "Recipe match input is null.");

        List<CardData> cards = input.GetCardDataList();
        if (cards == null || cards.Count == 0)
            return RecipeMatchResult.NotMatched(recipe, stack, "Stack has no cards.");

        for (int i = 0; i < cards.Count; i++)
        {
            if (!recipe.IsCardAllowedByCapabilities(cards[i]))
            {
                string rejectedId = cards[i] != null ? cards[i].id : "<null>";
                return RecipeMatchResult.NotMatched(
                    recipe,
                    stack,
                    $"Card '{rejectedId}' is not allowed by capability-driven recipe constraints.");
            }
        }

        string missingCapabilityReason;
        if (!recipe.TryValidateCapabilityRequirements(input, out missingCapabilityReason))
            return RecipeMatchResult.NotMatched(recipe, stack, missingCapabilityReason);

        return RecipeMatchResult.Matched(
            recipe,
            stack,
            BuildCapabilityMatchSignature(recipe, cards),
            "All cards are allowed by capabilities and capability requirements are satisfied.");
    }

    private static string BuildExactMatchSignature(RecipeData recipe, List<string> sortedStackIds)
    {
        string ingredientsSignature = sortedStackIds != null && sortedStackIds.Count > 0
            ? string.Join(",", sortedStackIds)
            : "<none>";

        return $"mode:ExactIngredients|ingredients:{ingredientsSignature}|capabilities:{recipe.BuildCapabilityRequirementsSignatureForDatabase()}";
    }

    private static string BuildCapabilityMatchSignature(RecipeData recipe, List<CardData> cards)
    {
        List<string> cardIds = new List<string>();

        if (cards != null)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                CardData card = cards[i];
                if (card == null || string.IsNullOrWhiteSpace(card.id))
                    continue;

                cardIds.Add(card.id);
            }
        }

        cardIds.Sort();

        string cardSignature = cardIds.Count > 0
            ? string.Join(",", cardIds)
            : "<none>";

        return $"mode:CapabilityRequirementsOnly|cards:{cardSignature}|capabilities:{recipe.BuildCapabilityRequirementsSignatureForDatabase()}";
    }
}
