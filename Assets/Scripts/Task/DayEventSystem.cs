using System.Collections.Generic;
using UnityEngine;

// ============================================================
// DayEventSystem
// ------------------------------------------------------------
// Selector de eventos del inicio del dia.
//
// Responsabilidades:
// - recibir el numero del nuevo dia
// - filtrar eventos validos
// - elegir todos los deterministicos aplicables
// - elegir como maximo un evento aleatorio ponderado
// - recordar que eventos no repetibles ya ocurrieron
//
// Importante:
// - NO ejecuta spawns
// - NO crea cartas
// - NO modifica el board
// ============================================================
public class DayEventSystem : MonoBehaviour, IDayStartProcessor
{
    public event System.Action<DayEventSelectionResult> OnDayEventsSelected;

    [Header("Event Definitions")]
    [SerializeField] private DayEventDefinition[] eventDefinitions;

    public DayEventSelectionResult LastSelectionResult { get; private set; }

    private readonly HashSet<string> triggeredNonRepeatableEventIds = new HashSet<string>();

    public void ProcessDayStart(int dayNumber)
    {
        LastSelectionResult = SelectEventsForDay(dayNumber);
        OnDayEventsSelected?.Invoke(LastSelectionResult);
    }

    public DayEventSelectionResult SelectEventsForDay(int dayNumber)
    {
        DayEventSelectionResult result = new DayEventSelectionResult
        {
            dayNumber = Mathf.Max(1, dayNumber)
        };

        if (eventDefinitions == null || eventDefinitions.Length == 0)
            return result;

        List<DayEventDefinition> randomCandidates = new List<DayEventDefinition>();

        for (int i = 0; i < eventDefinitions.Length; i++)
        {
            DayEventDefinition definition = eventDefinitions[i];
            if (!CanSelectEvent(definition, result.dayNumber))
                continue;

            if (definition.triggerMode == DayEventTriggerMode.RandomFromMinDay)
            {
                randomCandidates.Add(definition);
                continue;
            }

            RegisterSelectedEvent(definition, result.selectedEvents);
        }

        DayEventDefinition randomSelection = SelectWeightedRandomEvent(randomCandidates);
        if (randomSelection != null)
            RegisterSelectedEvent(randomSelection, result.selectedEvents);

        return result;
    }

    private bool CanSelectEvent(DayEventDefinition definition, int dayNumber)
    {
        if (definition == null)
            return false;

        if (definition.eventType == DayEventType.None)
            return false;

        if (!definition.IsDayInSupportedRange(dayNumber))
            return false;

        if (!definition.repeatable && !string.IsNullOrWhiteSpace(definition.id) && triggeredNonRepeatableEventIds.Contains(definition.id))
            return false;

        if (definition.eventType == DayEventType.SpawnCards && (definition.spawnEntries == null || definition.spawnEntries.Length == 0))
            return false;

        return true;
    }

    private void RegisterSelectedEvent(DayEventDefinition definition, List<DayEventDefinition> selectedEvents)
    {
        if (definition == null || selectedEvents == null)
            return;

        selectedEvents.Add(definition);

        if (!definition.repeatable && !string.IsNullOrWhiteSpace(definition.id))
            triggeredNonRepeatableEventIds.Add(definition.id);
    }

    private DayEventDefinition SelectWeightedRandomEvent(List<DayEventDefinition> candidates)
    {
        if (candidates == null || candidates.Count == 0)
            return null;

        float totalWeight = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            DayEventDefinition definition = candidates[i];
            if (definition == null || definition.weight <= 0f)
                continue;

            totalWeight += definition.weight;
        }

        if (totalWeight <= 0f)
            return null;

        float roll = Random.Range(0f, totalWeight);
        float accumulated = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            DayEventDefinition definition = candidates[i];
            if (definition == null || definition.weight <= 0f)
                continue;

            accumulated += definition.weight;
            if (roll <= accumulated)
                return definition;
        }

        return candidates[candidates.Count - 1];
    }
}
