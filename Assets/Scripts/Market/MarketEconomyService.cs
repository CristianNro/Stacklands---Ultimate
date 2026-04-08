using System.Collections.Generic;
using UnityEngine;
using StacklandsLike.Cards;

// ============================================================
// MarketEconomyService
// ------------------------------------------------------------
// Centraliza helpers economicos compartidos por compra y venta
// del market. En esta etapa concentra:
// - validacion de currency por contrato explicito
// - resolucion de combinaciones exactas por valor
// - consumo fisico de cartas pagadas o vendidas
// ============================================================
public static class MarketEconomyService
{
    public static bool IsCurrency(CardInstance instance)
    {
        return instance != null && instance.IsCurrency();
    }

    public static bool IsCurrency(CardData data)
    {
        return data != null && data.isCurrency && data.currencyType != CurrencyType.None;
    }

    public static bool MatchesCurrencyFilter(CurrencyType currencyType, CurrencyFilterMode filterMode, IReadOnlyList<CurrencyType> listedCurrencyTypes)
    {
        bool isListed = IsListedCurrencyType(listedCurrencyTypes, currencyType);

        switch (filterMode)
        {
            case CurrencyFilterMode.BlockListed:
                return currencyType != CurrencyType.None && !isListed;

            case CurrencyFilterMode.AllowOnlyListed:
            default:
                return isListed;
        }
    }

    private static bool IsListedCurrencyType(IReadOnlyList<CurrencyType> listedCurrencyTypes, CurrencyType currencyType)
    {
        if (listedCurrencyTypes == null)
            return false;

        for (int i = 0; i < listedCurrencyTypes.Count; i++)
        {
            if (listedCurrencyTypes[i] == currencyType)
                return true;
        }

        return false;
    }

    public static bool IsAcceptedCurrency(CardInstance instance, CurrencyFilterMode filterMode, IReadOnlyList<CurrencyType> listedCurrencyTypes)
    {
        return instance != null
            && instance.IsCurrency()
            && MatchesCurrencyFilter(instance.GetCurrencyType(), filterMode, listedCurrencyTypes);
    }

    public static bool IsAcceptedCurrency(CardData data, CurrencyFilterMode filterMode, IReadOnlyList<CurrencyType> listedCurrencyTypes)
    {
        return data != null
            && IsCurrency(data)
            && MatchesCurrencyFilter(data.currencyType, filterMode, listedCurrencyTypes);
    }

    public static bool TryGetPositiveValue(CardInstance instance, out int value)
    {
        value = MarketPricingService.GetEffectiveMarketValue(instance);
        return value > 0;
    }

    public static void DestroyCardUnit(CardView card)
    {
        if (card == null)
            return;

        CardInstance instance = card.Instance;
        CardStack currentStack = instance != null ? instance.CurrentStack : null;

        if (currentStack != null)
            currentStack.RemoveCard(card);

        Object.Destroy(card.gameObject);
    }

    public static List<CardData> BuildBestValueCombination(
        IReadOnlyList<CardData> options,
        int targetValue,
        CurrencyFilterMode filterMode,
        IReadOnlyList<CurrencyType> listedCurrencyTypes)
    {
        if (targetValue <= 0 || options == null || options.Count == 0)
            return null;

        List<ValueOption> validOptions = GetValidValueOptions(options, filterMode, listedCurrencyTypes);
        if (validOptions.Count == 0)
            return null;

        int maxValue = int.MaxValue / 4;
        int[] bestCardCounts = new int[targetValue + 1];
        int[] chosenOptionIndex = new int[targetValue + 1];

        for (int amount = 0; amount <= targetValue; amount++)
        {
            bestCardCounts[amount] = maxValue;
            chosenOptionIndex[amount] = -1;
        }

        bestCardCounts[0] = 0;

        for (int amount = 1; amount <= targetValue; amount++)
        {
            for (int optionIndex = 0; optionIndex < validOptions.Count; optionIndex++)
            {
                ValueOption option = validOptions[optionIndex];
                if (option.value > amount)
                    continue;

                int previousAmount = amount - option.value;
                if (bestCardCounts[previousAmount] == maxValue)
                    continue;

                int candidateCount = bestCardCounts[previousAmount] + 1;
                bool isBetterCount = candidateCount < bestCardCounts[amount];
                bool isSameCountButHigherValue = candidateCount == bestCardCounts[amount]
                    && chosenOptionIndex[amount] >= 0
                    && option.value > validOptions[chosenOptionIndex[amount]].value;

                if (!isBetterCount && !isSameCountButHigherValue)
                    continue;

                bestCardCounts[amount] = candidateCount;
                chosenOptionIndex[amount] = optionIndex;
            }
        }

        if (chosenOptionIndex[targetValue] < 0)
            return null;

        List<CardData> result = new List<CardData>();
        int remaining = targetValue;

        while (remaining > 0)
        {
            int optionIndex = chosenOptionIndex[remaining];
            if (optionIndex < 0)
                return null;

            ValueOption option = validOptions[optionIndex];
            result.Add(option.cardData);
            remaining -= option.value;
        }

        return result;
    }

    private static List<ValueOption> GetValidValueOptions(
        IReadOnlyList<CardData> options,
        CurrencyFilterMode filterMode,
        IReadOnlyList<CurrencyType> listedCurrencyTypes)
    {
        List<ValueOption> validOptions = new List<ValueOption>();

        for (int i = 0; i < options.Count; i++)
        {
            CardData cardData = options[i];
            if (!IsAcceptedCurrency(cardData, filterMode, listedCurrencyTypes))
                continue;

            int value = MarketPricingService.GetEffectiveMarketValue(cardData);
            if (value <= 0)
                continue;

            validOptions.Add(new ValueOption
            {
                cardData = cardData,
                value = value
            });
        }

        return validOptions;
    }

    private sealed class ValueOption
    {
        public CardData cardData;
        public int value;
    }
}
