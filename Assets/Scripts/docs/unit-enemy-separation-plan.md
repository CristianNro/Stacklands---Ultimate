# Unit Enemy Separation Plan

## Purpose

This document defines the migration needed to separate survivor units from enemies without breaking the current project structure.

The immediate motivation is:

- enemies should not inherit survivor needs
- combatants should still share combat data and combat runtime
- current `UnitCardData` assets should remain compatible

## Main design rule

The separation should happen at the data model level, not by duplicating combat runtime.

That means:

- shared combat state stays in `CombatParticipantRuntime`
- shared combat authored data moves to a new common base
- survivor-only needs stay outside enemy data

## Recommended hierarchy

Use this hierarchy:

- `CardData`
- `CombatantCardData`
- `SurvivorUnitCardData`
- `UnitCardData`
- `EnemyCardData`

### Why both `SurvivorUnitCardData` and `UnitCardData`

The project already has authored assets bound to `UnitCardData`.

To avoid breaking those assets during the migration:

- `UnitCardData` should remain as the concrete asset-facing survivor class
- `SurvivorUnitCardData` should exist as the semantic survivor-only layer

So the runtime meaning becomes:

- `CombatantCardData` = anything that can fight
- `SurvivorUnitCardData` = combatant with hunger / upkeep needs
- `UnitCardData` = concrete survivor asset class kept for compatibility
- `EnemyCardData` = combatant enemy asset class

## Data ownership after the split

### `CombatantCardData`

This new base should own:

- `faction`
- `maxHealth`
- `attackDamage`
- `attackInterval`
- `basePhysicalArmor`
- `baseMagicalArmor`
- `attackDefenseChannel`
- `attackDamageTypes`
- `receivedDamageModifiers`

This is the common authored combat contract for all combatants.

### `SurvivorUnitCardData`

This layer should own:

- `unitRole`
- `maxHunger`
- `dailyFoodConsumption`

These are survivor-only gameplay concepts.

### `UnitCardData`

This class should remain as the concrete existing survivor asset type.

It should not add new data beyond what `SurvivorUnitCardData` already owns.

Its role is compatibility and stable authoring.

### `EnemyCardData`

This class should inherit directly from `CombatantCardData`.

It should own:

- `guaranteedDrops`
- `randomDrops`

This keeps enemy-only reward authoring out of survivor units.

## CardType decision

The separation should also become explicit in card classification.

Recommended change:

- add `CardType.Enemy`

That makes these filters clearer:

- upkeep only for `CardType.Unit`
- enemy-specific handling can target `CardType.Enemy`
- combat drop logic can allow both `Unit` and `Enemy`

## Runtime ownership after the split

### `CombatParticipantRuntime`

This runtime should move from:

- `UnitCardData`

to:

- `CombatantCardData`

This keeps combat runtime shared across survivors and enemies.

### `UnitRuntime`

`UnitRuntime` should remain survivor-only.

It already carries:

- hunger
- equipment references

Those concepts fit survivors better than generic enemies.

So after the split:

- survivors get `UnitRuntime`
- enemies do not automatically get `UnitRuntime`

## System changes required

### `CardInstance`

`CardInstance` must change its specialized runtime configuration:

- any `CombatantCardData` gets `CombatParticipantRuntime`
- only `UnitCardData` gets `UnitRuntime`

### `DailyUpkeepSystem`

`DailyUpkeepSystem` must stop identifying feedable population by general unit combat type.

It should count:

- `UnitCardData`

and ignore:

- `EnemyCardData`

This should be type-based, not only `cardType`-based, while the migration settles.

### Combat systems

These should switch from `UnitCardData` to `CombatantCardData` where they only need combat data:

- `CombatParticipantRuntime`
- `CombatDamageResolver`
- `CombatEncounterSystem`
- `CombatEncounterFactory`
- `CombatFactionUtility`
- `CardDropCombatHandler`
- `CardView` health visibility check

## Validation changes required

Validation must be split by subtype:

- `CombatantCardData` validation for combat stats
- `SurvivorUnitCardData` validation for hunger / daily food
- `EnemyCardData` validation for drops and recommended enemy classification

## Compatibility notes

This migration deliberately avoids renaming the existing survivor asset class away from `UnitCardData`.

That keeps:

- current authored unit assets stable
- current create-menu path stable
- current inspector references stable

While still creating a real conceptual split between survivors and enemies.

## Implementation order

1. add `CombatantCardData`
2. add `SurvivorUnitCardData`
3. move combat fields from `UnitCardData` to `CombatantCardData`
4. make `UnitCardData : SurvivorUnitCardData`
5. make `EnemyCardData : CombatantCardData`
6. add `CardType.Enemy`
7. adapt runtimes and combat systems
8. adapt `DailyUpkeepSystem`
9. update validation
10. update docs and audit the migration

## Success criteria

The migration should be considered complete when:

1. enemies no longer author hunger or daily food consumption
2. survivors still work without asset breakage
3. combat still works for both sides
4. upkeep ignores enemies
5. enemy-specific systems can reason about `CardType.Enemy`
