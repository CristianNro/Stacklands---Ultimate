# Safe Extension Points

## Purpose

This file describes where new work should plug into the current codebase without making architectural debt worse.

These are not absolute guarantees. They are the safest areas relative to the current implementation.

## Preferred extension areas

### Cards

Safer extensions:

- new data fields that are consumed by a real system
- stricter validation around card identity, typed classifications and capabilities
- runtime state additions with clear ownership
- visual-only card presentation improvements

Avoid:

- adding many new base flags with no owning behavior
- pushing more unrelated behavior directly into `CardInstance`

### Stacks

Safer extensions:

- better validation of add/remove/split/merge flows
- stack query helpers that are truly stack-specific
- extraction of visual responsibilities out of `CardStack`

Avoid:

- introducing new gameplay subsystems directly into `CardStack`

### Recipes

Safer extensions:

- new recipe assets
- tighter validation around recipe data
- improved specificity rules
- stronger matching helpers
- extending capability-driven recipe requirements without reintroducing weak string contracts

Avoid:

- per-card hardcoded recipe exceptions when the recipe model can express the behavior

### Crafting

Safer extensions:

- task validation improvements
- clearer cancel/restart rules
- better separation between task logic and visuals

Avoid:

- bypassing the task system for timed recipe execution

### Day cycle

Safer extensions:

- new `IDayEndProcessor` implementations for explicit end-of-day rules
- new `IDayStartProcessor` implementations for start-of-day world logic
- richer `DayEventDefinition` data when it still keeps selection and execution separated
- UI additions that read from `GameTimeService` without becoming time authority

Avoid:

- putting food, event or world-spawn logic directly into `GameTimeService`
- bypassing `DayCycleCoordinator` when adding ordered day-start or day-end behavior

### Single-card transformations

Safer extensions:

- new `CardTransformationRule` assets
- new capability-driven pause or speed rules
- executor improvements that still reuse `CardSpawner` and board-safe placement
- additional visuals that read from `CardTransformationRuntime`

Avoid:

- re-implementing this mechanic as stack recipes
- bypassing `CardTransformationSystem` with ad hoc per-card timers
- moving transformation authority into `CardView`

### Board/layout

Safer extensions:

- stronger bounds validation
- centralized free-space queries
- stack-aware placement improvements

Avoid:

- creating new side systems that decide placement independently

### Economy

Safer extensions:

- extracting shared payment/change logic into reusable services
- better currency typing and validation
- consolidating exact-combination and card-consumption rules in a shared market service
- moving purchase/sell orchestration into market-domain coordinators instead of slot scripts
- routing market valuation through an explicit pricing service before introducing richer pricing rules
- routing reward/change delivery through a dedicated delivery service instead of slot scripts

Avoid:

- duplicating exact-combination logic across multiple market scripts

### Containers

Safer extensions:

- stronger storage validation
- better runtime snapshot structure
- clearer card-type filtering and storage limits
- optional subtype-specific filters when the base `CardType` contract is not precise enough
- better release rules for partial content output
- snapshot fields that correspond to real runtime state owners

Avoid:

- turning containers back into scene-travel systems
- storing more gameplay-critical state in ad hoc side fields without a snapshot model

## Unsafe extension pattern

Do not create new parallel systems that duplicate the responsibility of:

- stack ownership
- recipe evaluation
- crafting orchestration
- spawn positioning
- economy calculation
- board placement authority

## Roadmap alignment

If a feature touches core architecture, check `docs/architecture-roadmap.md` first and prefer changes that move the project toward the roadmap instead of sideways.
