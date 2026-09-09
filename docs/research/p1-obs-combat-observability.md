# P1-OBS — Combat Log + Biomechanics Inspector

## Question

What is the smallest observability layer required before P1-A3.2 Uppercut and P1-A3.3 Overhand can be tested causally?

## Frozen answer

Use one read-only `P1OBS_V1` observation schema and a bounded in-memory combat log.

The schema records, in stable order:

- actor;
- punch type, family and hand;
- action phase and normalized phase;
- punch-start distance and step state;
- pelvis yaw;
- base torso yaw and effective torso yaw;
- normalized forward load and presentation forward offset;
- head offset;
- authoritative glove hit-test path length;
- outcome;
- explicit hit-resolution reason;
- counter state.

The F3 developer panel shows the current biomechanics sample and recent combat-log lines. Existing arm-chain debug traces are enabled by the same developer toggle on non-mobile/editor builds.

## Authority boundary

P1-OBS is diagnostic only. It may read existing authoritative state and geometry, but it must not change:

- punch reach or endpoints;
- hit radius or collision rules;
- punch timing;
- damage or winner logic;
- stamina;
- guard or counter logic;
- opponent AI;
- A1 straight reach;
- A3.1 hook range coupling;
- C0/C1/C2/C3 mechanics.

Hit-resolution reason is produced by the same branch that already returns HIT/BLOCK/MISS. Adding the reason must not add a second resolver or reinterpret the result after the fact.

## In-memory log

The telemetry layer owns a bounded FIFO buffer of 32 semantic combat lines. This works even when WebGL intentionally disables persistent CSV writes. Starting a new bout clears the buffer.

The log contains outcome lines for both actors and full `P1_OBS` biomechanics lines for player punches.

## Acceptance

1. Family and hand are serialized deterministically.
2. Body-state fields reuse C0/C1/C2/C3 math.
3. Guard state resolves to the existing neutral whole-body contract.
4. Numeric observation fields remain finite.
5. Frozen body-state bounds are preserved.
6. Semantic serialization has a stable schema/key order.
7. Resolution reasons normalize to stable machine-readable tokens.
8. The in-memory log remains bounded FIFO and preserves newest events.
9. All prior deterministic regressions remain PASS.

No standalone human UAT is required. Product-owner direction defers human judgment to the single final integrated candidate with approved UI/UX and visual convergence.
