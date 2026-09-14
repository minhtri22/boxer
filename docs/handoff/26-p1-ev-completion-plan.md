# Boxer — P1-EV Completion Plan

STATUS: `APPROVED__EXECUTION_ACTIVE`

User approval recorded: 2026-09-14.

This plan continues from the reviewed state in
`25-p1-ev-takeover-review.md`. No implementation step below should begin until
the user approves this plan.

## 1. Objective

Finish the approved visual-reference integration on top of the qualified Round 2
fighter without changing the already-accepted combat/mechanics architecture.

Target final state:

`IMPLEMENTATION_PASS_PENDING_HUMAN_UAT`

The final handoff must be one integrated P1-EV candidate. No PvP, secondary
screens, replay, career or new AI-defense research begins in this task.

## 2. Frozen baseline and change boundary

Protected baseline: `b0eeed2` on `p1/whole-body-mechanics`.

P1-EV may change only what is required for visual integration, its tests,
build/provenance support and evidence.

The following implementation authorities remain unchanged:

- `Round2CombatRig` — solved pose/contact ownership;
- `Round2Motion` — articulated punch/body pose generation;
- `Round2Footwork` — pelvis/legs/feet movement and planting;
- `OpponentBoxer` / `PlayerBoxer` — actor state;
- `P1PunchMechanics` — punch-family mechanics;
- existing distance/contact/no-homing semantics.

If a requested visual feature requires changing one of those authorities, stop
and document the contradiction before changing it.

## 3. Phase P0 — Freeze and clean the current P1-EV candidate

Goal: preserve the current useful work while removing ambiguity about generated
files.

Actions after approval:

1. Record current dirty-file inventory and hashes for P1-EV source/resources.
2. Classify every dirty/untracked file as:
   - intended P1-EV source/resource/test/document/evidence;
   - generated Unity scene/settings/build metadata;
   - local log/tool noise.
3. Revert or exclude generated/local noise that is not part of P1-EV.
4. Review the meaningful diff with end-of-line noise ignored.
5. Verify no combat-authority file listed in Section 2 changed.

Acceptance:

- the candidate can be staged later without unrelated files;
- no Round 2 combat-authority source is modified;
- current P1-EV textures/resources remain preserved.

## 4. Phase P1 — Close the visual ownership runtime gate

Goal: prove the shell follows the rig instead of competing with it.

Run candidate B through the corrected 22-second Editor runtime fixture and make
the existing `EVEvaluation.AuditFrame/AuditFinal` checks complete normally.

Required output:

`evidence/p1-ev/ownership-runtime.txt`

Required PASS conditions:

- frames audited > 0;
- failed frames = 0;
- continuous torso anchor error < `0.00001 m`;
- cuff remains parented to its real glove and aligned to forearm direction;
- exactly one visible shoulder per side;
- trunk legs remain attached to thighs;
- boot uppers remain attached to shins;
- redundant B torso renderers remain disabled.

The current Unity SearchDatabase indexing exception must be handled as an
environment/test-harness issue. Do not weaken or remove ownership assertions to
make the run finish.

If the runtime audit finds a real ownership failure, fix only the visual layer
and rerun the same gate.

## 5. Phase P2 — Requalify contact/surface invariants

Goal: confirm the final visual shell is still geometrically coherent with Round 2.

Rerun the existing EV shell checks on the final candidate B:

- head/glove replacement preserves exact authoritative sphere radius;
- torso vertices remain < `1 mm` from the original contact union;
- torso triangle centers remain < `1 mm` from the original M05 union;
- head texture and EV shaders load from Resources;
- bottom control graphics remain disabled independently of input.

Do not enlarge contact geometry or relax the 1 mm requirement to rescue a visual
mesh.

Acceptance: EV shell tests `6/6 PASS` with measured errors recorded.

## 6. Phase P3 — Full deterministic regression on final P1-EV source

Goal: prove visual integration did not regress the qualified Round 2 system.

Run the complete deterministic Round 2 suite plus the EV-specific tests from the
same final source.

Required preserved evidence includes:

- historical Round 2 assertions;
- A1 ordering;
- A3.1 hook close/far relationship;
- punch-family continuity and recovery;
- moving-target contact matrix;
- planted-foot and no-homing invariants;
- HP/stamina/counter/winner semantics covered by the existing suite;
- zero-allocation hot-loop check where already part of the suite;
- EV geometry/surface checks.

Acceptance: no regression relative to the `182/182` Round 2 baseline and all EV
checks pass. Any changed count must be explained by added tests, not by removed
historical assertions.

## 7. Phase P4 — Final visual evidence and M01–M12 review

Goal: validate the selected B shell after the latest refinements, not an older
intermediate capture.

Capture a fresh motion series from the final source at the existing portrait
UAT composition and inspect at least:

- guard/stance;
- advance and retreat;
- lateral reposition;
- straight punch;
- hook;
- uppercut;
- overhand;
- recovery;
- close-range contact;
- effective-range contact.

Re-evaluate M01–M12 from
`docs/handoff/22-combat-animation-quality-gates.md`, with particular attention to:

- M05 contact perception;
- M09 duplicate/jitter ownership artifacts;
- M10 first-person composition and reference fidelity;
- M11 render/runtime cost;
- M12 regression.

The visual review must explicitly verify:

- Ramirez face/hair/beard treatment remains attached to the 3D head;
- red gloves, red/gold trunks and white/red boots stay with their rig parts;
- black/gold player gloves remain prominent;
- no full-frame fighter billboard hides real articulation;
- backdrop is distant world scenery and cannot cover fighter motion;
- no bottom control-button graphics return;
- HUD remains compact and legible.

## 8. Phase P5 — Performance comparison

Goal: detect a material P1-EV rendering regression on the same desktop/WebGL
environment used for the available Round 2 observation.

Measure and record:

- runtime FPS/frame time;
- visible stutter;
- any new managed allocation pattern;
- build size/load impact from P1-EV textures and meshes;
- editor rig-update evidence as diagnostic only.

Round 2's prior desktop WebGL observation (~87–90 FPS, p95 ~12–13 ms) is a
reference, not a target-device claim. P1-EV must be measured under matched local
conditions before claiming no meaningful regression.

Proposed stop/refine rule: if matched frame time regresses by more than 10% or
new repeated stutter appears, investigate the visual layer before release.
Physical iPhone performance remains Human UAT/device evidence and must not be
invented from desktop results.

## 9. Phase P6 — Commit only the qualified P1-EV source

Goal: create one reviewable source revision before build.

Stage explicitly only:

- intended P1-EV source changes;
- EV shaders/resources and their Unity `.meta` files;
- focused EV tests;
- required build/provenance support;
- P1-EV docs/evidence intended for version control.

Exclude local logs, generated scene file-ID churn, `local-web` project settings,
temporary directories and unrelated evidence rewrites.

Before commit:

- inspect staged diff;
- verify combat-authority files remain unchanged;
- verify no accidental whole-file line-ending rewrite is being staged.

The resulting full SHA becomes the P1-EV source provenance marker.

## 10. Phase P7 — Clean committed-source WebGL build

Goal: qualify exactly what was committed.

Build from a clean checkout of the P1-EV source SHA with the P1-EV build stage
enabled.

Required provenance:

- Unity `6000.5.8f1`;
- full source SHA;
- productVersion `ev-<full source SHA>`;
- build result and elapsed time;
- artifact file hashes;
- manifest hash / WASM hash;
- presentation marker identifying P1-EV articulated mesh shell;
- no post-build source diff in the clean build checkout.

Use `tools/verify_round2_artifact.py --version-prefix ev-` for the P1-EV marker
path after the source is committed.

## 11. Phase P8 — Browser/WebGL smoke and deployment verification

Goal: prove the built artifact starts and behaves as the integrated candidate.

Verify locally first:

- Unity startup succeeds;
- portrait layout is correct;
- expected source/version marker is visible to verification tooling;
- guard, punch, HIT/BLOCK/MISS and recovery occur;
- opponent moves/steps without homing during commitment;
- no JavaScript runtime error attributable to P1-EV;
- developer metrics remain opt-in only.

Then push the approved P1-EV commit/artifact path to
`p1/whole-body-mechanics`, deploy through the existing Pages workflow and compare
the publicly served files byte-for-byte/hash-for-hash with the tested artifact.

The existing Round 2 URL may be reused only after it demonstrably serves the new
P1-EV artifact.

## 12. Phase P9 — Final report and stop

Create a P1-EV final execution report containing at minimum:

- starting SHA (`b0eeed2`);
- final P1-EV source SHA;
- artifact/package SHA and file hashes;
- candidate A/B result and why B was selected;
- exact geometry errors;
- ownership-runtime result;
- deterministic/regression results;
- M01–M12 evaluation;
- matched performance result and limitations;
- build provenance;
- deployed-artifact verification;
- known limitations;
- exact UAT URL/artifact;
- status `IMPLEMENTATION_PASS_PENDING_HUMAN_UAT` or
  `IMPLEMENTATION_BLOCKED`.

Then stop for the single integrated Human UAT requested by the user.

Do not claim Human UAT PASS.

## 13. Approval checkpoint

No code, test-harness, shader, resource, build or deployment change should begin
until this plan is approved.

Approval authorizes execution of P0 through P9 under the frozen boundaries above,
with evidence-driven visual fixes permitted inside the P1-EV presentation layer.
