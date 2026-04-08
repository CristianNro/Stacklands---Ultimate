# Phase 4 CardDrag Audit

## Purpose

This document audits `CardDrag` as the current interaction bottleneck for stack, market, container and board drop flows.

Historical note:

This audit reflects the state before the later extraction work fully landed.

Several major findings here are now partially or fully resolved by:

- `CardDropResolver`
- `CardDropTargetResolver`
- domain drop handlers
- stronger board-facing placement helpers

Use this file as phase-history context, not as the final description of the current interaction architecture.

Current status note:

Phase 4 is now effectively closed for the current project stage.

What this document still preserves well:

- why `CardDrag` used to be the architectural bottleneck
- why the pipeline split was needed
- what ownership we were trying to move out of drag

What is no longer current-state accurate:

- `CardDrag` is no longer the main owner of market/container/stack drop branching
- handler precedence no longer lives only in one big `OnEndDrag(...)`
- target resolution is no longer only a raw hierarchy lookup concern

The goal is not to redesign everything at once.

The goal is to identify:

- what belongs to input handling
- what belongs to drop-resolution orchestration
- what still counts as gameplay/business logic inside `CardDrag`

## Files reviewed

- `CardInteraction/CardDrag.cs`
- `Market/MarketSellSlot.cs`
- `Market/MarketPackPurchaseSlot.cs`
- `CardData/Runtime/ContainerRuntime.cs`
- `StackManagment/CardStackFactory.cs`
- `Board/BoardRoot.cs`

## Current role of `CardDrag`

`CardDrag` currently does all of the following:

- detects drag begin / drag move / drag end
- decides whether the player can drag at all
- decides whether to split a stack
- decides whether to drag a full stack or a substack
- resolves board placement fallback
- resolves sell drop
- resolves purchase drop
- resolves container storage drop
- resolves stack merge
- resolves stack creation on single target cards
- decides when to restore surviving dragged objects back to the board

This means it is not just an input component.

It is an interaction controller plus a business-rule dispatcher.

## Findings

### 1. `CardDrag` mixes input intent with business resolution

Severity: high

`OnEndDrag(...)` does not only interpret pointer state.

It also decides:

- market sell precedence
- market purchase precedence
- container precedence
- stack merge validity
- stack creation fallback
- board placement fallback

That makes `CardDrag` the traffic controller of several subsystems at once.

Consequence:

- adding a new drop target means changing this file
- interaction order is implicit in if-block order
- future bugs will tend to appear as priority conflicts between drop cases

### 2. Drop precedence is hardcoded and fragile

Severity: high

Current priority order inside `OnEndDrag(...)` is effectively:

1. sell slot
2. purchase slot
3. target card discovery
4. container storage
5. stack merge
6. stack creation
7. board fallback

This is not represented as a rule table or ordered handler chain.

It is embedded in the structure of the method.

Consequence:

- changing drop precedence is risky
- adding a future drop rule can silently reorder existing behavior

### 3. Stack drag preparation is still correctly local, but tightly coupled

Severity: medium

`OnBeginDrag(...)` does a reasonable amount of local work:

- asks `CardInstance` if the card is movable
- asks `CardStack` if dragging from that position is allowed
- splits a stack when dragging from the middle

This is acceptable to keep near input for now.

The problem is not that `CardDrag` knows about stacks.

The problem is that the same component also resolves every downstream result after the drop.

### 4. Board placement fallback is repeated many times

Severity: medium

The method repeatedly does:

- compute board point
- place dragged object on board

This is structurally simple, but it reveals that failure handling is duplicated in many branches.

Consequence:

- future drop handlers will likely keep re-implementing the same fallback pattern
- success/failure branching is harder to reason about than it should be

### 5. Stack-specific flows are still orchestrated from the drag layer

Severity: medium

For stack drags, `CardDrag` currently decides:

- whether all cards should be stored into a container
- whether cards should merge into an existing stack
- whether a new stack should be created on top of a single target card

Even though validation is delegated to `ContainerRuntime`, `CardStack` and `CardStackFactory`, the orchestration still lives in `CardDrag`.

Consequence:

- stack interaction policy is split between interaction code and domain code
- testing drop behavior still requires reading `CardDrag` first

### 6. Market integration is cleaner than before, but still drag-owned

Severity: medium

`MarketSellSlot` and `MarketPackPurchaseSlot` now own more of their economic logic, which is good.

But `CardDrag` still decides when they get the chance to resolve the drop and what happens if they succeed.

Example:

- purchase success triggers special restoration of the dragged object to board if a surviving object still exists

That is interaction-specific, but it is also a domain-flavored post-resolution rule.

Consequence:

- market drop lifecycle is split across slot code and `CardDrag`

### 7. `CardDrag` still depends on scene lookup style targeting

Severity: medium

Drop resolution uses:

- `eventData.pointerCurrentRaycast.gameObject`
- `GetComponentInParent<MarketSellSlot>()`
- `GetComponentInParent<MarketPackPurchaseSlot>()`
- `GetComponentInParent<CardView>()`

This is workable, but it means drop semantics depend on scene hierarchy and component presence.

Consequence:

- target resolution is not explicit as a domain concept
- future overlap of drop zones may become harder to debug

### 8. The current class is still a reasonable shell for pure input

Severity: low

Not everything should leave `CardDrag`.

These responsibilities still make sense close to the component:

- caching local references
- toggling dragging state
- moving the dragged rect during pointer movement
- identifying the dragged visual root
- initiating a stack split when the drag starts

The refactor target should not be “empty `CardDrag`”.

It should be “thin `CardDrag`”.

## Responsibility split proposal

### Keep in `CardDrag`

- drag begin / drag move / drag end input lifecycle
- initial dragged object selection
- local visual drag state (`CanvasGroup`, `SetDragging`, dragged rect)
- collection of drop context data

### Move out of `CardDrag`

- ordered drop target resolution
- market sell / purchase dispatch rules
- container storage dispatch rules
- stack merge / stack creation dispatch rules
- board fallback policy after failed resolution

## Recommended next extraction

The safest next step is not a giant rewrite.

It is to introduce a single orchestration boundary such as:

- `CardDropResolver`

Responsibilities:

- accept a `CardDropContext`
- evaluate handlers in explicit priority order
- return a `CardDropResolutionResult`

### Minimal context shape

- dragged card
- dragged stack
- dragged rect
- hit object
- target card if any
- target container if any
- board drop point
- whether the drag started as stack drag

### Minimal result shape

- `handled`
- `keepDraggedObjectOnBoard`
- `boardPosition`
- optional reason/debug text

## Recommended handler order

The first staged version can preserve current behavior by keeping this order:

1. market sell handler
2. market purchase handler
3. container storage handler
4. stack merge handler
5. stack creation handler
6. board placement fallback

The difference is that the order becomes explicit and relocatable.

## Refactor strategy

### Step 1

Create:

- `CardDropContext`
- `CardDropResolutionResult`
- `CardDropResolver`

Without changing behavior yet.

### Step 2

Move current `OnEndDrag(...)` branching into `CardDropResolver` almost verbatim.

At this stage:

- `CardDrag` gathers context
- `CardDropResolver` decides outcome
- `CardDrag` only applies the result

### Step 3

Extract per-domain handlers.

This step is now already implemented in the current codebase through:

- `CardDropMarketHandler`
- `CardDropContainerHandler`
- `CardDropStackHandler`

The remaining phase-4 work is no longer basic handler extraction.

The remaining work is:

- continuing to harden target precedence and target contracts as the interaction layer evolves
- reducing dependence on scene-hierarchy discovery
- deciding whether some target types deserve stronger contracts than `GetComponentInParent(...)`

## Audit conclusion

`CardDrag` should no longer be treated as a simple drag component.

Today it is the main interaction chokepoint of the project.

The right architectural next step is not to remove its stack awareness, but to remove its ownership of drop-resolution business flow.
