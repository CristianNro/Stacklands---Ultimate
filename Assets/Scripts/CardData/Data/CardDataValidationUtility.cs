using System.Collections.Generic;
using StacklandsLike.Cards;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class CardDataValidationUtility
{
#if UNITY_EDITOR
    public static void ValidateAndLog(CardData cardData)
    {
        if (cardData == null)
            return;

        List<string> warnings = Validate(cardData);
        for (int i = 0; i < warnings.Count; i++)
            Debug.LogWarning($"[CardData] {warnings[i]}", cardData);
    }

    public static List<string> Validate(CardData cardData)
    {
        List<string> warnings = new List<string>();
        if (cardData == null)
            return warnings;

        if (string.IsNullOrWhiteSpace(cardData.id))
        {
            warnings.Add($"Card asset '{cardData.name}' is missing a stable id.");
        }
        else
        {
            CardData duplicate = FindDuplicateId(cardData);
            if (duplicate != null)
            {
                warnings.Add(
                    $"Duplicate card id '{cardData.id}'. Conflicting assets: '{duplicate.name}' and '{cardData.name}'.");
            }
        }

        if (string.IsNullOrWhiteSpace(cardData.cardName))
            warnings.Add($"Card asset '{cardData.name}' is missing cardName.");

        if (cardData.value < 0)
            warnings.Add($"Card asset '{cardData.name}' has negative value '{cardData.value}'.");

        if (cardData.weight < 0f)
            warnings.Add($"Card asset '{cardData.name}' has negative weight '{cardData.weight}'.");

        if (cardData.maxUses < 0)
            warnings.Add($"Card asset '{cardData.name}' has negative maxUses '{cardData.maxUses}'.");

        if (cardData.isCurrency && cardData.currencyType == CurrencyType.None)
        {
            warnings.Add(
                $"Card asset '{cardData.name}' is marked as currency but has CurrencyType.None.");
        }

        if (!cardData.isCurrency && cardData.currencyType != CurrencyType.None)
        {
            warnings.Add(
                $"Card asset '{cardData.name}' sets currencyType to '{cardData.currencyType}' while isCurrency is false.");
        }

        ValidateCapabilities(cardData, warnings);
        ValidateTransformationReference(cardData, warnings);
        ValidateSubtypeConsistency(cardData, warnings);

        return warnings;
    }

    private static void ValidateTransformationReference(CardData cardData, List<string> warnings)
    {
        if (cardData == null || cardData.transformationRule == null)
            return;

        if (cardData.transformationRule.sourceCard != null && cardData.transformationRule.sourceCard != cardData)
        {
            warnings.Add(
                $"Card asset '{cardData.name}' references transformation rule '{cardData.transformationRule.name}', but that rule points to a different sourceCard '{cardData.transformationRule.sourceCard.name}'.");
        }
    }

    private static void ValidateCapabilities(CardData cardData, List<string> warnings)
    {
        if (cardData.capabilities == null)
            return;

        HashSet<CardCapabilityType> seen = new HashSet<CardCapabilityType>();

        for (int i = 0; i < cardData.capabilities.Count; i++)
        {
            CardCapabilityType capability = cardData.capabilities[i];
            if (capability == CardCapabilityType.None)
            {
                warnings.Add($"Card asset '{cardData.name}' contains CardCapabilityType.None in capabilities.");
                continue;
            }

            if (!seen.Add(capability))
            {
                warnings.Add(
                    $"Card asset '{cardData.name}' repeats capability '{capability}'.");
            }
        }
    }

    private static void ValidateSubtypeConsistency(CardData cardData, List<string> warnings)
    {
        if (cardData is CombatantCardData combatantData)
        {
            if (combatantData.maxHealth <= 0)
                warnings.Add($"Combatant card '{cardData.name}' should have maxHealth greater than 0.");

            if (combatantData.attackDamage < 0)
                warnings.Add($"Combatant card '{cardData.name}' should not have negative attackDamage.");

            if (combatantData.attackInterval <= 0f)
                warnings.Add($"Combatant card '{cardData.name}' should have attackInterval greater than 0.");

            if (combatantData.basePhysicalArmor < 0)
                warnings.Add($"Combatant card '{cardData.name}' should not have negative basePhysicalArmor.");

            if (combatantData.baseMagicalArmor < 0)
                warnings.Add($"Combatant card '{cardData.name}' should not have negative baseMagicalArmor.");

            ValidateAttackDamageTypes(cardData, combatantData, warnings);
            ValidateDamageModifierEntries(cardData, combatantData, warnings);
        }

        if (cardData is SurvivorUnitCardData survivorData)
        {
            if (survivorData.cardType != CardType.Unit)
            {
                warnings.Add(
                    $"Survivor unit card '{cardData.name}' should use CardType.Unit, but is currently '{survivorData.cardType}'.");
            }

            if (survivorData.maxHunger < 0f)
                warnings.Add($"Unit card '{cardData.name}' should not have negative maxHunger.");

            if (survivorData.dailyFoodConsumption <= 0)
                warnings.Add($"Unit card '{cardData.name}' should have dailyFoodConsumption greater than 0.");
        }

        if (cardData is EnemyCardData enemyData)
        {
            if (enemyData.cardType != CardType.Enemy)
            {
                warnings.Add(
                    $"Enemy card '{cardData.name}' should use CardType.Enemy, but is currently '{enemyData.cardType}'.");
            }

            if (enemyData.faction != FactionType.Enemy)
            {
                warnings.Add(
                    $"Enemy card '{cardData.name}' should usually use FactionType.Enemy, but is currently '{enemyData.faction}'.");
            }

            ValidateEnemyGuaranteedDrops(cardData, enemyData, warnings);
            ValidateEnemyRandomDrops(cardData, enemyData, warnings);
        }

        if (cardData is BuildingCardData buildingData)
        {
            if (buildingData.durability <= 0)
                warnings.Add($"Building card '{cardData.name}' should have durability greater than 0.");
        }

        if (cardData is FoodResourceCardData foodData)
        {
            if (foodData.foodValue <= 0)
                warnings.Add($"Food resource card '{cardData.name}' should have foodValue greater than 0.");

            if (foodData.resourceType != ResourceType.Food)
                warnings.Add($"Food resource card '{cardData.name}' should use ResourceType.Food.");

            if (foodData.spoilAfterSeconds < 0f)
                warnings.Add($"Food resource card '{cardData.name}' should not have negative spoilAfterSeconds.");
        }

        if (cardData is ContainerCardData containerData)
        {
            if (containerData.capacity <= 0)
                warnings.Add($"Container card '{cardData.name}' should have capacity greater than 0.");

            if (containerData.releaseRadius < 0f)
                warnings.Add($"Container card '{cardData.name}' should not have negative releaseRadius.");

            if (containerData.maxCardsReleasedPerOpen < 0)
                warnings.Add($"Container card '{cardData.name}' should not have negative maxCardsReleasedPerOpen.");

            ValidateContainerConfiguration(cardData, containerData, warnings);
        }

        if (cardData is PackCardData packData && packData.embeddedPackData == null)
            warnings.Add($"Pack card '{cardData.name}' is missing embeddedPackData.");
    }

    private static void ValidateContainerConfiguration(CardData cardData, ContainerCardData containerData, List<string> warnings)
    {
        ValidateDuplicateCardTypes(cardData, containerData, warnings);
        ValidateDuplicateResourceTypes(cardData, containerData, warnings);

        bool actsAsCurrencyContainer = cardData.isCurrency && cardData.currencyType != CurrencyType.None;
        if (actsAsCurrencyContainer)
        {
            if (containerData.listedCardTypes != null && containerData.listedCardTypes.Count > 0)
            {
                warnings.Add(
                    $"Currency container '{cardData.name}' has listedCardTypes configured, but currency containers ignore general card-type filters.");
            }

            if (containerData.useResourceTypeFilter)
            {
                warnings.Add(
                    $"Currency container '{cardData.name}' has useResourceTypeFilter enabled, but currency containers do not use resource-type filtering.");
            }
        }

        if (containerData.listedCardTypes != null && containerData.listedCardTypes.Contains(CardType.Container))
        {
            warnings.Add(
                $"Container card '{cardData.name}' lists CardType.Container in its filter, but containers cannot store other containers anyway.");
        }

        if (!containerData.useResourceTypeFilter)
            return;

        bool allowsResourcesAtCardTypeLevel = AllowsResourcesAtCardTypeLevel(containerData);
        if (!allowsResourcesAtCardTypeLevel)
        {
            warnings.Add(
                $"Container card '{cardData.name}' enables resource-type filtering, but the base card-type filter does not allow resources to enter.");
        }

        if (containerData.resourceListMode == ContainerListMode.AllowOnlyListed
            && (containerData.listedResourceTypes == null || containerData.listedResourceTypes.Count == 0))
        {
            warnings.Add(
                $"Container card '{cardData.name}' uses AllowOnlyListed for resourceListMode but has no listedResourceTypes, so no resources will pass the subtype filter.");
        }
    }

    private static void ValidateDuplicateCardTypes(CardData cardData, ContainerCardData containerData, List<string> warnings)
    {
        if (containerData.listedCardTypes == null)
            return;

        HashSet<CardType> seen = new HashSet<CardType>();
        for (int i = 0; i < containerData.listedCardTypes.Count; i++)
        {
            CardType cardType = containerData.listedCardTypes[i];
            if (!seen.Add(cardType))
            {
                warnings.Add(
                    $"Container card '{cardData.name}' repeats listed card type '{cardType}'.");
            }
        }
    }

    private static void ValidateDuplicateResourceTypes(CardData cardData, ContainerCardData containerData, List<string> warnings)
    {
        if (containerData.listedResourceTypes == null)
            return;

        HashSet<ResourceType> seen = new HashSet<ResourceType>();
        for (int i = 0; i < containerData.listedResourceTypes.Count; i++)
        {
            ResourceType resourceType = containerData.listedResourceTypes[i];
            if (!seen.Add(resourceType))
            {
                warnings.Add(
                    $"Container card '{cardData.name}' repeats listed resource type '{resourceType}'.");
            }
        }
    }

    private static bool AllowsResourcesAtCardTypeLevel(ContainerCardData containerData)
    {
        bool resourceIsListed = containerData.listedCardTypes != null
            && containerData.listedCardTypes.Contains(CardType.Resource);

        switch (containerData.listMode)
        {
            case ContainerListMode.AllowOnlyListed:
                return resourceIsListed;

            case ContainerListMode.BlockListed:
            default:
                return !resourceIsListed;
        }
    }

    private static void ValidateAttackDamageTypes(CardData cardData, CombatantCardData unitData, List<string> warnings)
    {
        if (unitData.attackDamageTypes == null)
            return;

        HashSet<DamageType> seen = new HashSet<DamageType>();
        for (int i = 0; i < unitData.attackDamageTypes.Count; i++)
        {
            DamageType damageType = unitData.attackDamageTypes[i];
            if (!seen.Add(damageType))
            {
                warnings.Add(
                    $"Combatant card '{cardData.name}' repeats attack damage type '{damageType}'.");
            }
        }
    }

    private static void ValidateDamageModifierEntries(CardData cardData, CombatantCardData unitData, List<string> warnings)
    {
        if (unitData.receivedDamageModifiers == null)
            return;

        HashSet<DamageType> seen = new HashSet<DamageType>();
        for (int i = 0; i < unitData.receivedDamageModifiers.Count; i++)
        {
            DamageTypeModifierEntry entry = unitData.receivedDamageModifiers[i];
            if (entry == null)
            {
                warnings.Add($"Combatant card '{cardData.name}' contains a null receivedDamageModifier entry.");
                continue;
            }

            if (!seen.Add(entry.damageType))
            {
                warnings.Add(
                    $"Combatant card '{cardData.name}' repeats received damage modifier for '{entry.damageType}'.");
            }

            if (entry.percentModifier < -1f)
            {
                warnings.Add(
                    $"Combatant card '{cardData.name}' uses percentModifier '{entry.percentModifier}' for '{entry.damageType}', but modifiers below -1 would invert or heal damage.");
            }
        }
    }

    private static void ValidateEnemyGuaranteedDrops(CardData cardData, EnemyCardData enemyData, List<string> warnings)
    {
        if (enemyData.guaranteedDrops == null)
            return;

        for (int i = 0; i < enemyData.guaranteedDrops.Count; i++)
        {
            EnemyGuaranteedDropEntry entry = enemyData.guaranteedDrops[i];
            if (entry == null)
            {
                warnings.Add($"Enemy card '{cardData.name}' contains a null guaranteed drop entry.");
                continue;
            }

            ValidateEnemyDropRange(cardData, "guaranteed", entry.card, entry.minCount, entry.maxCount, warnings);
        }
    }

    private static void ValidateEnemyRandomDrops(CardData cardData, EnemyCardData enemyData, List<string> warnings)
    {
        if (enemyData.randomDrops == null)
            return;

        for (int i = 0; i < enemyData.randomDrops.Count; i++)
        {
            EnemyRandomDropEntry entry = enemyData.randomDrops[i];
            if (entry == null)
            {
                warnings.Add($"Enemy card '{cardData.name}' contains a null random drop entry.");
                continue;
            }

            ValidateEnemyDropRange(cardData, "random", entry.card, entry.minCount, entry.maxCount, warnings);

            if (entry.dropChance < 0f || entry.dropChance > 1f)
            {
                warnings.Add(
                    $"Enemy card '{cardData.name}' uses random drop chance '{entry.dropChance}' outside the [0,1] range.");
            }
        }
    }

    private static void ValidateEnemyDropRange(CardData cardData, string label, CardData dropCard, int minCount, int maxCount, List<string> warnings)
    {
        if (dropCard == null)
        {
            warnings.Add($"Enemy card '{cardData.name}' has a {label} drop entry with no card assigned.");
        }

        if (minCount <= 0)
        {
            warnings.Add($"Enemy card '{cardData.name}' has a {label} drop entry with minCount '{minCount}', but it should be greater than 0.");
        }

        if (maxCount <= 0)
        {
            warnings.Add($"Enemy card '{cardData.name}' has a {label} drop entry with maxCount '{maxCount}', but it should be greater than 0.");
        }

        if (maxCount < minCount)
        {
            warnings.Add($"Enemy card '{cardData.name}' has a {label} drop entry where maxCount '{maxCount}' is smaller than minCount '{minCount}'.");
        }
    }

    private static CardData FindDuplicateId(CardData source)
    {
        string sourcePath = AssetDatabase.GetAssetPath(source);
        string[] guids = AssetDatabase.FindAssets("t:CardData");

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (path == sourcePath)
                continue;

            CardData other = AssetDatabase.LoadAssetAtPath<CardData>(path);
            if (other == null)
                continue;

            if (string.Equals(other.id, source.id))
                return other;
        }

        return null;
    }
#endif
}
