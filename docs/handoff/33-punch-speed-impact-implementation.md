# Punch speed / impact implementation

Date: 2026-10-09. Approved plan: `32-punch-speed-impact-plan.md`.
Status: native implementation verified; WebGL / release checks pending.

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
- Motion samples use static roots, neutral footwork, analytic shared pose after IK. Jab peak 4.4369 -> 8.0669 m/s (1.8181x); cross 5.0911 -> 7.6228 (1.4973x). They are game-unit measurements, not physiological validation or measured phone rendering speed.
- Curve-only A keeps old durations and lowers peak jab/cross speed by about 9%; its purpose is to isolate easing, not to claim all new curves increase peak speed. Trial B provides the measured speed gain. Human feel must still be adjudicated from movement, input and contact.
- `motion-trace.csv`, `motion-summary.csv`, `motion-comparison.svg` contain legacy/curve/fast samples. These deterministic traces do not establish physical display/input latency.
- Baseline compiled WebGL clip/trace uses `w1-ef89d8cc0933fc455425af030f956b893e5e5bb1`. A legacy diagnostic URL in the new build shares the corrected timeline/feedback and is not an exact copy of the old deployed build.
- Initial compile/regression failure retained in `initial-compile.log`: former 60ms-negative gesture assertion conflicts with the approved new contract. Changed that test explicitly to require 60ms-positive AND 30ms-negative, not to drop the check. Initial expanded-test compile failure CS0819 retained in `native-gates.log`; fixed declaration then reran full gates successfully in `native-gates-fixed.log`.
- Existing evidence outside this scope is preserved. Native audits write to new `evidence/wave1/punch-feel`, not over previous release reports.

## Research used

- [Atha et al., BMJ 1985](https://pmc.ncbi.nlm.nih.gov/articles/PMC1419171/?page=3), single professional-boxer experiment: short explosive travel/impact as reference only; no Newton-to-HP conversion.
- [Liu et al., Frontiers 2023](https://www.frontiersin.org/journals/physiology/articles/10.3389/fphys.2022.1099682/pdf): distinguish peak/contact velocity and consider body coordination. The game timing remains a testable design choice, not a physiological model.
- [Unity 6000.5 Time.timeScale](https://docs.unity.com/en-us/engine/6000.5/script-reference/unityengine/time/timescale): avoid global hit-stop changing unrelated time-dependent systems. This implementation leaves timeScale untouched.

## Release gate

Pending: compiled fast-swipe/profile A/B browser tests, complete UI/media/contact regression, rendered comparison, build provenance and public payload verification. Device UAT remains separate; no HUMAN_PASS claim from native or desktop checks.
