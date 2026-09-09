# 16 — GPT-6 Astra Autonomous Execution Prompt

Use this after the short bootstrap/read prompt.

```text
BOXER AUTONOMOUS EXECUTION MISSION — VISUAL REFERENCE LOCKED

Repository: minhtri22/boxer
Branch: p1/whole-body-mechanics

READ FIRST:
1. docs/handoff/README-HANDOFF.md
2. docs/handoff/11-current-state.md
3. docs/handoff/14-agent-working-protocol.md
4. docs/handoff/08-visual-ux-direction.md
5. docs/handoff/reference-ui/README-VISUAL-REFERENCE.md
6. docs/handoff/reference-ui/ui-reference-notes.md
7. docs/handoff/reference-ui/visual-lock.yaml
8. inspect all images in docs/handoff/reference-ui/

MISSION
Continue developing Boxer autonomously from the current verified state, one evidence-backed roadmap milestone at a time. Do not stop merely because one subtask is complete. Continue to the next unlocked milestone unless a hard stop requires human UAT, a product decision, unavailable hardware/credentials, or ambiguous research evidence.

SOURCE-OF-TRUTH PRIORITY
1. docs/handoff/11-current-state.md
2. docs/handoff/state.yaml
3. docs/handoff/roadmap.yaml
4. docs/handoff/14-agent-working-protocol.md
5. docs/handoff/reference-ui/visual-lock.yaml
6. docs/handoff/reference-ui/ui-reference-notes.md
7. docs/handoff/08-visual-ux-direction.md
8. task-relevant handoff docs
9. source code
10. historical evidence

VISUAL AUTHORITY
The approved reference images are BINDING UAT VISUAL AUTHORITY, not optional moodboards. For combat UAT, `03-pov-combat-hud.jpg` is the primary visual target. For future production opponent art, `06-opponent-ramirez-turnaround.jpg` is the character authority. Other references lock future surfaces but DO NOT unlock them ahead of roadmap.

VERIFIED MECHANICS OVERRIDE ART WHEN NECESSARY
Never change verified control semantics, hit geometry, timing, range, or causal mechanics merely to copy a reference. Adapt presentation around verified mechanics.

AUTONOMOUS LOOP
1. Read current state.
2. Select next unlocked roadmap item.
3. State uncertainty/hypothesis.
4. Freeze scope and acceptance gate.
5. Implement minimum change.
6. Run deterministic tests.
7. Run regression tests.
8. Diagnose and repair only within frozen scope.
9. Commit source.
10. Build from clean committed source.
11. Verify source SHA = build marker = productVersion = metadata build_commit.
12. Run local smoke.
13. Commit evidence/artifact.
14. Update handoff state/evidence/roadmap.
15. Continue automatically unless a hard stop is reached.

CURRENT PRODUCT DIRECTION
Continue the verified whole-body boxing mechanics roadmap. Near-term themes may include hip/torso rotation, weight transfer, whole-body punch coupling and recovery. Use roadmap.yaml as authority; do not assume the exact sequence.

UAT VISUAL CONVERGENCE
Whenever a build is intended for human UAT, also improve the visual shell toward the approved reference where safe without destabilizing the current mechanics milestone.

REFERENCE MATCH CHECK — REQUIRED IN EVERY UAT REPORT
- POV framing: PASS/PARTIAL/FAIL
- top HUD hierarchy: PASS/PARTIAL/FAIL
- bottom control hierarchy: PASS/PARTIAL/FAIL
- player glove presentation: PASS/PARTIAL/FAIL
- opponent readability: PASS/PARTIAL/FAIL
- black/gold/red visual language: PASS/PARTIAL/FAIL
- premium championship atmosphere: PASS/PARTIAL/FAIL
- mobile text readability: PASS/PARTIAL/FAIL
- debug intrusiveness: PASS/PARTIAL/FAIL
Explain every PARTIAL or FAIL and identify the smallest next visual step.

HARD RULES
PROVE ONLY WHAT IS STILL UNCERTAIN → DECIDE → IMPLEMENT.
Do not reopen human-passed milestones without regression evidence.
Do not redesign the frozen control thesis.
Do not change authoritative combat geometry to fix presentation.
Do not start career/progression/replay/KO/full player legs before roadmap unlock.
Do not start A3.2/A3.3 before prerequisites.
Synthetic PASS != Human PASS.
Do not build dirty source.
Do not rebase after provenance lock.
Never force push.
Never modify docs/handoff/reference-ui/* without explicit human approval.

HARD STOP CONDITIONS
A. real-device human UAT required;
B. meaningful product choice between valid alternatives;
C. research evidence ambiguous;
D. required environment/hardware unavailable;
E. destructive/external action outside established workflow.

WHEN STOPPING RETURN EXACTLY
STOP_REASON:
CURRENT_SHA:
COMPLETED:
EVIDENCE:
REFERENCE_MATCH_CHECK:
QUESTION_FOR_HUMAN:
OPTIONS:
RECOMMENDATION:

STATE MANAGEMENT
After each accepted milestone update:
- docs/handoff/11-current-state.md
- docs/handoff/state.yaml
- docs/handoff/roadmap.yaml
- docs/handoff/12-test-evidence-matrix.md
- docs/handoff/evidence-index.json
Do not change visual reference authority files without human approval.

FINAL COMPLETION
Do not declare the game finished yourself. Only declare ROADMAP_EXECUTION_COMPLETE_PENDING_PRODUCT_REVIEW after all currently defined milestones are evidence-backed and human-validated where required.

BEGIN NOW:
1. read handoff state;
2. inspect approved visual references;
3. select next unlocked milestone;
4. perform preflight;
5. continue autonomously until a hard stop.
```
