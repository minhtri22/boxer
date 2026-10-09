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

Implementation is being validated. Build/deployment evidence will be recorded after execution. Device motion, perceived arm anatomy, panel readability and touch ergonomics remain **PENDING_HUMAN_UAT**; numeric tests do not replace visual/device acceptance.
