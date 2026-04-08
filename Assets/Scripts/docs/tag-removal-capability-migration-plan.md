# Tag Removal Capability Migration Plan

## Purpose

This document defines the technical migration plan to remove all string-based gameplay tags from the project and replace them with explicit typed capabilities and stronger domain contracts.

The target is not partial reduction. The target is full gameplay migration away from `CardData.tags` and `HasTag(...)`.

This plan assumes:

- preserving current gameplay is not the primary concern
- scene/content rework is acceptable
- the goal is a stronger and more scalable architecture

## Core decision

String tags will no longer be used as gameplay contracts.

They will be replaced by:

1. existing strong classifications where appropriate
2. a new explicit capability layer for gameplay permissions and recipe requirements

Use the existing typed fields when they already express the concept:

- `CardType`
- `ResourceType`
- `ItemType`
- `UnitRole`
- `FactionType`
- `CurrencyType`

Use capabilities only for behavior or permissions that are not classification:

- worker eligibility
- tree harvesting
- fire source
- crafting permission
- cooking permission
- tool-like affordances

## Why this migration is necessary

The current tag model creates several structural problems:

- typos become silent gameplay bugs
- no compile-time safety
- no guided authoring in inspector
- different systems can overload the same string with different meanings
- recipes depend on soft conventions instead of typed contracts

The recipe system is the current hotspot:

- `RecipeTagRequirement`
- `RecipeData`
- `CardStack.CountCardsWithTag(...)`
- `CardInstance.HasTag(...)`
- `StackCraftingExecutor`

## Target architecture

### 1. Card classification stays typed

The following remain as primary typed identifiers:

- `CardType`
- `ResourceType`
- `ItemType`
- `UnitRole`
- `FactionType`
- `CurrencyType`

These should not be duplicated as capabilities.

### 2. Card capabilities become the replacement for gameplay tags

Add a new enum:

- `CardCapabilityType`

Examples of candidate values:

- `Worker`
- `TreeHarvester`
- `FireSource`
- `Builder`
- `Cooker`
- `Farmer`
- `SeedPlanter`
- `WaterCarrier`
- `FuelSource`
- `AnimalHandler`

This list must be refined from the real set of gameplay tags currently used by content.

### 3. Recipe tag requirements become capability requirements

Replace:

- `RecipeTagRequirement`

With:

- `RecipeCapabilityRequirement`

Fields:

- `CardCapabilityType capability`
- `int minCount`
- `bool ignoreMatchingCardsInIngredientCheck`

This preserves the current hybrid recipe model:

- exact ingredient IDs
- plus extra capability requirements

## Impacted systems

### Card model

Files:

- `CardData/Data/CardData.cs`
- `CardData/Runtime/CardInstance.cs`

Changes:

- add `List<CardCapabilityType> capabilities`
- remove `List<string> tags`
- replace `HasTag(string)` with `HasCapability(CardCapabilityType)`

### Stack queries

Files:

- `StackManagment/CardStack.cs`

Changes:

- replace `CountCardsWithTag(string)` with `CountCardsWithCapability(CardCapabilityType)`
- replace any stack-level tag queries with capability-based queries

### Recipes

Files:

- `RecipesManagment/RecipeTagRequirement.cs`
- `RecipesManagment/RecipeData.cs`
- `RecipesManagment/RecipeDatabase.cs`
- `Task/StackCraftingExecutor.cs`

Changes:

- replace tag requirements with capability requirements
- replace tag-driven card filtering with capability-driven filtering
- rebuild uniqueness signatures around capability requirements
- rebuild match reasons around capabilities instead of tag strings

### Documentation

Files:

- `docs/card-system.md`
- `docs/recipe-system.md`
- `docs/architecture.md`
- `docs/architecture-roadmap.md`
- `docs/safe-extension-points.md`
- any phase documents that still describe tags as active core contracts

Changes:

- remove tags as recommended extension point
- redefine recipes around strong classifications plus capabilities
- redefine card metadata to exclude gameplay tags

### Content and assets

Requires migration of:

- all card assets that currently rely on tags
- all recipe assets using `RecipeTagRequirement`
- any scene-authored content that assumes tag strings

## Migration phases

## Phase 1: Capability foundation

Goal:

- add the new capability model without yet deleting recipe tag code

Tasks:

1. add `CardCapabilityType` enum
2. add `capabilities` to `CardData`
3. add `HasCapability(...)` to `CardInstance`
4. add `CountCardsWithCapability(...)` to `CardStack`
5. keep old tag path temporarily only if needed to complete the migration safely

Status:

- completed

Complexity:

- medium

Risk:

- low to medium

## Phase 2: Recipe model migration

Goal:

- migrate recipes from tag requirements to capability requirements

Tasks:

1. create `RecipeCapabilityRequirement`
2. replace `tagRequirements` in `RecipeData`
3. replace all tag-based validation in recipe evaluation
4. replace tag-based selection in `StackCraftingExecutor`
5. update selection signatures and match reasons

Status:

- completed
- recipes now author capability requirements through `RecipeCapabilityRequirement`

Complexity:

- high

Risk:

- high

This is the main gameplay-impacting phase.

## Phase 3: Hard removal of tags

Goal:

- remove all string tag contracts from runtime code

Tasks:

1. remove `tags` from `CardData`
2. remove `HasTag(...)` from `CardInstance`
3. remove `CountCardsWithTag(...)` from `CardStack`
4. delete `RecipeTagRequirement`
5. remove any compatibility code added during migration

Status:

- completed
- runtime no longer reads `CardData.tags`
- `RecipeTagRequirement` has been removed from the codebase

Complexity:

- medium

Risk:

- medium

## Phase 4: Content migration and cleanup

Goal:

- make all assets use capabilities and typed fields only

Tasks:

1. update card assets with the new capability data
2. update recipe assets to capability requirements
3. inspect warnings and remove migration leftovers
4. update docs to describe the new stable model

Complexity:

- high

Risk:

- high for content completeness, lower for architecture

## Execution order

Recommended order of implementation:

1. add `CardCapabilityType`
2. add `CardData.capabilities`
3. add `CardInstance.HasCapability(...)`
4. add stack capability queries
5. add `RecipeCapabilityRequirement`
6. migrate `RecipeData` evaluation
7. migrate `StackCraftingExecutor`
8. migrate `RecipeDatabase` validation and uniqueness signatures
9. remove old tag runtime paths
10. migrate content assets
11. clean documentation

## Validation checklist

The migration is only complete when all of the following are true:

- no gameplay system reads `CardData.tags`
- `CardInstance.HasTag(...)` no longer exists
- `CardStack.CountCardsWithTag(...)` no longer exists
- `RecipeTagRequirement` no longer exists
- recipe matching works only through:
  - exact ingredient IDs
  - typed capability requirements
- content warnings for missing migration data are resolved

## Estimated complexity

Overall complexity: high

Breakdown:

- code architecture complexity: high
- runtime risk during migration: medium-high
- content migration cost: high
- long-term payoff: very high

## Recommended implementation style

Do not migrate this opportunistically file by file without a sequence.

Treat it as one intentional architecture block:

1. introduce capabilities
2. migrate recipes
3. remove tags
4. migrate content

The migration should be executed in contiguous work, not spread across many unrelated changes, because partial coexistence of tags and capabilities increases confusion quickly.
