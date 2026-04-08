using System.Collections.Generic;
using UnityEngine;
using StacklandsLike.Cards;

// ============================================================
// StackCraftingExecutor
// ------------------------------------------------------------
// Ejecuta la resolucion de recetas sobre un stack existente.
// Mantiene fuera de CardStack la logica de consumo, seleccion de
// cartas por receta y spawn del resultado.
// ============================================================
public static class StackCraftingExecutor
{
    public static void CompleteRecipe(CardStack stack, RecipeData recipe)
    {
        if (stack == null)
            return;

        if (recipe == null)
        {
            stack.StopCraftingVisuals();
            return;
        }

        CardData rolledResult = recipe.RollResult();

        if (rolledResult == null)
        {
            stack.StopCraftingVisuals();
            Debug.LogWarning($"[{stack.name}] La receta no devolvio ningun resultado valido.");
            return;
        }

        Debug.Log($"[{stack.name}] Receta completada. Resultado sorteado: {rolledResult.cardName}");

        Vector2 spawnPos = stack.GetStackPosition();
        ApplyRecipeConsumption(stack, recipe);

        if (CardSpawner.Instance != null)
            CardSpawner.Instance.SpawnAnimated(rolledResult, spawnPos);

        stack.StopCraftingVisuals();
        stack.FinalizeCraftingMutation();
    }

    public static void ExecuteRepeatableCycle(CardStack stack, RecipeData recipe)
    {
        if (stack == null || recipe == null)
            return;

        CardData rolledResult = recipe.RollResult();
        if (rolledResult != null && CardSpawner.Instance != null)
            CardSpawner.Instance.SpawnAnimated(rolledResult, stack.GetStackPosition());

        ApplyRecipeConsumption(stack, recipe);
    }

    public static void ApplyRecipeConsumption(CardStack stack, RecipeData recipe)
    {
        if (stack == null || recipe == null)
            return;

        List<CardView> cardsToProcess = GetCardsToConsumeForRecipe(stack, recipe);

        for (int i = 0; i < cardsToProcess.Count; i++)
        {
            CardView card = cardsToProcess[i];
            if (card == null)
                continue;

            CardInstance instance = card.Instance;
            if (instance == null || instance.data == null)
                continue;

            RecipeIngredientConsumeMode? explicitMode = recipe.GetConsumeModeForCard(instance.data);

            if (explicitMode.HasValue)
            {
                ApplyConsumeModeToCard(stack, card, instance, explicitMode.Value);
                continue;
            }

            if (instance.ConsumeUseIfNeeded())
                stack.DestroyCardForSystem(card);
        }
    }

    private static List<CardView> GetCardsToConsumeForRecipe(CardStack stack, RecipeData recipe)
    {
        if (stack == null || recipe == null)
            return new List<CardView>();

        List<CardView> selectedCards = new List<CardView>();
        HashSet<CardView> alreadySelected = new HashSet<CardView>();
        if (recipe.matchMode == RecipeMatchMode.ExactIngredients)
            SelectExactIngredientCards(stack, recipe, selectedCards, alreadySelected);

        SelectCapabilityRequirementCards(stack, recipe, selectedCards, alreadySelected);

        return selectedCards;
    }

    private static void SelectCapabilityRequirementCards(CardStack stack, RecipeData recipe, List<CardView> selectedCards, HashSet<CardView> alreadySelected)
    {
        if (stack == null || recipe == null || selectedCards == null)
            return;

        List<RecipeCapabilityRequirement> requirements = recipe.GetCapabilityRequirementsSnapshot();
        if (requirements == null || requirements.Count == 0)
            return;

        for (int requirementIndex = 0; requirementIndex < requirements.Count; requirementIndex++)
        {
            RecipeCapabilityRequirement requirement = requirements[requirementIndex];
            if (requirement == null || requirement.capability == CardCapabilityType.None)
                continue;

            int requiredCount = Mathf.Max(0, requirement.minCount);

            for (int pickedCount = 0; pickedCount < requiredCount; pickedCount++)
            {
                CardView matchingCard = FindNextUnselectedCardWithCapability(stack, requirement.capability, alreadySelected);
                if (matchingCard == null)
                    break;

                alreadySelected.Add(matchingCard);
                selectedCards.Add(matchingCard);
            }
        }
    }

    private static CardView FindNextUnselectedCardWithCapability(CardStack stack, CardCapabilityType capability, HashSet<CardView> alreadySelected)
    {
        if (stack == null || capability == CardCapabilityType.None)
            return null;

        IReadOnlyList<CardView> cards = stack.Cards;

        for (int i = 0; i < cards.Count; i++)
        {
            CardView card = cards[i];
            if (card == null)
                continue;

            if (alreadySelected != null && alreadySelected.Contains(card))
                continue;

            CardInstance instance = card.Instance;
            if (instance == null || !instance.HasCapability(capability))
                continue;

            return card;
        }

        return null;
    }

    private static void SelectExactIngredientCards(CardStack stack, RecipeData recipe, List<CardView> selectedCards, HashSet<CardView> alreadySelected)
    {
        if (stack == null || recipe == null || selectedCards == null)
            return;

        List<RecipeIngredientRule> exactRequirements = recipe.GetExactIngredientRequirementsSnapshot();
        if (exactRequirements == null || exactRequirements.Count == 0)
            return;

        for (int requirementIndex = 0; requirementIndex < exactRequirements.Count; requirementIndex++)
        {
            RecipeIngredientRule requirement = exactRequirements[requirementIndex];
            if (requirement == null || requirement.card == null)
                continue;

            CardData targetCard = requirement.card;
            int requiredCount = requirement.GetNormalizedRequiredCount();

            for (int pickedCount = 0; pickedCount < requiredCount; pickedCount++)
            {
                CardView matchingCard = FindNextUnselectedExactIngredientCard(stack, targetCard, alreadySelected);
                if (matchingCard == null)
                    break;

                alreadySelected.Add(matchingCard);
                selectedCards.Add(matchingCard);
            }
        }
    }

    private static CardView FindNextUnselectedExactIngredientCard(CardStack stack, CardData targetCard, HashSet<CardView> alreadySelected)
    {
        if (stack == null || targetCard == null)
            return null;

        IReadOnlyList<CardView> cards = stack.Cards;

        for (int i = 0; i < cards.Count; i++)
        {
            CardView card = cards[i];
            if (card == null)
                continue;

            if (alreadySelected != null && alreadySelected.Contains(card))
                continue;

            CardInstance instance = card.Instance;
            if (instance == null || instance.data == null)
                continue;

            if (instance.data == targetCard)
                return card;
        }

        return null;
    }

    private static void ApplyConsumeModeToCard(CardStack stack, CardView card, CardInstance instance, RecipeIngredientConsumeMode mode)
    {
        switch (mode)
        {
            case RecipeIngredientConsumeMode.None:
                break;

            case RecipeIngredientConsumeMode.ConsumeOneUse:
                if (instance.ConsumeUseIfNeeded())
                    stack.DestroyCardForSystem(card);
                break;

            case RecipeIngredientConsumeMode.ConsumeEntireCard:
                stack.DestroyCardForSystem(card);
                break;
        }
    }
}
