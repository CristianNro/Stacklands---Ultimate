using System.Collections.Generic;
using UnityEngine;
using StacklandsLike.Cards;

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

    [Header("Interaction")]
    // Future contract: stack logic must eventually respect this.
    public bool stackable = true;
    // Future contract: drag/drop logic must eventually respect this.
    public bool isMovable = true;

    [Header("Balance")]
    // Future contract: stack capacity will be limited by total weight.
    public float weight = 1f;
    public int value = 0;

    [Header("Economy")]
    public bool isCurrency = false;
    public CurrencyType currencyType = CurrencyType.None;

    [Header("Uses")]
    public int maxUses = 0;

    [Header("Tags")]
    public List<string> tags = new();
}
