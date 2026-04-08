# Day Cycle System Plan

## Purpose

This document defines the recommended architecture for the game-wide day cycle.

The target is not only to count time.

The target is to introduce a shared world-time layer that can safely drive:

- day progression
- pause and time-speed controls
- daily food consumption
- day-start and day-end hooks
- scripted or random day events

The system should extend the current architecture, not fight it.

That means:

- use the existing `GameTimeService` as the base time authority
- keep daily logic out of recipe and task-specific systems
- separate time measurement from gameplay consequences

## Current implementation status

This plan is no longer only aspirational.

The following parts are already implemented:

- `GameTimeService` as shared day-aware clock
- `DayCycleCoordinator`
- `DailyUpkeepSystem`
- `DayEventDefinition`
- `DayEventSystem`
- `DayEventExecutor`
- `DayCycleControlsView`
- `DailyUpkeepSummaryView`

The most important remaining items are now future expansion work, not the initial architecture cut.

## Design goals

1. Keep one authoritative game-time source.
2. Make day progression deterministic and event-driven.
3. Keep daily consequences outside the clock itself.
4. Make random or scripted day events data-driven.
5. Let the current task/crafting systems reuse the same time source.

## Why this should not live inside recipes or tasks

The day cycle is a world rule, not a recipe rule.

It does not belong in:

- `RecipeSystem`
- `TaskSystem`
- `Market`
- `BoardRoot`

Those systems should react to day progression when needed, but they should not own it.

The best architectural fit is:

- time authority in `GameTimeService`
- cycle orchestration in a dedicated day coordinator
- domain-specific systems reacting to day events

## Recommended architecture

## 1. `GameTimeService` becomes the shared world clock

Current role:

- wrapper over `Time.deltaTime`
- supports pause and global time scale for timed systems

Recommended new responsibilities:

- track current day number
- track elapsed time inside the current day
- expose normalized day progress
- expose time-speed controls (`x1`, `x2`, `x3`)
- emit day lifecycle events

Recommended data:

- `dayDurationSeconds`
- `currentDay`
- `currentDayElapsed`
- `pauseTimedSystems`
- `timedSystemsTimeScale`

Recommended events:

- `OnDayStarted(int dayNumber)`
- `OnDayEnding(int dayNumber)`
- `OnDayEnded(int dayNumber)`
- `OnDayAdvanced(int newDayNumber)`
- optional: `OnTimeSpeedChanged(float newScale)`

Important rule:

`GameTimeService` should only be responsible for measuring and advancing time.

It should not directly:

- consume food
- spawn events
- create merchants
- create portals

That logic belongs elsewhere.

## 2. `DayCycleCoordinator` owns the daily flow

This should be the main orchestration layer for daily transitions.

Responsibilities:

- subscribe to `GameTimeService` day events
- define what happens when a day ends
- define what happens before a new day starts
- coordinate other domain systems in a clean order

This system should not implement all domain rules itself.

It should coordinate them.

Recommended order:

1. receive day-ending notification
2. run daily upkeep
3. finalize previous day
4. advance to the new day
5. resolve day-start events
6. execute chosen events

This lets the day cycle stay readable and deterministic.

## 3. `DailyUpkeepSystem` handles end-of-day food consumption

This should be a dedicated system for recurring world upkeep.

First target:

- consume food based on the number of active units

Responsibilities:

- count how many units require food
- find food resources available for consumption
- consume food cards according to game rules
- report shortages

This system should return a result object rather than only doing side effects.

Recommended result shape:

- `dayNumber`
- `requiredFood`
- `consumedFood`
- `missingFood`
- `consumedCards`
- optional future fields:
  - `affectedUnits`
  - `starvationTriggered`

This result is useful for:

- debugging
- UI summaries
- future consequence systems

Important rule:

Do not hide starvation or shortage rules inside `GameTimeService`.

Keep them in `DailyUpkeepSystem`.

## 4. `DayEventSystem` selects events for the next day

This should choose which world events happen when a new day begins.

Supported event categories:

- specific day events
- ranged day events
- random weighted events

Examples:

- spawn a portal on day 5
- spawn a merchant starting from day 3 with random chance
- trigger a threat after a certain range

Responsibilities:

- inspect the new day number
- filter valid event definitions
- choose random events where applicable
- return a structured list of events to execute

This system should select events.

It should not execute them directly.

## 5. `DayEventExecutor` applies the selected events

This should be the execution boundary for day events.

Responsibilities:

- spawn world objects such as portals or merchants
- apply day-start side effects
- keep event execution out of the selector

This gives a clean split:

- `DayEventSystem` decides what happens
- `DayEventExecutor` makes it happen

That separation will matter if later you want:

- previews
- event logs
- UI explanations
- save/load compatibility

## Recommended data model

## `DayEventDefinition`

Use a `ScriptableObject` for event authoring.

Suggested fields:

- `id`
- `displayName`
- `eventTriggerMode`
- `exactDay`
- `minDay`
- `maxDay`
- `weight`
- `repeatable`
- `eventType`
- `eventPayload`

Current first-pass implementation:

- `DayEventDefinition` already covers identity, trigger mode, exact/range day fields, weight, repeatability and `DayEventType`
- `SpawnCards` already uses a list of `DayEventSpawnEntry` (`CardData` + `count`)
- payload-specific data beyond spawns is intentionally postponed until a second event family really exists

Possible trigger modes:

- `ExactDay`
- `DayRange`
- `RandomFromMinDay`

Possible event types:

- `SpawnCards`

Payload can stay simple at first and be specialized later.

## `DayCycleResult` or daily summary objects

Useful optional result types:

- `DailyUpkeepResult`
- `DayEventSelectionResult`
- `DayCycleSummary`

These help keep flow explicit instead of relying on hidden side effects.

## Integration with existing systems

## `GameTimeService`

This is the right base.

Reason:

- `TaskSystem` already depends on it
- pause and time-scale controls already live there
- the project already has a timed-systems concept

This means the day cycle can reuse the same world-time foundation instead of inventing another one.

## `TaskSystem`

Should remain a consumer of scaled time.

It should not own day progression.

It may later react to:

- pause
- global time speed

But the day cycle should still remain above it as world authority.

## `BoardRoot`

Should not own the day cycle.

It may be queried by systems that need active entities, but it should stay focused on board-space authority.

## Containers and market

These should remain listeners or consumers of day events only if a future mechanic requires it.

They should not be part of the first implementation pass.

## Recommended implementation phases

## Step 1. Extend `GameTimeService` into a day-aware clock

Goal:

Give the project a real world-time source with day counting.

Implement:

- current day
- day duration
- elapsed day time
- normalized progress
- pause and time speed controls
- day lifecycle events

Keep it simple:

- no food
- no merchants
- no portals

Only timing and day transitions.

## Step 2. Add `DayCycleCoordinator`

Goal:

Create one explicit place that owns the order of day-end and day-start processing.

Implement:

- subscription to `GameTimeService`
- end-of-day orchestration
- start-of-day orchestration

Keep this focused on order and delegation.

Do not turn it into a giant god object.

Current implementation direction:

- `DayCycleCoordinator` should subscribe to the clock and expose ordered extension points
- day-end processors should implement `IDayEndProcessor`
- day-start processors should implement `IDayStartProcessor`
- the coordinator should keep ownership of the order, not of the domain rules themselves

## Step 3. Add `DailyUpkeepSystem`

Goal:

Implement the end-of-day food consumption rule.

Current implemented direction:

- each `UnitCardData` contributes `dailyFoodConsumption`
- only `FoodResourceCardData` counts as valid food
- food uses `FoodRuntime.remainingFoodValue`
- food is consumed partially
- units are not fed partially
- if a unit is not fully fed, it dies at day end
- `DailyUpkeepResult` reports consumed food, shortages, fed units and deaths

## Step 4. Add `DayEventDefinition` plus `DayEventSystem`

Goal:

Support specific-day and random-day events in a data-driven way.

Implement:

- event definitions
- filtering by day
- weighted random selection where needed

Keep event selection pure where possible.

Current first-pass implementation direction:

- deterministic events (`ExactDay`, `DayRange`) can all fire together
- random events (`RandomFromMinDay`) should resolve as at most one weighted pick per day
- non-repeatable events should be remembered by stable event id

## Step 5. Add `DayEventExecutor`

Goal:

Separate event execution from event selection.

Implement the first concrete event family:

- spawn cards

This keeps future event growth from leaking into the selector.

Current first-pass implementation direction:

- `DayEventExecutor` should listen to `DayEventSystem.OnDayEventsSelected`
- `SpawnCards` should reuse `CardSpawner`, not invent a parallel spawn path
- event cards should appear from a configurable anchor or fallback board area
- final placement should still go through current board occupancy and clamping rules

## Step 6. Add optional UI hooks

Goal:

Expose the day cycle to the player.

Examples:

- current day label
- progress bar
- pause button
- `x1`
- `x2`
- `x3`

This should come after the domain flow is stable.

Current lightweight implementation direction:

- `DailyUpkeepSummaryView` can listen to `DailyUpkeepSystem`
- it should remain visual-only and never become authority
- a runtime-generated panel is acceptable as a first validation layer before a bespoke UI is built
- `DayCycleControlsView` can expose the current day, the day-progress bar, one `Pause / Play` button and one speed-cycle button while delegating all authority to `GameTimeService`

## Recommended rules

- one authoritative game-time source
- one authoritative day counter
- daily consequences triggered by events, not polling
- daily food consumption handled by a dedicated domain system
- event selection and event execution remain separate
- no hidden economic, recipe or board side effects inside the clock itself

## Complexity estimate

Overall complexity: medium-high

Breakdown:

- day-aware clock: medium-low
- daily upkeep: medium
- event system: medium
- keeping ownership clean across systems: medium-high

The complexity is not in the timer itself.

The complexity is in keeping the system clean and extensible while integrating with the current architecture.

## Recommended final direction

Use this split:

- `GameTimeService` -> time authority
- `DayCycleCoordinator` -> orchestration
- `DailyUpkeepSystem` -> food consumption
- `DayEventSystem` -> event selection
- `DayEventExecutor` -> event execution

This fits the current project direction well and avoids mixing world-time rules into recipes, tasks, market or board systems.
