using UnityEngine;
using StacklandsLike.Cards;

[System.Serializable]
public class DamageTypeModifierEntry
{
    public DamageType damageType;
    [Range(-1f, 10f)] public float percentModifier = 0f;
}
