using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// ============================================================
// DayEventValidationUtility
// ------------------------------------------------------------
// Validacion minima para assets de eventos del dia.
//
// La idea es detectar temprano:
// - ids vacios
// - tipos nulos
// - pesos invalidos
// - rangos incoherentes
// - eventos de spawn sin entradas validas
// ============================================================
public static class DayEventValidationUtility
{
#if UNITY_EDITOR
    public static void ValidateAndLog(DayEventDefinition definition)
    {
        if (definition == null)
            return;

        List<string> warnings = Validate(definition);
        for (int i = 0; i < warnings.Count; i++)
            Debug.LogWarning($"[DayEvent] {warnings[i]}", definition);
    }

    public static List<string> Validate(DayEventDefinition definition)
    {
        List<string> warnings = new List<string>();
        if (definition == null)
            return warnings;

        if (string.IsNullOrWhiteSpace(definition.id))
            warnings.Add($"Day event asset '{definition.name}' is missing a stable id.");

        if (string.IsNullOrWhiteSpace(definition.displayName))
            warnings.Add($"Day event asset '{definition.name}' is missing displayName.");

        if (definition.eventType == DayEventType.None)
            warnings.Add($"Day event asset '{definition.name}' uses DayEventType.None.");

        if (definition.triggerMode == DayEventTriggerMode.RandomFromMinDay && definition.weight <= 0f)
            warnings.Add($"Random day event '{definition.name}' should have weight greater than 0.");

        if (definition.triggerMode == DayEventTriggerMode.DayRange && definition.maxDay < definition.minDay)
            warnings.Add($"Day range event '{definition.name}' should not have maxDay lower than minDay.");

        ValidateSpawnEntries(definition, warnings);

        DayEventDefinition duplicate = FindDuplicateId(definition);
        if (duplicate != null)
        {
            warnings.Add(
                $"Duplicate day event id '{definition.id}'. Conflicting assets: '{duplicate.name}' and '{definition.name}'.");
        }

        return warnings;
    }

    private static void ValidateSpawnEntries(DayEventDefinition definition, List<string> warnings)
    {
        if (definition == null || definition.eventType != DayEventType.SpawnCards)
            return;

        if (definition.spawnEntries == null || definition.spawnEntries.Length == 0)
        {
            warnings.Add($"Spawn day event '{definition.name}' has no spawnEntries configured.");
            return;
        }

        bool hasAnyValidEntry = false;

        for (int i = 0; i < definition.spawnEntries.Length; i++)
        {
            DayEventSpawnEntry entry = definition.spawnEntries[i];
            if (entry == null || entry.card == null)
            {
                warnings.Add($"Spawn day event '{definition.name}' contains an empty spawn entry.");
                continue;
            }

            if (entry.count <= 0)
            {
                warnings.Add($"Spawn day event '{definition.name}' contains '{entry.card.name}' with non-positive count '{entry.count}'.");
                continue;
            }

            hasAnyValidEntry = true;
        }

        if (!hasAnyValidEntry)
            warnings.Add($"Spawn day event '{definition.name}' has no valid spawn entries.");
    }

    private static DayEventDefinition FindDuplicateId(DayEventDefinition source)
    {
        if (source == null || string.IsNullOrWhiteSpace(source.id))
            return null;

        string sourcePath = AssetDatabase.GetAssetPath(source);
        string[] guids = AssetDatabase.FindAssets("t:DayEventDefinition");

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (path == sourcePath)
                continue;

            DayEventDefinition other = AssetDatabase.LoadAssetAtPath<DayEventDefinition>(path);
            if (other == null)
                continue;

            if (string.Equals(other.id, source.id))
                return other;
        }

        return null;
    }
#endif
}
