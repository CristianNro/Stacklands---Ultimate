using System.Collections.Generic;
using UnityEngine;

// ============================================================
// CombatEncounterFactory
// ------------------------------------------------------------
// Boundary de creacion para encuentros de combate.
//
// Responsabilidades:
// - crear el GameObject/root del encuentro
// - inicializar `CombatEncounter`
// - registrar participantes por bando
// - marcar a los participantes como ocupados y en combate
//
// En esta etapa:
// - no decide triggers automaticos de combate
// - no coloca visuales de formacion
// - no expulsa todavia a las cartas de otros sistemas por su cuenta
// ============================================================
public class CombatEncounterFactory : MonoBehaviour
{
    public static CombatEncounterFactory Instance { get; private set; }

    public event System.Action<CombatEncounter> OnEncounterCreated;

    [Header("Creation")]
    [SerializeField] private string encounterObjectName = "CombatEncounter";

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public CombatEncounter CreateEncounter(CardInstance friendly, CardInstance enemy)
    {
        if (friendly == null || enemy == null)
            return null;

        List<CardInstance> friendlyParticipants = new List<CardInstance> { friendly };
        List<CardInstance> enemyParticipants = new List<CardInstance> { enemy };
        return CreateEncounter(friendlyParticipants, enemyParticipants);
    }

    public CombatEncounter CreateEncounter(
        IReadOnlyList<CardInstance> friendlyParticipants,
        IReadOnlyList<CardInstance> enemyParticipants,
        Vector2? explicitAnchorPosition = null)
    {
        List<CardInstance> validFriendly = CollectValidParticipants(friendlyParticipants);
        List<CardInstance> validEnemy = CollectValidParticipants(enemyParticipants);

        if (validFriendly.Count == 0 || validEnemy.Count == 0)
            return null;

        Vector2 anchorPosition = explicitAnchorPosition ?? ResolveAnchorPosition(validFriendly, validEnemy);
        RectTransform encounterRoot = CreateEncounterRoot(anchorPosition);
        if (encounterRoot == null)
            return null;

        CombatEncounter encounter = encounterRoot.GetComponent<CombatEncounter>();
        if (encounter == null)
            encounter = encounterRoot.gameObject.AddComponent<CombatEncounter>();

        if (encounterRoot.GetComponent<CombatEncounterVisuals>() == null)
            encounterRoot.gameObject.AddComponent<CombatEncounterVisuals>();

        encounter.Initialize(anchorPosition);

        RegisterParticipants(encounter, validFriendly, CombatTeam.Friendly);
        RegisterParticipants(encounter, validEnemy, CombatTeam.Enemy);

        OnEncounterCreated?.Invoke(encounter);
        return encounter;
    }

    public bool TryAddParticipantsToEncounter(CombatEncounter encounter, IReadOnlyList<CardInstance> participants, CombatTeam team)
    {
        if (encounter == null || !encounter.IsActive() || participants == null || team == CombatTeam.None)
            return false;

        List<CardInstance> validParticipants = CollectValidParticipants(participants);
        if (validParticipants.Count == 0)
            return false;

        bool addedAny = false;

        for (int i = 0; i < validParticipants.Count; i++)
        {
            CardInstance instance = validParticipants[i];
            if (instance == null)
                continue;

            CardStack currentStack = instance.CurrentStack;
            if (currentStack != null && instance.View != null)
                currentStack.RemoveCard(instance.View);

            CombatParticipantRuntime runtime = instance.CombatParticipantRuntime;
            if (runtime == null || !runtime.isActiveAndEnabled)
                continue;

            if (!encounter.TryAddParticipant(instance, team))
                continue;

            runtime.EnterCombat(encounter.EncounterId, team);
            instance.SetBusy(true);
            addedAny = true;
        }

        return addedAny;
    }

    private void RegisterParticipants(CombatEncounter encounter, IReadOnlyList<CardInstance> participants, CombatTeam team)
    {
        if (encounter == null || participants == null || team == CombatTeam.None)
            return;

        for (int i = 0; i < participants.Count; i++)
        {
            CardInstance instance = participants[i];
            if (instance == null)
                continue;

            CombatParticipantRuntime runtime = instance.CombatParticipantRuntime;
            if (runtime == null || !runtime.isActiveAndEnabled)
                continue;

            if (!encounter.TryAddParticipant(instance, team))
                continue;

            runtime.EnterCombat(encounter.EncounterId, team);
            instance.SetBusy(true);
        }
    }

    private List<CardInstance> CollectValidParticipants(IReadOnlyList<CardInstance> source)
    {
        List<CardInstance> result = new List<CardInstance>();
        if (source == null)
            return result;

        for (int i = 0; i < source.Count; i++)
        {
            CardInstance instance = source[i];
            if (!IsValidParticipant(instance))
                continue;

            result.Add(instance);
        }

        return result;
    }

    private bool IsValidParticipant(CardInstance instance)
    {
        if (instance == null || instance.data == null)
            return false;

        CombatParticipantRuntime runtime = instance.CombatParticipantRuntime;
        if (runtime == null || !runtime.isActiveAndEnabled)
            return false;

        if (runtime.IsInCombat || runtime.IsDead())
            return false;

        return runtime.CombatantData != null;
    }

    private Vector2 ResolveAnchorPosition(IReadOnlyList<CardInstance> friendlyParticipants, IReadOnlyList<CardInstance> enemyParticipants)
    {
        Vector2 sum = Vector2.zero;
        int count = 0;

        AccumulatePositions(friendlyParticipants, ref sum, ref count);
        AccumulatePositions(enemyParticipants, ref sum, ref count);

        if (count <= 0)
            return Vector2.zero;

        return sum / count;
    }

    private void AccumulatePositions(IReadOnlyList<CardInstance> participants, ref Vector2 sum, ref int count)
    {
        if (participants == null)
            return;

        for (int i = 0; i < participants.Count; i++)
        {
            CardInstance instance = participants[i];
            if (instance == null || instance.RectTransform == null)
                continue;

            Vector2 boardPoint = BoardRoot.Instance != null
                ? BoardRoot.Instance.GetBoardPointFromWorldPosition(instance.RectTransform.position)
                : instance.RectTransform.anchoredPosition;

            sum += boardPoint;
            count++;
        }
    }

    private RectTransform CreateEncounterRoot(Vector2 anchorPosition)
    {
        if (BoardRoot.Instance != null)
            return BoardRoot.Instance.CreateBoardRectTransform(encounterObjectName, anchorPosition, clampToBoard: true);

        GameObject encounterGO = new GameObject(encounterObjectName, typeof(RectTransform));
        RectTransform encounterRect = encounterGO.GetComponent<RectTransform>();
        encounterRect.anchorMin = new Vector2(0.5f, 0.5f);
        encounterRect.anchorMax = new Vector2(0.5f, 0.5f);
        encounterRect.pivot = new Vector2(0.5f, 0.5f);
        encounterRect.localScale = Vector3.one;
        encounterRect.localRotation = Quaternion.identity;
        encounterRect.anchoredPosition = anchorPosition;
        return encounterRect;
    }
}
