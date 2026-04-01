\# Board and Layout



\# Visual and Animation Rules



\## Priority



Visual polish is important, but gameplay integrity is more important.



\## Allowed responsibilities



Animations may be used for:

\- crafted result spawning

\- movement arcs

\- stack feedback

\- progress display

\- selection feedback



\## Rules



\- animation code must not corrupt authoritative gameplay state

\- card and stack ownership must remain valid regardless of animation timing

\- if animation and gameplay state disagree, gameplay state wins

\- visual improvements must not break drag, stack, recipe, or crafting logic



\## UI quality concerns



The project has had concerns related to:

\- pixelation

\- scaling distortion

\- anchoring inconsistencies



Any visual/layout change must preserve correct behavior across resolutions.

