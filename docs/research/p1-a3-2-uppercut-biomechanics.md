# P1-A3.2 — Uppercut Base-Load → Vertical-Drive Coupling

## Question

What lower-body state can make uppercut execution causally distinct using input the player already controls?

## Constraint discovered before implementation

P1-C1 `forward_load` is a useful whole-body presentation state, but it is generated automatically from punch phase. It is therefore not a valid player-controlled causal variable for this experiment.

The smallest existing input-derived lower-body signal is the frozen punch-start `StepState`:

- `NEUTRAL` → `PLANTED` base proxy;
- `ADVANCING` or `RETREATING` → `MOVING` base proxy.

This is a gameplay abstraction of base readiness. It is not a claim that left-thumb input directly measures biological leg force or center of mass.

## Frozen hypothesis

For UPPERCUT only, a planted base should preserve the full rising glove path. Throwing while the base is translating should still execute, but with less vertical drive.

Frozen intervention:

- family: UPPERCUT only;
- `PLANTED` drive factor = `1.00`;
- `MOVING` drive factor = `0.85`;
- factor applies only to the vertical displacement from commit pose to target pose;
- target X and Z remain unchanged;
- punch timing, radius, damage, stamina, guard, counter, AI, A1 and A3.1 remain unchanged.

Formula:

```text
rise = target_y - commit_y

uppercut_target_y' = commit_y + rise * drive_factor
```

The moving case therefore reduces upward travel by 15% while preserving the same punch family, hand, horizontal aim and forward endpoint.

## Why this variable

The existing P1 design requires lower-body biomechanics to be derived from player input coordination rather than hidden animation-only state. `StepState` is already frozen at punch start, deterministic, replayable and under left-thumb control. It therefore provides a valid first causal probe without inventing a force simulation.

## Observability

The semantic P1 punch event records:

- `A32_MODE=UPPERCUT_BASE` for uppercuts;
- `A32_BASE=PLANTED|MOVING`;
- `A32_DRIVE=<factor>`.

P1-OBS already records punch family/hand, phase, step, body state, glove path, outcome reason and counter state, so the intervention can be audited without adding a second resolution path.

## Acceptance

1. Non-uppercuts are unchanged.
2. Neutral/planted uppercuts retain full vertical drive.
3. Advancing uppercuts receive the frozen moving-base factor.
4. Retreating uppercuts receive the same frozen moving-base factor.
5. Only vertical displacement changes; X/Z stay identical.
6. Lead/rear uppercuts use the same base-state rule.
7. The semantic event exposes mode/base/factor deterministically.
8. All prior 101 deterministic gates remain PASS.

Standalone human UAT is deferred by product-owner direction. This milestone may close as `DETERMINISTIC_PASS_UAT_DEFERRED`; final feel/readability is judged in the single integrated UAT candidate with approved UI/UX and visuals.
