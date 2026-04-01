\# Board and Layout



\## Board model



Cards exist inside a UI board and must respect layout constraints.



Relevant concepts may include:

\- BoardRoot

\- PlayArea

\- CardContainer

\- cards parent

\- margin-restricted placement zones



\## Required behavior



\- cards must stay within valid board areas

\- stacks must remain fully inside allowed bounds

\- clamping must consider full stack size, not only the top card

\- calculations must use the correct RectTransform context



\## Historical problems



Past issues included:

\- stack containers created in wrong parents

\- full stack height not considered during clamping

\- resolution and anchoring inconsistencies

\- bad behavior near margins



\## Rule



Any placement logic must account for complete stack footprint, not just a single card.

