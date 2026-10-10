# Bell repair and expanded product roadmap - 2026-10-10

Planning revision: handoff `38-owner-scope-and-arena-surround.md` is the current owner scope. The table below is the historical proposal, NOT authorization to implement customization, manual venue selection or settings/pause. PROFILE only; customization HOLD; earned Street -> Cage -> Tournament confirmed; settings/pause deferred. Audio repair receipts below stay unchanged.

## Frozen repair scope before implementation

- Branch `feature/boxer-product-loop-wave1`, expected HEAD `5fdb5db9d0960abdab125b5967323dd06d60aa92`; public compiled source `064aa84f9ff7a3dacf0f1be6b75877e48511b598`, release artifact `e10d14ab2b0ed9d2ef707613429515c4472145b7`.
- Owner reports missing start/end bell. Verified existing `bell.mp3` SHA256 `0ecb168ed8e1b7081c6112a3457ddc397b5a6355b03aede94c23a94ba80e8a13` decodes to silence (FFmpeg maximum and mean -91 dB). Duration/event-only tests passed previously but did not establish audible content; owner failure takes precedence.
- Original source SHA256 `1dec4970847aa75bc8ef8d2cf518253554486ef8d2ee4460b3f270c9ca3569ac` unchanged. Its 5.15-6.00s segment has signal (mean -18.9 dB, maximum -4.7 dB), unlike the derived bell. Crowd and intro have nonzero signal.
- Uncertainty: can corrected source-derived bell extraction produce non-silent audio at both existing transitions without changing gameplay/lifecycle?
- Hypothesis/minimal change: explicitly trim source audio, reset timestamps BEFORE local fades, encode only that segment; retain existing phase/token/master-gain/playback logic. No synthetic replacement sound, no gain boost to hide a silent file.
- Acceptance: preserve old silent asset; reproducible original hash/command; decoded PCM peak/RMS/duration check rejects old asset and accepts corrected asset; add non-silence check to actual browser decoder; existing 20 media assertions remain plus the new assertion, start/end/rematch/cancel/KO counts unchanged. Build from clean committed source, native/compiled regressions, public payload hash and decoder gates. Audio signal is not proof of target-phone speaker quality or listening approval.
- Regression evidence goes only to `evidence/wave1/bell-repair`; prior `coach-analysis`/`ring-intro` reports remain frozen. Native test changes are evidence routing only. All held combat sources/vitals/contact/AI/duration, approved visual references and accepted character shape remain unchanged. No facial/venue/economy implementation in this audio patch.

## Requested screen inventory and delivery plan

Current implemented surfaces: Home, opponent Preview, Ring Intro, Fight/HUD, Result/rematch, unified Coach, three real control lessons, two coaching information modules, last-completed-match review and supplemental-practice links. These have engineering/public receipts; phone Human UAT is not automatically PASS.

Remaining minimum product surfaces:

| Order | Surface / work package | Concrete behavior | Boundary / acceptance |
| --- | --- | --- | --- |
| 0 | Bell repair | Audible-content start/end cue in existing lifecycle | Nonzero decoded signal + real playback lifecycle + phone listening acceptance; no combat retune. |
| 1 | Fighter profile / customization | Name, nationality, actual supported appearance selection and persistence; visible preview | Reference 01; never enable absent hair/beard/body assets or hide unavailable options. |
| 2 | Modes / venue selection | Tournament, Street Boxing, Cage Boxing cards; preview of selected venue/rules/opponent | Reference 04; Street/Cage are boxing environments first, not silently MMA/kicks/grappling. |
| 3 | Street / Cage playable environments | Real street and fenced-cage visuals, appropriate lighting/crowd/ring-girl presentation, actual route into selected venue | Need approved environment references/assets. During HP hold, use current legal movement/contact boundaries and boxing kernel; visual cage is not a new collision/rule system. Mark approximation or lock unavailable venue, never claim an unbuilt arena is playable. |
| 4 | Post-match reward + wallet | Actual completed scored match grants configured in-game reward once; display delta and balance; local persistence | Unique match ID / idempotent settlement; no reward for training, cancelled intro, unfinished match, opening Result again or reloading. Win/loss/draw amounts need product decision. Virtual currency only, no real-money assumption. |
| 5 | Shop + owned equipment | Item detail/price, sufficient-funds check, buy once, owned/equip states, visible supported cosmetic change | One atomic wallet + ownership save; no duplicate debit/reward; real supported glove/trunk/wrap assets, no damage/HP/Stamina upgrades while combat held. |
| 6 | Record / achievements | Saved W-L-D, KO/points, real match history; honest empty state; badges only for observed completed conditions | Define conditions before award; no backfill of unstored old matches. Local records first; online global leaderboard needs backend/account/anti-cheat scope, not fake opponents/ranks. |
| 7 | Career progression | Venue/opponent availability and progress tied to actual saved records | Separate from raw achievements. Requires real opponents, progression rules and approved unlock/reward amounts. |
| 8 | Settings / pause / exit confirmation | Visible sound/haptics/motion permission/recalibration controls; language if supported; controlled pause/leave flow | Phone cannot rely on desktop M/H keys. Define clock/AI/audio behavior before claiming pause; exiting must not settle reward or corrupt history. |
| 9 | Integrated Home/navigation and expanded Result | Entrances to profile, modes, Coach, shop, records; Result shows outcome, reward, balance and Coach analysis | Consistent reference-02 mobile hierarchy, safe-area/touch checks, no duplicate Practice Controls. |

Orders are dependencies, not calendar estimates. Implement one testable slice at a time. Street/Cage art, shop and record-detail screens do not yet have approved full-screen references; the existing six images cover general art direction and venue/profile structure, not those new assets. Create/propose missing references for owner approval before claiming exact visual fidelity. Functional spec/empty states can precede asset production.

## FACE-001 - pain expression on resolved impact

- Owner requests visible pain instead of a static statue face. Current source applies a brief head/spine rotation through `BoxerFeedback.OpponentReaction` and `RamirezAcceptedRig`; there is no expression controller for brow/eyelid/jaw/mouth in that path. Facial bones/blendshape support in the accepted imported mesh still needs an actual asset audit; do not assume it exists or claim it is impossible without inspection.
- Separate presentation slice, scheduled after bell repair and before final integrated UAT; it can be scoped before the economy package. It is NOT a new screen and is not permission to retune combat or remodel the accepted body.
- Audit mesh/rig first. Prefer localized facial blendshapes/bones: brief eye narrowing/blink, brow contraction, cheek/mouth/jaw tension, then controlled return to guard/neutral; preserve recognizability and avoid exaggerated/cartoony expressions. Head and body HIT may use distinct expression envelopes. Block/Miss must not trigger the same pain response.
- Drive only from an existing resolved HIT receipt, with bounded quality/region-aware intensity; use receipt identity to prevent duplicate triggers. No pre-contact fake hit, expression-driven damage, movement/collision-anchor change, camera shake or per-frame allocation. Repeated hits blend without locking the face; reset clears expression; KO/result transition follows actual existing outcome, never an expression timer.
- First delivery focuses on the visible opponent Ramirez. The player's face is not visible in current first-person gameplay; later third-person portraits/replay require separate scope.
- Acceptance: static neutral vs HEAD HIT vs BODY HIT vs BLOCK vs MISS rendered comparisons; actual trusted-touch impact video at close readable POV, repeated hits, reset/rematch; no expression on blocked/missed attack; invariant combat anchors and measured frame cost. Visual/device review is required in addition to deterministic triggers. No new injury/blood system is implied.

## Open product decisions (planning, not blockers for bell)

Reward amounts and whether losses/draws pay; currency naming; initial supported shop catalogue; achievement definitions and unlock thresholds; whether records are local only or eventually online. Owner has now confirmed earned Street -> Cage -> Tournament, with no manual venue selection. Appearance customization is HOLD; settings/pause deferred. Other defaults are planning assumptions, not implemented economy or approved numerical balance. See handoff 38.

HP-KO-001 remains owner-held. Completing screens does not automatically resume combat balance.

## Repair engineering evidence

- Original released bell: decoded PCM peak=0, RMS=0, 0.885737s. Corrected source-derived bell: 0.85s, mono peak=0.782724, RMS=0.146582, active fraction=0.977191, SHA256 `21b2cd12f42967f0b2ee0da27b51e80dd3826ca7e530d4bb00fd17bb94a8c5a7`; source hash unchanged. Six signal/provenance checks PASS, including rejection of the old asset. No source-listening approval claimed.
- Native analysis 56/56, Coach 53/53, motion 14,741/14,741, contact geometry 172/172, vitals 111/111 and presentation 550/550 PASS. All 12 held-source checks PASS. Runtime Scripts/WebGLTemplate/Plugins are unchanged; the sole runtime content correction is the source-derived bell asset.
- First controller attempt with `-nographics` crashed in Unity `GfxDevice::DrawSharedGeometryJobs` during `Camera.Render` screenshot capture, exit `-1073741819`; preserve `controller.log` as negative evidence. It does not provide a controller PASS. Retried with `-force-d3d11` (no no-graphics switch), real editor render capture and controller completed `CHECKS=139 EXIT=0`; preserve `controller-d3d11.log`. No assertions relaxed and no unrelated process stopped.
- Committed compiled source `b2dabf90b3fa8ae55c902ffa10210f735f0778f8`: clean committed-source WebGL build succeeded, EXIT=0, 10.2618705s, 62,660,868 bytes. Prior build preserved at `_wave1-coach-064aa84-before-bell-repair`; output directory absent before build. Scene/settings restored unchanged. Only data/index/provenance and external bell payload differ; existing runtime script/plugin/template code is unchanged.
- Compiled real-UI/trusted-input suites: media 21/21 (all original 20 plus actual decoder signal), Coach 52/52, training/POV 33/33, isolated contact 18/18 PASS, zero JS/load/HTTP errors, all product version `w1-b2dabf90b3fa8ae55c902ffa10210f735f0778f8`. The extra AudioContext is decode-only and closed; it does not mutate native state or the game's active playback context. Real HIT/KO and normal timed completion keep exactly-one end cue. Browser decoded stereo bell peak=0.566018, RMS=0.105414, duration=0.85s, active fraction=0.971238. No numeric rule change, clipping claim or phone speaker approval inferred from event-only evidence.
- Native/editor routing edits only select separate `bell-repair` evidence destinations; original acceptance counts are retained, media increases from 20 to 21 with decoded-signal assertion.

## Public audio repair receipt

- Artifact `929e8874b7b80d1ef94ddc70c8651a6614e6ad16`, compiled source `b2dabf90b3fa8ae55c902ffa10210f735f0778f8`. Exact remote feature SHA verified; artifact source/tools/art match the compiled source commit. No main merge/rebase or accepted visual-reference change.
- Workflow [38046281808](https://github.com/minhtri22/boxer/actions/runs/38046281808): completed/success at the exact artifact. All five Pages policies preserved.
- Public [phone test](https://minhtri22.github.io/boxer/?release=b2dabf9): 21/21 media/decoded-signal assertions PASS, zero JS/load/HTTP errors, correct product version. Public stereo bell duration=0.85s, peak=0.566018, RMS=0.105414, active fraction=0.971238. All 12 payloads HTTP 200/SHA256-equal to local build, including the corrected bell; provenance hash `f80ca48d529bf52de9db8eb1c8410ebe2d37ab6c8974e17b7d4ddabb5a225141`. Receipt `evidence/wave1/bell-repair/deployment.txt`, verified `2026-10-10T10:53:23.2895483+00:00`.
- Status `DEPLOYMENT_PASS_PENDING_HUMAN_UAT`. Owner should listen on target phone: initial start, POINTS ending, KO when it actually occurs, and rematch; existing mute respected. These checks establish real non-silent content/decoder/lifecycle, not physical speaker output or listening acceptance.
- FACE-001 and all newly listed venues/economy/record/settings surfaces remain PLANNED, NOT implemented in this audio repair. HP-KO-001 remains HOLD.
