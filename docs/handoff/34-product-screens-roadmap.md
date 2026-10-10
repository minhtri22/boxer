# Remaining product screens - execution roadmap

Date: 2026-10-10. Owner authorizes continuing remaining screens and holds HP/KO balance in `backlog.md`.

## Current source of truth

- Checkout `boxer-wave1`, branch `feature/boxer-product-loop-wave1`, starting HEAD `10123ff984a4aa24b19dbe4aeda7aff3d7d01c3d`.
- Pre-screen public compiled source `3035003caa96a96f8a1d3c347b43eb56c55a4b44` provided Home / Preview / Intro / Fight / Result / Rematch and independent control training. Initial Slice A delivery: compiled source `f6ff54a0356454bedc2ba29425f9789eba1a87ec`, artifact commit `32d7d9df14bd2f6cacca4c6e24176c76bed20f86`, successful workflow `38025081874`. Its receipts remain historical under `coach-ui`.
- Owner-requested Slice A refinement: one TRAINING / COACH entrance, controls contained within it, actual last-completed-match analysis -> existing supplemental practice. See `36-coach-last-match-analysis.md` and `evidence/wave1/coach-analysis/deployment.txt` for latest release status; absent receipt means not publicly verified. No historical match backfill, training stat awards or combat retuning.
- All six approved reference images are present. The older September research roadmap/state remain historical; this product-screen sequence does not rewrite those evidence claims or reopen combat research.
- HP/KO ordinary-phone balance is unresolved and held. Technical KO wiring success is not a phone-balance pass.
- Latest audio-only repair: compiled `b2dabf90b3fa8ae55c902ffa10210f735f0778f8`, artifact `929e8874b7b80d1ef94ddc70c8651a6614e6ad16`, successful workflow `38046281808`; public media 21/21 now includes actual non-silent bell PCM, all 12 public payload hashes match. See handoff 37 and `evidence/wave1/bell-repair/deployment.txt`. New facial/venue/economy/record systems remain planned.

## Execution sequence

Latest owner scope: `38-owner-scope-and-arena-surround.md` supersedes handoff 37's remaining-screen planning. PROFILE only; appearance customization HOLD. Earned Career order is Street -> Cage -> Tournament, never a free venue picker. Reward/wallet, Shop and record/achievements are approved separate slices; thresholds and economy amounts need approval. Settings/pause is deferred. FACE-001 is a separate resolved-hit visual slice; no held combat/model rework. Street/Cage art and shop/record screens need additional approved references/assets. These systems are planned, not already delivered.

| Slice | Surface / reference | Actual minimum behavior | Boundary |
| --- | --- | --- | --- |
| A - implementation pass, phone UAT pending | Training / Coach, 05 | Home entry; five native touchable module cards; head/feet/punch entry into existing unscored practice; guard/conditioning coaching detail; Home/back/exit | No attribute upgrade, score, enemy AI or vitality spending in menus/practice. Approved portrait is 2D art, not new 3D coach; visual fidelity PARTIAL. |
| B - next | Fighter Profile, 01 | Identity metadata/persistence and real record linkage | No appearance controls or Blender/model rework; customization HOLD. |
| C - after saved record/economy | Earned Career, 04 | Street -> Cage -> Tournament progress and achievement-gated next venue | No manual venue choice; unlock numbers TBD; missing arenas remain unavailable. |
| D - integration | Home/nav and existing preview/result | Consistent safe-area/mobile hierarchy and functional cross-screen routes | Preserve fight kernel, training gestures, intro/audio epochs and held balance. |

Implement and verify one complete slice before starting the next. Reward/wallet, shop and record are now approved in handoff 38, but no numerical economy or global leaderboard is implicitly approved. Keep approved references untouched.

## Slice A hypothesis / scope

The existing three working control lessons can be made accessible through a reference-led coach hub without changing combat. Guard/conditioning explain current rules and point to practice; they do not claim performance upgrades or new drills.

Expected edits: product flow/navigation, bootstrap training entry/return, ProductScreens coach cards, a pure module catalog, read-only audit fields, dedicated navigation tests and approved-art resource import. No edits to FighterVitals, damage/quality/regen, motion profiles, collision rig, opponent behavior or 45s bout semantics.

Acceptance: native route/invalid-route/cancel/completion tests; all former combat/vitals/motion regressions remain passing; rendered mobile portrait/landscape checks; menu/lesson/detail locks; no false completion from exit; no duplicate actors; no scored HP/stamina/capacity changes; build source/provenance retained. Visual match must be reported PASS/PARTIAL/FAIL; synthetic checks are not Human UAT.

Apple developer account / Mac are not available. Continue Unity/WebGL device UAT as the existing temporary channel; do not require a laptop to play or claim native iOS performance.
