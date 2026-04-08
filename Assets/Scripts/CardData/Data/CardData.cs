using System.Collections.Generic;
using UnityEngine;
using StacklandsLike.Cards;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class CardData : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string cardName;
    public string displayName;
    [TextArea] public string description;

    [Header("Classification")]
    public CardType cardType;
    public Rarity rarity;

    [Header("Visual")]
    public Sprite cardImage;
    public Sprite cardIcon;

    [Header("Interaction")]
    // Active contract: stack logic must respect this.
    public bool stackable = true;
    // Active contract: drag/drop logic must respect this.
    public bool isMovable = true;

    [Header("Balance")]
    // Active contract: stack capacity and stack rules depend on total weight.
    public float weight = 1f;
    public int value = 0;

    [Header("Economy")]
    public bool isCurrency = false;
    public CurrencyType currencyType = CurrencyType.None;

    [Header("Uses")]
    public int maxUses = 0;

    [Header("Capabilities")]
    public List<CardCapabilityType> capabilities = new();

    [Header("Timed Transformation")]
    // Hook de authoring para futuras transformaciones temporales
    // de una sola carta. La logica runtime vive fuera de CardData.
    public CardTransformationRule transformationRule;

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        if (Application.isPlaying)
            return;

        CardDataValidationUtility.ValidateAndLog(this);
    }
#endif
}
