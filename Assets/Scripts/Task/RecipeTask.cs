using UnityEngine;

// ============================================================
// RecipeTask
// ------------------------------------------------------------
// Representa una tarea activa de crafting asociada a un stack.
// Esta clase NO es MonoBehaviour.
// Es solo runtime data que TaskSystem administra.
// ============================================================
[System.Serializable]
public class RecipeTask
{
    // Stack sobre el que corre la tarea
    public CardStack stack;

    // Receta que se está ejecutando
    public RecipeData recipe;

    // Tiempo total original
    public float totalTime;

    // Tiempo restante actual
    public float remainingTime;

    public BatchRecipeData batchRecipe;
    public bool isBatch;

    public RecipeTask(CardStack stack, RecipeData recipe)
    {
        this.stack = stack;
        this.recipe = recipe;

        totalTime = Mathf.Max(0.01f, recipe.craftTime);
        remainingTime = totalTime;
    }

    /// <summary>
    /// Devuelve progreso normalizado entre 0 y 1.
    /// 0 = recién empieza
    /// 1 = terminada
    /// </summary>
    public float GetProgress01()
    {
        if (totalTime <= 0f) return 1f;
        return Mathf.Clamp01(1f - (remainingTime / totalTime));
    }
}