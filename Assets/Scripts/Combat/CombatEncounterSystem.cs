using System.Collections.Generic;
using UnityEngine;

// ============================================================
// CombatEncounterSystem
// ------------------------------------------------------------
// Scheduler central para encuentros de combate activos.
//
// Responsabilidades:
// - registrar encuentros activos
// - avanzar timers de ataque con el tiempo compartido
// - exponer cuando un participante queda listo para atacar
//
// En esta etapa:
// - aun no aplica dano
// - aun no elige targets
// - aun no cierra encuentros por si mismo
// ============================================================
public class CombatEncounterSystem : MonoBehaviour
{
    public event System.Action<CombatEncounter, CardInstance> OnParticipantReadyToAttack;

    private readonly HashSet<CombatEncounter> activeEncounters = new HashSet<CombatEncounter>();
    private readonly List<CombatEncounter> encounterBuffer = new List<CombatEncounter>();

    private void OnEnable()
    {
        CombatEncounter.OnEncounterAvailable += HandleEncounterAvailable;
        CombatEncounter.OnEncounterUnavailable += HandleEncounterUnavailable;
        BootstrapExistingEncounters();
    }

    private void OnDisable()
    {
        CombatEncounter.OnEncounterAvailable -= HandleEncounterAvailable;
        CombatEncounter.OnEncounterUnavailable -= HandleEncounterUnavailable;
        activeEncounters.Clear();
        encounterBuffer.Clear();
    }

    private void Update()
    {
        float deltaTime = GameTimeService.GetTimedSystemsDeltaTime();
        RefreshActiveEncounters();
        AdvanceEncounters(deltaTime);
    }

    private void HandleEncounterAvailable(CombatEncounter encounter)
    {
        TryTrackEncounter(encounter);
    }

    private void HandleEncounterUnavailable(CombatEncounter encounter)
    {
        if (encounter != null)
            activeEncounters.Remove(encounter);
    }

    private void BootstrapExistingEncounters()
    {
        activeEncounters.Clear();

        CombatEncounter[] encounters = FindObjectsByType<CombatEncounter>(FindObjectsSortMode.None);
        for (int i = 0; i < encounters.Length; i++)
            TryTrackEncounter(encounters[i]);
    }

    private void TryTrackEncounter(CombatEncounter encounter)
    {
        if (!CanTrackEncounter(encounter))
            return;

        activeEncounters.Add(encounter);
    }

    private void RefreshActiveEncounters()
    {
        if (activeEncounters.Count == 0)
            return;

        encounterBuffer.Clear();
        encounterBuffer.AddRange(activeEncounters);

        for (int i = 0; i < encounterBuffer.Count; i++)
        {
            CombatEncounter encounter = encounterBuffer[i];
            if (CanTrackEncounter(encounter))
                continue;

            activeEncounters.Remove(encounter);
        }
    }

    private void AdvanceEncounters(float deltaTime)
    {
        if (deltaTime <= 0f || activeEncounters.Count == 0)
            return;

        encounterBuffer.Clear();
        encounterBuffer.AddRange(activeEncounters);

        for (int i = 0; i < encounterBuffer.Count; i++)
        {
            CombatEncounter encounter = encounterBuffer[i];
            if (!CanAdvanceEncounter(encounter))
                continue;

            AdvanceEncounterParticipants(encounter, encounter.FriendlyParticipants, deltaTime);
            AdvanceEncounterParticipants(encounter, encounter.EnemyParticipants, deltaTime);
        }
    }

    private void AdvanceEncounterParticipants(CombatEncounter encounter, IReadOnlyList<CardInstance> participants, float deltaTime)
    {
        if (participants == null)
            return;

        for (int i = 0; i < participants.Count; i++)
        {
            CardInstance instance = participants[i];
            if (!CanAdvanceParticipant(instance))
                continue;

            CombatParticipantRuntime runtime = instance.CombatParticipantRuntime;
            float previousTimer = runtime.AttackTimer;
            float currentTimer = runtime.AdvanceAttackTimer(deltaTime);
            float requiredInterval = runtime.CombatantData != null ? Mathf.Max(0.01f, runtime.CombatantData.attackInterval) : 0.01f;

            if (previousTimer < requiredInterval && currentTimer >= requiredInterval)
                OnParticipantReadyToAttack?.Invoke(encounter, instance);
        }
    }

    private bool CanTrackEncounter(CombatEncounter encounter)
    {
        return encounter != null && encounter.isActiveAndEnabled && !encounter.IsFinished();
    }

    private bool CanAdvanceEncounter(CombatEncounter encounter)
    {
        if (!CanTrackEncounter(encounter))
            return false;

        return encounter.HasBothTeams();
    }

    private bool CanAdvanceParticipant(CardInstance instance)
    {
        if (instance == null)
            return false;

        CombatParticipantRuntime runtime = instance.CombatParticipantRuntime;
        if (runtime == null || !runtime.isActiveAndEnabled)
            return false;

        if (!runtime.IsInCombat || runtime.IsDead())
            return false;

        return runtime.CombatantData != null;
    }
}
