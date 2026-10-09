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
118 checks EXIT=0; final media unit harness: 15/15 PASS. Controller tests simulate
the browser completion callback, not actual clip playback. Existing Round2/P1V
regressions run via Wave1SelfTests with frozen historical files restored.
Repaired compiled source `9ba71d51dfc23e42aa1fc7228d2c666deab1bb39`:
WebGL build PASS (10.1815667-second incremental repair after the first
310.3022826-second full build); 12 local payload hashes match provenance;
scene/settings unchanged after build. Real WebGL media 20/20 PASS (zero errors),
general UI/training 28/28 PASS (zero errors), full physical browser-input/contact
18/18 PASS (zero errors). The extra contact check proves actual KO stops crowd
and schedules end bell once, retaining all 17 previous contact requirements.
Media clip's measured playing→ended span is 5153.1 ms. Timeout ends at exactly
45 scored seconds, separately from the intro. Actual touch attacks yielded
9 HITs, KO at 26.099 scored seconds, player HP 94 / opponent HP 0, both actors
and gameplay input locked. No score/contact callback injection in browser tests.
Native model/controller code is identical between e2a03d6 and 9ba71d5; no
damage, fatigue, AI-choice or contact-threshold changes were made for this repair.
Deployment PASS: workflow [37877876524](https://github.com/minhtri22/boxer/actions/runs/37877876524)
completed successfully at artifact commit `638bcc282ff3b5e7c8e4e36021a906daa3af7931`.
Public provenance is byte-identical; all 12 public payloads return HTTP 200 and
match local hashes. Direct public WebGL media lifecycle also passed 20/20 with
zero JS/load/HTTP errors, including decoder, cancel, reset, timer and end bell.
Pages serves MP3 as audio/mp3 rather than the verifier's initial audio/mpeg
assumption. The first verifier mismatch is retained; real public audio decoding
was independently verified without changing the published artifact or gameplay
checks. All five Pages policies remained unchanged. Test:
https://minhtri22.github.io/boxer/?release=9ba71d5
Pending: phone/audio Human UAT. New evidence exclusively in
`evidence/wave1/ring-intro`; do not overwrite combat-v3/onboarding-v2 reports.
Status: **IMPLEMENTATION_PASS_PENDING_HUMAN_UAT**. Browser automation does not
verify speaker output, seamless-sounding loop quality, phone autoplay/motion or
device performance. Owner should listen on target phone. Source clip's existing
visual marks are retained; no image/character redesign is part of this work.

First compiled source `e2a03d625815617380912cbcdc37cf5e94b5df1f`: build PASS,
12 payload hashes PASS, but real media lifecycle FAIL (cancel callback targets
P0 Systems instead of Boxer P0 Bootstrap), leaving Intro frozen. Normal Fight
callback had the same routing issue. No release/deploy occurred. Preserve
attempt-1 media/general reports and failed artifact. Template now has a separate
sendRing route to the owning bootstrap; existing motion/input route is unchanged.
Native C# / model / contact source is unchanged by this repair.
