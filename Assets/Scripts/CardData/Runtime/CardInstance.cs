using UnityEngine;

public class CardInstance : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string runtimeId;

    [Header("Definition")]
    public CardData data;

    [Header("Runtime State")]
    public bool isDragging;
    public bool isSelected;
    public bool isBusy;

    public int usesRemaining;

    [Header("Stack")]
    public CardStack currentStack;

    [Header("Cached References")]
    public RectTransform RectTransform { get; private set; }
    public CardView View { get; private set; }

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        View = GetComponent<CardView>();

        if (string.IsNullOrWhiteSpace(runtimeId))
            runtimeId = System.Guid.NewGuid().ToString();
    }

    public string RuntimeId => runtimeId;

    public void Initialize(CardData data)
    {
        this.data = data;
        usesRemaining = data != null ? data.maxUses : 0;
    }

    public bool HasTag(string tag)
    {
        return data != null && data.tags.Contains(tag);
    }

    public bool HasLimitedUses() => usesRemaining > 0;

    public bool ConsumeUseIfNeeded()
    {
        if (usesRemaining == 0) return false;

        usesRemaining--;
        return usesRemaining <= 0;
    }

    private void OnDestroy()
    {
        if (BoardRoot.Instance != null)
            BoardRoot.Instance.UnregisterCard(this);
    }
}