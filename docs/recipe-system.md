\# Recipe System



\## Purpose



Recipes define how card combinations produce results.



\## Supported concepts



The system is expected to support:

\- normal recipes

\- batch recipes

\- tag-based matching



\## Priorities



Normal recipes should generally take priority over batch recipes when both could match.



Recipe matching must remain deterministic.



\## Tags



Tags are a core scalability mechanism.

Examples:

\- worker

\- resource

\- tool

\- food

\- structure



Tags should be preferred over hardcoded special-case logic when possible.



\## Important rule



Do not hardcode recipe logic per card unless the existing recipe architecture truly cannot express the behavior.

Prefer data-driven design where possible.

