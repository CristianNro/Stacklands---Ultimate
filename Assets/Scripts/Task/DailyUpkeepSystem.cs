using System.Collections.Generic;
using UnityEngine;
using StacklandsLike.Cards;

// ============================================================
// DailyUpkeepSystem
// ------------------------------------------------------------
// Primer sistema de mantenimiento diario.
//
// En esta etapa hace una sola cosa:
// - al final del dia resuelve alimentacion survivor por survivor
//
// Regla base actual:
// - cada survivor consume `dailyFoodConsumption` por dia
// - solo `FoodResourceCardData` cuenta como comida valida
// - la comida aporta `remainingFoodValue` runtime
// - una misma carta puede cubrir demanda parcial de varios dias
// - la carta solo se destruye cuando su comida restante llega a 0
// - un survivor sobrevive solo si se cubre todo su consumo diario
// - si no se cubre completo, el survivor muere
//
// Importante:
// - por ahora NO hay hambre parcial ni daño
// - la consecuencia de escasez es directa: muerte de la unidad no alimentada
// ============================================================
public class DailyUpkeepSystem : MonoBehaviour, IDayEndProcessor
{
    public event System.Action<DailyUpkeepResult> OnDailyUpkeepProcessed;

    public DailyUpkeepResult LastResult { get; private set; }

    public void ProcessDayEnd(int dayNumber)
    {
        LastResult = RunDailyUpkeep(dayNumber);
        OnDailyUpkeepProcessed?.Invoke(LastResult);
    }

    public DailyUpkeepResult RunDailyUpkeep(int dayNumber)
    {
        DailyUpkeepResult result = new DailyUpkeepResult
        {
            dayNumber = Mathf.Max(1, dayNumber)
        };

        BoardRoot board = BoardRoot.Instance;
        if (board == null)
            return result;

        List<CardInstance> units = CollectUnits(board.ActiveCards);
        List<CardView> foodCards = CollectFoodCards(board.ActiveCards);

        SortUnitsForFeeding(units);
        result.requiredFood = GetTotalRequiredFood(units);

        int nextFoodIndex = 0;

        for (int i = 0; i < units.Count; i++)
        {
            CardInstance unit = units[i];
            UnitCardData unitData = unit != null ? unit.data as UnitCardData : null;
            if (unit == null || unitData == null)
                continue;

            int requiredFoodForUnit = Mathf.Max(1, unitData.dailyFoodConsumption);
            DailyUpkeepResult.UnitFeedingRecord unitRecord = BuildUnitRecord(unit, requiredFoodForUnit);

            int fedAmount = FeedUnit(foodCards, ref nextFoodIndex, requiredFoodForUnit, result);
            result.consumedFood += fedAmount;

            if (fedAmount >= requiredFoodForUnit)
            {
                result.fedUnits.Add(unitRecord);
                continue;
            }

            result.deadUnits.Add(unitRecord);
            KillUnit(unit);
        }

        result.missingFood = Mathf.Max(0, result.requiredFood - result.consumedFood);
        return result;
    }

    private static List<CardInstance> CollectUnits(IReadOnlyList<CardInstance> activeCards)
    {
        List<CardInstance> result = new List<CardInstance>();
        if (activeCards == null)
            return result;

        for (int i = 0; i < activeCards.Count; i++)
        {
            CardInstance instance = activeCards[i];
            if (instance == null || instance.data == null)
                continue;

            if (instance.data is UnitCardData)
                result.Add(instance);
        }

        return result;
    }

    private static void SortUnitsForFeeding(List<CardInstance> units)
    {
        if (units == null)
            return;

        units.Sort(CompareUnitsForFeeding);
    }

    private static List<CardView> CollectFoodCards(IReadOnlyList<CardInstance> activeCards)
    {
        List<CardView> result = new List<CardView>();
        if (activeCards == null)
            return result;

        for (int i = 0; i < activeCards.Count; i++)
        {
            CardInstance instance = activeCards[i];
            if (instance == null || instance.data == null || instance.View == null)
                continue;

            if (IsFoodCard(instance.data))
                result.Add(instance.View);
        }

        // Orden determinista simple:
        // primero por posicion vertical, despues horizontal y por ultimo por runtimeId.
        result.Sort(CompareFoodCardsForConsumption);
        return result;
    }

    private static bool IsFoodCard(CardData cardData)
    {
        return cardData is FoodResourceCardData;
    }

    private static int GetTotalRequiredFood(IReadOnlyList<CardInstance> units)
    {
        if (units == null)
            return 0;

        int totalRequiredFood = 0;

        for (int i = 0; i < units.Count; i++)
        {
            CardInstance instance = units[i];
            UnitCardData unitData = instance != null ? instance.data as UnitCardData : null;
            if (unitData == null)
                continue;

            totalRequiredFood += Mathf.Max(1, unitData.dailyFoodConsumption);
        }

        return totalRequiredFood;
    }

    private static int CompareUnitsForFeeding(CardInstance a, CardInstance b)
    {
        if (ReferenceEquals(a, b))
            return 0;

        if (a == null)
            return 1;

        if (b == null)
            return -1;

        Vector3 aPosition = a.transform.position;
        Vector3 bPosition = b.transform.position;

        int compareY = -aPosition.y.CompareTo(bPosition.y);
        if (compareY != 0)
            return compareY;

        int compareX = aPosition.x.CompareTo(bPosition.x);
        if (compareX != 0)
            return compareX;

        return string.CompareOrdinal(a.RuntimeId, b.RuntimeId);
    }

    private static int CompareFoodCardsForConsumption(CardView a, CardView b)
    {
        if (ReferenceEquals(a, b))
            return 0;

        if (a == null)
            return 1;

        if (b == null)
            return -1;

        Vector3 aPosition = a.transform.position;
        Vector3 bPosition = b.transform.position;

        int compareY = -aPosition.y.CompareTo(bPosition.y);
        if (compareY != 0)
            return compareY;

        int compareX = aPosition.x.CompareTo(bPosition.x);
        if (compareX != 0)
            return compareX;

        string aRuntimeId = a.Instance != null ? a.Instance.RuntimeId : string.Empty;
        string bRuntimeId = b.Instance != null ? b.Instance.RuntimeId : string.Empty;
        return string.CompareOrdinal(aRuntimeId, bRuntimeId);
    }

    private static DailyUpkeepResult.UnitFeedingRecord BuildUnitRecord(CardInstance unit, int requiredFood)
    {
        if (unit == null || unit.data == null)
            return null;

        return new DailyUpkeepResult.UnitFeedingRecord
        {
            runtimeId = unit.RuntimeId,
            cardId = unit.data.id,
            displayName = string.IsNullOrWhiteSpace(unit.data.displayName) ? unit.data.cardName : unit.data.displayName,
            requiredFood = requiredFood
        };
    }

    private static int FeedUnit(
        IReadOnlyList<CardView> foodCards,
        ref int nextFoodIndex,
        int requiredFoodAmount,
        DailyUpkeepResult result)
    {
        if (foodCards == null || result == null || requiredFoodAmount <= 0)
            return 0;

        int fedAmount = 0;

        while (nextFoodIndex < foodCards.Count && fedAmount < requiredFoodAmount)
        {
            CardView foodCard = foodCards[nextFoodIndex];
            if (foodCard == null)
            {
                nextFoodIndex++;
                continue;
            }

            int missingForUnit = requiredFoodAmount - fedAmount;
            if (!ConsumeFoodCard(foodCard, missingForUnit, out DailyUpkeepResult.ConsumedFoodRecord record, out int foodUnitsProvided))
            {
                nextFoodIndex++;
                continue;
            }

            fedAmount += foodUnitsProvided;

            if (record != null)
                result.consumedCards.Add(record);

            CardInstance foodInstance = foodCard.Instance;
            FoodRuntime foodRuntime = foodInstance != null ? foodInstance.FoodRuntime : null;
            if (foodRuntime == null || foodRuntime.RemainingFoodValue <= 0)
                nextFoodIndex++;
        }

        return fedAmount;
    }

    private static bool ConsumeFoodCard(CardView foodCard, int requestedFoodAmount, out DailyUpkeepResult.ConsumedFoodRecord record, out int foodUnitsProvided)
    {
        record = null;
        foodUnitsProvided = 0;

        if (requestedFoodAmount <= 0)
            return false;

        if (foodCard == null || foodCard.Instance == null || foodCard.Instance.data == null)
            return false;

        CardInstance instance = foodCard.Instance;
        FoodRuntime foodRuntime = instance.FoodRuntime;
        if (foodRuntime == null || !foodRuntime.isActiveAndEnabled)
            return false;

        foodUnitsProvided = foodRuntime.ConsumeFoodValue(requestedFoodAmount);
        if (foodUnitsProvided <= 0)
            return false;

        record = new DailyUpkeepResult.ConsumedFoodRecord
        {
            runtimeId = instance.RuntimeId,
            cardId = instance.data.id,
            displayName = string.IsNullOrWhiteSpace(instance.data.displayName) ? instance.data.cardName : instance.data.displayName
        };

        if (foodRuntime.RemainingFoodValue > 0)
        {
            // Dejamos marcado que la comida sigue existiendo porque
            // todavia conserva valor alimenticio restante.
            record.cardSurvivedConsumption = true;
            instance.View?.Refresh();
            return true;
        }

        CardStack currentStack = instance.CurrentStack;
        if (currentStack != null)
        {
            currentStack.DestroyCardForSystem(foodCard);
            currentStack.FinalizeCraftingMutation();
            return true;
        }

        MarketEconomyService.DestroyCardUnit(foodCard);
        return true;
    }

    private static void KillUnit(CardInstance unit)
    {
        if (unit == null || unit.View == null)
            return;

        MarketEconomyService.DestroyCardUnit(unit.View);
    }
}
