using UnityEngine;

// ============================================================
// CardTransformationExecutor
// ------------------------------------------------------------
// Ejecuta la finalizacion de una transformacion temporal.
//
// Responsabilidades:
// - destruir o reemplazar la carta origen
// - spawnear resultados usando el flujo real del board
// - mantener la transformacion fuera de recetas y stacks
//
// Importante:
// - NO decide cuando una carta completa su tiempo
// - NO avanza progreso
// - solo aplica el resultado final de la regla
// ============================================================
public class CardTransformationExecutor : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private bool animateResults = true;
    [SerializeField] private float releaseRadius = 110f;
    [SerializeField] private float releaseDuration = 0.55f;

    public void ExecuteTransformation(CardInstance sourceInstance, CardTransformationRule rule)
    {
        if (sourceInstance == null || rule == null)
            return;

        Vector2 sourcePosition = ResolveSourceBoardPosition(sourceInstance);

        switch (rule.completionMode)
        {
            case CardTransformationCompletionMode.DestroyOnly:
                DestroySourceCard(sourceInstance);
                return;

            case CardTransformationCompletionMode.ReplaceWithSingleResult:
                DestroySourceCard(sourceInstance);
                if (!CanSpawnSingleResult(rule.resultCard))
                {
                    Debug.LogWarning(
                        $"[CardTransformation] '{rule.displayName}' destroyed the source card but could not spawn its single result.",
                        sourceInstance);
                    return;
                }

                SpawnSingleResult(rule.resultCard, sourcePosition);
                return;

            case CardTransformationCompletionMode.SpawnMultipleResults:
                DestroySourceCard(sourceInstance);
                if (!CanSpawnMultipleResults(rule.resultCards))
                {
                    Debug.LogWarning(
                        $"[CardTransformation] '{rule.displayName}' destroyed the source card but could not spawn its configured results.",
                        sourceInstance);
                    return;
                }

                SpawnMultipleResults(rule.resultCards, sourcePosition);
                return;
        }
    }

    private bool CanSpawnSingleResult(CardData resultCard)
    {
        if (resultCard == null)
            return false;

        return CardSpawner.Instance != null;
    }

    private bool CanSpawnMultipleResults(System.Collections.Generic.IReadOnlyList<CardTransformationResultEntry> resultEntries)
    {
        if (CardSpawner.Instance == null || resultEntries == null || resultEntries.Count == 0)
            return false;

        for (int i = 0; i < resultEntries.Count; i++)
        {
            CardTransformationResultEntry entry = resultEntries[i];
            if (entry == null || entry.card == null || entry.count <= 0)
                continue;

            return true;
        }

        return false;
    }

    private void SpawnSingleResult(CardData resultCard, Vector2 sourcePosition)
    {
        if (resultCard == null || CardSpawner.Instance == null)
            return;

        if (animateResults)
        {
            CardSpawner.Instance.SpawnAnimatedToPosition(
                resultCard,
                sourcePosition,
                sourcePosition,
                releaseDuration
            );
            return;
        }

        CardSpawner.Instance.Spawn(resultCard, sourcePosition);
    }

    private void SpawnMultipleResults(System.Collections.Generic.IReadOnlyList<CardTransformationResultEntry> resultEntries, Vector2 sourcePosition)
    {
        if (resultEntries == null || resultEntries.Count == 0 || CardSpawner.Instance == null)
            return;

        int totalCardsToSpawn = CountTotalResults(resultEntries);
        int spawnIndex = 0;

        for (int i = 0; i < resultEntries.Count; i++)
        {
            CardTransformationResultEntry entry = resultEntries[i];
            if (entry == null || entry.card == null || entry.count <= 0)
                continue;

            for (int countIndex = 0; countIndex < entry.count; countIndex++)
            {
                Vector2 targetPosition = GetSpreadTargetPosition(sourcePosition, spawnIndex, totalCardsToSpawn);

                if (animateResults)
                {
                    CardSpawner.Instance.SpawnAnimatedToPosition(
                        entry.card,
                        sourcePosition,
                        targetPosition,
                        releaseDuration
                    );
                }
                else
                {
                    CardSpawner.Instance.Spawn(entry.card, targetPosition);
                }

                spawnIndex++;
            }
        }
    }

    private int CountTotalResults(System.Collections.Generic.IReadOnlyList<CardTransformationResultEntry> resultEntries)
    {
        int total = 0;

        for (int i = 0; i < resultEntries.Count; i++)
        {
            CardTransformationResultEntry entry = resultEntries[i];
            if (entry == null || entry.card == null || entry.count <= 0)
                continue;

            total += entry.count;
        }

        return total;
    }

    private Vector2 GetSpreadTargetPosition(Vector2 sourcePosition, int spawnIndex, int totalCardsToSpawn)
    {
        if (BoardRoot.Instance == null || totalCardsToSpawn <= 1)
            return sourcePosition;

        float angle = (Mathf.PI * 2f * spawnIndex) / totalCardsToSpawn;
        Vector2 preferredOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * releaseRadius;
        Vector2 preferredPosition = sourcePosition + preferredOffset;

        return BoardRoot.Instance.FindNearestFreePoint(preferredPosition, releaseRadius * 0.35f, 24f, 6);
    }

    private Vector2 ResolveSourceBoardPosition(CardInstance sourceInstance)
    {
        if (sourceInstance == null)
            return Vector2.zero;

        if (sourceInstance.RectTransform != null && BoardRoot.Instance != null)
            return BoardRoot.Instance.GetBoardPointFromWorldPosition(sourceInstance.RectTransform.position);

        if (sourceInstance.RectTransform != null)
            return sourceInstance.RectTransform.anchoredPosition;

        return Vector2.zero;
    }

    private void DestroySourceCard(CardInstance sourceInstance)
    {
        if (sourceInstance == null || sourceInstance.View == null)
            return;

        CardStack currentStack = sourceInstance.CurrentStack;
        if (currentStack != null)
        {
            currentStack.DestroyCardForSystem(sourceInstance.View);
            currentStack.FinalizeCraftingMutation();
            return;
        }

        MarketEconomyService.DestroyCardUnit(sourceInstance.View);
    }
}
