# Blender review ledger — 2026-09-20 through 2026-09-27

Acceptance authority: realistic human anatomy and faithful reference appearance.
The observations below are internal review, not human approval.

| Study | Observation | Decision |
|---|---|---|
| clay01 | Native uniform human proportions avoid the old wide-shoulder runtime stretch; torso is too smooth/slight. | Continue anatomy work. |
| clay02 | First equipment exposes fingers and toes; glove profile elongated. | Reject these overlaps; trim occluded skin, revise glove. |
| material03 | Skin reads plastic; facial hair placed too high; waistband fit and gold boundaries poor. | Reject as final; preserve images. |
| material04 | Correct UV atlas and facial landmarks improve face; cloth/skin still synthetic. | No visual pass. |
| mass05 | More muscle mass; simpler numeric mass increase does not establish reference fidelity. | Retain only as study. |
| stance06 | More readable staggered stance and knees; gold strip intersects cloth at side. | Revise actual panel surface sampling. |
| detail07 | Glove orientation/logo improved; hairline has polygon steps, and seams can still protrude. | Reject as final; replace per-face scalp boundary with smooth density and project seams onto subdivided shell. |
| poses08 | Ten static pose samples render; finite skin coordinates and stable bone lengths. Hook shoulder/arm silhouette and synthetic surface appearance remain concerns. | Test evidence only; not a boxing animation or visual pass. |
| sculpt-candidate09 | Intact Blender sculpt has more convincing connected chest/shoulder/abdominal volumes than the smooth candidate A. | Test fitted rig while preserving sculpt. |
| hybrid10-clay vs baseline11-clay | Same candidate-A topology, camera, lighting, rest pose. Torso transfer moves 1,154 vertices (max 92.8 mm) but fails to preserve the intact sculpt's detail and proportions. | Reject C as production anatomy strategy. |
| studio12-rig | Intact sculpt retains stronger torso anatomy, but wrists have gaps and calves intersect boots. | Reject as finished rig. |
| studio13-fit | Removing hand-weight masking did not remove wrist gaps. | The first masking hypothesis was insufficient. |
| studio14-material | Nearest-surface UV transfer creates red eyelid/nose patches, stripes and misplaced pigmentation. | Reject atlas transfer; preserve native sculpt UVs. |
| studio15-joints | Measured wrist/ankle centres improve alignment but gaps persist; translating boot Z raises soles. | Reject this equipment fit. |
| studio16-continuity | Restricting the shorts mask to pelvis/thigh X bounds restores forearms. Calf-fitted shaft and independent sole height remove obvious boot float. Open fingers are now exposed. | Confirm global-Z masking as wrist root cause. Distal hand skin must not compete with visible boxing glove. |
| studio17-equipment | Distal skin omitted only beyond fitted wrist inside the cuff. Glove is the single visible hand. Rebuilding gold tape from evaluated cloth boundary removes the large sawtooth hem. | Guard render improves continuity; likeness/materials and dynamic deformation remain unapproved. |
| studio18-native-lookdev | Native procedural pigment avoids broken UV interpolation, but skin is waxy, hair/brows generic and wraps intersect forearms. | Reject as finished appearance. |
| studio19-pose-audit | Explicit dependency-graph invalidation and frame advancement produce a distinct rendered jab. Evaluated left glove centre changes from approximately (0.115,-0.344,1.402) to (0.100,-0.672,1.404) metres and remains there after rendering. | Retain render-state fix. Earlier nearly identical pose stills cannot prove motion coverage. |
| studio20-wrap-follow | Forearm attachment corrects cuff orientation, but material close-up reveals intersection/end-cap teeth. | Correct radius from measured skin sections and remove redundant cloth end caps. |
| studio21-reference-forms | New user front/side/back references packed in Blender. Local sculpt changes max 14.8 mm; bone lengths unchanged; shorter boot shafts/shorts and chin tuck. | Useful editable form checkpoint. Face, musculature, glove silhouette and clothing are still visibly short of reference quality. |
| studio22-sampled-skin | Resolving donor colours before interpolation removes some UV contour artifacts, but anatomical correspondence still gives wrong red eye/nose patches. 169,362 colour vertices add cost without adequate visual benefit. | Reject cross-character nearest-surface colour transfer. Do not keep increasing sample density. |
| studio23-wrap-surface | Skin-section fitting and open cloth sleeve remove prominent cuff end-cap teeth. | Retain geometric fix. Reprojected glove decals need outward-normal validation. |
| studio24-cloth-drape | Offline 36-frame simulation takes 91.25 s for 7,551 garment vertices, max displacement 102 mm. Rendered shorts tighten/rise and trim becomes jagged. | Reject simulated shape; retain pre-drape garment. No runtime cloth introduced. |

## Serious candidates

**A — native MakeHuman plus local sculpt fields.** Good editable topology, UVs and
weight provenance. Guard and ten pose fixtures exist. Rejected as a finished
realistic Ramirez because the rendered anatomy/surface/face still look synthetic.

**B — intact Blender realistic sculpt with a fitted rig.** Prediction: retain the
stronger connected anatomical volumes visible in the neutral comparison; skinning
may require corrective shapes and equipment refitting. Rig experiment is active;
static continuity has improved but no final visual or motion pass is established.
Do not infer final selection from a good unrigged screenshot.

**C — transfer only B's torso surface onto A.** Prediction: gain anatomical detail
while retaining A's UVs/rig. Controlled clay renders show loss of the intact
sculpt's detail; the relatively coarse cage and transition regions undermine the
benefit. Rejected. Original sculpt and A evidence remain intact.

## Open visual gates

- Reference likeness: muscular middleweight, face/head/hair silhouette, skin detail.
- Realistic red leather gloves, wraps, red/gold satin, white/red boots.
- Shoulder/elbow/hip/knee deformation without collapsed or swollen joints.
- Guard, punch, recovery, planted/moving feet and head movement in Blender.
- No cloth/skin intersections, floating seams, double anatomical representations.
- Reconcile art anatomy with frozen gameplay proportions before Unity integration.
- Bake/optimize render assets for WebGL only after the Blender visual gate.

The 7 mm sole clearance in the candidate-A pose report is an authoring offset,
not proof of correct planted contact. It must be removed in a final stance.

## New reference authority and remaining work

The user supplied ten viewport screenshots on 2026-09-27; exact copies and a
reading are in `reference/20260927/`. The user confirmed these were captured from
Tripo and that no downloadable source model is available. No asset was retrieved
from Tripo. The screenshots augment the approved Ramirez identity reference;
they do not lower the realistic-human acceptance gate.

The work remains in Blender. No Unity code, UAT build, deployment or final gate
approval follows from these experiments. Current appearance is NOT_APPROVED.

## 29 September continuation

The native skin UVs use multiple UDIM tiles. Head tile (0,0), with its mirrored
partner (0,2), was rendered as an exact texture-authoring guide. The built-in
imagegen tool was called with that guide and user references 21/25; it failed
with HTTP 404. No generated texture exists, and no paid API fallback was used.
This failed attempt is not a material-quality result.

`working/ramirez-reference-workbench.blend` is the single editable continuation
asset: reference-fitted sculpt, packed reference boards, fitted open wraps,
preserved garment shape from before the failed cloth drape, and corrected outward
glove surface projection. Numerical pose validation does not change the open
reference-fidelity gate. See `working/workbench-report.json` for the actual check
results and file hash. The skin/face/hair and clothing appearance remain unfinished.
