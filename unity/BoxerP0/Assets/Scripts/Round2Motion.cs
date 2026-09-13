using UnityEngine;

namespace BoxerP0
{
    // Shared local-space pose: the renderer and contact sampler consume this same solution.
    public static class Round2Motion
    {
        public const float GloveRadius = 0.115f;
        public const float HeadRadius = 0.18f;
        public const float SampleSeconds = 1f / 240f;
        public const float NumericEpsilon = 0.00001f;
        public static float Smooth(float t) { t = Mathf.Clamp01(t); return t*t*(3f-2f*t); }
        public static Vector3 Guard(bool left) => new Vector3(left ? -0.22f : 0.22f, left ? 1.49f : 1.46f, 0.28f);
        public static float HookFactor(PunchIntent intent,float distance) => P1PunchMechanics.IsHook(intent)
            ? Mathf.Lerp(1f,P1PunchMechanics.A3HookFarReachFactor,Mathf.InverseLerp(CloseBoundary,BoxingBoundary,distance)) : 1f;
        public static Vector3 Endpoint(PunchIntent intent, string step, float distance)
        {
            bool left = !PunchLabels.IsRearHand(intent);
            float side = left ? -1f : 1f;
            Vector3 p = PunchLabels.Family(intent) switch
            {
                PunchFamily.Hook => new Vector3(-side * 0.10f, 1.47f, 0.48f),
                PunchFamily.Uppercut => new Vector3(side * 0.12f, 1.59f, 0.49f),
                PunchFamily.Overhand => new Vector3(side * 0.10f, 1.60f, 0.60f),
                _ => new Vector3(side * 0.12f, 1.51f, left ? 0.61f : 0.67f)
            };
            p = P1PunchMechanics.ApplyA1StraightReach(intent, p, step);
            // Preserve hook close > far relationship on the new anatomical distance scale.
            p.z *= HookFactor(intent,distance);
            if (P1PunchMechanics.IsUppercut(intent))
                p.y = Mathf.Lerp(Commit(intent).y, p.y, P1PunchMechanics.EffectiveA32UppercutDriveFactor(intent, step));
            return p;
        }
        public static Vector3 Commit(PunchIntent intent)
        {
            bool left = !PunchLabels.IsRearHand(intent);
            float s = left ? -1f : 1f;
            Vector3 g = Guard(left);
            return PunchLabels.Family(intent) switch
            {
                PunchFamily.Hook => g + new Vector3(s*0.18f, -0.06f, -0.03f),
                PunchFamily.Uppercut => g + new Vector3(s*0.04f, -0.30f, -0.04f),
                PunchFamily.Overhand => g + new Vector3(s*0.08f, 0.18f, -0.06f),
                _ => g + new Vector3(s*0.03f, -0.03f, -0.06f)
            };
        }
        public static float Load(PunchIntent intent, ActionPhase phase, float t) =>
            intent == PunchIntent.None ? 0f : P1BodyRotationMath.Sample(intent, phase, t).Amplitude;
        public static Quaternion Torso(PunchIntent intent, ActionPhase phase, float t)
        {
            float sign = PunchLabels.IsRearHand(intent) ? -1f : 1f;
            return Quaternion.Euler(0f, sign * 14f * Load(intent, phase, t), 0f);
        }
        public static Vector3 Shift(PunchIntent intent, ActionPhase phase, float t) =>
            new Vector3(0f, -0.012f * Load(intent, phase, t), 0.035f * Load(intent, phase, t));
        public static ArmChainSolution Sample(bool left, PunchIntent intent, ActionPhase phase, float t,
            Vector3 endpoint)
        {
            bool active = intent != PunchIntent.None && left != PunchLabels.IsRearHand(intent);
            Vector3 g = Guard(left), requested = g;
            Vector3 shoulder = Torso(intent, phase, t) * new Vector3(left ? -0.38f : 0.38f, 1.43f, 0.02f) + Shift(intent, phase, t);
            if (active)
            {
                Vector3 c = Commit(intent);
                float u = Smooth(t);
                requested = phase switch
                {
                    ActionPhase.Commit => Vector3.Lerp(g, c, u),
                    ActionPhase.Extend => Vector3.Lerp(c, endpoint, u),
                    ActionPhase.Recover => Vector3.Lerp(endpoint, g, u),
                    _ => g
                };
                if (phase == ActionPhase.Extend)
                {
                    float arc = Mathf.Sin(Mathf.PI * u);
                    if (PunchLabels.Family(intent) == PunchFamily.Hook) requested += new Vector3((left ? -1f : 1f)*0.16f*arc, 0f, 0.12f*arc);
                    if (PunchLabels.Family(intent) == PunchFamily.Overhand) requested.y += 0.12f*arc;
                }
            }
            Vector3 pole = shoulder + new Vector3(left ? -0.12f : 0.12f, -0.5f, 0.16f);
            return ArmChainMath.Solve(shoulder, requested, pole, ArmVisualEmbodiment.UpperArmLength, ArmVisualEmbodiment.ForearmLength);
        }
        // Pocket = hook forward extent + head/glove surfaces. Outer range includes guard extension.
        public const float CloseBoundary = 0.48f + HeadRadius + GloveRadius;
        public const float BoxingBoundary = 0.67f + 0.28f + 2f * GloveRadius;
        public static string Band(float distance) => distance > BoxingBoundary ? "LONG" : distance < CloseBoundary ? "CLOSE" : "BOXING";

        // Relative sweep handles moving defender volumes; returns earliest surface contact.
        public static bool Sweep(Vector3 a, Vector3 b, Vector3 targetA, Vector3 targetB, float radius, out float fraction)
        {
            Vector3 p = a-targetA, d = (b-targetB)-p;
            float c = Vector3.Dot(p,p)-radius*radius;
            if (c <= NumericEpsilon*NumericEpsilon) { fraction=0f; return true; }
            float aa=Vector3.Dot(d,d), bb=Vector3.Dot(p,d), discriminant=bb*bb-aa*c;
            fraction=1f;
            if (aa < NumericEpsilon*NumericEpsilon || discriminant < 0f) return false;
            float hit=(-bb-Mathf.Sqrt(discriminant))/aa;
            if (hit<0f || hit>1f) return false;
            fraction=hit; return true;
        }
    }
}
