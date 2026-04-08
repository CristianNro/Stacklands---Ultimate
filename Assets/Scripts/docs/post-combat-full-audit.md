# Post Combat Full Audit

## Scope

This audit covers:

- combat V1
- combat V2
- survivor vs enemy data separation

## General conclusion

The combat stack is now structurally sound.

The project has:

- a working encounter model
- runtime-owned combat state
- typed V2 damage hooks
- enemy-authored drops
- floating damage feedback
- a real survivor vs enemy data separation

The current remaining issues are mostly:

- asset migration cleanup
- a few behavior edge cases
- documentation alignment outside the most recent docs

## Main findings

### 1. Enemy assets still need manual migration cleanup

The code-side separation is complete, but existing enemy assets can still remain partially serialized with the old shape until they are opened and saved again in Unity.

Observed example:

- `Assets/Cards/Enemies/Goblin.asset`

Risks:

- old serialized fields can still appear in YAML
- old `cardType` values can remain until the asset is revalidated and saved

Impact:

- gameplay may still work because runtime logic now keys mostly from subtype and combatant data
- but authoring state can look misleading until the asset is resaved

### 2. Survivors are released from combat, but not actively repositioned after encounter end

`CombatEncounterResolver` currently:

- releases survivors from combat
- destroys the encounter root

But it does not explicitly run a "return survivors to nearby free board positions" step.

Impact:

- this can still be acceptable because cards are already on the board canvas
- but it falls a bit short of the original V1 wording that survivors "return to the board"

This is not a critical bug, but it is still a design gap.

### 3. Enemy loot spawning depends on assets really being `EnemyCardData`

`CombatLootDropSystem` only rewards:

- `EnemyCardData`

That is correct for the new model, but it means old hostile cards still authored as survivor units will not drop loot through the V2 pipeline.

Impact:

- this is a clean boundary
- but it makes asset migration mandatory for loot behavior

### 4. Floating damage presenter depends on scene wiring

The floating damage numbers are presentation-only and correctly decoupled.

But they require:

- `CombatFloatingDamagePresenter` in scene
- `CombatEncounterResolver` in scene

The current scene appears to be wired correctly, so this is not a live bug, just an operational dependency.

### 5. V2 damage math is currently additive by design

Matching `DamageTypeModifierEntry` values are summed additively.

This is valid, but it is important to keep explicit because future balancing might expect a different stacking rule.

This is a design note, not a defect.

## Documentation state

The core current docs are aligned:

- `card-system.md`
- `combat-system-plan.md`
- `combat-roadmap.md`
- `combat-v2-plan.md`
- `combat-v2-roadmap.md`
- `unit-enemy-separation-plan.md`

Older historical phase docs still reference older assumptions in places.

That is acceptable as long as they are treated as historical records, not as the active contract.

## Recommended next cleanup

Before opening another large combat phase, the best cleanup pass would be:

1. resave all enemy assets as `EnemyCardData`
2. verify `cardType = Enemy`
3. verify `faction = Enemy`
4. decide whether survivor reposition after encounter end should become explicit

## Final assessment

Combat V1, V2 and the survivor/enemy separation are in a good state.

The current debt is mostly:

- migration cleanup
- a few polish-level runtime rules

not foundational architecture problems.
