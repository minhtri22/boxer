# P1-A3.1 Real-Device UAT Note

## Device / candidate

- Device: iPhone 12-class real-device Safari surrogate
- Branch: `p1/whole-body-mechanics`
- Candidate source marker shown in overlay: `b9099cf`
- Candidate artifact: P1-A3.1 UAT build

## Human observations

### Gesture vocabulary

PASS.

All four requested punch families behaved as expected:

- TAP → STRAIGHT
- HOLD + UP → UPPERCUT
- HOLD + HORIZONTAL → HOOK
- HOLD + DOWN → OVERHAND

This is sufficient to record P1-A2 human control mapping as PASS for this tester/device.

### Straight step/reach coupling

PASS for human perceptibility.

The player reported a noticeable benefit from moving/stepping in before a straight punch. This is sufficient to record the P1-A1 human learnability question as PASS for this tester/device.

### Hook A3.1

Implementation is already deterministic PASS, but human close-vs-far hook judgment was not isolated clearly enough in this session to close A3.1 human learnability/effect.

Status: PENDING explicit close-range vs far-range hook UAT.

### Opponent punch embodiment / fair reach

Still requires additional review.

The opponent hand/reach presentation is improved enough for continued testing, but the tester still wants closer inspection before declaring the visual/hit fairness problem closed.

Status: P1-B1 human visual fairness PENDING.

## Product-flow note — onboarding

The current Head / Footwork / Punch / Guard / Counter sequence is a **new-player onboarding flow**, not something that should run before every bout.

Required product behavior later:

- complete the full onboarding once;
- persist onboarding completion;
- subsequent bouts start directly;
- optional Training / Practice / Controls access can replay drills deliberately;
- research builds may expose an explicit reset for UAT, but repeated onboarding must not be the normal product loop.

No persistence implementation is authorized by this note alone.

## Combat-state note — HP and stamina

Current HP and stamina bars are not authoritative physical models.

They are research HUD feedback only:

- HP is not yet backed by a validated damage formula;
- stamina is not yet backed by a validated fatigue/exertion formula;
- neither should be used to judge biomechanics or balance;
- neither should gate or tune P1 mechanics until a future explicit damage/fatigue model is specified and tested.

For current P1 evidence, authoritative outcomes remain HIT / MISS / BLOCK / COUNTER plus the geometric and semantic event data.

## Current classification after this UAT

- P1-A1 implementation: PASS
- P1-A1 human learnability: PASS
- P1-A2 implementation: PASS
- P1-A2 human gesture mapping: PASS
- P1-B1 implementation: PASS
- P1-B1 human visual fairness: PENDING FURTHER REVIEW
- P1-A3.1 implementation: PASS
- P1-A3.1 human learnability/effect: PENDING

## Recommended next evidence step

Before A3.2/A3.3:

1. explicitly retest close-vs-far hook behavior;
2. inspect opponent reach/hand fairness again;
3. add Combat Log + Biomechanics Inspector so hit reasons, target geometry, distance and punch trajectories can be audited directly.
