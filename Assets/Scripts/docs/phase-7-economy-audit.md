# Phase 7 Economy Audit

## Purpose

This audit re-evaluates the economy and market architecture after the currency, container and interaction refactors.

The goal is to identify what part of phase 7 is already solved, what still keeps the market too coupled to UI slots, and what next step gives the best architectural payoff.

## Files reviewed

- `Market/MarketEconomyService.cs`
- `Market/MarketTransactionService.cs`
- `Market/MarketPackPurchaseSlot.cs`
- `Market/MarketSellSlot.cs`
- `docs/safe-extension-points.md`

## Current strengths

### 1. Currency is no longer a weak tag convention

The market already uses:

- `isCurrency`
- `CurrencyType`
- `CurrencyFilterMode`

That is a major improvement over the old `"normal currency"` string contract.

### 2. Shared economy helpers already exist

`MarketEconomyService` now centralizes:

- accepted-currency checks
- exact value-combination building
- physical card destruction for sold/paid units

That removed some of the duplication that used to live inside both market slots.

### 3. Transaction assembly is partially separated from slot UI

`MarketTransactionService` now owns:

- payment context construction
- payment unit selection
- payment consumption
- sellable unit construction

That is a real step toward domain services instead of pure slot-owned business logic.

### 4. Containers and currency now cooperate through stronger contracts

Container-backed payment already works through:

- explicit currency typing
- stored snapshots
- `ContainerRuntime` ownership

This is a stronger economic boundary than before.

## Remaining weaknesses

### 1. Market slots still coordinate too much domain flow

Severity: high

`MarketPackPurchaseSlot` and `MarketSellSlot` still decide:

- when a transaction is valid
- when to consume value
- how to spawn rewards or purchased content
- where results appear
- how change should be returned

Consequence:

- UI slot components still own too much gameplay coordination
- future vendors or pricing modes will keep landing in slot scripts

### 2. Change/reward spawning is still market-slot specific

Severity: medium-high

Both slots still contain reward-delivery policy:

- `MarketPackPurchaseSlot` decides whether change returns to a container or to the board
- `MarketSellSlot` decides reward spawn layout and reward composition flow

Consequence:

- economy service and delivery policy are not yet clearly separated

### 3. Valuation still depends almost entirely on direct card value

Severity: medium

The current value contract is much stronger than before, but it still relies mostly on:

- `CardData.value`
- runtime value overrides

There is not yet a stronger valuation boundary for:

- vendor-specific pricing
- buy/sell spreads
- dynamic modifiers
- future world-time price changes

Consequence:

- good enough for current gameplay
- not yet ideal for richer economy systems

Status update:

`MarketPricingService` now exists as an explicit market-valuation boundary, but at this stage it still resolves to the same effective value the project already used before.

### 4. Exact value-combination logic is centralized, but still market-facing

Severity: medium

`MarketEconomyService.BuildBestValueCombination(...)` is shared and useful, but it is still framed around market flows rather than a broader economy contract.

Consequence:

- the algorithm is reusable
- the ownership concept is still "market helper", not "economy policy"

### 5. Economy has no explicit delivery policy layer yet

Severity: medium

There is still no dedicated concept for:

- board delivery
- container delivery
- reward payout policy
- change payout policy

Consequence:

- those rules remain attached to slots instead of becoming reusable market-domain policy

Status update:

`MarketDeliveryService` now exists as an explicit delivery-policy layer for purchase change and sale rewards.

## What is already effectively done in phase 7

These original phase-7 goals are already partially covered:

- shared payment/change logic exists
- typed currency contracts exist
- transaction assembly is no longer fully duplicated

So phase 7 should not be treated as "economy starts now".

The base already exists.

## What phase 7 now really means

At this point, phase 7 is mainly about:

1. moving transaction orchestration further out of slot UI components
2. separating value logic from delivery logic
3. preparing pricing and payout policy for richer future economy systems
4. reducing the amount of market-specific domain branching inside slot scripts

## Best next step

The best next step is not a giant economy framework.

The best next step is to extract a market-facing transaction coordinator that sits above `MarketEconomyService` and `MarketTransactionService`, while moving delivery policy out of the slot scripts.

Suggested direction:

- keep `MarketEconomyService` as low-level economy helper logic
- keep `MarketTransactionService` as transaction assembly/consumption logic
- introduce something like `MarketTransactionCoordinator`
- let that coordinator decide:
  - how purchases complete
  - how sales complete
  - where rewards/change are delivered
  - when slot UI should succeed or fail

That would reduce slot responsibility without forcing a full pricing framework yet.

Status update:

This first extraction is now implemented through `MarketTransactionCoordinator`, which already moves purchase/sell orchestration and reward/change delivery policy out of the slot scripts.

## Recommended phase-7 order from here

1. Extract purchase/sell orchestration out of slot scripts
2. Introduce explicit reward/change delivery policy
3. Reassess whether card valuation should remain direct or move toward a richer pricing contract
4. Only then evaluate broader vendor or dynamic-price systems

## Audit conclusion

Phase 7 is already partially solved.

The core remaining debt is no longer raw currency typing.

The main remaining debt is:

- slot-owned orchestration
- delivery policy living in UI scripts
- valuation still being too close to card raw value for future expansion
