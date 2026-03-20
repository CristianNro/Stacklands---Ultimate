using UnityEngine;
using System.Collections.Generic;  // Necesario para List<T>

// Este componente es un GESTOR de recetas.
// Vive en la escena (por ejemplo en Board o PlayArea) como un singleton.
// Su responsabilidad:
// - Guardar TODAS las recetas del juego.
// - Buscar si existe una receta válida para un conjunto de cartas dado.
public class RecipeDatabase : MonoBehaviour
{
    // LISTA MAESTRA de todas las recetas del juego.
    // Arrastrás aquí cada RecipeData que crees desde el inspector.
    // Puedes tener 5, 10, 50 recetas: todas van acá.
    [SerializeField] private RecipeData[] recipes;

    // Singleton: permite que otros scripts accedan fácilmente a RecipeDatabase.Instance
    // sin tener que buscar el GameObject manualmente.
    // Es útil para que CardDrag pueda preguntar "¿existe una receta con estas cartas?".
    public static RecipeDatabase Instance { get; private set; }

    // Awake() se llama cuando la escena carga.
    private void Awake()
    {
        // Se asigna a sí mismo como la instancia única.
        Instance = this;
    }

    // Busca si existe una receta que corresponda al conjunto de cartas dado.
    // Devuelve la RecipeData que coincide, o null si no existe.
    public RecipeData FindRecipe(List<CardData> stackData)
    {
        if (stackData == null || stackData.Count == 0)
            return null;

        foreach (RecipeData recipe in recipes)
        {
            if (recipe == null) continue;

            if (MatchesRecipe(recipe, stackData))
                return recipe;
        }

        return null;
    }

    private bool MatchesRecipe(RecipeData recipe, List<CardData> stackData)
    {
        if (recipe == null || stackData == null)
            return false;

        // --------------------------------------------------------
        // 1. Filtramos las cartas del stack que esta receta quiere
        //    ignorar durante el matching de ingredientes.
        // --------------------------------------------------------
        List<CardData> filteredStackData = new List<CardData>();

        for (int i = 0; i < stackData.Count; i++)
        {
            CardData card = stackData[i];
            if (card == null) continue;

            if (recipe.ShouldIgnoreCardInIngredientMatch(card))
                continue;

            filteredStackData.Add(card);
        }

        // --------------------------------------------------------
        // 2. Si la cantidad no coincide con los ingredientes reales,
        //    no matchea.
        // --------------------------------------------------------
        if (filteredStackData.Count != recipe.ingredients.Count)
            return false;

        // --------------------------------------------------------
        // 3. Hacemos comparación por multiset (sin importar orden).
        //    Acá asumo que matcheás por CardData.id.
        // --------------------------------------------------------
        List<string> stackIds = new List<string>();
        List<string> recipeIds = new List<string>();

        for (int i = 0; i < filteredStackData.Count; i++)
        {
            if (filteredStackData[i] != null)
                stackIds.Add(filteredStackData[i].id);
        }

        for (int i = 0; i < recipe.ingredients.Count; i++)
        {
            if (recipe.ingredients[i] != null)
                recipeIds.Add(recipe.ingredients[i].id);
        }

        stackIds.Sort();
        recipeIds.Sort();

        if (stackIds.Count != recipeIds.Count)
            return false;

        for (int i = 0; i < stackIds.Count; i++)
        {
            if (stackIds[i] != recipeIds[i])
                return false;
        }

        return true;
    }
}