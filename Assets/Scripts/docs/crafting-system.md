# Crafting System

## Purpose

Crafting occurs over time and resolves a valid recipe against a stack.

## Current implementation

The current crafting flow uses:

- `RecipeSystem` to choose the recipe for a stack
- `TaskSystem` to track active recipe tasks
- `RecipeTaskService` as the recipe-facing task API
- `RecipeTaskCoordinator` to apply recipe-task lifecycle policy
- `RecipeTask` to store runtime task state
- `GameTimeService` as the timed-task time source and shared day-cycle clock
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

The main improvement already in place is that recipe-specific lifecycle policy no longer needs to live entirely inside `TaskSystem`.

`RecipeTask` also now has explicit lifecycle state instead of relying only on timer values, including a paused state for time-source driven suspension.

`TaskSystem` also now stores tasks through a minimal `ITimedTask` contract instead of depending only on `RecipeTask` as its scheduler-facing type.

Recipe-facing start/refresh/cancel operations also no longer live directly on `TaskSystem`; they now go through `RecipeTaskService`.

`TaskSystem` also no longer needs to read `Time.deltaTime` directly; it now advances tasks through `GameTimeService`, which gives the project a cleaner entry point for future pause and world-time rules.

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
- task lifecycle should be explicit enough to support future pause/time-control behavior
- timed progression should depend on a game-owned time source, not only on raw Unity frame time
- zero world-time progression should pause tasks explicitly instead of leaving them ambiguously running
- the same world-time source should also be able to drive the daily cycle without duplicating a second clock

## Future direction

The crafting system should move toward:

- cleaner separation between scheduling, execution and visuals
- explicit task states
- a task model reusable beyond crafting
- less crafting logic inside `CardStack`
- a scheduler that depends on a timed-task contract instead of one concrete task type
- domain-specific task APIs that do not have to live directly on the scheduler
