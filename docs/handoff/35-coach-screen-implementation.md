# Screen A - Training / Coach

Date: 2026-10-10. Roadmap: `34-product-screens-roadmap.md`. Balance disposition: `backlog.md`, HP-KO-001 HOLD.

## Implemented / scope

- Home keeps START and the existing quick practice entry; a new TRAINING / COACH button opens a native Unity hub.
- Reference-led black/gold layout: coach portrait left, five real selectable module cards right, selected-state highlight, TRAIN NOW / information CTA, Home. The portrait is a cropped rendering of the unchanged approved `05-training-coach.jpg`, copied byte-identical into Resources (SHA256 `2334dfa6f20ca43b6450bf39dc9071984ca8c80264b7ccfe8fb21adce1568b82`). It is 2D reference artwork, not a new animated 3D coach.
- Head / Footwork / Punches enter the selected existing unscored practice stage. Both exit and observed completion return Coach. Exit grants no completion. Each genuine completed control lesson persists its own bit; all three are required to mark the entire control tutorial completed.
- Guard / Conditioning are clearly labelled information modules explaining current rules. They link to real air-punch practice, but grant no attributes, HP, Stamina or score and do not invent AI drills. No fake upgrade/progress bars.
- Legacy quick practice still proceeds head -> feet -> punches and returns Home; the former calibration, gestures, light guides and live blurred practice scene remain authoritative.
- Navigation is denied during Fight. Coach/info lock input/actors/vitals/clock. New native cards and CTAs meet 44 logical pixels at a 320-wide fitted portrait; landscape retains fitted portrait layout, not a separate full-width tablet design.

## Held combat preserved

`combat-freeze.txt`: 12 checks, exact ten combat/controller/input/motion source files unchanged from `10123ff`; authoritative CombatBout/vitals prefix (6375 normalized characters) identical; Bootstrap duration still 45s. Only ProductFlow in the same CombatBout source file gains navigation states. No balance issue is claimed resolved.

## Verification

- Native Coach model/resource/navigation: 53/53.
- Existing native motion/timeline/feedback: 14,741/14,741; combat geometry 172/172; vitals 111/111; POV presentation 550/550.
- Existing injected controller audit: 118 checks, EXIT=0. Not physical touch evidence; startup logs may contain Unity indexing diagnostics, so no blanket claim of error-free Editor logs.
- Compiled source `f6ff54a0356454bedc2ba29425f9789eba1a87ec`, product `w1-f6ff54a0356454bedc2ba29425f9789eba1a87ec`: WebGL Succeeded, 27.1299589 build seconds, 62,614,952 bytes. Scene and ProjectSettings restored; only intended release payloads changed.
- Compiled Coach browser: 31/31; former UI/practice/POV browser: 33/33; local real decoder/intro/cancel/bells/crowd/rematch lifecycle: 20/20. All report the exact current compiled product version and zero JS/load/HTTP errors. Coach checks include no unused MP4 request throughout Home/Coach/practice, information text fit, per-lesson completion, cancel/no false award, persistence reload, real fight round-trip, 375-wide trusted touch and landscape.
- Existing physical-input/contact browser regression: 18/18, exact current product version, zero errors. The precise synthetic tactic still reaches native KO; this is terminal/contact regression evidence, NOT resolution of held ordinary-phone balance. Native suites and controller audit were rerun after the on-demand media change; totals above remain passing, Unity helper EXIT=0.
- Public deployment verification: pending.
- First compiled candidate `a3f14d9` is NOT deployed: initial Coach browser FAIL (27 checks, one hidden preloaded-video request aborted in the audit containing reload), plus manual render inspection found long Conditioning copy clipped. Its full report/screenshots are retained in `coach-browser-first-candidate`, unchanged FAIL. Shortened information copy and added measured native text-height gates in `3d7bf74`; its 30-check audit still failed only on the video request, retained in `coach-browser-reload-failure`.
- Timestamped `3d7bf74` reproduction in `coach-browser-idle-video-failure` locates the abort at 15,096 ms, while the current action was `feet-completed`, BEFORE any intro/fight/reload. Waiting for transfers before reload did not solve this idle preload problem. `f6ff54a` switches the hidden video to `preload="none"`: normal intro `play()` loads it on demand, menus/practice do not fetch it. All former assertions and the zero-error requirement are retained, plus a 31st assertion forbidding idle MP4 requests. Current Coach and media gates pass; no request failure was filtered out or relabelled PASS.
- Final allowed status remains `IMPLEMENTATION_PASS_PENDING_HUMAN_UAT`. Phone is the product; browser desktop synthetic input is only engineering evidence. HP-KO-001 remains an explicit owner-held issue.

## Reference-render inspection

Compared the approved `reference-ui/05-training-coach.jpg` with compiled 540x960 and fitted 375x812 portrait captures. This is visual inspection of renders, not phone Human UAT.

| Component | Observation | Visual verdict |
| --- | --- | --- |
| Coach portrait / identity | Byte-identical approved portrait, cropped to the left column; no invented replacement character | PASS for artwork identity; PARTIAL for embodiment (2D, not animated 3D) |
| Black/gold hierarchy | Approved visual palette, portrait left, five cards right, prominent gold TRAIN NOW CTA | PASS for hierarchy; PARTIAL for exact fidelity (native simplified card treatment) |
| Module illustrations / upgrade bars | Illustrated silhouettes and claimed attribute upgrade bars are not implemented; native text describes only real functionality | PARTIAL; no false progress or stat award |
| Selection / completion | Selected card has a visible gold/brown fill; genuine completed control lessons show DA TAP tags | PASS for rendered states, pending device readability/touch UAT |
| Guard / Conditioning copy | Shortened copy is fully visible, independently measured to fit the native text rectangle | PASS for inspected portrait; pending actual phone readability |
| 375-wide / landscape | Fitted portrait stays in bounds; landscape is letterboxed portrait, not a separate tablet composition | PASS for bounds; PARTIAL for responsive composition |
| Living gym / coach scene | Existing arena-backed menu plus cropped coach artwork, not the fully realized reference gym | PARTIAL; production animated coach/gym assets remain future work |

Overall visual-reference verdict: **PARTIAL**, disclosed first functional screen slice. No `REFERENCE_VISUAL_PASS` or `HUMAN_PASS` is inferred from native tests, screenshots or deployment.

## Remaining sequence

Fighter / Customization (reference 01), then Venues / Career surface (04), then integrated navigation. Progression, rewards, shop, new opponents and real 3D coach are not implicitly implemented by this screen package. Do not resume balance merely because screens finish.
