\# Spawning and Positioning



\## Spawning responsibilities



The spawner should:

\- instantiate the prefab

\- parent it correctly

\- initialize runtime state

\- initialize visual state

\- assign anchored position correctly

\- avoid broken coordinates on spawn



\## Positioning rules



This project relies heavily on `RectTransform.anchoredPosition`.



Do not mix:

\- world position logic

\- local transform assumptions

\- anchored UI coordinates



without being explicit and correct.



\## Historical fragility



Past issues have included:

\- newly spawned cards teleporting

\- cards spawning with wrong anchored positions

\- stacks created under wrong parents

\- drag behavior breaking after spawn



\## Rule



If a positioning bug appears, verify parent container and coordinate space first.

