# Crafting System

## Purpose

Crafting occurs over time and resolves a valid recipe against a stack.

## Current implementation

The current crafting flow uses:

- `RecipeSystem` to choose the recipe for a stack
- `TaskSystem` to track active recipe tasks
- `RecipeTask` to store runtime task state
- `CardStackCraftingVisuals` to display progress
- `StackCraftingExecutor` to apply recipe completion and consumption
- `CardStack` as the structural authority that owns cards and stack cleanup

## Current supported behavior

- progress updates over time
- tasks stop if the stack becomes invalid or trivial
- repeatable recipes continue while valid
- results can spawn with animation
- consumption can be explicit per ingredient or fall back to uses-based behavior

## Current weaknesses

### 1. task orchestration is minimal

`TaskSystem` is still a simple update loop over a list of tasks.

That is acceptable now, but it is not yet a reusable task framework.

### 2. crafting responsibilities are split awkwardly

Crafting authority is spread across:

- `RecipeSystem`
- `TaskSystem`
- `StackCraftingExecutor`
- `CardStack`

This makes the flow harder to evolve cleanly.

### 3. visuals live inside authority objects

This part has already started to improve.

Progress visuals now live in `CardStackCraftingVisuals`, and recipe execution has started moving into `StackCraftingExecutor`.

## Current rules

- progress must reflect the active task state
- invalid or destroyed stacks must stop tasks safely
- completion must use the real updated stack state
- repeatable recipes must re-check validity after each cycle

## Future direction

The crafting system should move toward:

- cleaner separation between scheduling, execution and visuals
- explicit task states
- a task model reusable beyond crafting
- less crafting logic inside `CardStack`
