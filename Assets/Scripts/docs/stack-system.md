# Stack System

## Current role of a stack

A stack is a gameplay structure, not only a visual overlap.

Today `CardStack` is responsible for:

- grouping cards
- maintaining order
- adding and removing cards
- splitting and merging stacks
- updating layout offsets
- exposing contents for recipe checks
- enforcing stackability rules
- enforcing a maximum total weight per stack
- offering structural mutation helpers used by crafting execution

`CardStackCraftingVisuals` now owns:

- active crafting visual state
- progress bar creation and cleanup
- progress bar updates

## Current invariants

The following behaviors are part of the current implementation and must remain correct:

- single cards may exist without a stack object
- multi-card stacks are represented by a `CardStack`
- removing one card must not corrupt the remaining cards
- splitting from the middle must preserve card ownership and positions
- merging must preserve references and valid hierarchy
- cards marked as non-stackable must not enter a stack
- a stack must reject incoming cards when the total weight limit would be exceeded

## Current weakness

`CardStack` currently owns too much.

It is not only a logical stack. It also contains:

- visual layout behavior
- some structural helpers still used by crafting execution

This makes it one of the most sensitive classes in the project.

## Current rules

- gameplay must rely on actual stack membership, not visual overlap alone
- stack cleanup must preserve the remaining card correctly
- stack position and full visual footprint matter for board clamping
- stack changes must keep downstream systems synchronized
- stack acceptance rules must live inside the stack too, not only in callers such as drag/drop

## Future direction

The long-term target is:

- `CardStack` as a clean gameplay aggregate
- stack visuals handled separately
- crafting execution handled outside the stack
- fewer UI responsibilities inside the authority object

## Practical guidance

For now, stack changes should be conservative.

Do not:

- duplicate stack ownership logic elsewhere
- fake stacks through parent changes without using the real stack flow
- add unrelated feature logic directly into `CardStack` unless no safer extension point exists
