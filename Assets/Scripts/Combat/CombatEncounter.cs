using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ============================================================
// CombatEncounter
// ------------------------------------------------------------
// Agregado principal de una batalla.
//
// Ownership:
// - mantiene los dos bandos
// - expone estado del encuentro
// - guarda la posicion/ancla del combate
//
// En esta etapa:
// - aun no resuelve dano ni timers
// - aun no coloca visuales
// - solo define la frontera del encuentro como entidad propia
// ============================================================
public class CombatEncounter : MonoBehaviour, ICardDropTargetSource
{
    private static readonly Dictionary<string, CombatEncounter> ActiveEncountersById = new Dictionary<string, CombatEncounter>();
    private static readonly HashSet<CombatEncounter> ActiveEncounters = new HashSet<CombatEncounter>();

    public static event System.Action<CombatEncounter> OnEncounterAvailable;
    public static event System.Action<CombatEncounter> OnEncounterUnavailable;

    [Header("Identity")]
    [SerializeField] private string encounterId;

    [Header("Runtime State")]
    [SerializeField] private CombatEncounterState state = CombatEncounterState.Active;
    [SerializeField] private Vector2 boardAnchorPosition;

    [Header("Participants")]
    [SerializeField] private List<CardInstance> friendlyParticipants = new List<CardInstance>();
    [SerializeField] private List<CardInstance> enemyParticipants = new List<CardInstance>();

    public string EncounterId => encounterId;
    public CombatEncounterState State => state;
    public Vector2 BoardAnchorPosition => boardAnchorPosition;
    public IReadOnlyList<CardInstance> FriendlyParticipants => friendlyParticipants;
    public IReadOnlyList<CardInstance> EnemyParticipants => enemyParticipants;

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(encounterId))
            encounterId = System.Guid.NewGuid().ToString();
    }

    private void OnEnable()
    {
        if (!string.IsNullOrWhiteSpace(encounterId))
            ActiveEncountersById[encounterId] = this;
        ActiveEncounters.Add(this);

        OnEncounterAvailable?.Invoke(this);
    }

    private void OnDisable()
    {
        if (!string.IsNullOrWhiteSpace(encounterId) && ActiveEncountersById.TryGetValue(encounterId, out CombatEncounter current) && current == this)
            ActiveEncountersById.Remove(encounterId);
        ActiveEncounters.Remove(this);

        OnEncounterUnavailable?.Invoke(this);
    }

    public void Initialize(Vector2 anchorPosition)
    {
        if (string.IsNullOrWhiteSpace(encounterId))
            encounterId = System.Guid.NewGuid().ToString();

        state = CombatEncounterState.Active;
        boardAnchorPosition = anchorPosition;
        friendlyParticipants.Clear();
        enemyParticipants.Clear();

        if (!string.IsNullOrWhiteSpace(encounterId))
            ActiveEncountersById[encounterId] = this;
    }

    public void SetBoardAnchorPosition(Vector2 anchorPosition)
    {
        boardAnchorPosition = anchorPosition;
    }

    public bool IsActive()
    {
        return state == CombatEncounterState.Active;
    }

    public bool IsFinished()
    {
        return state == CombatEncounterState.Finished;
    }

    public void MarkFinished()
    {
        state = CombatEncounterState.Finished;
    }

    public bool TryAddParticipant(CardInstance instance, CombatTeam team)
    {
        if (instance == null || team == CombatTeam.None)
            return false;

        RemoveParticipant(instance);

        List<CardInstance> targetList = team == CombatTeam.Friendly
            ? friendlyParticipants
            : enemyParticipants;

        targetList.Add(instance);
        return true;
    }

    public bool RemoveParticipant(CardInstance instance)
    {
        if (instance == null)
            return false;

        bool removedFriendly = friendlyParticipants.Remove(instance);
        bool removedEnemy = enemyParticipants.Remove(instance);
        return removedFriendly || removedEnemy;
    }

    public bool ContainsParticipant(CardInstance instance)
    {
        if (instance == null)
            return false;

        return friendlyParticipants.Contains(instance) || enemyParticipants.Contains(instance);
    }

    public IReadOnlyList<CardInstance> GetParticipants(CombatTeam team)
    {
        return team == CombatTeam.Friendly ? friendlyParticipants : enemyParticipants;
    }

    public int GetLivingParticipantCount(CombatTeam team)
    {
        IReadOnlyList<CardInstance> participants = GetParticipants(team);
        if (participants == null)
            return 0;

        int count = 0;

        for (int i = 0; i < participants.Count; i++)
        {
            CardInstance instance = participants[i];
            CombatParticipantRuntime runtime = instance != null ? instance.CombatParticipantRuntime : null;
            if (runtime == null || runtime.IsDead())
                continue;

            count++;
        }

        return count;
    }

    public bool IsTeamDefeated(CombatTeam team)
    {
        return GetLivingParticipantCount(team) <= 0;
    }

    public bool HasBothTeams()
    {
        return friendlyParticipants.Count > 0 && enemyParticipants.Count > 0;
    }

    public static CombatEncounter FindById(string encounterId)
    {
        if (string.IsNullOrWhiteSpace(encounterId))
            return null;

        ActiveEncountersById.TryGetValue(encounterId, out CombatEncounter encounter);
        return encounter;
    }

    public static CombatEncounter FindAtBoardPoint(Vector2 boardPoint)
    {
        foreach (CombatEncounter encounter in ActiveEncounters)
        {
            if (encounter == null || !encounter.isActiveAndEnabled)
                continue;

            Rect bounds = encounter.GetBoundsInBoardSpace();
            if (bounds.Contains(boardPoint))
                return encounter;
        }

        return null;
    }

    public static CombatEncounter FindOverlapping(RectTransform movingRect)
    {
        if (movingRect == null)
            return null;

        Rect movingBounds = GetRectBoundsInBoardSpace(movingRect);
        if (movingBounds.width <= 0f || movingBounds.height <= 0f)
            return null;

        foreach (CombatEncounter encounter in ActiveEncounters)
        {
            if (encounter == null || !encounter.isActiveAndEnabled)
                continue;

            Rect bounds = encounter.GetBoundsInBoardSpace();
            if (bounds.width <= 0f || bounds.height <= 0f)
                continue;

            if (bounds.Overlaps(movingBounds))
                return encounter;
        }

        return null;
    }

    public Rect GetBoundsInBoardSpace()
    {
        return GetRectBoundsInBoardSpace(transform as RectTransform);
    }

    private static Rect GetRectBoundsInBoardSpace(RectTransform rectTransform)
    {
        if (BoardRoot.Instance == null || BoardRoot.Instance.CardsContainer == null || rectTransform == null)
            return new Rect();

        Vector3[] worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);

        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        RectTransform boardRect = BoardRoot.Instance.CardsContainer;

        for (int i = 0; i < worldCorners.Length; i++)
        {
            Vector3 localCorner3 = boardRect.InverseTransformPoint(worldCorners[i]);
            Vector2 localCorner = new Vector2(localCorner3.x, localCorner3.y);
            min = Vector2.Min(min, localCorner);
            max = Vector2.Max(max, localCorner);
        }

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    public void PopulateDropTargetInfo(CardDropTargetInfo targetInfo)
    {
        if (targetInfo == null)
            return;

        targetInfo.targetCombatEncounter = this;
        if (targetInfo.primaryType == CardDropTargetType.None)
            targetInfo.primaryType = CardDropTargetType.CombatEncounter;
    }
}
