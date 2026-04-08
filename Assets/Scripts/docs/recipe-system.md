# Recipe System

## Purpose

Recipes define how stack contents can produce results.

## Current implementation

The current system already supports:

- exact ingredient matching
- exact ingredient requirements with minimum counts and optional extra-copy tolerance
- capability-only recipe matching
- additional requirement layers beyond exact ingredients
- typed capability requirements for recipe eligibility
- typed capability requirements with minimum and optional maximum counts
- capability-driven duration modifiers for recipe timing
- fallback specificity scoring for invalid overlap cases
- weighted result selection
- explicit ingredient consume rules with `CardData` references
- single execution recipes
- repeat-while-valid recipes

Main classes:

- `RecipeData`
- `RecipeMatcher`
- `RecipeDefinitionValidator`
- `RecipeSignatureBuilder`
- `RecipeTimingResolver`
- `RecipeResultResolver`
- `RecipeCapabilityEvaluator`
- `RecipeSpecificityCalculator`
- `RecipeRequirementSnapshotBuilder`
- `RecipeIngredientRuleResolver`
- `RecipeDatabase`
- `RecipeSystem`
- `RecipeMatchResult`
- `RecipeSelectionResult`
- `RecipeIngredientRule`
- `RecipeResultOption`

## Current behavior

- stacks with fewer than two cards are ignored by the recipe flow
- `RecipeDatabase` chooses the first deterministic valid recipe
- if multiple recipes still match at once, the database logs a warning and uses specificity only as a fallback
- recipe evaluation can now describe if a recipe matched, what the match signature was, whether overlap occurred, and why a winner was chosen
- `RecipeSystem` consumes the full selection result, starts or cancels tasks, and warns when a stack falls into a problematic overlap case
- `TaskSystem` executes the timed part
- `StackCraftingExecutor` performs recipe consumption and execution details
- recipe matching now has an intermediate input model, so the matcher no longer depends directly on `CardStack` as its only contract
- `RecipeMatcher` can now also evaluate a pure `RecipeMatchInput` without requiring a non-null `sourceStack`
- `RecipeData` now delegates recipe evaluation to `RecipeMatcher`, so the asset defines the recipe and the matcher evaluates it
- recipe validation and uniqueness signatures now live outside `RecipeData`, so the asset no longer owns all recipe-domain logic by itself
- recipe timing resolution and weighted result rolling also live outside `RecipeData`
- capability evaluation and specificity scoring also now live outside `RecipeData`
- effective ingredient snapshots and ingredient-rule lookup also now live outside `RecipeData`
- recipe duration can now be modified by capabilities present in the current stack input

## Current weaknesses

### 1. runtime evaluation is too coupled to scene objects

Recipe matching still depends on live scene-derived state for the full gameplay loop, but the matcher itself no longer requires a live `CardStack` just to evaluate a `RecipeMatchInput`.

That is better than before, but it still makes isolated validation and testing harder than it should be.

### 2. stack discovery is now board-driven, but still scene-bound

`RecipeSystem` no longer scans stacks every frame.

Now it bootstraps from `BoardRoot` and listens to board-level stack registration events. That is a real improvement, but recipe evaluation still depends on live scene objects and board singletons.

## Current rules

- recipe matching must remain deterministic for the same stack state
- recipe definitions should be unique by matching components
- specificity is a defensive fallback, not the main way to author recipe precedence
- recipe logic should remain data-driven where possible
- ingredient consume rules should use direct `CardData` references
- exact-ingredient recipes should express repeated production with `requiredCount` and `allowAdditionalCopies`, not by duplicating capability-only recipes
- capability requirements should be authored only through `RecipeCapabilityRequirement`
- `RecipeCapabilityRequirement.maxCount = 0` means unlimited; any positive value acts as a hard maximum
- recipe timing modifiers should prefer capabilities over hardcoded per-card exceptions
- repeatable exact recipes should consume only the minimum exact ingredient set required for each cycle
- adding a new recipe should not require new hardcoded per-card logic if the existing model can express it
- stack creation and destruction must notify the recipe flow without relying on per-frame world scans

## Future direction

The recipe system should evolve toward:

- stronger, validated identifiers
- full capability-based requirement contracts
- event-driven stack evaluation
- clearer separation between matching, execution and consumption
- better testability outside the scene graph
