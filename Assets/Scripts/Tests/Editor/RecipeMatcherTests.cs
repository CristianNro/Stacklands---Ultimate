using NUnit.Framework;
using StacklandsLike.Cards;
using UnityEngine;

public class RecipeMatcherTests
{
    private readonly System.Collections.Generic.List<Object> createdObjects = new System.Collections.Generic.List<Object>();

    [TearDown]
    public void TearDown()
    {
        for (int i = 0; i < createdObjects.Count; i++)
        {
            if (createdObjects[i] != null)
                Object.DestroyImmediate(createdObjects[i]);
        }

        createdObjects.Clear();
    }

    [Test]
    public void Evaluate_ExactIngredients_MatchesRequiredCards()
    {
        CardData wood = CreateCard("Wood");
        RecipeData recipe = CreateRecipe("WoodRecipe");
        recipe.matchMode = RecipeMatchMode.ExactIngredients;
        recipe.ingredients.Add(wood);

        RecipeMatchInput input = CreateInput(wood);

        RecipeMatchResult result = RecipeMatcher.Evaluate(recipe, input);

        Assert.That(result.matched, Is.True);
        Assert.That(result.stack, Is.Null);
    }

    [Test]
    public void Evaluate_ExactIngredients_AllowsAdditionalCopies_WhenConfigured()
    {
        CardData tree = CreateCard("Tree");
        CardData worker = CreateCard("Worker", CardCapabilityType.Worker);
        RecipeData recipe = CreateRecipe("FarmTree");
        recipe.matchMode = RecipeMatchMode.ExactIngredients;
        recipe.ingredients.Add(tree);
        recipe.ingredientRules.Add(new RecipeIngredientRule
        {
            card = tree,
            requiredCount = 1,
            allowAdditionalCopies = true,
            consumeMode = RecipeIngredientConsumeMode.None
        });
        recipe.capabilityRequirements.Add(new RecipeCapabilityRequirement
        {
            capability = CardCapabilityType.Worker,
            minCount = 1,
            ignoreMatchingCardsInIngredientCheck = true
        });

        RecipeMatchInput input = CreateInput(tree, tree, worker);

        RecipeMatchResult result = RecipeMatcher.Evaluate(recipe, input);

        Assert.That(result.matched, Is.True);
    }

    [Test]
    public void Evaluate_ExactIngredients_RejectsUnexpectedExtraCopies_WhenNotConfigured()
    {
        CardData tree = CreateCard("Tree");
        RecipeData recipe = CreateRecipe("SingleTree");
        recipe.matchMode = RecipeMatchMode.ExactIngredients;
        recipe.ingredients.Add(tree);

        RecipeMatchInput input = CreateInput(tree, tree);

        RecipeMatchResult result = RecipeMatcher.Evaluate(recipe, input);

        Assert.That(result.matched, Is.False);
        Assert.That(result.reason, Does.Contain("exceeded exact requirement"));
    }

    [Test]
    public void Evaluate_ExactIngredients_RequiresMinimumCount_WhenRuleOverridesIngredientCount()
    {
        CardData wood = CreateCard("Wood");
        RecipeData recipe = CreateRecipe("DoubleWood");
        recipe.matchMode = RecipeMatchMode.ExactIngredients;
        recipe.ingredients.Add(wood);
        recipe.ingredientRules.Add(new RecipeIngredientRule
        {
            card = wood,
            requiredCount = 2,
            allowAdditionalCopies = false,
            consumeMode = RecipeIngredientConsumeMode.None
        });

        RecipeMatchInput input = CreateInput(wood);

        RecipeMatchResult result = RecipeMatcher.Evaluate(recipe, input);

        Assert.That(result.matched, Is.False);
        Assert.That(result.reason, Does.Contain("Expected at least 2"));
    }

    [Test]
    public void Evaluate_ExactIngredients_MatchesWhenRequiredCountIsMet()
    {
        CardData wood = CreateCard("Wood");
        RecipeData recipe = CreateRecipe("DoubleWood");
        recipe.matchMode = RecipeMatchMode.ExactIngredients;
        recipe.ingredients.Add(wood);
        recipe.ingredientRules.Add(new RecipeIngredientRule
        {
            card = wood,
            requiredCount = 2,
            allowAdditionalCopies = false,
            consumeMode = RecipeIngredientConsumeMode.None
        });

        RecipeMatchInput input = CreateInput(wood, wood);

        RecipeMatchResult result = RecipeMatcher.Evaluate(recipe, input);

        Assert.That(result.matched, Is.True);
    }

    [Test]
    public void Evaluate_CapabilityRequirementsOnly_MatchesWhenAllCardsAreAllowedAndRequirementIsMet()
    {
        CardData workerA = CreateCard("WorkerA", CardCapabilityType.Worker);
        CardData workerB = CreateCard("WorkerB", CardCapabilityType.Worker);
        RecipeData recipe = CreateRecipe("WorkersOnly");
        recipe.matchMode = RecipeMatchMode.CapabilityRequirementsOnly;
        recipe.capabilityRequirements.Add(new RecipeCapabilityRequirement
        {
            capability = CardCapabilityType.Worker,
            minCount = 2,
            ignoreMatchingCardsInIngredientCheck = true
        });

        RecipeMatchInput input = CreateInput(workerA, workerB);

        RecipeMatchResult result = RecipeMatcher.Evaluate(recipe, input);

        Assert.That(result.matched, Is.True);
    }

    [Test]
    public void Evaluate_CapabilityRequirementsOnly_MatchesWithoutSourceStack()
    {
        CardData worker = CreateCard("Worker", CardCapabilityType.Worker);
        RecipeData recipe = CreateRecipe("WorkerOnly");
        recipe.matchMode = RecipeMatchMode.CapabilityRequirementsOnly;
        recipe.capabilityRequirements.Add(new RecipeCapabilityRequirement
        {
            capability = CardCapabilityType.Worker,
            minCount = 1,
            ignoreMatchingCardsInIngredientCheck = true
        });

        RecipeMatchInput input = CreateInput(worker);

        RecipeMatchResult result = RecipeMatcher.Evaluate(recipe, input);

        Assert.That(result.matched, Is.True);
        Assert.That(result.stack, Is.Null);
    }

    [Test]
    public void Evaluate_CapabilityRequirementsOnly_RejectsCardsOutsideAllowedCapabilities()
    {
        CardData worker = CreateCard("Worker", CardCapabilityType.Worker);
        CardData stone = CreateCard("Stone");
        RecipeData recipe = CreateRecipe("WorkersOnly");
        recipe.matchMode = RecipeMatchMode.CapabilityRequirementsOnly;
        recipe.capabilityRequirements.Add(new RecipeCapabilityRequirement
        {
            capability = CardCapabilityType.Worker,
            minCount = 1,
            ignoreMatchingCardsInIngredientCheck = true
        });

        RecipeMatchInput input = CreateInput(worker, stone);

        RecipeMatchResult result = RecipeMatcher.Evaluate(recipe, input);

        Assert.That(result.matched, Is.False);
        Assert.That(result.reason, Does.Contain("not allowed by capability-driven recipe constraints"));
    }

    [Test]
    public void Evaluate_CapabilityRequirementsOnly_RejectsWhenCapabilityExceedsMaximum()
    {
        CardData workerA = CreateCard("WorkerA", CardCapabilityType.Worker);
        CardData workerB = CreateCard("WorkerB", CardCapabilityType.Worker);
        RecipeData recipe = CreateRecipe("SingleWorkerOnly");
        recipe.matchMode = RecipeMatchMode.CapabilityRequirementsOnly;
        recipe.capabilityRequirements.Add(new RecipeCapabilityRequirement
        {
            capability = CardCapabilityType.Worker,
            minCount = 1,
            maxCount = 1,
            ignoreMatchingCardsInIngredientCheck = true
        });

        RecipeMatchInput input = CreateInput(workerA, workerB);

        RecipeMatchResult result = RecipeMatcher.Evaluate(recipe, input);

        Assert.That(result.matched, Is.False);
        Assert.That(result.reason, Does.Contain("exceeded maximum"));
    }

    [Test]
    public void Evaluate_ExactIngredients_RejectsWhenCapabilityExceedsMaximum()
    {
        CardData tree = CreateCard("Tree");
        CardData workerA = CreateCard("WorkerA", CardCapabilityType.Worker);
        CardData workerB = CreateCard("WorkerB", CardCapabilityType.Worker);
        RecipeData recipe = CreateRecipe("TreeWithOneWorker");
        recipe.matchMode = RecipeMatchMode.ExactIngredients;
        recipe.ingredients.Add(tree);
        recipe.capabilityRequirements.Add(new RecipeCapabilityRequirement
        {
            capability = CardCapabilityType.Worker,
            minCount = 1,
            maxCount = 1,
            ignoreMatchingCardsInIngredientCheck = true
        });

        RecipeMatchInput input = CreateInput(tree, workerA, workerB);

        RecipeMatchResult result = RecipeMatcher.Evaluate(recipe, input);

        Assert.That(result.matched, Is.False);
        Assert.That(result.reason, Does.Contain("exceeded maximum"));
    }

    private RecipeData CreateRecipe(string id)
    {
        RecipeData recipe = ScriptableObject.CreateInstance<RecipeData>();
        recipe.name = id;
        recipe.id = id;
        recipe.displayName = id;
        createdObjects.Add(recipe);
        return recipe;
    }

    private CardData CreateCard(string id, params CardCapabilityType[] capabilities)
    {
        ResourceCardData cardData = ScriptableObject.CreateInstance<ResourceCardData>();
        cardData.name = id;
        cardData.id = id;
        cardData.cardName = id;
        cardData.cardType = CardType.Resource;

        if (capabilities != null)
            cardData.capabilities.AddRange(capabilities);

        createdObjects.Add(cardData);
        return cardData;
    }

    private RecipeMatchInput CreateInput(params CardData[] cards)
    {
        RecipeMatchInput input = new RecipeMatchInput();

        for (int i = 0; i < cards.Length; i++)
        {
            CardData cardData = cards[i];
            if (cardData == null)
                continue;

            RecipeMatchInputCard inputCard = new RecipeMatchInputCard
            {
                data = cardData
            };

            if (cardData.capabilities != null)
                inputCard.capabilities.AddRange(cardData.capabilities);

            input.cards.Add(inputCard);
        }

        return input;
    }
}
