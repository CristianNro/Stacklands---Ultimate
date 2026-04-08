using System.Collections.Generic;
using StacklandsLike.Cards;

// ============================================================
// CombatFormationUtility
// ------------------------------------------------------------
// Reglas derivadas de formacion para encuentros de combate.
//
// Responsabilidades:
// - definir prioridad de lineas
// - leer el rol tactico de un participante
// - encontrar la frontline activa de un bando
// - recolectar participantes targeteables
//
// No:
// - resuelve dano
// - modifica encuentros
// - decide timers
// ============================================================
public static class CombatFormationUtility
{
    private static readonly CombatLineRole[] LinePriority =
    {
        CombatLineRole.Tank,
        CombatLineRole.Melee,
        CombatLineRole.Ranged
    };

    public static IReadOnlyList<CombatLineRole> OrderedLinePriority => LinePriority;

    public static CombatLineRole GetLineRole(CardInstance participant)
    {
        CombatantCardData data = participant != null && participant.CombatParticipantRuntime != null
            ? participant.CombatParticipantRuntime.CombatantData
            : null;

        return data != null ? data.combatLineRole : CombatLineRole.Melee;
    }

    public static bool TryGetFrontActiveLine(CombatEncounter encounter, CombatTeam team, out CombatLineRole lineRole)
    {
        lineRole = CombatLineRole.Melee;

        IReadOnlyList<CardInstance> participants = encounter != null ? encounter.GetParticipants(team) : null;
        if (participants == null)
            return false;

        for (int lineIndex = 0; lineIndex < LinePriority.Length; lineIndex++)
        {
            CombatLineRole candidateRole = LinePriority[lineIndex];
            if (HasLivingParticipantsInLine(participants, candidateRole))
            {
                lineRole = candidateRole;
                return true;
            }
        }

        return false;
    }

    public static List<CardInstance> CollectTargetableParticipants(CombatEncounter encounter, CombatTeam team)
    {
        List<CardInstance> result = new List<CardInstance>();
        if (encounter == null)
            return result;

        if (!TryGetFrontActiveLine(encounter, team, out CombatLineRole frontLine))
            return result;

        IReadOnlyList<CardInstance> participants = encounter.GetParticipants(team);
        CollectLivingParticipantsForLine(participants, frontLine, result);
        return result;
    }

    public static List<CombatLineRole> CollectOccupiedLines(IReadOnlyList<CardInstance> participants)
    {
        List<CombatLineRole> result = new List<CombatLineRole>();
        if (participants == null)
            return result;

        for (int lineIndex = 0; lineIndex < LinePriority.Length; lineIndex++)
        {
            CombatLineRole role = LinePriority[lineIndex];
            if (HasLivingParticipantsInLine(participants, role))
                result.Add(role);
        }

        return result;
    }

    public static List<CardInstance> CollectLivingParticipantsForLine(IReadOnlyList<CardInstance> participants, CombatLineRole role)
    {
        List<CardInstance> result = new List<CardInstance>();
        CollectLivingParticipantsForLine(participants, role, result);
        return result;
    }

    public static void CollectLivingParticipantsForLine(IReadOnlyList<CardInstance> participants, CombatLineRole role, List<CardInstance> result)
    {
        if (result == null)
            return;

        result.Clear();
        if (participants == null)
            return;

        for (int i = 0; i < participants.Count; i++)
        {
            CardInstance participant = participants[i];
            CombatParticipantRuntime runtime = participant != null ? participant.CombatParticipantRuntime : null;
            if (runtime == null || runtime.IsDead())
                continue;

            if (runtime.CombatantData == null || runtime.CombatantData.combatLineRole != role)
                continue;

            result.Add(participant);
        }
    }

    private static bool HasLivingParticipantsInLine(IReadOnlyList<CardInstance> participants, CombatLineRole role)
    {
        if (participants == null)
            return false;

        for (int i = 0; i < participants.Count; i++)
        {
            CardInstance participant = participants[i];
            CombatParticipantRuntime runtime = participant != null ? participant.CombatParticipantRuntime : null;
            if (runtime == null || runtime.IsDead())
                continue;

            if (runtime.CombatantData != null && runtime.CombatantData.combatLineRole == role)
                return true;
        }

        return false;
    }
}
