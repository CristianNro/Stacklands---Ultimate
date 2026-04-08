# Post Recipe And Drop Audit

## Purpose

This document records a consistency pass after two recent architecture cuts:

- exact-ingredient recipe expansion with `requiredCount` and `allowAdditionalCopies`
- phase 4 drop-resolution split into domain handlers

The goal is to confirm what is aligned, what still has known debt, and which files or docs could still feel slightly behind the real implementation.

## Code areas reviewed

- `RecipesManagment/RecipeIngredientRule.cs`
- `RecipesManagment/RecipeData.cs`
- `RecipesManagment/RecipeDatabase.cs`
- `Task/StackCraftingExecutor.cs`
- `CardInteraction/CardDropResolver.cs`
- `CardInteraction/CardDropMarketHandler.cs`
- `CardInteraction/CardDropContainerHandler.cs`
- `CardInteraction/CardDropStackHandler.cs`

## Docs reviewed

- `docs/recipe-system.md`
- `docs/architecture.md`
- `docs/architecture-roadmap.md`
- `docs/safe-extension-points.md`
- `docs/known-issues.md`
- `docs/testing-checklist.md`
- `docs/phase-4-carddrag-audit.md`

## What is aligned

### Recipes

- `RecipeIngredientRule` now carries exact-match shape, not only consume behavior.
- `RecipeData` already uses minimum counts and optional extra-copy tolerance during exact matching.
- `StackCraftingExecutor` now consumes only the exact minimum set required by the recipe, instead of treating exact recipes as full-stack consumption by default.
- `RecipeDatabase` now warns about potential overlaps introduced by expandable exact ingredients.

### Interaction / drop

- `CardDrag` is now a thinner input shell.
- `CardDropResolver` is now an orchestrator instead of a large rule owner.
- market, container and stack drop branches already live in separate handlers.

## What was slightly outdated

### `docs/phase-4-carddrag-audit.md`

This file still described handler extraction as a future step and used handler names that no longer match the actual implementation.

This was a documentation lag, not a code problem.

### `docs/known-issues.md`

Some current-risk wording was still too generic compared with the newer architecture:

- containers no longer suffer from the same old shallow-storage model as before
- market logic is no longer duplicated in the same way now that shared services exist

The remaining debt is more specific than the previous wording suggested.

### `docs/testing-checklist.md`

The checklist still covered recipes in a general way, but it did not explicitly mention:

- exact recipes with extra copies allowed
- exact recipes with repeated cycles over the same stack
- drag/drop validation after the handler split

## Remaining real debt

### Recipe exact matching still has content migration cost

Even though the code is coherent, old assets can still fail validation if:

- `requiredCount` remains at `0`
- recipe ids are duplicated
- exact ingredient rules drift away from the recipe identity they are meant to describe

This is expected content debt, not architecture drift.

### Recipe rules still kept some fallback id debt

The model was much stronger than before, but at the time of this audit `RecipeIngredientRule.cardId` still existed as a compatibility/fallback path.

That means recipe authoring is not yet fully free from legacy string identity concerns.

### Drop targeting is still scene-hierarchy driven

The drop flow is now much better split, but target discovery still depends on:

- raycast hit object
- `GetComponentInParent(...)`
- scene hierarchy shape

So the next meaningful interaction improvement is still target formalization, not more handler splitting for its own sake.

## Files or docs that can still look behind at a glance

These are not necessarily wrong, but they can feel a little behind if read without context:

- `docs/phase-4-carddrag-audit.md`
  Because it still reads like handler extraction is pending.
- `docs/recipe-system.md`
  Because it still describes ingredient-rule string fallback as migration debt without stressing that exact-match counting is already first-class.
- `docs/testing-checklist.md`
  Because it does not yet call out the new repeated-resource exact-recipe case explicitly.

## Audit conclusion

The recent changes are structurally coherent.

There is no major architecture contradiction between code and active docs.

The remaining gaps at the time of this audit were mostly:

- content migration concerns
- one legacy fallback in recipe rules
- target resolution still being scene-graph driven
- a few docs needing sharper wording around the new exact-recipe model and the current state of phase 4
