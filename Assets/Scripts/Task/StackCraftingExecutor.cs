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

        if (recipe.matchMode != RecipeMatchMode.TagRequirementsOnly)
            return new List<CardView>(stack.Cards);

        List<CardView> selectedCards = new List<CardView>();
        HashSet<CardView> alreadySelected = new HashSet<CardView>();

        if (recipe.tagRequirements == null || recipe.tagRequirements.Count == 0)
            return selectedCards;

        for (int requirementIndex = 0; requirementIndex < recipe.tagRequirements.Count; requirementIndex++)
        {
            RecipeTagRequirement requirement = recipe.tagRequirements[requirementIndex];
            if (requirement == null || string.IsNullOrWhiteSpace(requirement.tag))
                continue;

            int requiredCount = Mathf.Max(0, requirement.minCount);

            for (int pickedCount = 0; pickedCount < requiredCount; pickedCount++)
            {
                CardView matchingCard = FindNextUnselectedCardWithTag(stack, requirement.tag, alreadySelected);
                if (matchingCard == null)
                    break;

                alreadySelected.Add(matchingCard);
                selectedCards.Add(matchingCard);
            }
        }

        return selectedCards;
    }

    private static CardView FindNextUnselectedCardWithTag(CardStack stack, string tag, HashSet<CardView> alreadySelected)
    {
        if (stack == null || string.IsNullOrWhiteSpace(tag))
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
            if (instance == null || !instance.HasTag(tag))
                continue;

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
