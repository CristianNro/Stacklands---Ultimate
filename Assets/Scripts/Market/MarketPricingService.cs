using UnityEngine;

// ============================================================
// MarketPricingService
// ------------------------------------------------------------
// Define el valor efectivo que usa el market para operaciones
// economicas. En esta etapa devuelve el mismo valor efectivo
// que hoy ya usa el proyecto, pero crea un boundary explicito
// para futuras reglas de pricing, spreads o modificadores.
// ============================================================
public static class MarketPricingService
{
    public static int GetEffectiveMarketValue(CardInstance instance)
    {
        if (instance == null)
            return 0;

        return Mathf.Max(0, instance.GetEffectiveValue());
    }

    public static int GetEffectiveMarketValue(CardData cardData)
    {
        if (cardData == null)
            return 0;

        return Mathf.Max(0, cardData.value);
    }

    public static int GetEffectiveMarketValue(ContainerStorageService.StoredCardSnapshot snapshot)
    {
        if (snapshot == null || snapshot.definition == null)
            return 0;

        if (snapshot.runtime != null && snapshot.runtime.hasRuntimeValueOverride)
            return Mathf.Max(0, snapshot.runtime.runtimeValueOverride);

        return GetEffectiveMarketValue(snapshot.definition);
    }

    public static bool TryGetPositiveMarketValue(CardInstance instance, out int value)
    {
        value = GetEffectiveMarketValue(instance);
        return value > 0;
    }

    public static bool TryGetPositiveMarketValue(ContainerStorageService.StoredCardSnapshot snapshot, out int value)
    {
        value = GetEffectiveMarketValue(snapshot);
        return value > 0;
    }
}
