# Ramirez Blender Visual Gate — Review 4

Scope lock for this round: **boxing trunks + seven boxing poses only**.

Primary visual authority: `docs/handoff/reference-ui/06-opponent-ramirez-turnaround.jpg`

Combat context: `docs/handoff/reference-ui/03-pov-combat-hud.jpg`

Motion grounding: `review4-reference-grounding.md`.

## Trunks correction

- Rebuilt the trunks as thin left/right leg shells that meet at the upper inner seam but remain visibly separated at the leg openings.
- Fixed a Review4 weighting defect where the added sixth cloth ring was not being assigned to pelvis/thigh groups.
- Reduced shell thickness to `0.0015 m` and increased smooth subdivision to remove the rigid hard-shell read.
- Re-shaped the garment to follow hips/thighs more closely rather than forming a wide trapezoid around the pelvis.
- Added a small outer side split and kept only a narrow gold side seam instead of a broad rigid gold panel.
- Tightened the waistband and reduced plaque/label bulk.

Measured QA:

- left/right shorts vertices: `193` each;
- unweighted shorts vertices: `0` on both legs;
- lower-opening inner gap: `0.0116 m`;
- cloth solidify thickness: `0.0015 m`;
- waistband height: `0.0516 m`.

## Pose correction

- Lowered the common center of mass and increased forward crouch/chin tuck.
- Lowered elbows into torso-protecting guard while keeping gloves at head level.
- Jab now keeps the rear hand home while the lead side reaches from a grounded base.
- Cross now combines rear-hand extension, lead-hand guard, torso/hip rotation, and measurable rear-heel release.
- Slip left/right use pelvis translation, knee participation and torso side-bend so the head leaves centerline without a neck-only lean.
- Slip + counter keeps the head off center while the rear side drives a stable counter from a loaded base.
- Recover returns both wrists close to the neutral guard coordinates.

Automated pose QA (`scripts/qa_trunks_pose_gate.py`) reports **PASS** for every configured trunks/pose check. Key measured values:

- guard lead wrist height: `1.3807 m`;
- guard rear wrist height: `1.4564 m`;
- cross rear-heel release: `0.0294 m` relative to guard;
- jab grounded: PASS;
- slip left/right head-off-center: PASS;
- slip + counter head-off-center / loaded base / rear extension: PASS;
- recover guard return: PASS.

## Required evidence

Five static views:

- `renders/review4/static/01_front.png`
- `renders/review4/static/02_three_quarter_front.png`
- `renders/review4/static/03_side.png`
- `renders/review4/static/04_back.png`
- `renders/review4/static/05_three_quarter_back.png`

Trunks reference comparisons:

- `renders/review4/trunks-comparison/front_reference_vs_blender.jpg`
- `renders/review4/trunks-comparison/three_quarter_front_reference_vs_blender.jpg`
- `renders/review4/trunks-comparison/side_reference_vs_blender.jpg`
- `renders/review4/trunks-comparison/back_reference_vs_blender.jpg`

Seven material combat poses:

- `renders/review4/combat/01_neutral_guard.png`
- `renders/review4/combat/02_jab.png`
- `renders/review4/combat/03_cross.png`
- `renders/review4/combat/04_slip_left.png`
- `renders/review4/combat/05_slip_right.png`
- `renders/review4/combat/06_slip_counter.png`
- `renders/review4/combat/07_recover_guard.png`

Pose contact sheet:

- `renders/review4/pose-set-contact.jpg`

Matching clay deformation renders are under `renders/review4/combat-clay/`.

## Audit state

- Blender: `5.2.1 LTS`
- Rig: `53` bones
- Body: `13,380` vertices / `26,756` triangles
- Evaluated production geometry: `44,934` vertices / `88,364` triangles
- Materials: `11`
- Image textures: `0`
- Static review pose: `static_stance`

No Unity changes are permitted or required for this round.

## Gate state

| Gate | Internal QA |
| --- | --- |
| Trunks silhouette / thin cloth / two leg openings / waistband | PASS |
| Neutral guard | PASS |
| Jab | PASS |
| Cross | PASS |
| Slip left | PASS |
| Slip right | PASS |
| Slip + counter | PASS |
| Recover guard | PASS |

## Status

`BLENDER_TRUNKS_POSE_PASS_PENDING_HUMAN_UAT`

Absolute stop remains in force: do not export or integrate Ramirez into Unity until human UAT accepts this Blender correction round.
