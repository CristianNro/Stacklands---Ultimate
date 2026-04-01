# Card System

## Current implementation

Cards are currently defined by `CardData` assets and instantiated as a shared prefab with runtime components.

Relevant classes:

- `CardData`
- `ResourceCardData`
- `ItemCardData`
- `PackCardData`
- `UnitCardData`
- `BuildingCardData`
- `ContainerCardData`
- `CardInstance`
- `CardInitializer`
- `CardView`

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
- tag-driven configuration

Phase 1 already started reducing this problem, but the broader runtime architecture is still transitional.

### 2. runtime state is fragmented

`CardInstance` is not the only runtime state owner.

State is spread across:

- `CardInstance`
- `UnitRuntime`
- `BuildingRuntime`
- `ContainerRuntime`
- `MarketPackRuntime`

This is still workable, but it is not yet a clean runtime model.

One recent improvement is that `CardInstance` now centralizes:

- basic runtime flags
- current stack ownership
- cached references to specialized runtime components
- runtime activation for supported subtype-specific components
- helper access to active specialized runtimes so external systems depend less on sibling `GetComponent` lookups

`CardView` also now exposes its cached `CardInstance`, so hot paths such as stack and market flows can reuse the same runtime reference instead of resolving it repeatedly.

`ContainerRuntime` now also exposes its owning `CardInstance`, which helps avoid reverse runtime lookups in container-driven economy flows.

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
- economic identity should use explicit card fields such as `isCurrency` and `CurrencyType`, not only tags
- `ContainerCardData` should define storage rules only; travel between scenarios belongs to a future portal system, not to containers
- containers can also add an optional second validation layer for resources, filtering `ResourceType` after the broader `CardType` rule passes
- cards with limited uses should be treated as "used goods" for market selling once `usesRemaining` drops below `maxUses`

## Future direction

The card model should evolve toward:

- a thinner and more honest base `CardData`
- fewer decorative or unused fields
- clearer distinction between definition data and runtime state
- stronger capability-driven behavior instead of relying only on inheritance depth
- stronger stack and interaction contracts built from the preserved base fields

## Practical guidance

When extending cards today:

- prefer adding data that is consumed by a real system
- avoid adding flags "for later" unless their ownership is already clear
- if a new mechanic needs runtime state, decide first where that state truly belongs
