# Day Cycle System

## Current scope

The day-cycle layer is now a real gameplay system with four implemented responsibilities:

- global day-aware time through `GameTimeService`
- ordered day-end and day-start orchestration through `DayCycleCoordinator`
- food consumption and starvation through `DailyUpkeepSystem`
- day-start event selection and execution through `DayEventSystem` and `DayEventExecutor`

This system is intentionally separate from:

- recipes
- task scheduling
- market logic
- board authority

Those systems can react to time, but they do not own day progression.

## Main runtime flow

The current day-cycle flow is:

1. `GameTimeService` advances day time from the same scaled time source already used by timed tasks
2. when the day finishes, it emits:
   - `OnDayEnding(dayNumber)`
   - `OnDayEnded(dayNumber)`
   - `OnDayAdvanced(newDayNumber)`
   - `OnDayStarted(newDayNumber)`
3. `DayCycleCoordinator` receives the events and runs ordered processors
4. `DailyUpkeepSystem` consumes food at day end
5. `DayEventSystem` selects matching events at day start
6. `DayEventExecutor` turns selected `SpawnCards` events into real card spawns

## Core systems

## `GameTimeService`

Current responsibilities:

- pause / resume shared timed systems
- provide scaled delta time for `TaskSystem`
- track `CurrentDay`
- track `CurrentDayElapsed`
- expose `CurrentDayProgress01`
- cycle speed through `x1`, `x2`, `x3`

Important rule:

`GameTimeService` is still only the clock.

It does not:

- consume food
- decide events
- spawn cards

## `DayCycleCoordinator`

Current responsibilities:

- listen to `GameTimeService`
- keep ordered arrays of:
  - `IDayEndProcessor`
  - `IDayStartProcessor`
- execute day-end processors on `OnDayEnding`
- execute day-start processors on `OnDayStarted`

Important rule:

The coordinator owns the order, not the domain rules.

## `DailyUpkeepSystem`

Current responsibilities:

- collect active units from `BoardRoot.ActiveCards`
- collect valid food cards from `BoardRoot.ActiveCards`
- resolve feeding in deterministic order
- kill units that are not fully fed
- emit a structured `DailyUpkeepResult`

Current feeding rules:

- only `FoodResourceCardData` counts as valid food
- every `UnitCardData` defines `dailyFoodConsumption`
- every `FoodResourceCardData` defines total `foodValue`
- `FoodRuntime` stores the current `remainingFoodValue`
- food is consumed partially
- units are not fed partially:
  - a unit survives only if its full daily requirement is covered
  - otherwise it dies at day end

Current deterministic order:

- units: top-to-bottom, then left-to-right
- food: top-to-bottom, then left-to-right

## `DayEventSystem`

Current responsibilities:

- evaluate `DayEventDefinition` assets for the new day
- select all deterministic valid events:
  - `ExactDay`
  - `DayRange`
- select at most one weighted random event:
  - `RandomFromMinDay`
- remember non-repeatable events by `id`

Important rule:

It selects events only.

It does not execute them.

## `DayEventExecutor`

Current responsibilities:

- listen to `DayEventSystem.OnDayEventsSelected`
- execute `SpawnCards`
- reuse `CardSpawner`
- reuse current board positioning and free-space search

Current first event family:

- `SpawnCards`

Each `DayEventDefinition` can now spawn a list of `DayEventSpawnEntry`:

- `CardData card`
- `int count`

This means events can stay generic:

- a portal event is just a card spawn
- a merchant event is just a card spawn
- future single-card timed behavior can remain inside the card/runtime itself

## Card-data integration

## Food

Food now has explicit subtype support:

- `FoodResourceCardData`

Current food-specific fields:

- `foodValue`
- `spoilAfterSeconds`

Current runtime support:

- `FoodRuntime`
  - `remainingFoodValue`

Food state is also preserved through storage snapshots.

## Units

Units now expose:

- `dailyFoodConsumption`

This is the unit-side demand input for daily upkeep.

## UI layer

## `DayCycleControlsView`

Current UI behavior:

- shows current day
- shows day-progress bar
- has one `Pause / Play` button
- has one speed-cycle button:
  - `x1 -> x2 -> x3 -> x1`

If references are not assigned, it can create a lightweight runtime panel automatically.

## `DailyUpkeepSummaryView`

Current UI behavior:

- listens to `DailyUpkeepSystem`
- shows:
  - day number
  - consumed food
  - fed units
  - deaths by starvation
- auto-hides after a short duration

It can also build a fallback runtime panel automatically.

## Scene wiring

Minimum required setup for the current day-cycle system:

- `GameTimeService`
- `DayCycleCoordinator`
- `DailyUpkeepSystem`
- `DayEventSystem`
- `DayEventExecutor`
- `CardSpawner`
- `BoardRoot`

Required inspector wiring:

- `DayCycleCoordinator.dayEndProcessors`
  - should include `DailyUpkeepSystem`
- `DayCycleCoordinator.dayStartProcessors`
  - should include `DayEventSystem`
- `DayEventSystem.eventDefinitions`
  - should include authored `DayEventDefinition` assets
- `DayEventExecutor.dayEventSystem`
  - can be assigned manually or auto-resolved
- `DayEventExecutor.spawnAnchor`
  - optional anchor for visual spawn origin

Important note:

Events are selected when a new day starts.

That means an `ExactDay = 1` event does not automatically fire just because the scene started on day 1. It will fire only if the system explicitly processes that day start.

For straightforward runtime testing, `ExactDay = 2` is usually clearer.

## Current strengths

- shared time authority now exists
- daily flow is event-driven instead of frame-polled
- food consumption is explicit and data-driven
- starvation is deterministic and visible
- event selection is separated from event execution
- day-start events already reuse the existing spawn/board architecture

## Current known limitations

- there is still no food spoilage runtime yet
- there are no event families beyond `SpawnCards`
- there is no save/load persistence yet for non-repeatable day events
- scene wiring still depends on inspector setup

## Current extension points

- add more `IDayEndProcessor` implementations for future world upkeep
- add more `IDayStartProcessor` implementations for other start-of-day rules
- extend `DayEventType` only when a new family cannot be modeled as spawned cards
- attach future single-card timed behavior to the spawned card itself, not to the day-event system
