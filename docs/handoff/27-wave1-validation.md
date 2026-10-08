# Wave 1 validation and release gates

Owner date: 2026-10-08. Branch: `feature/boxer-product-loop-wave1`.
Compiled source: `e6d40488052a643cc22121661c40a1c72e69ae14`.
Gameplay/controller test source: `1b9a42de1087d551081cd8521d7f16e8112ab02c`.
Intervening commits change only favicon/branding assets, their converter and
HTML/browser audit diagnostics;
Unity gameplay, scenes, shaders and accepted assets are unchanged.
Base: `bcbcb6c374a233e7d379aa31b21b37af8727fbf4`.

## Gameplay and controller evidence

- `evidence/wave1/vitals-tests.txt`: **84/84 PASS** on release source.
- `evidence/wave1/controller-runtime.txt`: **56 checks, EXIT=0**.
  Includes real controller accepted/rejected action cost, impact/recovery fatigue,
  exact-once resolution, immediate HP-zero controller lock, and ten rematches
  without duplicate actors. Contact outcomes are deliberately injected in this
  Editor harness: this proves consequence wiring, NOT visible physical contact.
- Existing P0/P1 chain, P1V 8/8, and Round2 43/43 rerun without editing mechanics
  tests. New outputs are under `evidence/wave1/regressions`; frozen old evidence
  was restored byte-for-byte. The historical combined summary says 131/131;
  this report does not reinterpret that hardcoded total as an assertion count.
- Two early audit attempts failed because of test-harness bugs (untagged camera,
  and expecting a KO winner after an intentionally drawn timeout). Their results
  and intermediate glove images remain preserved. Only release reruns count.
- Editor startup logged a Unity Search index exception. No gameplay/shader/C#
  failure was observed in the successful runtime audit. Browser evidence is a
  separate gate, not inferred from Editor exit codes.

CombatBout is the sole source of HP, Stamina and burst Capacity. HUD and result
screens read it. Accepted attacks pay once; quality at commit scales HIT damage
and recovery. MISS and BLOCK do not damage HP. No HP regeneration. HP zero ends
the bout immediately; timeout compares remaining HP. See contract 26 for all
coefficients and the explicit non-physiological/determinism boundary.

## Reference inventory and visual status

All six approved JPGs are present in `docs/handoff/reference-ui`:
01 customization, 02 Home, 03 combat HUD/gloves, 04 venues/career,
05 training/coach, 06 Ramirez turnaround. **There is no dedicated approved
Preview or Result reference**; these new screens inherit the black/gold style.
Career, full gym, shop, AI coach, cloud/PvP and replay remain deferred.

Player gloves now have a continuous pad, distinct mirrored thumbs, curved cuff
piping/seams and a rougher leather shader, with no projecting gold box.
Status is **PARTIAL, not reference-fidelity PASS**: composition/silhouette,
wrinkles, crown visibility and mobile framing still require owner evaluation.
No accepted Ramirez art, collision geometry, reach or trajectory was changed.
The two new glove meshes total 33,434 triangles before any renderer overhead.
Mobile performance remains unmeasured; no zero-allocation WebGL claim is made.

Accepted Ramirez FBX remains SHA256
`c2958f4085cb83cdbd0c05a13beec79b2b21d253fe5bc5521bc5ae7fc18188b6`.
Existing native-body/contact-envelope discrepancies (roughly 8 cm head and
10 cm abdomen) and the 459,352-triangle character remain open blockers.
Correct numerical consequences of contact events do not certify visible hits.

## Build / browser / deployment

WebGL compilation and payload verification are **PASSED on attempt 6**;
final browser validation is **PASS (11/11 checks, zero JS/load/HTTP errors)**.
Deployment is **VERIFIED**: release artifact commit
`6b81d684c13697d8fba3d8d03ffbcddd417887c6`, compiled source above;
[Pages workflow 37722815248](https://github.com/minhtri22/boxer/actions/runs/37722815248)
completed successfully. Online provenance is byte-identical to the local release;
all seven deployed payload SHA256s match, including the cleaned boxer ICO.
First build failed with clang/system-resource
errors. The second attempt used session-only `BEE_BUILD_THREADS=1` but failed
again; its supervisor also reported `Out of memory`. Source/settings remained
unchanged after both completed attempts. Unity documents this limit in its
[6000.1.0b2 release notes](https://unity.com/es/releases/editor/beta/6000.1.0b2).
An unrelated PowerShell process PID 13596 held about 56 GB private memory.
After the earlier owner-requested pause, the owner explicitly authorized stopping
only PID 13596 and accepted loss of its unsaved data. That process was stopped;
Python PID 9152 and launcher PID 24528 stayed alive with unchanged start times.
Available commit increased from 3.74 to 31.79 GB (not a measured 56 GB release).
The owner then explicitly requested a build retry. Attempt 3 succeeded with
session-only `BEE_BUILD_THREADS=1` against gameplay source `1b9a42d`:
Unity exit 0, `ROUND2_WEB_BUILD_SUCCESS`, 139.92 seconds of BuildPipeline time,
60,732,462 reported bytes. All six exported payload files match provenance
SHA256; accepted Ramirez and both player-glove hashes match the recorded values.
One owned ILPP startup process required the existing scoped IPC recovery.
The first browser audit passed all nine functional checks but failed the strict
no-load-error gate on two `/favicon.ico` 404s. Its unchanged FAIL report and
screenshots are retained in `evidence/wave1/browser-attempt1`; the attempt-3
payload is preserved in the adjacent `_wave1-build-attempt3-favicon404` folder.
The HTML template now declares a self-contained SVG favicon, and the audit also
records URL/status for HTTP errors without relaxing its no-error gate.
Attempt 4 rebuilt source `733e105` successfully: Unity exit 0, 12.10 seconds of
incremental BuildPipeline time, 60,732,745 bytes; all six payload hashes verified.
That browser retry passed 10/10 checks without load errors; report/screenshots
are preserved under `evidence/wave1/browser-attempt2`.
The owner then requested the supplied boxer image as a real favicon, followed
by explicit removal of its Gemini mark. Attempt 5 (with the mark) was not
deployed; its payload is preserved in `_wave1-build-attempt5-gemini-logo`.
Built-in image editing removed the mark, and the cleaned image was converted
without cropping into a six-frame ICO (16/32/48/64/128/256). Exact prompt and
source/clean/icon hashes are recorded in `art/branding/favicon-provenance.md`.
Attempt 6 built the cleaned-favicon source above successfully: Unity exit 0,
25.89 seconds of incremental BuildPipeline time, 60,880,073 bytes. All seven
exported payload hashes, including the cleaned ICO, verified; source/settings
restore is clean. Final browser audit passed 11/11 checks against this output,
including an HTTP-loaded six-frame ICO with the cleaned icon hash. Browser
reported `w1-e6d40488052a643cc22121661c40a1c72e69ae14`. Final report and rendered
Home/Preview/Tutorial/Fight/Result/Rematch/Home-return images are under
`evidence/wave1/browser`. Real keyboard input spends resources; both controllers
lock at Result; resources freeze there; rematch resets without duplicated actors.
The desktop runs ended by POINTS: browser KO was not directly exercised; HP-zero
KO is covered by the numerical and injected-controller tests above.
No OS paging/security settings changed.
Baseline output and the failed partial output were moved into named adjacent
backup folders, not destroyed. Scene and ProjectSettings restore are checked
after each build completes.

Owner explicitly approved adding ONLY `feature/boxer-product-loop-wave1` to the
Pages allowlist, preserving every existing policy, and deploying when WebGL
tests pass. This is conditional authorization, not deployment evidence.
Do not dispatch until local browser UI/input tests and provenance verification
pass. Those gates passed; the exact additive Wave1 branch policy was created,
all four original branch policies were confirmed preserved, and the workflow
above was dispatched against the exact pushed release artifact commit.
Synthetic desktop tests are not iPhone sensor/performance/Human UAT.

Overall release remains **IMPLEMENTATION_BLOCKED / HUMAN_UAT_PENDING** while
inherited visual/contact/performance gates are unresolved. Numerical
model/controller PASS must not be relabeled as overall gameplay or Human PASS.
This deployed candidate is available for owner testing, not certified Human UAT.
See `evidence/wave1/deployment.txt` for exact deployment/hash evidence.
