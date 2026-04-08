# Phase 10 Data Audit

## Goal

This audit checks Phase 10 of the architecture roadmap:

- dead or premature data
- stale comments and naming
- docs that still describe earlier transitional states

The goal is not to aggressively delete metadata. The goal is to make the model honest again.

## Current conclusion

Phase 10 is now mostly a documentation-and-cleanup phase, not a large runtime refactor.

Recent phases already removed several important sources of false structure:

- string gameplay tags are gone from runtime contracts
- recipe matching no longer depends on legacy `cardId` fallback
- market pricing and delivery already moved behind explicit services
- board and drag already use much stronger ownership boundaries

What remains now is narrower:

- metadata that is still present but not yet consumed by gameplay rules
- comments that still call active fields "future"
- docs that still describe earlier architectural debt as if it were current

## Active data that should stay

These fields or enums are clearly active and should not be treated as dead data:

- `CardData.stackable`
- `CardData.isMovable`
- `CardData.weight`
- `CardData.value`
- `CardData.cardImage`
- `CardData.cardIcon`
- `CardData.displayName`
- `CurrencyType`
- `CurrencyFilterMode`
- `ContainerListMode`
- `RecipeMatchMode`
- `RecipeIngredientConsumeMode`
- `RecipeExecutionMode`
- `CardCapabilityType`
- `ResourceType`

Important note:

`ResourceType` is no longer just future-facing metadata. It already participates in container filtering through `ContainerRuntime`.

## Metadata that is still honest, but not yet strongly consumed

These are not dead in the sense of being wrong to keep, but today they are mostly descriptive or future-facing:

- `Rarity`
- `UnitRole`
- `FactionType`
- `ItemType`

Current status:

- `UnitRole` and `FactionType` are still real subtype metadata on `UnitCardData`
- `ItemType` is still real subtype metadata on `ItemCardData`
- `Rarity` is still valid descriptive metadata on `CardData`

What is missing is not their legitimacy as data, but stronger gameplay ownership.

So the current recommendation is:

- keep them
- do not advertise them as gameplay-driving contracts unless a real system starts consuming them
- do not remove them just to make the model smaller if content authors still need them as structured metadata

## Current misalignments found

### 1. `CardData` comments were stale

`stackable`, `isMovable` and `weight` were still described as "future contracts" in code comments even though they already drive gameplay behavior.

This was misleading because:

- drag already respects `isMovable`
- stack logic already respects `stackable`
- stack rules already use `weight`

This has now been corrected in code comments.

### 2. Some active docs still described older architectural debt

The biggest stale statements were:

- `CardDrag` being described as if it still owned most end-of-drag business logic
- recipe flow being described as if it still kept meaningful `string` fallback debt
- board/layout being described as if placement and occupancy were still weakly centralized

Those were true earlier in the roadmap, but they are no longer the best description of the current state.

### 3. Historical docs still mention tags as active gameplay contracts

This is acceptable in historical phase docs and migration plans, but it should not leak into active guidance docs.

At this point:

- active docs should describe capabilities
- historical docs may still describe the old tag-based state as part of the migration record

## Recommended cleanup policy

### Keep as active contracts

- fields already enforced by runtime rules
- enums already consumed by runtime systems
- metadata with stable authoring value and no contradictory meaning

### Keep as metadata only

- `Rarity`
- `UnitRole`
- `FactionType`
- `ItemType`

These should remain in the model, but the documentation should avoid implying they already drive broad gameplay rules if they do not.

### Revisit later only if needed

- deeper pruning of subtype metadata
- enum consolidation
- stronger naming cleanup across older folders and audit docs

That work only becomes worth it if:

- content authoring becomes confusing
- validation tooling needs a stricter distinction
- a new feature makes current names actively misleading

## What Phase 10 should do next

Phase 10 does not need a destructive sweep right now.

The best remaining work in this phase is:

1. keep active docs synchronized with the current runtime truth
2. avoid presenting metadata as stronger than it really is
3. add validation in Phase 11 before deleting structured metadata that content may still rely on

## Summary

The model is already much more honest than it was at the start of the roadmap.

Phase 10 is therefore not mainly about deleting fields. It is about:

- making current contracts explicit
- keeping metadata honest
- preventing active docs from lagging behind the real architecture
