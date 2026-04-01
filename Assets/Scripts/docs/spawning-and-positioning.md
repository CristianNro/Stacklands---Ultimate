# Spawning and Positioning

## Current implementation

Spawning is currently handled by `CardSpawner`.

Its responsibilities include:

- instantiating the shared card prefab
- parenting it under the board container
- initializing runtime and view state
- choosing a valid anchored position
- resolving overlap for automatic spawns
- animating some spawned results

## Coordinate model

This project relies on `RectTransform.anchoredPosition`.

That means any spawn or placement logic must be explicit about:

- parent `RectTransform`
- anchor/pivot normalization
- container space
- clamped board position

## Current strengths

- cards are normalized to UI coordinates on spawn
- board clamping is already integrated
- animated spawning already respects board constraints reasonably well
- free-space search now goes through `BoardRoot` instead of being reimplemented in both spawner and market

## Current fragility

Historically and architecturally, this area is still sensitive because:

- wrong parents break drag and positioning immediately
- stacks and single cards do not use the exact same footprint rules
- market spawning duplicates some positioning logic already present in `CardSpawner`

## Current rules

- if a spawn or placement bug appears, verify parent container and coordinate space first
- never mix world-space assumptions with anchored UI coordinates casually
- automatic spawns should respect board constraints and existing occupancy
- spawned cards must remain interactable after animation

## Future direction

Spawning and placement should converge toward:

- a single authoritative placement policy
- less duplicated free-space search logic
- stronger integration with board occupancy rules
- clearer separation between "where the card should exist" and "how it animates there"
