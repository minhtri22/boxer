# P1 — Master Combat Mechanics Matrix

## Status

**DESIGN DOCUMENT — IMPLEMENTATION LOCKED UNTIL P0 PASS**

This matrix is the central P1 design artifact. Every important combat action must trace from player input through biomechanics, geometry, combat state, energy, outcome, logging, replay and highlight value.

The goal is to avoid isolated subsystems such as "stamina", "damage", "dodge" and "replay" that do not share one causal model.

---

# 1. Core Resolution Pipeline

```text
INPUT
→ INTENT
→ BODY ACTION
→ BIOMECHANICAL STATE
→ POSITION / RANGE / ANGLE
→ TRAJECTORY / GUARD / TARGET INTERSECTION
→ OUTCOME
→ IMPACT / COST / RECOVERY
→ UPDATED COMBAT STATE
→ AUTHORITATIVE EVENT
→ REPLAY / ANALYTICS / HIGHLIGHT
```

---

# 2. Required State Domains

Every action may read or modify some subset of:

- foot position
- stance / lead side
- facing / angle
- head offset
- centre-of-mass offset
- guard geometry
- balance
- short-term action capacity
- long-term stamina
- active action
- action phase
- recovery state
- head condition
- body condition
- counter advantage
- ring pressure / boundary state

No variable should survive P1 if it has no observable tactical consequence.

---

# 3. Master Mechanics Matrix

| Input / Situation | Body action | Biomechanics abstraction | Spatial effect | Energy / balance effect | Offensive effect | Defensive effect | Resolution rule | Recovery / vulnerability | Event log requirement | Replay / highlight value |
|---|---|---|---|---|---|---|---|---|---|---|
| Left thumb forward | Step/advance | Rear-leg drive, COM translation, stance preserved | Distance closes | Low–moderate action cost; balance depends on cadence | Enables range entry and pressure | May reduce time to react while committed | Update fighter root position continuously | Short settling period if abrupt | `MOVE_START/UPDATE/END`, position, velocity, stance, balance | Useful before pressure sequence / finishing combination |
| Left thumb backward | Step/retreat | COM translates backward with foot support | Distance opens | Low–moderate cost; repeated retreat increases pressure disadvantage | Can create long-range counter setup | Avoids attacks by range rather than head movement | Root displacement determines whether trajectory reaches target | Brief directional commitment | Same as movement + boundary/ring-pressure events | Good for pull-counter / escape highlights |
| Left thumb lateral | Sidestep | Lateral COM displacement, hip/knee coordination | Changes angle and lateral alignment | Moderate cost; balance penalty if chained too fast | Creates new punching angle | Moves body/head trajectory laterally | Geometric position update; no dodge flag | Temporary balance/reorientation cost | position + facing + COM/balance | Strong tactical replay value |
| Phone tilt left/right | Slip/head movement | Head and upper-body lateral displacement with limited COM shift | Head hitbox shifts while root position mostly retained | Very low–low cost; repeated extremes may affect balance | Can create counter opening without losing range | Evades head trajectory geometrically | Punch trajectory misses if no intersection at impact | Small return-to-center / counter-ready window | sensor-derived head offset, peak offset, impact separation distance | Very high: "closest dodge", slip-counter |
| Phone backward (candidate) | Pull-back | Upper-body/head posterior displacement without full step | Head moves out of straight-punch reach; feet stay | Low–moderate balance cost | Preserves range for return counter if timed well | Evades by head depth | Geometry only; no invulnerability | Vulnerable to body/long follow-up if overused | head depth, COM, balance, miss distance | High: dramatic near-miss / pull counter |
| Phone forward (candidate) | Lean-in / inside head position | Forward upper-body shift | Reduces distance to opponent head/body | Moderate balance/exposure cost | Can improve inside-punch access | Potentially dangerous versus uppercut/hook | Geometry + target exposure | Requires recovery to neutral | forward offset + exposure | Situational highlight value |
| No active attack | High guard return | Arms return to defensive geometry | Guard volumes cover selected zones | Small or zero active cost; sustained guard may later have passive consequence | None | Blocks/intercepts covered trajectories | Guard collider checked before vulnerable target | Heavy blocks may produce guard recovery | guard state, contact point, absorbed impact | Medium: defensive sequences |
| Fast right-thumb straight | Jab | Rapid linear kinetic chain, lower commitment | Best at appropriate straight-punch range | Low cost, low balance disruption | Probe, interrupt, score, set range | Temporarily occupies one hand from guard | Hand trajectory vs target/guard | Short recovery | punch start/active/impact/end, trajectory, target zone | Low–medium alone; high when setting up combo |
| Committed rear straight | Cross | Rear-leg drive + trunk transfer + arm extension | Strong straight-line reach | Moderate cost, meaningful balance/commitment | High impact ceiling, counter finisher | Guard exposure during action | Geometry + range quality + kinetic-chain quality | Medium recovery; punishable miss | kinetic-chain quality, impact quality, whiff | High: counter cross, KO candidate |
| Hook input | Hook | Rotational trunk/shoulder action, circular fist path | Strong close/mid-range lateral path | Moderate–high action cost, rotational commitment | Bypasses some straight-line guard relationships | Creates larger whiff exposure | Curved/swept trajectory vs head/body/guard | Medium–high recovery on miss | angular trajectory, side, impact direction | Very high for clean head hit / finish |
| Uppercut input | Uppercut | Rising close-range chain | Short vertical/diagonal path | Moderate–high cost | Punishes lowered/forward head, inside range | Significant exposure if thrown from wrong range | Rising trajectory + range + target head/body position | High miss vulnerability | target posture + trajectory + impact | High when timed versus duck/lean-in |
| Punch while planted | Stable strike | Good support base / weight transfer | No root displacement required | Better balance/kinetic quality | Higher effective impact | Normal defense trade-off | Standard impact formula | Punch-specific | stance/balance snapshots | Normal |
| Punch during retreat | Moving strike | Reduced/altered force transfer depending technique | Distance increasing during active frames | Higher action cost / lower balance quality candidate | Lower impact ceiling but useful intercept | May maintain escape | Dynamic trajectory against moving bodies | Recovery overlaps movement | movement + strike overlap markers | Tactical highlight candidate |
| Punch during unstable recovery | Off-balance strike | Poor kinetic-chain efficiency | Variable | Significant efficiency penalty | Reduced impact/accuracy envelope | Exposes fighter | Geometry still authoritative; impact quality penalized | Extended recovery possible | balance before/after | Good analytics, rarely highlight |
| Clean head contact | Head impact | Linear + rotational components abstracted by direction/contact quality | Target head displaced/reacts | Receiver condition/balance affected | Damage/knockdown pressure | N/A | Contact already geometrically confirmed | Receiver reaction/recovery | target zone, direction, impact score, reaction | Very high for major impact |
| Clean body contact | Torso impact | Local body impact abstraction | Root may remain stable; breathing/body state affected | Long-term stamina/recovery pressure candidate | Accumulated body damage, interruption | N/A | Target zone + guard coverage | May reduce recovery temporarily | body zone, impact score, stamina consequence | High for dramatic body-shot finish |
| Guard contact | Block | Force intercepted by arms/gloves | Target hitbox protected | Defender action/guard capacity cost; possible balance effect | Attacker may retain pressure | Converts clean hit to reduced consequence | Guard intersection precedes target | Block stun/recovery proportional to impact candidate | absorbed impact, guard region | Medium; high for dramatic block-counter |
| Geometric miss | Miss | Punch travels without target/guard contact | Relative positions determine miss distance | Attacker still pays action cost | No damage | Defender may gain counter opportunity | No intersection during active trajectory | Whiff recovery applies | minimum separation, miss side, timing | Very high if near miss + counter |
| Successful slip + immediate counter | Counter sequence | Defender preserves range and exploits opponent recovery | Relative alignment favorable | Defender pays evade + attack costs; attacker in recovery | Counter modifier from timing/opening, not arbitrary bonus damage | Evade succeeded without root retreat | Counter window defined by opponent action/recovery timing | Both fighters' recovery states matter | causal link: evade event → counter event | Highest-priority highlight class |
| Successful step-back + counter | Pull/range counter | Whole body exits range then re-enters/attacks | Distance changes substantially | More movement cost than slip | Can punish overextension | Strong range defense | Attack miss due reach; counter resolves normally | Timing cost to regain range | miss-by-range + counter | High |
| Repeated punch spam | Local fatigue / commitment accumulation | Technique quality degrades through capacity/balance constraints | Usually stationary or pressure-moving | ActionCapacity drops quickly; recovery worsens | Lower effective impact and growing vulnerability | Guard return delayed/exposed | No artificial input lock unless necessary; state makes spam self-defeating | Increasing recovery/exposure | sequence index, capacity trend | Useful analytics; anti-spam evidence |
| Sustained movement | Locomotor fatigue | Repeated COM translation | Changes ring position | Long-term stamina gradual cost | Positioning benefit | Distance/angle defense | Standard movement geometry | Recovery while lower intensity | distance traveled, intensity | Tactical round recap |
| Heavy block streak | Guard attrition | Repeated upper-limb loading | Position may compress under pressure | Guard/action capacity and balance may deteriorate | Attacker gains pressure | Defender still avoids full damage | Block resolution repeated | Slower guard recovery candidate | cumulative absorbed impact | High for pressure sequence |
| Body-shot accumulation | Systemic fatigue pressure | Simplified physiological consequence | No direct geometry change after contact | Long-term stamina regeneration/capacity reduced | Strategic body attack value | Weakens future defense indirectly | Zone/impact-based consequence | Persistent but recoverable | cumulative body condition | High in comeback/body KO narrative |
| Boundary pressure | Ring-position constraint | Footwork options constrained | Retreat/lateral choices reduced | Psychological state not simulated unless justified | Pressure fighter gains tactical options | Defender has fewer spatial exits | Root position vs ring bounds | Reorientation cost | boundary proximity, pressure duration | Strong round-storytelling value |
| Knockdown threshold reached | Loss of stable posture | Aggregate impact/balance/condition event | Fighter transitions to knockdown state | Major state reset/cost | Ends exchange | N/A | Threshold/event policy, not pure single-force medical prediction | Get-up logic later | cause chain + last-impact metadata | Maximum highlight value |

---

# 4. Candidate Punch Quality Factors

P1 should calculate a normalized `ImpactQuality` from several independent factors rather than use fixed damage alone.

Candidate structure:

```text
ImpactQuality =
BaseTechnique
× FighterPowerCapability
× RangeQuality
× KineticChainQuality
× BalanceQuality
× StaminaQuality
× ContactQuality
× TargetExposure
```

Each factor should normally be bounded (for example 0..1 or a narrow modifier range) so tuning remains understandable.

Important:

- `RangeQuality` should come from actual geometry.
- `BalanceQuality` should reflect current stance/support state.
- `KineticChainQuality` should reflect whether the punch was thrown from a mechanically coherent state.
- `StaminaQuality` should reduce output progressively rather than hard-disable all action.
- `TargetExposure` distinguishes clean contact from partially guarded/off-angle contact.

Do not treat this formula as scientific force prediction.

---

# 5. Candidate Defense Resolution Order

At each punch active interval:

```text
1. Generate / update punch trajectory.
2. Evaluate attacker and target movement continuously.
3. Check guard intersection.
4. Check vulnerable target-zone intersection.
5. If no intersection, classify MISS.
6. Calculate miss distance and side.
7. Resolve consequences and possible counter window.
```

Possible resolution types:

```text
CLEAN_HIT
PARTIAL_HIT
BLOCK
MISS_HEAD_MOVEMENT
MISS_RANGE
MISS_ANGLE
WHIFF_OTHER
```

This richer taxonomy is valuable for replay/highlight detection.

---

# 6. Energy and Recovery Matrix

P1 should tune three distinct resources/states:

| State | Timescale | Meaning | Typical consumers | Recovery |
|---|---|---|---|---|
| `ActionCapacity` | sub-second to seconds | Immediate explosive readiness | punches, rapid slips, burst footwork | fast when not committing |
| `LongTermStamina` | tens of seconds to rounds | Accumulated endurance | sustained movement, combinations, impacts | slow in-round, stronger between rounds |
| `Balance` | fractions of seconds to seconds | Mechanical readiness/support quality | committed punch, abrupt direction change, missed power shot | quick if stance stable |

This structure should make spam self-defeating without feeling like an arbitrary cooldown system.

---

# 7. Anatomy / Target Matrix

Keep zones minimal and gameplay-readable.

| Zone | Main gameplay consequence candidates | Guard relationship | Replay metadata |
|---|---|---|---|
| Front head/face | head condition, reaction, accuracy feedback | high guard strong | impact direction, head snap/reaction |
| Left jaw/side head | balance/knockdown pressure candidate | side guard coverage | rotational reaction direction |
| Right jaw/side head | same mirrored | side guard coverage | rotational reaction direction |
| Chin/jawline | high knockdown-pressure candidate on clean impact | guard-dependent | strong highlight weighting |
| Upper torso | condition / interruption | elbows/forearms may cover | torso reaction |
| Left body | stamina/body-condition pressure | elbow coverage | side/body reaction |
| Right body | stamina/body-condition pressure | elbow coverage | side/body reaction |
| Central body | breathing/recovery disruption abstraction | compact guard coverage | body-fold reaction candidate |

No gameplay parameter should claim medical diagnostic validity.

---

# 8. Counter Window Model

Counter advantage should emerge from opponent commitment and timing.

Candidate definition:

```text
CounterWindow opens when:
- defender successfully avoids/blocks an attack,
- attacker remains in recovery / poor alignment,
- defender retains sufficient balance/action capacity,
- defender is still within viable counter range.
```

Counter advantage may modify:

- opponent defensive readiness,
- target exposure,
- timing quality,
- reaction opportunity.

It should not simply mean `damage × 2`.

---

# 9. Highlight-Relevant Derived Metrics

The combat model should calculate or make derivable:

- minimum glove-to-head miss distance,
- time from opponent miss/block to counter impact,
- counter impact quality,
- combo length and clean-hit ratio,
- comeback state delta,
- remaining stamina/condition at finish,
- knockdown cause chain,
- boundary pressure before finish,
- guard-break sequence,
- body-shot accumulation before finish.

Candidate highlight tags:

```text
PERFECT_SLIP_COUNTER
CLOSEST_DODGE
PULL_COUNTER
CLEAN_HOOK_FINISH
BODY_SHOT_FINISH
COMEBACK_KNOCKDOWN
LAST_SECONDS_FINISH
PRESSURE_SEQUENCE
COMBINATION_FINISH
GUARD_BREAK_COUNTER
```

---

# 10. P1 Validation Invariants

Before implementation is considered sound:

1. A geometrically missed punch never becomes a clean hit through RNG.
2. A perfectly timed slip can fail to create offense if range/balance is wrong.
3. A hard punch thrown off balance is weaker and/or more vulnerable than the same punch from a stable state.
4. Footwork alters both offense and defense through position, not only animation.
5. Guard prevents/reduces appropriate contacts but cannot make inactivity globally optimal.
6. Repeated attack spam degrades tactical effectiveness through capacity/balance/recovery.
7. Body attacks have strategic value beyond cosmetic alternate damage.
8. Every authoritative outcome produces enough event data for replay reconstruction.
9. A replay/highlight can explain *why* the outcome occurred from logged state.
10. Fighter stats modify envelopes; they do not override geometry and timing.

---

# 11. Items Requiring P1 Calibration

Do not freeze numeric values from theory alone.

Calibration is required for:

- movement speeds,
- head-offset ranges,
- effective reach,
- active punch windows,
- action costs,
- short recovery rates,
- long-term stamina drain/recovery,
- balance penalties,
- counter-window duration,
- impact-to-condition mapping,
- block absorption,
- knockdown-pressure accumulation/decay,
- body-shot stamina effects.

The matrix freezes **relationships and causality**, not production constants.
