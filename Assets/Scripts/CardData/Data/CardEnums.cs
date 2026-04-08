using System;

namespace StacklandsLike.Cards
{
    // -------------------------------------------------
    // Tipo general de carta
    // -------------------------------------------------
    public enum CardType
    {
        Resource = 0,
        Unit = 1,
        Building = 2,
        Item = 3,
        Terrain = 4,
        Food = 5,
        Tool = 6,
        Container = 7,
        Enemy = 8
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
    // TagRequirementsOnly se mantiene por compatibilidad,
    // pero ahora representa requisitos tipados por capacidad.
    // ============================================================
    public enum RecipeMatchMode
    {
        ExactIngredients,
        CapabilityRequirementsOnly
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

    // ============================================================
    // CardCapabilityType
    // ------------------------------------------------------------
    // Typed gameplay permissions and affordances.
    // Avoid duplicating strong classifications such as CardType,
    // ResourceType, ItemType or CurrencyType here.
    // ============================================================
    public enum CardCapabilityType
    {
        None,
        Worker,
        TreeHarvester,
        Builder,
        Cooker,
        Farmer,
        SeedPlanter,
        WaterCarrier,
        FireSource,
        FuelSource,
        AnimalHandler,
        Warrior,
        BabyHumanIncubator,
        PossibleIncubate
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
    // Canal base de defensa usado por un ataque
    // -------------------------------------------------
    public enum CombatDefenseChannel
    {
        Physical,
        Magical
    }

    // -------------------------------------------------
    // Rol tactico de linea dentro de un encuentro.
    // -------------------------------------------------
    public enum CombatLineRole
    {
        Tank,
        Melee,
        Ranged
    }

    // -------------------------------------------------
    // Tipos de dano adicionales que pueden modificar
    // la resolucion final de un ataque.
    // -------------------------------------------------
    public enum DamageType
    {
        Electricity,
        Holy,
        Water,
        Fire,
        Earth,
        Dead
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
