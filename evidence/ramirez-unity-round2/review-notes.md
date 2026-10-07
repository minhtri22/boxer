# Internal review of accepted Ramirez integration

Date: 2026-10-07. This is internal engineering/visual review, not Human UAT.

Final source locked for build: 4940f8061eaf1ed58bcf5356a1bce4817de6eab0.

The accepted workbench is shown in the final runtime captures with one continuous
skin and its own equipment. Studio aids and all old opponent/player renderers
are hidden. The existing imported player POV presentation remains unchanged;
its foreground gold pieces are equipment geometry, not a newly created projectile.
Materials are simple skin/red/gold/white presentation over accepted geometry.
Facial likeness, hair and final skin texture research were not restarted.

## Final mapping measurements

Final runtime: 1873 frames, 45 steps, 9 contacts, 34 seconds.
Native arm lengths: upper 0.2962449m, forearm 0.239553675m.
Max native bone-length error: 0.00000110268593m.
Glove-center to authoritative glove error: 0.0000008221337m.
Foot mapping error: 0.000002092327m.
Authoritative planted error: 0.000000175840015m.
Committed root drift: 0m, committed rotation drift: 0 degrees.
Adapter measured managed allocations: 0 bytes.
Adapter mean: 0.0272903759ms; max: 1.4469ms. These are Editor measurements;
the maximum is retained without attributing it to a cause we did not measure.

Initial runs are retained in runtime-initial.txt and runtime-clavicle.txt. Their
mapping errors are not the final implementation's results. Minimal clavicle
protraction solved wrist reach without stretching anatomy, and a native pelvis
crouch from planted feet solved the leg-length mismatch. No combat anchor or
controller was changed. runtime-native-pelvis.txt and captures-native-pelvis/
retain source-66f7a41 measurements; the final mapping additionally reads planted
foot rotation directly from Round2Footwork's shoe anchor. runtime-boot-rotation.txt
and captures-boot-rotation/ retain that intermediate check. Final source makes
allocation instrumentation Editor-only after WebGL proved that GC API unsupported.
WebGL allocation availability is -1, not a claimed zero; archived e81 diagnostics
are in source-e81ab436-diagnostics.zip and the full old artifact is retained at
D:/WORK/RESEARCH/POVGame/_ramirez-unity-source-e81ab436.

## Proven contradiction and release blockers

Final skin raycasts at long range 1.4m give head front-surface gap 0.07803786m
and abdomen gap 0.09883225m relative to authoritative contact surfaces. At boxing
range 0.9947959m they are 0.07933274m and 0.0998076349m; at close range
0.7213747m they are 0.02180524m and 0.09793973m. Poses differ across these
samples: this is a diagnostic front-ray discrepancy, not a complete skin-contact
oracle or a measured count of false hits. Close-range samples are in runtime.txt;
their target-surface discrepancy remains. A missing ray is not interpreted as
zero gap. Thus a volume hit
does not establish contact with the accepted skin. Exact glove-center alignment
does not solve this target-volume contradiction. The old volumes are not native
anatomical surfaces; they must not be described as coherent with this mesh.

The imported fighter has 31 skinned renderers, 231937 exported vertices and
459352 triangles. EVEvaluation.ShellTests fails its unchanged <=30000 triangle
budget. It aborts there, so its later legacy-bone assertions were not evaluated.
The new native-rig audit independently validates all new required bones, unit
scale, bind poses and skinned attachments. No historical test or threshold was
rewritten to accept the new art.

Changing body/head contact envelopes would change hit boundaries and needs an
explicit product decision against handoff 25's frozen combat restriction.
Reducing export topology requires a separate export optimization with visual
and deformation validation; this run preserves the accepted evaluated geometry.

## Regression counts

Actual PASS rows in individual P0/P1 suites total 132; P1-V adds 8; Round2 adds
43, for 183 individual deterministic checks. The unchanged legacy combined
report hardcodes 131/131 and labels P0 22/22, although P0 actually contains 23
PASS rows. This reporting discrepancy is disclosed rather than silently fixed.
Experiments and contact matrix also completed via Round2SelfTests.RunAll.
Historical evidence files were restored byte-for-byte; this run's outputs are
in regressions/. Blender3DAssetAudit.Run and the accepted native import audit pass.

## Scope limits

Eight opponent punch intents progressed through commit/extend/recover in live
Editor play mode. Guard, forward/back/lateral external player fixtures and AI
reposition were exercised. A complete AI slip/roll system is not in this round;
guard topology, independent torso/head transforms and recovery provide the M08
foundation, but this does not claim live full slip/roll behavior.
No Pages deployment, device interaction, Human UAT, PvP or other deferred system
was performed. The final release verdict remains IMPLEMENTATION_BLOCKED.

Reference comparison against 03-pov-combat-hud.jpg does not establish complete
composition fidelity: the full-body candidate appears smaller and simpler than
the approved close POV reference. Arena and player assets are retained, not
newly approved. Browser HUD and frame sampling are recorded separately in the
final report; camera-only Editor captures do not include IMGUI HUD.
