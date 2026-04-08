using UnityEngine;

// ============================================================
// DayEventExecutor
// ------------------------------------------------------------
// Ejecuta en el mundo los eventos seleccionados para el inicio
// del dia.
//
// Responsabilidades:
// - escuchar la seleccion hecha por `DayEventSystem`
// - convertir eventos `SpawnCards` en spawns reales
// - reutilizar `CardSpawner` y las reglas actuales del board
//
// Importante:
// - NO decide que eventos ocurren
// - NO filtra por dia
// - NO conoce reglas de economia, recetas o drag
// ============================================================
public class DayEventExecutor : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private DayEventSystem dayEventSystem;

    [Header("Spawn Placement")]
    [SerializeField] private RectTransform spawnAnchor;
    [SerializeField] private Vector2 fallbackSpawnCenter = new Vector2(0f, 180f);
    [SerializeField] private float releaseRadius = 120f;
    [SerializeField] private float releaseDuration = 0.6f;
    [SerializeField] private bool animateSpawns = true;

    private void OnEnable()
    {
        SubscribeToDayEventSystem();
    }

    private void OnDisable()
    {
        if (dayEventSystem != null)
            dayEventSystem.OnDayEventsSelected -= HandleDayEventsSelected;
    }

    private void Awake()
    {
        if (dayEventSystem == null)
            dayEventSystem = FindFirstObjectByType<DayEventSystem>();
    }

    private void SubscribeToDayEventSystem()
    {
        if (dayEventSystem == null)
            dayEventSystem = FindFirstObjectByType<DayEventSystem>();

        if (dayEventSystem == null)
            return;

        dayEventSystem.OnDayEventsSelected -= HandleDayEventsSelected;
        dayEventSystem.OnDayEventsSelected += HandleDayEventsSelected;
    }

    private void HandleDayEventsSelected(DayEventSelectionResult selectionResult)
    {
        if (selectionResult == null || selectionResult.selectedEvents == null || selectionResult.selectedEvents.Count == 0)
            return;

        for (int i = 0; i < selectionResult.selectedEvents.Count; i++)
        {
            DayEventDefinition definition = selectionResult.selectedEvents[i];
            ExecuteDayEvent(definition);
        }
    }

    private void ExecuteDayEvent(DayEventDefinition definition)
    {
        if (definition == null)
            return;

        switch (definition.eventType)
        {
            case DayEventType.SpawnCards:
                ExecuteSpawnCards(definition);
                break;
        }
    }

    // `SpawnCards` es la primera familia de eventos del sistema.
    // El evento solo describe que cartas aparecen; la posicion
    // final se resuelve reutilizando el flow normal del spawner.
    private void ExecuteSpawnCards(DayEventDefinition definition)
    {
        if (definition == null || definition.spawnEntries == null || definition.spawnEntries.Length == 0)
            return;

        if (CardSpawner.Instance == null)
            return;

        Vector2 spawnCenter = ResolveSpawnCenter();
        int totalCardsToSpawn = CountTotalCardsToSpawn(definition.spawnEntries);
        int spawnIndex = 0;

        for (int i = 0; i < definition.spawnEntries.Length; i++)
        {
            DayEventSpawnEntry entry = definition.spawnEntries[i];
            if (entry == null || entry.card == null || entry.count <= 0)
                continue;

            for (int countIndex = 0; countIndex < entry.count; countIndex++)
            {
                Vector2 targetPosition = GetSpawnTargetPosition(spawnCenter, spawnIndex, totalCardsToSpawn);
                SpawnCardEntry(entry.card, spawnCenter, targetPosition);
                spawnIndex++;
            }
        }
    }

    private void SpawnCardEntry(CardData card, Vector2 startPosition, Vector2 targetPosition)
    {
        if (card == null || CardSpawner.Instance == null)
            return;

        if (animateSpawns)
        {
            CardSpawner.Instance.SpawnAnimatedToPosition(
                card,
                startPosition,
                targetPosition,
                releaseDuration
            );
            return;
        }

        CardSpawner.Instance.Spawn(card, targetPosition);
    }

    private Vector2 ResolveSpawnCenter()
    {
        if (BoardRoot.Instance == null || BoardRoot.Instance.CardsContainer == null)
            return fallbackSpawnCenter;

        if (spawnAnchor != null)
            return BoardRoot.Instance.GetBoardPointFromWorldPosition(spawnAnchor.position);

        Rect boardRect = BoardRoot.Instance.CardsContainer.rect;
        Vector2 preferredCenter = new Vector2(fallbackSpawnCenter.x, boardRect.center.y + fallbackSpawnCenter.y);

        return BoardRoot.Instance.GetClampedPosition(preferredCenter, BoardRoot.Instance.CardsContainer);
    }

    private int CountTotalCardsToSpawn(DayEventSpawnEntry[] entries)
    {
        if (entries == null || entries.Length == 0)
            return 0;

        int total = 0;

        for (int i = 0; i < entries.Length; i++)
        {
            DayEventSpawnEntry entry = entries[i];
            if (entry == null || entry.card == null || entry.count <= 0)
                continue;

            total += entry.count;
        }

        return total;
    }

    private Vector2 GetSpawnTargetPosition(Vector2 spawnCenter, int spawnIndex, int totalCardsToSpawn)
    {
        if (BoardRoot.Instance == null)
            return spawnCenter;

        if (totalCardsToSpawn <= 1)
            return BoardRoot.Instance.FindNearestFreePoint(spawnCenter, releaseRadius * 0.35f, 24f, 6);

        float angle = (Mathf.PI * 2f * spawnIndex) / totalCardsToSpawn;
        Vector2 preferredOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * releaseRadius;
        Vector2 preferredPosition = spawnCenter + preferredOffset;

        return BoardRoot.Instance.FindNearestFreePoint(preferredPosition, releaseRadius * 0.35f, 24f, 6);
    }
}
