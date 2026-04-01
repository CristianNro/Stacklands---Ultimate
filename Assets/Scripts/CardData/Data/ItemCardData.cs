using UnityEngine;
using StacklandsLike.Cards;

[CreateAssetMenu(fileName = "ItemCard", menuName = "Cards/Item Card")]
public class ItemCardData : CardData
{
    [Header("Item")]
    public ItemType itemType;
}
