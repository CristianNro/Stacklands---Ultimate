# Combat V3 Roadmap

## Purpose

This document breaks Combat V3 into concrete implementation steps on top of the current V2 baseline.

## Step 1. Add combat line data

Goal:

Define a shared authored way for survivors and enemies to declare formation role.

Implement:

- `CombatLineRole`
- `CombatantCardData.combatLineRole`

Exit condition:

- every combatant can be authored as `Tank`, `Melee`, or `Ranged`

## Step 2. Add formation utility

Goal:

Centralize derived line logic outside the resolver and visuals.

Implement:

- `CombatFormationUtility`
- helpers for frontline lookup
- helpers for targetable participant collection
- helpers for occupied line collection

Exit condition:

- the project has one clear place that defines line priority and frontline logic

## Step 3. Refactor target selection

Goal:

Make combat resolution respect frontline protection.

Implement:

- update `CombatEncounterResolver.SelectTarget`
- only select living enemies from the opposing frontline
- preserve deterministic ordering inside that line

Exit condition:

- tanks protect melee and ranged
- melee protect ranged when no tanks remain

## Step 4. Refactor encounter visuals

Goal:

Make the encounter layout show the same structure the resolver uses.

Implement:

- three possible lines per side
- collapsing empty lines
- frontline closest to the encounter center

Exit condition:

- the encounter presentation makes current targetability legible

## Step 5. Audit V3

Goal:

Verify the line system behaves like an extension of V2, not a rewrite.

Review:

- targeting against mixed teams
- line collapse when a frontline dies
- ally and enemy reinforcements into existing encounters
- visual layout after deaths and reinforcements
- authoring defaults for survivors and enemies

Exit condition:

- Combat V3 feels additive and stable
