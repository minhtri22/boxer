# P1 — Whole-Body Boxing Mechanics Proof

## Status

P0 is closed for interaction feasibility. Safari/WebGL remains a surrogate-only delivery path; device-dependent stalls are a known non-blocking limitation and are not a P1 target.

P1 opens on branch `p1/whole-body-mechanics` from P0 artifact commit `076427513ee4da4cfa0f440e9c13805f4e1ed956`.

Current source state:

- P1-E0 instrumentation: PASS
- P1-A1 step-to-straight-reach implementation: PASS
- P1-A1 human learnability: PENDING REAL-DEVICE UAT
- P1-A2 punch gesture vocabulary: IMPLEMENTED, PENDING COMPILE/BUILD/HUMAN UAT
- P1-B1 opponent punch embodiment/fair reach correction: IMPLEMENTED, PENDING COMPILE/BUILD/HUMAN UAT

## Product thesis

> The player does not control a boxer from outside. The player is the boxer.

P1 must prove that Phone=Head, Left Thumb=Feet, and Right Thumb=Punch can behave as one coupled boxing body rather than three independent input channels.

## Primary P1 hypothesis

A player can improve punch quality by coordinating position, footwork, head/body state, punch timing, and recovery. Good coordination should produce measurably better range/quality/counter opportunities than disconnected or spammed input.

## Revised priority order

1. **P1-A1 — Step Direction → Effective Straight-Punch Reach**
   - Only LEAD_JAB / REAR_CROSS are affected.
   - ADVANCING = 1.06x, NEUTRAL = 1.00x, RETREATING = 0.94x.
   - Implementation PASS; human learnability still required for full A1 PASS.

2. **P1-A2 — Punch Gesture Vocabulary**
   - Complete the right-thumb punch vocabulary before deeper body coupling.
   - Canonical mapping:
     - TAP → STRAIGHT
     - HOLD + SWIPE UP → UPPERCUT
     - HOLD + SWIPE HORIZONTAL → HOOK
     - HOLD + SWIPE DOWN → OVERHAND
   - Gesture selects punch family only; hand selection is separate.
   - Current implementation uses deterministic minimal hand selection from previous punch state; OVERHAND defaults to rear hand.
   - New player trajectories exist for uppercut and overhand.
   - Existing A1 step-reach coupling remains authoritative only for straights.

3. **P1-A3 — Family-Specific Whole-Body Coupling**
   - Straight → step/reach coupling.
   - Hook → close-range + lateral/rotational coupling.
   - Uppercut → close-range + load/vertical-drive coupling.
   - Overhand → forward commitment + downward arc / guard-reading coupling.
   - Do not activate all couplings at once; each must be introduced as a controlled experiment.

4. **P1-B — Range / Balance / Recovery**
   - Position and movement direction must have tactical consequences.
   - Bad body state should reduce punch quality naturally rather than via arbitrary cooldowns.

   **P1-B1 — Opponent Punch Embodiment & Fair Reach** is pulled forward as a corrective prerequisite after human observation that the opponent glove could visually “fly” toward a retreating player while still registering a hit.

   B1 rule:
   - opponent punch target is locked at commitment;
   - opponent must not home/rotate after punch commitment;
   - punch endpoint is clamped to a finite physical reach;
   - the same clamped endpoint drives both the visible glove trajectory and hit resolution;
   - if the player retreats beyond that physical endpoint, the punch must visibly and logically miss.

5. **P1-C — Counter Geometry**
   - Correct evade direction/timing should create a geometrically meaningful counter opportunity.

6. **P1-D — Lightweight Opponent Attributes**
   - Reach/aggression/speed only, enough to force tactical adaptation.

Progression, ranking, shopping, social sharing, and KO clip generation are later layers. P1 only preserves the data needed to support them.

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
- diagnostic range/coordination values
- outcome: HIT / MISS / BLOCK
- counter status

Future fields may include impact point, glove velocity, body/hip rotation, balance, fall state, and camera state.

## P1-E0 — Instrumentation baseline

P1-E0 is closed PASS. It established punch-state semantic snapshots without changing combat outcomes.

## P1-A1 — Current causal experiment

P1-A1 promotes only step direction into effective straight-punch reach.

A1 does not pass because a formula exists. Full A1 PASS requires real-device human evidence that the player can intentionally exploit step-in reach and perceive retreating as giving up reach.

A2 and B1 source corrections may be present in the same future UAT build, but A1 human evaluation must remain a separate question: can the player intentionally exploit step-in/retreating straight reach?

## P1-A2 acceptance gate

Minimum implementation evidence:

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

Minimum human evidence:

1. Player can discover/remember the four gesture families without repeatedly opening a move list.
2. Player can intentionally request each family under combat pressure.
3. Misclassification is uncommon enough that the player trusts the control.
4. The gesture feels like punch trajectory rather than arbitrary UI input.

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

## P1-A3 acceptance principle

Do not create a universal `coordination_score → everything` mechanic.

Each punch family should earn its own physically interpretable coupling and causal test:

- step-in increases straight reach
- poor range weakens hook effectiveness
- uppercut requires close range / loaded position
- overhand rewards commitment but exposes recovery

These are future hypotheses, not frozen tuning values.

## Non-goals during current P1 slice

Do not add yet:

- career mode
- rankings
- equipment economy
- cosmetic shop
- KO sharing pipeline
- final damage model
- fatigue system
- skill tree
- production graphics
- native iOS optimization

KO/highlight generation remains a later consumer of the semantic combat stream, not a current implementation target.

## Decision rule

PROVE ONLY WHAT IS STILL UNCERTAIN → DECIDE → IMPLEMENT.

Do not deepen body mechanics on an incomplete punch vocabulary. Complete and prove A2 first, then activate family-specific mechanics one controlled coupling at a time.
