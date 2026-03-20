using UnityEngine;

[CreateAssetMenu(fileName = "BuildingCard", menuName = "Cards/Building Card")]
public class BuildingCardData : CardData
{
    [Header("Durability")]
    public int durability = 20;

    [Header("Capacity")]
    public int workerCapacity = 0;
    public int residentCapacity = 0;
    public int storageCapacity = 0;

    [Header("Production")]
    public bool needsWorker = false;
    public bool canProduce = false;
    public float productionTime = 0f;

    [Header("Construction")]
    public float buildTime = 0f;
}
