# Coding Rules

## General style

- keep code readable and explicit
- prioritize practical solutions
- avoid abstraction without clear ownership
- use names tied to gameplay concepts
- do not overengineer for hypothetical features

## Project-specific architectural rule

Do not respond to architectural debt by making inheritance deeper unless there is a very strong reason.

The preferred direction for this project is:

- smaller responsibilities
- clearer ownership
- composition where it improves extensibility
- less cross-system coupling

## Comments

Include inline comments when:

- the logic is not obvious
- there is an architectural reason behind a decision
- a historically fragile section needs protection

Avoid noisy comments on trivial lines.

## Logging

Logs are useful for:

- stack creation/removal
- recipe evaluation decisions
- crafting start/finish/cancel
- spawn and position issues
- drag/drop debugging
- container and market state transitions

Avoid excessive permanent spam logging.

## Documentation rule

If the code changes a system boundary or the expected flow of a core mechanic, update the matching document in `docs/`.

This repository now has architecture docs meant to stay synchronized with the real code, not aspirational code.

## Patterns to avoid unless explicitly justified

- giant rewrites without a staged plan
- excessive inheritance just for formality
- manager classes absorbing unrelated responsibilities
- speculative architecture disconnected from the current project
- duplicate business logic in multiple UI/gameplay components
