# Post Roadmap Consolidation Audit

## Scope

This audit revisits the overall roadmap after the recent consolidation work on:

- interaction and drop resolution
- recipes
- tasks
- market boundaries
- board authority
- validation
- domain tests

The goal is to answer two practical questions:

1. does any roadmap phase need to be reopened right now
2. which documentation still needed cleanup after the latest implementation passes

## High-level conclusion

No roadmap phase needs to be reopened immediately.

The remaining open items are now mostly feature-driven, not architecture-blocking.

That means the project is in a good place to switch focus from roadmap refactors to new mechanics, while reopening specific phases only when a concrete gameplay need justifies it.

## Phase-by-phase reading

### Phases 4 and 5

These are effectively closed for the current project stage.

- interaction already has a real drop pipeline
- target contracts are stronger
- recipes already use stronger typed contracts and a tested matcher

They should only be reopened if:

- a new drop-target family appears
- recipe matching needs a genuinely new model
- a new scene-coupling problem appears in live gameplay

### Phase 6

This phase is substantially consolidated and does not need immediate reopening.

The current task model is already good enough for present crafting and for future timed systems, unless a new mechanic demands a broader generic scheduler.

### Phase 7

This is still the clearest phase with meaningful architectural depth left.

However, it should not be reopened blindly.

It only makes sense to reopen when economy design becomes concrete enough to answer questions such as:

- do buy and sell values diverge
- do vendors behave differently
- do prices react to time or stock

Until those answers exist, keeping phase 7 parked is the right call.

### Phase 8

This phase does not need reopening right now.

Container snapshots already cover the current runtime needs well enough.

It should be reopened only if future mechanics require richer stored runtime state.

### Phase 9

This phase is in a healthy place.

Board authority is already much stronger than before, and no immediate reopen is needed unless new mechanics create fresh placement or occupancy pressure.

### Phases 10 and 11

These are now best understood as consolidation phases and are effectively closed for the moment.

- docs are much more honest than before
- validation exists across cards, containers, market and infrastructure
- domain tests now cover market economy, recipe matching, recipe timing and container snapshots

## Documentation cleanup completed in this pass

The latest cleanup aligned these points:

- phase 4 is now documented as closed for now
- phase 5 is now documented as closed for now
- phase 11 no longer implies subtype-to-`CardType` validation that no longer exists
- board/reference validation wording no longer reflects the older `playArea` assumption

## Remaining documentation posture

Some historical phase documents still mention older states on purpose.

That is acceptable when they are clearly framed as history rather than current-state guidance.

The active docs are now coherent enough that no additional documentation-only sweep is urgently required before moving to new gameplay work.

## Recommendation

The roadmap should now be treated as:

- a preserved architectural baseline
- a reference for reopening phases only when new gameplay demands it

The project can now move into new mechanics or content work without another broad architecture pass first.
