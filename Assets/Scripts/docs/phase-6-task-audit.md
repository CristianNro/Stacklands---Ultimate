# Phase 6 Task Audit

## Purpose

This audit evaluates the current timed-task architecture after the recipe and interaction refactors.

The goal is to identify what is already healthy, what still keeps `TaskSystem` too coupled to crafting, and what step has the best payoff for phase 6.

## Files reviewed

- `Task/TaskSystem.cs`
- `Task/RecipeTask.cs`
- `Task/StackCraftingExecutor.cs`
- `docs/crafting-system.md`

## Current strengths

### 1. Execution is already separated from scheduling

`TaskSystem` no longer performs recipe consumption or result spawning itself.

That work now lives in `StackCraftingExecutor`, which is a meaningful architectural improvement.

### 2. Task state is explicit enough to be inspectable

`RecipeTask` already isolates:

- target stack
- recipe
- total time
- remaining time

This is still minimal, but it is a clearer runtime object than baking all task state directly into `TaskSystem`.

### 3. Visual progress is not embedded in the scheduler

Progress visuals already moved out of the raw task data and into `CardStackCraftingVisuals`.

That means the scheduler is not directly responsible for UI lifecycle details anymore.

### 4. Repeatable loop behavior is now reasonably explicit

`TaskSystem` clearly distinguishes:

- single completion
- repeatable cycle execution
- validity re-check after each cycle

That makes the core flow easier to reason about than earlier versions.

## Remaining weaknesses

### 1. `TaskSystem` is still recipe-specific

Severity: high

Despite the name, `TaskSystem` is not yet a generic timed-task scheduler.

It still directly knows about:

- `RecipeTask`
- `RecipeData`
- `CardStack`
- repeatable recipe semantics
- crafting visuals
- crafting cancellation rules

Consequence:

- difficult to reuse for non-crafting timed processes
- future systems like single-card transformations would either duplicate logic or force awkward coupling

### 2. Task lifecycle and task policy are mixed together

Severity: high

`TaskSystem` currently handles both:

- generic scheduler concerns
  - ticking time
  - removing dead tasks
  - progress advancement
- crafting policy concerns
  - trivial stack invalidation
  - repeatable recipe validity
  - stack visual start/stop
  - cycle execution

Consequence:

- every new timed behavior risks adding more special cases to the same loop

### 3. `RecipeTask` is still a data bag, not a richer task model

Severity: medium-high

`RecipeTask` stores runtime values, but it does not yet model:

- explicit state
- cancellation reason
- completion status
- paused/running distinction
- scheduler hooks

Consequence:

- task lifecycle is inferred procedurally in `TaskSystem` instead of living in the task model

### 4. Stack validity rules live in the scheduler loop

Severity: medium-high

The scheduler currently decides that tasks should stop when:

- the stack is null
- the recipe is null
- the stack is empty
- the stack has only one card
- repeatable recipes stop matching

Those are valid crafting rules, but they are not generic scheduler rules.

Consequence:

- phase 6 cannot become reusable until those decisions move into a crafting-oriented policy layer

### 5. Execution boundary is better, but restart semantics remain embedded

Severity: medium

`TaskSystem` still decides when to:

- refresh an existing task
- preserve progress ratio
- cancel and recreate
- restart repeatable cycles

Consequence:

- scheduler and recipe behavior are still too tightly fused

## What is already effectively done in phase 6

These parts are already moving in the right direction:

- execution is outside `CardStack`
- runtime task state is explicit in `RecipeTask`
- visuals are not the scheduler's core responsibility

So phase 6 should not be treated as "build tasks from zero".

The base already exists.

## What phase 6 now really means

At this point, phase 6 is mainly about:

1. separating generic scheduling from crafting-specific rules
2. making task lifecycle more explicit
3. preparing the scheduler to support non-recipe timed processes
4. reducing the amount of recipe policy hardcoded into `TaskSystem`

## Best next step

The best next step is not to build a full generic task framework immediately.

The best next step is to introduce a crafting-oriented task coordinator or task policy layer, while keeping `TaskSystem` as the time-ticking owner.

Suggested direction:

- keep `TaskSystem` as the runtime scheduler loop for now
- extract crafting-specific rules into something like `RecipeTaskCoordinator`
- let that coordinator decide:
  - can a task start
  - can a task continue
  - what happens on completion
  - what happens on repeatable cycle restart

That would remove most recipe policy from `TaskSystem` without forcing a full scheduler rewrite.

Status update:

This first extraction is now implemented through `RecipeTaskCoordinator`.

Additional status update:

`RecipeTask` now also has explicit lifecycle state (`Running`, `Completed`, `Cancelled`) instead of being only a timer container.

Another status update:

`TaskSystem` now advances recipe tasks through `GameTimeService` instead of depending directly on `Time.deltaTime`, which creates a cleaner future integration point for pause, speed controls and day-based world time.

Another status update:

`TaskSystem` now stores active tasks through a minimal `ITimedTask` contract, so the scheduler no longer depends exclusively on `RecipeTask` as its storage type.

Another status update:

Recipe tasks can now enter an explicit `Paused` state when timed systems stop advancing, instead of remaining ambiguously "running with zero delta".

Another status update:

Recipe-specific task entrypoints now live in `RecipeTaskService` instead of `TaskSystem`, so the scheduler no longer exposes recipe lifecycle operations directly.

## Recommended phase-6 order from here

1. Extract crafting lifecycle policy out of `TaskSystem`
2. Give `RecipeTask` a slightly stronger lifecycle model
3. Reassess whether `TaskSystem` should still know concrete task types directly
4. Only then evaluate whether a more generic timed-task base is worth it

## Audit conclusion

Phase 6 is not blocked by missing systems.

The core debt is that `TaskSystem` still mixes:

- scheduler mechanics
- recipe lifecycle policy
- stack-specific invalidation rules

The best next step is to separate those layers before introducing any new timed gameplay system.
