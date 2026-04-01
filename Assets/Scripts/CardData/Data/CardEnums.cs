using System;

namespace StacklandsLike.Cards
{
    // -------------------------------------------------
    // Tipo general de carta
    // -------------------------------------------------
    public enum CardType
    {
        Resource,
        Unit,
        Building,
        Item,
        Terrain,
        Food,
        Tool,
        Container
    }

    // ============================================================
    // RecipeIngredientConsumeMode
    // ------------------------------------------------------------
    // Define qué le pasa a un ingrediente cuando una receta se completa.
    // ============================================================
    public enum RecipeIngredientConsumeMode
    {
        None,               // La carta queda intacta
        ConsumeOneUse,      // Pierde 1 uso; si llega a 0 se destruye
        ConsumeEntireCard   // Se destruye directamente
    }

    // ============================================================
    // RecipeMatchMode
    // ------------------------------------------------------------
    // Define como una receta decide si un stack aplica o no.
    // ============================================================
    public enum RecipeMatchMode
    {
        ExactIngredients,
        TagRequirementsOnly
    }

    // ============================================================
    // RecipeExecutionMode
    // ------------------------------------------------------------
    // Single:
    // - se ejecuta una sola vez
    //
    // RepeatWhileValid:
    // - consume una carta repetible por ciclo
    // - sigue mientras el stack siga siendo valido
    // ============================================================
    public enum RecipeExecutionMode
    {
        Single,
        RepeatWhileValid
    }

    // ============================================================
    // ContainerListMode
    // ------------------------------------------------------------
    // AllowOnlyListed:
    // - solo acepta cartas presentes en la lista
    //
    // BlockListed:
    // - acepta cualquier carta excepto las de la lista
    // ============================================================
    public enum ContainerListMode
    {
        AllowOnlyListed,
        BlockListed
    }

    // ============================================================
    // CurrencyType
    // ------------------------------------------------------------
    // Define el tipo explicito de moneda que representa una carta.
    // None implica que la carta no participa del sistema monetario.
    // ============================================================
    public enum CurrencyType
    {
        None,
        Normal
    }

    // ============================================================
    // CurrencyFilterMode
    // ------------------------------------------------------------
    // AllowOnlyListed:
    // - solo acepta tipos presentes en la lista
    //
    // BlockListed:
    // - acepta cualquier tipo excepto los presentes en la lista
    // ============================================================
    public enum CurrencyFilterMode
    {
        AllowOnlyListed,
        BlockListed
    }

    // -------------------------------------------------
    // Rareza de carta
    // -------------------------------------------------
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    // -------------------------------------------------
    // Roles de unidades
    // -------------------------------------------------
    public enum UnitRole
    {
        Villager,
        Worker,
        Warrior,
        Archer,
        Animal,
        Enemy,
        Boss
    }

    // -------------------------------------------------
    // Facción
    // -------------------------------------------------
    public enum FactionType
    {
        Player,
        Neutral,
        Enemy
    }

    // -------------------------------------------------
    // Tipo de recurso
    // -------------------------------------------------
    public enum ResourceType
    {
        Wood,
        Stone,
        Metal,
        Food,
        Organic,
        Currency,
        Special
    }

    // -------------------------------------------------
    // Tipo de item
    // -------------------------------------------------
    public enum ItemType
    {
        Weapon,
        Armor,
        Tool,
        Consumable,
        Misc
    }

}
