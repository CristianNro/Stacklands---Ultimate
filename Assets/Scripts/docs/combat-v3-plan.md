# Combat V3 Plan

## Purpose

This document defines the intended third combat iteration on top of the working V1/V2 base.

Combat V3 should deepen the current encounter model without replacing it.

The target is:

- keep `CombatEncounter` as the aggregate owner of one battle
- keep `CombatEncounterSystem` as the time scheduler
- keep `CombatEncounterResolver` as the main attack coordinator
- add line-based formation and frontline targeting
- make formation visible in `CombatEncounterVisuals`

## V3 feature goals

The agreed V3 additions are:

1. every combatant belongs to one combat line role:
   - `Tank`
   - `Melee`
   - `Ranged`
2. each team can occupy up to three lines
3. only the frontmost occupied enemy line can be targeted
4. empty lines collapse both logically and visually

## Current V2 baseline

The current combat system already has:

- shared combat data in `CombatantCardData`
- typed damage and armor resolution
- encounter-based combat scheduling
- deterministic target selection
- multi-unit encounters
- encounter visuals with two facing groups

The current system does not yet have:

- line-specific combat roles
- frontline-aware targeting
- collapsible multi-line encounter layout

## Main architectural rule

Combat V3 must extend the current encounter model rather than replacing it.

That means:

- do not replace `CombatEncounter`
- do not split the encounter into multiple child encounters
- do not move target-selection rules into `CardView`
- do not make `CardStack` responsible for line logic

## Shared authored data

The new authored field should live in `CombatantCardData`, because it applies to both survivors and enemies.

Recommended new field:

- `CombatLineRole combatLineRole`

Recommended values:

- `Tank`
- `Melee`
- `Ranged`

This should stay separate from `UnitRole`.

Reason:

- `UnitRole` expresses broader identity
- `CombatLineRole` expresses tactical formation behavior during combat

## Formation model

Each encounter team still keeps one participant list:

- `friendlyParticipants`
- `enemyParticipants`

V3 should not replace those lists with three stored lists per team.

Instead, line membership should be derived from participant data:

- look at each living participant's `combatLineRole`
- group by line role at read time

This keeps encounter ownership simple and avoids scattering source-of-truth.

## Frontline priority

Line priority should be fixed:

1. `Tank`
2. `Melee`
3. `Ranged`

The first non-empty living line becomes the active frontline for that team.

Examples:

- if a team has tanks, only tanks can be targeted
- if tanks are gone but melee remain, only melee can be targeted
- if only ranged remain, ranged become targetable

## Target selection

`CombatEncounterResolver` should keep ownership of target selection, but the rule should change.

Instead of:

- first living enemy in the whole opposing team list

V3 should use:

1. determine the opposing team's current frontline
2. filter living enemies to that line
3. select the first living target in deterministic encounter order

This keeps targeting deterministic while introducing line behavior.

## Formation utility

To avoid overloading `CombatEncounterResolver` and `CombatEncounterVisuals`, add a helper such as:

- `CombatFormationUtility`

Responsibilities:

- determine a participant's line role
- determine the current active frontline for a team
- collect targetable participants for a team
- collect occupied lines for visuals

This utility should not own damage, timers or encounter lifecycle.

## Encounter visuals

`CombatEncounterVisuals` should evolve from two flat rows into a line-based formation.

Recommended layout:

- friendly side above encounter center
- enemy side below encounter center
- each team can show up to three sub-lines
- the active frontline is closest to the encounter center
- rear lines are placed progressively farther away

The layout should collapse:

- if a line has no living participants, it should not reserve empty space

This means the visible formation should always tell the truth about what is targetable.

## Gameplay rule clarification

For this V3, all attackers still respect the same frontline rule.

That means:

- ranged units do not bypass tanks
- ranged units do not directly target rear lines
- melee units do not get special reach rules

Those extensions can be considered later, but are intentionally out of scope here.

## Recommended V3 architecture split

Use this shape:

- `CombatantCardData` -> authored combat line role
- `CombatFormationUtility` -> derived line grouping and frontline lookup
- `CombatEncounterResolver` -> frontline-aware target selection
- `CombatEncounterVisuals` -> collapsible multi-line formation rendering

## Recommended implementation order

1. add `CombatLineRole` enum
2. add `combatLineRole` to `CombatantCardData`
3. add `CombatFormationUtility`
4. refactor target selection in `CombatEncounterResolver`
5. refactor `CombatEncounterVisuals` to render collapsed lines
6. audit the resulting gameplay and authoring workflow

## Out of scope for this V3

Unless later requested, this V3 should not include:

- ranged attacks bypassing frontline
- taunt mechanics
- area attacks
- slot-opposite targeting
- pathfinding or movement between lines
- active abilities by line
