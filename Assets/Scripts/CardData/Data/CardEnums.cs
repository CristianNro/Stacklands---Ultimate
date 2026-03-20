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
        Tool
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

    // -------------------------------------------------
    // Tipo de daño (para combate)
    // -------------------------------------------------
    public enum DamageType
    {
        Physical,
        Fire,
        Poison,
        Magic,
        True
    }

    // -------------------------------------------------
    // Tipo de edificio
    // -------------------------------------------------
    public enum BuildingType
    {
        Housing,
        Production,
        Storage,
        Defense,
        Utility
    }

    // -------------------------------------------------
    // Tipo de tarea de unidad
    // -------------------------------------------------
    public enum TaskType
    {
        None,
        Gathering,
        Crafting,
        Building,
        Farming,
        Combat,
        Resting,
        Transporting
    }

    // -------------------------------------------------
    // Estado de una carta
    // -------------------------------------------------
    public enum CardState
    {
        Idle,
        Dragging,
        InStack,
        Working,
        Fighting,
        Resting,
        Disabled,
        Destroyed
    }

    // -------------------------------------------------
    // Estado de construcción
    // -------------------------------------------------
    public enum ConstructionState
    {
        NotBuilt,
        UnderConstruction,
        Completed,
        Damaged
    }

    // -------------------------------------------------
    // Estado de combate
    // -------------------------------------------------
    public enum CombatState
    {
        None,
        Attacking,
        Defending,
        Dead
    }
}
