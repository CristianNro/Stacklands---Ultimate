# Testing Checklist

A change should not be considered safe until these flows are validated against the systems it affects.

Phase 11 direction:

- this file is still the manual safety baseline
- high-value flows from this checklist should gradually move into editor validators or domain-level automated tests

## Spawn tests

- spawn a single card
- verify correct parent
- verify correct anchored position
- verify no teleport on first drag
- verify active card registration still works

## Drag tests

- drag a single card
- drag a root card from a stack
- drag a middle card and split a stack
- drag near borders
- drag after spawning a new result card
- drag onto empty board, card, stack, market slot and container when relevant
- verify the same drop cases still behave correctly after the handler split in phase 4

## Stack tests

- create a stack from two single cards
- add a third card
- remove one card
- separate a stack
- merge into an existing stack
- verify offsets remain correct
- verify trivial stack cleanup works correctly

## Recipe tests

- verify an exact recipe matches correctly
- verify an exact recipe with `requiredCount > 1` matches correctly
- verify an exact recipe with `allowAdditionalCopies = true` keeps matching when extra copies are added
- verify a capability-driven recipe only activates when intended
- verify specificity remains deterministic
- verify ingredient consume rules apply correctly
- verify repeatable recipes stop when the stack becomes invalid
- verify repeated exact-resource recipes consume only the minimum required ingredient count per cycle

## Crafting tests

- start crafting on a valid stack
- verify progress updates
- invalidate the stack during crafting
- destroy or trivialize the stack during crafting
- let crafting complete
- verify correct consumption or preservation
- verify result spawn
- verify continuation if the stack remains valid

## Board/layout tests

- place cards near edges
- place tall stacks near edges
- verify the full stack stays inside the valid area
- verify clamping uses the correct parent/container
- verify auto-spawned cards do not land on invalid coordinates

## Container tests

- store a single card
- store cards from a dragged stack
- release contents on board
- verify runtime value refresh for currency containers
- verify stored food preserves `remainingFoodValue`
- verify stored transformables preserve elapsed transformation progress

## Market tests

- buy a pack with a single valid currency card
- buy using a stack of currencies
- buy using a currency container
- verify correct change behavior
- sell a valid card
- sell a valid stack
- verify market interactions do not corrupt stack ownership

## Visual safety tests

- verify animation does not break ownership or registration
- verify cards remain interactable after animation
- verify no mismatch between visual and logical state
- verify progress visuals stop when tasks stop

## Day cycle tests

- let a day complete and verify the day number advances
- verify pause stops day progression
- verify the speed-cycle button rotates `x1 -> x2 -> x3 -> x1`
- verify `DailyUpkeepSystem` consumes food in deterministic order
- verify units that are not fully fed die at day end
- verify `SpawnCards` day events trigger on the intended day only
- verify non-repeatable day events do not fire twice in the same runtime session

## Single-card transformation tests

- spawn a card with `transformationRule` and verify progress starts automatically after spawn
- verify a transforming card keeps progressing inside a stack
- verify `pauseCapabilities` stop progress when matching stack context exists
- verify `speedModifiers` change progress speed when matching stack context exists
- verify `ReplaceWithSingleResult` replaces the source card
- verify `SpawnMultipleResults` creates all configured outputs in valid board positions
- verify `showProgressBar = false` hides the transformation bar
