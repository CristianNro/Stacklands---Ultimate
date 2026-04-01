\# Crafting System



\## Purpose



Crafting may occur over time instead of instantly.



A crafting task typically tracks:

\- target stack

\- active recipe

\- remaining time

\- progress state



\## Required behavior



\- progress must update correctly

\- if the stack disappears, the task must stop safely

\- if the stack becomes invalid, the task must stop or re-evaluate safely

\- completion must consume/preserve inputs correctly

\- the result must spawn correctly

\- after completion, the stack may continue crafting if still valid



\## Batch crafting expectations



Batch crafting should behave similarly to Stacklands, but safely.



Important expectations:

\- no accidental partial-match behavior

\- one completion must not consume multiple future inputs at once

\- after each completion, the stack must be re-evaluated from the real updated state

\- continuation must be deterministic



\## Important warning



Do not bypass the active crafting task system unless explicitly requested.

