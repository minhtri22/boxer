# P1 — Whole-Body Boxing Mechanics Proof

## Status

P0 is closed for interaction feasibility. Safari/WebGL remains a surrogate-only delivery path; device-dependent stalls are a known non-blocking limitation and are not a P1 target.

P1 opens on branch `p1/whole-body-mechanics` from P0 artifact commit `076427513ee4da4cfa0f440e9c13805f4e1ed956`.

Current verified state:

- P1-E0 instrumentation: PASS
- P1-A1 step-to-straight-reach implementation: PASS
- P1-A1 human learnability: PENDING

## Product thesis

> The player does not control a boxer from outside. The player is the boxer.

P1 must prove that Phone=Head, Left Thumb=Feet, and Right Thumb=Punch can behave as one coupled boxing body rather than three independent input channels.

## Primary P1 hypothesis

A player can improve punch quality by coordinating position, footwork, head/body state, punch timing, and recovery. Good coordination should produce measurably better range/quality/counter opportunities than disconnected or spammed input.

## Revised priority order

1. **P1-A1 — Step Direction → Effective Straight-Punch Reach**
   - Existing controlled coupling experiment.
   - Only LEAD_JAB / REAR_CROSS are affected.
   - ADVANCING = 1.06x, NEUTRAL = 1.00x, RETREATING = 0.94x.
   - Implementation PASS; human learnability still required for full A1 PASS.

2. **P1-A2 — Punch Gesture Vocabulary**
   - Complete the right-thumb punch vocabulary before deeper body coupling.
   - Gesture should express punch trajectory/family, not force the player to memorize arbitrary short-vs-long swipe thresholds.
   - Proposed canonical mapping:
     - TAP → STRAIGHT
     - HOLD + SWIPE UP → UPPERCUT
     - HOLD + SWIPE HORIZONTAL → HOOK
     - HOLD + SWIPE DOWN → OVERHAND
   - Hand selection is a separate concern from punch-family selection. Gesture selects the family; stance/body state/previous punch may select lead vs rear hand.
   - A2 must be implemented and human-tested before adding family-specific whole-body mechanics.

3. **P1-A3 — Family-Specific Whole-Body Coupling**
   - Straight → step/reach coupling.
   - Hook → close-range + lateral/rotational coupling.
   - Uppercut → close-range + load/vertical-drive coupling.
   - Overhand → forward commitment + downward arc / guard-reading coupling.
   - Do not activate all couplings at once; each must be introduced as a controlled experiment.

4. **P1-B — Range / Balance / Recovery**
   - Position and movement direction must have tactical consequences.
   - Bad body state should reduce punch quality naturally rather than via arbitrary cooldowns.

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

The vocabulary must remain small enough to learn without a move list.

### Canonical A2 mapping

| Gesture | Punch family | Physical intuition |
|---|---|---|
| TAP | STRAIGHT | direct line to target |
| HOLD + SWIPE UP | UPPERCUT | force travels upward |
| HOLD + SWIPE HORIZONTAL | HOOK | horizontal circular arc |
| HOLD + SWIPE DOWN | OVERHAND | hand travels over and down |

### Hand-selection principle

A2 should not require a separate gesture vocabulary for lead/rear variants.

Preferred architecture:

`gesture → punch family`

then:

`stance + previous punch + body state → lead/rear hand`

Examples to validate later:

- first TAP from guard → lead jab
- follow-up TAP in valid timing → rear cross
- horizontal swipe → lead/rear hook based on stance and direction/context
- upward swipe → lead/rear uppercut based on stance/context
- downward swipe → usually rear overhand unless the state machine has a valid lead variant

This is a hypothesis to test, not a final combo system.

## Replayability principle

> Every meaningful combat action should produce replayable semantic data.

P1 combat events should be rich enough to later reconstruct or generate KO/highlight clips without re-architecting the combat core.

Minimum semantic fields for a player punch event:

- timestamp
- punch family
- selected hand
- gesture family
- player position
- opponent position
- distance at punch start
- movement intent at punch start
- head angle / head offset at punch start
- whether the player is stepping in, neutral, or retreating
- quality score/components
- outcome: HIT / MISS / BLOCK
- whether it is a counter

Future fields may include impact point, glove velocity, body/hip rotation, balance, fall state, and camera state.

## P1-E0 — Instrumentation baseline

P1-E0 is closed PASS. It established punch-state semantic snapshots without changing combat outcomes.

## P1-A1 — Current experiment

P1-A1 promotes only step direction into effective straight-punch reach.

A1 does not pass because a formula exists. Full A1 PASS requires real-device human evidence that the player can intentionally exploit step-in reach and perceive retreating as giving up reach.

## P1-A2 acceptance gate

A2 must prove the punch vocabulary is both complete enough and immediately learnable.

Minimum implementation evidence:

1. TAP deterministically resolves to STRAIGHT family.
2. HOLD + UP deterministically resolves to UPPERCUT family.
3. HOLD + HORIZONTAL deterministically resolves to HOOK family.
4. HOLD + DOWN deterministically resolves to OVERHAND family.
5. Short accidental movement around TAP does not misclassify as a held swipe.
6. Hold threshold and directional dead zones are deterministic and documented.
7. Existing head/foot controls remain unchanged.
8. Gesture classification does not itself modify damage, power, balance, or stamina.

Minimum human evidence:

1. Player can discover/remember the four gesture families without opening a move list repeatedly.
2. Player can intentionally request each family under combat pressure.
3. Misclassification is uncommon enough that the player trusts the control.
4. The vocabulary feels like punch trajectory, not arbitrary UI gestures.

A2 does not need final hand-selection sophistication to pass gesture-family learnability. A minimal deterministic hand-selection rule is acceptable during the experiment.

## P1-A3 acceptance principle

Do not create a universal `coordination_score → everything` mechanic.

Each punch family should earn its own physically interpretable coupling and causal test. Examples:

- step-in increases straight reach
- poor range weakens hook effectiveness
- uppercut requires close range / loaded position
- overhand rewards commitment but exposes recovery

These are future hypotheses, not frozen tuning values.

## Non-goals during P1-A

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

KO/highlight generation remains a later consumer of the semantic combat stream, not a P1-A implementation target.

## Decision rule

PROVE ONLY WHAT IS STILL UNCERTAIN → DECIDE → IMPLEMENT.

Do not deepen body mechanics on an incomplete punch vocabulary. Complete and prove A2 first, then activate family-specific mechanics one controlled coupling at a time.
