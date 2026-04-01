# Phase 1 Step 2: Target Shape for CardData

## Purpose

This document defines the target minimum shape of `CardData` after the Phase 1 cleanup.

It is not the final runtime design.

Its purpose is to answer:

What should the base card asset represent, clearly and honestly, before later phases redesign runtime state and system boundaries?

## Important product decision

Even though they are not enforced yet by the current code, the following base fields must remain and become active contracts later:

- `stackable`
- `isMovable`
- `weight`

This is an explicit project decision.

They are not dead fields anymore. They are active gameplay rules and must remain enforced as the architecture evolves.

## Design goal

`CardData` should represent:

- stable asset identity
- player-facing card presentation data
- a very small set of universally meaningful gameplay traits
- lightweight cross-system metadata

`CardData` should not become:

- a dump of future mechanics
- a substitute for runtime state
- a place to store system-specific behavior that belongs to subtypes or services

## Proposed target structure

## 1. Identity

These belong in the base class.

- `id`
- `cardName`
- `displayName`
- `description`

Reason:

Every card needs stable identity and player-facing text, even if some of that text is not yet fully surfaced in UI.

## 2. Visual

These belong in the base class.

- `cardImage`

Reason:

The current shared card view depends on this directly and it is truly universal.

## 3. Core interaction contract

These remain in the base class.

- `stackable`
- `isMovable`
- `weight`

Reason:

They express broad card-level interaction permissions that can reasonably apply to many card types.

`weight` is part of that same interaction contract because it defines how much a card contributes to stack capacity.

Current state after implementation:

They are now enforced in the current stack and drag flow.

Today they already affect:

- drag flow
- stack creation and merge rules
- stack-capacity validation

## 4. Universal gameplay metadata

These remain in the base class for now.

- `value`
- `maxUses`
- `tags`

Reason:

These are currently consumed across multiple active systems:

- `value`: market, runtime value calculation, containers
- `maxUses`: runtime initialization and stored-card restoration
- `tags`: recipes, market restrictions, currency logic, container behavior

## 5. Metadata kept temporarily under review

These may stay in the base class temporarily, but should not be treated as strong gameplay contracts yet.

- `cardType`
- `rarity`

Reason:

They are still plausible card metadata, but current gameplay does not depend on them.

If they remain, they should be documented as metadata-only until a real consumer exists.

## 6. Fields that should leave the base class

These should not remain in the target minimum `CardData` base model.

- `isConsumable`
- `isDestroyable`
- `consumeOnRecipe`

Reason:

They are currently misleading because they imply rules that the runtime does not enforce.

## Proposed base model summary

### Base fields to keep as active

- `id`
- `cardName`
- `displayName`
- `description`
- `cardImage`
- `stackable`
- `isMovable`
- `weight`
- `value`
- `maxUses`
- `tags`

### Base fields to keep only as metadata for now

- `cardType`
- `rarity`

### Base fields to remove from the target shape

- `isConsumable`
- `isDestroyable`
- `consumeOnRecipe`

## Ownership rules

To keep the base model clean, use these rules:

### Rule 1

If a field applies to almost every card and already has a real cross-system consumer, it can stay in `CardData`.

### Rule 2

If a field belongs mainly to one card family, it belongs in the subtype, not the base.

### Rule 3

If a field describes mutable state, it does not belong in `CardData`.

### Rule 4

If a field implies a rule but no active system enforces that rule, it should not be treated as a trustworthy active field.

The explicit contract fields right now are:

- `stackable`
- `isMovable`
- `weight`

## Implications for later phases

Keeping `stackable`, `isMovable` and `weight` means later phases must preserve and extend their enforcement deliberately.

`weight` specifically implies a future rule:

- every card contributes a weight value
- stacks must have a maximum total allowed weight
- stack creation, merge and drop acceptance must respect that cap

That affects at least:

- `CardDrag`
- `CardStack`
- `CardStackFactory`
- drop resolution logic

If future systems ignore these fields, the model will become dishonest again.

## Recommended follow-up after Step 2

After this target shape is accepted, the next sub-step should be:

Review each subtype against the same standard and decide:

- what remains active subtype data
- what becomes deprecated placeholder data
- what should be removed

## Completion criteria

Step 2 is complete when:

1. the minimum target shape of `CardData` is explicit
2. the project knows which base fields are real contracts
3. the project knows which base fields are metadata-only
4. the project knows which base fields should leave the model
