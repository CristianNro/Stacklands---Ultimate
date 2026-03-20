using UnityEngine;

public class CardInitializer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CardInstance cardInstance;
    [SerializeField] private CardView cardView;
    [SerializeField] private UnitRuntime unitRuntime;
    [SerializeField] private BuildingRuntime buildingRuntime;

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
        if (unitRuntime == null) unitRuntime = GetComponent<UnitRuntime>();
        if (buildingRuntime == null) buildingRuntime = GetComponent<BuildingRuntime>();
    }

    public void Initialize(CardData data)
    {
        if (cardInstance == null)
        {
            Debug.LogError($"[{name}] No se encontró CardInstance.");
            return;
        }

        cardInstance.Initialize(data);

        if (unitRuntime != null)
            unitRuntime.enabled = false;

        if (buildingRuntime != null)
            buildingRuntime.enabled = false;

        if (data is UnitCardData unitData && unitRuntime != null)
        {
            unitRuntime.enabled = true;
            unitRuntime.Initialize(unitData);
        }

        if (data is BuildingCardData buildingData && buildingRuntime != null)
        {
            buildingRuntime.enabled = true;
            buildingRuntime.Initialize(buildingData);
        }

        if (cardView != null)
            cardView.Refresh();
    }
}