using System.Collections.Generic;
using UnityEngine;

// ============================================================
// TaskSystem
// ------------------------------------------------------------
// Responsabilidad actual:
// - mantener tareas de crafting activas
// - avanzar su tiempo
// - cancelar tareas inválidas
// - completar tareas terminadas
//
// En esta etapa:
// - RecipeSystem decide qué receta aplica
// - TaskSystem maneja el tiempo
// - CardStack todavía ejecuta el resultado final
// ============================================================
public class TaskSystem : MonoBehaviour
{
    public static TaskSystem Instance { get; private set; }

    // Lista simple de tareas activas.
    // Más adelante podría convertirse en algo más sofisticado.
    private readonly List<RecipeTask> activeTasks = new List<RecipeTask>();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (activeTasks.Count == 0)
            return;

        // Recorremos de atrás hacia adelante para poder remover sin romper índices.
        for (int i = activeTasks.Count - 1; i >= 0; i--)
        {
            RecipeTask task = activeTasks[i];

            // -----------------------------------------------------
            // Validación base:
            // - la tarea debe existir
            // - el stack debe seguir existiendo
            // - debe tener receta normal o batch recipe
            // -----------------------------------------------------
            if (task == null || task.stack == null || (!task.isBatch && task.recipe == null) || (task.isBatch && task.batchRecipe == null))
            {
                activeTasks.RemoveAt(i);
                continue;
            }

            // -----------------------------------------------------
            // Si el stack ya no tiene suficientes cartas para ser stack real,
            // no tiene sentido seguir la tarea.
            // -----------------------------------------------------
            if (task.stack.IsEmpty() || task.stack.HasOnlyOneCard())
            {
                task.stack.StopCraftingVisuals();
                activeTasks.RemoveAt(i);
                continue;
            }

            // -----------------------------------------------------
            // Avance del tiempo
            // -----------------------------------------------------
            task.remainingTime -= Time.deltaTime;

            // Actualizamos la UI del stack
            task.stack.SetCraftingProgress(task.GetProgress01());

            // -----------------------------------------------------
            // Si terminó, completamos la tarea
            // -----------------------------------------------------
            if (task.remainingTime <= 0f)
            {
                // =================================================
                // Caso A: receta BATCH
                // -------------------------------------------------
                // La batch se ejecuta de a un ciclo y puede reiniciarse
                // automáticamente mientras el stack siga siendo batch-válido.
                // =================================================
                if (task.isBatch)
                {
                    ExecuteBatchCycle(task);

                    // Si por alguna razón la tarea fue cancelada dentro de ExecuteBatchCycle,
                    // puede que ya no exista en activeTasks. Verificamos antes de seguir.
                    if (!activeTasks.Contains(task))
                        continue;
                }
                // =================================================
                // Caso B: receta NORMAL
                // -------------------------------------------------
                // La receta normal se ejecuta UNA sola vez y NO se reinicia sola.
                // =================================================
                else
                {
                    CardStack completedStack = task.stack;

                    // Completa la receta normal una sola vez
                    completedStack.CompleteRecipeFromTask(task.recipe);

                    // Removemos la tarea terminada
                    activeTasks.RemoveAt(i);

                    // IMPORTANTE:
                    // Las recetas normales NO se reinician automáticamente.
                    // Si el jugador quiere volver a producir, debe volver a apilar/interactuar.
                }
            }
        }
    }

    /// <summary>
    /// Arranca una tarea nueva o mantiene la existente si ya coincide
    /// con el mismo stack y la misma receta.
    /// </summary>
    public void StartOrRefreshRecipeTask(CardStack stack, RecipeData recipe)
    {
        if (stack == null || recipe == null)
            return;

        RecipeTask existingTask = FindTaskForStack(stack);

        // Si ya existe una tarea para ese stack y es la misma receta,
        // no la reiniciamos.
        if (existingTask != null && existingTask.recipe == recipe)
        {
            return;
        }

        // Si había otra receta corriendo en ese stack, la cancelamos primero.
        if (existingTask != null)
        {
            CancelTaskForStack(stack);
        }

        RecipeTask newTask = new RecipeTask(stack, recipe);
        activeTasks.Add(newTask);

        // Le decimos al stack que muestre la UI de crafting.
        stack.StartCraftingVisuals(recipe);
    }

    /// <summary>
    /// Cancela la tarea activa de un stack, si existe.
    /// </summary>
    public void CancelTaskForStack(CardStack stack)
    {
        if (stack == null)
            return;

        for (int i = activeTasks.Count - 1; i >= 0; i--)
        {
            if (activeTasks[i].stack == stack)
            {
                activeTasks.RemoveAt(i);
            }
        }

        stack.StopCraftingVisuals();
    }

    public void StartBatchTask(CardStack stack, BatchRecipeData batch)
    {
        if (stack == null || batch == null)
            return;

        RecipeTask existing = FindTaskForStack(stack);

        if (existing != null && existing.isBatch && existing.batchRecipe == batch)
            return;

        if (existing != null)
            CancelTaskForStack(stack);

        RecipeTask task = new RecipeTask(stack, null);
        task.batchRecipe = batch;
        task.isBatch = true;
        task.totalTime = batch.craftTimePerCycle;
        task.remainingTime = task.totalTime;

        activeTasks.Add(task);

        stack.StartCraftingVisuals(null);
    }

    /// <summary>
    /// Devuelve true si el stack tiene una tarea activa.
    /// </summary>
    public bool HasTaskForStack(CardStack stack)
    {
        return FindTaskForStack(stack) != null;
    }

    /// <summary>
    /// Busca la tarea activa de un stack.
    /// </summary>
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

    private void ExecuteBatchCycle(RecipeTask task)
    {
        var stack = task.stack;
        var batch = task.batchRecipe;

        if (stack == null || batch == null)
            return;

        // 1. buscar UNA carta repetible
        CardInstance target = null;

        var cards = stack.Cards;

        for (int i = 0; i < cards.Count; i++)
        {
            var inst = cards[i].GetComponent<CardInstance>();
            if (inst != null && inst.HasTag(batch.repeatableTag))
            {
                target = inst;
                break;
            }
        }

        if (target == null)
        {
            CancelTaskForStack(stack);
            return;
        }

        // 2. consumir ESA carta (solo una)
        stack.RemoveCard(target.GetComponent<CardView>());
        Destroy(target.gameObject);

        // 3. generar resultado
        var result = batch.RollResult();

        if (result != null && CardSpawner.Instance != null)
        {
            CardSpawner.Instance.SpawnAnimated(result, stack.GetStackPosition());
        }

        // 4. resetear timer (loop)
        task.remainingTime = task.totalTime;

        // 5. revalidar stack
        if (!batch.MatchesStack(stack))
        {
            CancelTaskForStack(stack);
        }
    }
}