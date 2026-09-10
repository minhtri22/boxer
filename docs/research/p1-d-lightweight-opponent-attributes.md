# P1-D — Lightweight Opponent Attributes

## Frozen question

Can reach, aggression and hand speed create small, inspectable tactical differences while preserving the current opponent decision policy and all previously verified combat mechanics?

## Hypothesis

A single-variable profile change is sufficient to create a distinct tactical constraint without introducing RPG statistics, damage modifiers or punch-family bias.

## Frozen profiles

`BALANCED` is the exact existing baseline.

| Profile | Reach | Post-attack gap | Commit / Extend / Recover |
|---|---:|---:|---:|
| BALANCED | 1.00x | 1.00x | 1.00x |
| LONG_REACH | 1.08x | 1.00x | 1.00x |
| PRESSURE | 1.00x | 0.80x | 1.00x |
| FAST_HANDS | 1.00x | 1.00x | 0.90x duration |

Baseline authority remains head reach `1.02m`, body reach `0.96m`, attack gap `0.65–1.20s`, commit `0.34s`, extend `0.17s`, recover `0.48s`.

## Causal boundary

- Reach changes only the finite locked punch endpoint.
- Aggression changes only the random post-attack idle gap.
- Speed changes only opponent action phase durations. Presentation reads the same runtime duration as combat authority.
- Attack selection RNG, body/head selection, damage, stamina, punch radius, guard, counter geometry, player mechanics and initial onboarding delays remain unchanged.

## Acceptance gate

1. `BALANCED` reproduces every baseline constant exactly.
2. Each non-balanced profile changes exactly one attribute axis.
3. Reach increases monotonically for `LONG_REACH`.
4. Post-attack gap decreases monotonically for `PRESSURE`.
5. All three phase durations decrease monotonically for `FAST_HANDS`.
6. Values remain inside the frozen bounds encoded by the deterministic suite.
7. Profile identity and factors are visible in F3 diagnostics and semantic telemetry.
8. All existing 123 deterministic checks plus 8 P1-D checks pass.

Intermediate human UAT remains deferred to the single final integrated candidate with the approved UI/UX visual package.
