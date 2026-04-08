using UnityEngine;

public class CardDropContext
{
    public CardView draggedCard;
    public CardInstance draggedInstance;
    public CardStack draggedStack;
    public bool startedAsStackDrag;

    public GameObject hitObject;
    public CardDropTargetInfo targetInfo;

    public Vector2 boardPoint;

    public CardView targetCard => targetInfo != null ? targetInfo.targetCard : null;
    public CardInstance targetInstance => targetInfo != null ? targetInfo.targetInstance : null;
    public ContainerRuntime targetContainer => targetInfo != null ? targetInfo.targetContainer : null;
    public CardStack targetStack => targetInfo != null ? targetInfo.targetStack : null;
    public CombatEncounter targetCombatEncounter => targetInfo != null ? targetInfo.targetCombatEncounter : null;
}
