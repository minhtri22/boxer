# P1-A1 — Step Direction → Effective Straight-Punch Reach

## Purpose

Test one narrowly-scoped whole-body coupling hypothesis:

> Step direction at punch start should create a small, learnable difference in effective reach for straight punches.

This experiment promotes exactly one previously-diagnostic variable into gameplay: categorical step state.

## Activated coupling

Applies only to:

- LEAD JAB
- REAR CROSS

Frozen factors:

- ADVANCING: `1.06x` forward endpoint reach
- NEUTRAL: `1.00x`
- RETREATING: `0.94x`

Step state is snapshotted when an accepted punch begins.

Only the forward (`z`) component of the straight-punch target pose is scaled.

## Explicit non-couplings

P1-A1 does **not** change:

- hooks
- punch timing
- punch radius
- block geometry
- counter rule
- damage / HP semantics
- stamina drain/recovery semantics
- footwork speed
- opponent AI
- coordination score behavior
- head movement behavior
- winner rule

`RangeFactor` and `CoordinationScore` remain diagnostic-only. The active A1 factor is separately emitted as `A1_REACH`.

## Deterministic acceptance

The synthetic suite must prove:

1. straight-punch ordering: `ADVANCING > NEUTRAL > RETREATING`
2. neutral keeps the exact P0 baseline endpoint
3. hooks remain unchanged for every step state
4. at the same synthetic distance boundary, advancing can connect where neutral cannot
5. at a nearer same-distance boundary, neutral can connect where retreating cannot
6. all existing 15 P0 tests still pass

Expected suite after adding A1 gates: `19/19 PASS`.

## Human UAT question

P1-A1 is not considered successful merely because geometry differs.

On the real interaction surrogate, the player must be able to learn and intentionally exploit:

> step in + jab/cross reaches farther; punching while retreating gives up reach.

Human UAT should compare repeated same-range attempts rather than general fight outcomes.

## Classification rule

- `P1-A1_IMPLEMENTATION_PASS`: compile + deterministic gates pass, no prohibited regressions.
- `P1-A1_HUMAN_LEARNABILITY_PASS`: player can intentionally exploit the coupling without being told exact numbers.
- `P1-A1_PASS`: both classifications above pass.

Do not open coordination/head/power/recovery coupling until A1 evidence is reviewed.
