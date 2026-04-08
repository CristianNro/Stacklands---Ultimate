using System.Collections.Generic;
using NUnit.Framework;
using StacklandsLike.Cards;
using UnityEngine;

public class MarketEconomyServiceTests
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
    public void MatchesCurrencyFilter_AllowOnlyListed_AcceptsOnlyListedTypes()
    {
        List<CurrencyType> accepted = new List<CurrencyType> { CurrencyType.Normal };

        Assert.That(
            MarketEconomyService.MatchesCurrencyFilter(CurrencyType.Normal, CurrencyFilterMode.AllowOnlyListed, accepted),
            Is.True);

        Assert.That(
            MarketEconomyService.MatchesCurrencyFilter(CurrencyType.None, CurrencyFilterMode.AllowOnlyListed, accepted),
            Is.False);
    }

    [Test]
    public void BuildBestValueCombination_PrefersFewerCards()
    {
        CardData coinOne = CreateCurrencyCard("CoinOne", 1);
        CardData coinThree = CreateCurrencyCard("CoinThree", 3);
        CardData coinFour = CreateCurrencyCard("CoinFour", 4);

        List<CardData> result = MarketEconomyService.BuildBestValueCombination(
            new List<CardData> { coinOne, coinThree, coinFour },
            6,
            CurrencyFilterMode.AllowOnlyListed,
            new List<CurrencyType> { CurrencyType.Normal });

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0], Is.EqualTo(coinThree));
        Assert.That(result[1], Is.EqualTo(coinThree));
    }

    [Test]
    public void BuildBestValueCombination_RespectsCurrencyFilter()
    {
        CardData normalCoin = CreateCurrencyCard("NormalCoin", 2, CurrencyType.Normal);
        CardData otherCoin = CreateCurrencyCard("OtherCoin", 2, CurrencyType.None);

        List<CardData> result = MarketEconomyService.BuildBestValueCombination(
            new List<CardData> { normalCoin, otherCoin },
            2,
            CurrencyFilterMode.AllowOnlyListed,
            new List<CurrencyType> { CurrencyType.Normal });

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(normalCoin));
    }

    [Test]
    public void BuildBestValueCombination_ReturnsNull_WhenExactValueIsImpossible()
    {
        CardData coinFour = CreateCurrencyCard("CoinFour", 4);
        CardData coinSix = CreateCurrencyCard("CoinSix", 6);

        List<CardData> result = MarketEconomyService.BuildBestValueCombination(
            new List<CardData> { coinFour, coinSix },
            5,
            CurrencyFilterMode.AllowOnlyListed,
            new List<CurrencyType> { CurrencyType.Normal });

        Assert.That(result, Is.Null);
    }

    [Test]
    public void BuildBestValueCombination_IgnoresCurrenciesWithNonPositiveValue()
    {
        CardData invalidCoin = CreateCurrencyCard("InvalidCoin", 0);
        CardData validCoin = CreateCurrencyCard("ValidCoin", 2);

        List<CardData> result = MarketEconomyService.BuildBestValueCombination(
            new List<CardData> { invalidCoin, validCoin },
            2,
            CurrencyFilterMode.AllowOnlyListed,
            new List<CurrencyType> { CurrencyType.Normal });

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(validCoin));
    }

    private CardData CreateCurrencyCard(string name, int value, CurrencyType currencyType = CurrencyType.Normal)
    {
        ResourceCardData cardData = ScriptableObject.CreateInstance<ResourceCardData>();
        cardData.name = name;
        cardData.id = name;
        cardData.cardName = name;
        cardData.cardType = CardType.Resource;
        cardData.isCurrency = currencyType != CurrencyType.None;
        cardData.currencyType = currencyType;
        cardData.value = value;
        createdObjects.Add(cardData);
        return cardData;
    }
}
