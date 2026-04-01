# Workflow for AI

## Required process

When modifying this project, follow this order:

1. inspect the relevant scripts and docs first
2. identify the exact current flow in code
3. compare the current code against the intended architecture
4. locate the smallest viable extension point or staged refactor step
5. make a focused change
6. verify drag, stack, recipe, crafting, market and board interactions still make sense
7. update docs when the real flow changes

## Root-cause rule

If a bug exists:

- prefer finding the real cause
- fix the cause
- preserve surrounding invariants where possible

Not preferred:

- replacing a subsystem just to hide the symptom

## Architecture rule

This project is no longer just preserving what exists.

It is now also moving through a documented architectural roadmap.

That means:

- do not make local fixes that worsen long-term ownership boundaries
- check `docs/architecture-roadmap.md` before extending core systems
- prefer changes that move the code toward cleaner separation

## Output preference

When proposing or implementing changes, prefer:

- complete methods
- complete class sections
- clearly identified replacement blocks
- changes that fit the current repo structure

Avoid:

- vague pseudo-code
- advice that assumes systems not present in the project
- generic engine architecture not grounded in this codebase
