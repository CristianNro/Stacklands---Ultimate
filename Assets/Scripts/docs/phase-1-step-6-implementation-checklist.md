# Phase 1 Step 6: Implementation Checklist

## Purpose

This document turns the Phase 1 analysis into a concrete implementation sequence.

It is the bridge between planning and code changes.

The checklist is ordered to:

- reduce ambiguity
- avoid mixing unrelated changes
- keep the card-foundation cleanup focused
- prepare later phases without prematurely implementing them

## Important constraint

Phase 1 is still a card-foundation cleanup phase.

That means:

- we can preserve future contracts such as `stackable`, `isMovable` and `weight`
- but we should not fully redesign stack behavior, drag behavior or runtime architecture here

Those implementations belong to later phases.

Phase 1 should leave the model cleaner and ready.

## Phase 1 implementation goal

By the end of implementation, the codebase should have:

- a smaller and clearer `CardData` base model
- slimmer and more honest subtypes
- a cleaned enum set
- updated docs that reflect the real state

without trying to solve Phase 2, 3 or 4 inside the same patch set.

## Execution blocks

## Block 1. Clean the base `CardData` contract

### Files

- `CardData/Data/CardData.cs`

### Actions

1. Keep active base fields:
   - `id`
   - `cardName`
   - `displayName`
   - `description`
   - `cardImage`
   - `stackable`
   - `isMovable`
   - `value`
   - `weight`
   - `maxUses`
   - `tags`

2. Keep metadata-only fields for now:
   - `cardType`
   - `rarity`

3. Remove from base:
   - `isConsumable`
   - `isDestroyable`
   - `consumeOnRecipe`

### Notes

- Do not remove `stackable`, `isMovable` or `weight`
- Add comments only if needed to mark them as intentional future contracts

### Expected outcome

`CardData` becomes smaller and stops advertising fake active rules.

## Block 2. Trim subtype noise

### Files

- `CardData/Data/ResourceCardData.cs`
- `CardData/Data/ItemCardData.cs`
- `CardData/Data/PackCardData.cs`
- `CardData/Data/UnitCardData.cs`
- `CardData/Data/BuildingCardData.cs`
- `CardData/Data/ContainerCardData.cs`

### Actions by file

#### `ResourceCardData.cs`

- keep `resourceType` only if we want it as metadata
- remove `maxStack`

#### `ItemCardData.cs`

- keep `itemType` only as metadata if desired
- remove `bonusDamage`
- remove `bonusArmor`
- remove `bonusWorkSpeed`
- remove `maxDurability`

#### `PackCardData.cs`

- keep `embeddedPackData`
- no structural cleanup needed unless naming/comments improve clarity

#### `UnitCardData.cs`

- keep `maxHealth`
- keep `maxHunger`
- keep `unitRole` and `faction` only as metadata if desired
- remove `damage`
- remove `armor`
- remove `attackSpeed`
- remove `attackRange`
- remove `workSpeed`
- remove `hungerDecayRate`
- remove `canEquipWeapon`
- remove `canEquipArmor`
- remove `canEquipTool`

#### `BuildingCardData.cs`

- keep `durability`
- remove:
  - `workerCapacity`
  - `residentCapacity`
  - `storageCapacity`
  - `needsWorker`
  - `canProduce`
  - `productionTime`
  - `buildTime`

#### `ContainerCardData.cs`

- keep current shape intact
- this is already a coherent active subtype

### Expected outcome

Each subtype becomes more honest about what it really supports today.

## Block 3. Clean enum noise

### Files

- `CardData/Data/CardEnums.cs`

### Actions

1. Keep as active:
   - `RecipeIngredientConsumeMode`
   - `RecipeMatchMode`
   - `RecipeExecutionMode`
   - `ContainerOpenMode`
   - `ContainerListMode`

2. Keep as metadata-only if desired:
   - `CardType`
   - `Rarity`
   - `UnitRole`
   - `FactionType`
   - `ResourceType`
   - `ItemType`

3. Remove from active model:
   - `DamageType`
   - `BuildingType`
   - `TaskType`
   - `CardState`
   - `ConstructionState`
   - `CombatState`

### Expected outcome

The enum layer stops pretending that inactive systems already exist.

## Block 4. Fix direct compile-time consumers

### Files to review immediately after model cleanup

- `CardData/Runtime/CardInstance.cs`
- `CardData/Runtime/CardInitializer.cs`
- `CardData/Data/CardView.cs`
- `CardData/Runtime/UnitRuntime.cs`
- `CardData/Runtime/BuildingRuntime.cs`
- `CardData/Runtime/ContainerRuntime.cs`
- `CardData/Runtime/ContainerStorageService.cs`
- `RecipesManagment/RecipeData.cs`
- `StackManagment/CardStack.cs`
- `Market/MarketPackRuntime.cs`
- `Market/MarketPackPurchaseSlot.cs`
- `Market/MarketSellSlot.cs`

### Actions

1. Remove broken references to deleted fields if any exist
2. Ensure the remaining fields still initialize correctly
3. Ensure metadata-only fields are not accidentally treated as active rules

### Expected outcome

The code compiles cleanly against the trimmed card model.

## Block 5. Mark deferred contracts clearly

### Contracts deferred to later phases

- `stackable`
- `isMovable`
- `weight`

### Action

Make sure the code and docs reflect this truth:

- these fields are intentionally preserved
- they are not yet fully enforced
- later phases must make them authoritative

### Expected outcome

No one mistakes these fields for dead noise or fully implemented behavior.

## Block 6. Documentation sync

### Files

- `docs/card-system.md`
- `docs/project-overview.md`
- `docs/architecture.md`
- `docs/phase-1-card-field-audit.md`
- `docs/phase-1-step-2-carddata-target-shape.md`
- `docs/phase-1-step-3-subtype-review.md`
- `docs/phase-1-step-4-enum-review.md`
- `docs/phase-1-step-5-migration-strategy.md`

### Actions

1. Update any doc wording that still implies removed fields are active
2. Record the final cleaned model if implementation diverges from the current plan

### Expected outcome

Docs remain a trustworthy description of the real codebase.

## Recommended implementation order

1. `CardData.cs`
2. subtype data files
3. `CardEnums.cs`
4. immediate compile-fix pass on runtime and consumers
5. docs sync

## Recommended patch grouping

### Patch 1

- clean `CardData.cs`

### Patch 2

- clean subtype files

### Patch 3

- clean `CardEnums.cs`

### Patch 4

- fix compile-time consumers

### Patch 5

- docs sync

This grouping keeps the work legible and reviewable.

## Things explicitly out of scope for this implementation step

- redesigning runtime state ownership
- rewriting `CardInitializer`
- redesigning recipes, tasks, board or drag/drop

Those still belong to later phases.

## Final readiness check before coding

Before starting the actual code changes, confirm:

1. we accept that some assets may need recreation
2. we are not trying to preserve misleading fields for convenience
3. we will stop Phase 1 at model cleanup, not let it sprawl into later-phase refactors

## Recommended first coding move

Start with Block 1:

Clean `CardData.cs` first.

That is the smallest, clearest foundation cut and it forces the rest of the system to reveal which assumptions still depend on the old inflated model.
