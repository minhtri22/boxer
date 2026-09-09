using UnityEngine;

namespace BoxerP0
{
    public readonly struct P1WholeBodyRecoveryPose
    {
        public readonly float GuardReturn01;
        public readonly float PelvisYawDegrees;
        public readonly float EffectiveTorsoYawDegrees;
        public readonly float ForwardLoad01;
        public readonly float VisualForwardOffsetMeters;

        public P1WholeBodyRecoveryPose(
            float guardReturn01,
            float pelvisYawDegrees,
            float effectiveTorsoYawDegrees,
            float forwardLoad01,
            float visualForwardOffsetMeters)
        {
            GuardReturn01 = guardReturn01;
            PelvisYawDegrees = pelvisYawDegrees;
            EffectiveTorsoYawDegrees = effectiveTorsoYawDegrees;
            ForwardLoad01 = forwardLoad01;
            VisualForwardOffsetMeters = visualForwardOffsetMeters;
        }

        public bool IsNeutral(float tolerance = 0.0001f)
        {
            return Mathf.Abs(GuardReturn01 - 1f) <= tolerance
                && Mathf.Abs(PelvisYawDegrees) <= tolerance
                && Mathf.Abs(EffectiveTorsoYawDegrees) <= tolerance
                && Mathf.Abs(ForwardLoad01 - P1WeightTransferMath.NeutralLoad01) <= tolerance
                && Mathf.Abs(VisualForwardOffsetMeters) <= tolerance;
        }
    }

    /// <summary>
    /// P1-C3 composite recovery contract. It observes already-frozen C0/C1/C2 states
    /// and adds no combat consequence.
    /// </summary>
    public static class P1WholeBodyRecoveryMath
    {
        public static P1WholeBodyRecoveryPose Sample(PunchIntent intent, ActionPhase phase, float normalizedPhase)
        {
            float t = Mathf.Clamp01(normalizedPhase);
            P1BodyRotationPose rotation = P1BodyRotationMath.Sample(intent, phase, t);
            P1WeightTransferPose weight = P1WeightTransferMath.Sample(intent, phase, t);
            float torso = P1StraightBodyCouplingMath.EffectiveTorsoYawDegrees(intent, rotation, weight);

            float guardReturn = phase switch
            {
                ActionPhase.Guard => 1f,
                ActionPhase.Recover => Smooth01(t),
                _ => 0f
            };

            return new P1WholeBodyRecoveryPose(
                guardReturn,
                rotation.PelvisYawDegrees,
                torso,
                weight.ForwardLoad01,
                weight.VisualForwardOffsetMeters);
        }

        private static float Smooth01(float t) => t * t * (3f - 2f * t);
    }
}
