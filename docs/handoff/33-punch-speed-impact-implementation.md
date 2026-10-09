# Punch speed / impact implementation

Date: 2026-10-09. Approved plan: `32-punch-speed-impact-plan.md`.
Status: native implementation and compiled interaction/contact/UI/media gates verified; public release checks pending. Device acceptance remains `IMPLEMENTATION_PASS_PENDING_HUMAN_UAT`.

## Implemented

- Per-intent timing from approved trial B: jab 40/70/190 ms, cross 60/85/230, hook 70/95/260, uppercut 80/100/275, overhand 90/105/300. Recovery still multiplied by committed Quality and existing forward-footwork modifier.
- Strike cubic has nonzero terminal velocity; pull-back is fast first, then guard settle. Renderer and collision sampler carry the same actor-specific profile. Ramirez retains legacy curve and existing attributes, not the new player timing.
- Phase clock carries overshoot. Shared rig advances both actions, relative swept contact and phase-specific vitals in chronological slices <= 1/240 s, splitting at phase boundaries. No entire Extend disappears during a 200 ms render frame. Action IDs prevent sweeps between separate punches.
- Final timeout slice resolves contact before points adjudication; after KO the remaining timeline slices stop. No global time-scale freeze, reach/hitbox increase, damage increase or guard bypass.
- HIT/BLOCK event carries receipt, attacker, intent, contact point/direction, region and Quality. Layered deterministic transient/noise/body-tone SFX distinguish head, body and glove block. Whoosh on launch; MISS emits no hit thud or target reaction.
- Bounded native-rig reaction: head 1.5 degrees, torso .65 degrees, blocking glove 2 degrees. Glove center remains an IK target; bones re-solve after reaction. Player leather contact-pressure material pulse lasts 35 ms without moving its center/collider.
- Quick swipe threshold 60 ms; mappings unchanged, release still commits. Cancelled touch never creates a punch. Training uses same motion, unscored air practice only; guidance updated.
- No combo buffer, camera shake or new WebGL haptics. These are optional follow-ups in the plan, not needed to verify core motion. Native haptic behavior remains unchanged; browser vibration is not promised.

## Evidence and limits

- Native `punch-feel-tests.txt`: 14,741/14,741; includes 30/60/120 FPS, 200 ms spikes, 480 Hz comparison, real swept geometry, low-energy receipts, simultaneous actors and bounded deterministic SFX.
- Native vitals 111/111; fairness 172/172; POV geometry 550/550; controller 118 checks / exit 0. Controller audit includes deliberately injected consequences; it is not physical touch evidence.
- Compiled WebGL `w1-3035003caa96a96f8a1d3c347b43eb56c55a4b44`: fast-swipe/profile A/B 45/45, full contact 18/18, UI/training/POV lifecycle 33/33. All three report zero browser errors and the current product version. Actual touch attack records nine HIT contacts and PLAYER_WIN / KO, player HP 100, opponent HP 0; idle guard still takes body damage and loses on points. Tests are synthetic desktop browser interaction, not phone Human UAT.
- Local compiled media lifecycle 20/20, zero browser errors, same product version: intro freeze/cancel/epoch separation, real video/audio decoding, start/end bell, crowd cleanup, result/rematch. This does not establish phone speaker loudness or listening quality.
- Browser quick-swipe harness dispatches trusted touch start/move/end on a real-time schedule, not by waiting for every protocol acknowledgement. Observed overhand gestures: legacy 88 ms, curve 90 ms, fast 107 ms. First serial-input attempt failed its fast-input condition because acknowledgements stretched nominal 80 ms strokes to 162-233 ms; its failed report is retained in `feel-browser-serial-input-failure/report.json` and was not relabelled PASS.
- `releaseToAcceptMs` measures Unity-processed release to acceptance only. It does not measure physical phone, browser-event-to-Unity, or display latency. Ten accepted far-range punches in each profile remain MISS with opponent HP 100 and declining stamina/capacity; these are not evidence of sustained-contact balance or universal fairness.
- Motion samples use static roots, neutral footwork, analytic shared pose after IK. Jab peak 4.4369 -> 8.0669 m/s (1.8181x); cross 5.0911 -> 7.6228 (1.4973x). They are game-unit measurements, not physiological validation or measured phone rendering speed.
- Curve-only A keeps old durations and lowers peak jab/cross speed by about 9%; its purpose is to isolate easing, not to claim all new curves increase peak speed. Trial B provides the measured speed gain. Human feel must still be adjudicated from movement, input and contact.
- `motion-trace.csv`, `motion-summary.csv`, `motion-comparison.svg` contain legacy/curve/fast samples. These deterministic traces do not establish physical display/input latency.
- Baseline compiled WebGL clip/trace uses `w1-ef89d8cc0933fc455425af030f956b893e5e5bb1`. A legacy diagnostic URL in the new build shares the corrected timeline/feedback and is not an exact copy of the old deployed build.
- `clips/baseline-combat.mp4` and `clips/fast-combat.mp4` are silent compiled-WebGL keyboard captures (25 FPS recording, not measured game/device FPS). Raw WebM and read-only traces are retained under each labelled folder. Normal-speed clips are approximately trimmed around combat, not instrumented video/input synchronization. `fast-first-two-quarter-speed.mp4` is explicitly 0.25x playback for pose inspection. Rendered filmstrip shows extend / pull-back with continuous elbow/forearm silhouette and no obvious tearing in sampled poses; actual contact screenshots show HP changes. These samples do not establish every frame, physical impact or phone feel.
- Initial compile/regression failure retained in `initial-compile.log`: former 60ms-negative gesture assertion conflicts with the approved new contract. Changed that test explicitly to require 60ms-positive AND 30ms-negative, not to drop the check. Initial expanded-test compile failure CS0819 retained in `native-gates.log`; fixed declaration then reran full gates successfully in `native-gates-fixed.log`.
- Existing evidence outside this scope is preserved. Native audits write to new `evidence/wave1/punch-feel`, not over previous release reports.

## Research used

- [Atha et al., BMJ 1985](https://pmc.ncbi.nlm.nih.gov/articles/PMC1419171/?page=3), single professional-boxer experiment: short explosive travel/impact as reference only; no Newton-to-HP conversion.
- [Liu et al., Frontiers 2023](https://www.frontiersin.org/journals/physiology/articles/10.3389/fphys.2022.1099682/pdf): distinguish peak/contact velocity and consider body coordination. The game timing remains a testable design choice, not a physiological model.
- [Unity 6000.5 Time.timeScale](https://docs.unity.com/en-us/engine/6000.5/script-reference/unityengine/time/timescale): avoid global hit-stop changing unrelated time-dependent systems. This implementation leaves timeScale untouched.

## Release gate

- Compiled source: `3035003caa96a96f8a1d3c347b43eb56c55a4b44`; Unity 6000.5.8f1 WebGL build succeeded in 265.1438414 seconds, 62,103,878 bytes. Generated scene / project settings restored byte-identical after build.
- Damage, Quality and stamina/HP formulas are unchanged. Faster cadence and phase-accurate time integration can still alter bout balance; phone testing must check sustained combinations, fatigue, guard/body exposure and KO, not assume balance is unchanged because formulas match.
- Local media lifecycle and sampled rendered comparison completed. Pending: public twelve-payload/provenance verification and public media lifecycle. The exact feature branch alone is authorized for deployment; retain other Pages policies. Device UAT remains separate; no HUMAN_PASS claim from native or desktop checks.
