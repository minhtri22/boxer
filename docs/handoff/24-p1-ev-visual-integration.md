# P1-EV visual integration experiment

STARTING_SHA: b0eeed2fcb60006f05956ebca9f211de09608d9d
Branch: p1/whole-body-mechanics. This execution is independent of other worktrees.
Authority: user P1-EV visual integration request and autonomous Round2 amendment.

## Root causes

The historical visual rig uses intersecting torso spheres and a spherical head.
Better materials cannot turn their silhouette into the approved athletic boxer.
Projecting facial art onto that sphere stretches the face. A full-frame image
would conceal the solved motion, so is not an acceptable fallback. An anatomical
head is narrower than the former sphere: retaining that sphere reports early
contact. The measured initial oracle grid finds 199 such cases (over 1 mm).

## Controlled candidates

CANDIDATE_ID: A — shaped segmented pieces
HYPOTHESIS: shaped primitives/materials are sufficient for the intended boxer.
IMPLEMENTATION_DELTA: tapered limbs, leather gloves, red/gold trunks, white boots,
UV face, separate historical torso volumes.
EXPECTED_EFFECT: cheapest visual replacement with unchanged contact geometry.
CONTACT_RESULT: historical resolver unchanged.
DISTANCE_RESULT: unchanged historical spacing.
MOVEMENT_RESULT: real rig/feet retained; 22-second motion fixture passes.
REGRESSION_RESULT: historical baseline corpus passes.
PERFORMANCE_RESULT: Editor rig mean 0.08086 ms; not a WebGL comparison.
FAILURE_MODE: obvious separate torso volumes and distorted face on a sphere.
VERDICT: rejected for reference fidelity, despite motion invariants passing.

CANDIDATE_ID: B — continuous original sphere union
HYPOTHESIS: removing intersections will make the torso sufficiently coherent.
IMPLEMENTATION_DELTA: one clipped/welded sphere-union torso and blended normals.
EXPECTED_EFFECT: continuous surface without changing the original contact union.
CONTACT_RESULT: vertex error 0.281185 mm; face-center error 0.199914 mm. Earlier
radial-envelope version failed 1 mm and was replaced, not waived.
DISTANCE_RESULT: unchanged historical spacing.
MOVEMENT_RESULT: 1163-frame fixture passes, feet remain articulated.
REGRESSION_RESULT: six added geometry checks pass.
PERFORMANCE_RESULT: Editor rig mean 0.08866 ms; separate run, no causal speed claim.
FAILURE_MODE: continuous but bulbous torso; spherical head still distorts the face.
VERDICT: initially preferred over A, then rejected on closer reference inspection.

CANDIDATE_ID: C — anatomical mesh on existing solved joints
HYPOTHESIS: anatomical topology and landmark-based UVs solve the silhouette/face
problem while lightweight skinning preserves real movement.
IMPLEMENTATION_DELTA: CC0 anatomical mesh, ten bone followers, one skinned body/head
surface; attached hair/eyes, gloves, trunks and boots. No Animator or new IK.
EXPECTED_EFFECT: coherent athletic torso/limbs and an undistorted head shape.
CONTACT_RESULT: existing relative glove sweep now confirms opponent head/torso
against the same rigid visible triangles using a static BVH. Guard pads retain
exact original geometry. Independent PhysX oracle: 578 cases, zero disagreement
in the corrected candidate, maximum surface gap 0.009857 mm. Initial two-sided
query produced 26 false contacts on rear faces through non-target neck openings;
approach-facing triangles fixed that error. The failed result is preserved.
DISTANCE_RESULT: LONG/BOXING/CLOSE and speeds/hysteresis remain. AI preferred
distance is derived once from actual neutral-jab head reach minus one quarter
of the glove radius, clamped inside BOXING. It replaces the obsolete 0.955 m
sphere-based stand-off. Head hits require physical reach; guard reach is longer.
MOVEMENT_RESULT: shared shoulders/elbows/wrists/hips/knees/ankles remain authoritative;
no competing writers, no root-only sliding, no commitment homing.
REGRESSION_RESULT: existing 182 assertions plus focused surface/contact/attachment
checks. Final committed-source outputs are recorded in the delivery report.
PERFORMANCE_RESULT: measured warmed narrow-phase sweep about 10 microseconds in
Editor and zero allocation; bone-follow loop about 0.007 ms and zero allocation.
Final WebGL measurements are separate; Editor values are not mobile FPS claims.
FAILURE_MODE: early UV/hair/finger/garment edge issues found and corrected in
motion captures. No finished facial animation or AAA hair simulation is claimed.
VERDICT: selected. A/B/C images and negative results remain in evidence/p1-ev.

## Contact and transform ownership

Controllers still own input, timing and root motion. Round2CombatRig still owns
solved joints, glove paths and combat resolution; Round2Footwork owns foot plants.
EVAnatomy reads these anchors into ten skin bones. EVVisualShell owns only its
materials and attached details. Replaced primitive renderers are disabled because
the skinned mesh is their sole visible replacement. No fighter billboard is drawn.

The BVH is built once from the packaged mesh. Per-sample conservative advancement
uses the distance function's Lipschitz bound, approach-facing triangles, and a
10 micrometre termination tolerance. Target motion is transformed into local space
at the existing <=1/240 second pose intervals. No runtime PhysX queries are added.
The independent oracle uses PhysX only inside Editor tests. The bounded-iteration
counter is checked, rather than silently treating an exhausted sweep as verified.

M05 remains a 1 mm numerical/sampling gate: the historical maximum curve-to-chord
error is 0.595 mm, new narrow-phase error <=0.01 mm, with the remainder allowing
floating-point/transform error. The visible scoring surfaces are the glove pad,
head skin and rigid chest/abdomen faces. Hair, eyes, cuffs, clothing and blended
arm/neck transitions are decorative or non-scoring, not extra damage targets.
Hair thickness is <=5.5 mm and does not enlarge head reach. Guard/HP/stamina/counter
rules and punch-family/A1/A3.1 relationships remain unchanged. Historical sphere
results are preserved; the new invariant is surface confirmation, not old numbers.

## Reference and presentation

All six approved JPGs were inspected: 01 character customization, 02 home,
03 POV combat HUD, 04 venues, 05 training, 06 Ramirez turnaround. Primary visual
guides are 03 and 06. No reference file was resized or overwritten.

The foreground player gloves use black leather and gold trim, Ramirez uses red
leather, red/gold trunks and white/red boots. Distant arena art is an empty,
depth-tested world background behind the physical ring, never over the fighter.
The compact top HUD preserves HP/stamina/round hierarchy. Bottom control graphics
are removed; touch input is unchanged. Debug metrics require explicit desktop
validation flags and do not appear in the normal UAT view.

## Finalization contract

One integrated UAT only. Final delivery must include committed source, fresh
WebGL output, productVersion/source marker, file hashes, runtime smoke, M01–M12,
performance evidence and branch push. No Human UAT PASS, PvP, replay, ring intro
or secondary screens. Source input/license provenance: tools/ev-art-source/README.md.
