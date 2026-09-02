# P1 — Combat Event Timeline, Replay, Highlight, and Shareability Foundation

## Status

**DESIGN DOCUMENT — IMPLEMENTATION LOCKED UNTIL P0 PASS**

Replay is not a post-production feature to bolt on later. P1 must make combat reconstructable by design.

The same combat timeline should eventually support:

- post-fight replay,
- cinematic alternate cameras,
- slow motion,
- automatic highlights,
- fight statistics,
- coaching/analysis,
- anti-cheat/audit support for future PvP,
- shareable short clips.

---

# 1. Design Principle

Every meaningful combat outcome must leave enough authoritative evidence to explain and reproduce what happened.

Target architecture:

```text
PLAYER / AI INPUTS
        ↓
COMBAT SIMULATION
        ↓
AUTHORITATIVE EVENT STREAM
        +
PERIODIC STATE SNAPSHOTS
        +
VERSION / SEED METADATA
        ↓
┌───────────────┬──────────────┬──────────────┬────────────────┐
│ Live gameplay │ Replay       │ Analytics    │ Highlight      │
│ presentation  │ reconstruction│ / coaching   │ generation     │
└───────────────┴──────────────┴──────────────┴────────────────┘
```

---

# 2. Why Input-Only Replay Is Insufficient

Unity physics is more reproducible when driven with fixed timesteps, but perfect cross-platform or cross-version determinism must not be assumed.

A replay based only on:

```text
input at tick N
```

may diverge later because of:

- physics implementation changes,
- platform floating-point differences,
- different engine versions,
- script execution-order changes,
- animation changes,
- bug fixes or gameplay rebalancing.

Therefore Boxer should use a hybrid model:

```text
INPUT LOG
+ AUTHORITATIVE EVENTS
+ PERIODIC SNAPSHOTS
```

Unity references:

- https://docs.unity3d.com/ScriptReference/Physics.Simulate.html
- https://docs.unity3d.com/Manual/TimeFrameManagement.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/physics-optimization-cpu-manual-simulation.html

---

# 3. Timeline Tick Model

P1 should define one authoritative combat tick domain independent of visual rendering.

Conceptually:

```text
CombatTickId: monotonically increasing integer
FixedCombatDelta: frozen simulation step
```

All combat-relevant inputs/events must reference `CombatTickId`.

Rendering may interpolate between simulation states.

Do not key authoritative combat events only to rendered frame numbers.

---

# 4. Data Layers

## Layer A — Input Intent Log

Records what each controller requested.

Examples:

```text
HEAD_OFFSET_REQUEST
FOOTWORK_VECTOR
PUNCH_INTENT
GUARD_INTENT / AUTO_GUARD_STATE
```

For human phone input, raw high-rate sensor data does not necessarily need permanent storage for every production fight. P1 should define what compressed/processed input is sufficient for reconstruction and diagnostics.

P0/debug builds may retain richer raw traces.

---

## Layer B — Authoritative Combat Events

Records causal simulation decisions.

Candidate event taxonomy:

### Movement

```text
MOVE_START
MOVE_UPDATE_KEYFRAME
MOVE_END
STANCE_CHANGE
ANGLE_CHANGE
BOUNDARY_PRESSURE
```

### Head / defense

```text
HEAD_MOVE_START
HEAD_OFFSET_PEAK
HEAD_RETURN
GUARD_ENTER
GUARD_EXIT
BLOCK
MISS_BY_HEAD_MOVEMENT
MISS_BY_RANGE
MISS_BY_ANGLE
```

### Attack

```text
ATTACK_START
ATTACK_ACTIVE
ATTACK_CONTACT
ATTACK_MISS
ATTACK_END
COMBO_LINK
```

### Impact / state

```text
CLEAN_HIT
PARTIAL_HIT
BODY_HIT
HEAD_HIT
BALANCE_BREAK
GUARD_DISRUPTION
COUNTER_WINDOW_OPEN
COUNTER_WINDOW_CLOSE
COUNTER_HIT
KNOCKDOWN
GET_UP
ROUND_END
FIGHT_END
```

### Resource/state milestones

Avoid logging every floating-point value as an event if snapshots handle continuous state. Log meaningful threshold/state transitions such as:

```text
LOW_ACTION_CAPACITY
LOW_STAMINA
GUARD_COMPROMISED
RECOVERED
```

---

# 5. Event Schema Candidate

Conceptual schema:

```text
CombatEvent
- schema_version
- combat_version
- fight_id
- tick
- event_id
- actor_id
- target_id? 
- event_type
- action_id?
- position
- facing
- head_offset
- target_zone?
- contact_point?
- trajectory_id?
- outcome?
- impact_quality?
- balance_before/after?
- action_capacity_before/after?
- stamina_before/after?
- counter_parent_event_id?
- metadata
```

Do not serialize fields that are irrelevant to an event unless the storage format makes sparse records inexpensive.

---

# 6. State Snapshot Candidate

Store periodic authoritative state checkpoints, plus mandatory snapshots around critical events if useful.

Conceptual:

```text
CombatSnapshot
- schema_version
- combat_version
- fight_id
- tick
- RNG_state / deterministic seeds where applicable
- FighterAState
- FighterBState
- ring / round state
```

Each fighter state may include:

```text
root position
facing
stance
head offset
COM offset
active action + phase
pose/animation semantic state
balance
action capacity
long-term stamina
head condition
body condition
guard state
recovery
counter state
```

Snapshot frequency must be tuned against storage size and replay correction requirements.

---

# 7. Replay Modes

## Mode A — Authoritative Timeline Playback

Preferred for archival/share reliability.

Use events + snapshots to reconstruct semantic combat states and drive presentation.

Advantages:

- robust against gameplay-code changes,
- good for old fights,
- suitable for alternate cameras,
- outcome cannot silently change.

---

## Mode B — Re-Simulation Replay

Use recorded processed inputs + original version + seeds to rerun simulation.

Useful for:

- debugging,
- QA,
- deterministic regression tests,
- detailed analysis.

Must verify against authoritative event hashes/checkpoints.

Do not rely on this as the only long-term player replay method.

---

## Mode C — Hybrid Corrected Replay

Re-simulate between snapshots/events, then correct drift at checkpoint boundaries.

Candidate balance between smoothness/storage and archival stability.

---

# 8. Presentation Independence

Combat semantics must not depend on the original gameplay camera.

The same timeline should eventually allow:

- original player POV,
- opponent POV,
- ringside camera,
- broadcast side camera,
- corner camera,
- overhead tactical camera,
- cinematic close camera,
- slow-motion finishing camera.

This is a major reason to log semantic state and trajectories rather than record only screen video.

---

# 9. Character Reconstruction Requirement

For third-person replay, fighter presentation must be reconstructable from fight metadata.

Store/reference immutable fight-time identity data:

```text
fighter appearance/config ID
outfit/glove IDs
body dimensions relevant to replay
stance
arena/environment version
```

Do not rely on the player's current cosmetic configuration when replaying an old fight.

---

# 10. Highlight Detection

Highlight generation should be event-driven.

Candidate highlight score:

```text
HighlightScore =
OutcomeImportance
+ TechnicalQuality
+ Rarity
+ VisualClarity
+ NarrativeContext
```

This is a ranking architecture, not a frozen formula.

## Candidate signals

### Perfect slip-counter

- opponent cleanly misses due to head movement,
- minimum glove-to-head distance below threshold,
- counter lands within short timing window,
- counter impact quality high.

### Pull counter

- attack misses by depth/range,
- defender returns immediately,
- clean counter lands.

### Combination finish

- multiple linked attacks,
- high clean-hit ratio,
- final knockdown/fight-ending event.

### Comeback

- fighter condition/stamina/ring state was materially worse,
- later produces knockdown/win.

### Body-shot finish

- body-hit chain contributes directly to decisive state transition.

### Last-seconds finish

- decisive event near round/fight deadline.

---

# 11. Minimum Derived Metrics for Highlight Selection

P1 combat logging should make these computable:

```text
miss_distance
miss_direction
reaction_time
miss_to_counter_time
counter_impact_quality
combo_length
combo_clean_hits
impact_sequence
condition_delta
stamina_delta
balance_delta
boundary_pressure_duration
round_time_remaining
fight_state_before_event
```

---

# 12. Clip Reconstruction Pipeline

Future, not P1 implementation scope:

```text
Fight ends
→ identify highlight candidates
→ select best time window
→ reconstruct timeline window
→ choose cinematic camera recipe
→ apply time scaling / slow motion
→ render clean presentation
→ optional overlays
→ encode vertical/horizontal clip
→ Share Highlight
```

P1 must only ensure the underlying data model makes this possible.

---

# 13. Social Clip Targets

Candidate outputs after the feature is later unlocked:

```text
6–8 s instant highlight
10–15 s social clip
20–30 s round/fight mini-recap
```

Candidate aspect ratios:

```text
9:16 — TikTok / Reels / Shorts
16:9 — general video / landscape
1:1 — optional social card/clip
```

Do not build exporters during P1 unless separately authorized.

---

# 14. Cinematic Camera Metadata

Do not store a fixed camera path during the fight unless required.

Prefer storing combat facts that allow a camera director to generate a shot later:

```text
attacker position
receiver position
trajectory
impact point
impact direction
head/body reaction vector
ring location
nearby boundaries
highlight type
```

Then a replay director may select a recipe such as:

### `SLIP_COUNTER_RECIPE`

```text
0.0–0.5s establish ringside angle
0.5–1.3s track incoming punch
1.3–1.8s slow near-miss
1.8–2.5s rotate/reframe counter
2.5–3.0s impact emphasis
3.0+ reaction / result
```

Exact cinematic direction belongs to a later presentation phase.

---

# 15. Replay Versioning

Every fight record must identify at minimum:

```text
combat_model_version
schema_version
build_version
content/animation compatibility version
```

If the simulation changes, old fight data must not silently reinterpret itself under new balance rules.

Possible strategy:

- semantic authoritative playback remains compatible,
- exact re-simulation supported only for selected combat versions.

---

# 16. Integrity / Validation

Candidate regression checks:

1. replay fight outcome equals recorded outcome,
2. knockdown ticks match authoritative record,
3. HIT/MISS/BLOCK sequence matches,
4. critical fighter states match snapshot hashes/tolerances,
5. highlight window contains its required causal event chain,
6. replay is independent of visual frame rate,
7. rendering alternate camera does not alter simulation outcome.

---

# 17. Storage Philosophy

Avoid two extremes:

### Too little

Input only → fragile replay divergence.

### Too much

Full transform state every rendered frame → large, redundant recordings and poor semantic analysis.

Preferred:

> **semantic events + fixed-tick processed inputs where useful + periodic state snapshots + critical-event metadata**

P1 implementation work should benchmark actual storage size before freezing snapshot rate.

---

# 18. P1 Exit Requirement for Replay Foundation

Before P1 design can be considered complete, the model must be able to answer from recorded data:

> Why did this punch hit, miss, or get blocked?

and:

> Can we reconstruct enough fighter state to show that exchange later from a different camera?

If either answer is no, the event/state schema is insufficient.

---

# 19. Strategic Product Loop Enabled Later

```text
FIGHT
→ MEMORABLE MOMENT
→ AUTO-DETECTED HIGHLIGHT
→ CINEMATIC REPLAY
→ SHARE
→ FRIEND / AUDIENCE DISCOVERY
→ NEW / RETURNING PLAYER
→ FIGHT
```

This is a future product loop, but its technical possibility begins with P1 combat data architecture.
