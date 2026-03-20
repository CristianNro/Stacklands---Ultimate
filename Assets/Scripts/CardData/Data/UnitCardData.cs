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

    [Header("Combat")]
    public int damage = 1;
    public int armor = 0;
    public float attackSpeed = 1f;
    public float attackRange = 1f;

    [Header("Work")]
    public float workSpeed = 1f;

    [Header("Needs")]
    public float maxHunger = 100f;
    public float hungerDecayRate = 1f;

    [Header("Equipment")]
    public bool canEquipWeapon = false;
    public bool canEquipArmor = false;
    public bool canEquipTool = false;
}
