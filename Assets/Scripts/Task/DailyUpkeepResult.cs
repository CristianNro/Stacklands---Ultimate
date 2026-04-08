using System.Collections.Generic;

// ============================================================
// DailyUpkeepResult
// ------------------------------------------------------------
// Resultado estructurado del mantenimiento diario.
//
// La idea es que el sistema de fin de dia no haga todo "en
// silencio". Este resultado deja un resumen claro para:
// - debug
// - UI futura
// - consecuencias posteriores por falta de comida
// ============================================================
public sealed class DailyUpkeepResult
{
    public int dayNumber;
    public int requiredFood;
    public int consumedFood;
    public int missingFood;
    public readonly List<ConsumedFoodRecord> consumedCards = new List<ConsumedFoodRecord>();
    public readonly List<UnitFeedingRecord> fedUnits = new List<UnitFeedingRecord>();
    public readonly List<UnitFeedingRecord> deadUnits = new List<UnitFeedingRecord>();

    public sealed class ConsumedFoodRecord
    {
        public string runtimeId;
        public string cardId;
        public string displayName;
        public bool cardSurvivedConsumption;
    }

    public sealed class UnitFeedingRecord
    {
        public string runtimeId;
        public string cardId;
        public string displayName;
        public int requiredFood;
    }
}
