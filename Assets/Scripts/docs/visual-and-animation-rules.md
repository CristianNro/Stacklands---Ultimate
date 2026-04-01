# Visual and Animation Rules

## Priority

Visual polish matters, but gameplay integrity matters more.

## Current implementation

The project already uses visuals for:

- crafted result spawning arcs
- animated card arrival
- overlap resolution slides
- stack offsets
- crafting progress display
- runtime value labels on cards

## Current rule

Gameplay state remains authoritative.

Animation or presentation code must not become the source of truth for:

- card ownership
- stack membership
- recipe validity
- task state
- board registration

## Current fragility

Some visuals are still too close to authority code:

- `CardStack` currently creates and owns crafting progress visuals
- `CardView` performs value refresh checks over time
- animation and placement still depend on board/runtime details directly

This works, but it is not the long-term ideal.

## Rules

- animation code must not corrupt authoritative gameplay state
- card and stack ownership must remain valid regardless of animation timing
- if animation and gameplay state disagree, gameplay state wins
- visual improvements must not break drag, stack, recipe, crafting, market, or container logic
- any UI/layout change must preserve correct behavior across resolutions

## Future direction

Visual systems should gradually move toward:

- clearer separation from domain authority
- more event-driven updates
- less polling or ad hoc visual creation inside gameplay classes
