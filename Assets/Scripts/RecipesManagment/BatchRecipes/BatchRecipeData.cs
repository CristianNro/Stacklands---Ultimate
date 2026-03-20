using System.Collections.Generic;
using UnityEngine;

// ============================================================
// BatchRecipeData
// ------------------------------------------------------------
// Receta que procesa stacks homogéneos de forma repetitiva.
//
// Ejemplo:
// Villager + Tree + Tree + Tree
// → procesa 1 Tree por ciclo
// ============================================================
[CreateAssetMenu(menuName = "Stacklands/Batch Recipe Data")]
public class BatchRecipeData : ScriptableObject
{
    [Header("Info")]
    public string id;
    public string displayName;

    [Header("Timing")]
    public float craftTimePerCycle = 2f;

    [Header("Allowed Tags (todo el stack debe cumplir esto)")]
    public List<string> allowedTags = new List<string>();

    [Header("Required Tags (mínimos para arrancar)")]
    public List<RecipeTagRequirement> requiredTags = new List<RecipeTagRequirement>();

    [Header("Repeatable Target")]
    public string repeatableTag;

    [Header("Results")]
    public List<RecipeResultOption> possibleResults = new List<RecipeResultOption>();

    // =========================================================
    // Helpers
    // =========================================================

    public bool IsCardAllowed(CardData data)
    {
        if (data == null || data.tags == null) return false;

        for (int i = 0; i < allowedTags.Count; i++)
        {
            if (data.tags.Contains(allowedTags[i]))
                return true;
        }

        return false;
    }

    public bool MatchesStack(CardStack stack)
    {
        if (stack == null) return false;

        var cards = stack.GetCardDataList();

        // 1. Validar que todas las cartas sean permitidas
        for (int i = 0; i < cards.Count; i++)
        {
            if (!IsCardAllowed(cards[i]))
                return false;
        }

        // 2. Validar mínimos requeridos
        for (int i = 0; i < requiredTags.Count; i++)
        {
            var req = requiredTags[i];
            if (req == null) continue;

            int count = stack.CountCardsWithTag(req.tag);

            if (count < req.minCount)
                return false;
        }

        // 3. Tiene al menos 1 repetible
        if (string.IsNullOrWhiteSpace(repeatableTag))
            return false;

        if (stack.CountCardsWithTag(repeatableTag) <= 0)
            return false;

        return true;
    }

    public CardData RollResult()
    {
        if (possibleResults == null || possibleResults.Count == 0)
            return null;

        float total = 0f;

        foreach (var r in possibleResults)
        {
            if (r != null && r.result != null && r.weight > 0)
                total += r.weight;
        }

        if (total <= 0f) return null;

        float roll = Random.Range(0f, total);
        float acc = 0f;

        foreach (var r in possibleResults)
        {
            if (r == null || r.result == null || r.weight <= 0) continue;

            acc += r.weight;
            if (roll <= acc)
                return r.result;
        }

        return null;
    }
}