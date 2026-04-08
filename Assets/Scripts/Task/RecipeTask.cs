using UnityEngine;

// ============================================================
// RecipeTask
// ------------------------------------------------------------
// Estado runtime de una receta activa sobre un stack.
// ============================================================
[System.Serializable]
public class RecipeTask : ITimedTask
{
    public enum RecipeTaskState
    {
        Running,
        Paused,
        Completed,
        Cancelled
    }

    public CardStack stack;
    public RecipeData recipe;
    public float totalTime;
    public float remainingTime;
    public RecipeTaskState state;

    public RecipeTask(CardStack stack, RecipeData recipe)
    {
        this.stack = stack;
        this.recipe = recipe;

        float resolvedCraftTime = recipe != null ? recipe.GetCraftTime(stack) : 0.01f;
        totalTime = Mathf.Max(0.01f, resolvedCraftTime);
        remainingTime = totalTime;
        state = RecipeTaskState.Running;
    }

    public void RefreshResolvedTime(float newTotalTime)
    {
        if (state != RecipeTaskState.Running && state != RecipeTaskState.Paused)
            return;

        float clampedNewTotalTime = Mathf.Max(0.01f, newTotalTime);
        float progress01 = GetProgress01();

        totalTime = clampedNewTotalTime;
        remainingTime = Mathf.Max(0f, totalTime * (1f - progress01));
    }

    public void Advance(float deltaTime)
    {
        if (state != RecipeTaskState.Running)
            return;

        remainingTime -= deltaTime;
    }

    public void Restart(float newTotalTime)
    {
        totalTime = Mathf.Max(0.01f, newTotalTime);
        remainingTime = totalTime;
        state = RecipeTaskState.Running;
    }

    public void MarkCompleted()
    {
        remainingTime = 0f;
        state = RecipeTaskState.Completed;
    }

    public void MarkCancelled()
    {
        state = RecipeTaskState.Cancelled;
    }

    public bool IsRunning()
    {
        return state == RecipeTaskState.Running;
    }

    public bool IsPaused()
    {
        return state == RecipeTaskState.Paused;
    }

    public bool IsFinished()
    {
        return state == RecipeTaskState.Completed || state == RecipeTaskState.Cancelled;
    }

    public bool Tick(float deltaTime)
    {
        return RecipeTaskCoordinator.TickTask(this, deltaTime);
    }

    public bool IsOwnedBy(object owner)
    {
        return ReferenceEquals(stack, owner);
    }

    public void Cancel()
    {
        if (state == RecipeTaskState.Cancelled)
            return;

        MarkCancelled();

        if (stack != null)
            stack.StopCraftingVisuals();
    }

    public void Pause()
    {
        if (state != RecipeTaskState.Running)
            return;

        state = RecipeTaskState.Paused;
    }

    public void Resume()
    {
        if (state != RecipeTaskState.Paused)
            return;

        state = RecipeTaskState.Running;
    }

    public float GetProgress01()
    {
        if (totalTime <= 0f) return 1f;
        return Mathf.Clamp01(1f - (remainingTime / totalTime));
    }
}
