# Post Unit Enemy Separation Audit

## Scope

This audit covers the migration that separated survivor units from enemies while preserving current authored unit assets.

## Final shape

The hierarchy now is:

- `CardData`
- `CombatantCardData`
- `SurvivorUnitCardData`
- `UnitCardData`
- `EnemyCardData`

Meaning:

- `CombatantCardData` = shared authored combat data
- `SurvivorUnitCardData` = hunger / daily food layer
- `UnitCardData` = concrete survivor asset class kept for compatibility
- `EnemyCardData` = enemy combatant with drops

## What was changed

- combat stats moved out of `UnitCardData` into `CombatantCardData`
- survivor-only needs moved into `SurvivorUnitCardData`
- `UnitCardData` now acts as the stable concrete survivor class
- `EnemyCardData` now inherits from `CombatantCardData`, not from `UnitCardData`
- `CombatParticipantRuntime` now reads from `CombatantCardData`
- `UnitRuntime` now initializes only for survivor units
- `DailyUpkeepSystem` now counts only `UnitCardData`
- `CardType.Enemy` now exists

## Important compatibility decision

The migration deliberately did not replace existing `UnitCardData` assets with a renamed class.

That was the correct choice for this project because it preserves:

- current unit asset references
- current authoring flow
- current scene and prefab expectations

## Important serialization risk that was handled

Adding `CardType.Enemy` introduced a risk:

- inserting a new enum value in the middle of `CardType` would have changed serialized values in existing assets

This was resolved by:

- assigning explicit numeric values
- appending `Enemy` at the end of the enum

That preserves old serialized `CardType` meanings.

## Runtime result

The runtime ownership now matches the intended design better:

- `CombatParticipantRuntime` is shared by survivors and enemies
- `UnitRuntime` is survivor-only
- enemies no longer inherit hunger or daily food consumption through data

## Residual risks

1. existing enemy assets must still be authored as `EnemyCardData` and should be reviewed to ensure:
   - `cardType = Enemy`
   - `faction = Enemy`

2. some docs outside the newest combat/card docs may still mention the older simpler model, especially older historical phase docs

3. if future systems look only at `CardType.Unit` to mean "anything that fights", they will now miss enemies

## Conclusion

The separation is now real at the domain level without breaking current survivor assets.

That is the right migration boundary for this codebase.
