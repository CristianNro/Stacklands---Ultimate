using UnityEngine;

public class UnitRuntime : MonoBehaviour
{
    public UnitCardData unitData;

    public int currentHealth;
    public float currentHunger;

    public CardInstance equippedWeapon;
    public CardInstance equippedArmor;
    public CardInstance equippedTool;

    public void Initialize(UnitCardData data)
    {
        unitData = data;
        currentHealth = data.maxHealth;
        currentHunger = data.maxHunger;
    }
}
