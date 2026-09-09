using UnityEngine;

namespace BoxerP0
{
    /// <summary>
    /// P1-C2: one presentation-only straight-punch consequence from whole-body state.
    /// Authoritative combat geometry remains unchanged.
    /// </summary>
    public static class P1StraightBodyCouplingMath
    {
        public const float MaxExtraTorsoYawDegrees = 4f;

        public static float ExtraTorsoYawDegrees(PunchIntent intent, float forwardLoad01)
        {
            if (PunchLabels.Family(intent) != PunchFamily.Straight) return 0f;
            if (forwardLoad01 <= P1WeightTransferMath.NeutralLoad01) return 0f;

            float t = Mathf.InverseLerp(
                P1WeightTransferMath.NeutralLoad01,
                P1WeightTransferMath.ExtendEndLoad01,
                Mathf.Clamp01(forwardLoad01));
            t = Smooth01(t);
            float sign = PunchLabels.IsRearHand(intent) ? 1f : -1f;
            return sign * MaxExtraTorsoYawDegrees * t;
        }

        public static float EffectiveTorsoYawDegrees(
            PunchIntent intent,
            P1BodyRotationPose baseRotation,
            P1WeightTransferPose weightTransfer)
        {
            return baseRotation.TorsoYawDegrees + ExtraTorsoYawDegrees(intent, weightTransfer.ForwardLoad01);
        }

        private static float Smooth01(float t) => t * t * (3f - 2f * t);
    }
}
