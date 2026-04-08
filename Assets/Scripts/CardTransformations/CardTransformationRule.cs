using System.Collections.Generic;
using StacklandsLike.Cards;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// ============================================================
// CardTransformationRule
// ------------------------------------------------------------
// Asset de authoring para transformaciones temporales de una
// sola carta.
//
// La regla define:
// - que carta puede transformarse
// - cuanto tarda
// - que capabilities pausan o modifican la velocidad
// - que pasa al completarse
//
// Importante:
// - esto NO es una receta
// - esto NO es una tarea de stack
// - describe evolucion temporal de una carta individual
// ============================================================
[CreateAssetMenu(fileName = "CardTransformationRule", menuName = "Card Transformations/Transformation Rule")]
public class CardTransformationRule : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;

    [Header("Source")]
    public CardData sourceCard;

    [Header("Timing")]
    [Min(0.01f)] public float baseDuration = 30f;
    public bool runOnlyOnBoard = true;
    public bool showProgressBar = true;

    [Header("Context Rules")]
    public List<CardCapabilityType> requiredCapabilities = new();
    public List<CardCapabilityType> pauseCapabilities = new();
    public List<CardTransformationSpeedModifier> speedModifiers = new();

    [Header("Completion")]
    public CardTransformationCompletionMode completionMode = CardTransformationCompletionMode.DestroyOnly;
    public CardData resultCard;
    public List<CardTransformationResultEntry> resultCards = new();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        CardTransformationValidationUtility.ValidateAndLog(this);
    }
#endif
}
