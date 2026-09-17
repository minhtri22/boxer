# P1 Blender 3D visual upgrade

Status: `IMPLEMENTATION_PASS_PENDING_HUMAN_UAT`

Branch: `p1/whole-body-mechanics`

Protected start baseline: `52be3115641f1cdb57f5c37fcfe24bd3e033b6ab`

## Scope completed

This candidate replaces the active procedural/primitive EV fighter rendering with Blender-authored FBX assets while leaving the qualified Round2 combat, contact, control, and footwork systems authoritative.

Implemented:

- Blender source pipeline in `tools/blender/build_boxer_assets.py`.
- Saved Blender source in `art/blender/source/boxer_uat3_blender_assets.blend`.
- Unity-ready FBX exports for Ramirez and the player POV glove/forearm.
- Full Ramirez rig with root, pelvis, spine, chest, neck/head, bilateral arm, hand, leg, foot, and toe bones.
- Player POV rig with root, forearm, and hand bones.
- Runtime visual follower in `Blender3DVisualFollower.cs`.
- `EVVisualShell` prefers the Blender asset path and retains the prior EV path only as initialization fallback.
- Existing Round2 primitive renderers are hidden while their transforms and colliders remain active for mechanics/contact ownership.
- Blender import and runtime ownership checks added to `EVEvaluation`.

No control mapping, combat rules, input architecture, winner logic, stamina model, PvP networking, or qualified Round2 contact geometry was redesigned.

## Selected implementation direction

Two deformation strategies were exercised during integration:

1. A single auto-weighted continuous skin surface.
2. Smooth overlapping anatomical meshes rigid-bound to the existing Blender rig.

The auto-weighted surface visibly twisted under the large state-driven Round2 bone rotations. The final candidate uses overlapping anatomical sections bound to individual rig bones. This keeps a continuous readable silhouette while preventing shoulder/body tearing and keeps the Round2 transforms as the sole gameplay authority.

The Blender rig is therefore a presentation adapter, not a second mechanics system. `Round2CombatRig` and `Round2Footwork` continue to provide the world-space anchors. The follower updates Blender bones after those mechanics updates.

## Asset outputs

Blender version: `5.2.1 LTS`

Ramirez:

- 10,906 vertices
- 20,676 triangles
- 43 skinned mesh objects
- 22 rig bones

Player POV glove/forearm:

- 1,352 vertices
- 2,680 triangles
- 6 skinned mesh objects
- 3 rig bones

Both remain below the task budgets of 30k triangles for the opponent and 5k triangles for the POV glove asset.

## Reference mapping

Primary references:

- `docs/handoff/reference-ui/03-pov-combat-hud.jpg`
- `docs/handoff/reference-ui/06-opponent-ramirez-turnaround.jpg`

The candidate follows the approved red/gold Ramirez equipment, black/gold POV gloves, boxing guard silhouette, clean combat screen, and ring staging. The approved Ramirez face texture is applied at runtime.

The final asset is a stylized low-poly approximation of the approved turnaround. It is materially better than the primitive/skeletal prototype and is stable under the current mechanics, but it is not a production-quality realistic character sculpt. It does not yet have artist-authored normal/roughness maps, realistic muscle deformation, or bespoke authored animation clips.

## Qualification evidence

Evidence is frozen under `evidence/p1-blender3d/` so generic Round2 evidence files can be restored to their committed state.

Deterministic qualification:

- Round2 deterministic suite: `43/43 PASS`.
- Contact matrix: `288/288`, zero result disagreements against the dense oracle.
- Blender/EV visual gates: `21/21 PASS`.
- Contact-union geometry remains within the previous 1 mm visual-shell tolerance.

Runtime graphics qualification:

- 1,202 audited frames.
- 0 failed frames.
- `visual_path=blender_fbx_follow_round2`.
- maximum hand-anchor error `8.36591e-07 m`.
- `RUNTIME_INVARIANTS=PASS`.
- committed root drift `0 m`.
- committed rotation drift `0 degrees`.
- Editor rig timing: mean `0.05254 ms`, max `3.74560 ms`; this is not WebGL or device frame-time evidence.

Unity import audit:

- Ramirez: 67 transforms, 43 skinned meshes, 20,676 triangles.
- POV asset: 11 transforms, 6 skinned meshes, 2,680 triangles.
- `BLENDER_3D_ASSET_AUDIT=PASS`.

Evidence files:

- `evidence/p1-blender3d/blender-asset-manifest.json`
- `evidence/p1-blender3d/blender-ramirez-preview.png`
- `evidence/p1-blender3d/geometry-tests.txt`
- `evidence/p1-blender3d/round2-tests.txt`
- `evidence/p1-blender3d/contact-matrix.csv`
- `evidence/p1-blender3d/ownership-runtime.txt`
- `evidence/p1-blender3d/runtime.txt`
- `evidence/p1-blender3d/unity-tests.log`
- `evidence/p1-blender3d/unity-runtime.log`
- `evidence/p1-blender3d/unity-asset-audit.log`
- `evidence/p1-blender3d/motion/motion-00.png` through `motion-09.png`

## Known limitations and remaining UAT

- Human visual acceptance on the real target device is still required.
- The model remains a simplified stylized fighter, not a final realistic character asset.
- Current punch/guard/footwork visuals are driven by the validated procedural Round2 anchors; there are no authored Blender animation clips in this round.
- Audio was not expanded because the existing task priority is the 3D visual upgrade and the current pipeline did not require audio changes for this qualification.
- WebGL qualification must be performed from a clean checkout of the committed candidate SHA before this handoff is considered build-complete.

The required next UAT is a real-device Round 2 visual check focusing on glove framing, opponent silhouette, guard readability, punch sharpness, perceived distance, and foot/hip motion.
