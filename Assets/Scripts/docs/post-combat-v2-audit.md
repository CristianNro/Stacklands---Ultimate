# Post Combat V2 Audit

## Scope

This audit covers the V2 expansion applied on top of the current combat V1:

- enemy-authored drop data
- typed damage data
- armor data
- dedicated damage math resolution
- enemy loot spawning
- floating damage presentation
- health authority cleanup

## What was changed

- `UnitRuntime` no longer mirrors mutable unit health
- `CombatantCardData` now exists as the shared authored combat base
- `EnemyCardData` now exists as a subtype of `CombatantCardData`
- `UnitCardData` now survives as the concrete survivor asset class on top of `SurvivorUnitCardData`
- enemy loot can now be authored through guaranteed and random drop entries
- combat data now supports:
  - physical armor
  - magical armor
  - defense channel
  - attack damage types
  - received damage modifiers
- damage math now lives in `CombatDamageResolver`
- `CombatEncounterResolver` now emits `CombatDamageResult`
- enemy loot spawning now lives in `CombatLootDropSystem`
- floating damage numbers now live in `CombatFloatingDamagePresenter`

## Findings after implementation

### 1. Health authority is cleaner now

The previous duplicated mutable health between `UnitRuntime` and `CombatParticipantRuntime` is resolved.

The combat-side authority is now clearly:

- `CombatParticipantRuntime`

This reduces the risk of silent desync while adding armor and typed damage.

### 2. Enemy data is now extensible without polluting all units

Using `CombatantCardData` as the shared combat base and `EnemyCardData` as a separate subtype is the healthier boundary.

It keeps:

- shared combat stats in `CombatantCardData`
- survivor-only needs in `SurvivorUnitCardData`
- enemy-only reward authoring in `EnemyCardData`

### 3. Damage math is now separated enough

`CombatEncounterResolver` still owns target selection, health application, death and finish.

The actual damage math now lives in `CombatDamageResolver`, which is a healthier long-term direction.

### 4. Current damage modifiers are additive

The current implementation sums all matching `DamageTypeModifierEntry.percentModifier` values and then applies the final combined multiplier once.

That is valid for V2, but it should remain an explicit design rule because future balancing might want:

- additive stacking
- multiplicative stacking
- cap-limited stacking

### 5. Enemy loot currently rolls each random entry independently

This matches the current requested design well.

If later design wants "choose one item from a weighted table" instead of "roll each entry", that should become a different loot mode, not a silent change to the current one.

### 6. Floating numbers are presentation-only

This is good.

They currently depend on a scene object with `CombatFloatingDamagePresenter` present and subscribed.

That is consistent with the current combat feedback architecture, but the scene must include this presenter for the V2 feedback to appear.

### 7. New scene wiring requirements

To use the full V2 runtime behavior, the scene should now include:

- `CombatEncounterSystem`
- `CombatEncounterResolver`
- `CombatEncounterFactory`
- `CombatEncounterFeedback`
- `CombatLootDropSystem`
- `CombatFloatingDamagePresenter`

`CombatDamageResolver` is auto-created by `CombatEncounterResolver` if missing, so it is not a mandatory extra scene object.

## Residual risks

The main remaining risks are:

1. existing unit assets now have new combat fields and will need authoring passes
2. enemy assets must migrate to `EnemyCardData` to use loot
3. floating damage feedback depends on scene wiring
4. current health still does not persist across scenario transitions or save/load because combat persistence has not been expanded yet

## Conclusion

Combat V2 was added by extending the current combat architecture, not by replacing it.

The result is structurally sound:

- combat data is richer
- health authority is cleaner
- reward spawning and presentation stay outside combat authority
- the resolver remains understandable
