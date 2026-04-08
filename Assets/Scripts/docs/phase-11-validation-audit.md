# Phase 11 Validation Audit

## Goal

Phase 11 exists to reduce dependence on manual inspection when content grows.

The target is not only more warnings. The target is stronger confidence that content and configuration are coherent before gameplay logic starts running.

## Current validation already present

### Recipes

The recipe system is currently the strongest area in terms of asset validation.

What already exists:

- `RecipeDatabase.OnValidate()` reloads and validates recipe assets
- `RecipeDefinitionValidator` checks recipe shape
- duplicate recipe ids are detected
- duplicate recipe signatures are detected
- ingredient-rule problems are detected
- invalid result options and invalid duration modifiers are detected
- ambiguous exact-expansion overlaps are warned

This means recipe content is already ahead of most other systems.

### Small isolated validation

There are also some smaller checks already in place:

- `CardView.OnValidate()` keeps local prefab references coherent
- `CardEnumEditorWindow` validates enum member names
- several runtime flows log warnings when required runtime references are missing

These are useful, but they are not a content-validation layer yet.

## Main current gap

The project still lacks strong asset-level validation for several core areas outside recipes.

The biggest missing validators today are:

- card definitions
- market packs and slot configuration
- containers
- prefab/runtime reference contracts

The testing side has the same shape:

- there is a good manual checklist
- there are almost no domain tests or editor validation passes that enforce those expectations automatically

## High-value validation targets

### 1. Card asset validation

Historically there was no central validator for `CardData` assets.

Useful checks would be:

- missing or duplicate `id`
- empty `cardName`
- `displayName` intentionally empty vs accidentally empty
- invalid currency configuration
  - `isCurrency = true` with `CurrencyType.None`
  - `isCurrency = false` with non-`None` currency type
- negative `value`
- negative `weight`
- negative `maxUses`
- capability lists containing duplicates or `None`
- contradictions in data that are actually enforced by current systems

This is one of the best first targets because `CardData` is shared by almost every system.

### 2. Container asset validation

Containers already have runtime rules, but there is no dedicated asset validator for their content rules.

Useful checks would be:

- invalid or contradictory `listedCardTypes`
- invalid or contradictory `listedResourceTypes`
- resource subtype filter enabled without allowing resources at all
- currency-container rules inconsistent with currency fields
- negative `capacity`
- negative `maxCardsReleasedPerOpen`

This would reduce silent authoring mistakes that only show up in play mode.

### 3. Market asset and scene configuration validation

The market system now has better runtime services, but validation is still weak.

Useful checks would be:

- `MarketPackData` with invalid or empty weighted results
- duplicated pack ids if ids exist in pack assets
- `MarketPackPurchaseSlot` missing `packData`
- impossible accepted-currency filter configuration
- sell slots with impossible reward currency configuration
- slots missing required scene references if they are meant to run in play

This is especially useful because economy bugs are often content-setup bugs, not algorithm bugs.

### 4. Prefab contract validation

The project still depends heavily on shared prefabs being wired correctly.

Useful checks would be:

- spawned card prefab missing `CardView`
- spawned card prefab missing `CardInstance`
- spawned card prefab missing drag or initialization pieces expected by the project
- stack prefab root contracts if any become stricter
- board scene references not assigned in authoring scenes

These checks would prevent a large class of null-reference issues.

Status:

- completed as a first cut for the main board/spawn path
- `CardSpawner` now validates:
  - missing `cardPrefab`
  - missing fallback parent when no `BoardRoot` exists
  - shared card prefab missing `RectTransform`
  - shared card prefab missing `CardInstance`
  - shared card prefab missing `CardView`
  - shared card prefab missing `CardDrag`
  - shared card prefab missing `CardInitializer` as a preferred setup
- `BoardRoot` now validates:
  - missing usable `cardsContainer`
  - negative paddings
  - missing `playArea`
  - `cardsContainer` placed outside `playArea`
  - incompatible canvases between `playArea` and `cardsContainer`

## Testing gap

`docs/testing-checklist.md` is already a good manual checklist, but Phase 11 should move some of those expectations into repeatable validation.

The best early candidates are:

### Editor/content validation

- recipe database validation on edit
- card asset validation on edit
- pack asset validation on edit
- container asset validation on edit

### Domain-level tests

- recipe matching
- recipe consume rules
- value-combination logic in market services
- container snapshot round-trip shape
- board clamping / placement helpers where feasible

These tests do not need to cover every visual flow. They should cover domain invariants that are easy to regress.

## Recommended implementation order

### Step 1. Add `CardData` validator

This is the highest-value next move.

Reason:

- shared by almost every system
- low ambiguity
- catches content mistakes early

Status:

- completed as a first cut
- `CardData` assets now self-validate in editor through `OnValidate()`
- the first validator pass covers ids, base numeric sanity, currency consistency and duplicate capabilities

### Step 2. Add container validator

Reason:

- container configuration is now richer
- mistakes are easy to make in inspector
- rules are already explicit enough to validate cleanly

Status:

- completed as a first cut inside the shared `CardData` validator path
- container assets now warn about:
  - duplicate listed card types
  - duplicate listed resource types
  - resource subtype filtering enabled while base card-type rules already reject resources
  - empty allow-only resource filters
  - currency containers configured with general card/resource filters that runtime will ignore
  - redundant `CardType.Container` entries in card-type filters

### Step 3. Add market pack / slot validator

Reason:

- economy setup is still fragile
- many issues show up as runtime warnings rather than authoring errors

Status:

- completed as a first cut
- market packs now self-validate in editor for missing `displayName`, invalid `price`, missing `packCard`, empty preset content and invalid random weighted options
- purchase and sell slots now validate:
  - missing `packData`
  - empty or contradictory accepted-currency filters
  - duplicate accepted currency types
  - invalid change/reward card lists
  - currency cards that do not pass the slot filter
  - currency cards with non-positive market value

### Step 4. Add one or two domain tests for recipe and economy services

Reason:

- gives Phase 11 a real safety net, not only editor warnings

Status:

- started with a first edit-mode test block for `MarketEconomyService`
- current coverage focuses on:
  - currency filter behavior
  - exact value-combination behavior
  - preference for fewer cards
  - impossible target values
  - ignoring non-positive market values
- a second edit-mode block now covers `RecipeMatcher`
- current recipe test coverage focuses on:
  - exact ingredient matching
  - exact matching with `allowAdditionalCopies = true`
  - rejection of extra copies when they are not allowed
  - capability-only matching
  - rejection of cards outside allowed capability-driven constraints
- recipe matcher coverage now also includes:
  - `requiredCount > 1`
  - capability `maxCount`
  - exact recipes rejected by capability maximum overflow
- a third edit-mode block now covers `RecipeTimingResolver`
- current timing coverage focuses on:
  - base craft time without modifiers
  - single capability multiplier
  - respecting `maxApplications`
- a fourth edit-mode block now covers `ContainerStorageService`
- current container coverage focuses on:
  - snapshot round-trip for runtime fields
  - stored total value using runtime overrides
  - capacity enforcement through `CanStoreCard`
  - add/remove snapshot record flows

## Architectural guidance

Phase 11 should prefer:

- validators that operate on assets and definitions
- editor-time checks
- pure-service tests where possible

Phase 11 should avoid:

- pushing validation logic into random runtime `MonoBehaviour` update loops
- adding "fix-up" behavior that silently mutates content at runtime
- replacing authoring mistakes with hidden fallback behavior

## Summary

Phase 11 is not starting from zero.

Recipes already demonstrate the intended direction:

- explicit validation
- useful warnings
- strong content contracts

The next step is to extend that pattern to:

- cards
- containers
- market content
- prefab setup

That is the cleanest way to turn the current architecture work into something safer for content iteration.

Status update:

This extension work is now in a healthy first-pass state.

Phase 11 can be treated as closed for now and reopened later only when new mechanics justify more validators or deeper automated coverage.
