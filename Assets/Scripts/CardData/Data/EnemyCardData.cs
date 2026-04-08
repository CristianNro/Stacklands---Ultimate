using System.Collections.Generic;
using UnityEngine;
using StacklandsLike.Cards;

[CreateAssetMenu(fileName = "EnemyCard", menuName = "Cards/Enemy Card")]
public class EnemyCardData : CombatantCardData
{
    [Header("Enemy Drops")]
    public List<EnemyGuaranteedDropEntry> guaranteedDrops = new();
    public List<EnemyRandomDropEntry> randomDrops = new();

#if UNITY_EDITOR
    private void Reset()
    {
        cardType = CardType.Enemy;
        faction = FactionType.Enemy;
    }

    protected override void OnValidate()
    {
        cardType = CardType.Enemy;
        faction = FactionType.Enemy;
        base.OnValidate();
    }
#endif
}
