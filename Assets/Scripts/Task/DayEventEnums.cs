// ============================================================
// DayEvent enums
// ------------------------------------------------------------
// Tipos base del sistema de eventos diarios.
//
// En esta etapa no modelamos "portal" o "mercader" como tipos
// especiales. El evento describe una accion generica de mundo,
// y las cartas spawneadas se encargan de su comportamiento.
// ============================================================
public enum DayEventTriggerMode
{
    ExactDay,
    DayRange,
    RandomFromMinDay
}

public enum DayEventType
{
    None,
    SpawnCards
}
