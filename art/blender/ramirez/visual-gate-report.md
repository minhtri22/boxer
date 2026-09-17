# Ramirez Blender Visual Gate

Authority: `docs/handoff/reference-ui/06-opponent-ramirez-turnaround.jpg`

Combat context: `docs/handoff/reference-ui/03-pov-combat-hud.jpg`

This report closes the Blender-only visual-authoring gate. It does not authorize
Unity integration, FBX export, gameplay changes, or human UAT acceptance.

## Evidence

- Static clay: `renders/clay/01_front_neutral.png` through `08_back_silhouette.png`
- Deformation clay: `renders/clay/09_boxing_guard.png` through `17_step_backward.png`
- Material review: `renders/material/18_front_final.png` through `26_slip_final.png`
- Reference comparisons: `renders/comparisons/*_reference_vs_blender.png`
- Source: `source/ramirez_master.blend`
- Asset audit: `asset-audit.json`

## Reference proportions

Reference-derived ratios are recorded in `reference-measurements.md`. The
approved front figure gives approximately: head/height `0.178`, shoulder/height
`0.300`, chest/height `0.256`, waist/height `0.183`, pelvis/height `0.217`, glove
width/height `0.106`, and shoulder/waist `1.64`.

The Blender body is normalized to `1.8011 m`. The current static diagnostic
reports `0.7140 m` shoulder-band width, `0.8377 m` chest-band width, `0.3781 m`
waist width, and `0.4104 m` pelvis width. The shoulder/chest diagnostic bands
intersect the abducted A-pose arms, so those two raw widths are intentionally
not treated as direct equivalents of the posed reference pixel widths. The
front/side/back silhouette comparisons are the visual authority for V01.

## Visual gates

| Gate | Result | Observation |
| --- | --- | --- |
| V01_REFERENCE_PROPORTIONS | PASS | Height is normalized to the 1.80 m target; head, torso, pelvis and leg lengths read in the same adult middleweight band. Front silhouette preserves broad shoulder/chest mass over a compact waist and stable pelvis. |
| V02_HEAD_NECK | PASS | Front, side and 3/4 views show visible neck clearance, jaw separation, sloped trapezius transition and shoulders starting below the neck. No head-to-torso insertion remains. |
| V03_SHOULDER_ANATOMY | PASS | Deltoid volume is continuous with clavicle/chest and upper arm. Jab/cross renders retain shoulder mass without detached shoulder balls or collapse. |
| V04_ARM_MUSCLE | PASS | Upper-arm and forearm taper is visible in neutral, guard and extension; elbows and wrists narrow relative to muscle bellies. Arms no longer read as constant-diameter tubes. |
| V05_TORSO_V_TAPER | PASS | Pectoral shelf, lat width, abdomen and oblique taper remain visible in clay. Front silhouette reads shoulders/chest > waist and side view avoids the previous barrel torso. |
| V06_PELVIS_LEGS | PASS | Pelvis transitions into substantial thighs, visible knee narrowing, calf bellies and narrow ankles. Step-forward/back renders retain lower-body volume without detached feet. |
| V07_BOXING_GUARD | PASS | Guard has asymmetric lead/rear hands, chin behind the gloves, elbows covering the torso, staggered feet and flexed knees. The final guard render no longer reads as the earlier square mannequin pose. |
| V08_DEFORMATION | PASS | Guard, jab, cross, hook, uppercut, slips and both step poses show no crushed neck, shoulder collapse, elbow candy-wrapper failure, torso tearing, severe shorts clipping or head penetration. Corrective smoothing preserves shoulder/elbow continuity. |
| V09_FACE_IDENTITY | LIMITED | Head is now leaner and more rectangular with stronger jaw/chin, lowered brow, curly dark hair and facial-hair treatment, but it remains a stylized MakeHuman-derived likeness rather than a close facial reconstruction of the approved portrait. |
| V10_GEAR | LIMITED | Red fused-shell gloves have believable boxing volume and wrist transition; maroon/gold trunks and high white/red/gold boots match the reference direction. Embroidery, glove crown detail, fabric folds and boot construction remain simplified. |
| V11_MATERIAL_DIRECTION | PASS | Skin, dark curly hair, deep red gloves/trunks, gold trim and white/red/gold boots match the approved palette and remain suitable for eventual real-time optimization. |
| V12_OVERALL_REFERENCE_FIDELITY | PASS | The candidate now preserves the approved fighter's key silhouette, muscular middleweight proportions, visible neck/trapezius chain, arm/leg mass, boxing guard and maroon/gold equipment language. Remaining differences are concentrated in facial likeness and fine gear detail rather than the rejected anatomical foundation. |

## Asset audit

- Evaluated production geometry: `40,354` vertices / `78,772` triangles
- Body mesh: `13,380` vertices / `26,756` triangles
- Materials: `11`
- Rig: `53` bones
- Image textures: `0` (`texture_resolutions: {}`)
- Procedural texture datablocks: `HairSurfaceNoise`
- Excluded from production counts: approved-reference image objects, studio floor,
  lights and camera

## Known differences from the approved reference

- Facial likeness is still generalized; beard/brow treatment is simplified.
- Hair uses a deterministic low-cost curl shell rather than strand grooming.
- Gloves use a fused real-time-friendly shell and simplified gold badge instead
  of the exact crown/stitched construction.
- Trunks do not reproduce the reference satin wrinkles, embroidery and crown
  decoration; boots are simplified but retain the high-support silhouette and
  white/red/gold direction.
- Muscle definition is intentionally lower-frequency than the painted/rendered
  reference; the clay gate prioritizes coherent anatomy and deformation.

## Status

`BLENDER_VISUAL_CANDIDATE_READY_FOR_HUMAN_REVIEW`

Absolute stop remains in force: do not integrate this asset into Unity until
human visual approval is given.
