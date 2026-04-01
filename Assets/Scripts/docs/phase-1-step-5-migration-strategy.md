# Phase 1 Step 5: Migration Strategy

## Purpose

This document defines how Phase 1 should be executed in code.

Unlike a compatibility-first migration, this project now explicitly prioritizes:

- a solid base
- architectural honesty
- long-term scalability

over preserving existing assets at all costs.

That means the migration strategy can be cleaner and more decisive.

## Core migration rule

Do not keep misleading fields, weak contracts or dead model pieces alive just to avoid reauthoring content.

If rebuilding assets or scenes is the cleaner path, choose the cleaner path.

## What this changes

This migration does not need to optimize for:

- zero asset breakage
- temporary duplicated fields
- long-lived compatibility shims
- preserving old inspector shapes just for convenience

This migration should optimize for:

- a smaller and clearer model
- fewer false contracts
- explicit future rules
- cleaner code boundaries for later phases

## Migration philosophy

The right approach is:

1. define the target model clearly
2. cut dead or misleading fields decisively
3. keep only intentional contracts
4. fix code against the target model
5. rebuild content on top of the cleaner base if needed

## Phase 1 migration target

By the end of Phase 1, the card foundation should reflect:

### Base `CardData`

Keep as active:

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

Keep as metadata-only for now:

- `cardType`
- `rarity`

Remove from the base:

- `isConsumable`
- `isDestroyable`
- `consumeOnRecipe`

### Subtypes

Keep active subtype data only where there is a real consumer.

Deprecate or remove fields that represent future systems without current ownership.

### Enums

Keep active only the enums that support real implemented contracts.

Treat the rest as metadata-only or remove them from the active model.

## Recommended execution order

## Step A. Lock the target model

Inputs:

- `docs/phase-1-card-field-audit.md`
- `docs/phase-1-step-2-carddata-target-shape.md`
- `docs/phase-1-step-3-subtype-review.md`
- `docs/phase-1-step-4-enum-review.md`

Goal:

Freeze what stays, what goes, and what remains metadata-only.

No code changes should begin until this is clear.

## Step B. Cut obviously dead base fields first

Priority removals from `CardData`:

- `isConsumable`
- `isDestroyable`
- `consumeOnRecipe`

Why these first:

- they have no real consumer
- they create false expectations
- they do not support an explicitly chosen future contract

## Step C. Preserve and formalize chosen future contracts

Fields to preserve even if not yet enforced:

- `stackable`
- `isMovable`
- `weight`

Why:

- they are now intentional future rules
- later phases must make them authoritative

Important:

Preserving them is not enough.

They must be treated as pending implementation contracts in later work.

## Step D. Reduce subtype noise

Goal:

Clean each subtype so it reflects either:

- active current behavior
- temporary metadata

or nothing at all.

Recommended direction:

- keep `PackCardData` and `ContainerCardData` mostly intact
- trim `ResourceCardData`
- reduce `ItemCardData`
- slim `UnitCardData` to real current runtime seeds plus temporary metadata
- slim `BuildingCardData` to real current runtime seeds plus temporary metadata

## Step E. Clean enum noise

Goal:

Stop the active model from depending on speculative enums.

Recommended direction:

- keep recipe/container enums as active
- keep classification enums as metadata-only if useful
- remove unused state/combat/building/task enums from the active model

## Step F. Fix compile-time consumers immediately

Once dead fields are removed, any broken references should be fixed immediately against the new target model.

This is good.

Compile breakage is acting like a checklist of stale assumptions.

## Step G. Rebuild content on the new model

Only after the codebase reflects the cleaner model should content be reauthored or recreated.

This avoids rebuilding assets against a still-uncertain contract.

## Hard cuts recommended

Because compatibility is not the top concern, these hard cuts are recommended:

### 1. Do not keep duplicate versions of the same concept

Example:

- do not keep a deprecated field and a new field indefinitely

### 2. Do not add compatibility wrappers unless they unblock a short transition

If a field is wrong for the new model, remove it instead of supporting it forever.

### 3. Do not preserve aspirational gameplay fields out of sentiment

If a system does not exist yet, its data should not dominate the active model.

## Risk management

Even with a clean-cut strategy, there are still risks.

### Risk 1. Over-pruning future-useful concepts

Mitigation:

- keep clear documentation of why a field was removed or deprecated
- preserve concepts in docs if not in the active model

### Risk 2. Cleaning the data model without following through in later phases

Mitigation:

- treat `stackable`, `isMovable` and `weight` as explicit follow-up contracts

### Risk 3. Rebuilding assets before the target model is stable

Mitigation:

- do content reconstruction only after the code contract is frozen

## What not to do

- do not do a giant unstructured rewrite of every card-related file at once
- do not preserve misleading fields just because they sound useful
- do not rebuild content before the model is settled
- do not introduce new enums or new data fields unless they replace a real weak contract

## Exit criteria for Step 5

Step 5 is complete when the project has:

1. a defined clean-cut migration order
2. permission to remove misleading legacy fields without guilt
3. a clear rule for when to rebuild assets
4. a sequence that later implementation work can follow without improvising

## Recommended next action after Step 5

The next practical step should be:

Create the concrete implementation checklist for Phase 1 code changes, grouped by file and ordered by dependency.

That checklist will translate the migration strategy into a patch sequence.
