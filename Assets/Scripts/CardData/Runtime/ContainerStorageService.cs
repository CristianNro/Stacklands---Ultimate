using System.Collections.Generic;
using UnityEngine;

// ============================================================
// ContainerStorageService
// ------------------------------------------------------------
// Registro global en memoria para el contenido de contenedores.
// Vive entre escenas durante la sesion actual.
// ============================================================
public class ContainerStorageService : MonoBehaviour
{
    [System.Serializable]
    public class StoredCardRuntimeSnapshot
    {
        // Usos restantes al momento de guardarla.
        public int usesRemaining;
        // Posicion que puede reutilizarse cuando otra mecanica necesita restaurar la carta.
        public Vector2 anchoredPosition;
        // Override opcional del value runtime.
        public bool hasRuntimeValueOverride;
        public int runtimeValueOverride;
    }

    [System.Serializable]
    public class StoredCardSnapshot
    {
        // Definicion de carta almacenada.
        public CardData definition;
        // Estado runtime relevante preservado dentro del contenedor.
        public StoredCardRuntimeSnapshot runtime = new StoredCardRuntimeSnapshot();
    }

    public static ContainerStorageService Instance { get; private set; }

    private readonly Dictionary<string, List<StoredCardSnapshot>> contentsByContainerId = new Dictionary<string, List<StoredCardSnapshot>>();

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

    public static ContainerStorageService GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        GameObject go = new GameObject("ContainerStorageService");
        return go.AddComponent<ContainerStorageService>();
    }

    public int GetStoredCount(string containerId)
    {
        return GetContainerContents(containerId).Count;
    }

    /// <summary>
    /// Devuelve una copia superficial del contenido actual para poder
    /// inspeccionarlo sin exponer la lista interna del servicio.
    /// </summary>
    public List<StoredCardSnapshot> GetStoredContentsSnapshot(string containerId)
    {
        return new List<StoredCardSnapshot>(GetContainerContents(containerId));
    }

    public bool CanStoreCard(string containerId, ContainerCardData containerData)
    {
        if (containerData == null)
            return false;

        if (containerData.capacity <= 0)
            return false;

        return GetStoredCount(containerId) < containerData.capacity;
    }

    public bool StoreCard(string containerId, ContainerCardData containerData, CardInstance instance)
    {
        if (string.IsNullOrWhiteSpace(containerId) || containerData == null || instance == null || instance.data == null)
            return false;

        if (instance.data is ContainerCardData)
            return false;

        if (!CanStoreCard(containerId, containerData))
            return false;

        StoredCardSnapshot snapshot = instance.CreateStoredSnapshot();
        if (snapshot == null)
            return false;

        List<StoredCardSnapshot> contents = GetContainerContents(containerId);
        contents.Add(snapshot);

        return true;
    }

    /// <summary>
    /// Calcula el valor total del contenido actual del contenedor.
    /// Por ahora suma el value base de cada carta almacenada.
    /// </summary>
    public int GetStoredTotalValue(string containerId)
    {
        List<StoredCardSnapshot> contents = GetContainerContents(containerId);
        int totalValue = 0;

        for (int i = 0; i < contents.Count; i++)
        {
            StoredCardSnapshot snapshot = contents[i];
            if (snapshot == null || snapshot.definition == null)
                continue;

            if (snapshot.runtime != null && snapshot.runtime.hasRuntimeValueOverride)
            {
                totalValue += Mathf.Max(0, snapshot.runtime.runtimeValueOverride);
                continue;
            }

            totalValue += Mathf.Max(0, snapshot.definition.value);
        }

        return totalValue;
    }

    public void ReleaseContents(string containerId, Vector2 centerPosition, float releaseRadius, int maxCardsToRelease = 0)
    {
        List<StoredCardSnapshot> contents = GetContainerContents(containerId);
        if (contents.Count == 0 || CardSpawner.Instance == null)
            return;

        int releaseCount = maxCardsToRelease > 0
            ? Mathf.Min(maxCardsToRelease, contents.Count)
            : contents.Count;

        List<StoredCardSnapshot> snapshotsToRelease = new List<StoredCardSnapshot>(releaseCount);

        for (int i = 0; i < releaseCount; i++)
            snapshotsToRelease.Add(contents[i]);

        for (int i = 0; i < snapshotsToRelease.Count; i++)
        {
            StoredCardSnapshot snapshot = snapshotsToRelease[i];
            if (snapshot == null || snapshot.definition == null)
                continue;

            float angle = snapshotsToRelease.Count == 1 ? 0f : (Mathf.PI * 2f * i) / snapshotsToRelease.Count;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * releaseRadius;
            GameObject spawned = CardSpawner.Instance.Spawn(snapshot.definition, centerPosition + offset);

            CardInstance spawnedInstance = spawned != null ? spawned.GetComponent<CardInstance>() : null;
            if (spawnedInstance != null)
                spawnedInstance.ApplyStoredSnapshot(snapshot);
        }

        contents.RemoveRange(0, releaseCount);
    }

    /// <summary>
    /// Remueve registros concretos del contenido del contenedor.
    /// Se usa cuando una mecanica externa, como el Market, consume
    /// solo una parte de las cartas almacenadas.
    /// </summary>
    public void RemoveStoredRecords(string containerId, List<StoredCardSnapshot> snapshotsToRemove)
    {
        if (snapshotsToRemove == null || snapshotsToRemove.Count == 0)
            return;

        List<StoredCardSnapshot> contents = GetContainerContents(containerId);

        for (int i = 0; i < snapshotsToRemove.Count; i++)
        {
            StoredCardSnapshot snapshot = snapshotsToRemove[i];
            if (snapshot == null)
                continue;

            contents.Remove(snapshot);
        }
    }

    /// <summary>
    /// Agrega snapshots al contenido del contenedor sin pasar por una carta
    /// instanciada en tablero. Esto sirve, por ejemplo, para devolver cambio
    /// directamente dentro de un cofre usado como forma de pago.
    /// </summary>
    public void AddStoredSnapshots(string containerId, List<StoredCardSnapshot> snapshotsToAdd)
    {
        if (snapshotsToAdd == null || snapshotsToAdd.Count == 0)
            return;

        List<StoredCardSnapshot> contents = GetContainerContents(containerId);

        for (int i = 0; i < snapshotsToAdd.Count; i++)
        {
            StoredCardSnapshot snapshot = snapshotsToAdd[i];
            if (snapshot == null || snapshot.definition == null)
                continue;

            contents.Add(snapshot);
        }
    }

    public static StoredCardSnapshot CreateSnapshotFromCardData(CardData data, int usesRemaining = 0, Vector2? anchoredPosition = null)
    {
        if (data == null)
            return null;

        return new StoredCardSnapshot
        {
            definition = data,
            runtime = new StoredCardRuntimeSnapshot
            {
                usesRemaining = usesRemaining,
                anchoredPosition = anchoredPosition ?? Vector2.zero,
                hasRuntimeValueOverride = false,
                runtimeValueOverride = 0
            }
        };
    }

    private List<StoredCardSnapshot> GetContainerContents(string containerId)
    {
        if (!contentsByContainerId.TryGetValue(containerId, out List<StoredCardSnapshot> contents))
        {
            contents = new List<StoredCardSnapshot>();
            contentsByContainerId[containerId] = contents;
        }

        return contents;
    }
}
