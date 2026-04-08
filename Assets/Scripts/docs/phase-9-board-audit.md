# Phase 9 Board Audit

## Scope

This audit revisits the current board authority boundary after the work already done in stacks, recipes, timed tasks, drag/drop, economy, and containers.

Historical note:

This audit captures the first main pass of Phase 9.

Later work already pushed board authority further through:

- board-level card lifecycle events
- board-space conversion helpers
- drag-time play-area clamping versus final board placement
- board-owned stack-root creation helpers
- more precise occupancy checks

The goal is not to redesign the board from scratch.

The goal is to understand whether `BoardRoot` is already authoritative enough to support future systems safely, and if not, where ownership is still diffuse.

## Files reviewed

- `Board/BoardRoot.cs`
- `Buttons/CardSpawner.cs`
- `CardInteraction/CardDrag.cs`
- `CardInteraction/CardDropTargetResolver.cs`
- `StackManagment/CardStack.cs`
- `StackManagment/CardStackFactory.cs`
- `Market/MarketPackPurchaseSlot.cs`
- `Market/MarketSellSlot.cs`
- `docs/board-and-layout.md`
- `docs/architecture-roadmap.md`

## What is already in a good place

### 1. Stack registration is now board-level

`BoardRoot` already owns active stack registration and exposes:

- `OnStackRegistered`
- `OnStackUnregistered`
- `ActiveStacks`

That is a real improvement because recipe evaluation no longer needs to scan the scene every frame.

### 2. Bounds and free-position queries are centralized

`BoardRoot` already owns:

- `GetClampedPosition(...)`
- `ClampCardToBoard(...)`
- `FindNearestFreePositionForRect(...)`
- `FindNearestFreePoint(...)`

That means placement policy is no longer duplicated between spawner and market.

### 3. Board container ownership is mostly consistent

`CardSpawner` and `CardStackFactory` already prefer `BoardRoot.CardsContainer`.

This reduced earlier risk around cards or stacks spawning under the wrong parent.

## Main findings

### Resolved in first Phase 9 step: card lifecycle registration is now board-level

`BoardRoot` now bootstraps existing `CardInstance` objects, and `CardInstance` now registers and unregisters itself through lifecycle instead of depending on opportunistic registration from `CardSpawner`.

Current behavior now:

- `BoardRoot` bootstraps pre-existing scene cards in `Awake()`
- `CardInstance` registers on enable and unregisters on disable/destroy
- `BoardRoot` now exposes `OnCardRegistered` and `OnCardUnregistered`

Impact:

- `ActiveCards` is now far closer to a trustworthy board-level view
- occupancy and future board-driven systems can rely on a real card lifecycle stream
- card registration is no longer tied to one specific spawn path

### High: board authority is still query-centric, not ownership-centric

Today `BoardRoot` answers spatial questions, but many systems still decide spatial flow locally.

Examples:

- `CardDrag` still computes board-local points and reparents directly
- `CardStackFactory` still instantiates stack objects directly and only uses board for parent selection
- market slots still compute slot-to-board spawn points locally

Impact:

- placement policy is more centralized than before, but not yet truly owned by the board
- multiple systems still know how to "enter board space" on their own

### Medium: occupancy is still approximate

`BoardRoot` free-space checks currently rely on `activeCards` positions and the moving card size.

That is useful, but it is still a lightweight heuristic, not a formal occupancy model.

Notable limitations:

- it does not use the other card's actual size during overlap checks
- it does not distinguish between "registered" and "spatially active" cards
- it is not stack-aware beyond clamping extents

This is acceptable for current gameplay, but it is not yet a deep board authority model.

### Resolved in first Phase 9 step: card-level board events now exist

Stacks already had board-level events.

Cards now do too.

This improves future systems that care about:

- card presence on board
- card spawn/despawn
- board-level single-card transforms
- global board-driven queries

because they no longer need ad hoc hooks just to know when cards enter or leave the board.

### Medium: board-space conversion is still scattered

There are repeated patterns converting:

- screen/UI positions into board-local positions
- slot positions into board spawn points
- card positions into board positions when removing from stacks

Those conversions are currently correct enough, but they are spread across:

- `CardDrag`
- `CardStack`
- `MarketPackPurchaseSlot`
- `MarketSellSlot`

That is a sign that board-space entry and placement are not yet fully owned by `BoardRoot`.

## Conclusion

Phase 9 is not blocked by a broken board.

The board is already useful and significantly more authoritative than earlier in the project.

But the audit confirms that `BoardRoot` is still:

- a strong spatial helper
- a stack registry
- not yet the full authority for board entity lifecycle and placement flow

The biggest remaining gap is not clamp math.

After the first Phase 9 step, the biggest remaining gap is now board-space ownership:

- card lifecycle is much healthier
- but multiple systems still decide how to enter board space on their own

## Recommended next step

The best next step for Phase 9 is now:

1. move more board-space entry logic behind board-facing helpers
2. reduce repeated screen-to-board and reparent-to-board conversions
3. let more systems ask the board to place things instead of rebuilding the flow locally

## Recommended follow-up after that

Once card lifecycle is formalized, the next valuable step would be:

- move more "enter board space" behavior behind board-facing helpers or services

That includes cases such as:

- reparenting dragged objects back to board
- converting slot positions to board spawn points
- future single-card timed transformations spawning onto the board

This follow-up is now started too:

- `BoardRoot` now exposes helpers to convert screen/world positions into board coordinates
- `BoardRoot` now exposes helpers to place or reparent `RectTransform`s into board space
- `CardDrag`, `CardStack`, and market slots already started consuming those helpers instead of repeating the same conversion code inline
- stack roots also started to be created through board-facing helpers instead of being assembled only by local feature code

The remaining debt in this area is no longer "no board helper exists".

The remaining debt is that some spatial flow is still initiated from feature systems instead of being fully requested through board-owned policies.

## Why this matters

Several future systems want a stronger board boundary:

- single-card timed transformations
- daily world-time effects
- richer spawn validation
- stronger occupancy rules
- board-wide audits and validation

Without card-level board lifecycle ownership, those systems would still need to patch around missing registration guarantees.
