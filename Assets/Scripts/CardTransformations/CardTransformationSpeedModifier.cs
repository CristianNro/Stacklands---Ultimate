using System;
using StacklandsLike.Cards;
using UnityEngine;

[Serializable]
public sealed class CardTransformationSpeedModifier
{
    public CardCapabilityType capability = CardCapabilityType.None;

    [Min(0.01f)]
    public float speedMultiplier = 1f;
}
