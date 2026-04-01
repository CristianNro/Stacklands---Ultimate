using System.Collections.Generic;
using UnityEngine;
using StacklandsLike.Cards;

// ============================================================
// MarketTransactionService
// ------------------------------------------------------------
// Centraliza el armado y consumo de transacciones del market.
// En esta etapa resuelve:
// - construccion de contexto de pago para compras
// - seleccion de unidades a consumir
// - consumo de pago desde cartas o contenedores
// - construccion de unidades vendibles para ventas
// ============================================================
public static class MarketTransactionService
{
    public sealed class PaymentUnit
    {
        public int value;
        public CardView cardView;
        public ContainerStorageService.StoredCardSnapshot storedSnapshot;
        public ContainerRuntime sourceContainer;
    }

    public sealed class PaymentContext
    {
        public CardView draggedCard;
        public CardStack draggedStack;
        public ContainerRuntime draggedContainer;
        public readonly List<PaymentUnit> availableUnits = new List<PaymentUnit>();

        public int TotalAvailableValue
        {
            get
            {
                int total = 0;

                for (int i = 0; i < availableUnits.Count; i++)
                    total += Mathf.Max(0, availableUnits[i].value);

                return total;
            }
        }
    }

    public sealed class SellableUnit
    {
        public int value;
        public CardView cardView;
    }

    public static PaymentContext BuildPaymentContext(
        CardView draggedCard,
        CardStack draggedStack,
        CurrencyFilterMode filterMode,
        IReadOnlyList<CurrencyType> listedCurrencyTypes)
    {
        PaymentContext context = new PaymentContext
        {
            draggedCard = draggedCard,
            draggedStack = draggedStack
        };

        if (draggedStack != null)
            return TryCollectStackPayment(draggedStack, context, filterMode, listedCurrencyTypes) ? context : null;

        if (draggedCard == null)
            return null;

        CardInstance draggedInstance = draggedCard.Instance;
        ContainerRuntime containerRuntime = draggedInstance != null && draggedInstance.HasActiveContainerRuntime()
            ? draggedInstance.ContainerRuntime
            : null;

        if (containerRuntime != null)
        {
            if (!TryCollectContainerPayment(containerRuntime, context, filterMode, listedCurrencyTypes))
                return null;

            context.draggedContainer = containerRuntime;
            return context;
        }

        return TryCollectSingleCardPayment(draggedCard, context, filterMode, listedCurrencyTypes) ? context : null;
    }

    public static List<PaymentUnit> SelectUnitsToConsume(List<PaymentUnit> availableUnits, int targetPrice, out int totalPaidValue)
    {
        totalPaidValue = 0;

        if (availableUnits == null || availableUnits.Count == 0 || targetPrice <= 0)
            return null;

        List<PaymentUnit> orderedUnits = new List<PaymentUnit>(availableUnits);
        orderedUnits.Sort((a, b) => a.value.CompareTo(b.value));

        List<PaymentUnit> selectedUnits = new List<PaymentUnit>();

        for (int i = 0; i < orderedUnits.Count; i++)
        {
            PaymentUnit unit = orderedUnits[i];
            if (unit == null || unit.value <= 0)
                continue;

            selectedUnits.Add(unit);
            totalPaidValue += unit.value;

            if (totalPaidValue >= targetPrice)
                break;
        }

        return selectedUnits;
    }

    public static void ConsumePaymentUnits(PaymentContext context, List<PaymentUnit> selectedUnits)
    {
        if (context == null || selectedUnits == null || selectedUnits.Count == 0)
            return;

        Dictionary<ContainerRuntime, List<ContainerStorageService.StoredCardSnapshot>> snapshotsByContainer =
            new Dictionary<ContainerRuntime, List<ContainerStorageService.StoredCardSnapshot>>();

        for (int i = 0; i < selectedUnits.Count; i++)
        {
            PaymentUnit unit = selectedUnits[i];
            if (unit == null)
                continue;

            if (unit.sourceContainer != null && unit.storedSnapshot != null)
            {
                if (!snapshotsByContainer.TryGetValue(unit.sourceContainer, out List<ContainerStorageService.StoredCardSnapshot> snapshots))
                {
                    snapshots = new List<ContainerStorageService.StoredCardSnapshot>();
                    snapshotsByContainer[unit.sourceContainer] = snapshots;
                }

                snapshots.Add(unit.storedSnapshot);
                continue;
            }

            if (unit.cardView != null)
                MarketEconomyService.DestroyCardUnit(unit.cardView);
        }

        foreach (KeyValuePair<ContainerRuntime, List<ContainerStorageService.StoredCardSnapshot>> pair in snapshotsByContainer)
        {
            ConsumeContainerUnits(pair.Key, pair.Value);
        }
    }

    public static List<SellableUnit> BuildSellableUnits(CardView draggedCard, CardStack draggedStack)
    {
        if (draggedStack != null)
            return BuildStackSellableUnits(draggedStack);

        return BuildSingleSellableUnit(draggedCard);
    }

    public static int GetTotalValue(IReadOnlyList<SellableUnit> sellableUnits)
    {
        if (sellableUnits == null)
            return 0;

        int totalValue = 0;

        for (int i = 0; i < sellableUnits.Count; i++)
            totalValue += Mathf.Max(0, sellableUnits[i].value);

        return totalValue;
    }

    public static void ConsumeSoldUnits(List<SellableUnit> sellableUnits)
    {
        if (sellableUnits == null)
            return;

        for (int i = 0; i < sellableUnits.Count; i++)
        {
            CardView card = sellableUnits[i]?.cardView;
            if (card == null)
                continue;

            MarketEconomyService.DestroyCardUnit(card);
        }
    }

    private static bool TryCollectSingleCardPayment(
        CardView draggedCard,
        PaymentContext context,
        CurrencyFilterMode filterMode,
        IReadOnlyList<CurrencyType> listedCurrencyTypes)
    {
        CardInstance instance = draggedCard != null ? draggedCard.Instance : null;
        if (!MarketEconomyService.IsAcceptedCurrency(instance, filterMode, listedCurrencyTypes))
            return false;

        if (!MarketEconomyService.TryGetPositiveValue(instance, out int value))
            return false;

        context.availableUnits.Add(new PaymentUnit
        {
            value = value,
            cardView = draggedCard
        });

        return true;
    }

    private static bool TryCollectStackPayment(
        CardStack draggedStack,
        PaymentContext context,
        CurrencyFilterMode filterMode,
        IReadOnlyList<CurrencyType> listedCurrencyTypes)
    {
        if (draggedStack == null || draggedStack.Cards.Count == 0)
            return false;

        for (int i = 0; i < draggedStack.Cards.Count; i++)
        {
            CardView card = draggedStack.Cards[i];
            CardInstance instance = card != null ? card.Instance : null;

            if (!MarketEconomyService.IsAcceptedCurrency(instance, filterMode, listedCurrencyTypes))
                return false;
        }

        for (int i = 0; i < draggedStack.Cards.Count; i++)
        {
            CardView card = draggedStack.Cards[i];
            CardInstance instance = card != null ? card.Instance : null;
            if (instance == null)
                continue;

            ContainerRuntime containerRuntime = instance.HasActiveContainerRuntime()
                ? instance.ContainerRuntime
                : null;

            if (containerRuntime != null)
            {
                AddContainerUnits(containerRuntime, context, filterMode, listedCurrencyTypes);
                continue;
            }

            if (!MarketEconomyService.TryGetPositiveValue(instance, out int value))
                continue;

            context.availableUnits.Add(new PaymentUnit
            {
                value = value,
                cardView = card
            });
        }

        return context.availableUnits.Count > 0;
    }

    private static bool TryCollectContainerPayment(
        ContainerRuntime containerRuntime,
        PaymentContext context,
        CurrencyFilterMode filterMode,
        IReadOnlyList<CurrencyType> listedCurrencyTypes)
    {
        CardInstance containerInstance = containerRuntime != null ? containerRuntime.OwnerInstance : null;
        if (containerRuntime == null || !MarketEconomyService.IsAcceptedCurrency(containerInstance, filterMode, listedCurrencyTypes))
            return false;

        int unitsBefore = context.availableUnits.Count;
        AddContainerUnits(containerRuntime, context, filterMode, listedCurrencyTypes);
        return context.availableUnits.Count > unitsBefore;
    }

    private static void AddContainerUnits(
        ContainerRuntime containerRuntime,
        PaymentContext context,
        CurrencyFilterMode filterMode,
        IReadOnlyList<CurrencyType> listedCurrencyTypes)
    {
        if (containerRuntime == null || context == null)
            return;

        ContainerStorageService storage = ContainerStorageService.GetOrCreate();
        List<ContainerStorageService.StoredCardSnapshot> contents = storage.GetStoredContentsSnapshot(containerRuntime.ContainerId);

        for (int i = 0; i < contents.Count; i++)
        {
            ContainerStorageService.StoredCardSnapshot snapshot = contents[i];
            if (snapshot == null || snapshot.definition == null)
                continue;

            if (!MarketEconomyService.IsAcceptedCurrency(snapshot.definition, filterMode, listedCurrencyTypes))
                continue;

            int value = snapshot.runtime != null && snapshot.runtime.hasRuntimeValueOverride
                ? Mathf.Max(0, snapshot.runtime.runtimeValueOverride)
                : Mathf.Max(0, snapshot.definition.value);
            if (value <= 0)
                continue;

            context.availableUnits.Add(new PaymentUnit
            {
                value = value,
                storedSnapshot = snapshot,
                sourceContainer = containerRuntime
            });
        }
    }

    private static List<SellableUnit> BuildSingleSellableUnit(CardView draggedCard)
    {
        if (draggedCard == null)
            return null;

        CardInstance instance = draggedCard.Instance;
        if (!IsSellable(instance))
            return null;

        return new List<SellableUnit>
        {
            new SellableUnit
            {
                value = instance.GetEffectiveValue(),
                cardView = draggedCard
            }
        };
    }

    private static List<SellableUnit> BuildStackSellableUnits(CardStack draggedStack)
    {
        if (draggedStack == null || draggedStack.Cards.Count == 0)
            return null;

        List<SellableUnit> sellableUnits = new List<SellableUnit>();

        for (int i = 0; i < draggedStack.Cards.Count; i++)
        {
            CardView card = draggedStack.Cards[i];
            CardInstance instance = card != null ? card.Instance : null;
            if (!IsSellable(instance))
                return null;

            sellableUnits.Add(new SellableUnit
            {
                value = instance.GetEffectiveValue(),
                cardView = card
            });
        }

        return sellableUnits;
    }

    private static bool IsSellable(CardInstance instance)
    {
        if (instance == null || instance.data == null)
            return false;

        if (instance.GetEffectiveValue() <= 0)
            return false;

        if (MarketEconomyService.IsCurrency(instance))
            return false;

        if (HasConsumedAnyUse(instance))
            return false;

        if (instance.HasActiveContainerRuntime() && !IsEmptyContainer(instance.ContainerRuntime))
            return false;

        return true;
    }

    private static bool HasConsumedAnyUse(CardInstance instance)
    {
        if (instance == null || instance.data == null)
            return false;

        if (instance.data.maxUses <= 0)
            return false;

        return instance.usesRemaining < instance.data.maxUses;
    }

    private static bool IsEmptyContainer(ContainerRuntime containerRuntime)
    {
        if (containerRuntime == null)
            return true;

        ContainerStorageService storage = ContainerStorageService.GetOrCreate();
        return storage.GetStoredCount(containerRuntime.ContainerId) <= 0;
    }

    private static void ConsumeContainerUnits(ContainerRuntime containerRuntime, List<ContainerStorageService.StoredCardSnapshot> snapshotsToRemove)
    {
        if (containerRuntime == null || snapshotsToRemove == null || snapshotsToRemove.Count == 0)
            return;

        ContainerStorageService storage = ContainerStorageService.GetOrCreate();
        storage.RemoveStoredRecords(containerRuntime.ContainerId, snapshotsToRemove);
        containerRuntime.RefreshRuntimeValueFromContents();
    }
}
