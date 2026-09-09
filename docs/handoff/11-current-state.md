# 11 — Current State

## Snapshot date

2026-09-09 autonomous execution snapshot. The last human-accepted binary remains artifact `4a985574f44d458357f875a1b7a6d3d00c7f9386`; subsequent whole-body milestones may advance on deterministic evidence and are deferred to one final integrated UAT.

## Repository

- Repo: `minhtri22/boxer`
- Active branch: `p1/whole-body-mechanics`
- Unity: `6000.5.8f1`
- Accepted WebGL artifact commit before this docs-only handoff commit: `4a985574f44d458357f875a1b7a6d3d00c7f9386`
- Source used for accepted binary: `4939e2bd5f44513d9d2ad3732ae07ac02f2ea148`
- Source short / build marker / productVersion / build_commit: `4939e2b`

## Verification status

| Milestone | Status |
|---|---|
| P0 controls / interaction surrogate | PASS |
| P1-E0 instrumentation | PASS |
| P1-A1 step→straight reach | PASS |
| P1-A2 punch gesture vocabulary | PASS |
| P1-B1 finite opponent reach / no homing | PASS |
| P1-A3.1 hook close-range coupling | HUMAN PASS |
| P1-B1.5 arm embodiment/topology/guard cleanup | HUMAN PASS for current milestone |
| P1-B2 opponent pelvis/leg embodiment | HUMAN PASS for current milestone |
| P1-C0 hip/torso rotation baseline | DETERMINISTIC PASS — UAT deferred |
| P1-C1 weight-transfer baseline | DETERMINISTIC PASS — UAT deferred |
| P1-C2 straight body coupling | DETERMINISTIC PASS — UAT deferred |

Latest human assessment: the version is substantially improved; hands are very good, legs are acceptable, and the user explicitly passed this milestone.

## Latest deterministic/build evidence

Latest P1-B1.5V+B2 implementation report recorded:

- Phase0: 22/22 PASS
- P1-A3.1: 4/4 PASS
- P1-B1.5R: 7/7 PASS
- P1-B1.5S: 5/5 PASS
- P1-B1.5T: 7/7 PASS
- P1-B1.5U: 6/6 PASS
- P1-B1.5V: 6/6 PASS
- P1-B2: 7/7 PASS
- prior combined: 64/64 PASS
- P1-C0: 7/7 PASS
- P1-C1: 7/7 PASS
- P1-C2: 7/7 PASS
- current combined: 85/85 PASS
- WebGL build succeeded
- data SHA256: `CEC7B56DB844F7B2D3C4CB7C8B7DF09B693CE0EB1B5113E91D0CD65E908CD381`
- wasm SHA256: `008E9737919271CC23C250C1279F2E7DF59C220AD171A895B107B0A5F95EDC68`

## Current body constants

Upper body:

- body height 1.80m
- shoulder height 1.43m
- shoulder width 0.38m
- upper arm 0.34m
- forearm 0.31m
- total arm 0.65m

Opponent lower body:

- pelvis height 0.92m
- hip width 0.26m
- thigh 0.46m
- shin 0.44m
- foot 0.22m

## Current scope boundaries

### Allowed next

- whole-body recovery;
- new deterministic instrumentation/tests for those questions.

### Still locked

- A3.2 uppercut body coupling;
- A3.3 overhand body coupling;
- final damage model;
- career/progression implementation;
- replay/KO clip generator;
- player legs in POV combat.

## Known limitations

- procedural primitive presentation is not production art;
- motion is still mechanically stiff compared with final boxing animation goals;
- current hit model is geometric, not force/velocity/anatomical rigid-body simulation;
- WebGL/iPhone remains a surrogate interaction/UAT path, not proof of native iOS performance;
- current HP/stamina semantics are provisional.

## Next decision gate

P1-C0 through P1-C2 are closed at deterministic level. Proceed to P1-C3 whole-body recovery and define one coherent return-to-guard contract across arm, torso, pelvis and stance state. Per product-owner direction, intermediate real-device UAT is deferred; the next human gate is the single final integrated candidate after approved UI/UX visual convergence.
