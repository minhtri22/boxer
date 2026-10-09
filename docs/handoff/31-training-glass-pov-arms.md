# Training glass and POV arms

## Scope

User request: training instructions must reveal the fighter behind the panel, with a blurred background; player forearms and elbows must be readable in practice and fights.

- Live, card-local glass: capture the current scene once per training frame, downsample, separable five-tap blur, sample matching screen UVs beneath the existing instruction card and head cue. Dark tint is 34%, previously 94%. The scene outside the cards stays sharp. Existing button locations and practice completion requirements are preserved.
- Buffers are capped at 256 x 512, have no depth, and are released when training ends. There is no training postprocess in Fight/Home/Intro/Result.
- Continuous skin mesh for each upper arm, rounded elbow and tapered forearm, ending under the existing glove cuff. Imported short `POVForearm` renderers are hidden, not duplicated.
- A presentation-only two-bone solution uses the shared shoulder and glove endpoints with an inward elbow pole. Its elbow is deliberately not the flared authoritative rig elbow. It writes only its own mesh and imported display bones. Combat anchors, colliders, camera/FOV, punch timing, reach, damage, HP/Stamina and contact sampling are unchanged.
- Existing accepted Ramirez art and owner ring intro/audio are untouched. This is a deterministic game/presentation model, not a physiology claim.

## Validation contract

Dedicated evidence: `evidence/wave1/training-pov`; previous release evidence is not overwritten.

- Existing vitals/flow: 111 checks.
- Existing geometry/fairness: 172 checks.
- Existing controller/KO/rematch: 118 checks (injected consequence harness, not physical input).
- New presentation invariants: 550 checks, sampled both arms across all eight punches and guard, three phases, five times. Finite mesh, bounded triangle count, continuous ring edges, immutable shoulder/wrist/root inputs, no colliders, portrait guard elbow projection, required shaders.
- Compiled browser UI: all original 28 checks plus five arms/glass/lifecycle/resize checks. Native read-only snapshot, real browser input, portrait and landscape screenshots.
- Existing real browser contact suite: all 18 checks, including ring audio at KO.
- Existing real decoded WebGL media lifecycle: all 20 checks.
- Public payload hash/version verification only after release gates pass.

## Acceptance

First compiled candidate `af62e84` is not deployed. UI suite passed 33 checks; visual inspection required a circular elbow fillet to avoid inner-bend surface overlap. Two browser audit races are preserved under `first-candidate`: contact re-read its baseline after the actual BLOCK had already occurred (HP remained 100); media sampled 2.2 seconds after the Intro UI without waiting for playback to start. The corrected contact test retains the exact matched Commit snapshot and additionally requires the same attack count; the media test waits for the current epoch's actual playing event before its unchanged mid-intro/five-second checks. No combat/media game rules or thresholds were relaxed.

Compiled source: `ef89d8cc0933fc455425af030f956b893e5e5bb1`; Unity 6000.5.8f1, WebGL build succeeded in 224.4709732 seconds, 62,077,628 bytes.

Final native gates: vitals 111/111, fairness 172/172, presentation 550/550, controller 118/118. Native controller log also contains an unrelated UnityEditor.Search startup indexing exception; do not describe that whole Editor log as error-free.

Final compiled browser gates: UI 33/33, real decoded media 20/20, physical input/contact 18/18, each with zero recorded JS/load/HTTP errors and the same compiled product version. Portrait and landscape images were inspected for continuous forearms/elbows and correctly registered card-local blur.

A full contact run while another browser audit was active ended on points with two player HITs and failed the win/KO checks. This genuine negative outcome is retained in `combat-browser-concurrent-failure`. The identical compiled game and unchanged contact tactic were then run separately: nine HITs, PLAYER_WIN / KO at 25.943999893 seconds, player HP 100, opponent HP 0. The failed trace frequently overshot the intended pocket before releasing; the successful run establishes that the strict suite can pass, not that timing-dependent input is universally reliable. No combat parameters, seeds, thresholds, scores or outcomes were changed between runs.

Status: **IMPLEMENTATION_PASS_PENDING_HUMAN_UAT**. Deployment receipt will record public verification separately. Device motion, perceived arm anatomy, panel readability, performance and touch ergonomics remain **PENDING_HUMAN_UAT**; numeric tests do not replace visual/device acceptance.
