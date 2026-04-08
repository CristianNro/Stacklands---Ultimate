// ============================================================
// RecipeTaskService
// ------------------------------------------------------------
// Fachada especifica para tareas de recetas. Consume el
// scheduler generico (`TaskSystem`) y encapsula el arranque,
// refresh, busqueda y cancelacion de tareas asociadas a stacks.
// ============================================================
public static class RecipeTaskService
{
    public static void StartOrRefreshTask(CardStack stack, RecipeData recipe)
    {
        if (stack == null || recipe == null || TaskSystem.Instance == null)
            return;

        RecipeTask existingTask = FindTaskForStack(stack);

        if (RecipeTaskCoordinator.TryRefreshExistingTask(existingTask, stack, recipe))
            return;

        if (existingTask != null)
            CancelTaskForStack(stack);

        RecipeTask newTask = RecipeTaskCoordinator.CreateTask(stack, recipe);
        if (newTask == null)
            return;

        TaskSystem.Instance.AddTask(newTask);
    }

    public static void CancelTaskForStack(CardStack stack)
    {
        if (stack == null || TaskSystem.Instance == null)
            return;

        TaskSystem.Instance.CancelTasksByOwner(stack);
    }

    public static bool HasTaskForStack(CardStack stack)
    {
        return FindTaskForStack(stack) != null;
    }

    public static RecipeTask FindTaskForStack(CardStack stack)
    {
        if (stack == null || TaskSystem.Instance == null)
            return null;

        return TaskSystem.Instance.FindTaskByOwner<RecipeTask>(stack);
    }
}
