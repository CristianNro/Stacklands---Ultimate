// ============================================================
// ITimedTask
// ------------------------------------------------------------
// Contrato minimo para tareas temporales administradas por
// TaskSystem. Permite separar el scheduler del tipo concreto
// de tarea sin introducir todavia un framework mas grande.
// ============================================================
public interface ITimedTask
{
    bool IsRunning();
    bool IsPaused();
    bool IsFinished();
    bool Tick(float deltaTime);
    bool IsOwnedBy(object owner);
    void Pause();
    void Resume();
    void Cancel();
}
