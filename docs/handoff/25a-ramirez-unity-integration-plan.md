# Ramirez Unity integration execution plan

Date: 2026-10-07. Authority: owner request to execute handoff 25.

STARTING_SHA: bc0b2129dae47deeb3ce29d548624cf5571d0f36
ART_SOURCE_SHA: 3e4e13c4b4451cb542e068652508ec12e248225f
Art import commit: 687f561 (only art/ramirez restored from ART_SOURCE_SHA).
Branch: integration/ramirez-unity-round2
Worktree: D:/WORK/RESEARCH/POVGame/boxer-ramirez-unity
Pre-edit source art checkout: art/ramirez-realistic-blender at
1855054c03b6bec1252594d0302ac602881f4922, clean.
Verified origin/p1/whole-body-mechanics: STARTING_SHA.

Verified immutable input SHA256:

- blend: 419466859d7f119f98d7e69dc2673f0b378dbf5a892f0e2a6d7254cbc974af12
- report: c30584adb6aa87c373ac06d1ee624ebd2fbaf4f470cb70f048df55de776289f3
- guard: 7437b67de3d4690ad79bfdfa4720c4daf1efb0361bbfbb6e9db65f4df47e17fa
- jab: c0586e8b14ff58e054fe4ec8cd8294ce6800242bff5ad9adf55328c5bd5db98d

## Hypothesis and ownership

Export only the selected visible fighter meshes and RAMIREZ_RIG at rest through
Blender 5.2.1 LTS, with explicit selection and no animation bake. Import a generic
rig with original proportions. A presentation adapter caches anchors and bind
rotations once; each frame it follows chest/pelvis orientation and solves the
imported limbs with their own bind lengths against authoritative wrists/feet.
No anatomical axis scaling. Measure any unreachable authoritative anchor rather
than silently modifying contact semantics or claiming coherence.

Final mapping adds minimal native clavicle rotation to reach the glove center,
and a presentation-only pelvis crouch computed from planted feet when native
leg lengths require it. Every limb retains its original bind length. The old
adapter's nonuniform hand scale is bypassed for RAMIREZ_RIG. Anchors and contact
volumes remain byte-identical to STARTING_SHA.
Native boot orientation reads the foot anchor's planted rotation, including its
cached bind correction; root-facing rotation does not override a planted foot.
The first source-66f7a41 build is retained as a diagnostic artifact at
D:/WORK/RESEARCH/POVGame/_ramirez-unity-source-66f7a41. Final source is committed
again after this rotation correction, with a new clean provenance build. No
commit is rebased.

The established Round2Build.Web API requires a full 40-character marker and
generates r2-<full SHA>, so all final marker/metadata checks use that full SHA.
The older short-marker Phase0 example in handoff 13 is not used to change this
Round 2 build contract.

Controllers retain input/timing/root/HP/stamina/telemetry; Round2Motion computes
poses; Round2CombatRig retains joints/gloves/relative swept contact;
Round2Footwork retains pelvis/legs/planting. Imported skin/equipment are readers.
One visible fighter representation replaces all existing opponent renderers.
First-person glove and HUD presentation remain owned by existing systems.

## Expected files

- art/ramirez/scripts/export_unity.py and export manifest
- Assets/Resources/Boxer3D/Ramirez_UAT3.fbx and importer settings
- new accepted-rig presentation adapter and its bootstrap routing
- focused editor integration audit and regression runner
- evidence/ramirez-unity-round2 (export, audit, runtime, performance, captures)
- this plan, final report, separately committed WebGL artifact

## Acceptance

Require correct hashes/versions; no authoring aids or duplicate geometry; finite
weighted bind meshes; unit proportions; bone mapping complete; equipment follows
skin; cached updates have no recurring allocation; all existing P0/P1/Round2/
presentation checks; runtime guard/all punch families/recovery/footwork and
commit-lock checks; visible/contact coherence at three distances; actual runtime
capture review M01-M12. Preserve historic test evidence. Commit source before
clean provenance build, then separately commit evidence/artifact. Local WebGL
startup and file HTTP/hash smoke required. Push only integration branch.

## Excluded

General Blender redesign, rejected assets/other-agent branch, combat semantic
changes, PvP/network/replay/career/secondary screens, Pages deployment and Human
UAT claims. Final status is IMPLEMENTATION_PASS_PENDING_HUMAN_UAT only after
required internal gates; otherwise IMPLEMENTATION_BLOCKED with exact evidence.
