using UnityEngine;

// ============================================================
// CombatEncounterFeedback
// ------------------------------------------------------------
// Escucha el resolver y dispara feedback visual liviano sobre
// las cartas sin mezclar presentacion con reglas de combate.
// ============================================================
public class CombatEncounterFeedback : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CombatEncounterResolver encounterResolver;

    private void Awake()
    {
        if (encounterResolver == null)
            encounterResolver = FindFirstObjectByType<CombatEncounterResolver>();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (encounterResolver == null)
            return;

        encounterResolver.OnAttackResolved -= HandleAttackResolved;
        encounterResolver.OnParticipantKilled -= HandleParticipantKilled;
    }

    private void TrySubscribe()
    {
        if (encounterResolver == null)
            encounterResolver = FindFirstObjectByType<CombatEncounterResolver>();

        if (encounterResolver == null)
            return;

        encounterResolver.OnAttackResolved -= HandleAttackResolved;
        encounterResolver.OnParticipantKilled -= HandleParticipantKilled;
        encounterResolver.OnAttackResolved += HandleAttackResolved;
        encounterResolver.OnParticipantKilled += HandleParticipantKilled;
    }

    private void HandleAttackResolved(CombatEncounter encounter, CardInstance attacker, CardInstance target, CombatDamageResult damageResult)
    {
        int damage = damageResult != null ? damageResult.FinalDamage : 0;

        if (attacker != null && attacker.View != null)
            attacker.View.PlayCombatAttackFeedback();

        if (damage > 0 && target != null && target.View != null)
            target.View.PlayCombatHitFeedback();
    }

    private void HandleParticipantKilled(CombatEncounter encounter, CardInstance participant)
    {
        if (participant != null && participant.View != null)
            participant.View.PlayCombatDeathFeedback();
    }
}
