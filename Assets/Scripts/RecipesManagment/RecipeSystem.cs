using System.Collections.Generic;
using UnityEngine;

// ============================================================
// RecipeSystem
// ------------------------------------------------------------
// Observa stacks y decide si deben arrancar, mantenerse o
// cancelarse segun el resultado completo de evaluacion.
// ============================================================
public class RecipeSystem : MonoBehaviour
{
    private readonly HashSet<CardStack> subscribedStacks = new HashSet<CardStack>();
    private BoardRoot subscribedBoardRoot;

    private void OnEnable()
    {
        BoardRoot.OnBoardRootAvailable += HandleBoardRootAvailable;
        TrySubscribeToBoardRoot();
        RegisterAllExistingStacks();
    }

    private void OnDisable()
    {
        BoardRoot.OnBoardRootAvailable -= HandleBoardRootAvailable;
        UnsubscribeFromBoardRoot();
        UnsubscribeAllStacks();
    }

    private void HandleBoardRootAvailable(BoardRoot boardRoot)
    {
        TrySubscribeToBoardRoot();
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

        RecipeTaskService.CancelTaskForStack(stack);
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
            RecipeTaskService.CancelTaskForStack(stack);

            return;
        }

        if (RecipeDatabase.Instance == null)
        {
            Debug.LogWarning("RecipeSystem: no existe RecipeDatabase.Instance.");
            return;
        }

        RecipeSelectionResult selection = RecipeDatabase.Instance.EvaluateStack(stack);
        RecipeData recipe = selection != null ? selection.SelectedRecipe : null;

        if (selection != null && selection.HasProblematicOverlap)
        {
            Debug.LogWarning(
                $"[RecipeSystem] Problematic recipe overlap detected on stack '{stack.name}'. {selection.selectionReason}");
        }

        if (recipe != null)
        {
            RecipeTaskService.StartOrRefreshTask(stack, recipe);

            return;
        }

        RecipeTaskService.CancelTaskForStack(stack);
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
