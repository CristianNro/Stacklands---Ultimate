using System.Collections.Generic;
using StacklandsLike.Cards;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class CardTransformationValidationUtility
{
#if UNITY_EDITOR
    public static void ValidateAndLog(CardTransformationRule rule)
    {
        if (rule == null)
            return;

        List<string> warnings = Validate(rule);
        for (int i = 0; i < warnings.Count; i++)
            Debug.LogWarning($"[CardTransformation] {warnings[i]}", rule);
    }

    public static List<string> Validate(CardTransformationRule rule)
    {
        List<string> warnings = new List<string>();
        if (rule == null)
            return warnings;

        if (string.IsNullOrWhiteSpace(rule.id))
        {
            warnings.Add($"Transformation rule '{rule.name}' is missing a stable id.");
        }
        else
        {
            CardTransformationRule duplicate = FindDuplicateId(rule);
            if (duplicate != null)
            {
                warnings.Add(
                    $"Duplicate transformation rule id '{rule.id}'. Conflicting assets: '{duplicate.name}' and '{rule.name}'.");
            }
        }

        if (string.IsNullOrWhiteSpace(rule.displayName))
            warnings.Add($"Transformation rule '{rule.name}' is missing displayName.");

        if (rule.sourceCard == null)
            warnings.Add($"Transformation rule '{rule.name}' is missing sourceCard.");

        if (rule.baseDuration <= 0f)
            warnings.Add($"Transformation rule '{rule.name}' should have baseDuration greater than 0.");

        ValidateRequiredCapabilities(rule, warnings);
        ValidatePauseCapabilities(rule, warnings);
        ValidateSpeedModifiers(rule, warnings);
        ValidateCompletion(rule, warnings);

        return warnings;
    }

    private static void ValidateRequiredCapabilities(CardTransformationRule rule, List<string> warnings)
    {
        if (rule.requiredCapabilities == null)
            return;

        HashSet<CardCapabilityType> seen = new HashSet<CardCapabilityType>();
        for (int i = 0; i < rule.requiredCapabilities.Count; i++)
        {
            CardCapabilityType capability = rule.requiredCapabilities[i];
            if (capability == CardCapabilityType.None)
            {
                warnings.Add($"Transformation rule '{rule.name}' contains CardCapabilityType.None in requiredCapabilities.");
                continue;
            }

            if (!seen.Add(capability))
                warnings.Add($"Transformation rule '{rule.name}' repeats required capability '{capability}'.");
        }
    }

    private static void ValidatePauseCapabilities(CardTransformationRule rule, List<string> warnings)
    {
        if (rule.pauseCapabilities == null)
            return;

        HashSet<CardCapabilityType> seen = new HashSet<CardCapabilityType>();
        for (int i = 0; i < rule.pauseCapabilities.Count; i++)
        {
            CardCapabilityType capability = rule.pauseCapabilities[i];
            if (capability == CardCapabilityType.None)
            {
                warnings.Add($"Transformation rule '{rule.name}' contains CardCapabilityType.None in pauseCapabilities.");
                continue;
            }

            if (!seen.Add(capability))
                warnings.Add($"Transformation rule '{rule.name}' repeats pause capability '{capability}'.");
        }
    }

    private static void ValidateSpeedModifiers(CardTransformationRule rule, List<string> warnings)
    {
        if (rule.speedModifiers == null)
            return;

        HashSet<CardCapabilityType> seen = new HashSet<CardCapabilityType>();
        for (int i = 0; i < rule.speedModifiers.Count; i++)
        {
            CardTransformationSpeedModifier modifier = rule.speedModifiers[i];
            if (modifier == null)
            {
                warnings.Add($"Transformation rule '{rule.name}' contains an empty speed modifier.");
                continue;
            }

            if (modifier.capability == CardCapabilityType.None)
            {
                warnings.Add($"Transformation rule '{rule.name}' contains a speed modifier with CardCapabilityType.None.");
                continue;
            }

            if (modifier.speedMultiplier <= 0f)
            {
                warnings.Add($"Transformation rule '{rule.name}' contains speed modifier '{modifier.capability}' with non-positive multiplier '{modifier.speedMultiplier}'.");
            }

            if (!seen.Add(modifier.capability))
                warnings.Add($"Transformation rule '{rule.name}' repeats speed modifier capability '{modifier.capability}'.");
        }
    }

    private static void ValidateCompletion(CardTransformationRule rule, List<string> warnings)
    {
        switch (rule.completionMode)
        {
            case CardTransformationCompletionMode.DestroyOnly:
                return;

            case CardTransformationCompletionMode.ReplaceWithSingleResult:
                if (rule.resultCard == null)
                    warnings.Add($"Transformation rule '{rule.name}' uses ReplaceWithSingleResult but has no resultCard.");
                return;

            case CardTransformationCompletionMode.SpawnMultipleResults:
                ValidateResultEntries(rule, warnings);
                return;
        }
    }

    private static void ValidateResultEntries(CardTransformationRule rule, List<string> warnings)
    {
        if (rule.resultCards == null || rule.resultCards.Count == 0)
        {
            warnings.Add($"Transformation rule '{rule.name}' uses SpawnMultipleResults but has no resultCards configured.");
            return;
        }

        bool hasValidEntry = false;

        for (int i = 0; i < rule.resultCards.Count; i++)
        {
            CardTransformationResultEntry entry = rule.resultCards[i];
            if (entry == null || entry.card == null)
            {
                warnings.Add($"Transformation rule '{rule.name}' contains an empty result entry.");
                continue;
            }

            if (entry.count <= 0)
            {
                warnings.Add($"Transformation rule '{rule.name}' contains '{entry.card.name}' with non-positive count '{entry.count}'.");
                continue;
            }

            hasValidEntry = true;
        }

        if (!hasValidEntry)
            warnings.Add($"Transformation rule '{rule.name}' has no valid result entries.");
    }

    private static CardTransformationRule FindDuplicateId(CardTransformationRule source)
    {
        if (source == null || string.IsNullOrWhiteSpace(source.id))
            return null;

        string sourcePath = AssetDatabase.GetAssetPath(source);
        string[] guids = AssetDatabase.FindAssets("t:CardTransformationRule");

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (path == sourcePath)
                continue;

            CardTransformationRule other = AssetDatabase.LoadAssetAtPath<CardTransformationRule>(path);
            if (other == null)
                continue;

            if (string.Equals(other.id, source.id))
                return other;
        }

        return null;
    }
#endif
}
