# Phase 1 Step 3: Card Subtype Review

## Purpose

This document defines the target role of each `CardData` subtype after the base `CardData` contract has been clarified.

The goal is not to make every subtype minimal at any cost.

The goal is to make every subtype honest:

- active fields should represent real current gameplay
- future-facing fields should stop pretending to be active systems
- misleading data should be removed or clearly deprecated

## Review standard

Every subtype is evaluated with the same questions:

1. Which fields are actively consumed by current code?
2. Which fields are conceptually valid, but not yet backed by a real system?
3. Which fields are misleading enough that they should leave the model?
4. Does the subtype itself still make sense as a type?

## ResourceCardData

File:

- `CardData/Data/ResourceCardData.cs`

## Current fields

- `resourceType`
- `maxStack`

## Current reality

`ResourceCardData` still makes sense as a subtype, but its own fields are weakly integrated right now.

### `resourceType`

Status:

- conceptually valid
- currently not consumed by gameplay code

Decision:

- `deprecate`

Reason:

This is a reasonable classification field for future mechanics, but the current game does not rely on it.

### `maxStack`

Status:

- not consumed by the current stack system

Decision:

- `remove`

Reason:

The current stack system has no per-card stack cap and is moving toward weight-based constraints instead.

## Target role

`ResourceCardData` should remain available as a semantic subtype, but it should not pretend to have active stack-size rules that do not exist.

## ItemCardData

File:

- `CardData/Data/ItemCardData.cs`

## Current fields

- `itemType`
- `bonusDamage`
- `bonusArmor`
- `bonusWorkSpeed`
- `maxDurability`

## Current reality

`ItemCardData` is currently more of a conceptual bucket than a gameplay-backed subtype.

### `itemType`

Status:

- reasonable identity field
- not consumed by current gameplay code

Decision:

- `deprecate`

Reason:

It can stay temporarily as future-facing metadata, but it is not an active rule yet.

### `bonusDamage`

Decision:

- `remove`

Reason:

There is no active equipment or modifier pipeline that consumes it.

### `bonusArmor`

Decision:

- `remove`

Reason:

No current consumer.

### `bonusWorkSpeed`

Decision:

- `remove`

Reason:

No current worker/equipment modifier system exists.

### `maxDurability`

Decision:

- `remove`

Reason:

It was cut to avoid implying that item durability is already an active supported system.

## Target role

`ItemCardData` should remain as a category, but in the short term it should stop advertising an unimplemented modifier system.

## PackCardData

File:

- `CardData/Data/PackCardData.cs`

## Current fields

- `embeddedPackData`

## Current reality

`PackCardData` is one of the healthiest subtypes in the project.

### `embeddedPackData`

Decision:

- `keep`

Reason:

It has clear ownership and a real active consumer in `MarketPackRuntime`.

## Target role

`PackCardData` should remain a concrete active subtype for cards that encapsulate pack-opening behavior.

## UnitCardData

File:

- `CardData/Data/UnitCardData.cs`

## Current fields

- `unitRole`
- `faction`
- `maxHealth`
- `damage`
- `armor`
- `attackSpeed`
- `attackRange`
- `workSpeed`
- `maxHunger`
- `hungerDecayRate`
- `canEquipWeapon`
- `canEquipArmor`
- `canEquipTool`

## Current reality

`UnitCardData` is only partially active.

A small part initializes real runtime state, but most of the class still represents future design.

### `unitRole`

Decision:

- `deprecate`

Reason:

Valid identity field, but no current gameplay system relies on it.

### `faction`

Decision:

- `deprecate`

Reason:

No current faction system uses it.

### `maxHealth`

Decision:

- `keep`

Reason:

Actively consumed by `UnitRuntime.Initialize`.

### `damage`

Decision:

- `remove`

Reason:

It was cut to avoid pretending a combat-stat pipeline already exists.

### `armor`

Decision:

- `remove`

Reason:

It was cut for the same reason as `damage`.

### `attackSpeed`

Decision:

- `remove`

Reason:

No active combat loop uses it.

### `attackRange`

Decision:

- `remove`

Reason:

No active combat loop uses it.

### `workSpeed`

Decision:

- `remove`

Reason:

It was cut to keep unit data limited to real current runtime seeds.

### `maxHunger`

Decision:

- `keep`

Reason:

Actively consumed by `UnitRuntime.Initialize`.

### `hungerDecayRate`

Decision:

- `remove`

Reason:

No active needs system uses it.

### `canEquipWeapon`

Decision:

- `remove`

Reason:

No active equipment system.

### `canEquipArmor`

Decision:

- `remove`

Reason:

No active equipment system.

### `canEquipTool`

Decision:

- `remove`

Reason:

No active equipment system.

## Target role

Short term:

`UnitCardData` should represent a lightweight active unit identity with only the fields that actually initialize meaningful runtime state and a small amount of harmless metadata.

Long term:

When combat, work, hunger and equipment become real systems, those fields can re-enter as active contracts with real consumers.

## BuildingCardData

File:

- `CardData/Data/BuildingCardData.cs`

## Current fields

- `durability`
- `workerCapacity`
- `residentCapacity`
- `storageCapacity`
- `needsWorker`
- `canProduce`
- `productionTime`
- `buildTime`

## Current reality

`BuildingCardData` is also partially active, but most of its fields are currently future-facing.

### `durability`

Decision:

- `keep`

Reason:

Actively consumed by `BuildingRuntime.Initialize`.

### `workerCapacity`

Decision:

- `remove`

Reason:

It was cut to avoid carrying inactive building simulation fields.

### `residentCapacity`

Decision:

- `remove`

Reason:

It was cut to avoid carrying inactive building simulation fields.

### `storageCapacity`

Decision:

- `remove`

Reason:

It was cut to avoid carrying inactive building simulation fields.

### `needsWorker`

Decision:

- `remove`

Reason:

It was cut to avoid carrying inactive building simulation fields.

### `canProduce`

Decision:

- `remove`

Reason:

It was cut to avoid carrying inactive building simulation fields.

### `productionTime`

Decision:

- `remove`

Reason:

It was cut to avoid carrying inactive building simulation fields.

### `buildTime`

Decision:

- `remove`

Reason:

It was cut to avoid carrying inactive building simulation fields.

## Target role

Short term:

`BuildingCardData` should be treated as a minimal building identity plus active durability seed, not as a production/building simulation model.

## ContainerCardData

File:

- `CardData/Data/ContainerCardData.cs`

## Current fields

- `openMode`
- `capacity`
- `listMode`
- `listedCards`
- `releaseRadius`
- `targetSceneName`

## Current reality

`ContainerCardData` is a genuinely active subtype.

Its fields are clearly owned and actively consumed by:

- `ContainerRuntime`
- `ContainerStorageService`

### All fields

Decision:

- `keep`

Reason:

This subtype already represents a real gameplay feature with coherent configuration.

## Target role

`ContainerCardData` should remain the reference example of a subtype with clear ownership and active consumers.

## Subtype summary

### Healthy active subtype

- `PackCardData`
- `ContainerCardData`

### Valid subtype with mostly future-facing fields

- `ResourceCardData`
- `ItemCardData`

### Partially active subtype with inflated future design

- `UnitCardData`
- `BuildingCardData`

## Main conclusion

The subtype system itself is not the main problem.

The problem is that some subtypes currently mix:

- a small active runtime seed
- a larger design wishlist

That makes the data model look more complete than the gameplay actually is.

## Recommended next move

After this review, the next technical step should be:

Review `CardEnums.cs` with the same honesty and decide which enums remain active, which become metadata-only, and which should leave the active model.
