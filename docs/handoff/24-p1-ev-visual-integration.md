# P1-EV visual integration experiment

Starting remote HEAD: b0eeed2fcb60006f05956ebca9f211de09608d9d.
User authority: P1-EV autonomous visual integration request, 2026-09-13/14.
Round 2 contact, distance, input vocabulary and AI motion are unchanged.
This request explicitly supersedes the reference's rendered bottom control
buttons; invisible input zones keep their original semantics.

## Question and candidates

A: shaped segmented shell, individual torso volumes, shared leather/skin/satin
materials, Ramirez head UV texture and rig-attached trunks/boots.
Prediction: cheapest reuse, but chest/abdomen intersection seams may continue
to read as an assembly of primitive volumes.

B: same shaped limbs, gloves, face and garments; replace the torso pieces with
one triangulated surface of their existing sphere union, using one rigid torso
anchor. Prediction: continuous silhouette and material eliminate intersection
edges without changing the contact surface or adding per-frame deformation.
The exact torso rotation preserves sphere-union positions because the central
abdomen/neck are rotationally symmetric and the chest targets already rotate.

Compare in the same existing 22-second Editor fixture, capture the same motion
sequence, then check final WebGL at portrait aspect. Both candidates retain actual
feet and the full arm/leg chain. No opaque fighter billboard or pose crossfade.
Select after visual inspection and surface/ownership validation. No intermediate
human approval is needed.

## Baseline and acceptance

Baseline Round2SelfTests.RunAll completed with exit zero on the fetched source.
Final gates: existing 182 assertions; shell/contact-surface and ownership checks;
corrected runtime planted-foot/no-homing fixture; visible face, trunk/boots,
black/gold foreground gloves, compact top HUD, no bottom button graphics; WebGL
startup and runtime; committed-source build, hashes, deployment and one UAT.

## Reference observations

All six approved JPGs and their README/notes/visual-lock were inspected. Primary:
03-pov-combat-hud.jpg (HUD hierarchy, black/gold POV gloves, depth/lighting) and
06-opponent-ramirez-turnaround.jpg (dark hair/beard, athletic silhouette, red
gloves and red/gold trunks, white/red boots). The other four supply shared brand,
materials and arena language only; they do not unlock their screens.

## Iteration evidence

First B render exposed inverted trunk/boot axial placement and TextMesh's UI
font material rendering through geometry; these are integration failures, not
a passing visual candidate. Corrected axial attachments and depth-tested text.
Generated Ramirez head UV asset is a material on the original 3D head, not
screen-space art. A second body texture request failed at the image service;
no image was produced or used for the torso. Body materials use procedural
surface detail instead. No reference file is modified.

Detailed final strategy results and delivery provenance belong in the final
P1-EV execution report after build/runtime validation.

## Selection

A and corrected B both completed the 22-second live fixture with planted feet,
zero committed-root/facing drift and real HIT/BLOCK resolution. Images under
evidence/p1-ev/candidate-a and candidate-b preserve both outcomes. A retains
visible torso-piece boundaries. B permits one shared torso surface/material and
blended shading normals, while its clipped triangles remain on the original
contact union: measured vertex error 0.281185 mm and face-center error 0.199914 mm.
B is selected. This is an engineering visual judgment pending Human UAT, not a
claim of photorealistic anatomy. The first radial-envelope B prototype failed
the 1 mm check near the neck and was replaced, not accepted by relaxing tolerance.

Final refinement adds leather albedo to the same glove volumes and uses the
existing empty championship-arena image only on a depth-tested world scenery
plane behind the physical ring. It contains no fighter and cannot cover
articulated foreground motion. Physical ring ropes/pads and textured floor remain
world geometry. UI font material was replaced by depth-tested attached lettering.
Normal combat has no bottom buttons or always-visible performance text; F3 and
explicit desktop metrics remain available for testing.
