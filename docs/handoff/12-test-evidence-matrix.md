# 12 — Test & Evidence Matrix

## Principle

`Synthetic PASS ≠ Human PASS`.

Use deterministic tests to prove regression and isolated mechanics. Use real-device UAT to prove readability/feel claims.

| Area | Deterministic | Build | iPhone UAT | Current verdict |
|---|---|---|---|---|
| P0 controls | PASS | PASS | PASS | PASS |
| A1 step→straight reach | PASS | PASS | accepted | PASS |
| A2 gesture vocabulary | PASS | PASS | accepted | PASS |
| B1 finite opponent reach/no homing | PASS | PASS | accepted | PASS |
| A3.1 hook range | PASS | PASS | PASS | HUMAN PASS |
| B1.5 arm topology/guard | PASS | PASS | PASS | HUMAN PASS |
| B2 opponent legs | PASS | PASS | PASS | HUMAN PASS |
| C0 hip/torso rotation | PASS | deferred | deferred to final integrated UAT | DETERMINISTIC PASS |
| C1 weight transfer | PASS | deferred | deferred to final integrated UAT | DETERMINISTIC PASS |
| C2 straight body coupling | PASS | deferred | deferred to final integrated UAT | DETERMINISTIC PASS |
| C3 whole-body recovery | PASS | deferred | deferred to final integrated UAT | DETERMINISTIC PASS |
| OBS combat log / biomechanics inspector | PASS | PASS (Unity batch) | deferred to final integrated UAT | DETERMINISTIC PASS |
| A3.2 uppercut base-drive coupling | PASS | PASS (Unity batch) | deferred to final integrated UAT | DETERMINISTIC PASS |
| A3.3 overhand recovery coupling | PASS | PASS (Unity batch) | deferred to final integrated UAT | DETERMINISTIC PASS |
| CG geometric counter opportunity | PASS | PASS (Unity batch) | deferred to final integrated UAT | DETERMINISTIC PASS |
| D lightweight opponent attributes | PASS | PASS (Unity batch) | deferred to final integrated UAT | DETERMINISTIC PASS |

## Latest suite counts

- Phase0: 22/22
- P1-A3.1: 4/4
- P1-B1.5R: 7/7
- P1-B1.5S: 5/5
- P1-B1.5T: 7/7
- P1-B1.5U: 6/6
- P1-B1.5V: 6/6
- P1-B2: 7/7
- P1-C0: 7/7
- P1-C1: 7/7
- P1-C2: 7/7
- P1-C3: 8/8
- P1-OBS: 8/8
- P1-A3.2: 7/7
- P1-A3.3: 7/7
- P1-CG: 8/8
- P1-D: 8/8
- Combined latest: 131/131

## Primary evidence locations

Research docs under `docs/research/`, including:

- `p1-a1-step-reach-experiment.md`
- `p1-a3-family-coupling-experiment.md`
- `p1-a3-1-build-result.md`
- `p1-a3-1-real-device-uat.md`
- `p1-b1-5r-explicit-elbow-chain.md`
- `p1-b1-5r-build-result.md`
- `p1-b1-5s-shoulder-contact-coherence-result.md`
- `p1-b1-5t-upper-arm-trace-cleanup-result.md`
- `p1-b1-5u-guard-pose-trace-final-result.md`
- `p1-b1-5v-b2-upper-cleanup-opponent-legs-result.md`
- `p1-obs-combat-observability.md`
- `p1-a3-2-uppercut-biomechanics.md`
- `p1-a3-3-overhand-biomechanics.md`
- `p1-counter-geometry.md`
- `p1-d-lightweight-opponent-attributes.md`
- `p1-d-lightweight-opponent-attributes-result.md`

Synthetic evidence under:

- `evidence/phase0/SYNTHETIC/`
- `evidence/phase1/SYNTHETIC/`

Web build evidence:

- `evidence/phase0/web-iphone/WEB_BUILD/build-metadata.txt`
- `builds/web/boxer-p0-web/`

## Evidence rule

Never write `HUMAN PASS` into a result document unless the user has actually performed and accepted real-device UAT for that candidate. On 2026-09-09 the product owner explicitly deferred standalone UAT for intermediate whole-body milestones. Only one final integrated UAT is required, after the approved UI/UX and visual package is present.
