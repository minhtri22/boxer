# P1-C3 — Whole-Body Recovery Contract

## Status

`FROZEN_FOR_IMPLEMENTATION`

## Unresolved question

What does “return to guard” mean once arm, torso/pelvis rotation and weight-transfer state all exist?

## Hypothesis

Recovery can be defined without a new gameplay mechanic: the existing Recover phase is the shared clock, and every whole-body presentation state must converge continuously to one neutral guard state by the end of that phase.

## Frozen neutral contract

At Guard / Recover end:

- arm guard-return progress = `1.0`;
- pelvis yaw = `0°`;
- effective torso yaw = `0°`;
- C2 extra straight yaw = `0°`;
- forward load = `0.50`;
- presentation forward offset = `0 m`;
- stance/feet remain on their existing neutral anchors (no recovery teleport).

No new cooldown, stamina cost, hit rule, recovery penalty, or timing multiplier is introduced.

## Acceptance

1. Recover starts continuously from the Extend endpoint.
2. Recover progress is monotonic.
3. Recover end is exactly the frozen neutral contract.
4. Guard is exactly the same neutral contract.
5. Straight-specific C2 extra yaw fully disappears by neutral.
6. Weight/stance state returns to `0.50` and zero visual offset.
7. Composite recovery state is finite and bounded.
8. All prior deterministic regressions remain PASS.

## UAT policy

Standalone C3 UAT is deferred. C3 may advance as `DETERMINISTIC_PASS`; final feel/readability is judged in the single integrated UAT candidate.
