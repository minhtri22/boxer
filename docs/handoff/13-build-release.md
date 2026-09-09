# 13 — Build & Release Protocol

## Environment

- Unity: `6000.5.8f1`
- Windows local reference: `D:\WORK\RESEARCH\POVGame\boxer`
- Web output: `builds/web/boxer-p0-web`
- GitHub Pages: `https://minhtri22.github.io/boxer/`

## Provenance invariant

For every UAT candidate:

`committed source SHA → source short → BOXER_BUILD_MARKER → generated productVersion → metadata build_commit`

The short marker, productVersion and metadata `build_commit` must match exactly.

## Required sequence

1. Sync branch.
2. Compile/test.
3. Commit **source changes first**.
4. Confirm tracked tree clean.
5. Freeze `SOURCE_SHA` and `SOURCE_SHORT`.
6. Set `BOXER_BUILD_MARKER=SOURCE_SHORT`.
7. Delete/recreate Web build output.
8. Build through `Phase0SceneBuilder.BuildWebPlayer`.
9. Verify build success and generated files.
10. Verify productVersion and metadata marker.
11. Record data/wasm SHA256.
12. Local HTTP smoke.
13. Create result doc/evidence.
14. Commit generated artifact/evidence separately.
15. Push branch.
16. **Do not rebase after final build.**
17. Trigger a **new** workflow_dispatch for UAT when required.

## Latest accepted build baseline

- source: `4939e2bd5f44513d9d2ad3732ae07ac02f2ea148`
- source short: `4939e2b`
- artifact: `4a985574f44d458357f875a1b7a6d3d00c7f9386`
- data SHA256: `CEC7B56DB844F7B2D3C4CB7C8B7DF09B693CE0EB1B5113E91D0CD65E908CD381`
- wasm SHA256: `008E9737919271CC23C250C1279F2E7DF59C220AD171A895B107B0A5F95EDC68`

## Web smoke

Minimum:

- index/root reachable (200)
- `.data` reachable (200)
- `.wasm` reachable (200)
- Unity startup without fatal error

Web/Safari is a surrogate interaction/UAT path. Do not infer native iOS performance from it.

## Workflow note

The Pages workflow is manual (`workflow_dispatch`) in the existing setup. If no connector action supports dispatch, do not pretend to trigger it; ask the local/user side to create the new run.

## Failure conditions

Reject provenance if:

- build came from dirty tracked source;
- source was committed after build;
- marker mismatches committed source;
- artifact was rebased after build;
- old workflow rerun is substituted for a new candidate run;
- a report claims human validation without real UAT.
