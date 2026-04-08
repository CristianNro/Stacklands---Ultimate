using UnityEngine;

[System.Serializable]
public class EnemyRandomDropEntry
{
    public CardData card;
    [Range(0f, 1f)] public float dropChance = 1f;
    [Min(1)] public int minCount = 1;
    [Min(1)] public int maxCount = 1;
}
