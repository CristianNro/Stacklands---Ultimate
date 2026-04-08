# Post Combat V3 Audit

## Scope reviewed

This audit reviews the line-based combat extension added on top of the current V1/V2 combat foundation and the survivor/enemy split.

Reviewed areas:

- authored combat line data
- frontline targeting
- encounter visuals
- reinforcement compatibility
- documentation alignment

## What changed

Implemented:

- `CombatLineRole` in `CardEnums`
- `CombatantCardData.combatLineRole`
- `CombatFormationUtility`
- frontline-aware target selection in `CombatEncounterResolver`
- collapsed multi-line layout in `CombatEncounterVisuals`

## What looks solid

### 1. Encounter ownership stayed clean

`CombatEncounter` still owns only two participant lists:

- `friendlyParticipants`
- `enemyParticipants`

The V3 line system is derived from those lists instead of introducing competing ownership.

### 2. Targeting and visuals use the same truth

The same line priority is now used for:

- deciding the active frontline
- filtering targetable participants
- deciding which lines should appear in the encounter layout

That keeps the system honest and readable.

### 3. The V3 change stayed incremental

The combat scheduler, loot, damage math and interaction-safety layers did not need to be rewritten.

The V3 extension only touched:

- authored combatant data
- target selection
- encounter layout

That is the correct boundary for this feature.

## Remaining limitations

### 1. Existing combatants need authoring to become tanks or ranged units

The new `combatLineRole` defaults safely to `Melee`.

That means older assets continue to work, but they will behave as melee fighters until authored otherwise.

### 2. Targeting is still simple inside one line

Inside the active frontline, the resolver still picks:

- the first living participant in encounter order

That is fine for V3, but it is not yet:

- slot-opposite targeting
- focus-fire heuristics
- role-aware priority inside the same line

### 3. Ranged only changes formation for now

In this V3, ranged units still obey the same frontline rule as every other role.

So `Ranged` currently affects:

- authored line placement
- encounter presentation
- targetability order

But not yet:

- backline reach
- projectile behavior
- bypass rules

### 4. Line collapse is immediate

When a frontline dies, the next occupied line becomes front immediately.

That is mechanically correct, though later it may deserve extra visual polish.

## No new structural problems found

I did not find a new architectural issue introduced by Combat V3.

The main remaining debt is still inherited from earlier phases:

- survivors are not explicitly repositioned after encounter end
- enemy assets still need correct authoring and resave where applicable

## Recommendation

Combat V3 is in a good state to use.

The next sensible follow-up is one of these:

1. author real tanks / melee / ranged units and test mixed-team battles
2. add more visual polish when lines collapse
3. later open a V4 for ranged reach rules or slot-based targeting
