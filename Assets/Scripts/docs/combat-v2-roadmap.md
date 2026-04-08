# Combat V2 Roadmap

## Purpose

This document breaks Combat V2 into concrete implementation steps on top of the working V1.

It should be used as the execution roadmap for the next combat expansion.

## Step 1. Add enemy-specific data

Goal:

Create a dedicated authored home for enemy-only rewards.

Implement:

- `EnemyCardData : CombatantCardData`
- `EnemyGuaranteedDropEntry`
- `EnemyRandomDropEntry`

Exit condition:

- enemy assets can author guaranteed and chance-based card drops without polluting all units

## Step 2. Add typed damage enums and entries

Goal:

Define the combat typing contract before changing runtime resolution.

Implement:

- `DamageType`
- `CombatDefenseChannel`
- `DamageTypeModifierEntry`

Exit condition:

- the project has a stable shared type system for V2 combat math

## Step 3. Extend the shared combatant base

Goal:

Make both units and enemies able to author richer combat behavior.

Implement:

- `basePhysicalArmor`
- `baseMagicalArmor`
- `attackDefenseChannel`
- `attackDamageTypes`
- `receivedDamageModifiers`

Exit condition:

- all combatants can describe armor, damage channel and elemental typing

## Step 4. Add validation

Goal:

Prevent broken V2 combat assets from silently entering gameplay.

Implement validation for:

- invalid drop ranges
- invalid drop chances
- duplicate damage types
- duplicate damage modifiers
- invalid armor values

Exit condition:

- V2 combat assets log useful warnings before runtime

## Step 5. Add `CombatDamageResolver`

Goal:

Move V2 combat math out of `CombatEncounterResolver`.

Implement:

- defense-channel selection
- physical or magical armor reduction
- typed damage modifier application
- final damage result object

Exit condition:

- combat math is centralized in a single dedicated service

## Step 6. Refactor `CombatEncounterResolver`

Goal:

Keep encounter orchestration while delegating math.

Implement:

- target selection remains here
- final damage is requested from `CombatDamageResolver`
- target health is reduced by final damage
- attack events expose final resolved damage

Exit condition:

- the encounter resolver remains readable after V2 math is added

## Step 7. Add `CombatLootDropSystem`

Goal:

Spawn enemy rewards through the normal combat kill flow.

Implement:

- listen to participant death
- detect `EnemyCardData`
- resolve guaranteed drops
- roll random drops
- spawn results through `CardSpawner`

Exit condition:

- killing an enemy can produce authored loot without manual scene glue

## Step 8. Add floating damage numbers

Goal:

Make incoming damage legible during combat.

Implement:

- `CombatFloatingDamagePresenter`
- floating TMP popup creation
- move + fade animation
- presentation based on final resolved damage

Exit condition:

- combat damage is visible as floating numbers above the damaged card

## Step 9. Audit V2 combat

Goal:

Verify the new layer did not break the V1 foundation.

Review:

- kill flow
- encounter completion
- ally and enemy reinforcements
- loot spawning
- damage math
- interaction safety
- combat feedback layering

Exit condition:

- Combat V2 feels additive, not destabilizing
