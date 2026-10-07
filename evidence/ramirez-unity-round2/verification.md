# Final source/artifact verification

Date: 2026-10-07. This is not Human UAT.

SOURCE_SHA=4940f8061eaf1ed58bcf5356a1bce4817de6eab0
BOXER_BUILD_MARKER=4940f8061eaf1ed58bcf5356a1bce4817de6eab0
productVersion=r2-4940f8061eaf1ed58bcf5356a1bce4817de6eab0
metadata source_sha=4940f8061eaf1ed58bcf5356a1bce4817de6eab0

Before build invocation: tracked source clean; builds/web/boxer-round2 absent.
Entry point BoxerP0.Editor.Round2Build.Web, Unity 6000.5.8f1; flags batchmode,
nographics, noUpm, quit. Final build-final-retry.log has ROUND2_WEB_BUILD_SUCCESS,
BuildPipeline result Succeeded, 319.4527153s, 58694742 bytes, normal editor exit 0.
All six generated file hashes independently recomputed and matched provenance.
HTML productVersion matched the full source SHA. Tracked scene/project settings
restored; git status --porcelain --untracked-files=no empty after build.
No source edits, rebase or artifact relabeling after the final build.

Source blend SHA256=419466859d7f119f98d7e69dc2673f0b378dbf5a892f0e2a6d7254cbc974af12
FBX SHA256=c2958f4085cb83cdbd0c05a13beec79b2b21d253fe5bc5521bc5ae7fc18188b6
data SHA256=32a9a9015f59fa4b823d48ee11d3bd0348d91b7fd443db077998f3e90afe4173
wasm SHA256=41cf50e390180906bfc4893f93aef492bfcf8d8aaff1539af84d3eb8c9ef643c
ZIP=ramirez-round2-4940f806.zip, 40027775 bytes
ZIP SHA256=980d9637e7d8ee4a704371148ad502657b5c9478ebb8554c09e65b936efb9456
ZIP entries read back; generated payload hashes match provenance. Same seven
files as the final build folder, under boxer-round2/.

Six authority files compared to STARTING_SHA have no diff: Round2Motion,
Round2CombatRig, Round2Footwork, OpponentBoxer, PlayerBoxer, P1PunchMechanics.
Historical thresholds and evidence preserved. 183 actual individual deterministic
assertions PASS; unchanged EVEvaluation triangle budget FAIL. See tests.txt.

Earlier source-66f7a41 diagnostic artifact:
D:/WORK/RESEARCH/POVGame/_ramirez-unity-source-66f7a41.
Source-e81ab436 diagnostic artifact and raw error JSON:
D:/WORK/RESEARCH/POVGame/_ramirez-unity-source-e81ab436.
Its post-success Editor OOM is preserved, not attributed to an invented cause.
Its pageerror-only startup PASS field is invalid: 13154 unsupported-GC console
errors occurred. That API is now Editor-only; the classifier now checks engine
console errors and unexpected 404s. Compressed raw evidence is preserved in
source-e81ab436-diagnostics.zip. Old measurements are not final performance.

Historical baseline artifact:
D:/WORK/RESEARCH/POVGame/_ramirez-unity-baseline-bc0b2129.
Actual provenance source=962e12398b0ae0ea73981646bcbbcd14d1368027;
productVersion=ev-962e12398b0ae0ea73981646bcbbcd14d1368027.
Relevant Scripts/Resources/WebGLTemplates have no diff to STARTING_SHA. Do not
relabel the historical artifact source as bc0b2129.

web-active-smoke.cjs is evidence-only: local HTTP, focused headless Edge, existing
desktop keys, unchanged onboarding timeout path. No application or built-byte
mutation. Browser rAF cadence is not Unity simulation FPS; F3 diagnostic images
show the existing Unity frame telemetry separately. See the final JSON/report.
The original unfocused smoke stopped during teardown and left only READY images;
those are partial evidence, not the final active-combat result.

No deployment, Pages workflow, sensor/device validation or Human UAT.

Final focused local smoke process exited 0. HTTP root/data/wasm=200; actual
onboarding/bout and keyboard paths observed; page errors=0, engine errors=0,
unexpected missing assets=0. Only favicon.ico 404s remain in the raw console.
Browser rAF baseline mean=13.0067ms, candidate=13.009041666666672ms,
delta=0.002341666666664466ms / 0.01800354176435537%. One pair, not a controlled
device benchmark. F3 telemetry snapshots: baseline 77 now/76 avg FPS,
p95 14ms/max 43ms; candidate 83 now/78 avg FPS, p95 15ms/max 47ms.
Allocation zero is Editor-only; WebGL allocation unavailable. Target-device
acceptance and historical triangle-budget gate are not turned into PASS.
