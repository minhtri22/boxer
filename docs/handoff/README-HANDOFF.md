# Boxer — GPT-6 Astra Handoff

> Read this file first. Then read `11-current-state.md`, `14-agent-working-protocol.md`, and the approved visual authority under `reference-ui/`. Do not implement anything until you can restate the verified state, frozen invariants, unresolved question, acceptance gate, and UAT visual target.

## Project identity

**Boxer** is a POV boxing career simulator/research game built around one product thesis:

> **The player does not control a boxer from outside. The player is the boxer.**

Core fantasy: **Fight from your own eyes.**

Combat loop:
`SEE → READ → MOVE / GUARD / EVADE → CREATE OPENING → COUNTER → RESET`

Anti-goal:
`SWIPE → SWIPE → SWIPE → SPAM → WIN`

## Repository and active implementation

- Repository: `minhtri22/boxer`
- Active branch: `p1/whole-body-mechanics`
- Unity: `6000.5.8f1`
- Local reference path: `D:\WORK\RESEARCH\POVGame\boxer`
- GitHub Pages: `https://minhtri22.github.io/boxer/`
- Current accepted UAT artifact before docs-only handoff/reference commits: `4a985574f44d458357f875a1b7a6d3d00c7f9386`
- Source used for that binary: `4939e2bd5f44513d9d2ad3732ae07ac02f2ea148`

## Frozen control thesis

- **Phone = Head**
- **Left Thumb = Feet**
- **Right Thumb = Punch Controller**
- **No active action = Return to Guard**

Punch gestures:
- `TAP → STRAIGHT`
- `HOLD + SWIPE UP → UPPERCUT`
- `HOLD + SWIPE HORIZONTAL → HOOK`
- `HOLD + SWIPE DOWN → OVERHAND`

## Human-approved visual authority

Before any UAT-facing implementation, read and inspect:
1. `08-visual-ux-direction.md`
2. `reference-ui/README-VISUAL-REFERENCE.md`
3. `reference-ui/ui-reference-notes.md`
4. `reference-ui/visual-lock.yaml`
5. all six approved images in `reference-ui/`

These are **binding UAT visual authority**, not optional moodboards.

Current combat UAT reference: `reference-ui/03-pov-combat-hud.jpg`.
Future production opponent authority: `reference-ui/06-opponent-ramirez-turnaround.jpg`.
Other images lock future customization/home/venue-career/training visual direction but do not unlock those systems before roadmap.

## Current verified state

- P0 interaction/control surrogate: PASS, Safari runtime limitation non-blocking.
- P1-A1 step → straight reach: PASS.
- P1-A2 punch family gesture vocabulary: PASS.
- P1-A3.1 hook close-range coupling: HUMAN PASS.
- P1-B1 opponent finite reach / target lock / no homing: PASS.
- P1-B1.5 arm embodiment/topology/guard cleanup: HUMAN PASS.
- P1-B2 opponent pelvis + leg embodiment: HUMAN PASS.

The body embodiment milestone is accepted. Move to whole-body boxing mechanics rather than further primitive-anatomy polishing.

## Next research direction

1. Hip / torso rotation.
2. Weight transfer.
3. Whole-body punch coupling.
4. Recovery to stance/guard.
5. Combat Log + Biomechanics Inspector.
6. Then open A3.2/A3.3.

For UAT-facing builds, safely converge the visual shell toward the approved combat reference without changing frozen mechanics.

## Hard invariants

1. Presentation fixes must not silently alter authoritative combat geometry.
2. Synthetic PASS is not Human PASS.
3. Build from committed, clean source only.
4. Build marker, productVersion and `build_commit` must match committed source short SHA.
5. Do not rebase after provenance lock.
6. A3.2/A3.3 remain locked until explicitly opened.
7. Opponent legs required in current POV; player legs remain out of scope until replay/cinematic need.
8. `docs/handoff/reference-ui/*` cannot change without explicit human approval.
9. Visual references do not unlock future systems ahead of roadmap.

## Reading order

1. `README-HANDOFF.md`
2. `11-current-state.md`
3. `14-agent-working-protocol.md`
4. `08-visual-ux-direction.md`
5. `reference-ui/README-VISUAL-REFERENCE.md`
6. `reference-ui/visual-lock.yaml`
7. inspect task-relevant approved images
8. task-relevant domain docs/source/evidence

Autonomous execution prompt: `16-astra-autonomous-execution-prompt.md`.

## Required Astra pre-implementation response

Before changing source, state:
- current verified state;
- frozen invariants;
- exact unresolved question;
- minimal experiment / implementation scope;
- acceptance gate;
- UAT visual target and expected reference match;
- files expected to change;
- explicitly out-of-scope items.

If any cannot be stated from the repository, stop and report the gap instead of guessing.
