# P1-CG — Geometric Counter Opportunity

## Question

Does a successful evade create a counter opportunity for a geometric reason, rather than every opponent recovery phase being treated as a counter window?

## Existing ambiguity

Before this milestone, `CounterWindowOpen` was equivalent to `opponent phase == Recover`. A player hit during any opponent recovery could therefore be labelled a counter even when no preceding evade created the opening.

## Frozen hypothesis

Arm one counter opportunity only when all conditions hold:

1. the opponent attack outcome is `MISS`;
2. the locked punch segment intersected the target center captured at commit;
3. the same segment does not intersect the target center at resolution;
4. target displacement is at least `0.08m`;
5. the movement is attributable to head offset or player-root footwork.

Classification:

- head attack with absolute head-offset change at least `0.08m` → `HEAD_EVADE`;
- otherwise player-root displacement at least `0.08m` → `FOOTWORK_EVADE`;
- body attacks cannot create `HEAD_EVADE`.

The opportunity becomes active only while the opponent is in Recover. A successful player counter consumes it. It clears when recovery ends, combat is disabled, or the opponent begins another attack.

## Authority boundary

P1-CG does not change:

- opponent target lock or finite reach;
- punch endpoints, radii or HIT/BLOCK/MISS resolution;
- player punch geometry;
- damage, stamina or winner logic;
- opponent AI choice/timing;
- A1, A3.1, A3.2 or A3.3.

It changes only whether an already-valid player HIT receives counter context and whether opponent guard is considered open for that causally-created opportunity.

## Observability

When armed, telemetry emits:

`P1_COUNTER_OPPORTUNITY TYPE=<HEAD_EVADE|FOOTWORK_EVADE> START_SEP=<m> END_SEP=<m> MOVE=<m>`

The F3 inspector displays `CLOSED`, `ARMED_<type>` or `OPEN_<type>`.

## Acceptance

1. A target that was already outside the path at commit creates no opportunity.
2. A target that remains intersected creates no opportunity.
3. A head target moved from hit to miss by sufficient head offset creates `HEAD_EVADE`.
4. Sufficient root movement creates `FOOTWORK_EVADE`.
5. Body attacks cannot be classified as head evades.
6. HIT and BLOCK outcomes create no opportunity.
7. An armed opportunity is active only during Recover and is consumed once.
8. Semantic serialization is deterministic.
9. All prior 115 deterministic gates remain PASS.

Standalone human UAT is deferred by product-owner direction. Human readability and learnability are judged once in the final integrated candidate with approved UI/UX and visuals.
