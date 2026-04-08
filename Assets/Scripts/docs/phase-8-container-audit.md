# Phase 8 Container Audit

## Purpose

This audit re-evaluates container and persistence architecture after the storage refactors, runtime snapshot work and currency-container support.

The goal is to identify what part of phase 8 is already solved, what debt still remains, and whether it makes more sense to continue here or move to the next roadmap phase.

## Files reviewed

- `CardData/Runtime/ContainerRuntime.cs`
- `CardData/Runtime/ContainerStorageService.cs`
- `CardData/Data/ContainerCardData.cs`
- `docs/card-system.md`
- `docs/safe-extension-points.md`

## Current strengths

### 1. Containers are now scoped correctly as storage-only systems

Containers no longer mix:

- card storage
- scenario travel
- scene switching

That is a major architectural cleanup.

The current meaning of a container is now much clearer:

- a card that stores other cards
- with typed storage rules
- and controlled release behavior

### 2. Storage now uses explicit snapshots

`ContainerStorageService` already stores:

- card definition
- runtime uses remaining
- anchored position
- runtime value override

That is much stronger than the old ad hoc data shape.

### 3. Container filtering is already much more expressive

`ContainerRuntime` now supports:

- `CardType` allow/block filtering
- explicit currency-container behavior
- optional `ResourceType` filtering

That is already a solid base for content authoring.

### 4. Currency containers now integrate with runtime value

`ContainerRuntime.RefreshRuntimeValueFromContents()` already turns stored value into a runtime override on the container card when the container acts as currency storage.

That is a good example of stronger ownership between storage and runtime state.

### 5. Partial release already exists

`maxCardsReleasedPerOpen` and `ReleaseContents(...)` already support limited-output release flows.

That means the system is already beyond a naive "dump everything" implementation.

## Remaining weaknesses

### 1. Snapshot depth is still intentionally narrow

Severity: medium-high

The snapshot model preserves some runtime state, but not all possible future runtime complexity.

Today it preserves:

- uses remaining
- anchored position
- runtime value override

It does not yet preserve richer subtype-specific runtime state such as:

- unit status
- building-specific runtime progress
- future transformation state
- future time-based status

Consequence:

- current gameplay is covered reasonably well
- future richer runtime systems will eventually need a deeper snapshot contract

### 2. Storage is still globally in-memory and session-scoped

Severity: medium

`ContainerStorageService` is a global in-memory registry that survives scene changes during the current session.

That is fine for current scope, but it is not yet:

- savegame persistence
- deterministic cross-session storage
- a richer data authority layer

Consequence:

- good enough for now
- not yet the final persistence model if the project grows toward save/load

### 3. Container storage still destroys and respawns live card instances

Severity: medium

That is the correct approach for this architecture today, but it means all persistence fidelity depends on snapshot completeness.

Consequence:

- current design is sane
- future state-heavy cards will depend on stronger snapshot evolution

### 4. Snapshot construction still lives partly on `CardInstance`

Severity: medium

`CardInstance.CreateStoredSnapshot()` and `ApplyStoredSnapshot(...)` are already a good step, but this means container persistence still depends on every relevant runtime owner cooperating correctly.

Consequence:

- reasonable for the current project
- future richer runtime systems may need a more explicit snapshot-participation contract

### 5. Nested storage and richer ownership are still intentionally unsupported

Severity: low-medium

Containers currently reject `ContainerCardData`.

That is the right decision for now, but it also means phase 8 is not yet addressing:

- nested containers
- more complex storage graphs
- inventory-like hierarchies

Consequence:

- no immediate problem
- only relevant if future design asks for nested storage

## What is already effectively done in phase 8

These original phase-8 goals are already largely covered:

- containers store explicit snapshots
- containers no longer act as scene-travel systems
- storage rules are much clearer
- storage/release behavior is significantly stronger than the original prototype level

So phase 8 should not be treated as "containers still need redesign from scratch".

That redesign already happened in a meaningful way.

## What phase 8 now really means

At this point, phase 8 is mainly about:

1. deciding how deep snapshot fidelity should become
2. deciding how future runtime systems participate in persistence
3. deciding whether persistence must remain session-only or evolve toward save/load

## Best next step

The best next step is not another broad redesign of containers.

The best next step would be to wait until another runtime-heavy system is introduced, then evolve snapshots deliberately around that real need.

Examples:

- single-card transformations
- richer unit state
- save/load
- building production state

Without one of those pressures, further refactoring in phase 8 would mostly be speculative.

## Recommended phase-8 order from here

1. keep the current snapshot model as the active baseline
2. when a new runtime-heavy system appears, decide whether its state must be persistable inside containers
3. only then deepen snapshot contracts
4. evaluate save/load persistence separately from in-session container storage

## Audit conclusion

Phase 8 is already much more complete than the roadmap originally implied.

The main remaining debt is not conceptual container design anymore.

The main remaining debt is:

- limited snapshot depth
- no broader persistence contract for richer future runtime systems

Because of that, phase 8 does not currently look like the highest-payoff next phase unless a new feature creates direct pressure on persistence depth.
