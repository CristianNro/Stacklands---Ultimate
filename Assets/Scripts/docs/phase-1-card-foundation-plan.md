# Phase 1: Card Foundation Plan

## Purpose

This document translates Phase 1 of `docs/architecture-roadmap.md` into an executable technical plan.

Phase 1 is focused on the card foundation only:

- make the base card model smaller and more honest
- separate core card identity from optional mechanics
- identify dead or misleading fields
- prepare the codebase for a cleaner runtime model in Phase 2

## Why Phase 1 comes first

Right now the project has a card model that works, but it is too broad.

`CardData` currently mixes:

- identity
- presentation metadata
- gameplay configuration
- flags that do not consistently control real behavior
- data that belongs to future systems more than current ones

If we skip this cleanup and jump directly into runtime refactors, we will carry an inflated and misleading data model into every next phase.

## Scope

Phase 1 includes:

- auditing the current card data model
- classifying every card field by ownership and real usage
- deciding which fields stay, move, deprecate or disappear
- documenting the target structure
- making the codebase ready for a later runtime redesign

Phase 1 does not include:

- redesigning `CardInstance`
- rewriting `CardInitializer`
- changing stack logic
- changing recipe execution logic
- changing market logic

Those belong to later phases.

## Current files in scope

Primary files:

- `CardData/Data/CardData.cs`
- `CardData/Data/ResourceCardData.cs`
- `CardData/Data/ItemCardData.cs`
- `CardData/Data/PackCardData.cs`
- `CardData/Data/UnitCardData.cs`
- `CardData/Data/BuildingCardData.cs`
- `CardData/Data/ContainerCardData.cs`
- `CardData/Data/CardEnums.cs`

Reference files affected indirectly:

- `CardData/Runtime/CardInstance.cs`
- `CardData/Runtime/CardInitializer.cs`
- `CardData/Data/CardView.cs`
- `RecipesManagment/RecipeData.cs`
- `Task/TaskSystem.cs`
- `Market/*`
- `CardData/Runtime/ContainerRuntime.cs`

## Current problems to solve

### 1. The base card class is too broad

`CardData` currently contains fields that belong to different layers:

- true base identity
- UI labels and visuals
- gameplay flags
- consumption behavior
- generic tags
- economy values

This makes it difficult to tell what is actually authoritative.

### 2. Some fields are not truly integrated

Several fields are declared but do not currently drive behavior in a reliable or important way.

Examples already identified:

- `stackable`
- `isMovable`
- `isConsumable`
- `isDestroyable`
- `weight`
- `consumeOnRecipe`

These fields are dangerous because they imply rules the runtime does not actually enforce.

### 3. Some subtype fields belong to future systems, not current gameplay

There are subtype-specific fields that are reasonable future ideas, but not yet part of a coherent implemented feature set.

Examples:

- many `UnitCardData` combat and need fields
- several `BuildingCardData` production/capacity fields
- some `ItemCardData` modifier fields

That does not automatically mean they must all be deleted, but they must stop pretending to be part of a solid active gameplay model if nothing consumes them meaningfully.

## Phase 1 target state

By the end of Phase 1, the card foundation should be in this state:

1. `CardData` contains only fields that are truly base-level and actively meaningful.
2. subtype data classes contain fields that belong clearly to their own concept.
3. every retained field has an identified owner and a real consumer.
4. every future-only field is either removed, clearly deprecated, or explicitly documented as inactive.
5. the model is ready for a cleaner runtime redesign in Phase 2.

## Work breakdown

## Step 1. Field inventory and classification

Goal:

Build a complete inventory of every field in the card data model and classify it.

Files:

- `CardData/Data/CardData.cs`
- all subtype data classes
- `CardData/Data/CardEnums.cs`

For each field, classify it as one of:

- keep as active base field
- keep as active subtype field
- move later to runtime state
- move later to capability-specific data
- deprecate
- remove

Questions to answer for every field:

1. Is this field used by current gameplay?
2. If yes, which code consumes it?
3. Is this truly base-card data, or feature-specific data?
4. Is this static asset data, or mutable runtime state?
5. If it is future-facing only, should it stay visible right now?

Expected output:

- a field matrix
- a keep/move/remove decision per field

## Step 2. Define the minimal base card contract

Goal:

Decide what absolutely belongs in the base `CardData`.

Recommended minimal base categories:

- stable identity
- player-facing naming
- description
- core visual
- value, if economy remains universal
- tags, until stronger identifiers replace them

Likely keep candidates in `CardData`:

- `id`
- `cardName`
- `displayName`
- `description`
- `cardImage`
- `value`
- `tags`

Likely review candidates:

- `cardType`
- `rarity`

Likely remove or deprecate candidates:

- `stackable`
- `isMovable`
- `isConsumable`
- `isDestroyable`
- `weight`
- `consumeOnRecipe`

Expected output:

- a written target shape for `CardData`

## Step 3. Re-evaluate subtype classes

Goal:

Make each subtype class honest about what it currently represents.

### `ResourceCardData`

Review:

- `resourceType`
- `maxStack`

Question:

Does `maxStack` actually govern stack logic now?

If not, it should not remain as an active authoritative field without documentation or implementation.

### `ItemCardData`

Review:

- `itemType`
- `bonusDamage`
- `bonusArmor`
- `bonusWorkSpeed`
- `maxDurability`

Question:

Are these active item systems today, or just placeholders?

### `PackCardData`

Review:

- `embeddedPackData`

This field is actively useful and likely should stay.

### `UnitCardData`

Review all combat/work/needs/equipment fields.

Question:

Which of these are truly active today?

If almost none are active, the class is currently acting more like a future design notebook than a reliable model.

### `BuildingCardData`

Review all durability/capacity/production/build fields.

Question:

Which ones are truly consumed by current gameplay?

### `ContainerCardData`

This one is closer to active gameplay than several others and should be reviewed more conservatively.

Main fields likely active:

- `openMode`
- `capacity`
- `listMode`
- `listedCards`
- `releaseRadius`
- `targetSceneName`

Expected output:

- subtype-by-subtype decision list

## Step 4. Audit enums

Goal:

Stop `CardEnums.cs` from acting as a storage room for unimplemented design.

Review all enums and classify them:

- active and used
- partially active
- inactive and future-only

Special attention:

- `DamageType`
- `BuildingType`
- `TaskType`
- `CardState`
- `ConstructionState`
- `CombatState`

Expected output:

- list of enums to keep active
- list to deprecate or remove

## Step 5. Define migration strategy

Goal:

Avoid breaking all assets at once when the cleanup begins.

Recommended strategy:

1. mark target fields for deprecation first
2. update consuming code to stop depending on deprecated fields
3. only then remove dead fields
4. keep documentation synchronized with real status

Possible migration tactics:

- temporary comments marking inactive fields
- `[FormerlySerializedAs]` if a rename is necessary later
- editor validation to detect assets still depending on old assumptions

Expected output:

- ordered migration sequence

## Step 6. Validation pass

Goal:

Confirm that the slimmed model still supports current gameplay.

Validation areas:

- card spawning
- view refresh
- recipe matching
- crafting
- market price usage
- container rules
- pack opening

Expected output:

- a focused regression checklist for card-foundation changes

## File-by-file expected direction

## `CardData/Data/CardData.cs`

Intent:

- reduce to true base fields only
- remove false-promises from the base model
- document what the base card contract means

## `CardData/Data/ResourceCardData.cs`

Intent:

- keep only resource-specific data that has a clear owner

## `CardData/Data/ItemCardData.cs`

Intent:

- decide whether this is an active gameplay model or a placeholder bucket

## `CardData/Data/PackCardData.cs`

Intent:

- keep pack-specific embedded data clean and explicit

## `CardData/Data/UnitCardData.cs`

Intent:

- separate active unit identity from speculative future stat systems

## `CardData/Data/BuildingCardData.cs`

Intent:

- separate active durability/building needs from future production design

## `CardData/Data/ContainerCardData.cs`

Intent:

- preserve active container configuration
- document it as a currently real gameplay model

## `CardData/Data/CardEnums.cs`

Intent:

- remove or flag enums that are not yet part of an active system

## Risks

Main risks of Phase 1:

1. Removing fields too aggressively and losing useful near-term configuration.
2. Keeping too many dead fields and accomplishing nothing.
3. Changing the model without documenting which runtime systems still depend on it.
4. Confusing "future idea" with "current active game rule".

## Decision policy for ambiguous fields

If a field is ambiguous, use this order:

1. keep it if current gameplay truly consumes it
2. keep it only as subtype data if its ownership is clear
3. deprecate it if it represents a reasonable future system but is currently inactive
4. remove it if it is both inactive and misleading

## Completion criteria

Phase 1 is complete when:

1. `CardData` has a smaller and clearer base contract.
2. every retained field has a known consumer.
3. dead or misleading fields are removed or clearly deprecated.
4. subtype classes are more honest about their active role.
5. docs reflect the new reality.

## Recommended execution order

1. inventory all fields
2. classify base `CardData`
3. classify subtype classes
4. classify enums
5. define migration sequence
6. implement cleanup in small patches
7. run validation pass

## Suggested immediate next action

The first concrete implementation task should be:

Create a field audit table for all `CardData` and subtype fields, with:

- file
- field
- current usage
- owner system
- keep/move/deprecate/remove
- notes

Without that audit, the cleanup risks becoming subjective and inconsistent.
