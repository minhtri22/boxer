# Boxer — Embodied POV Engine Strategy

## 1. Decision

Boxer is the first domain used to prove an embodied first-person mobile interaction model.

The reusable **Embodied POV Engine is NOT authorized during Phase 0**.

The project sequence is deliberately:

> **PROVE THE INTERACTION IN BOXING → PASS PHASE 0 → EXTRACT/BUILD ENGINE → HARDEN WITH BOXING → PROVE REUSE IN A SECOND DOMAIN**

This ordering is mandatory unless evidence later justifies changing it.

---

## 2. Why Boxing Comes First

Boxing is the first proof domain because it stresses the most important embodied-control primitives at once:

- device motion as continuous body/head control,
- simultaneous touch and device-motion input,
- low-latency sensor-to-visual response,
- first-person spatial readability,
- geometry-based hit/miss interaction,
- impact feedback through haptics and audio,
- fatigue/comfort constraints,
- tactical timing and counter windows.

If these primitives cannot produce convincing boxing behavior, building a generic engine around them would only formalize an unproven abstraction.

Therefore Phase 0 remains a **boxing-specific proof vehicle**, not a universal-engine implementation phase.

---

## 3. Phase 0 Boundary

### Authorized

Only the Boxer **Sensor Combat Harness** and supporting proof instrumentation are authorized.

The harness may contain reusable-looking code where natural, but such code must remain experimental and must not be treated as a stable engine API.

### Forbidden

During Phase 0, do not spend project effort on:

- a generic game framework,
- plugin architecture,
- generalized domain scripting,
- reusable asset pipelines,
- universal vehicle/weapon abstractions,
- generic physics wrappers,
- editor tooling,
- cross-domain templates,
- premature SDK/API design.

A clean experiment is more valuable than an elegant generic architecture before the interaction is proven.

---

## 4. Engine Authorization Gate

The Embodied POV Engine becomes an authorized workstream **only after Phase 0 receives PASS**.

A CONDITIONAL PASS does not authorize general engine work. It authorizes only the follow-up proof necessary to resolve the outstanding Phase 0 issue.

A FAIL does not authorize engine extraction.

### Required prerequisite

Before engine work starts, Phase 0 must provide evidence that the core interaction is viable, including the central acceptance criteria defined in the Phase 0 protocols.

---

## 5. Post-P0 Engine Goal

After Phase 0 PASS, the project may begin extracting proven primitives into a reusable **Embodied POV Engine**.

The engine goal is not:

> Replace assets and magically obtain another game.

The more accurate goal is:

> Reuse proven embodied-POV primitives while each game supplies its own domain rules, physics, actions, content, assets, scenarios, and progression.

---

## 6. Candidate Reusable Engine Primitives

Only primitives proven or required by concrete domains should be promoted into the engine.

Candidate shared layers include:

### 6.1 Input

- touch-zone abstraction,
- gesture recognition,
- multi-touch coordination,
- device orientation,
- angular velocity,
- calibration,
- dead zones,
- filtering,
- sensor fusion where justified,
- device capability profiles.

### 6.2 POV embodiment

- first-person camera/body rig,
- bounded device-to-body mapping,
- orientation-to-pose mapping,
- recentering,
- comfort constraints,
- motion-to-visual latency instrumentation.

### 6.3 Interaction geometry

- trajectories,
- collision regions,
- hit/miss/block events,
- contact quality,
- timing windows,
- event timestamps.

The engine should know concepts such as `Trajectory`, `ImpactEvent`, or `PoseState` rather than boxing-specific concepts such as `Jab` or `Petrov`.

### 6.4 Feedback

- haptic event abstraction,
- intensity/profile selection,
- audio event bus,
- camera-response hooks,
- feedback presets,
- battery/thermal modes.

### 6.5 Instrumentation

- input logging,
- sensor logging,
- frame-time logging,
- event telemetry,
- replay/event traces,
- experiment configuration,
- evidence export.

Instrumentation is part of the engine strategy because future domains should continue to follow the same proof-first discipline.

---

## 7. Boxing as the First Engine Domain

After engine extraction begins, Boxer remains the **reference domain**.

Boxer-specific modules may include:

- footwork rules,
- punch taxonomy,
- guard rules,
- head/body damage,
- stamina,
- knockdowns,
- boxer AI,
- boxing career systems,
- boxing assets and arenas.

These must not leak into the generic engine unless a second concrete domain demonstrates the same abstraction is genuinely shared.

---

## 8. Abstraction Rule

The project adopts this rule:

> **No abstraction without evidence of reuse.**

Preferred promotion rule:

- needed only by Boxer → keep in Boxer,
- needed by Boxer and a second domain in materially similar form → candidate for engine,
- needed by several domains → stabilize as engine API.

Do not generalize merely because future reuse seems plausible.

---

## 9. Cross-Domain Reuse Proof

The engine must not be called reusable merely because Boxer runs on it.

After a minimal post-P0 engine exists and Boxer proves the first concrete use, a **second-domain micro-prototype** must test reuse.

The second domain should be intentionally small. It is an architectural experiment, not a second production game.

Candidate domains include:

- POV racing,
- fighter aircraft / dogfight,
- Japanese wooden-sword / kendo-style combat,
- European sword or wooden-sword duel.

The exact second domain is not frozen yet.

### Required question

> Can the same engine primitives support a materially different embodied POV interaction without rewriting the core architecture?

### Evidence to collect

- percentage and identity of engine modules reused unchanged,
- modules extended,
- modules rewritten,
- domain-specific code added,
- assumptions that leaked from boxing into engine code,
- control mapping differences,
- performance/latency differences,
- new abstractions actually justified by two concrete uses.

If major parts of the supposed engine must be rewritten for the second domain, the abstraction boundary is considered unproven and must be revised.

---

## 10. Example Domain Mapping

The engine should provide primitives, while each game defines mappings.

### Boxer

```text
Device motion → Head movement
Left touch → Footwork
Right touch → Punch intent
Impact event → Boxing haptic/audio profile
```

### Racing candidate

```text
Device motion → Steering / chassis attitude
Left touch → Brake
Right touch → Throttle
Impact event → Road/collision haptic/audio profile
```

### Fighter aircraft candidate

```text
Device motion → Aircraft pitch/roll attitude
Touch input → Throttle / weapon / targeting actions
Impact event → Airframe/weapon feedback
```

### Sword-combat candidate

```text
Device/body motion → Evasion / stance component
Touch gesture → Weapon attack/parry intent
Trajectory event → Blade/weapon contact
Impact event → Weapon/body feedback
```

These are examples only. They are not authorization to implement those games now.

---

## 11. Repository Direction After P0

A possible future structure after engine work is authorized:

```text
boxer/
├─ engine/
│  └─ embodied-pov primitives
├─ games/
│  └─ boxer/
├─ experiments/
├─ evidence/
└─ docs/
   ├─ foundation/
   ├─ engine/
   ├─ protocols/
   └─ result/
```

Do not perform repository restructuring merely to match this diagram before Phase 0 PASS.

---

## 12. Current Status

**Boxing domain:** ACTIVE proof vehicle  
**Phase 0:** ACTIVE  
**Embodied POV Engine:** LOCKED  
**Engine authorization condition:** Phase 0 PASS  
**Second-domain reuse proof:** LOCKED until a post-P0 engine and Boxing reference implementation exist

The immediate project task remains unchanged:

> **Prove embodied POV boxing first.**
