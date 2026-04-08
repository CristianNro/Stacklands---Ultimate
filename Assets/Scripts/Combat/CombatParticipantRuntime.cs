using UnityEngine;
using StacklandsLike.Cards;

// ============================================================
// CombatParticipantRuntime
// ------------------------------------------------------------
// Estado runtime mutable de combate para una carta individual.
//
// Ownership:
// - `CombatantCardData` define stats base
// - este componente guarda el estado mutable de combate
// - sistemas futuros de encounter decidiran cuando entra o sale
//   de combate y como avanza su timer
//
// En esta etapa:
// - guarda salud actual
// - guarda timer de ataque
// - marca si esta en combate
// - marca a que lado pertenece
// ============================================================
public class CombatParticipantRuntime : MonoBehaviour
{
    [SerializeField] private CombatantCardData combatantData;
    [SerializeField] private int currentHealth;
    [SerializeField] private float attackTimer;
    [SerializeField] private CombatTeam team = CombatTeam.None;
    [SerializeField] private bool isInCombat;
    [SerializeField] private string encounterId;

    public CombatantCardData CombatantData => combatantData;
    public UnitCardData UnitData => combatantData as UnitCardData;
    public EnemyCardData EnemyData => combatantData as EnemyCardData;
    public int CurrentHealth => Mathf.Max(0, currentHealth);
    public float AttackTimer => Mathf.Max(0f, attackTimer);
    public CombatTeam Team => team;
    public bool IsInCombat => isInCombat;
    public string EncounterId => encounterId;
    public FactionType Faction => combatantData != null ? combatantData.faction : FactionType.Neutral;

    public void Initialize(CombatantCardData data)
    {
        combatantData = data;
        currentHealth = data != null ? Mathf.Max(1, data.maxHealth) : 0;
        attackTimer = 0f;
        team = CombatTeam.None;
        isInCombat = false;
        encounterId = string.Empty;
    }

    public void ResetCombatState()
    {
        attackTimer = 0f;
        team = CombatTeam.None;
        isInCombat = false;
        encounterId = string.Empty;
    }

    public void SetCurrentHealth(int value)
    {
        currentHealth = Mathf.Max(0, value);
    }

    public void RestoreFullHealth()
    {
        currentHealth = combatantData != null ? Mathf.Max(1, combatantData.maxHealth) : 0;
    }

    public void ApplyDamage(int amount)
    {
        if (amount <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
    }

    public bool IsDead()
    {
        return CurrentHealth <= 0;
    }

    public void SetAttackTimer(float value)
    {
        attackTimer = Mathf.Max(0f, value);
    }

    public float AdvanceAttackTimer(float deltaTime)
    {
        if (deltaTime <= 0f)
            return attackTimer;

        attackTimer = Mathf.Max(0f, attackTimer + deltaTime);
        return attackTimer;
    }

    public void ConsumeAttackInterval()
    {
        float attackInterval = combatantData != null ? Mathf.Max(0.01f, combatantData.attackInterval) : 0.01f;
        attackTimer = Mathf.Max(0f, attackTimer - attackInterval);
    }

    public bool IsReadyToAttack()
    {
        if (combatantData == null)
            return false;

        return attackTimer >= Mathf.Max(0.01f, combatantData.attackInterval);
    }

    public void EnterCombat(string newEncounterId, CombatTeam newTeam)
    {
        isInCombat = true;
        encounterId = string.IsNullOrWhiteSpace(newEncounterId) ? string.Empty : newEncounterId;
        team = newTeam;
        attackTimer = 0f;
    }

    public void ExitCombat()
    {
        ResetCombatState();
    }
}
