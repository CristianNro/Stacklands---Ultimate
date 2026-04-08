# Post Day-Cycle Audit

## Scope

This audit reviews the currently implemented:

- time service
- day cycle orchestration
- daily food upkeep
- day event selection
- day event execution
- day-cycle UI

It also highlights code that is now semantically stale or misleading.

## General conclusion

The new time and event systems are well aligned with the architecture of the project.

The most important things were done in the right direction:

- one shared time authority
- no food logic inside the clock
- no event logic inside the clock
- explicit orchestration layer
- explicit food domain system
- explicit event selection vs execution split
- event execution reuses current spawning and board rules

This means there is no need to reopen an old roadmap phase because of the current day-cycle work.

The system is already growing by extension, not by bypassing the existing architecture.

## What is solid

## Time authority

`GameTimeService` is a good fit as the single shared clock.

It already served timed tasks and now also drives day progression, which avoids a second competing time source.

## Daily flow ownership

`DayCycleCoordinator` keeps the right ownership boundary:

- it owns order
- it does not own domain rules

That keeps the system readable and extensible.

## Food model

Moving food into:

- `FoodResourceCardData`
- `FoodRuntime`

was the correct direction.

The system now supports:

- unit-side food demand
- food-side total value
- partial food consumption
- snapshot preservation of remaining food

## Event model

Changing day events from hardcoded types like portal or merchant to a generic `SpawnCards` event family was the right decision.

This keeps the day cycle generic and lets cards own their own behavior after spawn.

## UI boundary

Both:

- `DayCycleControlsView`
- `DailyUpkeepSummaryView`

remain visual-only and do not become gameplay authority.

That is the correct boundary.

## Documentation changes made during this audit

The following documentation was updated or added so the current implementation is easier to understand:

- `docs/architecture.md`
- `docs/card-system.md`
- `docs/day-cycle-system-plan.md`
- `docs/day-cycle-system.md`

## Code that is semantically stale or worth revisiting

These are not blockers, but they are the most visibly outdated pieces in the current implementation.

1. `GameTimeService.SetSpeedX1()`, `SetSpeedX2()`, `SetSpeedX3()`

These methods are still valid public API.

They are not obsolete in a technical sense.

But the built-in UI no longer depends on separate speed buttons because `DayCycleControlsView` now cycles speed with one button.

So these methods are now auxiliary API rather than the main UI path.

2. `FoodResourceCardData.spoilAfterSeconds`

This field is not obsolete, but it is currently only a forward hook.

It is present in data and validation, but no spoilage system consumes it yet.

That is acceptable and intentional, but it should stay documented as "planned, not active".

## Suggested next technical priorities

If day-cycle development continues, the next most natural steps are now:

1. optional save/load support for non-repeatable day events

2. richer event families only if `SpawnCards` stops being enough

3. tighter scene-wiring safety around `DayCycleCoordinator`
   - for example, auto-discovery or validation of missing / miswired processors

## Final conclusion

The current time and day-event systems are well aligned with the project and do not need a corrective refactor.

What was needed most after this audit was documentation cleanup, not another architectural rewrite.
