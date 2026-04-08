using System.Collections.Generic;
using UnityEngine;

// ============================================================
// TaskSystem
// ------------------------------------------------------------
// Scheduler generico minimo para tareas temporales.
// El dominio especifico (recetas u otros) debe vivir
// fuera de este sistema.
// ============================================================
public class TaskSystem : MonoBehaviour
{
    public static TaskSystem Instance { get; private set; }

    private readonly List<ITimedTask> activeTasks = new List<ITimedTask>();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (activeTasks.Count == 0)
            return;

        float deltaTime = GameTimeService.GetTimedSystemsDeltaTime();

        for (int i = activeTasks.Count - 1; i >= 0; i--)
        {
            ITimedTask task = activeTasks[i];

            if (task == null || task.IsFinished())
            {
                RemoveTaskSafely(i, task);
                continue;
            }

            if (deltaTime <= 0f)
            {
                task.Pause();
                continue;
            }

            task.Resume();

            if (!task.Tick(deltaTime))
                RemoveTaskSafely(i, task);
        }
    }

    public void AddTask(ITimedTask task)
    {
        if (task == null)
            return;

        activeTasks.Add(task);
    }

    public void CancelTasksByOwner(object owner)
    {
        if (owner == null)
            return;

        for (int i = activeTasks.Count - 1; i >= 0; i--)
        {
            if (activeTasks[i] != null && activeTasks[i].IsOwnedBy(owner))
            {
                activeTasks[i].Cancel();
                activeTasks.RemoveAt(i);
            }
        }
    }

    public TTask FindTaskByOwner<TTask>(object owner) where TTask : class, ITimedTask
    {
        if (owner == null)
            return null;

        for (int i = 0; i < activeTasks.Count; i++)
        {
            TTask typedTask = activeTasks[i] as TTask;
            if (typedTask != null && typedTask.IsOwnedBy(owner))
                return typedTask;
        }

        return null;
    }

    private void RemoveTaskSafely(int originalIndex, ITimedTask task)
    {
        if (originalIndex >= 0 && originalIndex < activeTasks.Count && ReferenceEquals(activeTasks[originalIndex], task))
        {
            activeTasks.RemoveAt(originalIndex);
            return;
        }

        if (task != null)
            activeTasks.Remove(task);
    }
}
