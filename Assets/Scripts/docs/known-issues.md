# Known Issues

This file records historically fragile areas and current architectural risks.

## Drag and drop

Past issues:

- teleporting when dragging cards from stacks
- bad initial anchored position after spawn
- inconsistent behavior depending on parent/container

Current architectural risk:

- interaction still depends on scene-hierarchy target discovery and can benefit from stronger explicit target contracts

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
- asset migration debt after the tag-to-capability change

Current architectural risk:

- recipe evaluation still wants further decoupling from scene-bound runtime context even after the move to stronger typed contracts

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

- board placement authority is much stronger now, but some feature flows still decide spatial policy outside the board boundary

## Containers

Past issues:

- runtime value not updating after storing/releasing contents
- container identity problems when using prefab-based instances

Current architectural risk:

- snapshot coverage may need to grow again if card runtime state becomes richer

## Market/economy

Past issues:

- purchase and change logic becoming hard to reason about
- special handling of containers and currencies adding branching complexity

Current architectural risk:

- some market-facing flow still depends on slot orchestration and interaction-layer precedence

## Prefabs/references

Past issues:

- required components missing on prefabs
- runtime initialization not called
- null reference chains caused by prefab setup rather than gameplay logic

Current architectural risk:

- shared prefab behavior depends on too many enabled/disabled runtime pieces
