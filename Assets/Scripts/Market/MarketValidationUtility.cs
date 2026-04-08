using System.Collections.Generic;
using StacklandsLike.Cards;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class MarketValidationUtility
{
#if UNITY_EDITOR
    public static void ValidateAndLogPack(BaseMarketPackData packData)
    {
        if (packData == null)
            return;

        List<string> warnings = ValidatePack(packData);
        for (int i = 0; i < warnings.Count; i++)
            Debug.LogWarning($"[MarketValidation] {warnings[i]}", packData);
    }

    public static void ValidateAndLogPurchaseSlot(MarketPackPurchaseSlot slot)
    {
        if (slot == null)
            return;

        List<string> warnings = ValidatePurchaseSlot(slot);
        for (int i = 0; i < warnings.Count; i++)
            Debug.LogWarning($"[MarketValidation] {warnings[i]}", slot);
    }

    public static void ValidateAndLogSellSlot(MarketSellSlot slot)
    {
        if (slot == null)
            return;

        List<string> warnings = ValidateSellSlot(slot);
        for (int i = 0; i < warnings.Count; i++)
            Debug.LogWarning($"[MarketValidation] {warnings[i]}", slot);
    }

    public static List<string> ValidatePack(BaseMarketPackData packData)
    {
        List<string> warnings = new List<string>();
        if (packData == null)
            return warnings;

        if (string.IsNullOrWhiteSpace(packData.displayName))
            warnings.Add($"Pack asset '{packData.name}' is missing displayName.");

        if (packData.price <= 0)
            warnings.Add($"Pack asset '{packData.name}' should have price greater than 0.");

        if (packData.packCard == null)
            warnings.Add($"Pack asset '{packData.name}' is missing packCard.");

        if (packData is MarketPackData randomPack)
            ValidateRandomPack(randomPack, warnings);

        if (packData is PresetMarketPackData presetPack)
            ValidatePresetPack(presetPack, warnings);

        return warnings;
    }

    public static List<string> ValidatePurchaseSlot(MarketPackPurchaseSlot slot)
    {
        List<string> warnings = new List<string>();
        if (slot == null)
            return warnings;

        if (slot.PackData == null)
            warnings.Add($"Purchase slot '{slot.name}' is missing packData.");

        ValidateCurrencyFilterConfiguration(
            $"Purchase slot '{slot.name}'",
            slot.AcceptedCurrencyFilterMode,
            slot.AcceptedCurrencyTypes,
            warnings);

        ValidateCurrencyCardList(
            $"Purchase slot '{slot.name}' change",
            slot.ChangeCurrencyCards,
            slot.AcceptedCurrencyFilterMode,
            slot.AcceptedCurrencyTypes,
            warnings);

        return warnings;
    }

    public static List<string> ValidateSellSlot(MarketSellSlot slot)
    {
        List<string> warnings = new List<string>();
        if (slot == null)
            return warnings;

        ValidateCurrencyFilterConfiguration(
            $"Sell slot '{slot.name}'",
            slot.AcceptedCurrencyFilterMode,
            slot.AcceptedCurrencyTypes,
            warnings);

        ValidateCurrencyCardList(
            $"Sell slot '{slot.name}' reward",
            slot.RewardCurrencyCards,
            slot.AcceptedCurrencyFilterMode,
            slot.AcceptedCurrencyTypes,
            warnings);

        return warnings;
    }

    private static void ValidateRandomPack(MarketPackData packData, List<string> warnings)
    {
        if (packData.cardsToRoll <= 0)
            warnings.Add($"Random pack '{packData.name}' should have cardsToRoll greater than 0.");

        if (packData.possibleCards == null || packData.possibleCards.Count == 0)
        {
            warnings.Add($"Random pack '{packData.name}' has no possibleCards configured.");
            return;
        }

        bool hasValidOption = false;
        for (int i = 0; i < packData.possibleCards.Count; i++)
        {
            RecipeResultOption option = packData.possibleCards[i];
            if (option == null)
            {
                warnings.Add($"Random pack '{packData.name}' contains a null option in possibleCards.");
                continue;
            }

            if (option.result == null)
            {
                warnings.Add($"Random pack '{packData.name}' contains an option without result.");
                continue;
            }

            if (option.weight <= 0f)
            {
                warnings.Add(
                    $"Random pack '{packData.name}' contains option '{option.result.name}' with non-positive weight '{option.weight}'.");
                continue;
            }

            hasValidOption = true;
        }

        if (!hasValidOption)
            warnings.Add($"Random pack '{packData.name}' has no valid weighted options to roll.");
    }

    private static void ValidatePresetPack(PresetMarketPackData packData, List<string> warnings)
    {
        if (packData.fixedCards == null || packData.fixedCards.Count == 0)
        {
            warnings.Add($"Preset pack '{packData.name}' has no fixedCards configured.");
            return;
        }

        bool hasValidCard = false;
        for (int i = 0; i < packData.fixedCards.Count; i++)
        {
            CardData cardData = packData.fixedCards[i];
            if (cardData == null)
            {
                warnings.Add($"Preset pack '{packData.name}' contains a null entry in fixedCards.");
                continue;
            }

            hasValidCard = true;
        }

        if (!hasValidCard)
            warnings.Add($"Preset pack '{packData.name}' has no valid fixedCards to open.");
    }

    private static void ValidateCurrencyFilterConfiguration(
        string ownerLabel,
        CurrencyFilterMode filterMode,
        IReadOnlyList<CurrencyType> listedTypes,
        List<string> warnings)
    {
        HashSet<CurrencyType> seen = new HashSet<CurrencyType>();
        bool hasValidType = false;

        if (listedTypes != null)
        {
            for (int i = 0; i < listedTypes.Count; i++)
            {
                CurrencyType currencyType = listedTypes[i];
                if (currencyType == CurrencyType.None)
                {
                    warnings.Add($"{ownerLabel} lists CurrencyType.None in its accepted currency types.");
                    continue;
                }

                if (!seen.Add(currencyType))
                    warnings.Add($"{ownerLabel} repeats accepted currency type '{currencyType}'.");

                hasValidType = true;
            }
        }

        if (filterMode == CurrencyFilterMode.AllowOnlyListed && !hasValidType)
        {
            warnings.Add($"{ownerLabel} uses AllowOnlyListed but has no valid accepted currency types.");
        }
    }

    private static void ValidateCurrencyCardList(
        string ownerLabel,
        IReadOnlyList<CardData> currencyCards,
        CurrencyFilterMode filterMode,
        IReadOnlyList<CurrencyType> listedTypes,
        List<string> warnings)
    {
        if (currencyCards == null || currencyCards.Count == 0)
        {
            warnings.Add($"{ownerLabel} currency card list is empty.");
            return;
        }

        HashSet<CardData> seen = new HashSet<CardData>();
        bool hasValidCard = false;

        for (int i = 0; i < currencyCards.Count; i++)
        {
            CardData cardData = currencyCards[i];
            if (cardData == null)
            {
                warnings.Add($"{ownerLabel} currency card list contains a null entry.");
                continue;
            }

            if (!seen.Add(cardData))
                warnings.Add($"{ownerLabel} currency card list repeats '{cardData.name}'.");

            if (!MarketEconomyService.IsAcceptedCurrency(cardData, filterMode, listedTypes))
            {
                warnings.Add(
                    $"{ownerLabel} currency card '{cardData.name}' is not accepted by the slot currency filter.");
                continue;
            }

            if (MarketPricingService.GetEffectiveMarketValue(cardData) <= 0)
            {
                warnings.Add(
                    $"{ownerLabel} currency card '{cardData.name}' has non-positive market value.");
                continue;
            }

            hasValidCard = true;
        }

        if (!hasValidCard)
            warnings.Add($"{ownerLabel} currency card list has no valid usable cards.");
    }
#endif
}
