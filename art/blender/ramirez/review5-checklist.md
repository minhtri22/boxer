# Ramirez Review 5 — Verification Checklist

The gate passes only when **machine checks and visual checks both pass**.

## A. Trunks

- [ ] Fabric thickness <= 0.002 m.
- [ ] Both leg shells have zero unweighted vertices.
- [ ] Left/right leg openings remain physically separate.
- [ ] Lower leg opening is wider than the upper hip ring enough to read as cloth flare, not a tube.
- [ ] Hem has measurable drape/split variation; it is not a flat horizontal cut.
- [ ] Waistband height remains within the approved boxing range.
- [ ] Front, 3/4, side and back views do not read as a trapezoid block, skirt or hard shell.
- [ ] Side split/seam follows the cloth silhouette and does not become a broad rigid plate.

## B. Neutral guard

- [ ] Pelvis is visibly lowered and both knees are flexed.
- [ ] Torso has a clear but controlled forward crouch.
- [ ] Chin is tucked inside the defensive structure.
- [ ] Feet form a grounded orthodox split stance.
- [ ] Gloves sit at cheek/head height and elbows protect the torso.

## C. Jab

- [ ] Lead hand extends on a direct target line.
- [ ] Rear hand remains at guard.
- [ ] Lead shoulder contributes to chin protection.
- [ ] Lead side advances from the body, not arm-only reach.
- [ ] Both feet remain grounded.

## D. Cross

- [ ] Rear hand reaches the target line.
- [ ] Lead hand remains at guard.
- [ ] Rear heel visibly releases and foot visibly pivots.
- [ ] Rear knee/hip drives forward.
- [ ] Hip and shoulder rotation are visibly stronger than neutral guard.
- [ ] Weight transfer is readable without losing balance.

## E. Slip left/right

- [ ] Head clearly exits the centerline.
- [ ] Pelvis moves laterally with the slip.
- [ ] Knees load asymmetrically.
- [ ] Torso participates; the motion does not read as neck-only or waist-only lean.
- [ ] Both feet remain grounded.

## F. Slip + counter

- [ ] Head stays off the original centerline during the loaded phase.
- [ ] Base remains compressed and stable.
- [ ] Rear-side hip/shoulder drive supports the counter.
- [ ] Rear foot participates in the counter.
- [ ] Pose does not read as “lean sideways + extend arm”.

## G. Recover

- [ ] Hands return close to neutral guard.
- [ ] Feet return to the stable guard base.
- [ ] Torso remains crouched; there is no upright mannequin reset.

## H. Evidence and scope

- [ ] Five static views exist under `renders/review5/static/`.
- [ ] Seven combat poses exist under `renders/review5/combat/`.
- [ ] Seven clay poses exist under `renders/review5/combat-clay/`.
- [ ] Four trunks reference comparisons exist under `renders/review5/trunks-comparison/`.
- [ ] Review5 contact sheet exists.
- [ ] No Unity changes are present.

## I. Mandatory visual-reference gate

This section is the final authority. Machine metrics cannot override it.

- [ ] Compare `renders/review5/static/` directly against `06-opponent-ramirez-turnaround.jpg` at front, 3/4, side and back.
- [ ] Compare all seven `renders/review5/combat/` poses against `03-pov-combat-hud.jpg` plus the locked boxing-motion references.
- [ ] No shoulder, upper-arm, elbow, wrist, hip, knee or ankle reads as anatomically displaced.
- [ ] No arm, glove or shoulder intersects, fuses with, or visibly drags the chest/torso mesh.
- [ ] Left/right limbs remain clearly separated from the torso where the reference shows separation.
- [ ] Guard, jab, cross, slips, slip-counter and recover all read as plausible boxing poses at normal viewing distance.
- [ ] Trunks silhouette remains visually comparable to the approved reference from every required view.
- [ ] A human visual review has explicitly recorded PASS in `review5-visual-gate.json`.

## Final rule

- Any unchecked item => `BLENDER_CORRECTION_FAIL`.
- Numeric/machine QA PASS + visual-reference gate FAIL => `BLENDER_CORRECTION_FAIL`.
- All items checked => `BLENDER_TRUNKS_POSE_PASS_PENDING_HUMAN_UAT`.
