# Project Overview

## Identity

This is a Unity game inspired by Stacklands.

The project is currently built around:

- cards represented as UI prefabs
- drag and drop interactions
- logical stacks
- recipe-driven stack crafting
- single-card timed transformations
- day-cycle driven world progression
- encounter-based combat with runtime health, typed damage hooks, line formations and enemy loot hooks
- timed crafting
- animated card spawning
- a small but growing market/container layer

## Current scope

The implemented base already includes:

- card definitions through `ScriptableObject` assets
- spawned runtime card instances
- stack creation, merge, split and cleanup
- recipe matching by exact ingredients plus additional requirement layers
- timed crafting tasks with repeatable recipes
- board clamping and spawn positioning
- market purchase/sell flows
- containers that can store cards
- day progression with upkeep and day-start events
- single-card transformation rules driven by runtime time

## Main goal

The project should grow by strengthening the current mechanics and architecture, not by replacing them with generic systems.

That said, the current architecture is still transitional. Several systems work correctly but are more tightly coupled than they should be for long-term scalability.

## Current architectural reality

Today the gameplay loop is functional, but the project still depends on:

- `MonoBehaviour`-heavy orchestration
- some classes with too many responsibilities
- partial runtime state models
- direct system-to-system coupling

This means the game is playable and extendable in the short term, but not yet on a truly solid long-term foundation.

## Core gameplay expectations

The following are true in the current project and should be preserved unless deliberately redesigned:

- cards can exist alone or inside stacks
- stacks matter mechanically, not only visually
- recipe matching is driven by stack contents
- crafting may run over time
- valid repeatable recipes can continue while the stack remains valid
- results can spawn with animation
- board constraints matter for both cards and full stacks
- containers and market systems already affect gameplay state

## Future direction

The intended direction is not "more inheritance" or "more managers".

The intended direction is:

- stronger separation between domain, runtime state, interaction and visuals
- cards defined by clearer capabilities
- recipes driven by typed contracts and explicit capabilities
- economy as its own layer
- better persistence of runtime state
- a more authoritative board model

## Related docs

- `docs/architecture.md`
- `docs/card-system.md`
- `docs/stack-system.md`
- `docs/recipe-system.md`
- `docs/crafting-system.md`
- `docs/day-cycle-system.md`
- `docs/combat-system-plan.md`
- `docs/combat-roadmap.md`
- `docs/combat-v2-plan.md`
- `docs/combat-v2-roadmap.md`
- `docs/combat-v3-plan.md`
- `docs/combat-v3-roadmap.md`
- `docs/post-combat-v2-audit.md`
- `docs/post-combat-v3-audit.md`
- `docs/unit-enemy-separation-plan.md`
- `docs/post-unit-enemy-separation-audit.md`
- `docs/post-combat-full-audit.md`
- `docs/architecture-roadmap.md`
