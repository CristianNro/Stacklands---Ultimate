using System.Collections.Generic;

// ============================================================
// StackRules
// ------------------------------------------------------------
// Fuente central de reglas para stacking.
//
// En esta etapa mantiene una politica global y simple:
// - solo cartas stackeables pueden entrar en stacks
// - el peso total del stack no puede exceder el maximo global
// ============================================================
public static class StackRules
{
    public const float MaxTotalWeight = 10f;

    public static bool CanCardParticipateInStack(CardInstance instance)
    {
        return instance != null && instance.IsStackable();
    }

    public static bool CanCardsExistInSameStack(IEnumerable<CardInstance> existingCards, IReadOnlyList<CardInstance> incomingCards, out string rejectionReason)
    {
        rejectionReason = null;

        float totalWeight = 0f;

        if (existingCards != null)
        {
            foreach (CardInstance existingInstance in existingCards)
            {
                if (!CanCardParticipateInStack(existingInstance))
                {
                    rejectionReason = "el stack contiene una carta no stackeable o invalida";
                    return false;
                }

                totalWeight += existingInstance.GetWeight();
            }
        }

        if (incomingCards == null || incomingCards.Count == 0)
        {
            rejectionReason = "no hay cartas entrantes";
            return false;
        }

        for (int i = 0; i < incomingCards.Count; i++)
        {
            CardInstance incomingInstance = incomingCards[i];
            if (!CanCardParticipateInStack(incomingInstance))
            {
                rejectionReason = "una de las cartas entrantes no es stackeable o no tiene CardInstance";
                return false;
            }

            totalWeight += incomingInstance.GetWeight();
        }

        if (totalWeight > MaxTotalWeight)
        {
            rejectionReason = $"el peso total excederia el maximo permitido ({totalWeight:0.##} / {MaxTotalWeight:0.##})";
            return false;
        }

        return true;
    }
}
