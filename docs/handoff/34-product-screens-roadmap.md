# Remaining product screens - execution roadmap

Date: 2026-10-10. Owner authorizes continuing remaining screens and holds HP/KO balance in `backlog.md`.

## Current source of truth

- Checkout `boxer-wave1`, branch `feature/boxer-product-loop-wave1`, starting HEAD `10123ff984a4aa24b19dbe4aeda7aff3d7d01c3d`.
- Pre-screen public compiled source `3035003caa96a96f8a1d3c347b43eb56c55a4b44` provided Home / Preview / Intro / Fight / Result / Rematch and independent control training. Slice A now adds Coach; current public compiled source `f6ff54a0356454bedc2ba29425f9789eba1a87ec`, artifact commit `32d7d9df14bd2f6cacca4c6e24176c76bed20f86`, successful workflow `38025081874`. See `35-coach-screen-implementation.md` and `evidence/wave1/coach-ui/deployment.txt` for verification and device-UAT boundaries.
- All six approved reference images are present. The older September research roadmap/state remain historical; this product-screen sequence does not rewrite those evidence claims or reopen combat research.
- HP/KO ordinary-phone balance is unresolved and held. Technical KO wiring success is not a phone-balance pass.

## Execution sequence

| Slice | Surface / reference | Actual minimum behavior | Boundary |
| --- | --- | --- | --- |
| A - implementation pass, phone UAT pending | Training / Coach, 05 | Home entry; five native touchable module cards; head/feet/punch entry into existing unscored practice; guard/conditioning coaching detail; Home/back/exit | No attribute upgrade, score, enemy AI or vitality spending in menus/practice. Approved portrait is 2D art, not new 3D coach; visual fidelity PARTIAL. |
| B - next | Fighter / Customization, 01 | Profile editing/persistence and genuine supported visual preview/options | Unsupported hair/body/style assets remain disclosed, not fake selections or combat stat changes. |
| C - next | Venues / Career selection, 04 | Venue-card navigation, availability, route to real current fight; honest locked future content | No invented opponent, reward, rank, arena implementation or economy merely to populate cards. |
| D - integration | Home/nav and existing preview/result | Consistent safe-area/mobile hierarchy and functional cross-screen routes | Preserve fight kernel, training gestures, intro/audio epochs and held balance. |

Implement and verify one complete slice before starting the next. The remaining-screen request unlocks these product surfaces, not unrelated progression/economy/replay systems. Keep approved references untouched.

## Slice A hypothesis / scope

The existing three working control lessons can be made accessible through a reference-led coach hub without changing combat. Guard/conditioning explain current rules and point to practice; they do not claim performance upgrades or new drills.

Expected edits: product flow/navigation, bootstrap training entry/return, ProductScreens coach cards, a pure module catalog, read-only audit fields, dedicated navigation tests and approved-art resource import. No edits to FighterVitals, damage/quality/regen, motion profiles, collision rig, opponent behavior or 45s bout semantics.

Acceptance: native route/invalid-route/cancel/completion tests; all former combat/vitals/motion regressions remain passing; rendered mobile portrait/landscape checks; menu/lesson/detail locks; no false completion from exit; no duplicate actors; no scored HP/stamina/capacity changes; build source/provenance retained. Visual match must be reported PASS/PARTIAL/FAIL; synthetic checks are not Human UAT.

Apple developer account / Mac are not available. Continue Unity/WebGL device UAT as the existing temporary channel; do not require a laptop to play or claim native iOS performance.
