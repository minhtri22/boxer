# 04 — Combat Model

## Authoritative vs presentation

A critical architectural distinction:

- **Authoritative combat** decides hit/block/miss/counter/state.
- **Presentation** renders arms, legs, gloves, telegraphs, impact cues and body posture.

Never repair a visual mismatch by silently changing authoritative combat geometry.

## Punch pipeline

Current conceptual pipeline:

`Punch Intent → Start Snapshot → Commit → Extend → Geometric Resolve → HIT/BLOCK/MISS (+counter context) → Recover`

`P1PunchSnapshot` captures start-of-punch state including timestamp, intent, player/opponent positions, planar distance, movement forward/lateral, head degrees/offset, step state, diagnostic range factor and coordination score.

## Current hit model

The current combat model is **geometric**, not a full rigid-body physics simulation. Punch resolution uses an authoritative path/endpoint and hit volumes rather than physical glove-body collision dynamics.

This is intentional for the current research phase. Do not claim force/velocity/anatomical collision fidelity that is not implemented.

## P1-A1 — Step → Straight Reach

Frozen factors:

- `ADVANCING = 1.06`
- `NEUTRAL = 1.00`
- `RETREATING = 0.94`

Only straight forward extension changes. Non-straights, height/lateral aim, timing, radius, damage and other systems remain unchanged by A1.

## P1-A3.1 — Hook Close-Range Coupling

Frozen values:

- full effectiveness at distance `<= 1.05m`
- linear falloff between `1.05m` and `1.25m`
- far hook forward reach factor `0.86` at `>= 1.25m`

Only hook forward extension changes.

A3 mode semantic event: `HOOK_RANGE` for hooks; `NONE` for non-hooks.

## Opponent finite reach / no homing

Current opponent punch rules include:

- face player before commit;
- lock target at attack start;
- no active target tracking/homing during committed punch;
- finite head/body reach;
- visual endpoint and hit-test endpoint were intentionally brought into readable alignment for opponent attacks;
- retreat after commit can escape a locked endpoint.

Do not reintroduce homing by interpolation to the player's live position during the attack.

## Counter model

Counter behavior exists and is regression-protected. Treat it as an existing contract until a dedicated counter-geometry experiment is opened.

## Diagnostic vs gameplay fields

`RangeFactor` and `CoordinationScore` in the P1 snapshot remain diagnostic unless a later research task explicitly promotes them into authoritative gameplay.

## Locked biomechanics

- A3.2 Uppercut body coupling: LOCKED / NOT STARTED.
- A3.3 Overhand body coupling: LOCKED / NOT STARTED.

The next whole-body phase should first study hip rotation, weight transfer and recovery with isolated causal experiments rather than immediately adding family-specific multipliers.

## Semantic event principle

Every meaningful punch should remain describable as a semantic event containing at least:

- family/type;
- hand;
- step state;
- range/distance;
- head/movement snapshot;
- coupling factors;
- outcome;
- counter flag.

This is the seed for future replay/highlight generation.
