# Combat fairness/contact fix — 2026-10-08

Current status: AUTOMATED_PASS_PENDING_DEPLOY_AND_HUMAN_UAT.

Owner explicitly requested combat fixes, actual input/contact tests, WebGL build
and Pages deployment. Existing branch `feature/boxer-product-loop-wave1`;
pre-fix deployed candidate was compiled at `3a861a13872dcdb39d83adcb3f712d41f5b4f79c`.

## Root causes and scoped corrections

- AI low-bit LCG modulo 4 plus an interleaved gap draw trapped the original
  seed in choices 1/3 (head/body Cross). Multiply-high selection now uses the
  full state. Seed and attack-gap/phase/profile constants are unchanged.
- Body attack mode used to skip guard targets even if the glove intersected.
  Target-mode helper now preserves guards for body aim. Runtime sweeps use all
  actual volumes for BOTH actors, so aim and counter labels do not make defense
  intangible. First physical contact determines HIT/BLOCK, otherwise MISS.
  Body stamina consequences follow the actual BODY contact reason, not AI aim.
- AI's old .955m spacing prevented neutral straights reaching an exposed head.
  Shared .82m engagement distance (+/- .03m spacing tolerance) lies in tested
  Jab/Cross reach. Hooks/uppercuts still require approaching close range.
- Player's forward root limit was -.05m, while AI could retreat to +2.25m:
  an invisible center-line wall prevented chasing. Player can now use +2.25m
  forward extent, retaining the original minimum torso separation and ring bounds.
- Forward/sideways input followed fixed world axes rather than the facing POV.
  Physical-input attempt 3 reached .62m at 1.37s, then continued past the opponent
  to 1.66m and eventually stuck at .85m. Forward now follows the opponent-facing
  rotation; 16 cardinal-direction/yaw checks cover the mapping. No speed change.
- HP/Stamina/Capacity formulas, damage, action costs, fatigue/recovery, KO,
  arm lengths, mesh assets and original gesture thresholds are unchanged.
  A high guard is not immunity: a body path below non-intersecting gloves can hit.

## Verification and release gates

New evidence: `evidence/wave1/combat-v3`. Prior evidence remains unchanged.
Initial 156/156 geometry/selection checks PASS, including all four selections with
the original interleaved gap draw (counts 31/28/32/37 across 128 attacks), body
guard interception, transform invariance, reachable exposed head at .79/.82/.85m,
old .955m miss reproduction and long-range misses. Existing vitals/flow/guide
105/105 plus Round2 43/43 and P1V 8/8 regressions PASS.
After facing-relative footwork: 172/172 combat checks, 105/105 vitals checks,
and controller 58 checks EXIT=0 (Unity PID 43428 completed normally).
These numerical tests are NOT physical-input or Human UAT evidence.
Controller harness rerun after the forward-bound fix: 58 checks, EXIT=0. Its hits
are explicitly injected consequence checks, NOT actual contact acceptance.

Actual WebGL audit uses browser CDP touches and ordinary keyboard only, observes
native controllers/pose sweeps, and never injects HP, outcomes, attack phases,
hidden StartAttack/StartBout or actor positions. Planned checks: idle guard
survives default bout with real blocks; all four real AI attack choices; close
body attack actually hits glove -> BLOCK and zero HP; forward swipe follows
retreating AI; long-range MISS; straight into closed guard BLOCK; patient
directional attacks can win and achieve physical HP-zero KO. Trace and rendered
captures are retained even on FAIL. Default required criteria are fixed before
running; an audit FAIL blocks deployment, not an excuse to relax its assertions.

First WebGL build succeeded at `e9e25814a841927c38118d00b537280786a61af6`
in 292.7701592s; 7 payload hashes matched provenance. General browser 28 checks
PASS, zero errors. Combat attempts 1/2 timed out waiting for close body contact;
attempt 3 recorded the actual fixed-world-axis movement defect above. All three
FAIL reports and captures are preserved in separate attempt directories.
This artifact is NOT deployed. Revised facing-relative candidate source
`874065708917a3240d887779fe5de7ab14787bb6`: numeric/controller PASS, build
Succeeded in 258.1323927s, 60,909,052 bytes, Unity exit 0; all seven payload
hashes match provenance and build scene/settings are restored unchanged.
General browser rerun PASS: 28/28, zero JS/load/HTTP errors, matching compiled
product version. Final full contact suite PASS: 17/17, zero errors. Ordinary
browser touch/keyboard only; 9 real HITs produce KO at 37.513000254519284s,
player HP 100, opponent HP 0; player blocks 18, opponent blocks 4, player MISS 1.
Both actors and gameplay input lock. Read-only frame/contact trace, trusted
multi-touch receipts, first HIT and result renders are retained. This is a
repeatable synthetic input tactic, NOT a promise that every casual gesture wins.
Deployment: NOT_RUN_YET.
Attempt 4 on the revised build PASSed close body guard interception (.62m,
HP 100 unchanged) and real long-range MISS / closed-guard straight BLOCK.
Its attack macro FAILed because the driver supplied the remaining LEFT point to
partial touchEnd. That released the left finger instead of the right. Attempt 5
tried a touchMove active-list diff; trusted DOM receipts proved it never released
the right finger, so punches still did not occur. Both FAILs are preserved.
A standalone real-CDP DOM probe on this installed Edge verified partial touchEnd
with the ENDED right point: active [1,2] -> active [1], changed [2]. This differs
from the bundled protocol's empty-only comment. The audit now uses that observed
behavior and asserts the trusted right-release/left-held receipt before attacks.
No native C# or gameplay change for these driver corrections; same compiled
build is being retested, not relabelled PASS.
Attempt 6 has verified trusted partial release and genuine native OVERHAND
attacks. It won by points (player HP 94.3672455910, opponent HP 77.6347743293),
with 2 geometrical head HITs; many point-blank overhands contacted high gloves
first. KO gate FAIL retained. Next tactic uses existing close-range up-swipe
uppercuts below the high guard; all win/HP-zero KO criteria remain mandatory.
Attempt 7's repeated .62m uppercuts were also BLOCKed (23 total opponent blocks,
zero player hits); preserved FAIL. Next input tactic maintains a .71-.76m pocket
with ordinary left-thumb steps before down-swiping overhands, rather than keeping
both guard gloves pressed together at the minimum torso stop. No numerical
threshold/seed, contact volumes, damage, timer or KO gate is changed.
Attempt 8's pocket-step tactic reached opponent HP 40 with 5 HITs but no KO;
an extra empty touchEnd during cleanup also failed. This FAIL is preserved.
First tactical probe won points with 7 HITs/opponent HP 16.3927387465 but did
not KO because wall-time travel estimates missed the pocket. Second probe used
observed ordinary thumb movement and windup timing: KO at 24.421s, 9 HITs.
Its status PROBE_PASS_NOT_RELEASE_GATE cannot authorize deployment. The full
17-check rerun above includes idle defense, all real AI choices, body guard,
MISS spend, straight BLOCK, trusted multitouch and physical KO; it is the gate.
Build was initially held when unrelated Python PID 37532 rose to 53.22 GB
private and the host had under 1 GB free commit. No external process was stopped.
On continuation the host has about 14 GB free RAM / 32 GB free commit; build resumes.
Pages helper requires both general browser and combat-input browser PASS with
matching compiled version, plus numeric/controller PASS. Deployment permission
already covers this exact feature branch; preserve all other policies.
Critical Python PID 9152 (and launcher 24528) must remain untouched.
Phone sensor/touch feel, mesh/contact visual fidelity and Human UAT remain
PENDING even if automated, build and deployment gates pass.
