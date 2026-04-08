# Market Economy Future Improvements

## Purpose

This document captures the most valuable next improvements for the commerce/economy system after the current phase-7 work.

It is not a commitment to implement all of them now.

The goal is to preserve architectural direction so future work can continue from the current state without reopening solved problems.

## Current baseline

The market already has:

- explicit currency contracts through `isCurrency`, `CurrencyType` and `CurrencyFilterMode`
- shared value-combination logic in `MarketEconomyService`
- transaction assembly and consumption in `MarketTransactionService`
- orchestration in `MarketTransactionCoordinator`
- reward/change delivery in `MarketDeliveryService`
- an explicit pricing boundary in `MarketPricingService`

That means future work should extend these services instead of pushing new business rules back into slot scripts.

## Improvement 1. Richer pricing policy

### Goal

Allow market value to differ from raw card value without removing `CardData.value` as the base visible value.

### Suggested implementation

Keep:

- `CardData.value` as base value
- `CardInstance.GetEffectiveValue()` as runtime-adjusted value

Extend:

- `MarketPricingService`

Possible additions:

- `GetPurchasePrice(...)`
- `GetSellPrice(...)`
- optional slot or vendor context input
- buy/sell multipliers
- card-type specific multipliers

### What to keep in mind

- do not hide major price differences from the player without some visible explanation
- keep current behavior as the default fallback
- avoid writing price rules directly into `MarketPackPurchaseSlot` or `MarketSellSlot`

## Improvement 2. Explicit payout policy

### Goal

Control not just how much value is paid, but how the market prefers to represent that value physically.

### Suggested implementation

Introduce a payout-policy concept above `MarketEconomyService.BuildBestValueCombination(...)`.

Possible policy inputs:

- prefer fewer cards
- prefer higher denominations
- prefer listed reward cards first
- prefer exact currency type order

Possible new service:

- `MarketPayoutPolicyService`

That service would decide which card set to use before `MarketDeliveryService` performs actual delivery.

### What to keep in mind

- the current combination algorithm is deterministic and simple; preserve that as a fallback
- avoid mixing payout preference with delivery location
- payout policy and delivery policy are separate responsibilities

## Improvement 3. Vendor-specific pricing

### Goal

Allow different market slots or future vendors to pay or charge different prices for the same card.

### Suggested implementation

Add market-context inputs to `MarketPricingService`, for example:

- slot type
- vendor id
- vendor multiplier
- allowed card groups

This can start small:

- per-slot buy multiplier
- per-slot sell multiplier

### What to keep in mind

- do not encode vendor logic directly in the slot scripts
- the slot should provide configuration, not implement pricing rules
- if a vendor changes price materially, decide how the UI will communicate that

## Improvement 4. Runtime-state-sensitive pricing

### Goal

Let market value react to card condition or state.

Examples:

- used tools sell for less
- spoiled food sells for less
- container contents affect container value

### Suggested implementation

Extend `MarketPricingService` to read:

- runtime value overrides
- uses remaining
- relevant specialized runtime state

If needed, create a dedicated helper:

- `MarketValueContextBuilder`

### What to keep in mind

- this should not break the current simple visible value model without intentional UI support
- prefer deriving market value from runtime state in one place, not scattered across services

## Improvement 5. Time-aware economy

### Goal

Prepare the market to respond to the future game-time/day system.

Examples:

- daily price changes
- stock refresh at day end
- temporary events
- food price inflation when supplies are low

### Suggested implementation

Use `GameTimeService` or a future day-cycle system as an input to economy services.

Potential additions:

- `MarketEconomyState`
- `VendorStockRuntime`
- daily refresh hooks
- time-aware pricing modifiers in `MarketPricingService`

### What to keep in mind

- do not make market slots listen to day events directly
- keep time-reactive logic in services or runtime market state objects
- separate "time changed" from "how price/stock reacts"

## Improvement 6. Stock and inventory rules

### Goal

Move from infinite static market behavior toward explicit stock ownership.

### Suggested implementation

Possible concepts:

- `VendorStockRuntime`
- `MarketOffer`
- `LimitedPurchaseRule`

This would allow:

- finite items per day
- restock rules
- sold-out states
- vendor-specific inventories

### What to keep in mind

- stock is not the same thing as pricing
- stock should not be tracked ad hoc in UI slot components
- keep slot visuals reacting to stock state, not owning it

## Improvement 7. Better reward/change delivery destinations

### Goal

Generalize delivery beyond the current board-or-container split.

### Suggested implementation

Extend `MarketDeliveryService` to support explicit delivery targets or policies, for example:

- board near slot
- same container
- same stack root area
- future mailbox or storage system

### What to keep in mind

- delivery policy should stay separate from transaction validity
- avoid encoding target routing in every slot implementation

## Recommended order if phase 7 is resumed

1. richer pricing policy
2. explicit payout policy
3. vendor-specific pricing
4. runtime-state-sensitive pricing
5. time-aware economy
6. stock and inventory rules
7. broader delivery destinations

## Architectural rule

If commerce work resumes, extend:

- `MarketPricingService`
- `MarketTransactionCoordinator`
- `MarketTransactionService`
- `MarketDeliveryService`

Avoid pushing new rules back into:

- `MarketPackPurchaseSlot`
- `MarketSellSlot`

Those slot scripts should keep moving toward configuration + interaction entrypoint only.
