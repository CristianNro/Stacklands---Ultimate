using System;
using StacklandsLike.Cards;
using UnityEngine;

[Serializable]
public class RecipeDurationCapabilityModifier
{
    [Header("Capability Match")]
    public CardCapabilityType capability = CardCapabilityType.None;

    [Header("Duration Effect")]
    [Min(0.01f)]
    public float multiplier = 1f;

    [Min(1)]
    public int maxApplications = 1;
}
