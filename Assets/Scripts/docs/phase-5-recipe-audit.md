# Phase 5 Recipe Audit

## Purpose

This audit re-evaluates the recipe architecture after the capability migration, exact-ingredient improvements and stack-event integration.

The goal is to identify what part of phase 5 is already complete, what debt still remains, and what next step has the best payoff.

## Files reviewed

- `RecipesManagment/RecipeData.cs`
- `RecipesManagment/RecipeDatabase.cs`
- `RecipesManagment/RecipeSystem.cs`
- `Task/StackCraftingExecutor.cs`
- `docs/recipe-system.md`

## Current strengths

### 1. Matching is much stronger than before

The recipe flow no longer depends on gameplay tags.

The current model already supports:

- exact ingredients
- exact ingredients with minimum counts
- exact ingredients with allowed extra copies
- capability requirements with minimum counts and optional maximum counts
- capability-only matching

This is a real architectural improvement.

### 2. Recipe selection is more explicit

`RecipeDatabase` now evaluates recipes through structured results:

- `RecipeMatchResult`
- `RecipeSelectionResult`

That means the system can explain:

- whether a recipe matched
- why it matched
- whether multiple recipes matched
- which one won

This is much better than a pure `bool`-based system.

### 3. Consumption is better aligned with matching

`StackCraftingExecutor` now consumes:

- the minimum exact ingredient set
- the required capability cards

instead of treating exact recipes like blanket full-stack consumption.

That is important because the exact-match model now allows repeated-resource stacks.

### 4. Stack discovery is no longer per-frame global scanning

`RecipeSystem` now reacts to board-driven stack registration and stack change events instead of scanning the full scene every frame.

That is one of the biggest architectural wins already achieved in this area.

## Remaining weaknesses

### 1. Recipe matching still depends too much on live runtime input

Severity: high

`RecipeData` no longer owns the full matching implementation, and the system already uses `RecipeMatchInput` plus `RecipeMatcher`.

That is a real improvement, but recipe evaluation still starts from live runtime stack state instead of a fully scene-independent domain snapshot pipeline.

Consequence:

- harder to test in isolation
- harder to validate recipes outside play mode
- domain matching still depends on scene-layer structures

Status update:

This debt is now reduced again.

`RecipeMatcher` can now evaluate a pure `RecipeMatchInput` without requiring a live `CardStack` reference just to satisfy capability validation.

That does not fully remove scene coupling from the full recipe flow, but it does mean the core matching path is now more genuinely input-driven and easier to test in isolation.

### 2. Ingredient rules used to keep fallback string debt

Severity: high

`RecipeIngredientRule` used to support `cardId` fallback.

Even though the strong `CardData` path now exists and should be preferred, the string path still keeps:

- legacy migration pressure
- additional validation burden
- another place where identity can drift

Consequence:

- recipe authoring is not yet fully strong-reference based
- validation remains more complex than it should be

Status update:

This debt is now resolved in code after the latest phase-5 step.

### 3. `RecipeData` still carries several responsibilities

Severity: medium-high

`RecipeData` used to own:

- match evaluation
- match signatures
- specificity scoring
- capability validation
- result rolling
- ingredient-rule interpretation
- database validation helpers

Status update:

This concentration is now reduced. Matching, validation, signature building, timing resolution, result rolling, capability evaluation, specificity scoring, effective snapshot building and ingredient-rule lookup already started moving to dedicated classes.

This is better than the old state, but it is still a large concentration of concerns for a single asset class.

Consequence:

- growth pressure will keep landing in `RecipeData`
- reuse outside the scene remains harder than necessary

### 4. `RecipeSystem` still uses an `Update()` lifecycle patch

Severity: medium

`RecipeSystem` still calls `TrySubscribeToBoardRoot()` from `Update()`.

This is understandable as a safety net for initialization order, but it is still a lifecycle workaround rather than a deterministic initialization model.

Consequence:

- not a gameplay bug right now
- still architectural noise

Status update:

This debt is now resolved in code after `BoardRoot` gained an explicit availability event.

### 5. Overlap handling still falls back to specificity

Severity: medium

The current system correctly treats specificity as defensive fallback, not authoring priority.

That is good.

But overlaps are still resolved reactively after runtime evaluation rather than being fully prevented by model shape or by stronger static validation.

Consequence:

- still safe enough
- not yet the strongest possible authoring contract

## What is already effectively done in phase 5

These original phase-5 goals are now largely covered:

- runtime moved off tags and onto capabilities
- stack evaluation moved off per-frame scans
- matching and selection results became more explicit
- exact ingredient contracts became stronger and more expressive

So phase 5 should no longer be understood as “replace tags”.

That part is already behind us.

## What phase 5 now really means

At this point, phase 5 is mainly about:

1. reducing dependency on live runtime stack state as the match source
2. extracting more domain logic out of `RecipeData`
3. making recipe validation and testing less scene-dependent

## Best next step

The best next step is not to rewrite the whole recipe system.

The best next step is to keep moving domain logic out of `RecipeData` now that the input model and matcher split already exist.

Suggested direction:

- keep `RecipeMatchInput` as the recipe-facing input model
- continue moving validation and signature logic out of `RecipeData`
- reduce the remaining scene-bound entrypoints over time

That would allow:

- keeping current gameplay behavior
- reducing direct dependency on `CardStack`
- preparing isolated tests
- making later extraction of a dedicated matcher much easier

## Recommended phase-5 order from here

1. Introduce a stack-independent recipe match input model
2. Make `RecipeDatabase` and `RecipeData` consume that model
3. Reduce or remove `cardId` fallback in `RecipeIngredientRule`
4. Extract matching from `RecipeData` into a dedicated matcher
5. Reassess whether result rolling and validation helpers should stay in `RecipeData`
6. Replace `RecipeSystem.Update()` lifecycle patch with a cleaner board initialization contract

## Audit conclusion

Phase 5 is already partially solved.

The main remaining debt is no longer “weak recipe concepts”.

The main remaining debt is now narrower:

- the full recipe flow still starts from live scene/runtime stacks even though the matcher itself is less stack-dependent than before
- some domain logic is still exposed through `RecipeData` as a facade, even after the extraction work
