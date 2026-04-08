using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class InfrastructureValidationUtility
{
#if UNITY_EDITOR
    public static void ValidateAndLogCardSpawner(CardSpawner spawner)
    {
        if (spawner == null)
            return;

        string[] warnings = ValidateCardSpawner(spawner);
        for (int i = 0; i < warnings.Length; i++)
            Debug.LogWarning($"[InfrastructureValidation] {warnings[i]}", spawner);
    }

    public static void ValidateAndLogBoardRoot(BoardRoot boardRoot)
    {
        if (boardRoot == null)
            return;

        string[] warnings = ValidateBoardRoot(boardRoot);
        for (int i = 0; i < warnings.Length; i++)
            Debug.LogWarning($"[InfrastructureValidation] {warnings[i]}", boardRoot);
    }

    private static string[] ValidateCardSpawner(CardSpawner spawner)
    {
        System.Collections.Generic.List<string> warnings = new System.Collections.Generic.List<string>();

        GameObject cardPrefab = spawner.CardPrefab;
        if (cardPrefab == null)
        {
            warnings.Add($"CardSpawner '{spawner.name}' is missing cardPrefab.");
            return warnings.ToArray();
        }

        if (cardPrefab.GetComponent<RectTransform>() == null)
            warnings.Add($"CardSpawner '{spawner.name}' uses prefab '{cardPrefab.name}' without RectTransform.");

        if (cardPrefab.GetComponent<CardInstance>() == null)
            warnings.Add($"CardSpawner '{spawner.name}' uses prefab '{cardPrefab.name}' without CardInstance.");

        if (cardPrefab.GetComponent<CardView>() == null)
            warnings.Add($"CardSpawner '{spawner.name}' uses prefab '{cardPrefab.name}' without CardView.");

        if (cardPrefab.GetComponent<CardDrag>() == null)
            warnings.Add($"CardSpawner '{spawner.name}' uses prefab '{cardPrefab.name}' without CardDrag.");

        if (cardPrefab.GetComponent<CardInitializer>() == null)
        {
            warnings.Add(
                $"CardSpawner '{spawner.name}' uses prefab '{cardPrefab.name}' without CardInitializer. Runtime fallback will initialize the card, but this is not the preferred setup.");
        }

        if (spawner.CardsParentFallback == null && BoardRoot.Instance == null)
        {
            warnings.Add(
                $"CardSpawner '{spawner.name}' has no cardsParent fallback assigned. Without a BoardRoot in scene, spawned cards will not have a controlled parent.");
        }

        return warnings.ToArray();
    }

    private static string[] ValidateBoardRoot(BoardRoot boardRoot)
    {
        System.Collections.Generic.List<string> warnings = new System.Collections.Generic.List<string>();

        if (boardRoot.GetComponent<RectTransform>() == null && boardRoot.CardsContainer == null)
        {
            warnings.Add(
                $"BoardRoot '{boardRoot.name}' has no RectTransform on the same object and no cardsContainer assigned.");
        }

        if (boardRoot.CardsContainer == null && boardRoot.GetComponent<RectTransform>() == null)
        {
            warnings.Add(
                $"BoardRoot '{boardRoot.name}' cannot resolve a valid cardsContainer.");
        }

        if (boardRoot.LeftPadding < 0f || boardRoot.RightPadding < 0f || boardRoot.TopPadding < 0f || boardRoot.BottomPadding < 0f)
        {
            warnings.Add($"BoardRoot '{boardRoot.name}' has negative padding values.");
        }

        if (boardRoot.PlayArea == null)
        {
            warnings.Add($"BoardRoot '{boardRoot.name}' is missing playArea.");
        }
        else if (boardRoot.CardsContainer == null)
        {
            warnings.Add($"BoardRoot '{boardRoot.name}' is missing cardsContainer.");
        }
        else
        {
            if (!boardRoot.CardsContainer.IsChildOf(boardRoot.PlayArea) && boardRoot.CardsContainer != boardRoot.PlayArea)
            {
                warnings.Add(
                    $"BoardRoot '{boardRoot.name}' uses a cardsContainer that is not inside playArea. Drag-time clamping and board placement may feel inconsistent.");
            }

            Canvas playAreaCanvas = boardRoot.PlayArea.GetComponentInParent<Canvas>();
            Canvas cardsContainerCanvas = boardRoot.CardsContainer.GetComponentInParent<Canvas>();

            if (playAreaCanvas != cardsContainerCanvas)
            {
                warnings.Add(
                    $"BoardRoot '{boardRoot.name}' uses playArea and cardsContainer under different canvases. Drag-time clamping and board placement may not align.");
            }
        }

        return warnings.ToArray();
    }
#endif
}
