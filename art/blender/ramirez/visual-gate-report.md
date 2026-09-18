# Ramirez Blender Visual Gate — Review 3

Primary visual authority: `docs/handoff/reference-ui/06-opponent-ramirez-turnaround.jpg`

Combat context: `docs/handoff/reference-ui/03-pov-combat-hud.jpg`

Motion grounding is documented in `review3-reference-grounding.md`.

## Review history

### Human Review 1 — FAIL

The first Blender candidate was rejected by human review for rigid/block shorts, mannequin-like static stance, weak punch/slip mechanics, insufficient hip/shoulder rotation and weight transfer, and incorrect combat footwork.

### Agent preflight Review 2 — FAIL

Review 2 corrected the stance base, split-leg trunks and the requested seven combat poses, but the review renders still exposed hard visual defects before handoff:

- hair read as separate beads/placeholder curls;
- boot uppers still read as rounded rectangular proxy blocks;
- glove gold badges could visibly detach from the glove in rotated views and the glove silhouette remained too capsule-like;
- gold treatment on the trunks covered too much of the outer leg and read as a rigid panel;
- cross and slip + counter needed a clearer retained lead-hand guard and stronger crouched boxing read.

Review 2 therefore remains `FAIL`; it is not a human-approved gate.

## Review 3 corrections

- Removed separate ico-sphere hair curls and retained a single displaced continuous short-curly hair surface.
- Rebuilt boot uppers from rounded continuous shells; changed cuff/tongue treatment, reduced lace bulk and replaced block side trim with a curved gold stripe.
- Attached each glove badge to its glove and rotated the glove shell/cuff with the forearm direction.
- Narrowed the trunks and reduced the gold side/hem region to textile trim instead of a broad rigid-looking panel; increased low-frequency cloth waviness at the leg openings.
- Pulled the lead hand back toward face guard for cross and slip + counter.
- Deepened the common boxing crouch and chin tuck while preserving staggered stance, rear-foot pivot and hip/shoulder rotation.
- Kept the source file saved in `static_stance`.

## Review 3 evidence

Static set — five required views:

- `renders/review3/static/01_front.png`
- `renders/review3/static/02_three_quarter_front.png`
- `renders/review3/static/03_side.png`
- `renders/review3/static/04_back.png`
- `renders/review3/static/05_three_quarter_back.png`

Material combat set — seven requested poses:

- `renders/review3/combat/01_neutral_guard.png`
- `renders/review3/combat/02_jab.png`
- `renders/review3/combat/03_cross.png`
- `renders/review3/combat/04_slip_left.png`
- `renders/review3/combat/05_slip_right.png`
- `renders/review3/combat/06_slip_counter.png`
- `renders/review3/combat/07_recover_guard.png`

Clay deformation set:

- matching seven poses under `renders/review3/combat-clay/`.

Contact sheets:

- `renders/review3/contact-static.jpg`
- `renders/review3/contact-combat.jpg`

Source and audits:

- `source/ramirez_master.blend`
- `rig-metrics.json`
- `asset-audit.json`

## Audit state

- Blender: `5.2.1 LTS`
- Rig: `53` bones
- Body: `13,380` vertices / `26,756` triangles
- Evaluated production geometry: `33,030` vertices / `64,556` triangles
- Materials: `11`
- Image textures: `0`
- Static review pose: `static_stance`

## Current gate state

| Gate | State | Evidence |
| --- | --- | --- |
| Static 5-view set | PENDING_HUMAN_REVIEW | `renders/review3/static/` |
| Boxing trunks | PENDING_HUMAN_REVIEW | static + combat sets |
| Neutral guard | PENDING_HUMAN_REVIEW | `combat/01_neutral_guard.png` |
| Jab | PENDING_HUMAN_REVIEW | `combat/02_jab.png` |
| Cross | PENDING_HUMAN_REVIEW | `combat/03_cross.png` |
| Slip left | PENDING_HUMAN_REVIEW | `combat/04_slip_left.png` |
| Slip right | PENDING_HUMAN_REVIEW | `combat/05_slip_right.png` |
| Slip + counter | PENDING_HUMAN_REVIEW | `combat/06_slip_counter.png` |
| Recover to guard | PENDING_HUMAN_REVIEW | `combat/07_recover_guard.png` |

The asset remains stylized and simplified relative to the approved Ramirez reference, especially facial likeness and fine cloth/glove/boot construction. No agent-side result is recorded as a human PASS.

## Status

`BLENDER_REVIEW3_READY_FOR_HUMAN_REVIEW`

Absolute stop remains in force: do not export or integrate Ramirez into Unity until the Blender static and combat-pose gates receive human approval.
