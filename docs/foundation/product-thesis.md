# Boxer — Product Thesis

## 1. Product Identity

Boxer is not primarily a traditional boxing game viewed from a broadcast camera.

It is a **first-person boxing career simulator** whose core ambition is to make the player experience the career through the fighter's own body and point of view.

The design target is:

> **Do not control a boxer. Be the boxer.**

The intended long-term experience begins with low-status, low-resource fighting and can eventually progress toward elite championship competition. The emotional arc is intended to move from anonymity and survival to mastery, reputation, and world-title stakes.

## 2. POV Is a System Constraint, Not a Camera Choice

First-person POV is not cosmetic. It should change how the game is designed.

Whenever practical, the player should experience the world from the fighter's own perspective:

- fighting,
- defensive movement,
- training,
- interacting with coaches,
- entering venues,
- resting and recovering,
- receiving medical treatment,
- selecting or preparing for fights,
- buying or equipping gear,
- experiencing victory and defeat.

Third-person presentation may eventually be used for explicitly externalized moments such as replay, broadcast highlights, posters, or profile views, but these are exceptions rather than the gameplay default.

## 3. Embodied Control Thesis

The current control hypothesis maps distinct parts of the player-device interaction to distinct functions of the fighter:

### Device = Head

Physical device orientation/motion represents evasive head movement.

The intended model is continuous rather than command-based. The player should not merely trigger a canned `DODGE_LEFT` animation; within a bounded operating range, device movement should influence the virtual head/camera position so that punch trajectories can genuinely miss or connect.

### Left Thumb = Feet

The left thumb controls footwork and position.

Candidate vocabulary includes:

- forward,
- backward,
- lateral movement,
- distance management.

The exact gesture grammar is not yet frozen. It must be proven experimentally.

### Right Thumb = Fists

The right thumb expresses attacking intent.

Candidate actions include:

- jab,
- cross,
- hook,
- uppercut,
- body attack,
- combinations.

The goal is not to maximize the number of gestures. The goal is to provide enough expressive control to create tactical boxing without turning play into gesture memorization.

### No Active Input = Return to Guard

The boxer naturally returns to a defensive stance when the player is not actively attacking or moving through another conflicting state.

This must not create free invulnerability. Guard must remain vulnerable to tactical choices such as body attacks, guard damage, stamina drain, positional pressure, timing, or other mechanisms proven later.

## 4. Desired Player Perception

The target mental model is:

> "I saw the punch, moved my head, made it miss, and countered."

The failure mental model is:

> "I remembered the dodge gesture and triggered an animation."

Likewise, attacking should feel like choosing the right punch at the right range and moment, not simply producing enough swipe events.

## 5. Combat Design Invariants

These invariants should remain true unless evidence proves one must change.

### 5.1 Reading matters

A player must be able to infer opponent intent from movement and context rather than depending on large warning icons.

### 5.2 Range matters

Different punches should have meaningful distance requirements. A player should sometimes need to move before attacking.

### 5.3 Defense is active even when guard is automatic

Automatic return-to-guard reduces button complexity, but good defense must still require reading, positioning, stamina management, and/or head movement.

### 5.4 Counters matter

Successful evasion should be capable of creating temporary tactical advantage. Dodge without a meaningful counter opportunity risks becoming an isolated animation mechanic.

### 5.5 Spam must be self-defeating

Repeated attack input should not be the dominant optimal strategy. Potential constraints include stamina, recovery, balance, range, counter vulnerability, and guard exposure.

### 5.6 Impact must not depend on abusive camera effects

POV allows strong hit feedback, but readability and comfort take priority over excessive shake, blur, rotation, flash, or screen obstruction.

### 5.7 Stats must not replace boxing skill

Future progression may modify capability, but opponents and equipment should not turn the game into a simple numerical RPG where tactical input becomes secondary.

## 6. Career Fantasy

If combat is proven, the career progression should create a clear rise in stakes.

Candidate progression:

1. Underground / street fighting
2. Amateur competition
3. Regional professional boxing
4. National contention
5. International competition
6. Championship level

Possible venues may include:

- streets and parking areas,
- improvised underground spaces,
- cages or club venues,
- local boxing halls,
- casinos and regional arenas,
- national stadiums,
- major championship arenas.

The visual jump from low-tier fighting to a massive title arena should be earned through gameplay rather than available immediately.

## 7. Career Systems — Candidate, Not Yet Authorized

Potential future systems include:

- fighter identity and nationality,
- height, weight, reach, stance, weight class,
- hair and facial customization,
- coaches,
- gyms,
- equipment,
- clothing,
- nutrition,
- training camps,
- injuries,
- medical treatment and hospital sequences,
- rankings,
- purse and expenses,
- rivalries,
- rematches,
- travel,
- sponsors,
- contracts.

Each system must justify its existence by improving fight preparation, consequence, identity, or progression. Systems that do not materially strengthen the boxing career fantasy should be removed or deferred.

## 8. Character Customization Constraint

POV creates a specific design problem: the player rarely sees their own face or full body during combat.

Therefore, future customization must be made meaningful through appropriate first/third-person contexts such as:

- mirrors,
- weigh-ins,
- profile screens,
- fight posters,
- walkout presentation,
- broadcast replay,
- victory photography,
- championship ceremonies.

This is a later proof problem. It is not justification to build customization during Phase 0.

## 9. Injury and Recovery Thesis

Injury can provide consequence and career storytelling, but it should not become a disguised waiting timer.

A strong injury system would create decisions such as:

- fight while compromised,
- pay for better treatment,
- skip an opportunity,
- accept slower recovery,
- change training load.

A weak injury system would simply say:

> Wait eight hours or pay.

The latter conflicts with the intended simulation depth and should be avoided unless later evidence strongly justifies it.

## 10. PvP Position

Real-time PvP is a potential end-state, not an initial requirement.

Timing-based boxing creates major networking and fairness risks:

- latency,
- prediction,
- hit validation,
- stat mismatch,
- pay-to-win pressure,
- disconnects,
- cheating.

PvP remains explicitly locked until offline embodied combat is proven.

## 11. Product Success Condition

The most important early success signal is not visual quality.

It is whether a player finishes a short test and wants to immediately fight again because the exchange itself felt satisfying.

A visually impressive prototype that produces only the reaction:

> "That looks good."

is insufficient.

The desired reaction is:

> "Let me fight again."

## 12. Current Thesis Status

The product vision is considered **promising but unproven**.

The embodied control architecture has enough technical plausibility to justify a dedicated Phase 0 experimental harness. It does not yet justify production development of the full game.

## 13. Impact Feedback and Energy Budget

Impact feedback is part of the embodied-control thesis, not a decorative effect.

### 13.1 Haptic thesis

The phone should communicate punch impact through short, event-based haptic feedback:

- blocked or light jab → very light, short feedback,
- clean straight → medium feedback,
- heavy hook / power shot → stronger, short feedback,
- body shot → distinct profile where supported,
- knockdown / major impact → strongest allowed profile, still bounded for comfort.

Haptic strength should not be a simple linear mapping from numerical damage. Punch type, impact quality, target region, guard state, and knockdown state may influence the feedback profile.

The design principle is:

> **Proportional but sparse.**

Do not vibrate continuously and do not emit haptic feedback for every low-value contact if it creates noise, fatigue, thermal cost, battery cost, or motion-sensor interference.

### 13.2 Haptic settings

Players must be able to disable or reduce haptics.

Candidate settings:

- Off
- Low
- Normal
- Strong

The exact options depend on platform capability and Phase 0 evidence.

### 13.3 Haptic / motion-sensor interaction

Because the physical device also represents the player's head, game-generated haptic events must be timestamped in the sensor pipeline.

The motion detector must be evaluated for false head movement caused by haptic impulses. A heavy hit must not accidentally become an evade input.

### 13.4 Audio thesis

Audio is a complementary feedback channel, not a mandatory gameplay dependency.

Candidate layers:

1. **Impact** — glove, guard, head and body contact.
2. **Body state** — breathing, heartbeat, muffled hearing, recovery cues.
3. **Environment** — crowd, coach, announcer, arena ambience.

A player who disables audio must still be able to understand and play the fight through visual and haptic channels.

Candidate audio settings:

- Sound Effects On / Off
- Crowd / Ambience Off / Low / Full
- Voice / Coach / Announcer controls if needed later

### 13.5 Energy and thermal principle

The game must treat battery drain and thermal load as measurable product constraints.

Haptics and audio contribute to device energy use, but the total budget must also account for:

- rendering load,
- target frame rate,
- display brightness assumptions,
- physics,
- motion sampling,
- audio processing,
- networking when introduced later.

The project must not assume that haptics or audio are harmless or dominant. Their actual incremental cost must be measured on real devices.

### 13.6 Player modes

Candidate operating presets to prove later:

- **Full Immersion** — haptic + sound + full impact feedback.
- **Balanced** — reduced haptic/camera effects with sound retained.
- **Battery Saver** — haptic off by default, reduced frame/visual load, optional audio.

These presets are hypotheses, not production commitments.

### 13.7 Proof requirement

Before production defaults are frozen, the project must compare at least:

- Full Feedback,
- No Haptic,
- No Audio,
- Battery Saver / reduced-feedback configuration.

Evidence should include:

- perceived impact,
- hit-strength recognition,
- fatigue / annoyance,
- false motion events,
- battery drain,
- thermal behavior,
- frame stability.

Feedback features are retained only if their experiential value justifies their measurable device cost.
