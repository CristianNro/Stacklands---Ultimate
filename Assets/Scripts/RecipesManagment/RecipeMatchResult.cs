using StacklandsLike.Cards;

// ============================================================
// RecipeMatchResult
// ------------------------------------------------------------
// Resultado de evaluar una receta concreta contra un stack.
// Expone si hubo match, la firma observada y el motivo principal
// del resultado para debugging y validacion de contenido.
// ============================================================
public class RecipeMatchResult
{
    public RecipeData recipe;
    public CardStack stack;
    public bool matched;
    public string matchSignature;
    public string reason;
    public int specificityScore;

    public static RecipeMatchResult Matched(RecipeData recipe, CardStack stack, string matchSignature, string reason)
    {
        return new RecipeMatchResult
        {
            recipe = recipe,
            stack = stack,
            matched = true,
            matchSignature = matchSignature,
            reason = reason,
            specificityScore = recipe != null ? recipe.GetSpecificityScore() : 0
        };
    }

    public static RecipeMatchResult NotMatched(RecipeData recipe, CardStack stack, string reason)
    {
        return new RecipeMatchResult
        {
            recipe = recipe,
            stack = stack,
            matched = false,
            matchSignature = string.Empty,
            reason = reason,
            specificityScore = recipe != null ? recipe.GetSpecificityScore() : 0
        };
    }
}
