using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// ============================================================
// CardSpawnPalette
// ------------------------------------------------------------
// Herramienta de apoyo para spawnear cartas desde inspector
// usando el CardSpawner actual del proyecto.
// Pensado principalmente para usarse durante Play Mode.
// ============================================================
public class CardSpawnPalette : MonoBehaviour
{
    [Header("Cards")]
    [SerializeField] private List<CardData> availableCards = new List<CardData>();
    [SerializeField] private string assetsFolder = "Assets/Cards";

    [Header("Spawn")]
    [SerializeField] private Vector2 spawnOffset = Vector2.zero;
    [SerializeField] private Vector2 spawnStep = new Vector2(32f, -32f);

    public IReadOnlyList<CardData> AvailableCards => availableCards;
    public string AssetsFolder => assetsFolder;

    public bool CanSpawn()
    {
        return Application.isPlaying && CardSpawner.Instance != null;
    }

    public void SpawnCardAtIndex(int index)
    {
        if (!CanSpawn())
        {
            Debug.LogWarning("[CardSpawnPalette] Spawn is only available during Play Mode and requires CardSpawner.Instance.");
            return;
        }

        if (index < 0 || index >= availableCards.Count)
            return;

        CardData cardData = availableCards[index];
        if (cardData == null)
        {
            Debug.LogWarning($"[CardSpawnPalette] Card entry at index {index} is null.");
            return;
        }

        Vector2 spawnPosition = GetBaseSpawnPosition() + spawnOffset + (spawnStep * index);
        CardSpawner.Instance.Spawn(cardData, spawnPosition);
    }

    public void SpawnAllCards()
    {
        if (!CanSpawn())
        {
            Debug.LogWarning("[CardSpawnPalette] Spawn is only available during Play Mode and requires CardSpawner.Instance.");
            return;
        }

        for (int i = 0; i < availableCards.Count; i++)
            SpawnCardAtIndex(i);
    }

    private Vector2 GetBaseSpawnPosition()
    {
        RectTransform rectTransform = transform as RectTransform;
        if (rectTransform != null)
            return rectTransform.anchoredPosition;

        return Vector2.zero;
    }

#if UNITY_EDITOR
    public void ReloadCardsFromAssets()
    {
        string searchFolder = string.IsNullOrWhiteSpace(assetsFolder)
            ? "Assets/Cards"
            : assetsFolder;

        string[] guids = AssetDatabase.FindAssets("t:CardData", new[] { searchFolder });
        List<CardData> loadedCards = new List<CardData>();

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            CardData cardData = AssetDatabase.LoadAssetAtPath<CardData>(path);
            if (cardData != null && !loadedCards.Contains(cardData))
                loadedCards.Add(cardData);
        }

        loadedCards.Sort(CompareCardsForPalette);
        availableCards = loadedCards;
        EditorUtility.SetDirty(this);
    }

    private static int CompareCardsForPalette(CardData a, CardData b)
    {
        if (a == null && b == null) return 0;
        if (a == null) return 1;
        if (b == null) return -1;

        string labelA = !string.IsNullOrWhiteSpace(a.displayName)
            ? a.displayName
            : !string.IsNullOrWhiteSpace(a.cardName) ? a.cardName : a.name;
        string labelB = !string.IsNullOrWhiteSpace(b.displayName)
            ? b.displayName
            : !string.IsNullOrWhiteSpace(b.cardName) ? b.cardName : b.name;

        return string.CompareOrdinal(labelA, labelB);
    }
#endif
}
