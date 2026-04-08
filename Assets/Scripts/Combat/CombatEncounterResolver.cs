using UnityEngine;

// ============================================================
// CombatEncounterResolver
// ------------------------------------------------------------
// Aplica las reglas de combate cuando un participante queda
// listo para atacar.
//
// Responsabilidades:
// - elegir un objetivo valido del bando opuesto
// - aplicar dano
// - destruir unidades derrotadas
// - finalizar el encuentro cuando un bando queda vacio
// - liberar sobrevivientes del estado de combate
//
// En esta etapa:
// - usa una regla de target simple y determinista
// - no hace layout visual
// - no crea encuentros
// ============================================================
public class CombatEncounterResolver : MonoBehaviour
{
    public event System.Action<CombatEncounter, CardInstance, CardInstance, CombatDamageResult> OnAttackResolved;
    public event System.Action<CombatEncounter, CardInstance> OnParticipantKilled;
    public event System.Action<CombatEncounter> OnEncounterFinished;

    [Header("Dependencies")]
    [SerializeField] private CombatEncounterSystem encounterSystem;
    [SerializeField] private CombatDamageResolver damageResolver;

    private void Awake()
    {
        if (encounterSystem == null)
            encounterSystem = FindFirstObjectByType<CombatEncounterSystem>();

        if (damageResolver == null)
            damageResolver = FindFirstObjectByType<CombatDamageResolver>();

        if (damageResolver == null)
            damageResolver = gameObject.GetComponent<CombatDamageResolver>() ?? gameObject.AddComponent<CombatDamageResolver>();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (encounterSystem != null)
            encounterSystem.OnParticipantReadyToAttack -= HandleParticipantReadyToAttack;
    }

    private void TrySubscribe()
    {
        if (encounterSystem == null)
            encounterSystem = FindFirstObjectByType<CombatEncounterSystem>();

        if (encounterSystem != null)
        {
            encounterSystem.OnParticipantReadyToAttack -= HandleParticipantReadyToAttack;
            encounterSystem.OnParticipantReadyToAttack += HandleParticipantReadyToAttack;
        }
    }

    private void HandleParticipantReadyToAttack(CombatEncounter encounter, CardInstance attacker)
    {
        if (!CanResolveAttack(encounter, attacker))
            return;

        CombatParticipantRuntime attackerRuntime = attacker.CombatParticipantRuntime;
        CardInstance target = SelectTarget(encounter, attackerRuntime.Team);
        if (target == null)
        {
            TryFinishEncounter(encounter);
            return;
        }

        attackerRuntime.ConsumeAttackInterval();

        CombatParticipantRuntime targetRuntime = target.CombatParticipantRuntime;
        if (targetRuntime == null)
            return;

        CombatDamageResult damageResult = damageResolver != null
            ? damageResolver.Resolve(attacker, target)
            : new CombatDamageResult(0, 0, 0, 0f, StacklandsLike.Cards.CombatDefenseChannel.Physical, null);

        targetRuntime.ApplyDamage(damageResult.FinalDamage);
        OnAttackResolved?.Invoke(encounter, attacker, target, damageResult);

        if (targetRuntime.IsDead())
            KillParticipant(encounter, target);

        TryFinishEncounter(encounter);
    }

    private bool CanResolveAttack(CombatEncounter encounter, CardInstance attacker)
    {
        if (encounter == null || attacker == null)
            return false;

        if (!encounter.IsActive() || !encounter.HasBothTeams())
            return false;

        CombatParticipantRuntime attackerRuntime = attacker.CombatParticipantRuntime;
        if (attackerRuntime == null || !attackerRuntime.isActiveAndEnabled)
            return false;

        if (!attackerRuntime.IsInCombat || attackerRuntime.IsDead())
            return false;

        return attackerRuntime.Team == CombatTeam.Friendly || attackerRuntime.Team == CombatTeam.Enemy;
    }

    private CardInstance SelectTarget(CombatEncounter encounter, CombatTeam attackerTeam)
    {
        if (encounter == null || attackerTeam == CombatTeam.None)
            return null;

        CombatTeam targetTeam = attackerTeam == CombatTeam.Friendly
            ? CombatTeam.Enemy
            : CombatTeam.Friendly;

        System.Collections.Generic.IReadOnlyList<CardInstance> candidates = CombatFormationUtility.CollectTargetableParticipants(encounter, targetTeam);
        if (candidates == null)
            return null;

        for (int i = 0; i < candidates.Count; i++)
        {
            CardInstance candidate = candidates[i];
            CombatParticipantRuntime runtime = candidate != null ? candidate.CombatParticipantRuntime : null;
            if (runtime == null || runtime.IsDead())
                continue;

            return candidate;
        }

        return null;
    }

    private void KillParticipant(CombatEncounter encounter, CardInstance participant)
    {
        if (participant == null)
            return;

        encounter?.RemoveParticipant(participant);

        CombatParticipantRuntime runtime = participant.CombatParticipantRuntime;
        if (runtime != null)
            runtime.ExitCombat();

        participant.SetBusy(false);
        OnParticipantKilled?.Invoke(encounter, participant);

        if (participant.View != null)
            MarketEconomyService.DestroyCardUnit(participant.View);
    }

    private void TryFinishEncounter(CombatEncounter encounter)
    {
        if (encounter == null || encounter.IsFinished())
            return;

        if (!encounter.IsTeamDefeated(CombatTeam.Friendly) && !encounter.IsTeamDefeated(CombatTeam.Enemy))
            return;

        ReleaseSurvivors(encounter, encounter.FriendlyParticipants);
        ReleaseSurvivors(encounter, encounter.EnemyParticipants);

        encounter.MarkFinished();
        OnEncounterFinished?.Invoke(encounter);
        Destroy(encounter.gameObject);
    }

    private void ReleaseSurvivors(CombatEncounter encounter, System.Collections.Generic.IReadOnlyList<CardInstance> participants)
    {
        if (participants == null)
            return;

        for (int i = 0; i < participants.Count; i++)
        {
            CardInstance participant = participants[i];
            CombatParticipantRuntime runtime = participant != null ? participant.CombatParticipantRuntime : null;
            if (runtime == null || runtime.IsDead())
                continue;

            runtime.ExitCombat();
            participant.SetBusy(false);
        }
    }
}
