# Recipe System

## Purpose

Recipes define how stack contents can produce results.

## Current implementation

The current system already supports:

- exact ingredient matching
- tag requirement matching
- recipe specificity scoring
- weighted result selection
- explicit ingredient consume rules
- single execution recipes
- repeat-while-valid recipes

Main classes:

- `RecipeData`
- `RecipeDatabase`
- `RecipeSystem`
- `RecipeIngredientRule`
- `RecipeTagRequirement`
- `RecipeResultOption`

## Current behavior

- stacks with fewer than two cards are ignored by the recipe flow
- `RecipeDatabase` picks the most specific valid recipe
- `RecipeSystem` starts or cancels tasks based on stack state
- `TaskSystem` executes the timed part
- `StackCraftingExecutor` performs recipe consumption and execution details

## Current weaknesses

### 1. too much reliance on strings

The current recipe model still depends on:

- string tags
- string ids for ingredient consume rules

This makes content scalable in the short term but fragile in the long term.

### 2. runtime evaluation is too coupled to scene objects

Recipe matching currently depends directly on `CardStack` and live scene state.

That works, but it makes isolated validation and testing harder than it should be.

### 3. stack discovery is now board-driven, but still scene-bound

`RecipeSystem` no longer scans stacks every frame.

Now it bootstraps from `BoardRoot` and listens to board-level stack registration events. That is a real improvement, but recipe evaluation still depends on live scene objects and board singletons.

## Current rules

- recipe matching must remain deterministic for the same stack state
- specificity must decide conflicts clearly
- recipe logic should remain data-driven where possible
- adding a new recipe should not require new hardcoded per-card logic if the existing model can express it
- stack creation and destruction must notify the recipe flow without relying on per-frame world scans

## Future direction

The recipe system should evolve toward:

- stronger, validated identifiers
- event-driven stack evaluation
- clearer separation between matching, execution and consumption
- better testability outside the scene graph
