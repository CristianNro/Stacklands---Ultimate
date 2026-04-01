\# Architecture



\## High-level model



This project uses a UI-based card system, not world-space physical objects.



Main flow:

1\. card data defines a card

2\. a card is spawned into the UI board

3\. the player drags the card

4\. the card can remain alone or join a stack

5\. a stack is evaluated for recipe matches

6\. if valid, a timed crafting task may start

7\. when crafting ends, the system consumes/preserves inputs

8\. the result card is spawned

9\. the stack is re-evaluated for continued valid crafting



\## Main system areas



\- card data

\- runtime card instance state

\- visual card view

\- spawning

\- drag and drop

\- stack management

\- recipe evaluation

\- timed crafting

\- board/layout constraints

\- animation/feedback



\## Core architectural rule



Systems should remain specialized.

Do not merge unrelated responsibilities into giant manager classes.

Do not duplicate logic across parallel systems.

