using UnityEngine;
using StacklandsLike.Cards;

[CreateAssetMenu(fileName = "ResourceCard", menuName = "Cards/Resource Card")]
public class ResourceCardData : CardData
{
    [Header("Resource")]
    public ResourceType resourceType;
}
