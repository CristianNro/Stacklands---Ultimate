\# Workflow for AI



\## Required process



When modifying this project, follow this order:



1\. inspect the existing relevant scripts

2\. identify the exact current flow

3\. locate the smallest safe extension point

4\. make a focused change

5\. preserve existing contracts where possible

6\. verify drag, stack, and crafting still work together

7\. only then propose optional follow-up refactors



\## Root-cause rule



If a bug exists:

\- prefer finding the root cause

\- fix the root cause

\- preserve the rest of the system



Not preferred:

\- replacing the subsystem to hide the bug



\## Output preference



When proposing changes, prefer:

\- complete methods

\- complete class sections

\- clearly identified replacement blocks



Avoid:

\- vague pseudo-code

\- disconnected snippets missing critical integration parts

\- advice that assumes systems not present in the project

