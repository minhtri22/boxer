# P1-C0 — Hip / Torso Rotation Baseline

## Status

`FROZEN_FOR_IMPLEMENTATION`

This experiment opens the first whole-body rotation baseline after the accepted P1-B2 body embodiment milestone.

## Unresolved question

Can a small, readable pelvis/torso rotation state make an opponent punch read as a whole-body action instead of arm-only motion without changing authoritative combat geometry?

## Hypothesis

A family-independent body rotation profile tied only to punch hand and action phase will improve visible whole-body coordination while preserving all current hit, timing, range, damage, AI and recovery authority.

## Frozen intervention

Only presentation changes.

- Guard: pelvis `0°`, torso `0°`.
- Commit: rotate smoothly from neutral to `65%` of the maximum amplitude.
- Extend: rotate smoothly from `65%` to `100%` of the maximum amplitude.
- Recover: rotate smoothly from `100%` back to neutral.
- Lead and rear hands use equal and opposite yaw signs.
- Maximum pelvis yaw: `7°`.
- Maximum torso yaw: `14°`.
- Punch family is intentionally ignored by this baseline.

The baseline is frozen for this experiment. Do not tune the angles to make UAT pass.

## Presentation scope

The rotation state may affect only visible presentation anchors:

- opponent decorative pelvis/shorts/waistband orientation;
- opponent visual shoulder anchors and arm-chain shoulder origin.

Authoritative glove transforms, opponent root facing, target lock, reach clamp, hit-test endpoints, punch timing and outcome resolution remain unchanged.

## Deterministic acceptance

1. Guard returns exactly to zero pelvis/torso yaw.
2. Lead and rear hands produce equal/opposite yaw.
3. Torso magnitude is greater than pelvis magnitude at non-zero amplitude.
4. Commit → Extend is monotonic to the frozen maximum.
5. Recover returns continuously to neutral.
6. The same hand produces the same body rotation for different punch families.
7. Rotation math remains finite and preserves local anchor radius.
8. All previously required P0/P1 regression suites remain PASS.

## Human UAT policy

Per explicit product-owner direction on 2026-09-09, no standalone C0 real-device UAT is required. C0 may advance on deterministic/regression evidence and will be judged only as part of the single final integrated UAT candidate with the approved UI/UX and visual package present.

Until that final integrated UAT, C0 status must remain `DETERMINISTIC_PASS` rather than `HUMAN_PASS`.

## Out of scope

- weight transfer;
- power/damage multipliers;
- straight or hook reach changes;
- punch-family-specific rotation;
- A3.2 uppercut biomechanics;
- A3.3 overhand biomechanics;
- player legs;
- opponent AI changes;
- career, replay or KO systems;
- production 3D rig/animation.
