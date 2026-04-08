// ============================================================
// IDayStartProcessor
// ------------------------------------------------------------
// Contrato minimo para sistemas que quieran participar del
// arranque de un nuevo dia.
//
// El coordinador diario ejecuta estos procesadores en orden.
// Mas adelante, `DayEventSystem` / `DayEventExecutor` pueden
// enchufarse aqui sin tocar el reloj base.
// ============================================================
public interface IDayStartProcessor
{
    void ProcessDayStart(int dayNumber);
}
