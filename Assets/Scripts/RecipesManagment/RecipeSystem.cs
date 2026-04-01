using System.Collections.Generic;
using UnityEngine;

// ============================================================
// RecipeSystem
// ------------------------------------------------------------
// Observa stacks y decide si deben arrancar, mantenerse o
// cancelarse segun la mejor receta disponible.
// ============================================================
public class RecipeSystem : MonoBehaviour
{
    private readonly HashSet<CardStack> subscribedStacks = new HashSet<CardStack>();
    private BoardRoot subscribedBoardRoot;

    private void OnEnable()
    {
        TrySubscribeToBoardRoot();
        RegisterAllExistingStacks();
    }

    private void Update()
    {
        TrySubscribeToBoardRoot();
    }

    private void OnDisable()
    {
        UnsubscribeFromBoardRoot();
        UnsubscribeAllStacks();
    }

    private void TrySubscribeToBoardRoot()
    {
        BoardRoot currentBoardRoot = BoardRoot.Instance;
        if (currentBoardRoot == null || subscribedBoardRoot == currentBoardRoot)
            return;

        UnsubscribeFromBoardRoot();

        subscribedBoardRoot = currentBoardRoot;
        subscribedBoardRoot.OnStackRegistered += HandleStackCreated;
        subscribedBoardRoot.OnStackUnregistered += HandleStackDestroyed;

        RegisterAllExistingStacks();
    }

    private void UnsubscribeFromBoardRoot()
    {
        if (subscribedBoardRoot == null)
            return;

        subscribedBoardRoot.OnStackRegistered -= HandleStackCreated;
        subscribedBoardRoot.OnStackUnregistered -= HandleStackDestroyed;
        subscribedBoardRoot = null;
    }

    private void RegisterAllExistingStacks()
    {
        if (BoardRoot.Instance == null)
        {
            CardStack[] allStacks = FindObjectsByType<CardStack>(FindObjectsSortMode.None);

            for (int i = 0; i < allStacks.Length; i++)
                HandleStackCreated(allStacks[i]);

            return;
        }

        IReadOnlyList<CardStack> activeStacks = BoardRoot.Instance.ActiveStacks;
        for (int i = 0; i < activeStacks.Count; i++)
            HandleStackCreated(activeStacks[i]);
    }

    private void HandleStackCreated(CardStack stack)
    {
        if (stack == null || subscribedStacks.Contains(stack))
            return;

        stack.OnStackChanged += HandleStackChanged;
        subscribedStacks.Add(stack);
        EvaluateStack(stack);
    }

    private void HandleStackDestroyed(CardStack stack)
    {
        if (stack == null || !subscribedStacks.Contains(stack))
            return;

        stack.OnStackChanged -= HandleStackChanged;
        subscribedStacks.Remove(stack);

        if (TaskSystem.Instance != null)
            TaskSystem.Instance.CancelTaskForStack(stack);
    }

    private void HandleStackChanged(CardStack stack)
    {
        EvaluateStack(stack);
    }

    private void EvaluateStack(CardStack stack)
    {
        if (stack == null) return;

        if (stack.IsEmpty() || stack.HasOnlyOneCard())
        {
            if (TaskSystem.Instance != null)
                TaskSystem.Instance.CancelTaskForStack(stack);

            return;
        }

        if (RecipeDatabase.Instance == null)
        {
            Debug.LogWarning("RecipeSystem: no existe RecipeDatabase.Instance.");
            return;
        }

        // La base ya devuelve la receta mas especifica para este stack.
        RecipeData recipe = RecipeDatabase.Instance.FindRecipe(stack);

        if (recipe != null)
        {
            if (TaskSystem.Instance != null)
                TaskSystem.Instance.StartOrRefreshRecipeTask(stack, recipe);

            return;
        }

        if (TaskSystem.Instance != null)
            TaskSystem.Instance.CancelTaskForStack(stack);
    }

    private void UnsubscribeAllStacks()
    {
        foreach (CardStack stack in subscribedStacks)
        {
            if (stack != null)
                stack.OnStackChanged -= HandleStackChanged;
        }

        subscribedStacks.Clear();
    }
}
