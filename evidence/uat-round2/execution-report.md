# Boxer UAT Round 2 — IMPLEMENTATION BLOCKED

Date: 2026-09-13 (Asia/Saigon). Execution authority:
`docs/handoff/20-uat-round2-execution-prompt.md` at `72634fa`.

No Round 2 gameplay patch or candidate was built. **The license blocker is resolved**:
after the user confirmed the existing Personal (PE) license, fresh Editor runs exited
0 and passed all 131 baseline regressions plus 8 P1-V tests. The earlier error is
retained as historical evidence, not a claim that the user lacks a license.

The active blocker is contact coherence: an additional Unity Editor audit invokes
the real incoming-punch resolver and arm solver for a default neutral jab at 1.4 m.
The resolver returns BLOCK while the rendered native glove has a positive minimum
surface gap of **0.043358 m** from every authoritative target throughout 257 sampled
extension poses. Thus M05 fails despite the historical regression suite passing.
No human UAT PASS, implementation PASS, or new build PASS is claimed.

## Provenance and work completed

| Required field | Evidence/status |
|---|---|
| Starting local SHA | `a2d3cb65f176650b72b035aac281656ab24117b7` |
| Synced / inspected source SHA | `72634fa83e9f903d093365de68eac1564bdc9c89` |
| Branch | `p1/whole-body-mechanics` |
| New artifact SHA | NONE — no Round 2 artifact exists |
| Existing artifact commit | `a2d3cb6` contains the existing files; their actual binary source SHA is unverified |
| Unity version | `6000.5.8f1 (5cb7df797b7d)` from project and attempted Editor execution |
| Baseline tests | Fresh 131/131 through P1-D + 8/8 P1-V PASS, Editor exit 0; `baseline-131.txt`, `baseline-p1v-8.txt` |
| Contact audit | Executed successfully, M05_CONTACT_COHERENCE=FAIL; `unity-contact-audit.txt` |
| Final gameplay tests | NOT RUN; gameplay remains identical to the baseline; only an Editor diagnostic was added |
| Build result | NOT RUN; stopped at confirmed contact-coherence issue under section 16 |
| New build marker / productVersion | NONE |
| Existing marker / productVersion | `local-web`; cannot certify committed-source provenance |
| WebGL artifact hashes | Existing-file SHA256 inventory in `preflight.json`; not new build evidence |
| FPS / frame time | UNMEASURED; existing artifact did not reach gameplay in this browser |
| Exact Round 2 UAT URL/artifact | NONE; do not present the existing Pages deployment as Round 2 |
| Temporary diagnostic URL | `http://127.0.0.1:8766/`, existing local artifact only; server stopped after inspection |

Fast-forward fetched six documentation-only commits. Existing dirty scene,
ProjectSettings and build-metadata files, untracked logs and `.agentloop/` were
preserved. A detached build worktree was created at
`D:\WORK\RESEARCH\POVGame\boxer-uat2-build` at the synced SHA, to avoid changing those files.

Read: mandatory handoff 18–22, README, current state, working protocol, visual
direction, all reference authority files and all six images. Inspected runtime
controllers, arm/leg embodiment, transform ownership, punch geometry/timing,
feedback, P1-V presentation, relevant deterministic suites, and WebGL builder.
See `motion-reference-review.md` for the three supplied video references.

## Baseline attempt

Executed in the detached worktree:

```text
Unity.exe -batchmode -nographics -quit
  -projectPath D:\WORK\RESEARCH\POVGame\boxer-uat2-build\unity\BoxerP0
  -executeMethod BoxerP0.Editor.P1DOpponentAttributesSelfTests.RunWithRegressions
  -logFile D:\WORK\RESEARCH\POVGame\uat2-baseline-unity.log
```

The initial sandbox attempt failed during database/licensing startup. A permitted
unsandboxed retry using Start-Process -Wait produced exit 198, zero matching license
entitlements. The relevant raw excerpt is in `baseline-unity-error.txt`.
The earlier interrupted log is `D:\WORK\RESEARCH\POVGame\uat2-baseline.log`.
After the user showed an existing Personal license, the same command succeeded with
log `D:\WORK\RESEARCH\POVGame\uat2-baseline-pe.log`. The P1-V method
`BoxerP0.Editor.P1VPresentationSelfTests.Run` also exited 0, logging to
`D:\WORK\RESEARCH\POVGame\uat2-baseline-p1v.log`.
No user Unity session was terminated; no license files, account credentials, or
entitlements were changed by this task.

Contract section 13's baseline requirement is now satisfied. The existing tests
do not establish M05: several embodiment tests check constants/finite coordinates
or unconditional assertions, and P1-V checks resources, pose tokens and bounds.
Their original tests and expected values were preserved.

The new diagnostic method `BoxerP0.Editor.Uat2ContactAudit.Run` uses the actual
private player pose methods (reflection), actual ArmChainMath, and actual
OpponentBoxer.ResolveIncomingPunch. The isolated fixture copies the default
relative root/guard/collider values from BoxerBootstrap. It does not run the full
game loop or P1-V GUI and is not a recorded gameplay reproduction. Process exit 0
means the audit completed; its M05 verdict is FAIL. Full log:
`D:\WORK\RESEARCH\POVGame\uat2-contact-audit.log`.

## Findings to resolve before implementation

Except for the explicitly labeled Unity contact audit, these are source findings,
not claims of reproducing the user's exact visual artifact in gameplay. Browser
startup was blocked before that could be observed.

1. **P1-V conceals the articulated scene.** `BoxerVisualShell.Start` always adds
   `P1VCombatPresentation`. Its `OnGUI` draws an opaque full-screen arena, then full
   opponent pose images and player glove images. `DrawOpponent` clamps apparent
   center/height and crossfades unrelated guard/attack images during commit/recovery.
   Thus adding movement to the underlying 3D feet alone cannot expose articulated
   footwork. The crossfade is a concrete double-silhouette mechanism; identification
   with the reported duplicated shoulder remains to be checked against runtime.

2. **Visible glove projection is detached from contact.** `ProjectGlove` clamps Y
   to 70–91% of screen height and X to 12–88%; `DrawPlayerGloves` uses fixed image
   dimensions. Neither is a world-space glove/target rendering. This can explain
   spatial disconnect even when authoritative endpoints intersect a target.

3. **Native arm reach does not match the existing endpoint.** `ArmChainMath.Solve`
   clamps the wrist to 0.649 m from the shoulder (0.34 + 0.31 minus epsilon).
   The fixed player shoulder and neutral jab endpoint are 1.271574 m apart, leaving
   a 0.622574 m endpoint-to-wrist gap. Cross gap is 0.714121 m. Glove radius is 0.115 m.
   `tools/uat2_preflight.py` extracts all eight endpoint constants and reproduces
   this clamp; results and source hashes are in `preflight.json`. That inventory is
   a static full-extension calculation. The separate Unity audit confirms an actual
   resolver/visual-solver discrepancy at default spacing. Moving the opponent
   closer alone does not reconcile visual and authoritative endpoint coordinates.
   Do not silently shorten reach, enlarge colliders, or lengthen frozen arm bones.

4. **Two opponent shoulder representations exist.** `BoxerVisualShell` creates
   extra `Opponent Shoulder Visual` spheres, while `ArmVisualEmbodiment` creates
   shoulder joints. `OpponentBodyRotationEmbodiment.Update` (order 210) moves the
   decorative shoulders separately from arm sampling (order 200). They are distinct
   transform owners/representations, not proven competing writes to one transform.

5. **Opponent guard pose changes convention at action boundaries.** Idle
   `PoseArmOpponentGuard` adds an outward X offset of 0.22 m to shoulders already at
   +/-0.38 m, giving +/-0.60 m glove centers, whereas controller guards are +/-0.22 m.
   Busy arm posing uses a different solver/path; this needs continuity checks.

6. **No opponent spacing/step controller exists.** `OpponentBoxer.Update` faces the
   player and starts attacks on a timer without a distance eligibility test.
   `OpponentLegEmbodiment` interpolates root-local neutral feet, not planted/swing
   world-space foot phases. Rotation locks while busy (existing no-homing behavior).

7. **Yellow/orange artifact remains unconfirmed.** Original glove decorations
   include gold cuffs/panels; `ArmVisualEmbodiment` already hides original glove
   renderers and their children each Update. Claiming those decorations are the
   observed artifact without runtime proof would be unjustified. That hide loop
   also calls GetComponentsInChildren four times per frame; P1-V rendering and
   decorative-anchor searches should be included in the allocation review.

The next implementation must first establish one coherent fighter rendering path
that preserves HUD hierarchy and authoritative combat. The current inspection does
not establish that a safe visual-only fix for all endpoint gaps is feasible.
Contract section 16 requires stopping if that demands unexpected geometry changes
or the reported artifact cannot be confidently diagnosed. No scope extension has
been made here.

## Existing artifact smoke (not candidate validation)

HTTP HEAD returned 200 for index, loader, framework, data and wasm; see
`existing-artifact-http.json`. In the in-app browser, clicking the existing start
button showed `MOTION DENIED — build local-web`; the start gate remained visible.
No permission was bypassed. An empty captured error/warning list does not prove
runtime health because Unity gameplay never loaded. Desktop browser behavior is
not iPhone performance evidence.

## M01–M12 status

No gate is awarded PASS from static inspection. FAIL below describes an unmet
baseline requirement supported by source; BLOCKED means candidate validation is
unavailable, not an observed regression.

| Gate | Status | Basis |
|---|---|---|
| M01 Distance | FAIL (source) | No AI distance maintenance; projection/contact and native endpoint gaps. |
| M02 Stance | BLOCKED | Idle/busy guard conventions differ; no candidate visual review. |
| M03 Footwork | FAIL (source) | No advance/retreat/lateral stepping implementation. |
| M04 Body connection | FAIL (source) | Full pose-image overlay cannot expose the underlying articulated body chain. |
| M05 Glove-to-target | FAIL (Unity audit + source) | BLOCK without native visual-glove overlap; P1-V also clamps glove screen coordinates. |
| M06 Families | BLOCKED | Baseline math tests pass; visual families not reviewed in candidate gameplay. |
| M07 Recovery | BLOCKED | Pose-image blending/guard transition needs runtime continuity evidence. |
| M08 Defense readiness | BLOCKED | Existing guard/head regressions pass; no candidate visual review. |
| M09 Stability | BLOCKED | Double-silhouette mechanisms found; exact reported artifact not reproduced. |
| M10 POV composition | BLOCKED | Approved images inspected; candidate absent. |
| M11 Performance | BLOCKED | No FPS/frame-time capture, no iPhone session. |
| M12 Regression | PARTIAL | Baseline compile and 139 tests PASS; no implementation/final candidate or build. |

Human UAT remains pending. All nine visual-reference match categories (POV, top
HUD, bottom controls, gloves, opponent, palette, atmosphere, mobile text, debug)
are unassessed for Round 2 because no candidate exists.

## Changed files and resumption

Only these diagnostic/documentation files are added:

- `tools/uat2_preflight.py`
- `evidence/uat-round2/preflight.json`
- `evidence/uat-round2/existing-artifact-http.json`
- `evidence/uat-round2/baseline-unity-error.txt`
- `evidence/uat-round2/motion-reference-review.md`
- `evidence/uat-round2/execution-report.md`
- `evidence/uat-round2/baseline-131.txt`
- `evidence/uat-round2/baseline-p1v-8.txt`
- `evidence/uat-round2/unity-contact-audit.txt`
- `unity/BoxerP0/Assets/Editor/Uat2ContactAudit.cs` and its Unity `.meta`

License activation is no longer required. The user decision needed is the permitted
scope for reconciling the demonstrated mismatch between frozen finite arm reach and
authoritative contact. A controlled contact/embodiment reconciliation can preserve
control gestures, A1/A3 categorical relationships, timers, HP/stamina and counter
semantics, but may change distance boundaries; it needs an explicit revised gate,
not silent tuning of existing passing tests. A visual-only solution would instead
need to demonstrate continuous, plausible anatomy at every accepted contact distance.
The current inspection has not established such a solution.

The existing contract explicitly says to stop if a required change would alter
authoritative hit/damage geometry unexpectedly. Accordingly no reach constants,
colliders, arm lengths or hit rules were changed, and no partial footwork patch was
presented as a complete candidate. After resolving this scope boundary, continue
the original distance/feet/stance/body/recovery/artifact sequence, regressions,
committed clean source build, runtime/performance evidence and final human UAT.
Do not deploy the old `local-web` artifact as a Round 2 candidate.
