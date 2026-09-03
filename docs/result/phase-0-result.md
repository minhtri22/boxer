# Phase 0 — Result

## Decision

**OVERALL: BLOCKED**

The P0 micro-prototype is implemented and has passed deterministic, build and
synthetic runtime QA. Overall PASS is not authorized because the required
iPhone 12 / human interaction and replay-intent observation have not been run.

## P0-A — Phone = Head

**Status: BLOCKED**

Implemented:

- neutral device-attitude calibration;
- relative continuous lateral head input;
- dead zone, bound and smoothing;
- real head-collider/camera displacement;
- no authoritative dodge flag;
- synthetic editor/runtime fallback for diagnosis.

Verified synthetically:

- dead-zone and bound mapping;
- signed continuous head offset changes;
- head offset coexists with footwork and punches in runtime telemetry.

Missing decisive evidence:

- real iPhone motion behavior;
- human judgment that phone motion feels like head evasion;
- comfort observation.

## P0-B — Feet + Head + Fists

**Status: BLOCKED**

Implemented:

- left-thumb movement intent;
- phone/head channel;
- right-thumb jab/cross/hook gesture grammar;
- simultaneous runtime handling;
- automatic return to high guard;
- short commitment/recovery anti-spam behavior.

Synthetic runtime telemetry records non-zero movement, non-zero head offset and a
player punch in the same exchange, including a counter hit. This proves an
implementation path exists but does not establish human usability or cognitive
load on a phone.

## P0-C — Read → Evade / Block → Counter

**Status: BLOCKED**

Implemented:

- readable primitive opponent windup/commit/extend/recover states;
- mixed bounded-random opponent attacks;
- geometry-derived HIT / MISS / BLOCK;
- player head collider moves continuously with head input;
- opponent recovery opens vulnerability for a counter;
- synthetic runtime records `PLAYER_COUNTER_HIT` while the opponent recovery
  counter window is open.

Missing decisive evidence:

- human ability to visually read attacks without intrusive arrows;
- intentional human slip → counter execution on the target device;
- evidence that swipe spam is not behaviorally dominant;
- evidence that automatic guard does not make inactivity optimal.

## P0-D — Immediate Replay Intent

**Status: NOT TESTED**

The 75-second bout and end prompt are implemented, but no participant completed
the required session and answered:

> Do you want to fight again?

No replay-intent result is inferred from developer or synthetic testing.

## Acceptance gate

| Criterion | Current state |
| --- | --- |
| Phone movement feels connected to head evasion | NOT TESTED |
| No mandatory dodge button | IMPLEMENTED |
| Feet + head + fists in same exchange | SYNTHETIC VERIFIED; HUMAN NOT TESTED |
| Intentional slip → counter | SYNTHETIC PATH VERIFIED; HUMAN NOT TESTED |
| Incoming attacks readable without arrows | NOT TESTED |
| Swipe spam not obviously dominant | MECHANISM IMPLEMENTED; HUMAN NOT TESTED |
| Auto guard not inactivity-optimal | MECHANISM IMPLEMENTED; HUMAN NOT TESTED |
| Mapping understood after short instruction | NOT TESTED |
| No severe short-session discomfort | NOT TESTED |
| Player wants another bout | NOT TESTED |

## Evidence used

- Spec: `docs/specs/phase-0-boxer-micro-prototype-spec.md`
- QA: `docs/qa/phase-0-qa.md`
- Deterministic tests: `evidence/phase0/SYNTHETIC/deterministic-self-tests.txt`
- Windows runtime smoke: `evidence/phase0/SYNTHETIC/runtime-smoke-summary.txt`
- Concurrent-input telemetry: `evidence/phase0/SYNTHETIC/runtime-telemetry-excerpt.csv`
- Windows build metadata: `evidence/phase0/EDITOR/windows-build/build-metadata.txt`
- iPhone blocker: `evidence/phase0/REAL_DEVICE/blocker.md`

## Next authorized action

```text
NEXT AUTHORIZED ACTION = complete the missing P0 real-device / human validation
```

Do not begin P1 implementation or reusable engine extraction before the P0 gate
is actually satisfied.
