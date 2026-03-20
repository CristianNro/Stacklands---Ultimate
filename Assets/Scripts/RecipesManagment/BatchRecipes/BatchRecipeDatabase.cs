using System.Collections.Generic;
using UnityEngine;

public class BatchRecipeDatabase : MonoBehaviour
{
    public static BatchRecipeDatabase Instance { get; private set; }

    [SerializeField] private List<BatchRecipeData> recipes = new List<BatchRecipeData>();

    private void Awake()
    {
        Instance = this;
    }

    public BatchRecipeData FindBatchRecipe(CardStack stack)
    {
        for (int i = 0; i < recipes.Count; i++)
        {
            var recipe = recipes[i];
            if (recipe == null) continue;

            if (recipe.MatchesStack(stack))
                return recipe;
        }

        return null;
    }
}