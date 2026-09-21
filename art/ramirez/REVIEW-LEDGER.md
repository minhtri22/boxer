# Blender review ledger — 2026-09-20 / 2026-09-21

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
