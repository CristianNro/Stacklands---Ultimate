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

The biggest remaining weakness is not clamping itself.

Card lifecycle is now much healthier than before:

- stacks are bootstrapped and emit board-level events
- cards are also bootstrapped and emit board-level events
- `ActiveCards` is no longer tied only to explicit spawn paths

The biggest remaining weakness is now that board-space entry and conversion are still somewhat scattered across other systems.

## Current rules

- cards must stay within valid board areas
- stacks must remain fully inside allowed bounds
- clamping must consider the complete stack footprint
- calculations must use the correct `RectTransform` context
- drag-time bounds and final board-placement bounds may differ:
- drag should respect `PlayArea` when it exists
- final placement should still resolve back into `CardsContainer`

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
- card-level lifecycle events
- board-space entry and reparent helpers for systems that currently convert positions ad hoc

That last boundary already started to move:

- `BoardRoot` now offers board-point conversion helpers
- `BoardRoot` now offers board-placement/reparent helpers
- drag, stack cleanup and market spawn positioning already began consuming those helpers
- stack roots now also begin to be created through board-facing helpers instead of only by local factory code

Occupancy also started to improve:

- free-space checks no longer rely only on approximate anchored-position distance
- the board now projects card rect bounds into container space
- moving stacks ignore their own child cards during occupancy checks
- drag-time clamping and final board placement are now intentionally different concepts
- drop-time stack targeting can also use real rect coverage over underlying cards when the cursor is not exactly over the destination card
