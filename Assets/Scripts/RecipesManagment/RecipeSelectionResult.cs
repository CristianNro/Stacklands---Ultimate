using System.Collections.Generic;

// ============================================================
// RecipeSelectionResult
// ------------------------------------------------------------
// Resultado final de la base de recetas para un stack.
// Conserva todos los matches validos, la receta elegida y el
// motivo de seleccion para poder depurar superposiciones.
// ============================================================
public class RecipeSelectionResult
{
    public CardStack stack;
    public RecipeMatchInput input;
    public RecipeMatchResult selectedMatch;
    public readonly List<RecipeMatchResult> matchingResults = new List<RecipeMatchResult>();
    public string selectionReason;

    public bool HasProblematicOverlap => matchingResults.Count > 1;
    public RecipeData SelectedRecipe => selectedMatch != null && selectedMatch.matched ? selectedMatch.recipe : null;
}
