# Ramirez accepted-art Unity Round 2 integration result

Date: 2026-10-07. Final status: `IMPLEMENTATION_BLOCKED`.

The owner-approved continuous fighter is integrated into Round 2. Source export,
native-rig mapping, regression and live motion work are complete. This is a
concrete diagnostic/UAT candidate, not a release acceptance or Human UAT PASS.
Body/head contact coherence and the historical presentation triangle budget
remain failed. Full POV reference composition and target-device performance
acceptance are also not established. No Pages deployment was authorized or run.

## Lineage and isolation

- STARTING_SHA: `bc0b2129dae47deeb3ce29d548624cf5571d0f36`.
- ART_SOURCE_SHA: `3e4e13c4b4451cb542e068652508ec12e248225f`.
- Art-only import commit: `687f561`.
- Final SOURCE_SHA: `4940f8061eaf1ed58bcf5356a1bce4817de6eab0`.
- ARTIFACT_COMMIT_SHA: `7ecb18e993390d9332cca0d015adba7715b09a9e`.
- Branch: `integration/ramirez-unity-round2`.
- Worktree: `D:/WORK/RESEARCH/POVGame/boxer-ramirez-unity`.

Only art/ramirez was restored from the exact art commit; no wholesale merge or
cherry-pick of the art/backup branch. Existing worktrees were not edited. The
six authority files Round2Motion, Round2CombatRig, Round2Footwork, PlayerBoxer,
OpponentBoxer and P1PunchMechanics have no diff from STARTING_SHA.
No force push or rebase; only the integration branch is the push destination.

## Immutable art, export and import

- Blender: 5.2.1 LTS, build `9e2066aef7ef`.
- Unity: 6000.5.8f1, revision `5cb7df797b7d`.
- Editable blend SHA256: `419466859d7f119f98d7e69dc2673f0b378dbf5a892f0e2a6d7254cbc974af12`.
- Workbench report SHA256: `c30584adb6aa87c373ac06d1ee624ebd2fbaf4f470cb70f048df55de776289f3`.
- FBX SHA256: `c2958f4085cb83cdbd0c05a13beec79b2b21d253fe5bc5521bc5ae7fc18188b6`.
- 31 weighted meshes, 24 native bones, 231937 exported vertices, 459352 triangles.
- Unity audit: 57 transforms, 31 skinned renderers; bind references valid, scale
  approximately (1,1,1), upper arm 0.2962449m, forearm 0.23955366m.

Versioned export: art/ramirez/scripts/export_unity.py. Two fresh Blender exports
were byte-identical. Selection is explicit: RAMIREZ_RIG and accepted visible
body/head, eyes, gloves/wraps, shorts/trim/waistband and boots/soles/laces.
Cameras, lights, floor, references, QA aids and hidden obsolete anatomy excluded.
No implicit .blend import, no animation bake or leaf bones. Settings: -Z forward,
Y up, unit scale, FBX_SCALE_UNITS, global_scale=1, space transform, evaluated mesh
modifiers, FACE smoothing, STRIP paths, no embedded textures. Only exporter
metadata time/UUID are normalized in-process; installed Blender addon and source
blend are unchanged. Full settings, bones, object counts, matrices and material
slots are in evidence/ramirez-unity-round2/export/manifest.json.

Semantic Skin/Red/Gold/White/Eye presentation uses existing EVSurface shader.
This is not a new facial, hair or texture research phase. Accepted shape is not
decimated or stretched to disguise a failed gate.

## Ownership and mapping

Blender3DVisualFollower routes RAMIREZ_RIG to RamirezAcceptedRig, after combat and
footwork updates. It hides competing old actor renderers but retains established
player POV assets and HUD. Model follows actor root; native pelvis reads R2
Pelvis, with the minimum presentation crouch needed by native leg lengths.
Spine01 reads R2 Chest, head stays independently orientable, limbs solve with
native lengths and existing elbow/knee poles. Wrist-to-visible-glove offset is
explicit; minimal native clavicle rotation reaches the authoritative glove
center. Boots read planted shoe position AND rotation with bind correction.
No authoritative anchor write, nonuniform anatomical scale, new contact collider
or change to timings, stamina/HP/counter/round/winner semantics.

Mapping caches references and bind data. The old hand scaling path is bypassed;
material instances are disposed. Allocation instrumentation is Editor-only:
WebGL reports unavailable (-1), not a fabricated zero. A focused browser run
proved GC.GetAllocatedBytesForCurrentThread unsupported by WebGL IL2CPP; that
integration defect was fixed and a new source was committed before final build.
The bad source-e81ab436 artifact was not silently relabeled as the final build.
Its raw smoke JSON says startup PASS because the first harness only classified
page exceptions. The console actually has 13154 unsupported-GC engine errors;
that PASS field is invalid evidence. The evidence-only harness now also checks
engine console errors and unexpected asset 404s. The old JSON is preserved,
not edited. Its +14.33% browser rAF delta is contaminated and not the final delta.
The earlier unfocused web-smoke.json also eventually persisted; its READY images
and pageerror-only PASS have 1138 unsupported-GC console errors. Its +4.80% rAF
delta is likewise invalid final-performance evidence. Both raw results remain
available; only the corrected final web-active-smoke.json is used below.

## Tests and live runtime

Final test log: tests-webgl-instrumentation-retry.log and tests.txt under the
new evidence folder. Actual individual assertion rows: P0/P1=132, P1-V=8,
Round2=43, total=183 PASS. The old combined suite lists 17 suite summaries and
hardcodes 131/131, including a P0 22/22 label although P0 has 23 PASS rows. This
reporting discrepancy is preserved and disclosed, not rewritten.

Round2 experiments completed 1860 fixtures against its independent dense contact
oracle; the original swept candidate reports zero false misses/contacts there.
That is the legacy anatomical-volume experiment, NOT an accepted-mesh oracle.
The contact matrix also completed. A1 advance>neutral>retreat, A3.1 hook ordering,
continuity, counter/stamina/HP/end-state and zero-allocation pose/sweep checks
remain PASS. Existing evidence bytes were restored; this run's copies are in
regressions/. Blender3DAssetAudit and focused native import audit PASS.

EVEvaluation.ShellTests FAILS its unchanged <=30000 triangle budget. It aborts
there; later legacy-bone checks were NOT executed. The native import/attachment
audit is separate evidence, not a claim that the historical suite passed.

Final real Editor play-mode fixture: 34s, 1873 sampled frames, 45 steps, 9 contacts.
Eight opponent intents traverse commit/extend/recover, using the frozen action
machine, durations and endpoints. Player forward/back/lateral fixtures provoke
AI distance changes and stance reset. This is synthetic, not human interaction.
Live runtime.txt and captures/ contain phase and periodic motion evidence.

- Glove-center max error: 0.0000008221337m.
- Native foot-center max error: 0.000002092327m.
- Native limb-length max error: 0.00000110268593m.
- Authoritative planted drift: 0.000000175840015m.
- Committed root/rotation drift: 0m / 0 degrees.
- Single visual ownership: true.
- Adapter measured Editor managed allocation: 0 bytes.
- Adapter Editor cost: mean 0.0272903759ms, max 1.4469ms.

No direct WebGL allocation measurement or native-device result is claimed.
The runtime uses reflection only in an Editor test fixture, not a shipped
controller. Its temporary baked-mesh ray collider is audit-only and destroyed.

## Mesh/contact contradiction

Front rays compare visible baked body surface with the original target-volume
front point (target 2=head, target 3=abdomen). Samples have different poses;
these are diagnostic distances, not a full surface oracle or false-hit count.
The log records absolute gap, not its signed direction; it establishes a surface
discrepancy, not by itself a count of false hits or which side each point lies on.

| Range / time | Head gap | Abdomen gap |
| --- | ---: | ---: |
| Long/too-far fixture 1.4m, 1.01s | 0.07803786m | 0.09883225m |
| Boxing 0.9947959m, 31.00s | 0.07933274m | 0.0998076349m |
| Close 0.7213747m, 22.01s | 0.02180524m | 0.09793973m |

Exact glove centers do not cure the body/head surface mismatch. HIT/BLOCK/MISS
semantics still use selected relative swept spheres, not the native skin. This
violates the handoff's visible anatomy/contact gate. No threshold, seed, baseline
or contact boundary was changed to make it pass. Adjusting anatomical target
envelopes would change hit boundaries and requires an explicit owner scope
decision against the frozen combat restriction.

## M01-M12 internal gates

PASS below means scoped internal engineering/sample review, never Human UAT.
Rendered phase stills do not certify every transient frame or device interaction.

| Gate | Verdict | Evidence and limitation |
| --- | --- | --- |
| M01 Distance | FAIL | AI traverses 1.4m to 0.72m, but target-volume front points remain discrepant with native anatomy; numeric hits cannot certify visible contact. runtime.txt. |
| M02 Stance | PASS | Guard and motion captures show a lead base, bent supporting legs, compact guard and readable feet. Native proportions retained; this is sampled visual review, not measured sole collision. |
| M03 Footwork | PASS | Forward/back/lateral fixture, 45 steps, stance resets, planted rotation and tiny mapping error; motion-03 through motion-10. |
| M04 Body connection | PASS | Native pelvis/legs and chest/clavicle read the body chain; phase captures show shoulder/torso/arm connection without isolated floating glove. Movement is subtle; no claim of final animation polish. |
| M05 Target relationship | FAIL | Visible glove center agrees with authority to <1mm, but head/body sphere front gaps remain. Foreground gold strips are original POVGoldBand geometry, not a newly spawned projectile; reference fidelity is still unapproved. |
| M06 Families | PASS | Jab/cross, lead/rear hook, uppercut and overhand phase captures retain different winding/extension directions; all 8 state sequences observed. |
| M07 Recovery | PASS | Continuous frozen phase tests and native commit/extend/recover captures return to guard with attached limbs/equipment. |
| M08 Defense foundation | PASS | Guard usable during reposition; torso and head have independent mapping; recovery remains ready. Full AI slip/roll is not required or implemented. |
| M09 Stability | PASS | One visible owner, no sampled duplicate shoulder/ghost glove, microscopic anchor/length error, no competing native writers. Unsampled one-frame visual artifacts still require Human UAT. |
| M10 POV composition | FAIL | Centered opponent, foreground player gloves, arena and original HUD preserved. Comparison with 03-pov-combat-hud.jpg does not meet full apparent-size/glove/background fidelity; no new HUD layout or bottom-control graphics added. |
| M11 Performance | FAIL | Editor allocation/cost is measured, but historical triangle budget fails; final local browser frame evidence below is a surrogate, not iPhone acceptance. |
| M12 Regression | FAIL | 183 individual deterministic assertions PASS and build/local smoke checked, but historical presentation budget fails and its later assertions abort. Mesh/contact gate also remains failed. |

## Final build, local smoke and performance

Clean committed SOURCE_SHA -> full BOXER_BUILD_MARKER ->
`r2-4940f8061eaf1ed58bcf5356a1bce4817de6eab0` productVersion -> identical metadata
source_sha and HTML marker: independently verified PASS. Output was absent before
invocation. Final build-final-retry.log: Succeeded, 319.4527153s, 58694742 bytes,
ROUND2_WEB_BUILD_SUCCESS, normal Editor exit 0. Tracked scene/settings restored
and tracked source clean after build. No subsequent source edit or rebase.

Local focused headless Edge smoke: root/index, data and wasm HTTP 200; onboarding
and actual bout timer/AI action observed; existing keyboard input exercised.
Engine console errors=0, page exceptions=0, unexpected asset 404=0. Three harmless
favicon.ico 404s are logged rather than described as no console messages.
Final evidence: web-active-smoke.json, web-active-*-bout-*.png and diagnostic
web-active-*-performance-debug.png. Browser/server teardown completed, process 0.

Same viewport (540x960), 120 rAF warmup callbacks then 600 samples per artifact,
baseline first then candidate, one pair only. This is browser rAF cadence, NOT
direct Unity simulation FPS, a matched deterministic scene trajectory, a
statistical regression study, or native-device performance. Actual historical
baseline artifact source is `962e12398b0ae0ea73981646bcbbcd14d1368027`, not
STARTING_SHA; relevant Scripts/Resources/WebGLTemplates have no diff between the
two source commits. Its preserved provenance is labeled accurately.

| Browser rAF sample | Baseline | Candidate |
| --- | ---: | ---: |
| Mean callback interval | 13.0067ms | 13.00904167ms |
| Derived callback frequency | 76.88345Hz | 76.86961Hz |
| p95 | 14.015ms | 14.100ms |
| Maximum | 15.550ms | 18.335ms |

Mean delta +0.00234167ms / +0.01800354%. No large mean regression appears in this
single desktop sample; that does not establish the target-device M11 PASS.
Existing Unity F3 telemetry in separate screenshots reads baseline 77 current /
76 average FPS, p95 14.0ms, max 43.0ms; candidate 83 current / 78 average FPS,
p95 15.0ms, max 47.0ms. These are different instantaneous rolling windows,
NOT directly subtractable paired averages. Outliers are retained without an
invented cause. Native adapter allocation=0 is an Editor-only result; no direct
WebGL managed allocation claim or iPhone gesture/performance test was made.

Normal WebGL screenshots have original top-left player, top-center timer,
top-right opponent HUD and no bottom graphics/debug overlay. Explicit F3
performance-debug captures are diagnostic-only, not normal UAT presentation.
Only existing desktop synthetic keys and onboarding timeouts are used; no
sensor result, hidden StartBout browser bypass or user-input completion is claimed.
The HTTP server/browser were closed after testing; no persistent UAT URL or
deployment exists. A local file:// open is not a supported WebGL serving path.

## Artifact and next authority boundary

Exact local WebGL folder:
`D:/WORK/RESEARCH/POVGame/boxer-ramirez-unity/builds/web/boxer-round2`.

Single packaged candidate (same bytes, not another build):
`D:/WORK/RESEARCH/POVGame/boxer-ramirez-unity/evidence/ramirez-unity-round2/ramirez-round2-4940f806.zip`.
ZIP size 40027775 bytes. ZIP entries read back; payload hashes match provenance.
All seven build/provenance files plus ZIP were read directly from artifact
commit Git blobs and matched the local bytes; core.autocrlf=false, no raw-output
line-ending normalization or post-build payload edit was performed.

- index.html SHA256: `7d9e9add62ec3282f35b7df6146786c7c952bda03724b96961ab6d9e6a7cd99e`.
- data SHA256: `32a9a9015f59fa4b823d48ee11d3bd0348d91b7fd443db077998f3e90afe4173`.
- wasm SHA256: `41cf50e390180906bfc4893f93aef492bfcf8d8aaff1539af84d3eb8c9ef643c`.
- ZIP SHA256: `980d9637e7d8ee4a704371148ad502657b5c9478ebb8554c09e65b936efb9456`.
- All other generated file hashes: builds/web/boxer-round2/provenance.txt;
  independently recomputed and matched, not only copied into this report.
- UAT/deployment URL: none. Local smoke used ephemeral loopback HTTP and closed.

To reopen the concrete artifact locally, serve its folder over HTTP, e.g.
`python -m http.server 8000 --bind 127.0.0.1 --directory "D:/WORK/RESEARCH/POVGame/boxer-ramirez-unity/builds/web/boxer-round2"`,
then use `http://127.0.0.1:8000/?desktop=1` for synthetic desktop input only.
The normal device path still requires real device permissions/interaction.
No live server is left running by this handoff.

Known limits: target envelopes mismatch native mesh; 459352 triangles exceeds
historical budget; simple materials and POV composition are not fully reference
faithful; browser timing is one synthetic sample, not iPhone performance; complete
slip/roll and Human UAT remain outside this completed integration run.
Earlier failed/intermediate artifacts are preserved in adjacent diagnostic
directories, with compressed e81 error evidence in source-e81ab436-diagnostics.zip.
Unity IPC startup retries and the intermediate post-build OOM are preserved;
their causes are not invented. Historical art/regression reports remain intact.

Stop here as required by handoff 25. Further contact-boundary adjustment requires
explicit owner authorization. Any export optimization needs its own visual and
deformation revalidation; it was not performed just to meet the old budget.
No PvP, matchmaking, replay, career, secondary screens, workflow dispatch or Pages.

Final status: `IMPLEMENTATION_BLOCKED`.
