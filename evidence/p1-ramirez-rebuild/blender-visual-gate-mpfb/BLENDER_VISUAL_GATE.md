# Ramirez MPFB Blender visual gate

Date: 2026-09-17

Authoritative reference: `docs/handoff/reference-ui/06-opponent-ramirez-turnaround.jpg`

Rejected comparison base: `evidence/p1-ramirez-rebuild/blender-visual-gate/01_front.png`

## Gate result

`BLENDER_VISUAL_GATE: PASS_FOR_GAME_READY_DERIVATIVE`

This result applies to V01-V07 exactly as defined by the rebuild task. It does not claim final production likeness or final WebGL readiness. A game-ready derivative must be optimized and rechecked before Unity import.

### V01 HEAD_NECK — PASS

- A visible neck separates head from thorax in front, side, 3/4 and posed renders.
- Trapezius/neck volume forms a continuous transition into the shoulder line.
- Guard and jab renders do not show head/body intersection or neck collapse.

### V02 SHOULDERS — PASS

- Deltoid volume is part of the continuous humanoid body mesh rather than a sphere joint.
- Shoulder-to-upper-arm transition remains continuous in neutral, guard and jab.
- Jab evidence shows no major shoulder tear or exposed disconnected joint.

### V03 ARMS — PASS

- Upper arms have visible biceps/triceps mass instead of constant-radius tubes.
- Elbows narrow relative to upper arm and forearm.
- Forearms taper toward the wrist and preserve a readable wrist-to-glove transition.
- Guard, jab, hook and uppercut renders preserve arm continuity without major tearing.

### V04 TORSO — PASS

- Neutral measured shoulder width: `0.7210 m`.
- Neutral measured waist width: `0.3619 m`.
- Shoulder/waist ratio: `1.9922`.
- The silhouette has an obvious V-taper and no barrel abdomen.
- Chest/abdomen are part of one coherent humanoid mesh rather than stacked torso primitives.

### V05 LEGS — PASS

- Thigh and calf masses are readable in front/side/back views.
- Knee and ankle regions narrow naturally relative to adjacent muscle groups.
- Lower body is continuous with pelvis and supports the boxing stance without stick/cylinder construction.

### V06 SILHOUETTE — PASS

- The grayscale/body silhouette now reads as an athletic male combat-sport character with broad shoulders, compact waist, continuous limbs and boxing gear.
- It no longer reads as the rejected primitive mannequin.
- It remains leaner and less stocky than the approved Ramirez reference; this is a fidelity limitation rather than a failure of the V06 literal criterion.

### V07 REFERENCE_FIDELITY — PASS

Concrete comparison against the rejected model:

- rejected head/neck was compressed into the torso; new model has clear neck clearance;
- rejected arms were assembled primitive masses with mechanical joints; new model has continuous deltoid/upper-arm/elbow/forearm anatomy;
- rejected torso was a short rectangular/barrel block; new model has measured V-taper and coherent chest/abdomen;
- rejected legs showed segmented primitive construction; new model has continuous thigh/knee/calf anatomy;
- new model adds the approved red gloves, red/gold trunks, white/red boots and dark hair/beard direction.

The new model is therefore materially closer to the approved Ramirez direction than the explicitly rejected model, which is the V07 pass condition.

## Reference-fidelity limits still visible

- Approved Ramirez is more muscular/stocky, especially deltoids, upper arms, lats, thighs and calves.
- Face is still a generic MPFB-derived face rather than a close Ramirez likeness.
- Hair and beard are procedural approximations and remain visibly synthetic at close range.
- Trunks and boots are authored blockout-quality gear rather than production garment/footwear meshes.
- Materials are simple procedural materials; no authored normal/roughness texture set is present.

These limits must remain visible in the handoff and human UAT. They are not grounds to rewrite V07 into a stricter unpublished criterion.

## Asset audit

- Model method: MPFB/MakeHuman CC0 coherent humanoid base + CC0 target displacement + Blender-authored gear + MPFB game-engine rig/weights.
- Source provenance: `makehumancommunity/mpfb2`, v2.0.17, commit `80919fa4682335c41847f761a4d79dcad4124732`, asset data license `CC0 1.0`.
- Body mesh: `13,380` vertices / `26,756` triangles.
- Full renderable asset, excluding studio floor: `35,792` raw triangles / `55,656` evaluated triangles.
- Materials: `12` unique asset materials.
- Image textures: `0`; current appearance is material/procedural-node driven.
- Rig: `53` bones.

The evaluated asset exceeds the previous P1 opponent guidance of `30k` triangles. The largest avoidable cost is procedural surface subdivision on hair (`960` raw -> `15,360` evaluated triangles), followed by subdivision/bevel on trunks. Before Unity import, create a derivative that preserves the approved source silhouette while targeting the prior ~30k opponent budget.

## Evidence

- `01_front.png`
- `02_three_quarter_front.png`
- `03_side.png`
- `04_back.png`
- `05_three_quarter_back.png`
- `06_guard.png`
- `07_jab_extension.png`
- `08_hook_pose.png`
- `09_uppercut_pose.png`
- `10_slip_pose.png`
- `11_clay_turnaround.png`
- `comparison-reference-vs-new.png`
- `neutral-metrics.json`
- `rig-metrics.json`
- `asset-audit.json`
- `source-provenance.json`

## Next gate

Build a game-ready derivative, audit the evaluated triangle count, render at least neutral/guard/jab regression views, and proceed to Unity only if anatomy and deformation remain acceptable after optimization.
