using UnityEngine;
using System.Collections.Generic;
using StacklandsLike.Cards;

#if UNITY_EDITOR
using UnityEditor;
#endif

// ============================================================
// RecipeDatabase
// ------------------------------------------------------------
// Base de recetas unificada.
// Carga todos los RecipeData del proyecto y elige una coincidencia
// deterministica para cada stack.
// ============================================================
public class RecipeDatabase : MonoBehaviour
{
    [SerializeField] private RecipeData[] recipes;

    public static RecipeDatabase Instance { get; private set; }

    private void Awake()
    {
#if UNITY_EDITOR
        LoadRecipesFromProjectIfAvailable();
#endif
        ValidateRecipeDefinitions(recipes);
        Instance = this;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        LoadRecipesFromProjectIfAvailable();
    }

    private void LoadRecipesFromProjectIfAvailable()
    {
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/Recipes" });
        List<RecipeData> loadedRecipes = new List<RecipeData>();

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            RecipeData recipe = AssetDatabase.LoadAssetAtPath<RecipeData>(path);

            if (recipe != null && !loadedRecipes.Contains(recipe))
                loadedRecipes.Add(recipe);
        }

        loadedRecipes.Sort(CompareRecipesByStableOrder);
        ValidateRecipeDefinitions(loadedRecipes);

        if (recipes != null && recipes.Length == loadedRecipes.Count)
        {
            bool sameContent = true;

            for (int i = 0; i < loadedRecipes.Count; i++)
            {
                if (recipes[i] != loadedRecipes[i])
                {
                    sameContent = false;
                    break;
                }
            }

            if (sameContent)
                return;
        }

        recipes = loadedRecipes.ToArray();
        EditorUtility.SetDirty(this);
    }

    private int CompareRecipesByStableOrder(RecipeData a, RecipeData b)
    {
        if (a == null && b == null) return 0;
        if (a == null) return 1;
        if (b == null) return -1;

        string idA = !string.IsNullOrWhiteSpace(a.id) ? a.id : a.name;
        string idB = !string.IsNullOrWhiteSpace(b.id) ? b.id : b.name;

        int idCompare = string.CompareOrdinal(idA, idB);
        if (idCompare != 0)
            return idCompare;

        return string.CompareOrdinal(a.name, b.name);
    }
#endif

    private void ValidateRecipeDefinitions(IReadOnlyList<RecipeData> sourceRecipes)
    {
        if (sourceRecipes == null)
            return;

        Dictionary<string, RecipeData> byId = new Dictionary<string, RecipeData>();
        Dictionary<string, RecipeData> bySignature = new Dictionary<string, RecipeData>();
        List<RecipeData> validRecipes = new List<RecipeData>();

        for (int i = 0; i < sourceRecipes.Count; i++)
        {
            RecipeData recipe = sourceRecipes[i];
            if (recipe == null)
                continue;

            if (!recipe.IsValidForDatabase(out string validationError))
            {
                Debug.LogWarning($"[RecipeDatabase] Invalid recipe '{recipe.name}': {validationError}");
                continue;
            }

            if (byId.TryGetValue(recipe.id, out RecipeData existingById))
            {
                Debug.LogWarning(
                    $"[RecipeDatabase] Duplicate recipe id '{recipe.id}'. Conflicting assets: '{existingById.name}' and '{recipe.name}'.");
            }
            else
            {
                byId[recipe.id] = recipe;
            }

            string signature = recipe.BuildUniquenessSignature();
            if (bySignature.TryGetValue(signature, out RecipeData existingBySignature))
            {
                Debug.LogWarning(
                    "[RecipeDatabase] Duplicate recipe components detected. " +
                    $"Assets '{existingBySignature.name}' and '{recipe.name}' share the same matching signature: {signature}");
            }
            else
            {
                bySignature[signature] = recipe;
            }

            ValidateIngredientRuleMigration(recipe);
            validRecipes.Add(recipe);
        }

        for (int i = 0; i < validRecipes.Count; i++)
        {
            for (int j = i + 1; j < validRecipes.Count; j++)
                WarnIfExactRecipeOverlap(validRecipes[i], validRecipes[j]);
        }
    }

    private void ValidateIngredientRuleMigration(RecipeData recipe)
    {
        if (recipe == null || recipe.ingredientRules == null)
            return;

        for (int i = 0; i < recipe.ingredientRules.Count; i++)
        {
            RecipeIngredientRule rule = recipe.ingredientRules[i];
            if (rule == null)
                continue;

            if (rule.card == null)
            {
                Debug.LogWarning(
                    $"[RecipeDatabase] Recipe '{recipe.name}' has an ingredient rule without a CardData reference.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(rule.card.id))
            {
                Debug.LogWarning(
                    $"[RecipeDatabase] Recipe '{recipe.name}' has an ingredient rule referencing card '{rule.card.name}' without a valid CardData.id.");
            }
        }
    }

    private void WarnIfExactRecipeOverlap(RecipeData a, RecipeData b)
    {
        if (a == null || b == null)
            return;

        if (a.matchMode != RecipeMatchMode.ExactIngredients || b.matchMode != RecipeMatchMode.ExactIngredients)
            return;

        if (a.BuildCapabilityRequirementsSignatureForDatabase() != b.BuildCapabilityRequirementsSignatureForDatabase())
            return;

        Dictionary<string, RecipeIngredientRule> aRules = BuildExactRuleMap(a.GetExactIngredientRequirementsSnapshot());
        Dictionary<string, RecipeIngredientRule> bRules = BuildExactRuleMap(b.GetExactIngredientRequirementsSnapshot());

        if (aRules.Count == 0 || bRules.Count == 0 || aRules.Count != bRules.Count)
            return;

        foreach (KeyValuePair<string, RecipeIngredientRule> pair in aRules)
        {
            if (!bRules.ContainsKey(pair.Key))
                return;
        }

        foreach (KeyValuePair<string, RecipeIngredientRule> pair in aRules)
        {
            RecipeIngredientRule aRule = pair.Value;
            RecipeIngredientRule bRule = bRules[pair.Key];

            int aMin = aRule.GetNormalizedRequiredCount();
            int bMin = bRule.GetNormalizedRequiredCount();
            int aMax = aRule.allowAdditionalCopies ? int.MaxValue : aMin;
            int bMax = bRule.allowAdditionalCopies ? int.MaxValue : bMin;

            if (Mathf.Max(aMin, bMin) > Mathf.Min(aMax, bMax))
                return;
        }

        Debug.LogWarning(
            "[RecipeDatabase] Potential exact-recipe overlap detected. " +
            $"Recipes '{a.name}' and '{b.name}' can both match some of the same exact-ingredient stacks.");
    }

    private Dictionary<string, RecipeIngredientRule> BuildExactRuleMap(List<RecipeIngredientRule> rules)
    {
        Dictionary<string, RecipeIngredientRule> result = new Dictionary<string, RecipeIngredientRule>();
        if (rules == null)
            return result;

        for (int i = 0; i < rules.Count; i++)
        {
            RecipeIngredientRule rule = rules[i];
            if (rule == null)
                continue;

            string cardId = rule.GetIngredientCardId();
            if (string.IsNullOrWhiteSpace(cardId) || result.ContainsKey(cardId))
                continue;

            result[cardId] = rule;
        }

        return result;
    }

    public RecipeSelectionResult EvaluateStack(CardStack stack)
    {
        return EvaluateInput(RecipeMatchInput.FromStack(stack));
    }

    public RecipeSelectionResult EvaluateInput(RecipeMatchInput input)
    {
        CardStack stack = input != null ? input.sourceStack : null;

        RecipeSelectionResult selection = new RecipeSelectionResult
        {
            stack = stack,
            input = input,
            selectionReason = "No recipes matched."
        };

        if (input == null || stack == null)
        {
            selection.selectionReason = input == null ? "Recipe match input is null." : "Stack is null.";
            return selection;
        }

        RecipeData firstMatch = null;
        RecipeData fallbackBestMatch = null;
        RecipeMatchResult firstMatchResult = null;
        RecipeMatchResult fallbackBestMatchResult = null;
        bool foundMultipleMatches = false;

        for (int i = 0; i < recipes.Length; i++)
        {
            RecipeData recipe = recipes[i];
            if (recipe == null) continue;

            RecipeMatchResult matchResult = recipe.EvaluateMatch(input);
            if (matchResult == null || !matchResult.matched)
                continue;

            selection.matchingResults.Add(matchResult);

            if (firstMatch == null)
            {
                firstMatch = recipe;
                fallbackBestMatch = recipe;
                firstMatchResult = matchResult;
                fallbackBestMatchResult = matchResult;
                continue;
            }

            foundMultipleMatches = true;

            if (recipe.GetSpecificityScore() > fallbackBestMatch.GetSpecificityScore())
            {
                fallbackBestMatch = recipe;
                fallbackBestMatchResult = matchResult;
            }
        }

        if (!foundMultipleMatches)
        {
            selection.selectedMatch = firstMatchResult;
            selection.selectionReason = firstMatchResult != null
                ? $"Single valid recipe matched. Winner: '{firstMatchResult.recipe.name}'. Reason: {firstMatchResult.reason}"
                : "No recipes matched.";
            return selection;
        }

        string stackName = stack != null ? stack.name : "<null>";
        selection.selectedMatch = fallbackBestMatchResult;
        selection.selectionReason =
            $"Problematic overlap on stack '{stackName}'. " +
            $"Winner: '{fallbackBestMatch.name}'. " +
            $"Reason: fallback specificity among {selection.matchingResults.Count} valid matches.";

        return selection;
    }

    public RecipeData FindRecipe(CardStack stack)
    {
        RecipeSelectionResult selection = EvaluateStack(stack);
        return selection != null ? selection.SelectedRecipe : null;
    }
}
