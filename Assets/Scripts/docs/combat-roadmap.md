# Combat Roadmap

## Purpose

This document breaks the combat plan into incremental implementation steps.

It should be used as the feature roadmap for combat, separate from the large historical architecture roadmap.

## Guiding rule

Combat should be introduced by extending the existing project structure, not by bypassing it.

That means:

- reuse `CardInstance`, `BoardRoot`, `CardSpawner` and the shared time model where appropriate
- do not turn `CardStack` into the combat owner
- do not put combat authority into `CardView` or drag scripts

## Step 1. Add combat data to units

Goal:

Give units the minimum authored stats needed for V1 combat.

Implement:

- `attackDamage`
- `attackInterval`

Validation:

- damage should not be negative
- interval should be greater than zero

Exit condition:

- unit assets can fully describe their basic combat behavior

## Step 2. Add `CombatParticipantRuntime`

Goal:

Create explicit per-card mutable combat state.

Implement:

- current health
- attack timer
- encounter reference
- combat side
- combat lock state

Integrate with:

- `CardInstance`

Exit condition:

- a spawned unit can own combat state without storing it in shared data

Current status:

- implemented as a first per-card combat runtime integrated through `CardInstance`
- already stores health, attack timer, combat team and combat lock state

## Step 3. Add `CombatEncounter`

Goal:

Create an explicit aggregate that owns one battle.

Implement:

- friendly team list
- enemy team list
- encounter state
- anchor position

Exit condition:

- combat is modeled as its own grouped entity rather than as a normal stack

Current status:

- implemented as a first aggregate with:
  - stable encounter id
  - friendly and enemy participant lists
  - active / finished state
  - board anchor position

## Step 4. Add `CombatEncounterSystem`

Goal:

Create the central scheduler for active encounters.

Implement:

- encounter tracking
- attack timer updates
- per-frame combat progression

Exit condition:

- encounters can advance over time without embedding the loop in visuals or drag code

Current status:

- implemented as a first scheduler for active encounters
- already advances participant attack timers with shared game time
- already exposes `OnParticipantReadyToAttack`
- damage, targeting and encounter completion still belong to the next step

## Step 5. Add `CombatEncounterResolver`

Goal:

Keep the combat rules separate from the scheduler.

Implement:

- target selection
- damage application
- death handling
- encounter completion
- survivor release

Exit condition:

- combat rules are readable and testable outside the update loop

Current status:

- implemented as a first combat rules boundary
- already handles:
  - deterministic target selection
  - damage application
  - death handling
  - encounter completion
  - survivor release

## Step 6. Add encounter creation flow

Goal:

Create a safe entry point for starting combat.

Implement:

- encounter factory or equivalent creation boundary
- participant registration
- combat lock setup

Keep V1 trigger rules simple.

Exit condition:

- the game can create a valid encounter from a supported trigger without ad hoc scene manipulation

Current status:

- implemented through `CombatEncounterFactory`
- already creates the encounter root
- already initializes teams
- already marks participants as busy and in combat
- already supports the first automatic trigger through drop / stack attempts
- already supports adding reinforcements into an active encounter when the dropped units match one of the encounter factions
- same faction keeps using the normal stack flow
- different faction starts a `CombatEncounter`

## Step 7. Add `CombatEncounterVisuals`

Goal:

Present the battle without becoming the authority.

Implement:

- facing formation layout
- light feedback for attacks
- health display on the cards

Exit condition:

- combat is readable to the player and visually distinct from a normal stack

Current status:

- implemented as a first facing layout for both teams
- participants are positioned in two rows around the encounter anchor
- combat health is now shown on unit cards as a persistent overlay
- attack and hit feedback now run from resolver events through `CombatEncounterFeedback`

## Step 8. Add combat interaction safety

Goal:

Prevent non-combat systems from corrupting active encounters.

Implement:

- drag lock for cards in combat
- recipe exclusion for cards in combat
- container exclusion for cards in combat
- market exclusion for cards in combat

Exit condition:

- cards in combat are protected from conflicting gameplay flows

Current status:

- drag already respects `IsMovable`, which now blocks busy combat cards
- recipe input now refuses stacks containing busy / combat cards
- containers now reject storing busy / combat cards
- market buy / sell now rejects dragged cards or stacks containing busy / combat cards

## Step 9. Add validation and tests

Goal:

Stabilize the first version before deepening the feature.

Implement:

- asset validation for combat data
- domain tests for:
  - target selection
  - attack timer progression
  - damage application
  - encounter completion

Exit condition:

- combat logic has basic automated safety coverage

## V1 completion criteria

Combat V1 should be considered complete when:

1. two opposing sides can enter a `CombatEncounter`
2. units attack automatically over time
3. health changes are visible
4. dead units are destroyed
5. survivors return to the board
6. combat cards are blocked from conflicting interactions

## Deferred for later versions

These should stay out of the first implementation unless they become essential:

- pathfinding
- player-issued active combat commands
- ranged projectiles
- front-row / back-row tactical rules
- critical hits
- dodge
- armor systems
- status effects
- combat rewards beyond simple death handling

## Recommended first coding step

Start with Step 1:

- add the minimum combat stats to unit data

That is the smallest useful change and gives the rest of the system a stable authored contract to build on.
