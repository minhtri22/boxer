# Boxer product backlog

Updated: 2026-10-10. The owner directs completing the remaining product screens before returning to combat balance.

## HP-KO-001 - Ordinary phone play rarely reaches KO

- Status: **HOLD - OWNER REVIEW AFTER REMAINING SCREENS**.
- Owner report: repeated taps and swipes on phone reduce HP too little before the 45-second timeout. Phone is the primary platform; WebGL is a temporary device-UAT delivery path, not a laptop product.
- Reproduction: public `w1-3035003caa96a96f8a1d3c347b43eb56c55a4b44`; simple tap 71 accepted / 4 HIT / Ramirez HP 89.1043; mixed 45 accepted / 5 HIT / HP 69.6771. Both POINTS at 45s; no HP regeneration.
- Evidence: `evidence/wave1/hp-ko-diagnostic-20261010/report.json` and `diagnosis.md`. Synthetic browser reproduction is not phone Human UAT.
- Analysis: sustained automatic guard, block capacity cost vs regeneration, limited contact openings and short bout. The HP-zero KO terminal branch exists; a precisely timed desktop strategy does not establish ordinary-phone balance.
- Deferred candidates: accumulated defense fatigue, shared guard pose/contact response, more usable openings, duration/damage pacing review and phone-led acceptance.
- Scope freeze while held: no retuning HP, damage, stamina/capacity costs/regeneration, timing, AI, guard/contact geometry or bout duration to address this item. Keep the current deployed combat implementation unchanged.
- Resume only after the remaining screen package is ready and the owner explicitly returns to this item. Do not automatically resume after UI completion or label the balance issue resolved.

## Asset / product follow-ups (not implicitly authorized mechanics)

- PROFILE: approved identity/profile screen only; no appearance selector. See current scope in `38-owner-scope-and-arena-surround.md`.
- CUSTOMIZATION-001: **HOLD - OWNER ART APPROACH REVIEW**. Current Blender character differs substantially from reference. No hair/beard/body/skin controls or fresh character refinement until a viable approach is agreed.
- CAREER-001: approved earned order **Street -> Cage -> Tournament**, no free venue picker. Achievement/unlock thresholds remain TBD; current tournament demo does not imply earned progression.
- SETTINGS-001: **DEFERRED - NOT NEEDED NOW**. No new settings/pause/exit package; preserve necessary existing navigation.
- ARENA-001: four photo audience sides + localized sparse Fight-only flashes deployed; compiled `dd9434b`, public arena 12/12 and payload hashes 12/12 PASS. **DEPLOYMENT_PASS_PENDING_HUMAN_UAT**, production fidelity PARTIAL (repeated 2D photo, visible corner join). Scope/evidence in handoff 38. No new Street/Cage environments or mechanics implied.

- FACE-001: owner requests visible pain on real resolved HIT; current head/spine rotation is not facial emotion. Plan and acceptance in `37-bell-repair-and-product-roadmap.md`; facial rig/blendshape asset audit still required. Presentation only; no held collision/vitals changes, fake hit, or mandatory blood/injury system.
- Owner approves post-match virtual rewards/wallet, shop/owned equipment and saved record/achievements; earned Street/Cage/Tournament environments depend on Career. Current plan is handoff 38; current gameplay still implements one tournament arena, no delivered economy/shop or global leaderboard. Do not label planned surfaces as playable systems.

- Production animated 3D coach asset is unavailable; an approved reference-art portrait may anchor the first coach screen, clearly reported as 2D art rather than a live coach.
- Full fighter hair/beard/body customization, multiple playable venues/opponents, ranks/rewards, shop/economy and training stat upgrades need real assets / product implementation. Screen navigation does not imply those systems exist.
