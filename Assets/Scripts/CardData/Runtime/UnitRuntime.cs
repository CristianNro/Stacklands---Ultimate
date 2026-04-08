using UnityEngine;

public class UnitRuntime : MonoBehaviour
{
    public SurvivorUnitCardData unitData;

    public float currentHunger;

    public CardInstance equippedWeapon;
    public CardInstance equippedArmor;
    public CardInstance equippedTool;

    public void Initialize(SurvivorUnitCardData data)
    {
        unitData = data;
        currentHunger = data.maxHunger;
    }
}
