\# Card System



\## Card data



Cards are defined by data assets.



Typical fields may include:

\- id

\- internal name

\- display name

\- description

\- image

\- rarity

\- card type

\- stackability

\- value

\- subtype-specific properties



\## Important rule



Do not store mutable runtime state in shared ScriptableObject assets.



\## Runtime state



A distinction may exist between:

\- static card definition

\- runtime state of a spawned card



Runtime state can include:

\- uses remaining

\- health

\- temporary state

\- stack membership

\- crafting participation



\## Card view



The visual representation of a card should:

\- render image and text

\- reflect current runtime/data state

\- keep references valid

\- avoid breaking drag or stack behavior



\## Design rule



If a behavior belongs to card data or recipe rules, do not hardcode it in random gameplay scripts.

