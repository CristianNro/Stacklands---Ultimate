using UnityEngine;
using System.Collections;
using System.Diagnostics;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Este componente se encarga de instanciar cartas correctamente.
public class CardSpawner : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardsParent;

    [Header("Auto Placement")]
    // Porcentaje minimo del tamaño de la carta que debe quedar visible
    // para no considerarla "tapada" por otra.
    [SerializeField] private float minimumVisibleFraction = 0.18f;
    // Distancia entre posiciones candidatas cuando buscamos un lugar libre.
    [SerializeField] private float searchStep = 16f;
    // Cantidad maxima de anillos que se prueban alrededor del punto ideal.
    [SerializeField] private int maxSearchRings = 4;
    // Duracion base para spawns automaticos con salto.
    [SerializeField] private float animatedSpawnDuration = 0.55f;
    // Duracion del deslizamiento final cuando la carta debe correrse.
    [SerializeField] private float overlapResolveDuration = 0.28f;

    [Header("Debug")]
    [SerializeField] private bool debugSpawnTimings = false;
    [SerializeField] private float debugLogThresholdMs = 1f;

    public static CardSpawner Instance { get; private set; }

    public Transform CardsParent => GetSpawnParent();
    public GameObject CardPrefab => cardPrefab;
    public Transform CardsParentFallback => cardsParent;

    private void Awake()
    {
        Instance = this;
        CardView.PrewarmSharedResources();
    }

    private Transform GetSpawnParent()
    {
        if (BoardRoot.Instance != null && BoardRoot.Instance.CardsContainer != null)
            return BoardRoot.Instance.CardsContainer;

        return cardsParent;
    }

    public GameObject Spawn(CardData data, Vector2 anchoredPosition)
    {
        Stopwatch stopwatch = debugSpawnTimings ? Stopwatch.StartNew() : null;
        long instantiateMs = 0L;
        long initializeMs = 0L;

        Transform parent = GetSpawnParent();
        GameObject go = Instantiate(cardPrefab, parent);
        if (stopwatch != null)
            instantiateMs = stopwatch.ElapsedMilliseconds;

        RectTransform rt = go.GetComponent<RectTransform>();
        NormalizeRectTransform(rt);

        CardInitializer initializer = go.GetComponent<CardInitializer>();
        if (initializer != null)
        {
            initializer.Initialize(data);
        }
        else
        {
            CardInstance instanceFallback = go.GetComponent<CardInstance>();
            if (instanceFallback != null)
                instanceFallback.Initialize(data);

            CardView viewFallback = go.GetComponent<CardView>();
            if (viewFallback != null)
                viewFallback.Refresh();
        }
        if (stopwatch != null)
            initializeMs = stopwatch.ElapsedMilliseconds;

        // IMPORTANTE:
        // La posición final se corrige contra el board para que nunca nazca fuera.
        Vector2 finalPosition = FindNearestFreeSpawnPosition(anchoredPosition, rt);

        rt.anchoredPosition = finalPosition;

        if (stopwatch != null)
            LogSpawnTiming("Spawn", data, instantiateMs, initializeMs, stopwatch.ElapsedMilliseconds);

        return go;
    }

    public GameObject SpawnAnimated(CardData data, Vector2 startPos, float duration = 0.35f)
    {
        if (duration <= 0f)
            duration = animatedSpawnDuration;

        Vector2 rawEndPos = startPos + new Vector2(120f, 160f);
        return SpawnAnimatedToPosition(data, startPos, rawEndPos, duration);
    }

    /// <summary>
    /// Instancia una carta en una posicion inicial y la anima hasta una
    /// posicion final concreta, usando un arco de salto.
    /// </summary>
    public GameObject SpawnAnimatedToPosition(CardData data, Vector2 startPos, Vector2 endPos, float duration = 0.35f)
    {
        Stopwatch stopwatch = debugSpawnTimings ? Stopwatch.StartNew() : null;
        long instantiateMs = 0L;
        long initializeMs = 0L;
        long clampMs = 0L;

        if (duration <= 0f)
            duration = animatedSpawnDuration;

        Vector2 rawEndPos = endPos;

        Vector2 clampedStartPos = startPos;
        Vector2 clampedEndPos = rawEndPos;

        if (BoardRoot.Instance != null)
        {
            // Instanciamos primero para poder usar el tamaño real del prefab
            Transform parent = GetSpawnParent();
            GameObject go = Instantiate(cardPrefab, parent);
            if (stopwatch != null)
                instantiateMs = stopwatch.ElapsedMilliseconds;

            RectTransform rt = go.GetComponent<RectTransform>();
            NormalizeRectTransform(rt);

            CardInitializer initializer = go.GetComponent<CardInitializer>();
            if (initializer != null)
            {
                initializer.Initialize(data);
            }
            else
            {
                CardInstance instanceFallback = go.GetComponent<CardInstance>();
                if (instanceFallback != null)
                    instanceFallback.Initialize(data);

                CardView viewFallback = go.GetComponent<CardView>();
                if (viewFallback != null)
                    viewFallback.Refresh();
            }
            if (stopwatch != null)
                initializeMs = stopwatch.ElapsedMilliseconds;

            clampedStartPos = BoardRoot.Instance.GetClampedPosition(startPos, rt);
            clampedEndPos = BoardRoot.Instance.GetClampedPosition(rawEndPos, rt);
            if (stopwatch != null)
                clampMs = stopwatch.ElapsedMilliseconds;

            rt.anchoredPosition = clampedStartPos;

            StartCoroutine(AnimateMoveThenResolveOverlap(rt, clampedStartPos, clampedEndPos, duration));

            if (stopwatch != null)
                LogSpawnTiming("SpawnAnimatedToPosition", data, instantiateMs, initializeMs, stopwatch.ElapsedMilliseconds, clampMs);

            return go;
        }

        // Fallback si no existe BoardRoot
        GameObject fallbackGo = Spawn(data, startPos);
        RectTransform fallbackRt = fallbackGo.GetComponent<RectTransform>();
        StartCoroutine(AnimateMove(fallbackRt, startPos, rawEndPos, duration));
        return fallbackGo;
    }

    /// <summary>
    /// Busca una posicion libre cercana al punto ideal para evitar que las
    /// cartas creadas por sistemas automaticos tapen otras ya existentes.
    /// </summary>
    public Vector2 FindNearestFreeSpawnPosition(Vector2 preferredPosition, RectTransform cardRect = null, RectTransform ignoreRect = null)
    {
        if (BoardRoot.Instance == null)
            return preferredPosition;

        RectTransform referenceRect = cardRect != null ? cardRect : BoardRoot.Instance.CardsContainer;
        return referenceRect != null
            ? BoardRoot.Instance.FindNearestFreePositionForRect(preferredPosition, referenceRect, minimumVisibleFraction, searchStep, maxSearchRings, ignoreRect)
            : preferredPosition;
    }

    private void NormalizeRectTransform(RectTransform rt)
    {
        if (rt == null) return;

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
    }

    private IEnumerator AnimateMove(RectTransform rt, Vector2 startPos, Vector2 endPos, float duration)
    {
        float time = 0f;
        float jumpHeight = 180f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;
            Vector2 pos = Vector2.Lerp(startPos, endPos, t);

            float arc = 4f * t * (1f - t);
            pos.y += arc * jumpHeight;

            rt.anchoredPosition = pos;

            yield return null;
        }

        rt.anchoredPosition = endPos;
    }

    /// <summary>
    /// Primero hace que la carta llegue al punto de destino pedido.
    /// Si al aterrizar pisaria otra carta, recien ahi se desliza hacia
    /// el espacio libre mas cercano.
    /// </summary>
    private IEnumerator AnimateMoveThenResolveOverlap(RectTransform rt, Vector2 startPos, Vector2 intendedEndPos, float duration)
    {
        yield return AnimateMove(rt, startPos, intendedEndPos, duration);

        if (rt == null)
            yield break;

        Vector2 resolvedEndPos = FindNearestFreeSpawnPosition(intendedEndPos, rt, rt);
        if (Vector2.Distance(resolvedEndPos, intendedEndPos) < 0.01f)
            yield break;

        yield return AnimateSlide(rt, intendedEndPos, resolvedEndPos, overlapResolveDuration);
    }

    private IEnumerator AnimateSlide(RectTransform rt, Vector2 startPos, Vector2 endPos, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);
            yield return null;
        }

        rt.anchoredPosition = endPos;
    }

    private void LogSpawnTiming(string label, CardData data, long instantiateMs, long initializeMs, long totalMs, long clampMs = -1L)
    {
        if (!debugSpawnTimings || totalMs < debugLogThresholdMs)
            return;

        string cardLabel = data != null
            ? (!string.IsNullOrWhiteSpace(data.displayName) ? data.displayName : data.cardName)
            : "<null>";

        string message =
            $"[CardSpawner] {label} '{cardLabel}' took {totalMs} ms " +
            $"(instantiate: {instantiateMs} ms, init/refresh: {Mathf.Max(0, initializeMs - instantiateMs)} ms";

        if (clampMs >= 0L)
            message += $", clamp/setup: {Mathf.Max(0, clampMs - initializeMs)} ms";

        long finalStart = clampMs >= 0L ? clampMs : initializeMs;
        message += $", final stage: {Mathf.Max(0, totalMs - finalStart)} ms).";

        UnityEngine.Debug.Log(message, this);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        InfrastructureValidationUtility.ValidateAndLogCardSpawner(this);
    }
#endif
}
