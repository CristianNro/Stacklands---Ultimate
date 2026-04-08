using UnityEngine;

// Define el offset visual que usa un stack cuando toma a esta carta como ancla.
// No forma parte del pipeline de drop; es solo un contrato de layout para stacks.
public class CardStackLayoutAnchor : MonoBehaviour
{
    [SerializeField] private Vector2 stackOffset = new Vector2(0f, -28f);

    public Vector2 GetStackOffset()
    {
        return stackOffset;
    }
}
