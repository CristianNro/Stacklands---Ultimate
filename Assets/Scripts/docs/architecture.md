# Architecture

## High-level model

This project uses a UI-based card system, not world-space physical objects.

Current runtime flow:

1. `CardData` defines a card asset
2. `CardSpawner` instantiates the shared card prefab into the board UI
3. `CardInitializer` delegates initialization to `CardInstance`, which now configures the relevant specialized runtime pieces for that card type
4. the player drags the card or stack through `CardDrag`
5. cards can stay single, form a stack, split from a stack, enter containers, or interact with market slots
6. `RecipeSystem` evaluates stacks and starts or cancels tasks
7. `TaskSystem` advances timed crafting
8. crafting consumes inputs and spawns results
9. resulting board state may trigger a new valid recipe loop

## Main current system areas

- card definitions: `CardData/*`
- runtime card state: `CardInstance`, `UnitRuntime`, `BuildingRuntime`, `ContainerRuntime`, `MarketPackRuntime`, `FoodRuntime`, `CardTransformationRuntime`
- visual card state: `CardView`
- board and bounds: `BoardRoot`
- spawning and positioning: `CardSpawner`
- drag and drop: `CardDrag`
- stack ownership and layout: `CardStack`, `CardStackFactory`, `StackRules`
- recipes: `RecipeData`, `RecipeDatabase`, `RecipeSystem`
- timed tasks: `TaskSystem`, `RecipeTask`
- timed-task time source: `GameTimeService`
- day cycle: `GameTimeService`, `DayCycleCoordinator`, `DailyUpkeepSystem`, `DayEventSystem`, `DayEventExecutor`
- single-card transformations: `CardTransformationRule`, `CardTransformationRuntime`, `CardTransformationSystem`, `CardTransformationExecutor`
- combat: `CombatEncounter`, `CombatParticipantRuntime`, `CombatEncounterSystem`, `CombatEncounterResolver`, `CombatDamageResolver`, `CombatLootDropSystem`, `CombatFormationUtility`, `CombatEncounterVisuals`, `CombatEncounterFeedback`, `CombatFloatingDamagePresenter`
- economy/market: `Market/*`
- container persistence: `ContainerStorageService`

## Current strengths

- The core loop is understandable.
- Gameplay rules are mostly localized by feature area.
- Data-driven recipes and packs already exist.
- Board-space positioning is consistent enough to support stacking and animated spawning.

## Current weaknesses

- There is too much direct coupling between systems.
- Several `MonoBehaviours` mix domain rules and presentation.
- Some recipe and economy flows still carry residual scene-bound runtime coupling, even after the stronger typed recipe and market boundaries added in recent phases.
- Some systems duplicate business logic instead of consuming shared services.
- Runtime state persistence is still partial for future richer card-state systems.

One recent improvement is that stack acceptance rules are now centralized through `StackRules` instead of living only as scattered checks.

Another recent improvement is that `CardInstance` now owns the basic runtime flags and stack membership instead of exposing that state as loose public fields.

The next boundary already started to move too: runtime consumers such as drag and market flows are beginning to query specialized capabilities through `CardInstance` instead of discovering sibling components ad hoc.

The interaction boundary also started to move: `CardDrag` now builds a drop context, resolves a structured `CardDropTargetInfo`, and delegates drop outcome resolution to `CardDropResolver`, while market, container and stack drop branches are already split into dedicated handlers instead of carrying all end-of-drag branching inline.

That interaction layer also now has an explicit ordered handler pipeline, so drop precedence no longer depends only on incidental `if` ordering inside one large method.

That same interaction boundary is now also more tolerant to player overlap intent: stack targeting can fall back to real rect coverage against underlying cards, not only the cursor raycast, and important targets can now expose themselves through explicit `ICardDropTargetSource` contracts before hierarchy lookup fallback is used.

The recipe flow also improved: `RecipeSystem` now bootstraps existing stacks once, subscribes explicitly when `BoardRoot` becomes available, and then reacts to stack lifecycle events instead of scanning all stacks every frame.

World-time also now has a clearer shape: `GameTimeService` acts as the shared clock, while `DayCycleCoordinator`, `DailyUpkeepSystem`, `DayEventSystem` and `DayEventExecutor` keep day progression, food upkeep and day-start world events outside recipes, tasks and board authority.

The economy area also started to improve: shared market value-combination and currency-consumption logic now lives in `MarketEconomyService` instead of being duplicated inside both buy and sell slots.

That boundary moved further too: transaction assembly and consumption now lives in `MarketTransactionService`, so market slots are starting to behave more like coordinators than business-logic owners.

The currency contract also became explicit in the card model: market rules can now rely on `isCurrency`, `CurrencyType` and slot-level currency filters instead of depending on the old `"normal currency"` tag convention.

Containers also became narrower as a system boundary: they now behave only as card storage with typed allow/block filters and controlled release, while future scene travel should move to a dedicated portal system.

Container persistence also started to harden: stored content now uses an explicit card snapshot shape with separate definition data and runtime state instead of a flat ad hoc record.

That migration path is now complete in runtime code too: gameplay tags have been removed from active contracts in favor of explicit typed capabilities and capability-based recipe requirements.

## Core architectural rule

The project must move toward smaller, more specialized systems with clearer ownership.

Do not respond to complexity by creating giant manager classes or deeper inheritance trees.

The preferred direction is:

- thinner shared base models
- stronger runtime contracts
- capability-driven extensions
- clearer separation between gameplay authority and visuals

## Future target shape

Long-term, the project should converge toward these boundaries:

- card definition layer
- runtime state layer
- gameplay domain services
- board/stack authority layer
- interaction resolution layer
- visual presentation layer

See `docs/architecture-roadmap.md` for the staged migration plan.
