# P1 — Whole-Body Boxing Mechanics Proof

## Status

P0 is closed for interaction feasibility. Safari/WebGL remains a surrogate-only delivery path; device-dependent stalls are a known non-blocking limitation and are not a P1 target.

P1 opens on branch `p1/whole-body-mechanics` from P0 artifact commit `076427513ee4da4cfa0f440e9c13805f4e1ed956`.

Current verified state after real-device UAT on the P1-A3.1 candidate:

- P1-E0 instrumentation: PASS
- P1-A1 step-to-straight-reach implementation: PASS
- P1-A1 human learnability: PASS — player can perceive/use the step-in reach advantage
- P1-A2 punch gesture vocabulary implementation: PASS
- P1-A2 human control mapping: PASS — TAP/UP/HORIZONTAL/DOWN all produced the expected punch families on real iPhone
- P1-B1 opponent punch embodiment/fair reach implementation: PASS
- P1-B1 human visual fairness: PENDING FURTHER REVIEW — opponent hand/reach still needs closer inspection
- P1-A3.1 hook close-range coupling implementation: PASS
- P1-A3.1 human learnability/effect: PENDING EXPLICIT CLOSE-vs-FAR HOOK UAT

## Product thesis

> The player does not control a boxer from outside. The player is the boxer.

P1 must prove that Phone=Head, Left Thumb=Feet, and Right Thumb=Punch can behave as one coupled boxing body rather than three independent input channels.

## Primary P1 hypothesis

A player can improve punch quality by coordinating position, footwork, head/body state, punch timing, and recovery. Good coordination should produce measurably better range/quality/counter opportunities than disconnected or spammed input.

## Revised priority order

1. **P1-A1 — Step Direction → Effective Straight-Punch Reach**
   - Only LEAD_JAB / REAR_CROSS are affected.
   - ADVANCING = 1.06x, NEUTRAL = 1.00x, RETREATING = 0.94x.
   - Implementation PASS.
   - Real-device human learnability PASS: player reported a noticeable/useful advantage from stepping in before straight punches.

2. **P1-A2 — Punch Gesture Vocabulary**
   - Canonical mapping:
     - TAP → STRAIGHT
     - HOLD + SWIPE UP → UPPERCUT
     - HOLD + SWIPE HORIZONTAL → HOOK
     - HOLD + SWIPE DOWN → OVERHAND
   - Gesture selects punch family only; hand selection is separate.
   - Current implementation uses deterministic minimal hand selection from previous punch state; OVERHAND defaults to rear hand.
   - Real-device UAT PASS for gesture mapping: all four requested families triggered as expected.

3. **P1-A3 — Family-Specific Whole-Body Coupling**
   - A3.1 currently active only for HOOK close-range coupling.
   - Straight → step/reach coupling already proven under A1.
   - Hook → close-range coupling active under A3.1.
   - Uppercut → punch-start base state changes vertical drive under A3.2: planted `1.00`, moving `0.85`.
   - Overhand → advancing commitment extends recovery under A3.3: `0.280s` to `0.336s`.
   - A3.2/A3.3 deterministic gates are closed; human judgment is deferred to the final integrated UAT candidate.

4. **P1-O1 — Combat Observability & Biomechanics Inspector**
   - Add an in-memory fight log for WebGL/mobile UAT.
   - Add explicit hit-resolution explanations.
   - Add debug hitbox/trajectory visualization.
   - Add enough body-state instrumentation to audit biomechanics without requiring production character graphics.
   - This is observability only; it must not change mechanics.

5. **P1-B — Range / Balance / Recovery**
   - Position and movement direction must have tactical consequences.
   - Bad body state should reduce punch quality naturally rather than via arbitrary cooldowns.

   **P1-B1 — Opponent Punch Embodiment & Fair Reach** was pulled forward as a corrective prerequisite after human observation that the opponent glove could visually “fly” toward a retreating player while still registering a hit.

   B1 rule:
   - opponent punch target is locked at commitment;
   - opponent must not home/rotate after punch commitment;
   - punch endpoint is clamped to a finite physical reach;
   - the same clamped endpoint drives both the visible glove trajectory and hit resolution;
   - if the player retreats beyond that physical endpoint, the punch must visibly and logically miss.

   Current human status:
   - implementation regression gates PASS;
   - real-device visual fairness remains under review because opponent hand/reach still needs closer inspection.

6. **P1-C — Counter Geometry**
   - Correct evade direction/timing now creates a geometrically meaningful counter opportunity under P1-CG.
   - A would-hit-at-commit path must miss the moved target at resolve; only then does Recover expose the counter window.

7. **P1-D — Lightweight Opponent Attributes**
   - Reach/aggression/speed only, enough to force tactical adaptation.

Progression, ranking, shopping, social sharing, and KO clip generation are later layers. P1 only preserves the data needed to support them.

## Onboarding product rule

The current training flow is a **first-time onboarding sequence**, not a pre-fight ritual.

Product rule:

> A player should complete the full controls onboarding once as a new player. Normal subsequent bouts should start directly without replaying the full training sequence.

Implications:

- full Head / Footwork / Punch / Guard / Counter onboarding is one-time;
- completion state must eventually persist across sessions/account/profile scope;
- future access to training should be explicit and optional (for example: Training / Controls / Practice), not forced before every bout;
- test builds may temporarily expose a reset/replay control for research, but production flow must not force repeated onboarding.

This correction is product-flow documentation only for now; do not implement persistence until the relevant product/session layer is opened.

## HP / stamina authority rule

Current HP and stamina bars are **non-authoritative research HUD semantics**. They were introduced as reactive visual feedback and are not backed by a validated boxing damage/fatigue model.

Therefore:

- current HP values must NOT be treated as physically meaningful damage;
- current stamina drain/recovery must NOT be treated as validated exertion/fatigue;
- they must not be used as evidence for biomechanics correctness;
- do not tune combat around the current bars;
- a future damage/fatigue phase must define explicit formulas and evidence before HP/stamina become authoritative gameplay state.

Until then, the authoritative outcome evidence remains the validated hit/block/miss/counter geometry and semantic combat events, not the visual HP/stamina percentages.

## Punch vocabulary design rule

> The right-thumb gesture should describe the intended punch trajectory; the body system should decide how well that punch can actually be executed.

This separates two problems cleanly:

- **Intent selection**: what punch family the player wants.
- **Physical execution**: which hand, range, body state, balance, timing, and recovery determine the quality/outcome.

### Canonical A2 mapping

| Gesture | Punch family | Physical intuition |
|---|---|---|
| TAP | STRAIGHT | direct line to target |
| HOLD + SWIPE UP | UPPERCUT | force travels upward |
| HOLD + SWIPE HORIZONTAL | HOOK | horizontal circular arc |
| HOLD + SWIPE DOWN | OVERHAND | hand travels over and down |

### Current A2 implementation freeze

- `HoldSeconds = 0.12s`
- TAP tolerates small accidental movement.
- Held swipe requires deterministic minimum travel.
- Horizontal wins when `absX >= absY * 0.85`.
- Otherwise positive vertical displacement = UPPERCUT.
- Otherwise negative vertical displacement = OVERHAND.
- Gesture classification never changes damage, stamina, balance or power.

### Hand-selection principle

A2 does not require a separate gesture vocabulary for lead/rear variants.

Architecture:

`gesture → punch family`

then:

`previous punch/body state → lead/rear hand`

Current minimal implementation:

- first STRAIGHT from guard → LEAD JAB
- follow-up STRAIGHT after lead hand → REAR CROSS
- HOOK/UPPERCUT alternate hand from prior requested punch state
- OVERHAND defaults to REAR OVERHAND

This selector is intentionally minimal and can be replaced later by stance/body-state logic without changing gesture vocabulary.

## Replayability principle

> Every meaningful combat action should produce replayable semantic data.

P1 punch semantic events include:

- timestamp
- exact punch type
- punch family
- selected hand
- player position
- opponent position
- distance at punch start
- movement intent at punch start
- head angle / head offset at punch start
- step state
- A1 authoritative straight-reach factor
- A3 family-coupling mode/factor where active
- diagnostic range/coordination values
- outcome: HIT / MISS / BLOCK
- counter status

Future fields may include impact point, glove velocity, body/hip rotation, balance, fall state, and camera state.

## P1-E0 — Instrumentation baseline

P1-E0 is closed PASS. It established punch-state semantic snapshots without changing combat outcomes.

## P1-A1 — Causal experiment

P1-A1 promotes only step direction into effective straight-punch reach.

Implementation PASS and real-device human learnability PASS.

## P1-A2 acceptance gate

Implementation evidence:

1. TAP deterministically resolves to STRAIGHT family.
2. HOLD + UP deterministically resolves to UPPERCUT family.
3. HOLD + HORIZONTAL deterministically resolves to HOOK family.
4. HOLD + DOWN deterministically resolves to OVERHAND family.
5. Small accidental TAP movement does not misclassify as a held swipe.
6. Hold threshold and directional boundary are deterministic.
7. Uppercut/overhand have visibly distinct trajectories.
8. Existing head/foot controls remain unchanged.
9. A1 reach affects only STRAIGHT family.
10. Gesture classification does not itself modify damage, power, balance, stamina or winner logic.

Human evidence status:

- all four gesture families intentionally triggered as expected on real iPhone: PASS.

## P1-B1 acceptance gate

Implementation evidence:

1. Out-of-range opponent target is clamped to finite reach.
2. In-range target is unchanged.
3. Opponent facing is frozen during an active punch.
4. Visible glove endpoint and hit-test endpoint are the same locked endpoint.
5. Existing opponent punch timing/radius remain unchanged.

Human evidence:

1. Retreating beyond opponent reach visibly causes a miss.
2. Opponent glove no longer appears detached/flying from the body.
3. When a hit lands, the player can visually understand why the punch reached.
4. The correction does not make all opponent punches trivially avoidable at normal fight range.

Current human verdict: PENDING FURTHER REVIEW.

## P1-A3.1 — Hook close-range coupling

Frozen experiment:

- `distance <= 1.05m` → `A3_FACTOR = 1.00`
- `1.05m < distance < 1.25m` → linear falloff
- `distance >= 1.25m` → `A3_FACTOR = 0.86`
- only HOOK family is affected
- distance is frozen at accepted punch start

Implementation classification: `P1-A3.1_IMPLEMENTATION_PASS`.

Human classification remains pending until close-vs-far hook behavior is explicitly judged on real device.

## P1-A3 acceptance principle

Do not create a universal `coordination_score → everything` mechanic.

Each punch family should earn its own physically interpretable coupling and causal test:

- step-in increases straight reach
- poor range weakens hook effectiveness
- uppercut requires close range / loaded position
- overhand rewards commitment but exposes recovery

P1-OBS now makes the physical reason for HIT/BLOCK/MISS and the A3.2/A3.3 intervention state auditable.

## Non-goals during current P1 slice

Do not add yet:

- career mode
- rankings
- equipment economy
- cosmetic shop
- KO sharing pipeline
- final damage model
- authoritative fatigue/stamina system
- skill tree
- production graphics
- native iOS optimization

KO/highlight generation remains a later consumer of the semantic combat stream, not a current implementation target.

## Decision rule

PROVE ONLY WHAT IS STILL UNCERTAIN → DECIDE → IMPLEMENT.

Do not deepen biomechanics when the current hit/reach reason cannot yet be inspected clearly. Preserve causal isolation and add observability before the next family coupling.
