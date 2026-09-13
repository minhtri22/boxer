# Boxer UAT Round 2 integrated candidate

STATUS: IMPLEMENTATION_PASS_PENDING_HUMAN_UAT

This is implementation/build acceptance with the evidence limits below, not
Human UAT PASS. M11 has desktop evidence only; target-device performance and
perceived boxing quality remain unverified until the integrated phone UAT.

STARTING_SHA: `795981cea39fb80220a25aba429a1ea5721f0497`

SOURCE_SHA: `725a21552b760c61ffffa67d91cc7ade9b29cb0d`

ARTIFACT_SHA: `337116cbaa786177c892ebb8a4806d494e42b3f6` (Git packaging commit)

ARTIFACT_MANIFEST_SHA256:
`fcd0bda420bd3d7e66abcd71e05c5f3c6c90400ead95efafc78c2c28a86de57a`

WASM_SHA256:
`0ad6191028bd6796f6a7c7001e361daaab4c273f0d68e46d3ca2bd71ff45a0f0`

## Root causes

- The old resolver tested a guard-to-intent-endpoint segment before the finite
  visible arm reached it. Baseline neutral jab endpoint exceeded available arm
  reach by 0.622574 m; a reproduced BLOCK at 1.4 m retained a 0.043358 m minimum
  visible surface gap.
- A separate embodiment writer solved another glove trajectory and discarded
  the action snapshot before recovery ended.
- Opaque P1-V arena/pose images and separately projected gloves hid or contradicted
  articulated world geometry. Decorative shoulder pieces duplicated joints;
  gold glove details belonged to a different representation. No projectile
  system was needed to explain that artifact.
- Root movement did not maintain boxing distance, and locally attached feet
  moved with the root without world-space planting. The legacy body target also
  failed to cover the visible upper chest.

## Candidates tested and A/B results

The full per-candidate HYPOTHESIS, IMPLEMENTATION_DELTA, EXPECTED_EFFECT,
CONTACT_RESULT, DISTANCE_RESULT, MOVEMENT_RESULT, REGRESSION_RESULT,
PERFORMANCE_RESULT, FAILURE_MODE and VERDICT are in
[the experiment report](../../../docs/handoff/23-round2-contact-optimization.md).

| Candidate | Experiment / prediction | Result | Decision |
|---|---|---|---|
| A: retain historical endpoint and force presentation to it | Fixed shoulder plus frozen bones must reach the old endpoint | Deterministic 0.622574 m excess; requires stretching/detachment or disproportionate neutral-jab torso translation | Reject this specific variant before runtime A/B |
| B: shared solved pose, point overlap per frame | Same visible volumes should remove phantom contacts cheaply | 1 false MISS, 0 false contacts / 1,860 cases | Reject sole point-sample authority |
| C: shared solved pose, relative sphere sweeps with bounded substeps | Preserve real volume crossings even at coarse rendered frame rates | 0 false MISS, 0 false contacts / same 1,860 cases | Select |

Selection corpus: five punch groups, three ranges, 31 lateral offsets,
15/30/60/120 FPS; independent 2,049-point reference. Separate final production
contact matrix: eight intents, three ranges, three defensive/moving-target
conditions, four frame rates = 288 cases, zero classification disagreements.
These results are finite-corpus evidence, not proof for every possible motion.

CONTACT_ARCHITECTURE_SELECTED / SELECTED_ARCHITECTURE:
shared anatomical pose plus relative swept-sphere contact with at most 1/240 s
pose intervals; earliest eligible surface wins. No physics queries or IK package.

CANDIDATES_TESTED: B and C run on the same corpus; A excluded by deterministic
geometry. The exclusion does not claim every conceivable authority-led model
is impossible.

WHY_SELECTED: C preserves B's finite-body geometry and one visible ownership
path while eliminating its measured coarse-sampling false MISS. Correctness
has priority over B's potentially lower sample cost.

REJECTED_CANDIDATES: A as specified above; B as sole overlap authority.

## Contact model before / after

CONTACT_MODEL_BEFORE: predictive intent segment, separate clamped visual wrist,
separate presentation images, incomplete torso contact representation.

CONTACT_MODEL_AFTER: render and resolver consume the same solved arm/body frame.
Glove radius is 0.115 m; head/chest spheres 0.18 m; abdomen 0.25 m. The visible
chest and authoritative target use the same overlapping sphere union. Moving
target/glove relative motion is swept during Extend; no future endpoint lookahead.
One outcome per punch; MISS only after no eligible contact through extension.
Counter-open guard bypass and body/head eligibility remain explicit combat rules.
Commit/recovery and non-scoring limbs are not damaging contact windows/targets.

CONTACT_TOLERANCE: 0.001 m verification allowance. Maximum measured midpoint
chord deviation was 0.000595322635 m at 1/240 s; add 0.00001 m numerical allowance
and remaining sampling margin. This is less than 0.9% of glove radius. Colliders
are not expanded by this tolerance. Contact is reported in its enclosing frame;
very coarse rendering can omit the exact subframe contact pose. Tests exercise
15–120 FPS and moving targets; the bound is not asserted for untested speeds.

## Distance, stance and footwork models

DISTANCE_BANDS / DISTANCE_MODEL:

- CLOSE: root separation below 0.775 m = hook forward extent 0.48 + head radius
  0.18 + glove radius 0.115.
- BOXING: 0.775–1.180 m. Outer guard-contact envelope = cross extent 0.67 +
  defender guard offset 0.28 + two glove radii 0.23.
- LONG: above 1.180 m. These are nominal geometry bands, not a promise that
  every family hits every target within BOXING. Solved yaw, offsets, family,
  step and target motion determine actual contact.
- AI maintains 0.955 m with a 0.045 m deadband; advance 0.36 m/s, retreat
  0.32 m/s, small lateral reposition at up to 0.10 m/s while idle. Root position
  and aim freeze after commitment. Long-reach opponent profile remains effective
  within the anatomical constraint.

FOOTWORK_MODEL: alternate world-space plant/gather; 0.18 s steps, 0.045 m lift,
lead/rear offsets 0.17 m and lateral base 0.22 m each side. One foot stays planted
while the other gathers. This is a small sparring motor, not sophisticated AI.

STANCE_MODEL: asymmetric compact guard, bent knees, supported pelvis, small
pelvis/torso/shoulder rotation and weight shift tied to punch phase; continuous
return to the same guard. Upper arm 0.34 m and forearm 0.31 m remain frozen.
Head/torso can move independently for existing head avoidance and future defense;
no new AI slip/roll research system was added.

## Visual ownership model

Controllers own input, root and phase. Round2Motion computes poses.
Round2CombatRig alone writes visible shoulders/arms/gloves/head/torso and resolves
contact. Round2Footwork alone writes pelvis/legs/feet. Shell details attach to
their anatomical owner. P1-V draws the existing HUD/control hierarchy; it no
longer overlays opaque full-body poses or independent projected gloves. Removed
shoulder/pose duplicates were proven redundant representations. The world ring,
dark/gold palette and foreground POV gloves remain. Camera pitch/FOV exposes
feet at the new physical range; artistic equivalence to the reference remains
a human judgment, and the procedural fighter is not a finished Ramirez asset.

## M01–M12 evaluation

PASS below means the stated implementation evidence passed, never human visual
acceptance. LIMITED explicitly identifies evidence that cannot support full
target-device or perceptual acceptance.

| Gate | Evaluation | Evidence / limit |
|---|---|---|
| M01 Combat distance | PASS, human perception pending | Geometry-derived bands; matrix; AI moves into range; final portrait/landscape canvas observed |
| M02 Stable stance | PASS, human perception pending | Asymmetric guard, bent knees, visible base; corrected Editor motion series and WebGL portrait |
| M03 Footwork | PASS | Advance/retreat/lateral/reset fixture; maximum planted-foot drift 1.88044154e-7 m; visible stepping |
| M04 Weight/body connection | PASS, readability pending | Phase-linked pelvis/torso/shoulder/arm chain and planted support; subtle procedural motion |
| M05 Contact coherence | PASS within tested corpus | Replacement invariant above supersedes old geometry authority; B/C and 288-case production matrix; no detached impact proxy |
| M06 Punch families | PASS, readability pending | Distinct straight/hook/uppercut/overhand paths; all eight intents in tests; manual browser rapid inputs are not claimed as eight trials |
| M07 Recovery | PASS | Phase-boundary continuity assertions, snapshot retained through recovery, runtime return to guard |
| M08 Defensive readiness | PASS for current scope | Usable guard, moving head/torso target paths, reposition preserves topology; full AI slip/roll is not implemented |
| M09 Visual stability | PASS within observed runs | One transform writer per component; no duplicate shoulder/detached gold glove artifact observed; no-homing drift exactly zero |
| M10 First-person composition | PASS for structural composition; visual fidelity LIMITED | Portrait feet/body exposed, prominent foreground gloves, existing HUD hierarchy and ring; procedural art and wide FOV still need human assessment |
| M11 Performance | LIMITED: desktop gates pass; iPhone and delta UNVERIFIED | Zero-byte hot-loop test, cheap custom sweeps; WebGL rolling average about 87–90 FPS; spikes retained; no comparable baseline or physical phone available |
| M12 Regression | PASS for automated/build scope | 131 historical + 8 P1-V + 43 new checks; WebGL live guard/body outcomes and winner; physical sensor/audio/haptic experience pending |

## Test results and performance results

TEST_RESULTS: 182/182 deterministic assertions PASS; 1,860 candidate comparisons;
288/288 production matrix agreements. Historical logs retained unchanged; old
numeric geometry results remain historical evidence, while new solved-wrist
assertions preserve A1 advance > neutral > retreat and A3.1 close > far. HP,
stamina, counter, timing, round and gesture semantics retain their existing paths.

Corrected Editor PlayMode fixture at d735703 (unchanged motion implementation
in final source): 1,177 actual frames / 22 seconds, 20 contacts, 7,525 samples,
mean rig update 0.05066 ms, max 4.91950 ms. Committed root/rotation drift zero.
An earlier callback-counting fixture was invalid and is documented as such;
its counts are not presented as frame performance.

Final-source microbenchmark: 100,000 pose-pair+sweep iterations, 153.193 ms,
zero managed bytes allocated. This isolated Editor test does not establish all
application allocations or browser render/physics cost.

Final-source desktop WebGL: observed Unity rolling averages 87–90 FPS;
sampled p95 12–13 ms; observed maximum up to 88 ms. See
[runtime scope and observations](webgl-runtime.md). DOM rAF is not Unity FPS.
No separate GPU/render or physics timing measurement was obtained.

PERFORMANCE_DELTA: UNMEASURED against starting source under matched conditions.
No improvement or no-regression claim is made for physical iPhone performance.

## Build provenance

Unity: 6000.5.8f1. Product version:
`r2-725a21552b760c61ffffa67d91cc7ade9b29cb0d`.

Source was committed before tests/build in the dedicated detached build checkout.
Output directory was absent before the final build; Unity Library cache was
reused. Scene/settings are generated by the committed build entrypoint. Runtime
source directories had no post-build diff; unrelated user scene/settings changes
in the main checkout were not used or staged. Tests and WebGL processes exited
zero. Build reported Succeeded, 94.3531859 s and 25,810,179 bytes.

[provenance.txt](provenance.txt) and [artifact-verification.json](artifact-verification.json)
record full file hashes, version and source. The copied deployment artifact
verified byte-identical. CRLF in raw generated evidence/provenance is preserved;
Git whitespace verification uses cr-at-eol, without modifying build bytes.

## Known limitations and handoff

- This is a procedural articulated boxer; production character likeness,
  polished arena art and subtle biomechanics remain visually simplified.
- Physical iPhone motion permission/sensors, audio/haptic perception and
  sustained device performance were unavailable for internal validation.
- Swept contact can occur between rendered frames; frozen non-scoring and
  counter rules remain explicit exceptions to generic visual-body intersection.
- Short hands, long hands, lateral offsets and guard vs head produce different
  effective distances within the nominal bands. No homing is used to guarantee hits.
- Desktop F3 diagnostics overlap and clip on narrow screens; they are opt-in
  developer output, not the normal UAT view.

UAT_URL: https://minhtri22.github.io/boxer/

Desktop-only synthetic alternative: https://minhtri22.github.io/boxer/?desktop=1

Local artifact: `builds/web/boxer-round2/index.html` (serve over HTTP; phone sensor
UAT should use the HTTPS URL). Deployment verification is recorded separately.

Deployment: [Pages run 34766523068](https://github.com/minhtri22/boxer/actions/runs/34766523068)
completed successfully from ARTIFACT_SHA. All seven publicly served files were
downloaded for in-memory SHA256 comparison and matched the tested local artifact;
see [deployed-verification.json](deployed-verification.json). The normal HTTPS
URL exposes the motion-permission start gate, and the explicit desktop URL
displays the correct full source marker. Final report/tool-only commits do not
change Unity source or the deployed artifact.

Public-host runtime smoke also completed Unity loading and rendered the active
training scene; the browser error-log query returned no JavaScript errors.

NEXT_STEP_IF_HUMAN_PASS: record the user's Round 2 acceptance against this exact
source/artifact and retain it as the accepted baseline. Any later product phase
requires its own scope; no PvP, secondary screens or replay work starts here.
