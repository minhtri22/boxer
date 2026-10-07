# Wave 1: product loop and authoritative vitals

Date: 2026-10-08. Owner approved Home/Preview/Result/Rematch, player glove
refinement, and real gameplay HP/Stamina affecting impact/recovery and HP-zero
bout termination. These are deterministic game rules, not physiological truth.
Base: bcbcb6c374a233e7d379aa31b21b37af8727fbf4. Branch:
feature/boxer-product-loop-wave1. Prior Ramirez candidate remains untouched.

## Initial versioned rules (balance is pending device UAT)

- Both fighters: HP=100, long-term Stamina=100, short-term Capacity=100.
- Accepted action capacity cost: jab 12, cross 16, hook 18, uppercut 20,
  overhand 22. Long-term cost is capacity cost x 0.2. Clamp resources to 0..100;
  exhausted fighters may still attack with reduced quality; no input-lock invention.
- Quality captured BEFORE paying cost at accepted commit:
  Q = 0.4 + 0.6 x (0.6 x Capacity/100 + 0.4 x Stamina/100).
- Effective recovery = existing family/profile recovery x (1 + 0.75 x (1-Q)).
  Commit/extend durations, reach, trajectories and collision geometry unchanged.
- Base HIT damage: jab 6, cross 9, hook 8, uppercut 10, overhand 12; multiply
  by captured Q. No invented counter damage multiplier. Body HIT additionally
  drains defender Stamina by damage x 0.35. BLOCK deals zero HP damage and
  drains defender Capacity by 25% of attack cost; MISS deals neither.
- Each accepted attack has a unique receipt; one resolution only. Rejected
  input costs nothing. Duplicate/stale receipts, inactive bouts and post-KO
  events cannot change state. Tutorial is unscored and has no vitals costs.
- Capacity regeneration: guard 18/s, recovery 6/s, commit/extend 0/s.
  Stamina regeneration: guard 4/s, recovery 1/s, commit/extend 0/s.
  Actual planar locomotion intensity 0..1 reduces stamina by 2/s x intensity.
  No HP regeneration. Menu, result and application pause do not regenerate.
- HP=0 immediately locks both fighters; winner is opponent of depleted fighter.
  At 45s timeout, compare remaining HP (epsilon 0.0001), otherwise DRAW.
  Final reason KO or POINTS is displayed; do not present this as real boxing scoring.
- Model is deterministic for the same ordered accepted/resolved events and dt
  sequence; no claim of bit-identical cross-framerate physics or clinical accuracy.
- Reset restores actors, receipts, vitals, counters, input fingers and AI RNG.
  No actor recreation per rematch; tutorial shown once per session, replayable.

## UI/reference and ownership

All six approved images are present in docs/handoff/reference-ui. Home and
combat/gloves use 02/03; 01/04/05 remain later surfaces; 06 is accepted Ramirez.
No dedicated approved Preview/Result reference exists. Their design inherits
black/gold, condensed typography and mobile hierarchy, with fidelity reported
honestly rather than treated as new owner-approved concept art.

HOME -> PREVIEW -> ONBOARDING (first session bout) -> FIGHT -> RESULT;
RESULT -> REMATCH or HOME. Home also permits replaying the existing tutorial.
CAREER/full gym/shop/PvP/replay/cloud/AI coach remain out of scope.
CombatBout owns vitals and receipts; controllers own accepted actions;
Round2CombatRig owns geometric outcome; HUD reads vitals only. UI consumes
menu touches so they cannot become punches. Player glove art follows native
POV anchors; no combat colliders are altered to improve the appearance.

## Acceptance

Pure tests: resource bounds, costs per family, rejected/duplicate/stale action,
MISS/BLOCK/HIT, symmetric fighters, body cost, quality/recovery monotonicity,
KO/timeouts, no post-end writes, reset, dt accounting and flow transitions.
Runtime: real controller accepts/rejects, HUD/state agree, no hidden input in
menus, KO stops both actors, repeated rematches reset without duplicate objects.
All prior geometric tests remain unchanged and are rerun. Each new build must
come from clean committed source and match marker/version/provenance/hashes.
Human/device UAT and visual reference comparison are separate gates. Existing
native-mesh/contact mismatch and triangle-budget failures remain disclosed.
