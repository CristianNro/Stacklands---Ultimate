using System.Collections.Generic;

// ============================================================
// DayEventSelectionResult
// ------------------------------------------------------------
// Resultado estructurado de la seleccion de eventos del dia.
//
// No ejecuta nada.
// Solo deja explicitado:
// - para que dia se selecciono
// - que eventos quedaron elegidos
// ============================================================
public sealed class DayEventSelectionResult
{
    public int dayNumber;
    public readonly List<DayEventDefinition> selectedEvents = new List<DayEventDefinition>();
}
