# 10 — Research Roadmap

## Research method

Boxer uses incremental causal validation:

> **PROVE ONLY WHAT IS STILL UNCERTAIN → DECIDE → IMPLEMENT**

Do not bundle multiple uncertain biomechanics into one patch if causal attribution would be lost.

## Completed / accepted milestones

### P0 — Interaction feasibility
Question: can POV phone/head + left-thumb feet + right-thumb punch intent be understood on a real phone surrogate?

Verdict: PASS with known Safari/WebGL runtime limitations treated as non-blocking surrogate limitations. Do not reopen generic Safari optimization unless strategy changes.

### P1-E0 — Instrumentation
Punch-start semantic snapshot and diagnostics.

Verdict: PASS.

### P1-A1 — Step → Straight Reach
Question: should advancing/neutral/retreating state slightly change straight reach?

Frozen: 1.06 / 1.00 / 0.94.

Verdict: PASS.

### P1-A2 — Punch Gesture Vocabulary
Question: can one right-thumb controller express punch families?

Verdict: PASS.

### P1-B1 — Opponent finite reach / fair target lock
Question: can opponent punches stop homing/stretching and become readable/fair?

Verdict: PASS.

### P1-A3.1 — Hook Close-Range Coupling
Question: should hooks naturally lose forward effectiveness outside their pocket?

Frozen: full <=1.05m; linear to 1.25m; far factor 0.86.

Verdict: HUMAN PASS.

### P1-B1.5 — Arm embodiment and readability
Iterations B1.5R/S/T/U/V corrected explicit elbow topology, shoulder anchoring, glove coherence, upper-arm readability, guard pose and late yellow/orange glove-decoration artifact.

Verdict for current milestone: HUMAN PASS.

### P1-B2 — Opponent pelvis + leg embodiment
Question: can the opponent stop reading as torso-on-pedestal and become a readable full-body combat target?

Topology: pelvis → thigh → knee joint → shin → foot.

Verdict: HUMAN PASS.

## Completed whole-body frontier

### Whole-body boxing mechanics

Open a new controlled sequence, recommended order:

1. **Hip/Torso Rotation Baseline** — observe and expose pelvis/torso rotational state without making it a global power scalar.
2. **Weight Transfer Baseline** — define a small, testable stance/weight variable.
3. **Step + Hip + Straight coupling** — determine whether straights gain/lose reach/quality from body coordination beyond A1.
4. **Recovery to stance/guard** — whole-body recovery, not arm-only return.
5. **Combat Log + Biomechanics Inspector** — make body state and outcomes inspectable.
6. **A3.2 Uppercut biomechanics** — only after inspector exists.
7. **A3.3 Overhand biomechanics** — only after inspector exists.

All seven whole-body frontier items are now deterministic PASS with intermediate human UAT deferred to the final integrated candidate.

## Current frontier

### Counter geometry

Prove that an evade caused by head movement or footwork creates a geometrically meaningful counter opportunity, using P1-OBS to distinguish the causal miss reason and timing window.

Verdict: deterministic PASS. The opportunity requires a would-hit-at-commit path to miss the moved target at resolution and opens only during Recover.

### Lightweight opponent attributes

Reach, aggression and speed are now isolated, inspectable tactical variables without RPG statistics or hidden damage modifiers. BALANCED preserves the baseline; LONG_REACH changes reach only, PRESSURE changes the post-attack gap only, and FAST_HANDS changes phase duration only.

Verdict: deterministic PASS, 8/8 P1-D checks and 131/131 combined checks. Human judgment is deferred to the final integrated candidate.

### Next: approved visual/UI convergence

Replace the procedural prototype presentation with the locked combat visual package, production 3D rig/animation and complete combat HUD while preserving all verified mechanics and telemetry. This milestone prepares the single final integrated UAT candidate.

## Later research

- semantic replay;
- KO/highlight generation;
- career/progression;
- damage/knockdown sophistication.

## Stop condition for a research patch

Stop when:

- the one intended uncertainty has been answered;
- deterministic regression is clean;
- required real-device UAT is complete;
- evidence is committed;
- no extra mechanics are smuggled into the same patch.
