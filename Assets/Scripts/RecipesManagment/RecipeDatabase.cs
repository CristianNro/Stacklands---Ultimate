using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

// ============================================================
// RecipeDatabase
// ------------------------------------------------------------
// Base de recetas unificada.
// Carga todos los RecipeData del proyecto y elige la coincidencia
// mas especifica para cada stack.
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

        loadedRecipes.Sort(CompareRecipesBySpecificity);

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

    private int CompareRecipesBySpecificity(RecipeData a, RecipeData b)
    {
        if (a == null && b == null) return 0;
        if (a == null) return 1;
        if (b == null) return -1;

        int specificityCompare = b.GetSpecificityScore().CompareTo(a.GetSpecificityScore());
        if (specificityCompare != 0)
            return specificityCompare;

        return string.CompareOrdinal(a.name, b.name);
    }
#endif

    public RecipeData FindRecipe(CardStack stack)
    {
        if (stack == null)
            return null;

        RecipeData bestMatch = null;

        for (int i = 0; i < recipes.Length; i++)
        {
            RecipeData recipe = recipes[i];
            if (recipe == null) continue;

            if (!recipe.MatchesStack(stack))
                continue;

            if (bestMatch == null || recipe.GetSpecificityScore() > bestMatch.GetSpecificityScore())
                bestMatch = recipe;
        }

        return bestMatch;
    }
}
