using System.Collections.Generic;
using UnityEngine;

// ============================================================
// TaskSystem
// ------------------------------------------------------------
// Mantiene tareas activas de recetas.
// La receta define si el flujo es single-shot o repetible.
// ============================================================
public class TaskSystem : MonoBehaviour
{
    public static TaskSystem Instance { get; private set; }

    private readonly List<RecipeTask> activeTasks = new List<RecipeTask>();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (activeTasks.Count == 0)
            return;

        for (int i = activeTasks.Count - 1; i >= 0; i--)
        {
            RecipeTask task = activeTasks[i];

            if (task == null || task.stack == null || task.recipe == null)
            {
                activeTasks.RemoveAt(i);
                continue;
            }

            if (task.stack.IsEmpty() || task.stack.HasOnlyOneCard())
            {
                task.stack.StopCraftingVisuals();
                activeTasks.RemoveAt(i);
                continue;
            }

            // Las recetas repetibles deben seguir validando su loop.
            if (task.recipe.IsRepeatable() && !task.recipe.MatchesStack(task.stack))
            {
                CancelTaskForStack(task.stack);
                continue;
            }

            task.remainingTime -= Time.deltaTime;
            task.stack.SetCraftingProgress(task.GetProgress01());

            if (task.remainingTime > 0f)
                continue;

            if (task.recipe.IsRepeatable())
            {
                ExecuteRepeatableCycle(task);

                if (!activeTasks.Contains(task))
                    continue;
            }
            else
            {
                CardStack completedStack = task.stack;
                RecipeData completedRecipe = task.recipe;

                StackCraftingExecutor.CompleteRecipe(completedStack, completedRecipe);
                activeTasks.Remove(task);
            }
        }
    }

    public void StartOrRefreshRecipeTask(CardStack stack, RecipeData recipe)
    {
        if (stack == null || recipe == null)
            return;

        RecipeTask existingTask = FindTaskForStack(stack);

        // Si la tarea sigue siendo exactamente la misma receta,
        // la dejamos correr para no resetear el progreso cada frame.
        if (existingTask != null && existingTask.recipe == recipe)
            return;

        if (existingTask != null)
            CancelTaskForStack(stack);

        RecipeTask newTask = new RecipeTask(stack, recipe);
        activeTasks.Add(newTask);

        stack.StartCraftingVisuals(recipe);
    }

    public void CancelTaskForStack(CardStack stack)
    {
        if (stack == null)
            return;

        for (int i = activeTasks.Count - 1; i >= 0; i--)
        {
            if (activeTasks[i].stack == stack)
                activeTasks.RemoveAt(i);
        }

        stack.StopCraftingVisuals();
    }

    public bool HasTaskForStack(CardStack stack)
    {
        return FindTaskForStack(stack) != null;
    }

    public RecipeTask FindTaskForStack(CardStack stack)
    {
        if (stack == null) return null;

        for (int i = 0; i < activeTasks.Count; i++)
        {
            if (activeTasks[i].stack == stack)
                return activeTasks[i];
        }

        return null;
    }

    private void ExecuteRepeatableCycle(RecipeTask task)
    {
        CardStack stack = task.stack;
        RecipeData recipe = task.recipe;

        if (stack == null || recipe == null)
            return;

        StackCraftingExecutor.ExecuteRepeatableCycle(stack, recipe);

        task.totalTime = recipe.GetCraftTime();
        task.remainingTime = task.totalTime;

        if (!recipe.MatchesStack(stack))
            CancelTaskForStack(stack);
    }
}
