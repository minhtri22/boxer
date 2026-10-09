# Ring introduction and bout audio — 2026-10-09

Owner approved use and temporal splitting of their supplied 6-second video.
Work remains on `feature/boxer-product-loop-wave1`; prior combat-v3 evidence and
HP/Stamina/damage/recovery/KO formulas are preserved.

## Lifecycle

Home → Preview → Intro (5.166667-second ring girl) → start bell → Fight
→ KO/45 scored seconds → one end bell → Result. Rematch repeats Intro.
Training is still separate and never invokes the media or enables AI.

Intro locks both controllers and gameplay input, resets all resources to 100,
does not start CombatBout and cannot spend resources, regenerate or run its
clock. Only the matching media-completion token opens a fresh Fight.
Back cancels; cancelled/duplicate/old-token callbacks cannot open/reset combat.
The browser does not use a timer to bypass a blocked or failed video.
An explicit retry/unlock and back button keep errors recoverable without
starting a hidden fight. Editor's 5.166667-second timer is presentation fallback,
not evidence of actual browser playback. Editor controller harness separately
simulates callback delivery and is labelled as such.

Browser presentation uses inline video and Web Audio buffers, unlocked by the
existing initial click. Waiting/pause stops the intro sound; playing restarts
it at video.currentTime. Start bell/crowd precede the normal native callback.
The result event is phase guarded, so end bell cannot repeat. Home/rematch stops
prior sounds and invalidates previous media promises. M mutes ring audio along
with the existing hit/block effects. No changes to accepted Ramirez/glove art.

Audio source, derivation and limitations: `art/media/ring-girl/README.md`.
Web implementation follows [Web Audio source lifecycle](https://developer.mozilla.org/en-US/docs/Web/API/AudioBufferSourceNode)
and [media play promise handling](https://developer.mozilla.org/en-US/docs/Web/API/HTMLMediaElement/play).
Audio cut points are spectrally inferred, not approved by listening; the bell
tail has residual crowd, and faded loop edges are not isolated crowd stems.

## Verification

Model/vitals/flow: 111/111 PASS; geometry/regression: 172/172 PASS; controller:
118 checks EXIT=0; initial media unit harness: 14/14 PASS. Controller tests simulate
the browser completion callback, not actual clip playback. Existing Round2/P1V
regressions run via Wave1SelfTests with frozen historical files restored.
Pending: real WebGL media lifecycle and contact tests, build, deployment and
phone/audio Human UAT. New evidence exclusively in
`evidence/wave1/ring-intro`; do not overwrite combat-v3/onboarding-v2 reports.
Status is not HUMAN_PASS.

First compiled source `e2a03d625815617380912cbcdc37cf5e94b5df1f`: build PASS,
12 payload hashes PASS, but real media lifecycle FAIL (cancel callback targets
P0 Systems instead of Boxer P0 Bootstrap), leaving Intro frozen. Normal Fight
callback had the same routing issue. No release/deploy occurred. Preserve
attempt-1 media/general reports and failed artifact. Template now has a separate
sendRing route to the owning bootstrap; existing motion/input route is unchanged.
Native C# / model / contact source is unchanged by this repair.
