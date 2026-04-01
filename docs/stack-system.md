\# Stack System



\## Role of a stack



A stack is a gameplay structure, not only a visual overlap.



It is responsible for:

\- grouping cards

\- maintaining order

\- adding and removing cards

\- updating layout offsets

\- exposing contents for recipe checks

\- merging correctly with single cards or other stacks



\## Invariants



The following should remain true:

\- multi-card stacks are supported

\- a stack of one card must not be treated incorrectly as empty or invalid

\- removing one card must not corrupt remaining cards

\- merging must preserve references and valid positioning

\- stack state must remain authoritative for gameplay



\## Visual rule



Cards in a stack are typically offset vertically, for example `(0, -15)` per card.

This rule may evolve, but layout consistency must be preserved.



\## Important warning



Do not fake stack behavior through visuals alone.

Gameplay must rely on actual stack state.

