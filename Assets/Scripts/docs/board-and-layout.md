# Board and Layout

## Current implementation

The board is currently represented by `BoardRoot`.

Important concepts in the current code:

- `BoardRoot`
- `CardsContainer`
- optional `PlayArea`
- anchored UI positioning
- board padding and clamping
- full stack footprint checks

## What the board already does

`BoardRoot` currently:

- stores references to the playable card container
- registers active cards
- registers active stacks
- clamps single cards and stacks inside allowed bounds
- calculates stack-aware extents when needed

## Current weakness

The board is useful today, but it is not yet a fully authoritative gameplay boundary.

It does not yet own:

- a formal occupancy model
- centralized placement policy for all systems
- richer higher-level board events beyond basic registration

As a result, some systems still make local decisions instead of consulting a stronger board authority.

## Current rules

- cards must stay within valid board areas
- stacks must remain fully inside allowed bounds
- clamping must consider the complete stack footprint
- calculations must use the correct `RectTransform` context

## Historical fragility

Past and current risk areas include:

- stack objects created in the wrong parent
- clamping based only on a single card instead of full stack extents
- inconsistent assumptions about container space
- duplicated "is this spot free?" logic in different systems

## Future direction

The board should evolve into a more authoritative layer for:

- entity registration
- occupancy and free-space queries
- placement policy
- spawn validation
- stack-aware bounds logic
