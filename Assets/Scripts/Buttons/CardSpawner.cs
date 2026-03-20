using UnityEngine;
using System.Collections;

// Este componente se encarga de instanciar cartas correctamente.
public class CardSpawner : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardsParent;

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

        CardInstance instance = go.GetComponent<CardInstance>();
        if (instance != null && BoardRoot.Instance != null)
            BoardRoot.Instance.RegisterCard(instance);

        // IMPORTANTE:
        // La posición final se corrige contra el board para que nunca nazca fuera.
        Vector2 finalPosition = anchoredPosition;

        if (BoardRoot.Instance != null)
            finalPosition = BoardRoot.Instance.GetClampedPosition(anchoredPosition, rt);

        rt.anchoredPosition = finalPosition;

        return go;
    }

    public GameObject SpawnAnimated(CardData data, Vector2 startPos, float duration = 0.35f)
    {
        Vector2 rawEndPos = startPos + new Vector2(120f, 160f);

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

            CardInstance instance = go.GetComponent<CardInstance>();
            if (instance != null)
                BoardRoot.Instance.RegisterCard(instance);

            clampedStartPos = BoardRoot.Instance.GetClampedPosition(startPos, rt);
            clampedEndPos = BoardRoot.Instance.GetClampedPosition(rawEndPos, rt);

            rt.anchoredPosition = clampedStartPos;

            StartCoroutine(AnimateMove(rt, clampedStartPos, clampedEndPos, duration));

            return go;
        }

        // Fallback si no existe BoardRoot
        GameObject fallbackGo = Spawn(data, startPos);
        RectTransform fallbackRt = fallbackGo.GetComponent<RectTransform>();
        StartCoroutine(AnimateMove(fallbackRt, startPos, rawEndPos, duration));
        return fallbackGo;
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
}