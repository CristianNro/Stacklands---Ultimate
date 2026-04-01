using UnityEngine;
using StacklandsLike.Cards;

[CreateAssetMenu(fileName = "UnitCard", menuName = "Cards/Unit Card")]
public class UnitCardData : CardData
{
    [Header("Unit Identity")]
    public UnitRole unitRole;
    public FactionType faction = FactionType.Player;

    [Header("Health")]
    public int maxHealth = 10;

    [Header("Needs")]
    public float maxHunger = 100f;
}
