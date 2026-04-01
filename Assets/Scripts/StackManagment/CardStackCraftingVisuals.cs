using UnityEngine;
using UnityEngine.UI;

// ============================================================
// CardStackCraftingVisuals
// ------------------------------------------------------------
// Encapsula el estado visual de crafting asociado a un stack.
// Mantiene la barra de progreso y el tracking visual de receta
// activa sin mezclar esa responsabilidad con ownership del stack.
// ============================================================
public class CardStackCraftingVisuals : MonoBehaviour
{
    private GameObject progressBarRoot;
    private RectTransform progressFillRect;
    private RecipeData activeRecipe;
    private bool isCrafting;

    public RecipeData ActiveRecipe => activeRecipe;
    public bool IsCrafting => isCrafting;

    public void StartVisuals(RecipeData recipe)
    {
        if (isCrafting && activeRecipe == recipe)
            return;

        activeRecipe = recipe;
        isCrafting = true;

        CreateProgressBar();
        SetProgress(0f);
    }

    public void StopVisuals()
    {
        activeRecipe = null;
        isCrafting = false;
        DestroyProgressBar();
    }

    public void SetProgress(float progress01)
    {
        if (!isCrafting || progressFillRect == null)
            return;

        float progress = Mathf.Clamp01(progress01);
        float fullWidth = 100f;
        progressFillRect.sizeDelta = new Vector2(fullWidth * progress, 0f);
    }

    private void CreateProgressBar()
    {
        if (progressBarRoot != null)
            return;

        progressBarRoot = new GameObject("CraftProgressBar", typeof(RectTransform));
        progressBarRoot.transform.SetParent(transform, false);

        RectTransform rootRT = progressBarRoot.GetComponent<RectTransform>();
        rootRT.anchoredPosition = new Vector2(0f, 65f);
        rootRT.sizeDelta = new Vector2(100f, 12f);
        rootRT.anchorMin = new Vector2(0.5f, 0.5f);
        rootRT.anchorMax = new Vector2(0.5f, 0.5f);
        rootRT.pivot = new Vector2(0.5f, 0.5f);

        GameObject backgroundGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        backgroundGO.transform.SetParent(progressBarRoot.transform, false);

        RectTransform bgRT = backgroundGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        Image bgImage = backgroundGO.GetComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.6f);

        GameObject fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGO.transform.SetParent(backgroundGO.transform, false);

        progressFillRect = fillGO.GetComponent<RectTransform>();
        progressFillRect.anchorMin = new Vector2(0f, 0f);
        progressFillRect.anchorMax = new Vector2(0f, 1f);
        progressFillRect.pivot = new Vector2(0f, 0.5f);
        progressFillRect.anchoredPosition = Vector2.zero;
        progressFillRect.sizeDelta = new Vector2(0f, 0f);

        Image progressFillImage = fillGO.GetComponent<Image>();
        progressFillImage.color = new Color(0.2f, 0.9f, 0.2f, 1f);
    }

    private void DestroyProgressBar()
    {
        if (progressBarRoot == null)
            return;

        Destroy(progressBarRoot);
        progressBarRoot = null;
        progressFillRect = null;
    }

    private void OnDisable()
    {
        StopVisuals();
    }
}
