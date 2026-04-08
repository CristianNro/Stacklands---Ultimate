using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StacklandsLike.Cards;

// ============================================================
// CombatEncounterVisuals
// ------------------------------------------------------------
// Presentacion visual minima de un encuentro de combate.
//
// Responsabilidades:
// - posicionar ambos bandos en lineas enfrentadas
// - mantener las cartas legibles como encuentro, no como stack
//
// No:
// - resuelve dano
// - decide targets
// - decide cuando termina el encuentro
// ============================================================
public class CombatEncounterVisuals : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CombatEncounter encounter;

    [Header("Layout")]
    [SerializeField] private float horizontalSpacing = 110f;
    [SerializeField] private float frontlineOffsetY = 82f;
    [SerializeField] private float lineSpacing = 92f;
    [SerializeField] private Vector2 encounterPadding = new Vector2(28f, 28f);

    private RectTransform encounterRect;

    private void Awake()
    {
        if (encounter == null)
            encounter = GetComponent<CombatEncounter>();

        encounterRect = transform as RectTransform;
        EnsureDropSurface();
    }

    private void LateUpdate()
    {
        if (encounter == null || !encounter.IsActive())
            return;

        RefreshLayout();
    }

    private void RefreshLayout()
    {
        PositionTeamFormation(encounter.FriendlyParticipants, encounter.BoardAnchorPosition, true);
        PositionTeamFormation(encounter.EnemyParticipants, encounter.BoardAnchorPosition, false);
        RefreshEncounterBounds();
    }

    private void PositionTeamFormation(IReadOnlyList<CardInstance> participants, Vector2 anchorPosition, bool isFriendlyTeam)
    {
        if (participants == null)
            return;

        List<CombatLineRole> occupiedLines = CombatFormationUtility.CollectOccupiedLines(participants);
        if (occupiedLines.Count == 0)
            return;

        List<CardInstance> lineParticipants = new List<CardInstance>();
        float direction = isFriendlyTeam ? 1f : -1f;

        for (int lineIndex = 0; lineIndex < occupiedLines.Count; lineIndex++)
        {
            CombatLineRole lineRole = occupiedLines[lineIndex];
            CombatFormationUtility.CollectLivingParticipantsForLine(participants, lineRole, lineParticipants);
            if (lineParticipants.Count == 0)
                continue;

            float rowY = anchorPosition.y + (direction * (frontlineOffsetY + (lineSpacing * lineIndex)));
            Vector2 rowCenter = new Vector2(anchorPosition.x, rowY);
            PositionLine(lineParticipants, rowCenter);
        }
    }

    private void PositionLine(IReadOnlyList<CardInstance> activeParticipants, Vector2 rowCenter)
    {
        if (activeParticipants == null || activeParticipants.Count == 0)
            return;

        float totalWidth = horizontalSpacing * (activeParticipants.Count - 1);
        float startX = rowCenter.x - (totalWidth * 0.5f);

        for (int i = 0; i < activeParticipants.Count; i++)
        {
            CardInstance participant = activeParticipants[i];
            if (participant == null || participant.RectTransform == null)
                continue;

            Vector2 desiredPosition = new Vector2(startX + (horizontalSpacing * i), rowCenter.y);
            PlaceParticipant(participant.RectTransform, desiredPosition);
            participant.RectTransform.SetAsLastSibling();
        }
    }

    private void PlaceParticipant(RectTransform participantRect, Vector2 desiredBoardPosition)
    {
        if (participantRect == null)
            return;

        if (BoardRoot.Instance != null)
        {
            BoardRoot.Instance.TryPlaceRectOnBoard(participantRect, desiredBoardPosition, clampToBoard: true);
            return;
        }

        participantRect.anchoredPosition = desiredBoardPosition;
    }

    private void RefreshEncounterBounds()
    {
        if (encounterRect == null)
            return;

        Rect? combinedBounds = GetCombinedParticipantBounds();
        if (!combinedBounds.HasValue)
            return;

        Rect bounds = combinedBounds.Value;
        encounterRect.anchoredPosition = bounds.center;
        encounterRect.sizeDelta = new Vector2(
            bounds.width + encounterPadding.x,
            bounds.height + encounterPadding.y);
    }

    private Rect? GetCombinedParticipantBounds()
    {
        bool hasBounds = false;
        Rect combined = new Rect();

        hasBounds |= TryEncapsulate(encounter.FriendlyParticipants, ref combined);
        hasBounds |= TryEncapsulate(encounter.EnemyParticipants, ref combined);

        return hasBounds ? combined : (Rect?)null;
    }

    private bool TryEncapsulate(IReadOnlyList<CardInstance> participants, ref Rect combined)
    {
        if (participants == null)
            return false;

        bool contributed = false;

        for (int i = 0; i < participants.Count; i++)
        {
            CardInstance participant = participants[i];
            RectTransform rect = participant != null ? participant.RectTransform : null;
            if (rect == null)
                continue;

            Rect bounds = GetBoundsInBoardSpace(rect);
            if (bounds.width <= 0f || bounds.height <= 0f)
                continue;

            if (!contributed)
            {
                if (combined.width <= 0f && combined.height <= 0f)
                    combined = bounds;
                else
                    combined = Encapsulate(combined, bounds);
            }
            else
            {
                combined = Encapsulate(combined, bounds);
            }

            contributed = true;
        }

        return contributed;
    }

    private Rect GetBoundsInBoardSpace(RectTransform rect)
    {
        if (rect == null || BoardRoot.Instance == null || BoardRoot.Instance.CardsContainer == null)
            return new Rect();

        Vector3[] worldCorners = new Vector3[4];
        rect.GetWorldCorners(worldCorners);

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

    private Rect Encapsulate(Rect a, Rect b)
    {
        return Rect.MinMaxRect(
            Mathf.Min(a.xMin, b.xMin),
            Mathf.Min(a.yMin, b.yMin),
            Mathf.Max(a.xMax, b.xMax),
            Mathf.Max(a.yMax, b.yMax));
    }

    private void EnsureDropSurface()
    {
        if (gameObject.GetComponent<Image>() != null)
            return;

        Image image = gameObject.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0f);
        image.raycastTarget = true;
    }
}
