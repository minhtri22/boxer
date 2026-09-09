# 16 — GPT-6 Astra Autonomous Execution Prompt

Use after the short bootstrap/read prompt.

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
Continue Boxer autonomously one evidence-backed roadmap milestone at a time. Continue to the next unlocked milestone unless a hard human/product/research/environment gate is reached.

VISUAL AUTHORITY
Approved reference images are BINDING UAT VISUAL AUTHORITY, not moodboards. Combat UAT uses `03-pov-combat-hud.jpg`; future production opponent uses `06-opponent-ramirez-turnaround.jpg`. Other references lock future surfaces but do not unlock them ahead of roadmap.

MECHANICS OVERRIDE ART WHEN NECESSARY
Never change verified control semantics, hit geometry, timing, range or causal mechanics merely to copy a reference. Adapt presentation around verified mechanics.

AUTONOMOUS LOOP
READ STATE → SELECT NEXT UNLOCKED ITEM → STATE HYPOTHESIS → FREEZE SCOPE/GATE → IMPLEMENT MINIMUM CHANGE → TEST → REGRESSION → FIX IN SCOPE → SOURCE COMMIT → CLEAN BUILD → PROVENANCE CHECK → SMOKE → ARTIFACT/EVIDENCE COMMIT → UPDATE HANDOFF → CONTINUE.

REFERENCE MATCH CHECK REQUIRED FOR EVERY UAT BUILD:
- POV framing PASS/PARTIAL/FAIL
- top HUD hierarchy PASS/PARTIAL/FAIL
- bottom control hierarchy PASS/PARTIAL/FAIL
- player glove presentation PASS/PARTIAL/FAIL
- opponent readability PASS/PARTIAL/FAIL
- black/gold/red language PASS/PARTIAL/FAIL
- premium championship atmosphere PASS/PARTIAL/FAIL
- mobile text readability PASS/PARTIAL/FAIL
- debug intrusiveness PASS/PARTIAL/FAIL
Explain every PARTIAL/FAIL and smallest next visual step.

HARD RULES
PROVE ONLY WHAT IS STILL UNCERTAIN → DECIDE → IMPLEMENT.
Synthetic PASS != Human PASS. Never change authoritative combat geometry to fix presentation. Never modify `docs/handoff/reference-ui/*` without explicit human approval. Do not unlock career/replay/player legs/A3.2/A3.3 early. Do not build dirty source. Do not rebase after provenance lock. Never force push.

HARD STOPS
real-device UAT; meaningful product choice; ambiguous evidence; unavailable required environment; destructive/external action outside established workflow.

WHEN STOPPING RETURN:
STOP_REASON:
CURRENT_SHA:
COMPLETED:
EVIDENCE:
REFERENCE_MATCH_CHECK:
QUESTION_FOR_HUMAN:
OPTIONS:
RECOMMENDATION:

BEGIN NOW: read handoff state, inspect approved references, select next unlocked milestone, preflight, continue until hard stop.
```
