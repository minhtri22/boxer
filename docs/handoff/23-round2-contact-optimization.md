# Round 2 contact / embodiment optimization

Starting point: `795981cea39fb80220a25aba429a1ea5721f0497`.
Authority: the user's Autonomous Contact / Embodiment Optimization Amendment.
This supersedes the old numeric geometry freeze for Round 2 only. Historical
results under `evidence/uat-round2` and the P1 suites are preserved.

## Root causes

1. Controllers resolved a guard-to-intent-endpoint segment before the visible
   finite arm could reach it. Baseline jab excess was 0.622574 m; the reproduced
   BLOCK at 1.4 m had a minimum visual surface gap of 0.043358 m.
2. ArmVisualEmbodiment recomputed a second trajectory, including a different
   idle guard and a snapshot lifetime that ended before recovery finished.
3. P1-V's opaque arena and full-body pose crossfades hid articulated geometry.
   Separate projected glove images contradicted world-space trajectories.
4. Decorative shoulders duplicated anatomical shoulder joints. Gold glove
   decorations lived under the hidden authoritative glove, while another glove
   was visible. This was a competing representation, not proof of a projectile
   system. There is no projectile used by the new contact model.
5. Opponent roots did not maintain range; local-space feet followed root motion
   without world-space planting. The old body target did not cover the visible
   upper torso, so simply moving glove contact to that target was insufficient.

## Controlled selection

The same finite arm pose and target corpus were used for B and C. Reference:
2,049 independent point samples per trajectory, 5 punch groups, 3 ranges,
31 lateral offsets and 15/30/60/120 FPS: 1,860 cases.

### A — preserve historical endpoint, move presentation to it

- HYPOTHESIS: relocate the visible glove to the old resolver endpoint.
- IMPLEMENTATION_DELTA: remove visual wrist clamping or translate its shoulder.
- EXPECTED_EFFECT: preserve historical numerical contact distances.
- CONTACT_RESULT: deterministic 0.622574 m reach excess already disproves a
  fixed-shoulder implementation with frozen 0.34/0.31 m bones.
- DISTANCE_RESULT: retains the previously demonstrated false BLOCK distance.
- MOVEMENT_RESULT: requires arm stretching, detached shoulder or approximately
  an entire extra arm-length of torso translation for a neutral jab.
- REGRESSION_RESULT: contradicts frozen anatomical proportions/stance.
- PERFORMANCE_RESULT: not benchmarked; correctness exclusion precedes timing.
- FAILURE_MODE: anatomical inconsistency.
- VERDICT: rejected before A/B; no artificial runtime prototype.

### B — anatomical pose with instantaneous sphere overlap

- HYPOTHESIS: sampling the visible glove at rendered frames is sufficient.
- IMPLEMENTATION_DELTA: one shared arm pose; overlap current glove/target spheres.
- EXPECTED_EFFECT: remove false pre-contact outcomes cheaply.
- CONTACT_RESULT: 1 false MISS, 0 false contacts in the 1,860-case corpus.
- DISTANCE_RESULT: uses the same anatomical envelope as C.
- MOVEMENT_RESULT: body/feet/pose ownership can be shared with C.
- REGRESSION_RESULT: a visible grazing contact is lost at coarse frame intervals.
- PERFORMANCE_RESULT: fewer samples than C; rejected on the higher-priority
  coherence criterion, so a performance advantage cannot decide selection.
- FAILURE_MODE: temporal aliasing/tunnelling.
- VERDICT: rejected as sole contact authority.

### C — shared anatomical pose, relative swept spheres, bounded substeps

- HYPOTHESIS: sweep the actual solved glove and moving target volumes between
  bounded pose samples; resolve the earliest contact without look-ahead.
- IMPLEMENTATION_DELTA: pure Round2Motion, shared Round2Frame and one
  Round2CombatRig. Sample interval at most 1/240 second during extension.
- EXPECTED_EFFECT: coherent contact with no phantom long reach and no low-FPS
  missed crossings; no runtime IK package or physics queries.
- CONTACT_RESULT: 0 false MISS, 0 false contacts in the selection corpus.
  The separate contact matrix covers both hands, head/torso/guard targets and
  moving targets; its CSV and summary are the validation authority.
- DISTANCE_RESULT: CLOSE < 0.775 m; BOXING 0.775–1.180 m; LONG > 1.180 m.
  Not every family hits at every BOXING distance: outer range includes guard;
  scoring head contact generally requires being nearer than guard contact.
- MOVEMENT_RESULT: world-planted alternating feet and captured opponent aim.
- REGRESSION_RESULT: 42 initial new invariant checks passed; historical 131+8
  passed. Final committed-source results are recorded separately.
- PERFORMANCE_RESULT: initial 100,000 pose-pair/sweep iterations allocated
  zero managed bytes. Corrected 22-second Editor play fixture measured mean rig
  update 0.03579 ms; this is not WebGL or iPhone performance.
- FAILURE_MODE: very coarse rendering can still omit the exact subframe impact
  pose. Sweep tests continuous volume contact, not pixel-perfect raster overlap.
- VERDICT: selected; integrated build and runtime gates still required.

## M05 replacement invariant and tolerance

Visible glove radius is 0.115 m. Head/chest target spheres have radius 0.18 m;
abdomen 0.25 m. Chest is rendered using the same overlapping sphere union that
contact tests. Guard contact uses the visible glove spheres. Earliest intersected
eligible surface decides HIT/BLOCK. Commit/recovery are non-damaging; limbs and
legs outside the scoring/guard volumes are not scoring targets. Counter-open
guard bypass remains an explicit existing rule, not a geometric MISS claim.

The selection corpus measured maximum midpoint chord deviation 0.000595323 m
at 1/240 s substeps. M05 uses a **1 mm verification tolerance**: the measured
0.596 mm chord error plus 0.01 mm numerical allowance and remaining margin for
the sampled sphere-union path. This is under 0.9% of glove radius. The resolver
does **not inflate colliders by 1 mm**; it solves exact relative segment/sphere
contact. The tolerance assesses interpolation error, not extra punch reach.
The moving-target corpus must pass separately; this empirical tolerance is not
a universal bound on untested motion speeds. Frame interval is explicitly varied
at 15–120 FPS. Contact notification is at the enclosing rendered frame; its age
is at most that frame interval, not an extra spatial reach allowance.

For each tested attack: HIT/BLOCK needs continuous visible-volume intersection;
MISS needs the sampled continuous path outside all eligible target volumes.
Counter and body/head attack eligibility are evaluated separately from geometry.

## Distance and motion ownership

- Hook pocket boundary: 0.48 m hook forward extent + 0.18 m head radius +
  0.115 m glove radius = 0.775 m.
- Outer guard-contact boundary: 0.67 m cross intent + 0.28 m defender guard
  forward offset + two 0.115 m gloves = 1.180 m. Solver/yaw and lateral offset
  refine actual eligibility; the contact matrix is authoritative for outcomes.
- AI targets 0.955 m, with ±0.045 m spacing deadband; closes at 0.36 m/s,
  retreats at 0.32 m/s, and makes small 0.10 m/s lateral adjustments.
- No root translation or facing update after opponent commitment. Target aim
  is captured once. No opponent homing was introduced.
- Feet alternate 0.18 s steps, 0.045 m lift. One plant stays in world space while
  the other gathers. Lead/rear offsets are ±0.17 m; lateral base ±0.22 m.
- Controllers own input, action phase, root movement and telemetry semantics.
  Round2Motion owns pose computation. Rig owns visible arm/glove/head/torso
  transforms and contact sampling. Footwork owns pelvis/legs/shoes. Shell owns
  static decor and attached facial/glove details. P1-V owns existing HUD/controls.
- Frozen upper arm/forearm lengths remain 0.34/0.31 m. A1 ordering and A3.1
  close/far relationship are tested on solved wrists, not merely constants.
- HP, stamina costs/recovery, round winner, gesture vocabulary and phase
  durations retain their previous paths. Contact resolves at actual extension
  intersection instead of historical 0.72/0.78 endpoint look-ahead thresholds.

The first Editor motion fixture erroneously updated once per Editor callback,
not once per rendered frame. Its movement/performance data are invalid. The
corrected fixture gates by Time.frameCount. This correction is recorded instead
of presenting the original 662,747 callback count as a frame measurement.

The implementation is a procedural articulated research boxer, not a completed
production Ramirez character. No PvP, replay or secondary screens are enabled.
