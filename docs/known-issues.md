\# Known Issues



This file records historically fragile areas that must be treated carefully.



\## Drag and drop



Past issues:

\- teleporting when dragging cards from stacks

\- bad initial anchored position after spawn

\- inconsistent behavior depending on parent/container



\## Stack integrity



Past issues:

\- stacks of one card treated incorrectly

\- broken offsets after adding/removing cards

\- merge behavior corrupting layout or references



\## Crafting



Past issues:

\- progress bar staying static

\- crafting tasks continuing when they should stop

\- invalid stack state during crafting completion

\- bad re-evaluation after one batch completion



\## Board/layout



Past issues:

\- stack parent created in the wrong container

\- clamping only top card instead of full stack

\- resolution and anchoring problems



\## Prefabs/references



Past issues:

\- required components missing on prefabs

\- runtime initialization not called

\- null reference chains caused by prefab setup rather than gameplay logic

