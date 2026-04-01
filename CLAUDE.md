# CLAUDE.md — Stacklands Ultimate

## Project Context
Unity card game inspired by Stacklands. Players drag and stack cards to trigger crafting recipes.
Developed in C# / Unity (UI Toolkit / uGUI).

## Working Directory
`Assets/Scripts/` — all game logic lives here.

## Architecture Summary

```
Input (CardDrag)
  → CardStack (state container)
    → RecipeSystem (decision layer)
      → TaskSystem (execution over time)
        → CardStack.CompleteRecipeFromTask()
          → CardSpawner (spawn output)
```

## Folder Structure

| Folder | Responsibility |
|---|---|
| `Board/` | BoardRoot — boundary clamping, card registry |
| `Buttons/` | CardSpawner, CardSpawnerButton — spawn UI |
| `CardData/Data/` | ScriptableObject definitions (CardData, enums, subtypes) |
| `CardData/Runtime/` | MonoBehaviour runtime state (CardInstance, UnitRuntime, BuildingRuntime) |
| `CardInteraction/` | CardDrag, CardDropTarget — drag-and-drop |
| `StackManagment/` | CardStack, CardStackFactory — stack lifecycle |
| `RecipesManagment/` | RecipeData, RecipeDatabase, RecipeSystem |
| `RecipesManagment/BatchRecipes/` | BatchRecipeData, BatchRecipeDatabase |
| `Task/` | TaskSystem, RecipeTask — timer-based execution |

## Key Design Principles
- **Data-driven**: all content defined via ScriptableObjects
- **Tag-based logic**: cards have string tags; recipes/batches match by tags
- **Decoupled systems**: RecipeSystem decides, TaskSystem executes, CardStack is passive container
- **No logic in CardStack**: CardStack never makes gameplay decisions

## Card Type Hierarchy
```
CardData (abstract ScriptableObject)
  ├── ResourceCardData  (resourceType, maxStack)
  ├── UnitCardData      (health, damage, armor, hunger, workSpeed, equipment)
  ├── BuildingCardData  (workerCapacity, buildTime, productionTime)
  └── ItemCardData      (bonusDamage, bonusArmor, bonusWorkSpeed, maxDurability)
```

## Current Phase Status
- **Phase 1 COMPLETE**: Cards, stacks, normal recipes, weighted results, batch recipes
- **Phase 2 IN PROGRESS**: Worker states, speed modifiers (UnitRuntime/BuildingRuntime exist but have no active logic)
- **Phase 3–5**: Not started

## What Works Right Now
- Drag & drop cards onto each other → form stacks
- RecipeSystem detects matching recipes → TaskSystem counts down
- Normal recipes: run once, consume ingredients, spawn result
- Batch recipes: repeat per cycle consuming one repeatable card per cycle
- Weighted random results (RecipeResultOption)
- Tag-based ingredient requirements (RecipeTagRequirement)
- Per-ingredient consumption rules (RecipeIngredientRule)
- Board boundary clamping

## What Is Defined But NOT Implemented
| Feature | Data Defined | Logic Implemented |
|---|---|---|
| Unit combat | UnitCardData (damage, armor, attackSpeed) | NO |
| Unit hunger | UnitCardData (hungerDecayRate) | NO |
| Worker tasks | UnitRuntime, TaskType enum | NO |
| Equipment slot | UnitRuntime (equippedWeapon/Armor/Tool) | NO |
| Building construction | BuildingCardData (buildTime) | NO |
| Building production | BuildingRuntime (currentProductionProgress) | NO |
| Item durability | ItemCardData (maxDurability) | NO |
| Save/Load | — | NO |

## Known Limitations (from docs)
- RecipeSystem scans ALL stacks every Update → O(n) overhead
- No worker state (busy/free) distinction in tasks
- No multi-task execution per stack
- No UI feedback beyond progress bar
- Batch matching is first-valid (no priority)

## Naming Conventions
- ScriptableObjects: `*Data` suffix (RecipeData, CardData)
- Runtime MonoBehaviours: `*Runtime` suffix (UnitRuntime, BuildingRuntime)
- Systems (singleton-like): `*System` suffix (RecipeSystem, TaskSystem)
- Factories: `*Factory` suffix
- Views: `*View` suffix

## Language Note
Code comments mix Spanish and English. New code can follow either convention — prefer consistency within a file.
