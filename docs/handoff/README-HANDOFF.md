# Boxer — GPT-6 Astra Handoff

> Read this file first. Then read `11-current-state.md` and `14-agent-working-protocol.md`. Do not implement anything until you can restate the verified state, frozen invariants, unresolved question, and acceptance gate.

## Project identity

**Boxer** is a POV boxing career simulator/research game built around one product thesis:

> **The player does not control a boxer from outside. The player is the boxer.**

Core fantasy: **Fight from your own eyes.**

The intended combat loop is:

`SEE → READ → MOVE / GUARD / EVADE → CREATE OPENING → COUNTER → RESET`

Anti-goal:

`SWIPE → SWIPE → SWIPE → SPAM → WIN`

## Repository and active implementation

- Repository: `minhtri22/boxer`
- Active branch: `p1/whole-body-mechanics`
- Unity: `6000.5.8f1`
- Local reference path: `D:\WORK\RESEARCH\POVGame\boxer`
- GitHub Pages: `https://minhtri22.github.io/boxer/`
- Current accepted UAT artifact before this docs-only handoff commit: `4a985574f44d458357f875a1b7a6d3d00c7f9386`
- Source used for that binary: `4939e2bd5f44513d9d2ad3732ae07ac02f2ea148`

## Frozen control thesis

- **Phone = Head** — device orientation controls head/evasion intent.
- **Left Thumb = Feet** — directional footwork/position intent.
- **Right Thumb = Punch Controller** — punch-family intent, not explicit right-hand control.
- **No active action = Return to Guard** — idle returns to guard; guard is not free invulnerability.

Punch gesture vocabulary:

- `TAP → STRAIGHT`
- `HOLD + SWIPE UP → UPPERCUT`
- `HOLD + SWIPE HORIZONTAL → HOOK`
- `HOLD + SWIPE DOWN → OVERHAND`

Gesture selects **punch family**. Hand selection is contextual (stance / previous hand / sequence / body state).

## Current verified state

Human UAT has accepted the current upper- and lower-body embodiment milestone:

- P0 interaction/control surrogate: PASS, with Safari runtime limitation treated as non-blocking surrogate limitation.
- P1-A1 step → straight reach: PASS.
- P1-A2 punch family gesture vocabulary: PASS.
- P1-A3.1 hook close-range coupling: HUMAN PASS.
- P1-B1 opponent finite reach / target lock / no homing: PASS.
- P1-B1.5 arm embodiment/topology/guard cleanup: HUMAN PASS for current milestone.
- P1-B2 opponent pelvis + leg embodiment: HUMAN PASS for current milestone.

The current boxer body is now readable enough to stop polishing primitive anatomy and move to **whole-body boxing mechanics**.

## Next research direction

Do **not** jump to career, damage-model redesign, replay generator, A3.2 or A3.3 yet.

Next problem family:

1. Hip rotation / torso rotation.
2. Weight transfer.
3. Whole-body punch coupling.
4. Recovery back to stance/guard.
5. Then Combat Log + Biomechanics Inspector.
6. Then open A3.2 uppercut and A3.3 overhand biomechanics.

## Hard invariants

1. Presentation fixes must not silently alter authoritative combat geometry.
2. Synthetic PASS is not Human PASS.
3. Build from committed, clean source only.
4. Build marker, generated productVersion and metadata `build_commit` must match the committed source short SHA exactly.
5. Do not rebase after a provenance-locked build.
6. A3.2/A3.3 remain locked until explicitly opened.
7. Current POV combat requires opponent legs; player legs are out of scope until replay/cinematic/full-body needs require them.

## Reading order

For any new task:

1. `README-HANDOFF.md`
2. `11-current-state.md`
3. `14-agent-working-protocol.md`
4. Read only task-relevant domain files:
   - controls → `03-control-system.md`
   - combat → `04-combat-model.md`
   - body → `05-fighter-body-model.md`
   - AI → `06-opponent-ai.md`
   - architecture → `09-technical-architecture.md`
   - roadmap/research → `10-research-roadmap.md`, `15-open-questions.md`
   - evidence/build → `12-test-evidence-matrix.md`, `13-build-release.md`

## Required Astra pre-implementation response

Before changing source, state:

- current verified state;
- frozen invariants;
- exact unresolved question;
- minimal experiment / implementation scope;
- acceptance gate;
- files expected to change;
- what is explicitly out of scope.

If any of these cannot be stated from the repository, stop and report the gap instead of guessing.
