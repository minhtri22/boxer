# P1-B1.5R — Explicit Elbow / Arm Chain Readability

## UAT motivation

Real iPhone UAT on the previous B1.5 candidate produced:

- opponent preparation readability: PASS
- punch-family visual distinction: PASS
- A3.1 hook close-range feel: PASS
- anatomical arm continuity: FAIL/PARTIAL

Observed failure: gloves could still read as detached/flying because the elbow was not a visually explicit joint and the rendered chain did not guarantee a readable shoulder → upper arm → elbow → forearm → glove connection.

## Scope

This correction is visual/kinematic only. It must not alter authoritative combat geometry, hit outcomes, punch timing, punch radius, A1, A3.1, B1 opponent reach, stamina, damage/HP, counter logic, opponent AI, gestures, or winner logic.

## Frozen visual model

Each arm renders:

1. shoulder joint
2. upper-arm segment
3. explicit elbow joint
4. forearm segment
5. visual glove proxy

The original glove transform/collider remains authoritative for combat but its renderer is hidden. The visual glove proxy is driven by the anatomical chain and may be clamped inside the physical two-bone reach envelope.

Frozen visual lengths:

- upper arm: 0.34 m
- forearm: 0.31 m
- maximum visual reach: 0.65 m

The two-bone solver must preserve segment lengths within tolerance and clamp unreachable visual wrist requests instead of allowing a detached glove.

## Family articulation

- STRAIGHT: near-extension with a soft elbow, not hyperextension.
- HOOK: clearly bent elbow with lateral pole bias.
- UPPERCUT: lower elbow/load bias with upward forearm path.
- OVERHAND: raised elbow preparation with forward/downward arc.

Commit, extend and recover must preserve anatomical continuity.

## Deterministic gates

Dedicated `P1B15RSelfTests` covers:

- arm-length preservation
- anatomical max-reach clamp
- straight near-extension
- hook visible bend vs straight
- uppercut lower elbow
- overhand higher elbow
- mirrored left/right consistency

## Classification

Implementation may only be called `P1-B1.5R_IMPLEMENTATION_PASS` after Unity compile and all existing + dedicated tests pass.

Human classification remains pending until real-device UAT confirms:

1. elbow is readable,
2. glove no longer appears detached,
3. the chain remains continuous for all punch families.
