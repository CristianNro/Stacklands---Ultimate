\# Board and Layout



\# Coding Rules



\## General style



\- keep code readable and explicit

\- prioritize practical solutions

\- avoid unnecessary abstraction

\- use clear names tied to gameplay concepts

\- do not overengineer for hypothetical future features



\## Comments



Include inline comments when:

\- the logic is not obvious

\- there is an architectural reason behind a decision

\- a bug-prone section needs protection



Avoid noisy comments on trivial lines.



\## Logging



Logs are useful for:

\- stack creation/removal

\- recipe selection

\- crafting start/finish/cancel

\- spawn and position issues

\- drag/drop debugging



Avoid excessive permanent spam logging.



\## Prohibited style patterns unless explicitly needed



\- giant rewrites

\- excessive inheritance just for formality

\- manager classes absorbing unrelated responsibilities

\- speculative architecture disconnected from the current codebase

