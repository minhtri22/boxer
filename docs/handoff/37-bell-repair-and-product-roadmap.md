# Bell repair and expanded product roadmap - 2026-10-10

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

Reward amounts and whether losses/draws pay; currency naming; initial shop catalogue/approved cosmetics; achievement definitions; venue unlock order and whether Cage means boxing-only; whether records are local only or eventually online. Default planning assumption: in-game currency, cosmetic-only shop, boxing in all three venues, local records first. These defaults are not an implemented economy or approved numerical balance.

HP-KO-001 remains owner-held. Completing screens does not automatically resume combat balance.
