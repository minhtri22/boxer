# Phase 0 — Web iPhone Surrogate Result

## Current decision

- **P0 Interaction:** CONDITIONAL / PARTIAL — real iPhone 12 Safari now provides meaningful interaction evidence, but punch legibility and bout closure require one targeted retest.
- **Web Delivery Quality:** DEGRADED / PROTOTYPE — sufficient for control validation; not representative of native visual/audio/haptic quality.
- **Native iOS Performance:** NOT TESTED

## Real-device evidence — first iPhone 12 Safari pass

The deployed Web surrogate successfully reached a real iPhone 12 through Safari without Apple Developer Program signing.

Observed directly by the tester:

- **Phone = Head:** PASS initial evidence. Moving the phone left/right produced perceivable continuous head movement.
- **Web delivery:** PASS for surrogate access; the prototype was playable enough to produce embodied-control feedback.
- **Punch input:** FUNCTIONAL but CONTROL LEGIBILITY PARTIAL. The tester could punch with the right thumb but could not clearly perceive how both lead and rear hands were being selected.
- **Character legibility:** FAIL/PARTIAL. Runtime primitives appeared visually ambiguous and magenta on the Web build, so player/opponent differentiation was insufficient.
- **Haptics:** no useful vibration was perceived when hit. This is treated as a Web-delivery limitation for now, not a P0 interaction failure. Native iOS haptics remain NOT TESTED.

This first real-device pass is enough to keep the Web surrogate path alive and justify one small control-legibility patch. It is not enough to declare final P0 PASS.

## Targeted P0 Control Legibility + Bout Closure patch

Branch: `p0/control-legibility-bout-closure`

The patch is intentionally narrow:

1. Keep `Phone = Head`.
2. Keep `Left Thumb = Feet`.
3. Keep `Right Thumb = Punch Controller`.
4. Resolve/display four explicit player punches: `LEAD JAB`, `REAR CROSS`, `LEAD HOOK`, `REAR HOOK`.
5. Make lead/rear glove selection visible and log the resolved punch label.
6. Use Web-safe runtime placeholder materials and strongly contrasting player/opponent colors.
7. Change the test bout to a fixed 90 seconds.
8. At 0 seconds, lock new attacks and show a minimal PLAYER/OPPONENT result summary.
9. Use a **P0 TEST-ONLY WIN RULE**: valid landed hits only. Counter hits are a reported subset and are not double-counted.

This temporary winner rule must not be carried forward as P1 judging architecture.

## Required second real-device test

| Test | Status |
| --- | --- |
| Phone = Head remains causal/usable | RETEST REQUIRED |
| Left-thumb footwork remains usable | RETEST REQUIRED |
| Right-thumb punch controller produces both lead/rear hands clearly | RETEST REQUIRED |
| LEAD JAB legible | RETEST REQUIRED |
| REAR CROSS legible | RETEST REQUIRED |
| LEAD HOOK legible | RETEST REQUIRED |
| REAR HOOK legible | RETEST REQUIRED |
| Player/opponent immediately distinguishable | RETEST REQUIRED |
| 90 s bout ends and attacks lock | RETEST REQUIRED |
| Result/log counters are coherent | RETEST REQUIRED |
| Tester can explain one HIT/MISS/BLOCK/COUNTER causally | RETEST REQUIRED |

No native iOS performance or haptic conclusion may be inferred from this surrogate.

## Gate after retest

After the second 2–3 minute iPhone test, stop feature work and run a **P0 Combat Review Gate**:

- review the current modules carefully,
- evaluate the prototype from the player's perspective,
- identify actual interaction gaps before authorizing art, audio, deeper physics, additional combat code, or P1 implementation.

## Next authorized action

`NEXT AUTHORIZED ACTION = local agent checks out p0/control-legibility-bout-closure, builds/deploys the Web artifact, then user runs the second real iPhone 12 Safari validation.`
