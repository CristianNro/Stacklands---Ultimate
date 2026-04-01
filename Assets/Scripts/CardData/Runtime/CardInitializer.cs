using UnityEngine;

public class CardInitializer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CardInstance cardInstance;
    [SerializeField] private CardView cardView;

    private void Reset()
    {
        AutoAssignReferences();
    }

    private void Awake()
    {
        AutoAssignReferences();
    }

    private void AutoAssignReferences()
    {
        if (cardInstance == null) cardInstance = GetComponent<CardInstance>();
        if (cardView == null) cardView = GetComponent<CardView>();
    }

    public void Initialize(CardData data)
    {
        if (cardInstance == null)
        {
            Debug.LogError($"[{name}] No se encontró CardInstance.");
            return;
        }

        cardInstance.Initialize(data);

        if (cardView != null)
            cardView.Refresh();
    }
}
