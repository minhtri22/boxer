# Boxer — Ramirez Unity integration handoff

Date: 2026-09-30
Repository: `minhtri22/boxer`
Authority: the product owner explicitly accepted the current Ramirez character
and authorized progression to a Unity candidate.

## Handoff verdict

`RAMIREZ_ART_ACCEPTED_FOR_UNITY_INTEGRATION`

The next agent must integrate the current editable Ramirez into Unity and finish
one provenance-locked Round 2 UAT candidate. Do not reopen a general Blender
redesign phase. Fix a Blender asset only when export or runtime evidence proves a
specific integration defect such as missing weights, inverted normals, broken
bone mapping, detached equipment, or invalid scale.

This approval supersedes the earlier sequencing rule that blocked Unity work
until further Blender fidelity work. Historical study reports remain unchanged
because they truthfully record the state and verdict at the time they ran.

This approval is not:

- a Human UAT PASS for the Unity game;
- permission to change frozen combat semantics;
- permission to import the other agent's backup/experiment branch;
- permission to start PvP, replay, matchmaking, career, or secondary screens.

## Immutable input

Art source commit:
`3e4e13c4b4451cb542e068652508ec12e248225f`

Editable Blender asset:
`art/ramirez/working/ramirez-reference-workbench.blend`

SHA-256:
`419466859d7f119f98d7e69dc2673f0b378dbf5a892f0e2a6d7254cbc974af12`

Evidence/report:
`art/ramirez/working/workbench-report.json`

Report SHA-256:
`c30584adb6aa87c373ac06d1ee624ebd2fbaf4f470cb70f048df55de776289f3`

Review renders:

- `art/ramirez/working/guard.png` —
  `7437b67de3d4690ad79bfdfa4720c4daf1efb0361bbfbb6e9db65f4df47e17fa`
- `art/ramirez/working/jab.png` —
  `c0586e8b14ff58e054fe4ec8cd8294ce6800242bff5ad9adf55328c5bd5db98d`

The workbench contains the native 1.80 m sculpt, `RAMIREZ_RIG`, fitted wraps,
gloves, shorts, boots, packed user reference boards, and explicit unfinished
status notes. Reference boards, cameras, lights, studio floor, and temporary QA
objects are authoring aids and must not be exported as game geometry.

Ten Blender pose checks are finite. Maximum measured bone-length error is
`3.3961e-7 m`. The left-glove guard-to-jab centre displacement is `0.327683 m`.
Planted sole height is approximately `0.055 mm`; the stepping foot lifts about
`15.055 mm`. These are engineering checks, not proof of Unity motion quality.

Rejected inputs that must not replace the workbench:

- `studio22-sampled-skin`: invalid cross-character colour correspondence around
  the eyes and nose;
- `studio24-cloth-drape`: garment shrank/ridged after a 36-frame offline drape;
- `experiment/other-agent-backup`: isolated backup/experiment branch, not an
  integration source;
- historical primitive/sphere character shells as the visible final opponent.

## Required branch and worktree isolation

The verified Unity baseline is:

`bc0b2129dae47deeb3ce29d548624cf5571d0f36`

That commit is the current remote `origin/p1/whole-body-mechanics` at handoff
time. The local shared checkout at `93c0ef59...` is behind it and must not be
used as the integration base.

Create a new branch and worktree; do not work in either existing agent's
worktree:

```powershell
Set-Location -LiteralPath 'D:\WORK\RESEARCH\POVGame\boxer-ramirez-realistic'
git fetch origin
git rev-parse origin/p1/whole-body-mechanics
git worktree add -b integration/ramirez-unity-round2 `
  'D:\WORK\RESEARCH\POVGame\boxer-ramirez-unity' `
  bc0b2129dae47deeb3ce29d548624cf5571d0f36
Set-Location -LiteralPath 'D:\WORK\RESEARCH\POVGame\boxer-ramirez-unity'
git rev-parse --show-toplevel
git branch --show-current
git status --short --branch
git restore --source 3e4e13c4b4451cb542e068652508ec12e248225f -- art/ramirez
git add -- art/ramirez
git commit -m "art: import owner-approved Ramirez Unity source"
```

Expected new branch: `integration/ramirez-unity-round2`.
Expected new worktree: `D:/WORK/RESEARCH/POVGame/boxer-ramirez-unity`.

Do not merge or cherry-pick `art/ramirez-realistic-blender` wholesale. That
branch diverges from the Unity baseline and contains recovered historical WIP.
Import only `art/ramirez` from the exact art source commit above. Do not write to
or push `p1/whole-body-mechanics`, `art/ramirez-realistic-blender`,
`experiment/other-agent-backup`, or `main`.

Push only:

```powershell
git push origin HEAD:refs/heads/integration/ramirez-unity-round2
```

No force push, `--all`, `--mirror`, rebase after the final provenance build, or
implicit import from another worktree.

## Unity environment

- Project: `unity/BoxerP0`
- Unity: `6000.5.8f1` (`5cb7df797b7d`)
- Product: `Boxer P0`
- Build entry point: `BoxerP0.Editor.Round2Build.Web`
- Web output: `builds/web/boxer-round2`
- Asset audit baseline: `BoxerP0.Editor.Blender3DAssetAudit.Run`
- Existing opponent resource contract:
  `Assets/Resources/Boxer3D/Ramirez_UAT3`

Use Blender 5.2.1 LTS for a deterministic FBX export. Do not depend on Unity's
implicit `.blend` conversion. Add a versioned export script and record the source
`.blend` hash, Blender version, export settings, FBX hash, vertex/triangle counts,
bone names, bind-pose scale, and material slots.

Export only the accepted fighter representation:

- `RAMIREZ_RIG`;
- the continuous body/head sculpt;
- eyes if retained by the selected Unity material path;
- gloves and wraps;
- shorts/waistband/gold trim;
- boots, soles, and laces.

Exclude studio cameras/lights/floor, all `REFERENCE` objects/collections, review
markers, and obsolete/hidden duplicate anatomy. There must be one visible owner
per anatomical component.

## Integration objective

Replace the current Unity opponent's visible character with the accepted
Ramirez while retaining the established Round 2 control and combat authority.
The final runtime view must show actual articulated body motion, stance and feet.

Required ownership:

- controllers own input, action timing, root movement, HP, stamina and telemetry;
- `Round2Motion` owns solved pose computation;
- `Round2CombatRig` owns authoritative joints, glove paths and contact sampling;
- `Round2Footwork` owns pelvis/legs/foot planting;
- the imported skin/equipment reads those transforms and owns presentation only;
- contact remains the selected relative swept-volume architecture unless a
  deterministic contradiction is demonstrated and documented.

Do not stretch the imported body along an anatomical axis to match legacy
anchors. Build an explicit, testable mapping from authoritative Unity anchors to
the imported rig. Preserve upper-arm/forearm proportions and existing punch,
counter, stamina, HP, round and winner semantics.

The Unity materials may apply skin, red leather, red/gold satin and white/red
boot presentation to the accepted geometry. Do not restart facial likeness or
body sculpt research. Material work must not hide body articulation or introduce
a full-frame fighter overlay.

## Required implementation loop

Before edits, record:

1. current branch, worktree and exact HEAD;
2. verified art and Unity source hashes;
3. export/import hypothesis and one ownership mapping;
4. expected files to change;
5. deterministic acceptance checks;
6. out-of-scope systems.

Then:

1. create deterministic Blender export and provenance;
2. import the FBX and materials into Unity;
3. map the accepted rig to existing authoritative joints;
4. remove/disable competing visible body representations;
5. run the asset audit and focused bind/attachment checks;
6. run all existing Round 2, P1 and presentation regressions;
7. run live runtime motion for guard, jab, cross, hook, uppercut, overhand,
   advance, retreat, lateral reposition, slip/roll foundation and recovery;
8. revalidate visible mesh/contact coherence at too-far, boxing and close range;
9. measure allocations, rig-update cost and WebGL frame behavior;
10. evaluate M01–M12 from `22-combat-animation-quality-gates.md`;
11. commit source, build from clean committed source, verify provenance, then
    commit evidence/artifact separately;
12. push only the integration branch and provide one integrated UAT artifact.

Do not request intermediate visual approval. The owner wants one integrated UAT
after implementation and internal validation.

## Mandatory regression and visual checks

At minimum verify:

- no double shoulder, ghost silhouette, detached glove or competing old body;
- no forearm/wrap separation or finger exposure outside the glove;
- stance base and feet remain readable; no root-only sliding;
- guard and recovery remain continuous;
- all punch families remain visibly distinct;
- A1 advance/neutral/retreat ordering and A3.1 hook relationship remain intact;
- opponent does not home after commitment;
- visible glove path agrees with authoritative HIT/BLOCK/MISS;
- body/head contact does not use an obsolete sphere outside visible anatomy;
- no unexplained yellow/orange projectile artifact;
- first-person gloves, HUD hierarchy and arena identity remain intact;
- normal UAT has no debug overlay or bottom control graphics;
- no meaningful WebGL performance regression or per-frame allocation pattern.

Historical tests are evidence, not permission to ignore a visible regression.
If a historical numeric assumption conflicts with the accepted mesh, preserve
the old result and document the new invariant rather than silently rewriting it.

## Build and final evidence

Follow `13-build-release.md` exactly:

`committed SOURCE_SHA → BOXER_BUILD_MARKER → productVersion → metadata source_sha`

All must match. The build output directory must be absent before
`BoxerP0.Editor.Round2Build.Web`. Do not rebase after locking provenance.

Final report must include:

- `STARTING_SHA` = `bc0b2129dae47deeb3ce29d548624cf5571d0f36`;
- `ART_SOURCE_SHA` = `3e4e13c4b4451cb542e068652508ec12e248225f`;
- final `SOURCE_SHA` and artifact commit SHA;
- source `.blend` SHA and exported FBX SHA;
- Unity/Blender versions and export settings;
- changed ownership/mapping path;
- deterministic tests and assertion counts;
- asset audit and runtime smoke;
- M01–M12, each with evidence and honest PASS/FAIL;
- performance delta, allocation result and WebGL behavior;
- productVersion/build-marker/provenance verification;
- artifact hashes;
- exact local artifact path and UAT URL if deployed;
- known limitations;
- final status exactly one of:
  `IMPLEMENTATION_PASS_PENDING_HUMAN_UAT` or `IMPLEMENTATION_BLOCKED`.

Never claim `HUMAN_PASS`. Do not deploy or trigger Pages unless the owner has
already authorized deployment for this continuation or explicitly instructs it.
Preparing and locally smoking the concrete UAT artifact is required before any
deployment approval question.

## Stop condition

Stop after the integrated Round 2 Unity UAT candidate and report are ready. Do
not start PvP, matchmaking, server work, replay, highlights, career, ring intro,
or secondary screens.
