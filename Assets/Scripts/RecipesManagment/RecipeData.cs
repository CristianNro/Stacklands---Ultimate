using System;
using System.Collections.Generic;
using StacklandsLike.Cards;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Stacklands/Recipe")]
public class RecipeData : ScriptableObject
{
    [Header("Identity")]
    // Identificador estable de la receta para referencias internas.
    public string id;
    // Nombre legible para mostrar en inspector o UI.
    public string displayName;

    [Header("Behavior")]
    // Define si la receta matchea por ingredientes exactos o solo por requisitos.
    public RecipeMatchMode matchMode = RecipeMatchMode.ExactIngredients;
    // Define si corre una vez o si repite ciclos mientras el stack siga siendo valido.
    public RecipeExecutionMode executionMode = RecipeExecutionMode.Single;

    [Header("Ingredients")]
    // Lista exacta de cartas requeridas cuando la receta usa ExactIngredients.
    public List<CardData> ingredients = new List<CardData>();

    [Header("Capability Requirements")]
    // Requisitos tipados que el stack debe cumplir para activar la receta.
    public List<RecipeCapabilityRequirement> capabilityRequirements = new List<RecipeCapabilityRequirement>();

    [Header("Results")]
    // Resultado clasico de la receta cuando no se usan resultados ponderados.
    public CardData result;
    // Pool de resultados posibles con peso relativo para sorteos.
    public List<RecipeResultOption> possibleResults = new List<RecipeResultOption>();

    [Header("Timing")]
    // Tiempo base de cada ejecucion o de cada ciclo repetible.
    public float craftTime = 1f;
    public List<RecipeDurationCapabilityModifier> durationCapabilityModifiers = new List<RecipeDurationCapabilityModifier>();

    [Header("Ingredient Consumption Rules")]
    // Reglas explicitas de consumo para cartas concretas al completar la receta.
    public List<RecipeIngredientRule> ingredientRules = new List<RecipeIngredientRule>();

    /// <summary>
    /// Punto central de matching.
    /// La receta decide sola como validar el stack segun su modo.
    /// </summary>
    public virtual bool MatchesStack(CardStack stack)
    {
        return EvaluateMatch(stack).matched;
    }

    public virtual RecipeMatchResult EvaluateMatch(CardStack stack)
    {
        return EvaluateMatch(RecipeMatchInput.FromStack(stack));
    }

    public virtual RecipeMatchResult EvaluateMatch(RecipeMatchInput input)
    {
        return RecipeMatcher.Evaluate(this, input);
    }

    public bool IsRepeatable()
    {
        return executionMode == RecipeExecutionMode.RepeatWhileValid;
    }

    public virtual float GetCraftTime()
    {
        return RecipeTimingResolver.GetBaseCraftTime(this);
    }

    public virtual float GetCraftTime(CardStack stack)
    {
        return GetCraftTime(RecipeMatchInput.FromStack(stack));
    }

    public virtual float GetCraftTime(RecipeMatchInput input)
    {
        return RecipeTimingResolver.GetCraftTime(this, input);
    }

    public bool IsCardAllowedByCapabilities(CardData cardData)
    {
        return RecipeCapabilityEvaluator.IsCardAllowedByCapabilities(this, cardData);
    }

    public bool ValidateCapabilityRequirements(CardStack stack)
    {
        return ValidateCapabilityRequirements(RecipeMatchInput.FromStack(stack));
    }

    public bool ValidateCapabilityRequirements(RecipeMatchInput input)
    {
        return RecipeCapabilityEvaluator.ValidateCapabilityRequirements(this, input);
    }

    public int GetSpecificityScore()
    {
        return RecipeSpecificityCalculator.GetSpecificityScore(this);
    }

    public bool HasMultipleResults()
    {
        return RecipeResultResolver.HasMultipleResults(this);
    }

    public CardData RollResult()
    {
        return RecipeResultResolver.RollResult(this);
    }

    public RecipeIngredientConsumeMode? GetConsumeModeForCard(CardData cardData)
    {
        return RecipeIngredientRuleResolver.GetConsumeModeForCard(this, cardData);
    }

    public RecipeIngredientRule GetRuleForCard(CardData cardData)
    {
        return RecipeIngredientRuleResolver.GetRuleForCard(this, cardData);
    }

    public bool HasCapabilityRequirements()
    {
        return RecipeCapabilityEvaluator.CountValidCapabilityRequirements(this) > 0;
    }

    public List<RecipeIngredientRule> GetExactIngredientRequirementsSnapshot()
    {
        return RecipeRequirementSnapshotBuilder.BuildExactIngredientRequirementsSnapshot(this);
    }

    public List<RecipeCapabilityRequirement> GetCapabilityRequirementsSnapshot()
    {
        return RecipeRequirementSnapshotBuilder.BuildCapabilityRequirementsSnapshot(this);
    }

    public string BuildCapabilityRequirementsSignatureForDatabase()
    {
        return RecipeSignatureBuilder.BuildCapabilityRequirementsSignature(this);
    }

    public bool ShouldIgnoreCardInIngredientMatch(CardData cardData)
    {
        return RecipeCapabilityEvaluator.ShouldIgnoreCardInIngredientMatch(this, cardData);
    }

    public override string ToString()
    {
        return "Receta para " + result + ": " + string.Join(", ", ingredients);
    }

    public bool IsValidForDatabase(out string validationError)
    {
        return RecipeDefinitionValidator.Validate(this, out validationError);
    }

    public string BuildUniquenessSignature()
    {
        return RecipeSignatureBuilder.BuildUniquenessSignature(this);
    }

    internal bool TryValidateCapabilityRequirements(RecipeMatchInput input, out string failureReason)
    {
        return RecipeCapabilityEvaluator.TryValidateCapabilityRequirements(this, input, out failureReason);
    }

}
