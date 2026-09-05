# P1 — Whole-Body Boxing Mechanics Proof

## Status

P0 is closed for interaction feasibility. Safari/WebGL remains a surrogate-only delivery path; device-dependent stalls are a known non-blocking limitation and are not a P1 target.

P1 opens on branch `p1/whole-body-mechanics` from P0 artifact commit `076427513ee4da4cfa0f440e9c13805f4e1ed956`.

## Product thesis

> The player does not control a boxer from outside. The player is the boxer.

P1 must prove that Phone=Head, Left Thumb=Feet, and Right Thumb=Punch can behave as one coupled boxing body rather than three independent input channels.

## Primary P1 hypothesis

A player can improve punch quality by coordinating position, footwork, head/body state, punch timing, and recovery. Good coordination should produce measurably better range/quality/counter opportunities than disconnected or spammed input.

## Priority order

1. **P1-A — Whole-Body Punch Mechanics**
   - Couple footwork, head/body state, and punch execution.
   - Do not add new punch types.
   - Do not add career/progression systems.

2. **P1-B — Range / Balance / Recovery**
   - Position and movement direction must have tactical consequences.
   - Bad body state should reduce punch quality naturally rather than via arbitrary cooldowns.

3. **P1-C — Counter Geometry**
   - Correct evade direction/timing should create a geometrically meaningful counter opportunity.

4. **P1-D — Lightweight Opponent Attributes**
   - Reach/aggression/speed only, enough to force tactical adaptation.

Progression, ranking, shopping, social sharing, and KO clip generation are later layers. P1 only preserves the data needed to support them.

## Replayability principle

> Every meaningful combat action should produce replayable semantic data.

P1 combat events should be rich enough to later reconstruct or generate KO/highlight clips without re-architecting the combat core.

Minimum semantic fields for a player punch event:

- timestamp
- punch type / hand
- player position
- opponent position
- distance at punch start
- movement intent at punch start
- head angle / head offset at punch start
- whether the player is stepping in, neutral, or retreating
- quality score/components
- outcome: HIT / MISS / BLOCK
- whether it is a counter

Future fields may include impact point, glove velocity, body/hip rotation, balance, fall state, and camera state, but they are not required for P1-E0.

## P1-E0 — Instrumentation baseline

Before changing combat outcomes, instrument the current P0 mechanics so we can measure coordination without guessing.

### E0 must NOT change

- hit geometry
- block geometry
- counter rule
- punch animation timing
- movement speed
- stamina gating
- winner rule
- opponent AI

### E0 must add

For each accepted player punch, capture a semantic snapshot and compute diagnostic-only values:

- `distance_m`
- `move_forward`
- `move_lateral`
- `head_deg`
- `head_offset_m`
- `step_state` = ADVANCING / NEUTRAL / RETREATING
- `range_factor`
- `coordination_score`

`coordination_score` is diagnostic only in E0. It MUST NOT alter HIT/MISS/BLOCK, damage, speed, or recovery.

## Initial diagnostic model

The first model is intentionally simple and falsifiable.

- Advancement near punch start increases range contribution.
- Retreating reduces range contribution.
- Large lateral movement while punching reduces coordination.
- Moderate head displacement can coexist with a punch, but extreme displacement should score lower as an unstable state.

No claim is made that these are final boxing biomechanics. They are only a controlled P1 baseline for later A/B experiments.

## P1-A acceptance gate

P1-A does not pass because a formula exists. It passes only if real play shows that coordinated actions create repeatable, learnable advantages.

Minimum human evidence:

1. Player can intentionally create a better long-range straight by stepping in at the right time.
2. Player can intentionally avoid overextending while retreating.
3. Player can perceive a difference between a well-coordinated punch and a disconnected/spam punch.
4. The mechanic rewards timing/position rather than requiring memorization of hidden rules.
5. Existing P0 controls remain understandable.

## Non-goals

Do not add during P1-A:

- career mode
- rankings
- equipment economy
- cosmetic shop
- KO sharing pipeline
- real damage model
- fatigue system
- skill tree
- combo tree
- production graphics
- native iOS optimization

## Decision rule

PROVE ONLY WHAT IS STILL UNCERTAIN → DECIDE → IMPLEMENT.

If E0 telemetry cannot distinguish coordinated vs disconnected punches, revise the metric before making it authoritative. Do not tune combat around an unproven score.
