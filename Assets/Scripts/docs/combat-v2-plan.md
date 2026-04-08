# Combat V2 Plan

## Purpose

This document defines the intended second combat iteration on top of the current V1.

Combat V2 should deepen the current system without replacing its ownership model.

The target is:

- keep `CombatEncounter` as the battle aggregate
- keep `CombatEncounterSystem` as the time scheduler
- keep `CombatEncounterResolver` as the main combat rule coordinator
- extend authored combat data for richer damage, defenses and rewards
- add better feedback without moving combat authority into `CardView`

## V2 feature goals

The agreed V2 additions are:

1. enemy-specific drop tables
2. richer combat stats on units and enemies
3. typed damage and typed damage modifiers
4. floating damage numbers

## Current V1 baseline

The current implementation already has:

- `UnitCardData.maxHealth`
- `UnitCardData.attackDamage`
- `UnitCardData.attackInterval`
- `CombatParticipantRuntime.currentHealth`
- `CombatEncounterResolver` with deterministic target selection and flat damage
- `CombatEncounterFeedback` with hit / attack / death feedback

The current system does not yet have:

- enemy-only authored drop definitions
- armor resolution
- magic vs physical attack channels
- attack damage types
- weakness / resistance data
- floating damage number presentation

## Main architectural rule

Combat V2 must extend the current system, not bypass it.

That means:

- do not replace `CombatEncounterResolver`
- do not move damage math into `CardView`
- do not create a second parallel combat runtime
- do not invent a separate loot pipeline outside combat kill events

## Enemy-specific drops

### Problem

Today enemies are no longer modeled as a survivor subtype.

They are modeled as their own authored combat subtype on top of the shared combat base.

That is enough for V1 combat, but not enough for authored enemy-specific drops.

### Recommended direction

Introduce a subtype:

- `EnemyCardData : CombatantCardData`

This subtype should hold two drop lists:

- `guaranteedDrops`
- `randomDrops`

### Guaranteed drop entry

Use a serializable entry such as:

- `CardData card`
- `int minCount`
- `int maxCount`

This list means:

- the enemy always drops the entry card
- the actual spawned amount is chosen between min and max

### Random drop entry

Use a serializable entry such as:

- `CardData card`
- `float dropChance`
- `int minCount`
- `int maxCount`

This list means:

- each entry rolls independently
- on success, the actual spawned amount is chosen between min and max

### Runtime integration

Do not put loot generation directly inside `CombatEncounterResolver`.

Instead add a separate listener service:

- `CombatLootDropSystem`

Responsibilities:

- listen to `CombatEncounterResolver.OnParticipantKilled`
- detect whether the dead card uses `EnemyCardData`
- resolve guaranteed and random drops
- spawn those cards through the normal spawn flow

This keeps combat kill authority and reward spawning separate.

## Richer combat stats

### Problem

Today damage is resolved as:

- flat attack damage
- direct subtraction from target health

This is too small for V2.

### New authored combat data

Extend `CombatantCardData` so both survivors and enemies share the same V2 combat contract.

Recommended new fields:

- `int basePhysicalArmor`
- `int baseMagicalArmor`
- `CombatDefenseChannel attackDefenseChannel`
- `List<DamageType> attackDamageTypes`
- `List<DamageTypeModifierEntry> receivedDamageModifiers`

`EnemyCardData` should inherit these automatically from `CombatantCardData`.

### Why the attack defense channel is needed

The request includes:

- physical armor
- magical armor
- attack damage types

That means an attack needs two different concepts:

1. which armor bucket it resolves against
2. which elemental / thematic types it carries

So each attack should declare:

- `Physical`
or
- `Magical`

and separately one or more `DamageType` tags.

Without that extra channel, the resolver would not know whether to use physical or magical armor.

## Damage typing

### Damage type enum

Add a new enum:

- `DamageType`

Initial values:

- `Electricity`
- `Holy`
- `Water`
- `Fire`
- `Earth`
- `Dead`

### Damage modifier entry

Use a serializable entry such as:

- `DamageType damageType`
- `float percentModifier`

Meaning:

- positive percent => receives more damage from that type
- negative percent => receives less damage from that type

Example:

- `Fire, 0.25` means +25% received damage from fire
- `Dead, -0.50` means -50% received damage from dead damage

## Damage resolution architecture

### Problem

`CombatEncounterResolver` currently does target selection, damage application, death handling and encounter completion.

If armor and typed damage are added directly there, it will start mixing too many responsibilities.

### Recommended direction

Add a dedicated rule service:

- `CombatDamageResolver`

Responsibilities:

- calculate final damage from attacker and target
- resolve physical vs magical armor
- apply typed damage modifiers
- clamp final damage as needed
- return a structured result

### Suggested result object

Use a small value object such as:

- `baseDamage`
- `finalDamage`
- `CombatDefenseChannel defenseChannel`
- `List<DamageType> attackDamageTypes`

This keeps future feedback and debugging richer.

### Ownership boundary

Keep this split:

- `CombatEncounterResolver` chooses the target, asks for damage, applies final health change, handles death and finish
- `CombatDamageResolver` knows combat math

## Floating damage numbers

### Problem

The current feedback only flashes and animates the card.

It does not show the damage amount.

### Recommended direction

Add a presentation listener such as:

- `CombatFloatingDamagePresenter`

Responsibilities:

- listen to attack result events
- create floating TMP numbers near the target card
- animate upward movement + fade out

This should not own combat rules.

### Event dependency

Floating numbers should display final damage after all modifiers and armor are applied.

That means this system should read the final resolved damage, not the original authored attack damage.

## Health authority note

There is an important V2 caution:

- `UnitRuntime` already has `currentHealth`
- `CombatParticipantRuntime` also has `currentHealth`

For combat, the authoritative runtime should remain:

- `CombatParticipantRuntime`

V2 should avoid spreading combat damage across both runtimes.

This does not require a full refactor immediately, but it must stay explicit while adding armor, typed damage and floating numbers.

## Validation needs

V2 data requires stronger validation.

Add validation for:

- `EnemyCardData` drop entries with missing cards
- negative drop chances
- invalid min / max combinations
- duplicate damage types in attack damage lists
- duplicate damage modifier entries
- invalid armor values if negatives are not allowed

## Recommended V2 architecture split

Use this shape:

- `CombatantCardData` -> shared authored combat stats
- `UnitCardData` -> concrete survivor asset class kept for compatibility
- `EnemyCardData` -> enemy-only authored drop tables
- `CombatParticipantRuntime` -> mutable health / timer state
- `CombatEncounterResolver` -> encounter-level attack / death orchestration
- `CombatDamageResolver` -> damage calculation
- `CombatLootDropSystem` -> loot generation on enemy death
- `CombatFloatingDamagePresenter` -> floating damage UI

## Recommended implementation order

1. add `EnemyCardData` and drop entry data
2. add `DamageType`, `CombatDefenseChannel` and damage modifier entries
3. extend the shared combat base with new authored combat fields
4. add validation for all new combat data
5. introduce `CombatDamageResolver`
6. refactor `CombatEncounterResolver` to use final resolved damage
7. add `CombatLootDropSystem`
8. add `CombatFloatingDamagePresenter`

## Out of scope for this V2

Unless later requested, this V2 should not include:

- status effects
- critical hits
- dodge
- ranged projectiles
- faction diplomacy matrices
- inventory-style corpse loot windows
- active abilities
