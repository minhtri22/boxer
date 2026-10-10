# Coach last-match analysis / one training entry - 2026-10-10

## Frozen scope before implementation

1. Branch `feature/boxer-product-loop-wave1`, expected starting HEAD `3c5adcbb2e3821d514a879f3332e64a0a3632e7d`; public compiled source `f6ff54a0356454bedc2ba29425f9789eba1a87ec`.
2. Verified baseline: working Coach hub, three existing unscored control lessons, two information modules; duplicated Practice Controls entry still on Home; no preserved completed-match analytics.
3. Uncertainty: can a read-only completed-bout record survive resets/reload and route to real supplemental training without false statistics/progress or combat changes?
4. Hypothesis: capture authoritative counters/vitals once at CompleteBout, persist one validated versioned record, expose it inside Coach and after Result, and use transparent count-based suggestions linking existing modules.
5. Tightly scoped variable: product navigation + read-only result snapshot/persistence + presentation. Remove the separate Home practice route; lessons remain in Coach.
6. Acceptance: no Practice Controls/NEW? entry; no Home/Preview/Result bypass into practice; empty state before completed bout; exact immutable completed stats after Home/practice/intro cancellation/reload; next completed rematch replaces only once; real suggested lesson routing; 375-wide/landscape native fit and trusted touch; corrupt/unsupported persisted record rejected.
7. Regression: Coach native/model tests; current vitals/motion/contact/POV/controller/media suites; compiled real-UI training lessons/light guides; exact combat-hold source freeze; clean committed-source build + public bytes/decoder verification.
8. Out of scope: HP-KO-001 remains HOLD; no vitals/damage/regen/timing/AI/guard/geometry/duration changes, training stat upgrades, fake workout efficacy, replay, career or new coach assets. Native injected/synthetic tests are not phone Human UAT.

## Metric / suggestion contract

- Only completed scored bouts replace the last-match record. Home, preview, intro cancellation and unscored practice never replace it; a running rematch retains the previous completed match until it ends.
- Hits, blocked player shots, misses are separate resolved outcomes. Accuracy = player hits / resolved player outcomes, labelled as such; accepted punches and unresolved punches are separate. Do not count blocked shots as hits, or infer intentional dodges from opponent misses.
- HP, Stamina and Capacity are END-OF-MATCH snapshots, not averages, minimum energy, physiological measurements or upgraded attributes.
- Suggestions are transparent UI guidance, not a skill score: opponent hits -> head-control practice; player misses -> footwork; punch outcomes -> punch practice; remaining slots -> current conditioning/guard information. Maximum three existing modules, no fabricated AI exercise or reward.
- One validated local PlayerPrefs record, schema 1; no server account/history claim. Existing old versions have no historical match record; show an honest empty state until a new bout completes.

## Implemented / current engineering state

- Home has START and one TRAINING / COACH entry. Independent BeginTraining/TutorialFinished route is removed; practice can begin only from Coach. Each observed lesson completion returns Coach; the same real control lessons/light guides/blur remain.
- Coach adds TRẬN TRƯỚC -> TẬP BỔ SUNG; Result adds analysis entry without moving the existing rematch/Home targets. Review has six statistic lines and three touchable existing-module suggestions, or an honest empty state.
- `CoachMatchReview.Capture` reads final Phase0Telemetry counters, accepted player actions and CombatBout/vitals only after completion; product-side PlayerPrefs stores one validated schema-1 record. Combat telemetry and all ten held combat sources remain unchanged. No backfill from old versions or fake replay/history.
- Native record/validation/math/navigation: 56/56. Coach model/resource/navigation: 53/53. Existing motion 14,741/14,741; geometry 172/172; vitals 111/111; POV presentation 550/550. Controller: 139 checks, EXIT=0, including snapshots matching authoritative result and retaining prior record while a new bout runs. These are injected/synthetic checks, not device UAT.
- First native invocation failed compilation because the new snapshot `.meta` GUID had 33 digits and Unity ignored the asset. Corrected to 32 digits; rerun `native-meta-fix.log` returned EXIT=0. Preserve the initial `native-tests.log` as negative evidence; no PASS is inferred from its failure.
- Compiled screen/touch/visual/build/public gates: pending. Prior `coach-ui` evidence remains frozen; this package writes under `evidence/wave1/coach-analysis`.
