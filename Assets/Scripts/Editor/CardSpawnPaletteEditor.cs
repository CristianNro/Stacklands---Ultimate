#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CardSpawnPalette))]
public class CardSpawnPaletteEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CardSpawnPalette palette = (CardSpawnPalette)target;
        if (palette == null)
            return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Asset Sync", EditorStyles.boldLabel);

        if (GUILayout.Button("Reload From Assets/Cards"))
        {
            palette.ReloadCardsFromAssets();
        }

        EditorGUILayout.HelpBox(
            $"Current source folder: {palette.AssetsFolder}",
            MessageType.None);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Spawn Actions", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(!Application.isPlaying || CardSpawner.Instance == null))
        {
            if (GUILayout.Button("Spawn All"))
            {
                palette.SpawnAllCards();
            }

            var availableCards = palette.AvailableCards;
            for (int i = 0; i < availableCards.Count; i++)
            {
                CardData cardData = availableCards[i];
                string buttonLabel = cardData != null
                    ? $"Spawn {ResolveCardLabel(cardData)}"
                    : $"Spawn Entry {i}";

                if (GUILayout.Button(buttonLabel))
                {
                    palette.SpawnCardAtIndex(i);
                }
            }
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "The spawn buttons are enabled only during Play Mode.",
                MessageType.Info);
        }
        else if (CardSpawner.Instance == null)
        {
            EditorGUILayout.HelpBox(
                "CardSpawner.Instance was not found in the running scene.",
                MessageType.Warning);
        }
    }

    private static string ResolveCardLabel(CardData cardData)
    {
        if (!string.IsNullOrWhiteSpace(cardData.displayName))
            return cardData.displayName;

        if (!string.IsNullOrWhiteSpace(cardData.cardName))
            return cardData.cardName;

        return cardData.name;
    }
}
#endif
