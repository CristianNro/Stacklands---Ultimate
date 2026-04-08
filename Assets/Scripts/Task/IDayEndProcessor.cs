// ============================================================
// IDayEndProcessor
// ------------------------------------------------------------
// Contrato minimo para sistemas que quieran participar del
// cierre diario.
//
// El coordinador diario ejecuta estos procesadores en orden.
// Mas adelante, `DailyUpkeepSystem` deberia enchufarse aqui.
// ============================================================
public interface IDayEndProcessor
{
    void ProcessDayEnd(int dayNumber);
}
