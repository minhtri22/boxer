# Fighter Profile metadata slice - 2026-10-10

## Scope fixed before implementation

- Starting checkout `boxer-wave1`, branch `feature/boxer-product-loop-wave1`, HEAD `68a69266196978bc839cd633e911202ba799a785`; public compiled `dd9434bfb74287845f980cf72ff9363c0b201952`, artifact `6a604fe136af30c58fd75e2fec91e9ff111ccb91`.
- Owner continuation authorizes next handoff-38 slice: PROFILE only. Appearance customization and HP-KO-001 HOLD; settings/pause deferred; no free venue selection. Earned Street -> Cage -> Tournament remains planned, thresholds unapproved.
- Reference 01 inspected: premium black/gold card, name/nationality and strong confirmation hierarchy. Reuse live existing arena/POV scene, no new model/portrait or screenshot pretending to be a player preview. No hair, skin, beard, glove/trunk/wrap/style selectors, fake level or upgrade stats. Visual fidelity must be reported separately.
- Uncertainty: can an explicitly saved local identity survive WebGL reload, while draft/cancel/invalid/stale callbacks cannot alter saved metadata or combat state, and native phone text entry remains usable?
- Hypothesis: versioned pure identity model (stable local GUID, bounded normalized Unicode name and self-reported nationality), one local preference; explicit save/cancel; browser-native HTML inputs for temporary mobile WebGL channel, native IMGUI fallback for editor. Scoped keyboard capture while form is open, restored on exit. Browser form resize/scroll must support small portrait, landscape and visual-viewport keyboard changes without gameplay touches leaking to the canvas.
- Identity is local browser/device metadata, NOT an account, verified nationality, online ranking or new 3D appearance. Unsaved install remains BOXER / nationality unselected; never silently assign Vietnam or another country. Country suggestions are examples, not a complete verified country registry.
- Record linkage: show/link only actual existing last-completed-match review on this device, labelled as such. No invented cumulative W-L-D, historical migration/backfill, rewards, shop or Career unlocks. Profile rename does not claim old unassociated matches were played under that identity.
- Runtime edits limited to profile data/form/controller/bridge, product navigation, display name and read-only audit. Held combat sources, authoritative model prefix, 45s bout, actors/contact/AI/arena/reference files unchanged.
- Home adds PROFILE at former retired-training slot. Existing retired-slot assertion must honestly change to expected Profile navigation in the new suite, not preserve a false no-navigation claim or silently remove it. All other old assertions/counts retained; new Profile checks are additive, evidence in a separate `fighter-profile` suite. Prior arena-surround and other evidence frozen.
- Acceptance: strict Unicode/length/schema/GUID/invalid storage tests; draft/cancel/stable-ID/reload/route/stale-callback tests; actual trusted browser form entry/save/cancel/reload; empty and genuine last-match link; no menu gameplay/audio/flash; keyboard-focus restoration and ordinary Fight input regression; rendered 320/375/540 portrait and 960x540 landscape, simulated reduced visual viewport explicitly NOT phone keyboard proof. Full old native and compiled media/contact/Coach/POV/arena gates plus profile gate, committed-source WebGL build, exact public byte/version checks. Status stays pending physical-phone UAT.

## Primary platform research

Unity 6 documents browser text input/keyboard focus and disabling `WebGLInput.captureAllKeyboardInput` when HTML needs typing: [Input in Web](https://docs.unity3d.com/6000.0/Documentation/Manual/webgl-input.html), [captureAllKeyboardInput](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WebGLInput-captureAllKeyboardInput.html). This supports a browser-native text form for this WebGL UAT slice; it is not evidence that a particular iPhone keyboard or native iOS build passes.

## Delivery status

Implementation/build/deployment receipts to be appended after real verification. No Profile delivery claim from this plan alone. Next product slice after Profile is persistent match history + exactly-once reward/wallet; amounts require owner decision before that economy implementation.

## Native verification before first build

- Profile identity/Unicode/schema/route model: 69/69 PASS. Real editor controller lifecycle: 19/19 PASS, prior native preference restored byte-for-byte (or removed if absent); stale/malformed/closed/reopened/offscreen saves rejected; corrupt storage not silently repaired.
- Existing controller 139/139, arena 3700/3700, analysis 56/56, Coach 53/53, punch motion 14741/14741, combat geometry 172/172, vitals 111/111, presentation 550/550 PASS. Combat HOLD hash/prefix/duration gate 12/12 unchanged.
- Separate `fighter-profile` evidence routing preserves prior suites. New Coach compiled suite retains original non-practice assertion, adds Profile route + cancel assertions (54 expected); new Profile compiled suite 38 checks fixed before execution. Public release now includes the new form script (13 payload hashes expected).
- No physical keyboard/phone result inferred from editor checks. Appearance/reference actor remains unchanged and HOLD.
