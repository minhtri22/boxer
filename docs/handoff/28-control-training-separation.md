# Standalone control training / immediate combat

Owner request and date: 2026-10-08. Work is on the existing isolated
`feature/boxer-product-loop-wave1` branch. Gameplay source at implementation:
`2864812c75ddf4a7a2a97e5898a26a6ec28887d0`.
Light-guide source: `7a3525f2e8aabf008a505e2d41db1edbd529f461`.
Compiled source: `3a861a13872dcdb39d83adcb3f712d41f5b4f79c` (only the
versioned payload cache policy changed after the C# tests).

## Requested behavior

- Home START -> Preview ENTER RING -> Fight immediately, including the first
  bout. No tutorial countdown, unscored AI attacks or hidden waiting period.
- Home offers NEW? LEARN CONTROLS to an untrained player and PRACTICE CONTROLS
  once completion is remembered. Training is optional and always available.
- Dedicated practice screen has three lessons: phone tilt/head dodge; lower-left
  four-direction movement swipes; lower-right four punch families.
- Punch mapping remains the existing vocabulary: up = uppercut, down = overhand,
  horizontal left/right = rear/lead hooks, repeated taps = alternating straights.
  Swipe requires the existing 0.12-second hold; the UI says hold lightly, swipe
  and release. No hidden changes to gesture thresholds or combat balance.
- Progress requires observed input and accepted punches, not timer expiration.
  Both horizontal hook directions and two accepted taps are required. NEXT is
  enabled only after the current lesson is practiced. Completion returns Home,
  not Fight. Exit is available at any time and never marks incomplete training
  as completed. Versioned PlayerPrefs records genuine completion across reload.
- Training never starts CombatBout; enemy AI stays disabled throughout, and
  model HP/Stamina/Capacity and scored clock remain untouched. Player gloves
  animate during punch practice but practice is not a scored-contact test.
- Mobile/head sensor permission delay cannot block entry into a real bout.
  Head training explicitly offers recenter and Home/exit if motion is unavailable.
- Owner's additional light-guide request: native boxing-boot icon with up/down
  and lateral trails in the lower-left zone; human-head icon with alternating
  left/right trails at the top center; boxing-glove icon in the lower-right zone
  with down/up/right/left trails and a fixed pulsing tap target. Each example
  repeats every 2.4 seconds with a directional arrow and Vietnamese caption.
  They are presentation-only: no synthetic practice credit or gameplay input.
  No guide is rendered in a real bout, Home, Preview or Result. Native procedural
  textures are generated once (not per frame); no external raster asset is used.

## Verification status

Numerical/vitals/flow/light-guide tests: **105/105 PASS**. Existing P0/P1 regressions plus
Round2 **43/43** and P1V **8/8** reran with frozen prior evidence restored.
Controller runtime: **58 checks, EXIT=0**, including separate AI-disabled
practice, no advancement without observed actions, explicit practice exit,
first-bout immediate start, existing fatigue/KO wiring and ten reset rematches.
Contact consequences in that Editor harness remain injected, not physical UAT.
Unity Search startup again logged an index exception; no game C# error observed.
Light-guide WebGL build: **PASS**, Unity exit 0, 11.27-second final incremental
build, 60,907,364 reported bytes, all seven payload hashes verified. Native icon
and animation source built successfully first in 113.25 seconds; its output is
preserved in `_wave1-training-light-first-build`. Scene/ProjectSettings restore
is clean. The final template uses immutable Unity cache for full-commit-versioned
payload URLs; it does not ignore request failures or relax browser error gates.
Previous pre-light-guide WebGL build: **PASS**, Unity exit 0, 10.79-second incremental build,
60,887,380 reported bytes, all seven payload hashes verified. Scene and
ProjectSettings restore is clean. Initial full build at gameplay source also
succeeded; its output is preserved in `_wave1-onboarding-v2-initial-build`.
The previously deployed Wave1 output is preserved in
`_wave1-deployed-before-onboarding-v2`. The pre-light-guide browser run passed
22 functional checks but failed its strict load-error gate: one data/cache
transfer reported `net::ERR_ABORTED` around its deliberate reload. This is retained
as FAIL in `browser-before-light-guides`, not relabeled PASS. The current audit
waits for payload transfers before its deliberate reload and additionally
captures every animated guide, verifies that examples alone do not grant
progress, and checks that non-training screens hide the guides.
Final browser run: **28/28 PASS**, exit 0, zero JS/load/HTTP errors, matching
compiled product version. Real UI and synthetic touches completed all lessons;
all nine footwork/punch examples have rendered captures plus head and tap-pulse
captures. Completion persists across reload. The cleaned six-frame ICO loads
over HTTP with its expected hash. At portrait 540x960, inspected head/boot/glove
icons, captions, directional arrows/trails and pulse frames are readable and
do not overlap controls. This is not phone sensor or reference-fidelity Human
approval. Candidate is ready for the owner's authorized Pages deployment.
New evidence goes to `evidence/wave1/onboarding-v2`; previous Wave1 evidence
remains unchanged. Browser audit uses real UI and synthetic CDP touches, not
hidden StartBout injection; desktop head keys are not phone-sensor Human UAT.

## Scope retained / separate difficulty issue

Damage, fatigue coefficients, enemy reach, contact envelopes and AI selection
were not tuned by this change. Original accepted Ramirez and cleaned boxer
favicon are unchanged. Visual/contact/performance gates remain unresolved.

Read-only investigation of the owner's difficulty report found: BLOCK/MISS
still spends accepted-action resources; guarded contacts produce zero HP loss;
body-target opponent attacks exclude guard targets in Round2CombatRig. The
current seeded LCG selection plus one gap draw per attack deterministically
alternates selection 1 and 3 (head/body Cross), rather than all four choices.
Browser evidence from the prior candidate also had opponent HP 100 versus
player HP 3.38 at timeout after its straight-punch sequence. These findings
justify a separate combat-fairness fix, not a claim that skill or arithmetic
alone explains the owner's phone session. No balance/contact fix is included.
