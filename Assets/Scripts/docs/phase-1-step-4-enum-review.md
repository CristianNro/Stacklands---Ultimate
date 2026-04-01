# Phase 1 Step 4: Enum Review

## Purpose

This document reviews the current enums in `CardEnums.cs` against the real implementation and proposes a healthier enum strategy for the future.

The goal is not to fill the project with more enums by default.

The goal is to use enums where they strengthen real contracts and reduce ambiguity.

## Review standard

Each enum is evaluated by:

1. current usage in code
2. current architectural honesty
3. future usefulness
4. whether it should remain active, become metadata-only, or leave the active model

## Current enums

## Enums that are actively part of current gameplay contracts

These are already real and should stay active:

### `RecipeIngredientConsumeMode`

Status:

- active

Consumers:

- `RecipeData`
- `CardStack`
- `RecipeIngredientRule`

Decision:

- `keep-active`

### `RecipeMatchMode`

Status:

- active

Consumers:

- `RecipeData`
- `CardStack`

Decision:

- `keep-active`

### `RecipeExecutionMode`

Status:

- active

Consumers:

- `RecipeData`
- `TaskSystem`

Decision:

- `keep-active`

### `ContainerOpenMode`

Status:

- active

Consumers:

- `ContainerRuntime`

Decision:

- `keep-active`

### `ContainerListMode`

Status:

- active

Consumers:

- `ContainerRuntime`

Decision:

- `keep-active`

## Enums that currently behave more like metadata than active rules

These are conceptually valid, but the current codebase does not depend on them meaningfully.

### `CardType`

Status:

- not consumed by runtime behavior

Decision:

- `keep-as-metadata`

Reason:

It may still be useful for editor organization, asset browsing or future UI filtering, but it should not be treated as an active gameplay contract yet.

### `Rarity`

Decision:

- `keep-as-metadata`

Reason:

Same situation as `CardType`.

### `UnitRole`

Decision:

- `keep-as-metadata`

Reason:

A valid identity layer for future systems, but not an active gameplay driver right now.

### `FactionType`

Decision:

- `keep-as-metadata`

Reason:

No faction logic currently exists, but it may remain as lightweight future identity metadata.

### `ResourceType`

Decision:

- `keep-as-metadata`

Reason:

Potentially useful classification, but not active today.

### `ItemType`

Decision:

- `keep-as-metadata`

Reason:

Potentially useful classification, but not active today.

## Enums that should leave the active model

These currently represent speculative architecture more than real game structure.

### `DamageType`

Decision:

- `remove-from-active-model`

Reason:

No combat system currently consumes it.

### `BuildingType`

Decision:

- `remove-from-active-model`

Reason:

No building behavior or UI currently depends on it.

### `TaskType`

Decision:

- `remove-from-active-model`

Reason:

The project does not use a generalized task framework yet.

### `CardState`

Decision:

- `remove-from-active-model`

Reason:

The runtime currently uses individual booleans instead of this enum.

Keeping both models at once only increases confusion.

### `ConstructionState`

Decision:

- `remove-from-active-model`

Reason:

No construction system currently uses it.

### `CombatState`

Decision:

- `remove-from-active-model`

Reason:

No combat runtime currently uses it.

## Main conclusion about current enums

The problem is not that the project has enums.

The problem is that some enums are pretending to represent active architecture when they are actually only future design placeholders.

That creates false confidence in the model.

## Proposed future enum additions

These are not for immediate blind implementation.

They are proposed because they would strengthen real contracts the roadmap already points toward.

## 1. `CardMobilityMode`

Suggested values:

- `Movable`
- `Locked`

Why it may help:

Today the project wants `isMovable` to become a real rule.

A bool is enough if the rule is simple forever.

But if the project later needs:

- completely immovable cards
- cards movable only by systems, not by player input
- temporary lock states

then an enum would scale better than a bool.

Recommendation:

- do not replace `isMovable` immediately
- keep this enum as a future option if mobility rules become richer

## 2. `StackingMode`

Suggested values:

- `Allowed`
- `Forbidden`
- `RootOnly`

Why it may help:

You already decided that `stackable` must become a real contract.

A bool is enough only if the rule remains binary.

If future gameplay needs special stack behavior, a stacking enum would be cleaner than proliferating booleans.

Recommendation:

- keep `stackable` for now
- consider this enum only if stacking rules gain more than two states

## 3. `CurrencyType`

Suggested values:

- `None`
- `Normal`
- `Premium`
- `Special`

Why it may help:

Current market logic relies on string tags such as `"normal currency"`.

That is a weak contract for a system that already has meaningful economy behavior.

This enum would be a strong candidate for future architecture because it replaces a brittle string-based business rule.

Recommendation:

- high-value future candidate

## 4. `ContainerBehaviorType`

Suggested values:

- `NormalStorage`
- `CurrencyStorage`
- `SceneContainer`

Why it may help:

Today container special behavior depends partly on tags and partly on `openMode`.

If containers continue growing, a behavior enum would make ownership clearer than hidden tag conventions.

Recommendation:

- medium-value future candidate

## 5. `CardValueMode`

Suggested values:

- `Static`
- `RuntimeOverride`
- `DerivedFromContents`

Why it may help:

Today `CardInstance` handles runtime value overrides ad hoc, and containers can behave like derived-value cards.

If value logic keeps growing, this contract may become useful.

Recommendation:

- future candidate only if economy/value logic expands

## 6. `StackCapacityMode`

Suggested values:

- `Unlimited`
- `MaxWeight`
- `MaxCount`
- `MaxWeightAndCount`

Why it may help:

You already decided that `weight` should drive future stack capacity.

If stack rules become configurable by card or stack root, this enum would give the system a strong and explicit way to define how capacity is enforced.

Recommendation:

- high-value future candidate

## 7. `RecipePriorityMode`

Suggested values:

- `SpecificityOnly`
- `SpecificityThenManualPriority`

Why it may help:

Current recipe selection uses specificity scoring only.

If content grows, a clearer explicit priority contract may become necessary.

Recommendation:

- future candidate if recipe conflicts become more complex

## Proposed priority of new enum candidates

### High-value candidates

- `CurrencyType`
- `StackCapacityMode`

Reason:

Both replace weak future contracts that are currently handled by strings or implicit rules.

### Medium-value candidates

- `ContainerBehaviorType`
- `RecipePriorityMode`

Reason:

Useful if those systems keep growing beyond their current scope.

### Optional candidates

- `CardMobilityMode`
- `StackingMode`
- `CardValueMode`

Reason:

These are only justified if current bool-based rules become multi-state systems.

## Recommendation for next phases

Do not add all proposed enums immediately.

Only add a new enum when:

1. it replaces an already fragile contract
2. a real system will consume it soon
3. it clarifies ownership better than tags or booleans

## Practical next move

For the near future, the most promising enum additions are:

1. `CurrencyType`
2. `StackCapacityMode`

They align directly with already-decided future work:

- reducing string-based economy rules
- introducing weight-based stack limits
