# Boxer — Master Proof-First Project Plan

## 0. Operating Rule

Boxer follows one non-negotiable rule:

> **PROVE ONLY WHAT IS STILL UNCERTAIN → DECIDE → IMPLEMENT**

The project must not repeat proofs that are already well established by prior art, platform capability, existing games, or accepted engineering practice unless Boxer introduces a materially different risk.

A proof phase exists only for assumptions that can still invalidate the product.

---

# 1. Product Thesis

Boxer is a **first-person boxing career simulator** built around one embodied control model:

- **Phone = Head**
- **Left Thumb = Feet**
- **Right Thumb = Fists**
- **No active action = Return to Guard**

Target player loop:

```text
SEE → READ → MOVE / GUARD / EVADE → CREATE OPENING → COUNTER → RESET
```

Failure loop:

```text
SWIPE → SWIPE → SWIPE → SPAM → WIN
```

Reference documents:

- `README.md`
- `docs/foundation/product-thesis.md`
- `docs/foundation/engine-strategy.md`
- `docs/foundation/development-environment.md`
- `docs/protocols/phase-0-pov-embodied-control-proof.md`
- `docs/protocols/phase-0-feedback-energy-proof.md`

---

# 2. Development Environment

Current project environment:

- Windows 11 64-bit
- Intel Core Ultra 7 258V
- 32 GB RAM
- Intel Arc 140V
- Unity + C#
- Primary real-device target: iPhone 12

The reusable Embodied POV Engine remains **LOCKED until Phase 0 PASS**.

Phase 0 uses boxing as the first concrete proof domain.

---

# 3. Prior-Art Accepted — Do Not Re-Prove in P0

The following are considered sufficiently established to proceed directly to integration unless Boxer-specific evidence later shows a problem:

## 3.1 Mobile motion sensing exists and is usable

Accepted:

- modern smartphones expose orientation / attitude / gyroscope / accelerometer data,
- device motion can be sampled at interactive rates,
- relative orientation and calibration are standard engineering problems.

Do not spend Phase 0 proving that gyroscopes work.

## 3.2 Touch and device motion can coexist

Accepted:

- mobile games and HCI systems already combine touch input and physical device motion,
- multiple simultaneous input channels are technically feasible.

Do not run a standalone research program merely to prove touch + tilt coexistence.

## 3.3 Haptics are viable mobile feedback

Accepted:

- phones can produce event-based haptic feedback,
- varying haptic profiles can communicate different impact classes,
- users may disable haptics.

Battery/thermal cost is an engineering optimization question unless actual Boxer measurements reveal a material problem.

## 3.4 Audio feedback is viable

Accepted:

- impact audio, body-state audio and ambience are standard game feedback channels,
- audio may be optional,
- gameplay must not depend exclusively on audio.

Do not make audio feasibility a Phase 0 gate.

## 3.5 Geometry-based hit detection is established

Accepted:

- trajectories, colliders, hitboxes, guard regions and HIT/MISS/BLOCK outcomes are standard game mechanics.

Boxer should use them rather than trying to prove collision detection itself.

## 3.6 POV gameplay is established

Accepted:

- first-person gameplay is a proven presentation model,
- motion-controlled and touch-controlled first-person interaction already exists in commercial games and prototypes.

The open question is not whether POV works in general.

---

# 4. What Phase 0 Actually Needs to Prove

# Phase 0 — Boxer Interaction Integration Proof

## Status

**ACTIVE**

## Goal

Answer one product question:

> **Does Boxer’s specific combination of phone-as-head, left-thumb footwork, right-thumb punching and automatic return-to-guard create a convincing, readable and enjoyable boxing exchange in first-person POV?**

Phase 0 is therefore an **integration + game-feel proof**, not a sensor-research phase.

Only four uncertainties remain central.

---

## P0-A — Phone = Head

### Question

When the player physically moves/leans the phone to evade, does it feel like moving the boxer’s head rather than triggering a device gesture?

### Required behavior

- continuous head displacement,
- bounded movement,
- natural return toward center,
- actual punch trajectory can miss because head position moved,
- no separate dodge button required.

### Failure condition

The interaction feels like:

> tilt phone → trigger dodge animation

instead of:

> see punch → move head out of line

---

## P0-B — Feet + Head + Fists Integration

### Question

Can the player combine all three control channels naturally in one exchange?

Required target sequence:

```text
left-thumb retreat
+ phone/head slip
+ right-thumb counter
```

A PASS requires that the channels feel complementary rather than mutually disruptive.

The project does not need to prove that simultaneous inputs are technically possible; it must prove that **this specific Boxer mapping is usable**.

---

## P0-C — Boxing Read → Evade → Counter Loop

### Question

Does the player react to the opponent rather than memorize gestures?

The minimum desirable loop is:

```text
READ PUNCH
→ EVADE OR GUARD
→ CREATE OPENING
→ COUNTER
→ RESET
```

### Failure conditions

- swipe spam dominates,
- player watches controls instead of opponent,
- automatic guard makes inactivity optimal,
- dodge has no tactical counter value,
- opponent attacks are unreadable without intrusive warning arrows.

---

## P0-D — Immediate Control Comprehension & Agency

### Question

After a short bout, does the player understand that their own head, footwork and punch inputs caused the observed combat outcome?

Ask:

> “Bạn có cảm thấy chính thao tác đầu/chân/tay của mình tạo ra kết quả vừa xảy ra không?”

Then ask the player to explain one exchange: why the attack hit, missed, was blocked, or was countered. PASS requires a meaningful cause/effect explanation tied to the player's own inputs.

---

# 5. Phase 0 Authorized Artifact

Build one **Unity Boxer Micro-Prototype**.

This is no longer a generic sensor laboratory.

It should be just complete enough to create a real boxing exchange.

Minimum scope:

- first-person camera,
- two placeholder player gloves,
- one opponent,
- neutral simple ring/space,
- phone motion → head displacement,
- left-thumb footwork,
- right-thumb punch input,
- automatic high guard when inactive,
- basic jab/cross/hook or equivalent minimal attack set,
- opponent jab/cross/hook/body threat,
- head/body/guard collision,
- HIT / MISS / BLOCK,
- one meaningful counter window,
- basic stamina/recovery only if needed to prevent spam,
- lightweight haptic impact,
- basic impact audio,
- debug/logging sufficient to diagnose control issues.

Visual polish is not required.

---

# 6. Phase 0 Explicit Non-Goals

Do not build during Phase 0:

- reusable Embodied POV Engine,
- generic plugin/framework architecture,
- production characters,
- championship arena art,
- character creator,
- career mode,
- gym,
- coach system,
- inventory,
- shop,
- hospital,
- economy,
- progression,
- multiplayer,
- backend,
- monetization.

Phase 0 code may be clean, but must not be prematurely generalized into an engine.

Architecture rule:

> **No abstraction without evidence of reuse.**

---

# 7. Phase 0 Test Procedure

## Test 1 — First Contact

Give the player at most a very short explanation:

- left thumb moves,
- right thumb punches,
- move the phone to move the head,
- release input to return to guard.

Then begin a short bout.

Observe whether the player understands the mapping without persistent UI instruction.

---

## Test 2 — Slip → Counter

Opponent throws a readable straight attack.

Player should:

```text
see attack
→ move phone/head
→ punch misses
→ counter
→ return to guard
```

This is the primary Phase 0 interaction test.

---

## Test 3 — Move + Evade + Counter

Opponent applies pressure.

Player must combine:

```text
footwork
+ head movement
+ counter punch
```

Observe control conflict and cognitive load.

---

## Test 4 — Unscripted Short Bout

Run a short unscripted fight with mixed attacks.

Look for spontaneous emergence of:

```text
move → read → guard/evade → counter → reset
```

versus:

```text
spam punches → hope to win
```

---

## Test 5 — Control Comprehension & Agency

Immediately after the bout ask the P0-D agency question and ask the tester to explain one successful or failed exchange. Replay intent is not a Phase 0 acceptance criterion for the placeholder-visual prototype.

---

# 8. Phase 0 Acceptance Gate

Phase 0 should be evaluated primarily as a product integration gate, not a laboratory benchmark suite.

Minimum PASS criteria:

| Criterion | PASS |
| --- | --- |
| Phone movement clearly feels connected to head evasion | Yes |
| No mandatory separate dodge button needed | Yes |
| Player can combine feet + head + fists | Yes |
| Player can intentionally execute slip → counter | Yes |
| Incoming attacks are readable without intrusive directional UI | Yes |
| Swipe spam is not clearly dominant | Yes |
| Automatic guard does not make inactivity optimal | Yes |
| Control mapping is understood after short instruction | Yes |
| Player can explain the outcome of an exchange from their own controls | Yes |
| No severe physical discomfort in the short P0 session | Yes |

Quantitative telemetry may be collected for diagnosis, but Phase 0 should not be blocked by arbitrary laboratory thresholds for already-established technology.

If latency, false motion, haptic interference, battery or thermal behavior is visibly problematic in the actual prototype, it becomes a targeted engineering issue and must be measured then.

---

# 9. Phase 0 Decision

## PASS

The integrated Boxer control loop feels sufficiently convincing to justify further development.

Unlock:

1. **Embodied POV Engine architecture/extraction work**
2. **Boxer Combat Foundation**

These should begin as separate explicitly scoped tasks after Phase 0 PASS.

## CONDITIONAL PASS

The core loop works but one contained issue remains, for example:

- head-motion scaling,
- control conflict,
- attack readability,
- guard behavior,
- punch recovery,
- input latency.

Only fix and retest that issue.

## FAIL — REDESIGN

The player understands the concept but the mapping does not produce good boxing interaction.

Redesign the control model before adding systems.

## FAIL — STOP

The embodied POV interaction provides insufficient value relative to its usability cost.

Do not compensate by building career, graphics or content.

---

# 10. Post-P0 — Embodied POV Engine Foundation

## Status

**LOCKED UNTIL P0 PASS**

Once Boxer proves the integrated interaction, extract only the reusable primitives supported by evidence.

Candidate engine primitives:

- device-motion input abstraction,
- touch/gesture abstraction,
- configurable action mapping,
- POV camera/body rig,
- interaction trajectory/collision events,
- haptic feedback abstraction,
- audio event abstraction,
- device/performance profiles,
- telemetry hooks.

Do not make Boxer-specific concepts such as `Jab`, `Petrov`, `Ring`, or `WeightClass` part of the generic engine layer.

---

# 11. Boxer Combat Foundation

## Status

**LOCKED UNTIL P0 PASS**

Goal:

> Turn the proven interaction into a deep boxing system.

Proof/build areas:

- distance/range,
- jab/cross/hook/uppercut,
- body vs head attacks,
- guard geometry,
- stamina,
- recovery,
- balance,
- counter windows,
- knockdown,
- get-up interaction,
- round structure.

Core question:

> Can timing, range, defense and counters matter more than swipe frequency?

---

# 12. Opponent Intelligence & Boxing Styles

## Status

**LOCKED UNTIL COMBAT FOUNDATION IS SOUND**

Candidate styles:

- pressure fighter,
- out-boxer,
- counter puncher,
- power puncher,
- defensive specialist.

Goal:

> Different opponents must force different tactical decisions, not merely have larger stats.

---

# 13. Minimal Career Loop

## Status

**LOCKED UNTIL CORE COMBAT IS FUN**

Candidate loop:

```text
choose fight
→ prepare/train
→ fight
→ win/loss/injury
→ money/ranking
→ recovery
→ next fight
```

Candidate systems:

- fight selection,
- rankings,
- purses,
- training choices,
- recovery,
- basic injuries,
- first coach effects,
- first gym effects,
- limited equipment.

Every system must answer:

> How does this change the next fight or the meaning of the career?

---

# 14. Fighter Identity & World Progression

Candidate systems after career loop is proven:

- nationality,
- face/hair,
- height,
- weight,
- reach,
- stance,
- weight classes,
- outfits,
- walkouts,
- fight posters,
- arena ladder from street to championship.

POV-specific question:

> Does customization remain meaningful when the player rarely sees their own full body during combat?

---

# 15. Career Depth

Later candidate systems:

- coaches,
- gyms,
- equipment,
- nutrition,
- injuries,
- medical treatment/hospital,
- rivalries,
- rematches,
- sponsors,
- contracts,
- travel.

Avoid artificial waiting/paywall mechanics unless later evidence strongly supports them.

---

# 16. Cross-Domain Reuse Proof

The Embodied POV Engine may only be called genuinely reusable after a second domain proves reuse.

Candidate second-domain micro-prototype:

- racing,
- fighter aircraft,
- sword/kendo combat.

Do not build a full second game.

The goal is to determine whether proven primitives can be reused while replacing domain rules, assets and mappings.

---

# 17. Online PvP

**Late-stage proof only.**

Required before production PvP:

- latency envelope,
- hit validation,
- prediction/rollback strategy if needed,
- matchmaking fairness,
- stat normalization,
- anti-cheat assumptions,
- disconnect handling,
- pay-to-win prevention.

---

# 18. Commit / Evidence Discipline

For each meaningful proof or implementation gate:

1. Define scope.
2. Commit specification if needed.
3. Implement only authorized work.
4. Run tests.
5. Record evidence/results.
6. Decide PASS / CONDITIONAL PASS / FAIL.
7. Update `plan.md` only after the decision.

Do not mix speculative next-phase work into proof commits.

---

# 19. Current Project State

**Current phase:** Phase 0 — Boxer Interaction Integration Proof  
**Current stack:** Unity + C#  
**Primary development machine:** Windows / Core Ultra 7 258V / 32 GB / Arc 140V  
**Primary real-device target:** iPhone 12  
**Boxing domain:** Active proof vehicle  
**Reusable Embodied POV Engine:** LOCKED until P0 PASS  
**Production career/game systems:** BLOCKED  
**Authorized next work:** Unity Boxer micro-prototype sufficient to test phone=head + feet/head/fists integration + read/evade/counter + control comprehension/agency
**Primary gate:** Does the player understand that their own embodied controls caused the observed combat outcome?
