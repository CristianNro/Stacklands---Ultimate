# Known Issues

This file records historically fragile areas and current architectural risks.

## Drag and drop

Past issues:

- teleporting when dragging cards from stacks
- bad initial anchored position after spawn
- inconsistent behavior depending on parent/container

Current architectural risk:

- `CardDrag` is too central and currently owns too many gameplay decisions

## Stack integrity

Past issues:

- stacks of one card treated incorrectly
- broken offsets after adding/removing cards
- merge behavior corrupting layout or references

Current architectural risk:

- `CardStack` mixes stack authority, visuals and crafting side effects

## Recipes

Past issues:

- incorrect matching after stack changes
- ambiguous priority between possible matches
- fragile tag-driven behaviors

Current architectural risk:

- recipe rules still rely too much on strings and scene-coupled evaluation

## Crafting

Past issues:

- progress bar staying static
- crafting tasks continuing when they should stop
- invalid stack state during crafting completion
- bad re-evaluation after one completion

Current architectural risk:

- crafting authority is spread across multiple classes

## Board/layout

Past issues:

- stack parent created in the wrong container
- clamping only the top card instead of full stack size
- resolution and anchoring problems

Current architectural risk:

- placement and occupancy logic are not centralized strongly enough

## Containers

Past issues:

- runtime value not updating after storing/releasing contents
- container identity problems when using prefab-based instances

Current architectural risk:

- stored card data is not a full runtime snapshot

## Market/economy

Past issues:

- purchase and change logic becoming hard to reason about
- special handling of containers and currencies adding branching complexity

Current architectural risk:

- economy logic is duplicated across market-facing systems

## Prefabs/references

Past issues:

- required components missing on prefabs
- runtime initialization not called
- null reference chains caused by prefab setup rather than gameplay logic

Current architectural risk:

- shared prefab behavior depends on too many enabled/disabled runtime pieces
