using UnityEngine;

// ============================================================
// RecipeTaskCoordinator
// ------------------------------------------------------------
// Encapsula la politica especifica de lifecycle de crafting.
// Deja a TaskSystem mas cerca de un scheduler temporal y
// concentra las reglas de inicio, continuidad y finalizacion
// de tareas de recetas en una sola pieza.
// ============================================================
public static class RecipeTaskCoordinator
{
    public static bool TryRefreshExistingTask(RecipeTask existingTask, CardStack stack, RecipeData recipe)
    {
        if (existingTask == null || stack == null || recipe == null)
            return false;

        if (existingTask.stack != stack || existingTask.recipe != recipe)
            return false;

        existingTask.RefreshResolvedTime(recipe.GetCraftTime(stack));
        return true;
    }

    public static RecipeTask CreateTask(CardStack stack, RecipeData recipe)
    {
        if (stack == null || recipe == null)
            return null;

        RecipeTask task = new RecipeTask(stack, recipe);
        stack.StartCraftingVisuals(recipe);
        return task;
    }

    public static bool TickTask(RecipeTask task, float deltaTime)
    {
        if (!CanContinue(task))
            return false;

        task.Advance(deltaTime);
        task.stack.SetCraftingProgress(task.GetProgress01());

        if (task.remainingTime > 0f)
            return true;

        if (task.recipe.IsRepeatable())
            return ExecuteRepeatableCycle(task);

        StackCraftingExecutor.CompleteRecipe(task.stack, task.recipe);
        task.MarkCompleted();
        return false;
    }

    private static bool CanContinue(RecipeTask task)
    {
        if (task == null || !task.IsRunning() || task.stack == null || task.recipe == null)
            return false;

        if (task.stack.IsEmpty() || task.stack.HasOnlyOneCard())
        {
            task.MarkCancelled();
            task.stack.StopCraftingVisuals();
            return false;
        }

        if (task.recipe.IsRepeatable() && !task.recipe.MatchesStack(task.stack))
        {
            task.MarkCancelled();
            task.stack.StopCraftingVisuals();
            return false;
        }

        return true;
    }

    private static bool ExecuteRepeatableCycle(RecipeTask task)
    {
        CardStack stack = task.stack;
        RecipeData recipe = task.recipe;

        if (stack == null || recipe == null)
        {
            if (task != null)
                task.MarkCancelled();

            return false;
        }

        StackCraftingExecutor.ExecuteRepeatableCycle(stack, recipe);

        if (stack == null || stack.IsEmpty() || stack.HasOnlyOneCard())
        {
            task.MarkCancelled();

            if (stack != null)
                stack.StopCraftingVisuals();

            return false;
        }

        task.Restart(recipe.GetCraftTime(stack));

        if (!recipe.MatchesStack(stack))
        {
            task.MarkCancelled();
            stack.StopCraftingVisuals();
            return false;
        }

        stack.SetCraftingProgress(task.GetProgress01());
        return true;
    }
}
