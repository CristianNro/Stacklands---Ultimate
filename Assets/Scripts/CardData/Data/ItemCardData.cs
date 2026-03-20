using UnityEngine;
using StacklandsLike.Cards;

[CreateAssetMenu(fileName = "ItemCard", menuName = "Cards/Item Card")]
public class ItemCardData : CardData
{
    [Header("Item")]
    public ItemType itemType;

    [Header("Modifiers")]
    public int bonusDamage = 0;
    public int bonusArmor = 0;
    public float bonusWorkSpeed = 0f;

    [Header("Durability")]
    public int maxDurability = 0;
}
