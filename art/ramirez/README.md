# Ramirez — isolated Blender character work

Branch: `art/ramirez-realistic-blender`.
Owner worktree: `D:/WORK/RESEARCH/POVGame/boxer-ramirez-realistic`.

**Status: IN_PROGRESS / NOT_APPROVED. No Blender visual PASS, Human UAT PASS,
Unity integration, or new WebGL delivery is asserted by these studies.**

The user's current instruction is to finish a realistic human character in
Blender, faithful to `docs/handoff/reference-ui/06-opponent-ramirez-turnaround.jpg`,
before further game implementation. Existing gameplay work is preserved separately.

## Reproducible studies

Use Blender 5.2.1 LTS, `--background --python <script> -- <arguments>`.

- `scripts/build_ramirez.py`: candidate A, native CC0 MakeHuman anatomy at a
  uniform 1.80 m stature; editable continuous mesh, native UV atlas, anatomical
  rig, separate equipment, explicit curls and facial hair. `--sculpt-torso`
  enables candidate C, an experimental torso surface transfer.
- `scripts/ramirez_pose.py`: shared Blender-only pose generator. This is an
  anatomical deformation fixture, not gameplay timing or a production animation.
- `scripts/review_poses.py`: reopen a saved candidate, render ten pose samples,
  measure finite coordinates, fixed bone lengths and sole bounds, save a keyed
  pose-study `.blend` and JSON. Numeric checks do not prove anatomical realism.
- `scripts/compare_anatomy.py`: neutral-light comparison of the intact Blender
  Studio realistic male sculpt. Same light setup and 1.80 m stature.
- `scripts/rig_studio_candidate.py`: candidate B, native Blender sculpt with its
  multiresolution data retained and a rig fitted to its anatomy.

## Evidence retained

`reviews/` records successive unapproved studies, including negative results.
Do not treat a newer directory as an automatic PASS. See `REVIEW-LEDGER.md`.

Do not scale individual anatomical axes or stretch the model to legacy Unity
anchors. The art rig and the frozen gameplay arm dimensions have not yet been
reconciled. That is a later integration gate, not permission to change gameplay.

## Source provenance

- Native MakeHuman inputs: existing versioned
  `tools/ev-art-source/makehuman-cc0-inputs.zip`, with its original licenses and hashes.
- Additional muscle target: `source/morph-provenance.json`, CC0 in the source header.
- Skin atlases: selected CC0 assets from the official MakeHuman `skins02` pack;
  `source/skin-provenance.json` records archive and selected-file hashes.
  Authors: MargaretToigo (bronze diffuse), Mindfront (Aksel normal/specular maps).
  Catalog: <https://static.makehumancommunity.org/assets/assetpacks/skins02.html>.
- Blender Human Base Meshes v1.4.1: official Blender download, hash in
  `source/blender-base-provenance-v1.4.1.json`; bundled README states all provided
  assets are CC0. Source README retained with the sculpt comparison. The bundle
  contains a stale unrelated Rain Rig license text; no Rain rig is used here.
  <https://developer.blender.org/docs/release_notes/4.0/asset_bundles/>.

Downloaded archives and all local trials remain on disk. Only selected inputs,
scripts, review evidence and useful editable checkpoints should be committed;
do not stage unrelated worktrees or generated Blender backups.
