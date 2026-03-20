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
    public bool stackable = true;
    public bool isMovable = true;
    public bool isConsumable = false;
    public bool isDestroyable = true;

    [Header("Balance")]
    public float weight = 1f;
    public int value = 0;

    [Header("Durability")]

    public bool consumeOnRecipe = true;
    public int maxUses = 0;

    [Header("Tags")]
    public List<string> tags = new();
}
