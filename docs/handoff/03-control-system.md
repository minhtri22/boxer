# 03 — Control System

## Frozen control hypothesis

Boxer's control scheme is a product-level invariant for the current research track:

- **Phone = Head**
- **Left Thumb = Feet**
- **Right Thumb = Punch Controller**
- **No active action = Return to Guard**

## Phone = Head

Device orientation is used as head/evasion intent. The design goal is continuous, geometric head movement rather than a canned left/right dodge button.

Current Web/iPhone research uses browser/device-orientation as a surrogate interaction route. Lack of orientation must not globally freeze the game; late orientation can establish neutral and activate head control without reload.

## Left Thumb = Feet

Left-thumb directional input expresses movement/footwork intent. Current implementation changes player positioning; current research has only partially coupled this to punch mechanics.

P1-A1 promotes categorical forward step state into straight-punch reach:

- advancing → 1.06
- neutral → 1.00
- retreating → 0.94

Do not replace this with arbitrary continuous tuning without a new experiment.

## Right Thumb = Punch Intent

The right thumb chooses punch family, not a literal right-hand attack.

Gesture vocabulary:

- `TAP → STRAIGHT`
- `HOLD + SWIPE UP → UPPERCUT`
- `HOLD + SWIPE HORIZONTAL → HOOK`
- `HOLD + SWIPE DOWN → OVERHAND`

Current hold threshold is implementation-specific and regression-protected.

## Hand-selection principle

Gesture selects family. The actual hand should be selected from stance/sequence/body context.

Examples of intended semantics:

- first straight often maps to lead jab;
- next straight may map to rear cross;
- hook/uppercut can alternate by prior hand/context;
- overhand is generally rear-hand biased.

Do not change the gesture vocabulary into explicit left/right punch buttons unless the product thesis is intentionally revised.

## Guard

No active offensive input returns to guard. This is an important part of the rhythm:

`ACT → RECOVER → GUARD`

Guard must remain readable but should not become a permanent invulnerability state.

## Control acceptance

A successful control revision must preserve:

1. immediate comprehension;
2. no accidental global startup lock;
3. consistent mapping between physical input and body intent;
4. compatibility with semantic event logging;
5. low input burden on mobile.
