using System.Collections.Generic;
using NUnit.Framework;
using StacklandsLike.Cards;
using UnityEngine;

public class RecipeTimingResolverTests
{
    private readonly List<Object> createdObjects = new List<Object>();

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
    public void GetCraftTime_ReturnsBaseTime_WhenNoModifiersApply()
    {
        RecipeData recipe = CreateRecipe("BaseTime");
        recipe.craftTime = 10f;

        float result = RecipeTimingResolver.GetCraftTime(recipe, CreateInput());

        Assert.That(result, Is.EqualTo(10f).Within(0.001f));
    }

    [Test]
    public void GetCraftTime_AppliesCapabilityMultiplier()
    {
        RecipeData recipe = CreateRecipe("WarmRecipe");
        recipe.craftTime = 10f;
        recipe.durationCapabilityModifiers.Add(new RecipeDurationCapabilityModifier
        {
            capability = CardCapabilityType.Worker,
            multiplier = 0.5f,
            maxApplications = 1
        });

        float result = RecipeTimingResolver.GetCraftTime(recipe, CreateInput(CreateCard("Worker", CardCapabilityType.Worker)));

        Assert.That(result, Is.EqualTo(5f).Within(0.001f));
    }

    [Test]
    public void GetCraftTime_RespectsMaxApplications()
    {
        RecipeData recipe = CreateRecipe("WarmRecipe");
        recipe.craftTime = 16f;
        recipe.durationCapabilityModifiers.Add(new RecipeDurationCapabilityModifier
        {
            capability = CardCapabilityType.Worker,
            multiplier = 0.5f,
            maxApplications = 2
        });

        float result = RecipeTimingResolver.GetCraftTime(
            recipe,
            CreateInput(
                CreateCard("WorkerA", CardCapabilityType.Worker),
                CreateCard("WorkerB", CardCapabilityType.Worker),
                CreateCard("WorkerC", CardCapabilityType.Worker)));

        Assert.That(result, Is.EqualTo(4f).Within(0.001f));
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
