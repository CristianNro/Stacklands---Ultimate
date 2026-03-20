using System.Collections.Generic;
using UnityEngine;

// ============================================================
// RecipeSystem
// ------------------------------------------------------------
// Responsabilidad de esta etapa:
// - encontrar stacks existentes
// - suscribirse a sus cambios
// - revisar si un stack matchea con alguna receta
// - ordenarle al stack arrancar o frenar crafting
//
// IMPORTANTE:
// En esta etapa el stack todavía EJECUTA el crafting.
// RecipeSystem solo DECIDE si corresponde o no.
// ============================================================
public class RecipeSystem : MonoBehaviour
{
    // Guardamos referencia a los stacks suscriptos para no suscribir dos veces.
    private readonly HashSet<CardStack> subscribedStacks = new HashSet<CardStack>();

    private void Start()
    {
        // Al arrancar la escena, buscamos stacks que ya existan.
        RegisterAllExistingStacks();
    }

    private void Update()
    {
        // Por ahora usamos este barrido simple para detectar stacks nuevos.
        // No es lo ideal final, pero es estable para esta etapa.
        RegisterAllExistingStacks();
    }

    /// <summary>
    /// Busca todos los CardStack activos en escena y se suscribe
    /// a los que todavía no estén registrados.
    /// </summary>
    private void RegisterAllExistingStacks()
    {
        CardStack[] allStacks = FindObjectsByType<CardStack>(FindObjectsSortMode.None);

        foreach (CardStack stack in allStacks)
        {
            if (stack == null) continue;
            if (subscribedStacks.Contains(stack)) continue;

            SubscribeToStack(stack);

            // Apenas lo registramos, evaluamos su estado actual.
            EvaluateStack(stack);
        }
    }

    /// <summary>
    /// Se suscribe al evento de cambio del stack.
    /// </summary>
    private void SubscribeToStack(CardStack stack)
    {
        if (stack == null) return;
        if (subscribedStacks.Contains(stack)) return;

        stack.OnStackChanged += HandleStackChanged;
        subscribedStacks.Add(stack);
    }

    /// <summary>
    /// Cuando cambia un stack, revaluamos su receta.
    /// </summary>
    private void HandleStackChanged(CardStack stack)
    {
        EvaluateStack(stack);
    }

    /// <summary>
    /// Decide si un stack actual:
    /// - debe empezar crafting
    /// - debe seguir igual
    /// - debe cancelar crafting
    /// </summary>
    private void EvaluateStack(CardStack stack)
    {
        if (stack == null) return;

        // ---------------------------------------------------------
        // Si el stack quedó trivial, cancelamos cualquier tarea.
        // ---------------------------------------------------------
        if (stack.IsEmpty() || stack.HasOnlyOneCard())
        {
            if (TaskSystem.Instance != null)
                TaskSystem.Instance.CancelTaskForStack(stack);

            return;
        }

        // ---------------------------------------------------------
        // 1. PRIORIDAD MÁXIMA: receta NORMAL
        // ---------------------------------------------------------
        if (RecipeDatabase.Instance != null)
        {
            List<CardData> stackData = stack.GetCardDataList();
            RecipeData recipe = RecipeDatabase.Instance.FindRecipe(stackData);

            if (recipe != null)
            {
                // Validación genérica de requisitos por tags
                if (recipe.HasTagRequirements())
                {
                    for (int i = 0; i < recipe.tagRequirements.Count; i++)
                    {
                        RecipeTagRequirement requirement = recipe.tagRequirements[i];
                        if (requirement == null) continue;
                        if (string.IsNullOrWhiteSpace(requirement.tag)) continue;

                        int countInStack = stack.CountCardsWithTag(requirement.tag);

                        if (countInStack < requirement.minCount)
                        {
                            if (TaskSystem.Instance != null)
                                TaskSystem.Instance.CancelTaskForStack(stack);

                            return;
                        }
                    }
                }

                // Si pasó las validaciones, arrancamos o mantenemos la tarea NORMAL
                if (TaskSystem.Instance != null)
                    TaskSystem.Instance.StartOrRefreshRecipeTask(stack, recipe);

                return;
            }
        }
        else
        {
            Debug.LogWarning("RecipeSystem: no existe RecipeDatabase.Instance.");
        }

        // ---------------------------------------------------------
        // 2. Si NO hay receta normal, intentamos BATCH recipe
        // ---------------------------------------------------------
        if (BatchRecipeDatabase.Instance != null)
        {
            BatchRecipeData batchRecipe = BatchRecipeDatabase.Instance.FindBatchRecipe(stack);

            if (batchRecipe != null)
            {
                if (TaskSystem.Instance != null)
                    TaskSystem.Instance.StartBatchTask(stack, batchRecipe);

                return;
            }
        }

        // ---------------------------------------------------------
        // 3. No matcheó nada → cancelar cualquier tarea del stack
        // ---------------------------------------------------------
        if (TaskSystem.Instance != null)
            TaskSystem.Instance.CancelTaskForStack(stack);
    }
    
    private void OnDestroy()
    {
        foreach (CardStack stack in subscribedStacks)
        {
            if (stack != null)
                stack.OnStackChanged -= HandleStackChanged;
        }

        subscribedStacks.Clear();
    }
}