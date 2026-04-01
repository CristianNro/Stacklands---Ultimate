# Phase 1: Card Field Audit

## Purpose

This audit documents the real usage status of the current card data model.

It exists to answer one question with evidence:

Which fields are truly part of the active card foundation, and which ones are dead, misleading, subtype-specific or future-only?

This file is the concrete output of Phase 1, Step 1 from `docs/phase-1-card-foundation-plan.md`.

## Decision labels

- `keep`: active and meaningfully consumed now
- `keep-review`: active enough to keep for now, but ownership should be revisited
- `deprecate`: currently inactive or weakly integrated, but may be preserved temporarily during migration
- `remove`: not meaningfully used and misleading in the current model
- `move-later`: should not stay where it currently lives, but removal belongs to a later phase

## Base card fields

| File | Field | Current usage | Main consumer(s) | Decision | Notes |
|---|---|---|---|---|---|
| `CardData.cs` | `id` | Actively used | `RecipeData`, recipe consume rules | `keep` | Important stable identifier in the current recipe model |
| `CardData.cs` | `cardName` | Actively used | `CardView`, crafting logs | `keep` | Serves as internal/fallback visible name |
| `CardData.cs` | `displayName` | Actively used | `CardView` | `keep` | Player-facing name layer is valid |
| `CardData.cs` | `description` | Not consumed by runtime code | none in current scripts | `keep-review` | Legitimate metadata, but currently UI-inactive |
| `CardData.cs` | `cardType` | Declared but not consumed | none in current scripts | `deprecate` | Looks authoritative but does not currently drive behavior |
| `CardData.cs` | `rarity` | Declared but not consumed | none in current scripts | `deprecate` | Same problem as `cardType` |
| `CardData.cs` | `cardImage` | Actively used | `CardView` | `keep` | Core visual field |
| `CardData.cs` | `stackable` | Actively used | `CardInstance`, `CardStack`, `CardStackFactory`, `CardDrag` | `keep` | Now governs whether a card may participate in stack creation or merge |
| `CardData.cs` | `isMovable` | Actively used | `CardInstance`, `CardStack`, `CardDrag` | `keep` | Now governs whether a card or moved substack may begin drag |
| `CardData.cs` | `isConsumable` | Not consumed | none in current scripts | `remove` | No active consumer |
| `CardData.cs` | `isDestroyable` | Not consumed | none in current scripts | `remove` | No active destruction gate uses it |
| `CardData.cs` | `weight` | Actively used | `CardInstance`, `CardStack`, `CardStackFactory`, `CardDrag` | `keep` | Now contributes to the maximum total weight allowed per stack |
| `CardData.cs` | `value` | Actively used | `CardInstance`, market, container value | `keep` | Strong current consumer network |
| `CardData.cs` | `consumeOnRecipe` | Not consumed | none in current scripts | `remove` | Recipe consumption ignores it completely |
| `CardData.cs` | `maxUses` | Actively used | `CardInstance`, market change, storage restore | `keep` | Real current gameplay/runtime meaning |
| `CardData.cs` | `tags` | Actively used | recipes, market, containers, `CardInstance.HasTag` | `keep` | Critical today, even if too string-driven |

## Resource card fields

| File | Field | Current usage | Main consumer(s) | Decision | Notes |
|---|---|---|---|---|---|
| `ResourceCardData.cs` | `resourceType` | Declared but not consumed | none in current scripts | `deprecate` | Reasonable future field, but inactive today |
| `ResourceCardData.cs` | `maxStack` | Declared but not consumed | none in current scripts | `remove` | Current stack system has no per-card stack cap |

## Item card fields

| File | Field | Current usage | Main consumer(s) | Decision | Notes |
|---|---|---|---|---|---|
| `ItemCardData.cs` | `itemType` | Declared but not consumed | none in current scripts | `deprecate` | Valid category concept, but not active now |
| `ItemCardData.cs` | `bonusDamage` | Declared but not consumed | none in current scripts | `remove` | No equipment/combat modifier pipeline exists yet |
| `ItemCardData.cs` | `bonusArmor` | Declared but not consumed | none in current scripts | `remove` | Same as above |
| `ItemCardData.cs` | `bonusWorkSpeed` | Declared but not consumed | none in current scripts | `remove` | No active worker/equipment modifier system |
| `ItemCardData.cs` | `maxDurability` | Declared but not consumed | none in current scripts | `remove` | Removed to avoid advertising an unimplemented durability system |

## Pack card fields

| File | Field | Current usage | Main consumer(s) | Decision | Notes |
|---|---|---|---|---|---|
| `PackCardData.cs` | `embeddedPackData` | Actively used | `MarketPackRuntime` | `keep` | Clean and meaningful current ownership |

## Unit card fields

| File | Field | Current usage | Main consumer(s) | Decision | Notes |
|---|---|---|---|---|---|
| `UnitCardData.cs` | `unitRole` | Declared but not consumed | none in current scripts | `deprecate` | Reasonable identity field, but inactive now |
| `UnitCardData.cs` | `faction` | Declared but not consumed | none in current scripts | `deprecate` | Not part of current gameplay |
| `UnitCardData.cs` | `maxHealth` | Actively used on initialize | `UnitRuntime` | `keep` | Real runtime initialization consumer |
| `UnitCardData.cs` | `damage` | Declared but not consumed | none in current scripts | `remove` | Removed to avoid pretending combat stats are active |
| `UnitCardData.cs` | `armor` | Declared but not consumed | none in current scripts | `remove` | Removed to avoid pretending combat stats are active |
| `UnitCardData.cs` | `attackSpeed` | Declared but not consumed | none in current scripts | `remove` | No active combat loop uses it |
| `UnitCardData.cs` | `attackRange` | Declared but not consumed | none in current scripts | `remove` | No active combat loop uses it |
| `UnitCardData.cs` | `workSpeed` | Declared but not consumed | none in current scripts | `remove` | Removed to keep unit data focused on current runtime seeds |
| `UnitCardData.cs` | `maxHunger` | Actively used on initialize | `UnitRuntime` | `keep` | Real runtime consumer exists |
| `UnitCardData.cs` | `hungerDecayRate` | Declared but not consumed | none in current scripts | `remove` | Need system does not exist yet |
| `UnitCardData.cs` | `canEquipWeapon` | Declared but not consumed | none in current scripts | `remove` | No equipment runtime |
| `UnitCardData.cs` | `canEquipArmor` | Declared but not consumed | none in current scripts | `remove` | No equipment runtime |
| `UnitCardData.cs` | `canEquipTool` | Declared but not consumed | none in current scripts | `remove` | No equipment runtime |

## Building card fields

| File | Field | Current usage | Main consumer(s) | Decision | Notes |
|---|---|---|---|---|---|
| `BuildingCardData.cs` | `durability` | Actively used on initialize | `BuildingRuntime` | `keep` | Real runtime consumer exists |
| `BuildingCardData.cs` | `workerCapacity` | Declared but not consumed | none in current scripts | `remove` | Removed to avoid carrying inactive building simulation rules |
| `BuildingCardData.cs` | `residentCapacity` | Declared but not consumed | none in current scripts | `remove` | Removed to avoid carrying inactive building simulation rules |
| `BuildingCardData.cs` | `storageCapacity` | Declared but not consumed | none in current scripts | `remove` | Removed to avoid carrying inactive building simulation rules |
| `BuildingCardData.cs` | `needsWorker` | Declared but not consumed | none in current scripts | `remove` | Removed to avoid carrying inactive building simulation rules |
| `BuildingCardData.cs` | `canProduce` | Declared but not consumed | none in current scripts | `remove` | Removed to avoid carrying inactive building simulation rules |
| `BuildingCardData.cs` | `productionTime` | Declared but not consumed | none in current scripts | `remove` | Removed to avoid carrying inactive building simulation rules |
| `BuildingCardData.cs` | `buildTime` | Declared but not consumed | none in current scripts | `remove` | Removed to avoid carrying inactive building simulation rules |

## Container card fields

| File | Field | Current usage | Main consumer(s) | Decision | Notes |
|---|---|---|---|---|---|
| `ContainerCardData.cs` | `openMode` | Actively used | `ContainerRuntime` | `keep` | Real feature field |
| `ContainerCardData.cs` | `capacity` | Actively used | `ContainerStorageService` | `keep` | Real feature field |
| `ContainerCardData.cs` | `listMode` | Actively used | `ContainerRuntime` | `keep` | Real feature field |
| `ContainerCardData.cs` | `listedCards` | Actively used | `ContainerRuntime` | `keep` | Real feature field |
| `ContainerCardData.cs` | `releaseRadius` | Actively used | `ContainerRuntime`, storage release flow | `keep` | Real feature field |
| `ContainerCardData.cs` | `targetSceneName` | Actively used | `ContainerRuntime`, `ContainerStorageService` | `keep` | Real feature field |

## Enum audit

| Enum | Current status | Decision | Notes |
|---|---|---|---|
| `CardType` | Declared but not consumed by runtime logic | `deprecate` | If retained, it should be as metadata only until a real consumer exists |
| `RecipeIngredientConsumeMode` | Actively used | `keep` | Core recipe consumption contract |
| `RecipeMatchMode` | Actively used | `keep` | Core recipe contract |
| `RecipeExecutionMode` | Actively used | `keep` | Core recipe/task contract |
| `ContainerOpenMode` | Actively used | `keep` | Core container contract |
| `ContainerListMode` | Actively used | `keep` | Core container contract |
| `Rarity` | Declared but not consumed | `deprecate` | Metadata only right now |
| `UnitRole` | Declared but not consumed | `deprecate` | Identity idea, not active rule |
| `FactionType` | Declared but not consumed | `deprecate` | Future-facing |
| `ResourceType` | Declared but not consumed | `deprecate` | Future-facing |
| `ItemType` | Declared but not consumed | `deprecate` | Future-facing |
| `DamageType` | Not consumed | `remove` | Purely speculative today |
| `BuildingType` | Not consumed | `remove` | Purely speculative today |
| `TaskType` | Not consumed | `remove` | Purely speculative today |
| `CardState` | Not consumed | `remove` | Current runtime uses bools, not this enum |
| `ConstructionState` | Not consumed | `remove` | No current system uses it |
| `CombatState` | Not consumed | `remove` | No current system uses it |

## Summary by outcome

### Keep

These are clearly part of the active model today:

- `id`
- `cardName`
- `displayName`
- `cardImage`
- `stackable`
- `isMovable`
- `weight`
- `value`
- `maxUses`
- `tags`
- `embeddedPackData`
- `maxHealth`
- `maxHunger`
- `durability`
- all `ContainerCardData` fields
- active recipe/container enums

### Keep-review

These are legitimate data but currently underused:

- `description`

### Deprecate

These are reasonable concepts, but not active enough to remain unquestioned:

- `cardType`
- `rarity`
- `resourceType`
- `itemType`
- `unitRole`
- `faction`
- `UnitRole`
- `FactionType`
- `ResourceType`
- `ItemType`
- `Rarity`
- `CardType`

### Remove

These are the most misleading or least justified in the current active model:

- `isConsumable`
- `isDestroyable`
- `consumeOnRecipe`
- `maxStack`
- `bonusDamage`
- `bonusArmor`
- `bonusWorkSpeed`
- `maxDurability`
- `damage`
- `armor`
- `attackSpeed`
- `attackRange`
- `workSpeed`
- `hungerDecayRate`
- `canEquipWeapon`
- `canEquipArmor`
- `canEquipTool`
- `workerCapacity`
- `residentCapacity`
- `storageCapacity`
- `needsWorker`
- `canProduce`
- `productionTime`
- `buildTime`
- `DamageType`
- `BuildingType`
- `TaskType`
- `CardState`
- `ConstructionState`
- `CombatState`

## Main conclusions

### 1. The current base model is inflated

The active base card contract is much smaller than the current code suggests.

One important outcome is already in place:

- `stackable`
- `isMovable`
- `weight`

They are no longer pending contracts. They are now active gameplay rules in drag and stacking.

### 2. Containers and packs are healthier than other subtype models

`ContainerCardData` and `PackCardData` already have fields with real ownership and real consumers.

### 3. Units and buildings are partially real, but mostly aspirational

Both have one or two actively consumed initialization fields, plus a larger set of future-facing design fields that should stop pretending to be active systems.

### 4. The project currently uses tags and values as real gameplay drivers

Even if that is architecturally weak in the long term, it is the truth of the current implementation and must be documented honestly.

## Recommended next move

Use this audit to perform Step 2 of Phase 1:

Define the minimal target shape of `CardData` and decide which deprecated fields stay temporarily versus which removable fields can be cleaned up immediately.
