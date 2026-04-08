using UnityEngine;

// ============================================================
// CombatLootDropSystem
// ------------------------------------------------------------
// Escucha muertes de combate y resuelve drops solo para
// enemigos authorados con `EnemyCardData`.
// ============================================================
public class CombatLootDropSystem : MonoBehaviour
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
        if (encounterResolver != null)
            encounterResolver.OnParticipantKilled -= HandleParticipantKilled;
    }

    private void TrySubscribe()
    {
        if (encounterResolver == null)
            encounterResolver = FindFirstObjectByType<CombatEncounterResolver>();

        if (encounterResolver == null)
            return;

        encounterResolver.OnParticipantKilled -= HandleParticipantKilled;
        encounterResolver.OnParticipantKilled += HandleParticipantKilled;
    }

    private void HandleParticipantKilled(CombatEncounter encounter, CardInstance participant)
    {
        EnemyCardData enemyData = participant != null ? participant.data as EnemyCardData : null;
        if (enemyData == null || CardSpawner.Instance == null)
            return;

        Vector2 dropAnchor = ResolveDropAnchor(participant);

        SpawnGuaranteedDrops(enemyData, dropAnchor);
        SpawnRandomDrops(enemyData, dropAnchor);
    }

    private void SpawnGuaranteedDrops(EnemyCardData enemyData, Vector2 dropAnchor)
    {
        if (enemyData == null || enemyData.guaranteedDrops == null)
            return;

        for (int i = 0; i < enemyData.guaranteedDrops.Count; i++)
        {
            EnemyGuaranteedDropEntry entry = enemyData.guaranteedDrops[i];
            if (entry == null || entry.card == null)
                continue;

            int dropCount = ResolveDropCount(entry.minCount, entry.maxCount);
            SpawnDropCopies(entry.card, dropCount, dropAnchor);
        }
    }

    private void SpawnRandomDrops(EnemyCardData enemyData, Vector2 dropAnchor)
    {
        if (enemyData == null || enemyData.randomDrops == null)
            return;

        for (int i = 0; i < enemyData.randomDrops.Count; i++)
        {
            EnemyRandomDropEntry entry = enemyData.randomDrops[i];
            if (entry == null || entry.card == null)
                continue;

            if (Random.value > Mathf.Clamp01(entry.dropChance))
                continue;

            int dropCount = ResolveDropCount(entry.minCount, entry.maxCount);
            SpawnDropCopies(entry.card, dropCount, dropAnchor);
        }
    }

    private void SpawnDropCopies(CardData cardData, int count, Vector2 dropAnchor)
    {
        if (cardData == null || count <= 0 || CardSpawner.Instance == null)
            return;

        for (int i = 0; i < count; i++)
            CardSpawner.Instance.Spawn(cardData, dropAnchor);
    }

    private int ResolveDropCount(int minCount, int maxCount)
    {
        int clampedMin = Mathf.Max(1, minCount);
        int clampedMax = Mathf.Max(clampedMin, maxCount);
        return Random.Range(clampedMin, clampedMax + 1);
    }

    private Vector2 ResolveDropAnchor(CardInstance participant)
    {
        if (participant == null || participant.RectTransform == null)
            return Vector2.zero;

        if (BoardRoot.Instance != null)
            return BoardRoot.Instance.GetBoardPointFromWorldPosition(participant.RectTransform.position);

        return participant.RectTransform.anchoredPosition;
    }
}
