using UnityEngine;

public class BuildingRuntime : MonoBehaviour
{
    public BuildingCardData buildingData;

    public int currentHealth;
    public float currentProductionProgress;

    public void Initialize(BuildingCardData data)
    {
        buildingData = data;
        currentHealth = data.durability;
        currentProductionProgress = 0f;
    }
}
