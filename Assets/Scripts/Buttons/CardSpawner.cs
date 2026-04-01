using UnityEngine;
using System.Collections;

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

    public static CardSpawner Instance { get; private set; }

    public Transform CardsParent => GetSpawnParent();

    private void Awake()
    {
        Instance = this;
    }

    private Transform GetSpawnParent()
    {
        if (BoardRoot.Instance != null && BoardRoot.Instance.CardsContainer != null)
            return BoardRoot.Instance.CardsContainer;

        return cardsParent;
    }

    public GameObject Spawn(CardData data, Vector2 anchoredPosition)
    {
        Transform parent = GetSpawnParent();
        GameObject go = Instantiate(cardPrefab, parent);

        RectTransform rt = go.GetComponent<RectTransform>();
        NormalizeRectTransform(rt);

        CardInitializer initializer = go.GetComponent<CardInitializer>();
        if (initializer != null)
        {
            initializer.Initialize(data);
        }
        else
        {
            Debug.LogWarning($"[{go.name}] No tiene CardInitializer. Se inicializa por fallback.");

            CardInstance instanceFallback = go.GetComponent<CardInstance>();
            if (instanceFallback != null)
                instanceFallback.Initialize(data);

            CardView viewFallback = go.GetComponent<CardView>();
            if (viewFallback != null)
                viewFallback.Refresh();
        }

        // IMPORTANTE:
        // La posición final se corrige contra el board para que nunca nazca fuera.
        Vector2 finalPosition = FindNearestFreeSpawnPosition(anchoredPosition, rt);

        rt.anchoredPosition = finalPosition;

        CardInstance instance = go.GetComponent<CardInstance>();
        if (instance != null && BoardRoot.Instance != null)
            BoardRoot.Instance.RegisterCard(instance);

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

            RectTransform rt = go.GetComponent<RectTransform>();
            NormalizeRectTransform(rt);

            CardInitializer initializer = go.GetComponent<CardInitializer>();
            if (initializer != null)
            {
                initializer.Initialize(data);
            }
            else
            {
                Debug.LogWarning($"[{go.name}] No tiene CardInitializer. Se inicializa por fallback.");

                CardInstance instanceFallback = go.GetComponent<CardInstance>();
                if (instanceFallback != null)
                    instanceFallback.Initialize(data);

                CardView viewFallback = go.GetComponent<CardView>();
                if (viewFallback != null)
                    viewFallback.Refresh();
            }

            clampedStartPos = BoardRoot.Instance.GetClampedPosition(startPos, rt);
            clampedEndPos = BoardRoot.Instance.GetClampedPosition(rawEndPos, rt);

            rt.anchoredPosition = clampedStartPos;

            CardInstance instance = go.GetComponent<CardInstance>();
            if (instance != null)
                BoardRoot.Instance.RegisterCard(instance);

            StartCoroutine(AnimateMoveThenResolveOverlap(rt, clampedStartPos, clampedEndPos, duration));

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
}
