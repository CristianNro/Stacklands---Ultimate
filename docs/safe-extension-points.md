\# Safe Extension Points



\## Preferred extension areas



New features should plug into existing systems through their natural extension points.



\### Cards

Safe extensions:

\- new card data fields

\- new card categories integrated into existing data flow

\- runtime state extensions that remain instance-specific



\### Stacks

Safe extensions:

\- better stack validation

\- improved offset/layout update logic

\- safer add/remove flows

\- clearer merge behavior



\### Recipes

Safe extensions:

\- new recipe definitions

\- stronger tag matching

\- controlled priority rules

\- better batch recipe orchestration



\### Crafting

Safe extensions:

\- better task validation

\- improved cancellation/restart rules

\- better continuation logic after completion

\- progress visualization tied safely to task state



\### Board/layout

Safe extensions:

\- improved boundary logic

\- valid placement zones

\- better clamping that respects stack size



\### Visuals

Safe extensions:

\- spawn animation improvements

\- clearer stack feedback

\- better progress presentation



\## Unsafe extension pattern



Do not create new parallel systems that duplicate the responsibility of:

\- stack ownership

\- recipe evaluation

\- crafting orchestration

\- spawn positioning

