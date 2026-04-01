using System.Collections.Generic;
using UnityEngine;

// ============================================================
// MarketPackRegistry
// ------------------------------------------------------------
// Mantiene la relacion runtime entre una instancia fisica de pack
// en el tablero y la definicion de pack que define su apertura.
// ============================================================
public class MarketPackRegistry : MonoBehaviour
{
    public static MarketPackRegistry Instance { get; private set; }

    private readonly Dictionary<string, BaseMarketPackData> packByCardRuntimeId = new Dictionary<string, BaseMarketPackData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static MarketPackRegistry GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        GameObject go = new GameObject("MarketPackRegistry");
        return go.AddComponent<MarketPackRegistry>();
    }

    public void Register(CardInstance cardInstance, BaseMarketPackData packData)
    {
        if (cardInstance == null || packData == null || string.IsNullOrWhiteSpace(cardInstance.RuntimeId))
            return;

        packByCardRuntimeId[cardInstance.RuntimeId] = packData;
    }

    public BaseMarketPackData GetPackData(CardInstance cardInstance)
    {
        if (cardInstance == null || string.IsNullOrWhiteSpace(cardInstance.RuntimeId))
            return null;

        packByCardRuntimeId.TryGetValue(cardInstance.RuntimeId, out BaseMarketPackData packData);
        return packData;
    }

    public void Unregister(CardInstance cardInstance)
    {
        if (cardInstance == null || string.IsNullOrWhiteSpace(cardInstance.RuntimeId))
            return;

        packByCardRuntimeId.Remove(cardInstance.RuntimeId);
    }
}
