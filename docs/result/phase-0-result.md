# Phase 0 — Result

## Decision

**OVERALL: BLOCKED**

The P0 micro-prototype is implemented and has passed deterministic, build and
synthetic runtime QA. Overall PASS is not authorized because the required
iPhone 12 / human interaction and Control Comprehension & Agency observation have not been run.

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

## P0-D — Immediate Control Comprehension & Agency

**Status: NOT TESTED**

The decisive question is now:

> “Bạn có cảm thấy chính thao tác đầu/chân/tay của mình tạo ra kết quả vừa xảy ra không?”

The tester must then explain one exchange: why the attack hit, missed, was blocked,
or was countered. PASS requires a meaningful cause/effect explanation tied to the
tester’s own controls. Replay intent is not used as a P0 acceptance criterion.

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
| Player explains an exchange from their own control actions | NOT TESTED |

## Evidence used

- Spec: `docs/specs/phase-0-boxer-micro-prototype-spec.md`
- QA: `docs/qa/phase-0-qa.md`
- Deterministic tests: `evidence/phase0/SYNTHETIC/deterministic-self-tests.txt`
- Windows runtime smoke: `evidence/phase0/SYNTHETIC/runtime-smoke-summary.txt`
- Concurrent-input telemetry: `evidence/phase0/SYNTHETIC/runtime-telemetry-excerpt.csv`
- Windows build metadata: `evidence/phase0/EDITOR/windows-build/build-metadata.txt`
- iPhone blocker: `evidence/phase0/REAL_DEVICE/blocker.md`

## Web iPhone surrogate evidence

The native iOS path remains **NOT TESTED**. A Web-delivered surrogate is authorized
to test embodied interaction through Safari using real iPhone motion + touch.
Web delivery quality is reported separately from P0 interaction quality; Safari
loading/rendering overhead alone does not fail the interaction concept.

See `docs/specs/phase-0-web-iphone-surrogate-spec.md`,
`docs/qa/phase-0-web-iphone-qa.md`, and
`docs/result/phase-0-web-iphone-result.md`.

## Next authorized action

```text
NEXT AUTHORIZED ACTION = complete the missing P0 real-device / human validation
```

Do not begin P1 implementation or reusable engine extraction before the P0 gate
is actually satisfied.
