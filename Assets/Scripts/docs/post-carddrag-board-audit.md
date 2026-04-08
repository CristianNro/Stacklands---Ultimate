# Post CardDrag and Board Audit

## Scope

This audit revisits the current state of:

- `CardDrag`
- `CardDropResolver`
- `CardDropTargetResolver`
- `BoardRoot`
- `CardStack`
- `CardStackFactory`

The goal is to capture what changed after the Phase 4 and Phase 9 work, and identify what still feels weak or outdated.

## Summary

The interaction and board layers are in a much better place than before.

Major improvements that are now real in code:

- `CardDrag` no longer owns the full drop-resolution branching flow
- drop handling already runs through `CardDropResolver` and domain handlers
- `BoardRoot` now owns card and stack lifecycle registration
- `BoardRoot` now provides board-space conversion and placement helpers
- stack roots are now created through board-facing helpers
- drag-time bounds and final board-placement bounds are now distinct concepts
- board occupancy is more precise than before

The current weakness is no longer "everything is inside `CardDrag`".

The current weakness is that some interaction contracts still describe an older design than the code that actually runs.

## Files reviewed

- `CardInteraction/CardDrag.cs`
- `CardInteraction/CardDropResolver.cs`
- `CardInteraction/CardDropTargetResolver.cs`
- `CardInteraction/CardDropResolutionResult.cs`
- `StackManagment/CardStack.cs`
- `StackManagment/CardStackFactory.cs`
- `Board/BoardRoot.cs`
- `docs/phase-4-carddrag-audit.md`
- `docs/phase-9-board-audit.md`
- `docs/board-and-layout.md`
- `docs/architecture-roadmap.md`

## Findings

### Resolved in cleanup: market now resolves through a single ordered pipeline

Before cleanup, current code did this:

1. runs `CardDropMarketHandler.TryResolve(...)` explicitly before the pipeline
2. then iterates `OrderedHandlers`
3. where `OrderedHandlers` already includes `CardDropMarketHandler.TryResolve`

That means market resolution still has two entry points in the same resolver.

Even if behavior happens to remain correct most of the time, this is a real architectural inconsistency because:

- handler priority is no longer represented in one single place
- market is treated differently from the rest of the handler chain
- changes to market drop behavior now have two resolution paths to reason about

That duplication is now removed.

`CardDropResolver` now has a single ordered pipeline as the only owner of precedence.

### Resolved in cleanup: `CardDropResolutionResult.boardPosition` was removed

`CardDrag` now restores dragged objects to the board using the real released position of the dragged rect, not `resolution.boardPosition`.

That is correct for current UX.

Because of that, the old field stopped being honest as a contract and was removed.

`CardDropResolutionResult` is now smaller and better aligned with the real flow.

### Medium: `phase-4-carddrag-audit.md` is now historically useful, but architecturally outdated

The original Phase 4 audit still describes `CardDrag` as if it owns:

- market precedence
- container precedence
- stack merge/creation branching
- most board fallback orchestration

That is no longer true in the current codebase.

The document is still valuable as historical context, but it should no longer be read as the current shape of the system.

### Medium: `phase-9-board-audit.md` still understates recent board authority gains

The current board audit correctly describes the big direction, but some findings are now partially outdated because:

- card lifecycle is formalized
- board-space conversion is partially centralized
- stack root creation already moved toward board-owned helpers
- drag-time vs final-placement constraints are now distinct in code

The next board audit should treat board ownership as partially achieved, not only "started".

### Low: `CardDrag` still owns some interaction policy, but this is acceptable for now

What remains in `CardDrag` today:

- stack split initiation on drag begin
- moving the dragged rect
- applying drag-time play-area clamping
- restoring surviving dragged objects to board when resolution asks for it
- computing the current released board position

This is reasonable.

It is no longer a critical design smell the way it used to be.

### Low: board authority is stronger, but still singleton-based

This is known and acceptable for the current project stage.

The important point is that the singleton is now thinner and more truthful than before.

## Documentation gaps found

### `docs/phase-4-carddrag-audit.md`

Needs an explicit note that:

- the major extraction goals of Phase 4 are already implemented
- some findings are now historical rather than current-state findings

### `docs/phase-9-board-audit.md`

Needs a note that:

- board-space ownership has progressed further since the first audit pass
- placement helpers, drag/play-area distinction, and stack-root creation already moved toward `BoardRoot`

### `docs/board-and-layout.md`

Mostly aligned, but it should mention more clearly that:

- drag-time bounds can differ from final board placement
- occupancy is now rect-projected rather than purely distance-based

### `docs/architecture-roadmap.md`

Still broadly aligned.

The only thing worth refining later is the wording of Phase 9 so it reflects that board authority is now deeper than "helper spatial fuerte", especially after:

- card lifecycle events
- placement helpers
- stack-root creation
- stronger occupancy

## Conclusion

The current state is much healthier than the older audits suggest.

The most important remaining issue in this area is not drag instability anymore.

The most important remaining issue is no longer this cleanup block.

That part is now resolved in code.

The next remaining issues are the broader ones already captured by the board and interaction docs:

- how far board authority should go over placement policy
- whether some drop targets deserve stronger contracts than hierarchy discovery

## Recommended next step

Before moving on to another big architectural area, a good cleanup pass here would be:

1. refresh the older Phase 4 and Phase 9 docs so they stop describing superseded ownership
2. decide whether to keep deepening board authority or move on to the next phase

## Follow-up audit: current Phase 4 state

This follow-up pass revisits the interaction layer after the later drag, board and cleanup fixes landed.

### What is now solid

- `CardDrag` is now a much thinner input shell than it used to be.
- drop precedence lives in `CardDropResolver` through a single ordered handler pipeline
- target discovery and drop resolution are already separated by `CardDropTargetResolver`
- market, container and stack flows already live in dedicated handlers
- drag-time clamping and final board placement are now distinct concepts
- board fallback now preserves the actual released visual position instead of snapping to the cursor center
- stack targeting can now also come from real rect coverage over an underlying card, not only from the cursor raycast

### Current actionable findings

#### Resolved: `CardDropTargetResolver` now supports stronger explicit target contracts

The resolver now supports explicit target providers through `ICardDropTargetSource`.

Current explicit providers:

- `MarketSellSlot`
- `MarketPackPurchaseSlot`
- `CardView`

Hierarchy lookup still exists as compatibility fallback, but no longer has to be the only path for stronger target contracts.

#### Resolved: `CardDropContext` no longer duplicates target graph state

`CardDropContext` now keeps `targetInfo` as the single source of truth and exposes convenience accessors instead of storing duplicated target fields.

#### 3. `CardDropResolutionResult` is now honest, but still very low-expressiveness

Severity: low

The cleanup already removed the outdated `boardPosition`, which was the right move.

What remains is a very small contract:

- `handled`
- `placeDraggedObjectOnBoard`

That is enough for the current flows, but it does not communicate intent especially clearly.

What still makes sense to improve:

- eventually replace the two-bool shape with a small explicit outcome enum, for example:
  - handled and consumed
  - handled and return-to-board
  - unhandled

This is not urgent, but it would make future handlers easier to read.

#### Resolved: stack layout already moved toward a better-named contract

`CardStack` now reads `CardStackLayoutAnchor` as the preferred contract for per-card stack offset.

`CardDropTarget` remains only as a compatibility alias so existing assets do not break immediately.

### Current recommendation

Phase 4 no longer needs another large refactor.

The interaction layer is already in a healthy place for the current project stage.

What still makes sense now is only targeted cleanup:

1. only revisit target contracts if a new drop type or a real hierarchy bug appears
2. consider a more expressive drop outcome contract only if handler branching grows again
3. migrate legacy assets from `CardDropTarget` to `CardStackLayoutAnchor` only when content churn is acceptable
