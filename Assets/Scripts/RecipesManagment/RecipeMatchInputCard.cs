using System.Collections.Generic;
using StacklandsLike.Cards;

public class RecipeMatchInputCard
{
    public CardData data;
    public readonly List<CardCapabilityType> capabilities = new List<CardCapabilityType>();

    public bool HasCapability(CardCapabilityType capability)
    {
        return capability != CardCapabilityType.None && capabilities.Contains(capability);
    }
}
