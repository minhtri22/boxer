# Coach functional screen slice - 2026-10-10

Compiled source: `f6ff54a0356454bedc2ba29425f9789eba1a87ec`.
Status: implementation checks PASS, phone Human UAT pending. Overall reference-render fidelity PARTIAL (approved 2D coach portrait; simplified native cards; not a new 3D coach/gym).

## Current evidence

- `provenance.txt`: successful clean committed-source WebGL build, 27.1299589 seconds / 62,614,952 bytes, full payload digests.
- `coach-tests.txt`: 53/53 native module/navigation/resource checks.
- `punch-feel-tests.txt`: 14,741/14,741 existing timeline/motion/feedback checks.
- `combat-tests.txt`: 172/172 geometry invariants.
- `vitals-tests.txt`: 111/111.
- `presentation-tests.txt`: 550/550.
- `controller-runtime.txt`: CHECKS=118 EXIT=0, injected Editor controller checks, not device input.
- `combat-freeze.txt`: 12/12 unchanged combat kernel/controller/input/motion/duration checks versus owner-held baseline `10123ff`.
- `coach-browser/report.json`: 31/31, trusted synthetic touches, completion/cancel/reload, no idle MP4 request, full information text fit, portrait/landscape, zero errors.
- `browser/report.json`: 33/33 existing product/practice/POV regression, zero errors.
- `combat-browser/report.json`: 18/18 actual compiled synthetic input/contact regression, zero errors. Expert scripted KO is not an ordinary-phone balance pass.
- `media-browser/report.json`: 20/20 real decoder/intro/cancel/bells/crowd/rematch lifecycle, zero errors; not physical speaker/phone UAT.

All four current browser reports identify the same exact compiled version. Full traces/captures and Editor logs remain in the local evidence directory; curated current captures and negative reports are committed. Runs using `native-idle-media-fix.log` and `controller-idle-media-fix.log` returned Unity helper EXIT=0. Startup/indexing/import-worker diagnostics are not hidden by a blanket clean-log claim.

## Preserved negative candidates

- `coach-browser-first-candidate`: source `a3f14d9`, FAIL 27, idle video abort and manually observed clipped Conditioning copy. Never deployed.
- `coach-browser-reload-failure`: source `3d7bf74`, FAIL 30, text fit fixed but video abort remained despite transfer wait/reload lifecycle.
- `coach-browser-idle-video-failure`: source `3d7bf74`, FAIL 30, timestamped request trace locates `net::ERR_ABORTED` at 15,096 ms during `feet-completed`, before intro/reload. `preload="none"` fixes unused idle fetching; zero-error gate remains strict.
- `browser-text-fix-candidate`: source `3d7bf74`, former UI regression PASS; locally preserved, not the current release gate.

Public release receipt and public decoder audit will be added only after actual deployment verification. HP-KO-001 remains HOLD in `docs/handoff/backlog.md`; no combat balance changes in this package.
