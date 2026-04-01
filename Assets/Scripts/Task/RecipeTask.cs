using UnityEngine;

// ============================================================
// RecipeTask
// ------------------------------------------------------------
// Estado runtime de una receta activa sobre un stack.
// ============================================================
[System.Serializable]
public class RecipeTask
{
    public CardStack stack;
    public RecipeData recipe;
    public float totalTime;
    public float remainingTime;

    public RecipeTask(CardStack stack, RecipeData recipe)
    {
        this.stack = stack;
        this.recipe = recipe;

        float resolvedCraftTime = recipe != null ? recipe.GetCraftTime() : 0.01f;
        totalTime = Mathf.Max(0.01f, resolvedCraftTime);
        remainingTime = totalTime;
    }

    public float GetProgress01()
    {
        if (totalTime <= 0f) return 1f;
        return Mathf.Clamp01(1f - (remainingTime / totalTime));
    }
}
