# Post Single-Card Transformation Audit

## Scope

This audit reviews the currently implemented:

- transformation data assets
- per-card runtime state
- transformation scheduling
- transformation execution
- transformation progress visuals

## General conclusion

The new single-card transformation system is well aligned with the project direction.

The most important architectural choices were made correctly:

- the system is separate from recipes
- the system is separate from `TaskSystem`
- runtime progress belongs to the card, not the stack
- time comes from the shared `GameTimeService`
- completion reuses the existing spawn flow

This means the system is a strong extension of the current architecture, not a bypass around it.

## What is solid

## Ownership boundary

The current split is good:

- `CardTransformationRule` owns authoring data
- `CardTransformationRuntime` owns per-card progress
- `CardTransformationSystem` owns scheduling
- `CardTransformationExecutor` owns completion effects

That makes the flow easy to reason about.

## Shared time integration

Using `GameTimeService` as the time source was the right decision.

The project still has one shared gameplay clock instead of parallel timer subsystems.

## Context model

Using stack-provided capabilities as the first context source is also a good fit.

This keeps the mechanic capability-driven instead of hardcoding scene-type rules like:

- stacks always speed up
- containers always pause

## Visual boundary

The current transformation progress bar inside `CardView` is acceptable for the first version because:

- it is optional per rule
- it only reflects `CardTransformationRuntime`
- it does not become gameplay authority

## Current limitations worth keeping in mind

These are the main real limitations of the current implementation:

1. cards stored in containers do not progress
   - once stored, they stop existing as active runtime objects on the board
   - so `runOnlyOnBoard = false` is not fully meaningful yet for stored cards

2. completion still allows source destruction even when result spawning fails
   - this is now intentional behavior, not a bug
   - it is useful for consumptive or destructive transformations

3. `sourceCard` mismatches are now blocked in runtime initialization
   - editor validation still warns about bad authoring
   - `CardInstance` now also refuses to start a mismatched transformation rule during play

## Suggested next technical priorities

If transformation development continues, the next most useful steps are:

1. add automated tests for scheduler and executor behavior
2. support stored-card context only when cards inside containers gain active runtime presence

## Final conclusion

The current transformation system is already good enough for real gameplay cases like:

- baby growth
- food spoilage
- egg hatching
- timed self-destruction

The remaining work is mostly hardening and test coverage, not a structural rewrite.
