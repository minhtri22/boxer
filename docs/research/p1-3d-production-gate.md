# Boxer — 3D Production Gate

## Decision

3D production is intentionally deferred until the core combat model has been proven and reviewed.

Blender/production-quality 3D assets are **not** a prerequisite for the current mechanics research. The current MVP should remain visually simple enough that mechanics, geometry, and causality are easy to inspect.

## 3D gate

Do not begin the full 3D production pass until the following four systems have been proven to an acceptable level through implementation evidence and human UAT:

1. **Body / whole-body mechanics**
   - player body representation and coupling are understandable;
   - opponent body/punch embodiment is readable;
   - movement, guard, evade, commitment, and recovery can be inspected and explained.

2. **Punches / range / HIT-MISS**
   - punch families are distinguishable and intentional;
   - close/far range behavior is understandable;
   - HIT, MISS, and BLOCK are geometrically and visually explainable;
   - visible punch reach agrees with combat resolution.

3. **Scoring**
   - scoring rules are explicitly defined;
   - scoring can be derived from authoritative combat events rather than placeholder presentation state;
   - the player can understand why a round/bout score changed.

4. **Health / force (HP + stamina/power)**
   - damage/health has an explicit validated formula;
   - stamina/force/exertion has an explicit validated formula;
   - placeholder HUD percentages are no longer treated as authoritative combat physics.

## Production rule

> PROVE COMBAT FIRST → 3D LAST.

When all four gates are sufficiently closed, the game may move to a 3D presentation pass using Blender assets while preserving the already-proven Unity mechanics and semantic combat model.

The intended migration is **3D presentation, not a mechanics rewrite**.

Potential later 3D scope:

- rigged boxer bodies;
- shoulders, upper arms, elbows, forearms, gloves;
- readable body rotation and weight transfer;
- boxing ring/environment;
- hit reactions;
- presentation polish and replay/highlight cameras.

Until these gates are closed, avoid spending research time on production-quality 3D modeling, materials, lighting, or cosmetic animation polish that cannot help prove the four core systems above.
