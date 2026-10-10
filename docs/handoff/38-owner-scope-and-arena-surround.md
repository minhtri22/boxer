# Owner scope revision / four-sided audience and flash - 2026-10-10

## Current owner decisions

- Fighter PROFILE is required; appearance customization is HOLD. No hair, beard, body, skin, mesh/Blender refinement or new character-appearance selector in the profile slice. Owner reports current character remains far from approved reference; an accepted import is not reference-fidelity approval. Find a viable art approach separately before resuming customization.
- Career advances by actual saved achievement, not a free venue/mode selector. Owner confirmed order: **Street Boxing -> Cage Boxing -> Tournament**. Current tournament WebGL demo is not proof of earned career progression. Unlock thresholds/rewards need product decisions before implementation; do not infer rank from old unstored matches, practice or page reloads.
- Post-match virtual reward/wallet, Shop/owned equipment and Record/achievements are approved next product packages. They remain separate slices with exactly-once ledger rules. Shop must not require held character remodels or sell combat-stat upgrades while HP-KO-001 is held; disclose unavailable item assets rather than fake equipping.
- Settings/pause/exit package (old item 6) is NOT required now; defer it. Necessary buttons for the currently implemented flow remain, but no new settings/pause project in this slice.
- FACE-001 remains a separate planned visual issue. Read-only facial-asset audit can establish feasible approaches; appearance HOLD does not authorize a fresh Blender/model rebuild to satisfy it. Report a dependency if missing facial controls require held art work.

## Frozen arena implementation scope before edits

1. Checkout `boxer-wave1`, branch `feature/boxer-product-loop-wave1`, expected HEAD `f1049cd6f4b65c1c9c3cd16a1fd57f35a0395055`; public compiled `b2dabf90b3fa8ae55c902ffa10210f735f0778f8`, artifact `929e8874b7b80d1ef94ddc70c8651a6614e6ad16`.
2. Verified baseline: `EVVisualShell.Arena()` builds one distant audience photo plane at front and disables old primitive crowd renderers. The other three directions have no equivalent crowd scenery. This confirms the owner's arena report.
3. Uncertainty: can four inward-facing, depth-tested audience sides and small audience flash cues fill the surrounding views without altering physical ring, character geometry or contact/HP/input rules?
4. Hypothesis: reuse the exact approved existing arena image on four world-space sides around the same ring; one shared plane mesh/material. Add bounded localized additive flash quads outside the legal combat area, not actual scene lights/postprocessing.
5. Single scope: arena visual scenery/flash only plus read-only diagnostics/tests/evidence routing. No new playable Street/Cage arenas, progression, profile, reward or shop implementation inside this patch.
6. Acceptance: four actual inward-facing crowd planes, seam/diagonal render inspection; no collider/light/physics changes; shared resources and bounded flash nodes; one localized flash at a time, sparse schedule, no full-screen strobe. Fight-only effect, reset off outside Fight; no damage/score/contact dependency. Diagnostic render comparisons in four directions and flash on/off, then compiled real-UI observation and immutable combat sources.
7. Regression: held source 12-check freeze; existing native analysis 56, Coach 53, motion 14741, geometry 172, vitals 111, POV 550, controller 139; compiled Coach 52, training/POV 33, media 21 including non-silent bell, physical contact 18. Dedicated arena tests/render evidence. Clean committed-source WebGL build/public bytes/decoder verification before deployment claims.
8. Out of scope: body/face art refinement, new crowd images, true animated 3D spectators, HDR/bloom/real flash lighting, cinematics/camera steering, mechanics retune, free venue selection, shop economy numerical balance, deferred settings/pause. Phone Human UAT remains required. Four photo scenery sides are not a claim of full 3D grandstand production quality.

## Revised product order after this arena fix

1. Profile metadata (name/nationality/identity and real record linkage), no appearance controls.
2. Persistent completed-match history/record and exactly-once virtual reward/wallet; outcome ID ties together result, ledger and achievements.
3. Shop / owned equipment using genuinely available item support, no Blender remodel or held stat upgrades.
4. Career progression display from Street -> Cage -> Tournament, unlocks derived from saved achievements. Requires actual venue assets before calling each stage playable; no bypass/free choice.
5. FACE-001 feasibility/visual slice where possible without held model rework; otherwise report the art dependency.

Reference 01 is layout guidance for PROFILE only while customization is held. Reference 04 guides the career map but not a manual mode picker. Full new shop/history and Street/Cage environment references still need owner approval. HP-KO-001 stays HOLD; settings/pause is deferred.

## Native implementation / visual evidence

- Presentation-only `ArenaSurround`: four inward-facing world quads, original front placement/UV/scale retained; two shared meshes/materials; 24 initially disabled local flash renderers. At most one 0.16s pulse every 1.5s (each side receives one approximately every 6s), maximum strength 0.75. Fight-only, unscaled presentation time; leaving Fight/disable clears effect. No scene lights, colliders, actor/camera transform writes or gameplay random/state dependencies. Read-only WebGL audit reports actual owned renderer counts/intensity/observed side mask; no remote setters.
- Native arena 3700/3700, analysis 56/56, Coach 53/53, motion 14741/14741, geometry 172/172, vitals 111/111, POV presentation 550/550 PASS, process EXIT=0; held source freeze 12/12 PASS. Reports in `evidence/wave1/arena-surround`, prior suite evidence retained separately.
- Initial negative evidence retained: `native-initial-failure.log` exposed a Unity-forbidden `MaterialPropertyBlock` constructor in a MonoBehaviour field initializer; fixed by creating it in explicit initialization. `native-editmode-failure.log` / `arena-tests-editmode-failure.txt` then exposed a test lifecycle assumption: a non-ExecuteAlways edit-mode component does not automatically run the disable callback. The unit test now explicitly invokes that callback, labelled as such; compiled real-UI transition tests must independently prove real gameplay resets. No runtime ExecuteAlways workaround or relaxed intensity/count bound.
- D3D11 isolated diagnostic render verified all four cardinal directions plus the 45-degree corner. Actual shader flash-on/off comparison is visible and localized. These cameras/synthetic visual pulses exist only in editor diagnostics, not phone/input evidence. Only scenery layer is rendered; black space above/below it is not a complete-game screenshot.
- Visual adjudication: **coverage PASS, production fidelity PARTIAL**. Reused 2D texture repeats and its corner join remains visible; this is not animated 3D crowd, seamless panoramic artwork or reference-fidelity PASS. No new imagery/model refinement generated while appearance work is held. Full game and phone visual acceptance remain separate.
- WebGL build, compiled runtime regressions and new public deployment are not established by native reports; append exact receipts only after completion. Current public release remains the bell repair until a new deployment receipt exists.
