\# AGENTS.md



\## Purpose



This project is a Unity game inspired by Stacklands.

The AI assistant must not generate generic solutions or rewrite systems without need.



The goal is to extend and stabilize the current architecture while preserving existing gameplay behavior.



\## Main rules



\- Preserve the existing architecture.

\- Prefer incremental changes over large refactors.

\- Do not replace current systems unless explicitly requested.

\- Inspect the current scripts before proposing changes.

\- Avoid ungrounded assumptions.

\- Protect drag and drop, stack integrity, recipe matching, timed crafting, and spawn positioning above all else.

\- Prefer directly integrable code over vague pseudo-code.

\- Keep future extensibility in mind, but do not overengineer.



\## Documentation map



Read these files depending on the task:



\- `docs/project-overview.md` → general project identity and goals

\- `docs/architecture.md` → high-level system architecture

\- `docs/card-system.md` → card data, runtime state, and visual representation

\- `docs/stack-system.md` → stack behavior and invariants

\- `docs/recipe-system.md` → recipe matching and priorities

\- `docs/crafting-system.md` → timed crafting tasks and lifecycle

\- `docs/spawning-and-positioning.md` → spawning flow and anchored positioning rules

\- `docs/board-and-layout.md` → board constraints and layout behavior

\- `docs/visual-and-animation-rules.md` → animation and visual safety rules

\- `docs/coding-rules.md` → coding style and implementation constraints

\- `docs/workflow-for-ai.md` → preferred process for modifying the project

\- `docs/known-issues.md` → historical bugs and fragile areas

\- `docs/safe-extension-points.md` → where new features should plug into the system

\- `docs/testing-checklist.md` → minimum validation before considering a change safe



\## Final rule



This project must evolve by extending current systems, not by replacing them with generic solutions.

