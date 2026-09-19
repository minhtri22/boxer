# Ramirez Review 5 — Checklist Results

Current state after the human visual rejection, clavicle-chain anatomy correction, reference comparison, and final QA rerun.

## A. Trunks — PASS

- [x] Fabric thickness <= 0.002 m: `0.00125 m`.
- [x] Both leg shells have zero unweighted vertices.
- [x] Left/right leg openings physically separate: inner gap `0.0407 m`.
- [x] Cloth flare present: upper opening width `0.2080 m`, lower `0.2693 m`.
- [x] Hem drape/split is measurable: range `0.0660 m`.
- [x] Waistband height valid: `0.0516 m`.
- [x] Front / 3/4 / side / back evidence uses the reference comparison set.
- [x] Side treatment is a narrow curved seam rather than a broad rigid plate.

## B. Neutral guard — PASS

- [x] Lowered center of mass with both knees flexed.
- [x] Knee angles: lead `156.20°`, rear `156.60°`.
- [x] Forward crouch gate passes.
- [x] Orthodox split stance gate passes.
- [x] Hands-home gate passes relative to head position.
- [x] Elbows remain compact to the torso in the Review5 guard target.

## C. Jab — PASS

- [x] Lead extension passes.
- [x] Rear guard passes.
- [x] Body-involvement gate passes.
- [x] Both feet remain grounded.
- [x] Review5 target increases lead-side shoulder participation rather than using arm-only reach.

## D. Cross — MACHINE PASS

- [x] Rear extension passes.
- [x] Lead guard passes.
- [x] Forward-drive gate passes.
- [x] Rear heel/toe relationship passes the corrected planted-toe metric.
- [x] Rear heel sits `0.0965 m` above the toe while the toe remains at the floor plane.
- [x] Rear foot uses explicit yaw + pitch rather than lifting the whole ankle chain.
- [x] Pelvis / upper-torso rotation remains body-supported.

## E. Slip left/right — PASS

- [x] Both heads leave centerline.
- [x] Both pelvis-shift gates pass.
- [x] Both feet remain grounded.
- [x] Slip-left asymmetric knee-load delta: `6.99°`.
- [x] Slip-right asymmetric knee-load delta: `7.26°`.
- [x] Slips now combine knee load, pelvis translation and torso angle rather than neck-only lean.

## F. Slip + counter — MACHINE PASS

- [x] Head remains off centerline.
- [x] Loaded-base gate passes.
- [x] Knee angles: lead `151.14°`, rear `163.29°`.
- [x] Rear extension passes.
- [x] Rear heel/toe relationship passes the corrected planted-toe metric.
- [x] Rear heel sits `0.0942 m` above the toe while the toe remains at the floor plane.
- [x] Pelvis / upper-torso counter rotation remains body-supported.

## G. Recover — PASS

- [x] Left hand returns within the guard-return tolerance.
- [x] Right hand returns within the guard-return tolerance.
- [x] Recover uses the same compressed stance family as neutral guard.

## H. Evidence and scope — PASS

- [x] 5 static views under `renders/review5/static/`.
- [x] 7 combat poses under `renders/review5/combat/`.
- [x] 7 clay poses under `renders/review5/combat-clay/`.
- [x] 4 trunks comparisons under `renders/review5/trunks-comparison/`.
- [x] `renders/review5/pose-set-contact.jpg` exists.
- [x] No Unity diff.

## I. Mandatory visual-reference gate — INTERNAL PASS / HUMAN UAT NEXT

- [x] Reference comparison is now a hard QA requirement in `review5-checklist.md`.
- [x] `review5-visual-gate.json` exists and is consumed by machine QA.
- [x] Approved turnaround visual match checked for anatomy/pose continuity.
- [x] Approved combat visual match checked for anatomy/pose continuity.
- [x] Anatomy plausible in every required pose.
- [x] No arm/glove/shoulder fusion or penetration into the torso.
- [x] Limb alignment plausible in every required pose.
- [x] Trunks construction remains structurally valid.
- [x] Review8 glove/forearm proportion passes the hard reference-lock gate: glove width `0.1945 m`, rendered forearm p90 diameter `0.1267 m`, ratio `1.535` on both sides.
- [x] Review8 trunk coverage includes `FrontOverlap`, continuous `InnerBody`, and separate `InnerLeg.L/R`; required static/combat visual crops show continuous burgundy coverage instead of the prior exposed-skin/torn-notch failure.
- [x] Review8 approved-reference comparison completed in `renders/review8-hard-gate/approved-reference-vs-final.jpg`.
- [x] Skin/material/lighting fidelity remains within the current approved-reference visual gate.

Latest machine QA: structural/biomechanics gate passes, hard reference-lock gate passes, and `visual_reference.pass = true`; human UAT is the next external gate, not a substitute for internal QA.

## Internal verdict

`BLENDER_REFERENCE_LOCK_PASS_PENDING_HUMAN_UAT`

The Blender correction passed the locked internal gates and is ready for human visual review; Unity remains blocked.
