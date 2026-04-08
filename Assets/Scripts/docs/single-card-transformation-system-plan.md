# Single Card Transformation System Plan

## Purpose

This document defines the technical design for timed single-card transformations.

This system is intentionally separate from recipes.

Recipes remain responsible for stack-driven crafting and multi-card matching.

Single-card transformations are responsible for cases such as:

- `Bebe -> Aldeano`
- fresh food -> spoiled food
- egg -> chick
- crop stage progression
- timed self-destruction

## Why this should not be solved with recipes

The current recipe flow is stack-centered:

- `RecipeSystem` evaluates `CardStack`
- `TaskSystem` schedules stack crafting
- `StackCraftingExecutor` consumes stack-selected cards

Forcing single-card timed evolution into that model would create artificial stack requirements and blur the meaning of recipes.

That would make the architecture less honest.

The cleaner direction is:

- recipes for stack interactions
- transformations for single-card timed state changes

## Core rules agreed for this system

1. Transformations only run while the card is on the board.
2. Transformations do not cancel just because the card is stacked.
3. Transformations can pause while the card is inside a container, but only if the current context exposes a pausing capability.
4. Transformations can speed up or slow down depending on capabilities found in the current context.
5. On completion, a transformation can:
   - destroy the source card
   - destroy and spawn one result
   - destroy and spawn multiple results

## Complexity

Overall complexity: medium-high.

Why it is not low:

- it introduces a new timed system parallel to crafting
- it needs explicit runtime ownership
- it must read board, stack and container context without becoming tightly coupled to them
- it needs clear completion behavior and result spawning rules

Why it is not extreme:

- it does not need to replace existing systems
- it can be introduced incrementally
- it can reuse established patterns from crafting, snapshots and spawning

## Target architecture

The system should be split into these areas:

1. transformation data
2. transformation runtime state
3. transformation scheduling
4. transformation execution
5. optional visuals

## Proposed files

### Data

- `CardTransformations/CardTransformationRule.cs`
- `CardTransformations/CardTransformationResultEntry.cs`
- `CardTransformations/CardTransformationSpeedModifier.cs`

### Runtime / services

- `CardTransformations/CardTransformationRuntime.cs`
- `CardTransformations/CardTransformationSystem.cs`
- `CardTransformations/CardTransformationExecutor.cs`

### Optional visuals

- `CardTransformations/CardTransformationVisuals.cs`

## Data model

## `CardTransformationRule`

This should be the main authoring asset or embedded rule definition for single-card evolution.

Suggested fields:

- `id`
- `displayName`
- `sourceCard`
- `baseDuration`
- `runOnlyOnBoard`
- `showProgressBar`
- `requiredCapabilities`
- `pauseCapabilities`
- `speedModifiers`
- `completionMode`
- `resultCard`
- `resultCards`

## `CardTransformationCompletionMode`

Suggested enum:

- `DestroyOnly`
- `ReplaceWithSingleResult`
- `SpawnMultipleResults`

## `CardTransformationSpeedModifier`

Suggested fields:

- `CardCapabilityType capability`
- `float speedMultiplier`

Rules:

- values greater than `1` accelerate
- values between `0` and `1` slow down
- `0` should not be used as pause, because pause is clearer as a separate rule

## `pauseCapabilities`

This should be a list of capabilities.

If the current context contains any of them, the transformation pauses.

This is better than hardcoding:

- containers always pause
- stacks always accelerate

Because it keeps the rule capability-driven and reusable.

## `requiredCapabilities`

This should be a list of capabilities that must be present in the current context.

If any required capability is missing, the transformation stays paused and does not advance.

This is the preferred way to express:

- an egg only transforms inside a warm stack
- a baby only grows inside a sheltering stack
- a special item only evolves near a specific enabling card

## Context model

The transformation system should evaluate context, not raw scene type.

The runtime should ask:

- what capabilities exist around me right now?

Not:

- am I in a container?
- am I in a stack?

Those are implementation details of how capabilities are gathered.

## Current-context rules

### If the card is on the board and not in a container

The context should include:

- capabilities from the current stack, excluding the transforming card itself by default

This allows cases like:

- egg gets `Warmth`
- food gets `Preservation`
- baby gets `Shelter`

without making the transforming card self-buff by accident.

### If the card is stored inside a container

The context should include:

- capabilities from the container that currently stores it

This allows:

- storage containers that pause spoilage
- incubators that accelerate eggs
- preservation containers that freeze aging

## Runtime model

## `CardTransformationRuntime`

This should be the per-card runtime state owner.

Responsibilities:

- hold current progress
- hold active rule
- know whether the transformation is active, paused or completed
- evaluate current effective speed
- expose runtime state to visuals if needed

Suggested fields:

- `CardTransformationRule activeRule`
- `float elapsedTime`
- `bool isRunning`
- `bool isPaused`
- `float currentSpeedMultiplier`

Important rule:

This runtime belongs to the card, not to the stack.

## Scheduler

## `CardTransformationSystem`

This should be the central scheduler for individual timed transformations.

Responsibilities:

- find cards that have a transformation rule
- start tracking them
- update active runtimes
- pause or resume depending on context
- complete the transformation when time reaches the threshold

It should react only to cards, not stacks.

## Update behavior

Per update:

1. skip cards not on board
2. gather current context capabilities
3. if any pause capability is present, mark paused
4. otherwise compute effective speed multiplier
5. advance elapsed time by `deltaTime * effectiveMultiplier`
6. when elapsed time reaches duration, execute completion

## Speed calculation

Recommended rule:

1. if any pause capability is present, pause
2. otherwise start from `1f`
3. multiply by every matching speed modifier in context

This is simple, explicit and composable.

Example:

- base `1.0`
- context has `Warmth x2`
- context has `Shelter x1.25`
- final speed = `2.5`

If that ever becomes too permissive, it can later move to:

- strongest modifier only
- capped multiplier

But the first version should stay simple.

## Executor

## `CardTransformationExecutor`

Responsibilities:

- destroy the source card when needed
- spawn zero, one or multiple result cards
- place spawned results correctly on board
- preserve only the runtime state that is explicitly allowed

Recommended first rule:

- do not try to preserve arbitrary runtime state by default
- treat transformation as destroy-and-spawn unless a future case explicitly needs state carry-over

That keeps the system honest and predictable.

## Result spawning

The first implementation should reuse `CardSpawner`.

Spawn position should be based on the source card board position before destruction.

If multiple cards spawn:

- use the source position as origin
- apply the standard board-safe spread behavior

## Conditions and limits

The first version should stay intentionally narrow.

Recommended initial rules:

- only exact source-card based transformations
- no string ids
- no condition scripting
- no nested rule graphs
- no reuse of `TaskSystem`

## Why not reuse `TaskSystem`

Even though both are timed systems, they represent different ownership:

- crafting task belongs to a stack recipe flow
- transformation belongs to one card

Reusing `TaskSystem` now would save code short-term but would blur the boundary we are trying to make stronger.

Better approach:

- create a dedicated card-transformation scheduler
- unify later only if both systems naturally converge

## Integration points

## `CardData`

There should be an explicit place for transformation rule reference.

Possible shape:

- `CardTransformationRule transformationRule`

or

- `List<CardTransformationRule> transformationRules`

Recommendation:

Start with one rule per card.

That covers the use cases already discussed and keeps authoring simple.

## `CardInstance`

Should expose access to transformation runtime if present.

Recommendation:

- do not overload `CardInstance` with transformation logic
- let it own or expose the component reference, not the behavior itself

## `BoardRoot`

Should eventually help this system by being more authoritative about active cards on board.

For the first version, a bootstrap plus spawn/despawn hooks is acceptable.

## `ContainerRuntime`

Should not own transformation rules.

Its role is only to influence context through capabilities when the card is stored inside it.

## Visuals

The current first implementation already exposes an optional progress bar through `CardView`,
controlled by `CardTransformationRule.showProgressBar`.

That keeps the first version easy to author without changing the shared card prefab manually.

Longer term, if transformation visuals grow richer, they should still remain presentation-only
and may deserve a dedicated visuals component.

## Recommended implementation order

1. Create data types:
   - `CardTransformationRule`
   - `CardTransformationCompletionMode`
   - `CardTransformationSpeedModifier`
2. Add transformation rule reference to `CardData`
3. Create `CardTransformationRuntime`
4. Create `CardTransformationSystem`
5. Create `CardTransformationExecutor`
6. Hook spawn/despawn/bootstrap flow
7. Validate board-only behavior
8. Validate stack-based speed modifiers
9. Validate container-based pause capabilities
10. Only then consider visuals

## Current implementation status

The system is now partially implemented.

Implemented data layer:

- `CardTransformationRule`
- `CardTransformationCompletionMode`
- `CardTransformationSpeedModifier`
- `CardTransformationResultEntry`
- `CardData.transformationRule`
- editor validation for transformation assets and mismatched card references

Implemented runtime foundation:

- `CardTransformationRuntime`
- `CardInstance` reference wiring
- snapshot preservation for transformation elapsed time

Current implementation status after that first runtime pass:

- `CardTransformationSystem` now exists as the scheduler for active board cards
- `CardTransformationExecutor` now exists for destroy / replace / multi-spawn completion
- stack context capabilities are now supported
- rules can now require specific context capabilities before they start progressing
- each rule can now opt in or out of the card-level progress bar through `showProgressBar`

Current known limitation:

- cards stored inside containers still leave the board entirely, so container-context pause or speed behavior is not active yet in runtime

Next missing pieces or follow-up work:

- tests for transform scheduling and completion
- container-context behavior once stored cards gain active runtime presence
- stronger executor safety when a completion result cannot be spawned

## Validation scenarios

Minimum tests for the first implementation:

- single card on board transforms after time
- same card inside stack still progresses
- same card inside a stack with speed capability progresses faster
- same card inside a container with pause capability pauses
- same card inside a container without pause capability keeps progressing if that is allowed by the final implementation
- destroy-only transformation works
- single-result transformation works
- multi-result transformation works
- spawned results land in valid board positions

## Main architectural risk

The biggest risk is not timing.

The biggest risk is letting this system become a second recipe system or a second task system with fuzzy ownership.

The boundary must remain:

- stack recipes combine cards
- single-card transformations evolve one card over time

If that boundary stays clear, this system can scale well.
