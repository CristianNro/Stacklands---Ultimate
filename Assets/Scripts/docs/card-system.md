# Card System

## Current implementation

Cards are currently defined by `CardData` assets and instantiated as a shared prefab with runtime components.

Relevant classes:

- `CardData`
- `CombatantCardData`
- `SurvivorUnitCardData`
- `ResourceCardData`
- `FoodResourceCardData`
- `ItemCardData`
- `PackCardData`
- `UnitCardData`
- `EnemyCardData`
- `BuildingCardData`
- `ContainerCardData`
- `CardInstance`
- `CardInitializer`
- `CardView`
- `CardTransformationRule`

## What is already working

The current card system already separates static definition from spawned runtime instance at a basic level:

- static data lives in `ScriptableObject` assets
- mutable runtime data now starts from `CardInstance`, which also owns basic flags, stack membership and specialized runtime references
- the same visual prefab can represent multiple card definitions

## Current problems

### 1. `CardData` is too broad

`CardData` still contains a mix of:

- true core identity
- UI-facing metadata
- behavior flags
- balance values
- some transitional gameplay configuration that still needs stronger typed ownership

Phase 1 already reduced a large part of this problem, but the broader runtime architecture is still transitional.

### 2. runtime state is fragmented

`CardInstance` is not the only runtime state owner.

State is spread across:

- `CardInstance`
- `UnitRuntime`
- `BuildingRuntime`
- `ContainerRuntime`
- `MarketPackRuntime`
- `FoodRuntime`
- `CombatParticipantRuntime`
- `CardTransformationRuntime`

This is still workable, but it is not yet a clean runtime model.

One recent improvement is that `CardInstance` now centralizes:

- basic runtime flags
- current stack ownership
- cached references to specialized runtime components
- runtime activation for supported subtype-specific components
- helper access to active specialized runtimes so external systems depend less on sibling `GetComponent` lookups

`CardView` also now exposes its cached `CardInstance`, so hot paths such as stack and market flows can reuse the same runtime reference instead of resolving it repeatedly.

`ContainerRuntime` now also exposes its owning `CardInstance`, which helps avoid reverse runtime lookups in container-driven economy flows.

The same pattern now starts to apply to combat too: units can expose a dedicated `CombatParticipantRuntime` instead of forcing future combat state into `CardInstance` or `UnitRuntime`.

### 3. initialization is switch-based

`CardInitializer` is now thinner, but the transition is not complete yet.

Today it mainly delegates initialization to `CardInstance`, which already configures the specialized runtimes. The remaining issue is architectural dependency, not raw line count.

This is acceptable for the current scope, but it will become brittle if many more behaviors are added.

## Current rules

- never store mutable gameplay state in shared `ScriptableObject` assets
- runtime identity belongs to the spawned instance, not the asset
- visual refresh should reflect runtime state, not become the authority
- a card definition should not promise behavior that no system actually implements
- `stackable`, `isMovable` and `weight` are active interaction contracts and must stay synchronized with drag and stack rules
- `CardInstance` should be the first owner consulted for basic runtime state before reaching for sibling components directly
- market, drag and stack flows should prefer `CardInstance` helpers when they need runtime capabilities like container behavior or current stack ownership
- economic identity should use explicit card fields such as `isCurrency` and `CurrencyType`
- `ContainerCardData` should define storage rules only; travel between scenarios belongs to a future portal system, not to containers
- containers can also add an optional second validation layer for resources, filtering `ResourceType` after the broader `CardType` rule passes
- cards with limited uses should be treated as "used goods" for market selling once `usesRemaining` drops below `maxUses`
- `CardData` can expose both `cardImage` and `cardIcon`, so card presentation can be modularized without overloading a single visual field
- `CardView` should treat `cardImage` as the background art layer and `cardIcon` as the foreground icon layer when both are present
- gameplay identity should move toward typed classifications plus explicit capabilities, not free-form string tags
- `CardData.capabilities` is now the new typed gameplay layer for permissions and affordances
- `CardInstance.HasCapability(...)` should be preferred for new gameplay-facing runtime checks
- edible resources can now use `FoodResourceCardData` to expose explicit food-specific properties such as `foodValue`, instead of overloading generic resource data
- edible resources now use `FoodResourceCardData.foodValue` as their total available food amount, and `FoodRuntime` keeps the remaining runtime value after partial daily consumption
- `FoodResourceCardData.spoilAfterSeconds` is currently only a prepared future hook for spoilage, not an active gameplay rule yet
- `CombatantCardData` is now the shared authored combat base for both survivors and enemies
- `SurvivorUnitCardData` now owns survivor-only needs such as hunger and daily food consumption
- `UnitCardData` is now the concrete survivor asset class kept for compatibility with existing authored units
- `UnitCardData.dailyFoodConsumption` is now the active survivor-side daily food demand used by the day-cycle upkeep system
- `CombatantCardData` now carries the first authored combat inputs for V1 through `attackDamage` and `attackInterval`
- `CombatantCardData` now also carries V2 combat inputs through:
  - `basePhysicalArmor`
  - `baseMagicalArmor`
  - `attackDefenseChannel`
  - `attackDamageTypes`
  - `receivedDamageModifiers`
- `CombatantCardData` now also carries the V3 formation field `combatLineRole`
- `EnemyCardData` now extends `CombatantCardData` with guaranteed and random drop tables
- `CardType.Enemy` now exists as an explicit classification for enemies
- units can now also own mutable combat state through `CombatParticipantRuntime`
- `CombatParticipantRuntime` now reads authored combat stats from `CombatantCardData`, not only survivor unit data
- unit combat health authority now lives in `CombatParticipantRuntime`; `UnitRuntime` no longer mirrors a second mutable `currentHealth`
- `UnitRuntime` is now survivor-only and no longer initializes for enemy cards
- combat itself is now modeled around a dedicated `CombatEncounter` aggregate instead of overloading `CardStack`
- active encounters can now advance participant attack timers through `CombatEncounterSystem`
- combat damage math can now be delegated to `CombatDamageResolver`
- enemy death rewards can now be produced by `CombatLootDropSystem`
- combat presentation can now add floating damage numbers through `CombatFloatingDamagePresenter`
- combat formations can now derive frontlines and targetable lines through `CombatFormationUtility`
- `CardData.transformationRule` is now the explicit data hook for timed single-card evolution
- `CardTransformationRuntime` now owns per-card transformation progress and the transformation scheduler advances active cards on the board
- transformation rules can now require context capabilities from the current stack before they progress
- `CardView` can now show a transformation progress bar for cards whose `CardTransformationRuntime` is active, but the rule can opt out through `CardTransformationRule.showProgressBar`

## Future direction

The card model should evolve toward:

- a thinner and more honest base `CardData`
- fewer decorative or unused fields
- clearer distinction between definition data and runtime state
- stronger capability-driven behavior instead of relying only on inheritance depth
- stronger stack and interaction contracts built from the preserved base fields
- no gameplay dependency on legacy string tags
- structured metadata should remain honest about whether it is gameplay-driving or authoring-only

## Practical guidance

When extending cards today:

- prefer adding data that is consumed by a real system
- avoid adding flags "for later" unless their ownership is already clear
- if a new mechanic needs runtime state, decide first where that state truly belongs
