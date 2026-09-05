# P1-A3 — Family-Specific Whole-Body Coupling

## Decision

P1-A3 is executed as sequential causal sub-experiments, not one multi-variable patch.

The first activated experiment is **P1-A3.1 — Hook Close-Range Coupling**.

Uppercut load/vertical-drive and overhand commitment/recovery remain locked until A3.1 evidence is reviewed.

## Why start with hooks

A hook is the cleanest next family to couple because its core physical property is range: it is a close-range circular punch and should lose effectiveness when thrown from outside the pocket.

This can be tested without inventing hip rotation, shoulder torque, damage, stamina, or animation systems that do not yet exist.

## Hypothesis

> A hook thrown from close range should preserve its baseline trajectory, while a hook thrown from outside the natural pocket should give up forward extension and become easier to miss.

This should be learnable as a tactical rule:

- get close before hooking;
- do not use hooks as long-range straight punches.

## Frozen A3.1 mechanics

Only `HOOK` family is affected.

- distance at punch start `<= 1.05 m` → factor `1.00`
- distance at punch start `>= 1.25 m` → factor `0.86`
- between `1.05 m` and `1.25 m` → linear falloff from `1.00` to `0.86`

Only the hook target pose forward component (`z`) is scaled.

The distance is captured at accepted punch start and remains frozen for that punch.

## Explicit non-couplings

A3.1 does NOT change:

- straight punches;
- A1 step-reach factors;
- uppercut geometry;
- overhand geometry;
- punch timing;
- punch radius;
- damage / HP semantics;
- stamina;
- guard/block rules;
- counter window;
- footwork speed;
- opponent AI;
- opponent finite-reach B1 correction;
- winner rule;
- aggregate `CoordinationScore` gameplay authority.

`CoordinationScore` remains diagnostic-only.

## Semantic evidence

Every resolved player punch already emits `P1_PUNCH`.

A3.1 extends the semantic record with:

- `A3_MODE=HOOK_RANGE` for hooks;
- `A3_MODE=NONE` for all other families;
- `A3_FACTOR=<effective factor>`.

Examples:

```text
P1_PUNCH TYPE=LEAD_HOOK FAMILY=HOOK ... DIST=0.980 A3_MODE=HOOK_RANGE A3_FACTOR=1.000 ...
P1_PUNCH TYPE=REAR_HOOK FAMILY=HOOK ... DIST=1.300 A3_MODE=HOOK_RANGE A3_FACTOR=0.860 ...
P1_PUNCH TYPE=REAR_UPPERCUT FAMILY=UPPERCUT ... A3_MODE=NONE A3_FACTOR=1.000 ...
```

## Deterministic acceptance gates

Existing suite remains required:

- 22/22 existing P0/P1 A1/A2/B1 gates PASS.

New P1-A3.1 suite adds 4 gates:

1. close hook baseline remains exactly `1.00`;
2. far hook reaches frozen `0.86` factor;
3. non-hook families remain exactly unchanged;
4. same controlled geometry boundary is crossed by close hook but not far hook.

Combined expected deterministic evidence: **26/26 PASS**, represented as 22 existing gates + 4 dedicated A3.1 gates.

## Implementation classification

`P1-A3.1_IMPLEMENTATION_PASS` requires:

- Unity compile 0 errors;
- existing 22/22 regression PASS;
- A3.1 4/4 PASS;
- generated Web artifact metadata carries the A3.1 tag;
- no prohibited coupling/regression.

## Human learnability classification

`P1-A3.1_HUMAN_LEARNABILITY_PASS` requires real-device evidence that the player can intentionally exploit:

> close range → hook is viable; outside range → hook gives up reach.

Do not disclose exact 1.05 / 1.25 / 0.86 values during the learnability test.

## Full A3 status

A3 is NOT complete after A3.1.

Pending controlled sub-experiments:

- **P1-A3.2 — Uppercut Close-Range / Load Coupling**
- **P1-A3.3 — Overhand Commitment / Recovery Coupling**

These remain locked until A3.1 evidence is reviewed.

## Next observability step

After A3.1 build/UAT, implement the planned Combat Log + Biomechanics Inspector before deepening A3.2/A3.3. This avoids tuning anatomy/physics from an opaque placeholder model.
