# Boxer — GPT-6 Astra Handoff

> Read this first, then `11-current-state.md`, `14-agent-working-protocol.md`, `08-visual-ux-direction.md`, and the approved visual authority under `reference-ui/`.

## Project identity
**Boxer** is a POV boxing career simulator/research game. Thesis: **The player does not control a boxer from outside. The player is the boxer.** Core fantasy: **Fight from your own eyes.**

Combat loop: `SEE → READ → MOVE / GUARD / EVADE → CREATE OPENING → COUNTER → RESET`
Anti-goal: `SWIPE → SWIPE → SWIPE → SPAM → WIN`

## Active implementation
Repository `minhtri22/boxer`; active branch `p1/whole-body-mechanics`; Unity `6000.5.8f1`; Pages `https://minhtri22.github.io/boxer/`.
Accepted UAT artifact before docs/reference commits: `4a985574f44d458357f875a1b7a6d3d00c7f9386`; source used: `4939e2bd5f44513d9d2ad3732ae07ac02f2ea148`.

## Frozen controls
Phone=Head; Left Thumb=Feet; Right Thumb=Punch Controller; No active action=Return to Guard.
Gestures: TAP→STRAIGHT; HOLD+UP→UPPERCUT; HOLD+HORIZONTAL→HOOK; HOLD+DOWN→OVERHAND.

## Human-approved visual authority
Before any UAT-facing work inspect `reference-ui/README-VISUAL-REFERENCE.md`, `ui-reference-notes.md`, `visual-lock.yaml`, and all six approved images. They are **binding UAT visual authority**. Combat target: `03-pov-combat-hud.jpg`. Future opponent production authority: `06-opponent-ramirez-turnaround.jpg`. Other images lock future surfaces but do not unlock them.

## Verified state
P0 PASS; P1-A1 PASS; P1-A2 PASS; P1-A3.1 HUMAN PASS; P1-B1 PASS; P1-B1.5 HUMAN PASS; P1-B2 HUMAN PASS.

## Next direction
Hip/torso rotation → weight transfer → whole-body punch coupling → recovery → Combat Log + Biomechanics Inspector → then A3.2/A3.3.

## Hard invariants
Presentation cannot silently alter authoritative combat geometry. Synthetic PASS != Human PASS. Build only clean committed source. Build marker/productVersion/build_commit must match source short SHA. No rebase after provenance lock. A3.2/A3.3 remain locked. Player legs remain out of current POV scope. `docs/handoff/reference-ui/*` requires explicit human approval to change. Visual references never unlock future systems ahead of roadmap.

Autonomous execution prompt: `16-astra-autonomous-execution-prompt.md`.
