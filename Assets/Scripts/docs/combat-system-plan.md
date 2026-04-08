# Combat System Plan

## Purpose

This document defines the intended gameplay and technical architecture for the first combat implementation.

The target is not a full tactics system.

The target is a clear and scalable V1 that:

- feels compatible with the current card game loop
- does not overload `CardStack`
- keeps combat authority separate from drag, recipes, market and board ownership

## Combat gameplay for V1

The agreed first version is:

- combat happens inside a dedicated `CombatEncounter`
- an encounter always has two opposing teams
- units attack automatically over time
- each unit uses:
  - health
  - attack damage
  - attack interval
- a unit attacks when its timer completes
- target selection follows a deterministic rule and now respects the opposing frontline
- cards in combat are locked from normal interactions
- dead units are destroyed
- when one side is empty, the encounter ends
- surviving units return to the board
- hostilidad V1 se define por `CombatantCardData.faction`
- mismo `faction` no inicia combate
- distinto `faction` puede iniciar combate
- el trigger inicial V1 nace desde el intento de juntar cartas por drop / stack
- una unidad soltada sobre un `CombatEncounter` activo puede sumarse como refuerzo si su `faction` coincide con uno de los dos bandos del encuentro

## Why combat should not be modeled as a normal stack

Even if the visual resembles a grouped pile of cards, combat has different rules than a normal stack:

- it has two explicit opposing teams
- it needs facing / formation layout
- it needs targeting and attack timers
- it needs combat-only lock rules
- it should not behave like inventory grouping or recipe grouping

That means the project should not fake combat as a standard `CardStack` with a different offset.

The cleaner direction is:

- `CardStack` for inventory / crafting / manipulation grouping
- `CombatEncounter` for battle grouping

## Core design goals

1. Keep combat separate from `CardStack`.
2. Keep combat separate from `TaskSystem`.
3. Keep combat state explicit and runtime-owned.
4. Reuse existing board and spawn infrastructure where it helps.
5. Keep visuals separate from combat authority.
6. Make the first version deterministic and easy to debug.

## Architecture overview

The combat system should be split into these areas:

1. combat data on units
2. participant runtime state
3. encounter ownership
4. encounter scheduling
5. encounter resolution
6. encounter visuals

## Proposed main pieces

### `CombatEncounter`

This should be the domain owner of one battle instance.

Responsibilities:

- own the two teams
- know whether combat is active or finished
- hold encounter-local state
- expose a stable encounter boundary for other systems

It should not:

- draw the layout directly
- become the global combat scheduler
- implement drag rules itself

Suggested fields:

- `encounterId`
- `friendlyParticipants`
- `enemyParticipants`
- `state`
- `boardAnchorPosition`

### `CombatParticipantRuntime`

This should be the runtime owner of one card's mutable combat state.

Responsibilities:

- current health during combat
- attack timer progress
- current encounter reference
- current side / team
- combat lock state

Suggested fields:

- `currentHealth`
- `attackTimer`
- `encounter`
- `team`
- `isInCombat`

This should be a specialized runtime component, similar in spirit to:

- `FoodRuntime`
- `CardTransformationRuntime`

`CardInstance` should expose it, but should not become the place where combat is resolved.

### `CombatEncounterSystem`

This should be the central runtime scheduler for active encounters.

Responsibilities:

- track active encounters
- advance participant timers
- trigger attack attempts
- detect deaths
- detect encounter completion

It should use shared time, ideally through the same global time conventions already used elsewhere in the project.

### `CombatEncounterResolver`

This should apply the actual combat consequences.

Responsibilities:

- choose targets
- apply damage
- kill defeated units
- decide when an encounter ends
- release survivors back to the board

Keeping this separate from the scheduler helps preserve clean ownership:

- system advances time
- resolver applies combat rules

### `CombatEncounterVisuals`

This should own the visual presentation of one encounter.

Responsibilities:

- place units in two facing formations
- update combat-facing visuals such as health bars
- keep visual feedback out of combat authority

It should not:

- own damage
- own timers
- own encounter completion

## Data model for V1

The first version should stay intentionally small.

Suggested unit-side combat fields:

- `attackDamage`
- `attackInterval`

`maxHealth` already exists.

The project also needs a stable way to distinguish combat sides.

This can likely build on existing faction-oriented data later, but the first implementation should use a clear explicit runtime side for the encounter itself.

## Encounter lifecycle

### Start

When a valid combat trigger happens:

1. create a `CombatEncounter`
2. assign cards to friendly and enemy sides
3. initialize participant runtimes
4. lock those cards for combat
5. move them into the encounter visuals

### Runtime loop

While the encounter is active:

1. advance attack timers
2. when a timer completes, choose a target
3. apply damage
4. destroy dead units
5. if one side is empty, finish the encounter

### Finish

When combat ends:

1. mark encounter finished
2. release surviving units from combat lock
3. place survivors back on the board
4. destroy the encounter object

## Deterministic target selection for V1

The first rule should stay simple.

Current rule:

- choose the first valid enemy in a deterministic order inside the opposing frontline

The frontline now follows this derived line priority:

1. `Tank`
2. `Melee`
3. `Ranged`

This keeps combat readable and deterministic while adding a first tactical protection layer.

## Interaction rules during combat

While a card is in combat:

- it should not be draggable
- it should not be stored in containers
- it should not participate in recipes
- it should not participate in market interactions

This keeps the first version safe and avoids ambiguous ownership.

## Board integration

Combat should use board infrastructure without turning `BoardRoot` into a combat system.

Good board uses:

- encounter root placement
- returning survivors to valid board positions
- using board-safe positioning when combat ends

Bad board uses:

- storing all combat rules in `BoardRoot`

## Visual guidance

The encounter should visually place teams facing each other.

The first version should include:

- formation layout
- health shown on the card
- optional light hit feedback

The first version should avoid:

- complex movement
- projectile systems
- pathfinding
- manual player-issued orders

## Main architectural risks

The biggest risks are:

1. making combat a disguised `CardStack`
2. letting drag / interaction code decide combat rules
3. scattering combat state across too many unrelated scripts

The main boundary that must remain clear is:

- stack grouping is not combat grouping

## Recommended final direction for V1

Use this split:

- `UnitCardData` -> base combat stats
- `CombatParticipantRuntime` -> mutable combat state per card
- `CombatEncounter` -> one battle instance
- `CombatEncounterSystem` -> time and update loop
- `CombatEncounterResolver` -> targeting, damage, death, finish
- `CombatEncounterVisuals` -> presentation

That gives a first combat version that is simple enough to ship and strong enough to extend later.

## Current implementation status

The first combat foundation already exists.

Implemented so far:

- unit combat data in `UnitCardData`
- `CombatParticipantRuntime`
- `CombatEncounter`
- `CombatEncounterSystem`
- `CombatEncounterResolver`
- `CombatEncounterFactory`
- `CombatFormationUtility`
- `CombatEncounterVisuals`
- `CombatEncounterFeedback`

What is still missing before combat is playable end-to-end:

- broader trigger coverage beyond the current drop / stack flow
- optional richer combat visuals beyond the current V1 feedback
