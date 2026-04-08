using UnityEngine;

public class CardDropTargetInfo
{
    public CardDropTargetType primaryType;
    public GameObject hitObject;

    public MarketSellSlot marketSellSlot;
    public MarketPackPurchaseSlot marketPurchaseSlot;

    public CardView targetCard;
    public CardInstance targetInstance;
    public ContainerRuntime targetContainer;
    public CardStack targetStack;
    public CombatEncounter targetCombatEncounter;

    public bool HasMarketSellTarget => marketSellSlot != null;
    public bool HasMarketPurchaseTarget => marketPurchaseSlot != null;
    public bool HasContainerTarget => targetContainer != null;
    public bool HasStackTarget => targetStack != null;
    public bool HasCardTarget => targetCard != null;
    public bool HasCombatEncounterTarget => targetCombatEncounter != null;
}
