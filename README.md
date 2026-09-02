# Boxer

**POV Boxing Career Simulator — proof first, build second.**

Boxer is a mobile boxing game concept built around one core fantasy:

> **The player does not control a boxer from outside. The player is the boxer.**

The entire experience is intended to be presented from the fighter's first-person point of view: fighting, moving, training, recovering, interacting with coaches, entering arenas, managing injuries, and progressing from unknown fighter to world champion.

## Core Design Thesis

The game's defining control hypothesis is an embodied mapping between the player's physical actions and the boxer:

| Player input | Boxer action |
| --- | --- |
| Left thumb gesture | Footwork / lower body movement |
| Right thumb gesture | Punching / upper body attack intent |
| Device physical motion | Head movement / evasive movement |
| No active input | Return to defensive guard |

Internal shorthand:

> **Phone = Head · Left Thumb = Feet · Right Thumb = Fists**

This mapping is a hypothesis, not yet a proven design.

## Project Principle

This repository follows a strict **PROVE → DECIDE → IMPLEMENT** workflow.

We do not build major game systems because they sound attractive. Every high-risk assumption must first be isolated, simulated or prototyped, measured, and passed through explicit acceptance criteria.

The first question is therefore **not**:

> How do we build the full boxing game?

It is:

> Can a mobile player, in first-person POV, read an opponent, control distance, evade punches by physically moving the phone, counterattack with touch gestures, and feel that they are boxing rather than operating a gesture interface?

Until that is proven, character creation, career simulation, equipment, coaches, hospitals, economy, arenas, multiplayer, and reusable engine work remain deferred.

## Development & Test Environment

The current project environment is fixed for Phase 0 unless a later documented decision changes it:

- **Primary development OS:** Windows 11 Home Single Language, 64-bit
- **CPU:** Intel Core Ultra 7 258V
- **RAM:** 32 GB
- **GPU:** Intel Arc 140V
- **Game development stack:** **Unity + C#**
- **Primary real-device test target:** **iPhone 12**

Codex and other implementation agents must read [`docs/foundation/development-environment.md`](docs/foundation/development-environment.md) before choosing implementation or test strategy.

Important constraint: the project develops primarily on Windows, while native iOS signing/deployment requires an available macOS/Xcode path. If such a path is unavailable during a task, Unity-side implementation and synthetic/editor tests should proceed, but iPhone-only proofs must remain explicitly blocked rather than being inferred or fabricated.

## Engine Strategy

The long-term architectural direction is a reusable **Embodied POV Engine** that may later support other first-person mobile game domains such as racing, fighter aircraft, or sword combat.

However, the engine is explicitly **LOCKED during Phase 0**.

The required order is:

```text
prove embodied POV interaction in boxing
→ Phase 0 PASS
→ extract/build reusable engine primitives
→ harden the engine with Boxer as the first reference domain
→ prove reuse with a second-domain micro-prototype
```

Boxing is therefore the **first proof domain and first engine reference domain**.

Do not generalize experimental Phase 0 code into a universal framework before the control thesis passes.

Architecture rule:

> **No abstraction without evidence of reuse.**

See [`docs/foundation/engine-strategy.md`](docs/foundation/engine-strategy.md).

## Product Vision

If the core interaction is proven, the long-term boxing experience may include:

- Fighter creation by nationality, appearance, height, weight, reach, stance, and weight class.
- Progression from underground/street fights through amateur, regional, national, international, and championship competition.
- Multiple fight environments such as street venues, cages, local boxing halls, professional arenas, and championship stadiums.
- Training from first-person POV: heavy bag, mitt work, conditioning, running, strength work, and sparring.
- Coaches with different boxing philosophies and tactical specializations.
- Equipment, clothing, gloves, shoes, nutrition, recovery, and training resources.
- Injuries with meaningful consequences, treatment choices, recovery, and hospital sequences.
- Rankings, purses, expenses, contracts, rivalries, rematches, and career decisions.
- Eventually, PvP competition using player-developed fighters, subject to separate fairness and networking proofs.

None of these features are authorization to implement them now.

## Phase 0 — POV Embodied Control Proof

Phase 0 exists to validate the core interaction model before game production or engine production begins.

Current proof areas:

1. **Sensor feasibility** — can mobile orientation/motion data support responsive head movement?
2. **Signal separation** — can intentional device motion be distinguished from normal hand jitter and touch interaction?
3. **Touch coexistence** — can footwork, punches, and device motion operate simultaneously without control conflict?
4. **POV readability** — can players understand range, incoming attacks, ring position, and openings from first person?
5. **Defensive model** — can idle guard remain useful without becoming a free invulnerability state?
6. **Embodied boxing feel** — does the interaction become learned motor behavior rather than gesture memorization?
7. **Comfort** — can the game remain readable and comfortable without excessive motion sickness, fatigue, or screen instability?
8. **Feedback/energy** — do haptics and audio add enough value without unacceptable sensor interference, battery cost, or thermal cost?

See:

- [`docs/protocols/phase-0-pov-embodied-control-proof.md`](docs/protocols/phase-0-pov-embodied-control-proof.md)
- [`docs/protocols/phase-0-feedback-energy-proof.md`](docs/protocols/phase-0-feedback-energy-proof.md)
- [`docs/foundation/development-environment.md`](docs/foundation/development-environment.md)

## Phase 0 Gate

No production combat engine or reusable POV engine should be started until Phase 0 passes its hard criteria.

The only implementation allowed during Phase 0 is a **measurement harness / experimental simulator** whose purpose is to test hypotheses, capture data, and support a go/no-go decision.

A successful Phase 0 should demonstrate a player loop resembling:

```text
SEE
  ↓
READ
  ↓
MOVE / GUARD / EVADE
  ↓
CREATE OPENING
  ↓
COUNTER
  ↓
RESET
```

A failed interaction looks like:

```text
SWIPE → SWIPE → SWIPE → SWIPE → WIN
```

If the latter is the dominant behavior, the control model has failed regardless of graphics quality.

## Repository Structure

Current foundation:

```text
boxer/
├─ README.md
├─ plan.md
└─ docs/
   ├─ foundation/
   │  ├─ product-thesis.md
   │  ├─ engine-strategy.md
   │  └─ development-environment.md
   └─ protocols/
      ├─ phase-0-pov-embodied-control-proof.md
      └─ phase-0-feedback-energy-proof.md
```

A future `engine/` layer may be introduced only after Phase 0 PASS. Repository structure must follow proven architecture rather than lead it.

## Current Status

**Stage:** Phase 0 — foundation and proof design  
**Boxing domain:** Active proof vehicle  
**Development stack:** Unity + C# on Windows  
**Primary test device:** iPhone 12  
**Production game implementation:** Not authorized  
**Reusable Embodied POV Engine:** Locked until Phase 0 PASS  
**Primary risk:** Whether POV + touch + physical-device motion produces readable, comfortable, repeatable boxing behavior on a mobile device.
