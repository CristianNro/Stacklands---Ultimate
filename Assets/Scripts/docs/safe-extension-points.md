# Safe Extension Points

## Purpose

This file describes where new work should plug into the current codebase without making architectural debt worse.

These are not absolute guarantees. They are the safest areas relative to the current implementation.

## Preferred extension areas

### Cards

Safer extensions:

- new data fields that are consumed by a real system
- stricter validation around card identity or tags
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

Avoid:

- per-card hardcoded recipe exceptions when the recipe model can express the behavior

### Crafting

Safer extensions:

- task validation improvements
- clearer cancel/restart rules
- better separation between task logic and visuals

Avoid:

- bypassing the task system for timed recipe execution

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
