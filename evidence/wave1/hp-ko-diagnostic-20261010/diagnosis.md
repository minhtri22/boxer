# HP / KO gameplay diagnosis - 2026-10-10

Owner disposition: HOLD in `docs/handoff/backlog.md` on 2026-10-10; complete remaining screens first. Proposed balance changes below are not authorized or implemented.

User confirms HP falls too little before timeout, not HP regeneration. Phone is the target platform; browser delivery is temporary UAT pending Apple developer account / Mac access. Desktop synthetic evidence does not replace phone Human UAT.

## Verified current state

- Public provenance: compiled source `3035003caa96a96f8a1d3c347b43eb56c55a4b44`, product `w1-3035003caa96a96f8a1d3c347b43eb56c55a4b44`; exact same version reported by new diagnostic browser run.
- Gameplay source / build unchanged. No commit, push, workflow dispatch, parameter tuning or implementation fix performed during this diagnosis.
- FighterVitals HP changes only through reset and HIT damage. Tick regenerates Capacity/Stamina, not HP. CombatBout.Resolve ends KO when HP <= 0. Timeout is 45 seconds and compares remaining HP.
- BLOCK inflicts zero HP; Capacity block cost is 25% of accepted attack cost (3 to 5.5), while Guard Capacity regeneration is 18/s. Guard collision/pose does not become weaker just because Capacity is depleted. This combination favors sustained defense. Attack Quality declines with attacker stamina/capacity, reducing damage even on the few HITs.

## New public diagnostic (not a release gate or phone UAT)

Trusted browser touch into compiled game, one fixed initial forward step; no score/pose/outcome setters and no reads used to time opponent phases or steer to a precise distance. Second scenario uses fixed-period forward steps and a fixed cycle of tap/up/down/left/right gestures. Read-only snapshots record consequences, not drive tactical decisions.

| Pattern | Accepted | HIT | BLOCK | MISS | Player HP | Ramirez HP | End |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| Tap only | 71 | 4 | 67 | 0 | 100 | 89.1043 | 45 s / POINTS |
| Mixed + fixed steps | 45 | 5 | 39 | 0 | 100 | 69.6771 | 45 s / POINTS |

One accepted mixed action remains unresolved when timeout ends the bout; do not count it as a HIT/MISS. Neither trace contains HP increases. Raw snapshots, version and errors are in `report.json`; screenshots show actual result screens.

Previous full contact test reached KO at 29.9252 seconds with nine HITs, but its input loop read opponent Commit and steered into a 0.715-0.755 m pocket. It proves KO wiring, not ordinary-phone balance. Earlier idle-guard acceptance also explicitly required surviving the default bout.

## Conclusion / proposed next scope

Reproduced the reported outcome under simple input. Evidence supports a defense/contact-opportunity/short-bout balance problem, not a missing HP-zero terminal branch. The exact phone interaction remains Human UAT, not established by this desktop run.

Propose separate approved balance repair: meaningful accumulated defense fatigue with shared rendered/collision guard response, more usable openings/counter range for phone control, then evaluate duration/damage pacing with ordinary taps/mixed swipes. Keep authoritative HIT/BLOCK/MISS and no fake KO on timeout. Do not tune numeric health or silently enlarge hitboxes simply to make the test pass. Device Human UAT remains pending.
