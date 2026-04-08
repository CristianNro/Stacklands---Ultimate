using UnityEngine;

[System.Serializable]
public class EnemyGuaranteedDropEntry
{
    public CardData card;
    [Min(1)] public int minCount = 1;
    [Min(1)] public int maxCount = 1;
}
