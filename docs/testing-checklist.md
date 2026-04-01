\# Testing Checklist



A change should not be considered safe until at least these cases are validated.



\## Spawn tests

\- spawn a single card

\- verify correct parent

\- verify correct anchored position

\- verify no teleport on first drag



\## Drag tests

\- drag a single card

\- drag a card from a stack

\- drag near borders

\- drag after spawning a new result card



\## Stack tests

\- create a stack from two single cards

\- add a third card

\- remove one card

\- separate a stack

\- merge into an existing stack

\- verify offsets remain correct



\## Recipe tests

\- verify a normal recipe matches correctly

\- verify a batch recipe only activates when intended

\- verify tag-based matching still works

\- verify priority remains deterministic



\## Crafting tests

\- start crafting on a valid stack

\- verify progress updates

\- invalidate the stack during crafting

\- destroy/remove the stack during crafting

\- let crafting complete

\- verify correct consumption/preservation

\- verify result spawn

\- verify continuation if stack remains valid



\## Board/layout tests

\- place cards near edges

\- place tall stacks near edges

\- verify full stack stays inside valid area

\- verify no bad clamping artifacts



\## Visual safety tests

\- verify animation does not break ownership/state

\- verify card remains interactable after animation

\- verify no mismatch between visual and logical state

