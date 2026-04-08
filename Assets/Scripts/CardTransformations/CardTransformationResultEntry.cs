using System;
using UnityEngine;

[Serializable]
public sealed class CardTransformationResultEntry
{
    public CardData card;

    [Min(1)]
    public int count = 1;
}
