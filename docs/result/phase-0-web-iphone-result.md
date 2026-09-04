# Phase 0 — Web iPhone Surrogate Result

## Current decision

- **P0 technical integration:** PASS for the Web surrogate path.
- **Real iPhone 12 Safari surrogate:** PASS for meaningful interaction testing.
- **Core control channels:** WORKING.
- **Control comprehension:** PASS initial evidence, with a noticeable learning burden for a new player.
- **Web Delivery Quality:** GOOD ENOUGH FOR INTERACTION VALIDATION; not representative of native visual/audio/haptic quality.
- **Native iOS Performance:** NOT TESTED.
- **P1 implementation:** LOCKED pending the onboarding experiment and P0 Combat Review Gate.

## Real-device evidence — second iPhone 12 Safari pass

The control-legibility/bout-closure branch was built, deployed and tested on a real iPhone 12 in Safari.

Observed directly by the tester:

- **Phone = Head:** works and is perceivable.
- **Left Thumb = Feet:** works.
- **Right Thumb = Punch Controller:** works, with explicit `LEAD JAB`, `REAR CROSS`, `LEAD HOOK`, `REAR HOOK` legibility.
- **Player/opponent differentiation:** PASS after the packaged project-owned Web shader fix; the prior magenta fallback is resolved.
- **HIT/BLOCK/COUNTER chain:** produces meaningful runtime events/results.
- **Bout closure:** the 90-second test bout completed and produced PLAYER/OPPONENT stats and a test-only winner.
- **Web performance:** tester reported the browser experience as fairly smooth for this proof.
- **Learning burden:** tester reported that the control initially felt uncomfortable/overloaded because several embodied channels must be learned together.

This establishes that the interaction concept works technically on the real target phone through the surrogate. It does **not** yet establish final fun/depth or production readiness.

## Player-review findings

1. **Input ergonomics — MODIFY:** the combined control model is initially overloaded for a new player. Do not redesign the control model yet; teach each channel separately first.
2. **Combat readability — CONDITIONAL PASS:** the tester can see opponent telegraphs, but a new player tends to rush in and spam punches instead of reading the opponent.
3. **Agency / causality — MODIFY VIA TRAINING:** causal understanding should be taught before judging a new player's bout behavior.
4. **Skill expression — PROMISING, NOT PROVEN:** once familiar, the tester expects different personal play styles may emerge.
5. **Fun potential — UNRESOLVED:** no final fun verdict is authorized from the current prototype.

## Current experiment — onboarding micro-drills + short bout

Continue on branch: `p0/control-legibility-bout-closure`.

Do not change the locked control model:

- `Phone = Head`
- `Left Thumb = Feet`
- `Right Thumb = Punch Controller`

Add only a lightweight onboarding sequence before the bout:

1. **HEAD CONTROL** — move the phone/head left and right.
2. **FOOTWORK** — demonstrate left/right/forward/back with the left thumb.
3. **PUNCHES** — demonstrate `LEAD JAB`, `REAR CROSS`, `LEAD HOOK`, `REAR HOOK` with the right-thumb punch controller.
4. **GUARD** — stop punching to return to automatic high guard and record two blocks.
5. **COUNTER** — read/evade-or-block/counter and record one counter.
6. **SHORT BOUT** — 45 seconds, with onboarding counters reset before `BOUT_START`.

Each drill has a short timeout so onboarding cannot permanently block the tester. Completion is objective-driven where possible. This is a P0 learning-curve experiment, not production tutorial architecture.

## Retest questions

After onboarding + the 45-second bout, ask only:

1. Does the combined control still feel overloaded after learning the channels separately?
2. Does the player begin to wait/read the opponent rather than immediately spam punches?
3. After repeated short bouts, does improvement feel skill-based rather than merely familiarity with controls?

## Stop rule

After this experiment is built/deployed/tested:

- do not add production art,
- do not add audio polish,
- do not start haptic research,
- do not add stamina/damage/KO,
- do not add advanced AI,
- do not begin P1 implementation.

Run the **P0 Combat Review Gate** first and decide `KEEP / MODIFY / REJECT` module-by-module.

## Next authorized action

`NEXT AUTHORIZED ACTION = local agent pulls the current p0/control-legibility-bout-closure branch, runs Unity compile/self-tests, rebuilds the tracked Web artifact, pushes the build back to the same branch, deploys it, then the user performs the onboarding + 45-second iPhone Safari retest.`
