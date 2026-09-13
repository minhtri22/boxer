# Final-source desktop WebGL smoke

Source: `725a21552b760c61ffffa67d91cc7ade9b29cb0d`.
Artifact: the byte-verified `builds/web/boxer-round2` copy; Unity 6000.5.8f1.
Environment: Codex in-app Chromium on this Windows workstation, explicit
`?desktop=1`, input status `DESKTOP_SYNTHETIC`. No phone sensor claim.

Observed through the actual browser canvas and DOM:

- Correct full source marker displayed; startup status remains DESKTOP_SYNTHETIC.
- 390x844 portrait: articulated opponent head, torso, bent legs, both feet and
  foreground player gloves visible; existing HUD and lower controls remain.
- Normal presentation has one training/result card. F3 intentionally enables
  legacy developer diagnostics; their dense text clips at phone width. F3 is
  not the normal UAT composition.
- AI entered range and attacked; live combat log recorded swept body HIT and
  guard BLOCK. Portrait bout reached opponent winner with 11 opponent hits and
  12 player blocks. A previous landscape run reached DEFEAT with five hits.
- Keyboard requests were accepted; rapid subsequent requests were rejected by
  the existing recovery/spam gate. This is not eight separately executed
  manual punch-family trials; the deterministic matrix provides that coverage.
- No JavaScript error entries were returned by the browser error-log query.

Unity F3 displayed samples (rounded by the existing HUD, not a benchmark export):

| View/state | Current FPS | Average FPS | Frame p95 ms | Frame max ms |
|---|---:|---:|---:|---:|
| Portrait, footwork training | 91 | 87 | 13.0 | 88.0 |
| Landscape, punch training | 27 | 89 | 12.0 | 37.0 |
| Landscape, live AI counter training / player Extend | 83 | 90 | 12.0 | 12.0 |
| Landscape, bout 13 seconds remaining / player Commit | 83 | 90 | 12.0 | 13.0 |

Occasional maximum-frame spikes are retained, not hidden by the average.
These are observed rolling HUD windows; they do not establish a sustained
whole-session percentile or an iPhone frame-rate guarantee. Browser DOM rAF
reported about 90 callbacks/s in later 600-sample windows, but rAF is explicitly
not treated as Unity FPS.

No same-environment starting-artifact run exists: before/after performance delta
is UNMEASURED. Physical iPhone sensor, haptic/audio perception, sustained mobile
FPS and final visual acceptance remain for the single integrated human UAT.

Deployment follow-up: the same artifact loaded and rendered the live training
scene at `https://minhtri22.github.io/boxer/?desktop=1`; its source marker matched
and the public-host JavaScript error query returned no entries. All seven hosted
file hashes matched; see deployed-verification.json.
