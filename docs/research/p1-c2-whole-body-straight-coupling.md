# P1-C2 — Whole-Body Straight Coupling

## Status

`FROZEN_FOR_IMPLEMENTATION`

## Unresolved question

Beyond A1 step-to-reach, which single consequence should be promoted first from the new whole-body state: reach, speed, recovery, power, or presentation?

## Decision / hypothesis

Promote **presentation only** first. A straight punch may gain one small extra torso-drive yaw as forward load moves from neutral to the C1 drive state. This should make jab/cross read as kinetic-chain actions without changing combat outcome authority.

## Frozen intervention

- Applies only to `PunchFamily.Straight`.
- Uses C1 `forward_load`.
- At `forward_load <= 0.50`, extra torso yaw is `0°`.
- At `forward_load >= 0.62`, extra torso yaw reaches `4°`.
- Between those values, interpolate smoothly.
- Lead/rear hands use equal/opposite yaw signs.
- Extra yaw is added to the C0 torso presentation yaw.
- Pelvis yaw remains exactly C0.

## Acceptance

1. Non-straight punch families receive zero C2 extra yaw.
2. Neutral/preload state receives zero C2 extra yaw.
3. Straight drive reaches exactly `4°` extra torso yaw at C1 drive endpoint.
4. Lead/rear straights are equal/opposite.
5. C0 pelvis yaw is unchanged by C2.
6. C2 output is finite, monotonic, and bounded.
7. All prior deterministic regressions remain PASS.

## Authority boundary

C2 must not change authoritative reach, target endpoints, timing, hit radius, damage, stamina, AI, guard, counter windows, A1 factors, or A3.1 hook factors.

## UAT policy

Standalone C2 UAT is deferred. C2 may advance as `DETERMINISTIC_PASS`; the visual judgment is part of the single final integrated UAT candidate.
