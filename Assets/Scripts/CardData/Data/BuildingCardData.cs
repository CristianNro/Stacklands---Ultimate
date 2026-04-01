using UnityEngine;

[CreateAssetMenu(fileName = "BuildingCard", menuName = "Cards/Building Card")]
public class BuildingCardData : CardData
{
    [Header("Durability")]
    public int durability = 20;
}
