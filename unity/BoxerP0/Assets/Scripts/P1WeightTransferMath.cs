using UnityEngine;

namespace BoxerP0
{
    public readonly struct P1WeightTransferPose
    {
        public readonly float ForwardLoad01;
        public readonly float VisualForwardOffsetMeters;

        public P1WeightTransferPose(float forwardLoad01, float visualForwardOffsetMeters)
        {
            ForwardLoad01 = forwardLoad01;
            VisualForwardOffsetMeters = visualForwardOffsetMeters;
        }
    }

    /// <summary>
    /// P1-C1 diagnostic/presentation-only weight-transfer baseline.
    /// It intentionally ignores punch family/hand and returns no gameplay multiplier.
    /// </summary>
    public static class P1WeightTransferMath
    {
        public const float NeutralLoad01 = 0.50f;
        public const float CommitEndLoad01 = 0.42f;
        public const float ExtendEndLoad01 = 0.62f;
        public const float VisualOffsetScaleMeters = 0.25f;

        public static P1WeightTransferPose Sample(PunchIntent intent, ActionPhase phase, float normalizedPhase)
        {
            if (intent == PunchIntent.None || phase == ActionPhase.Guard)
                return FromLoad(NeutralLoad01);

            float t = Smooth01(Mathf.Clamp01(normalizedPhase));
            float load = phase switch
            {
                ActionPhase.Commit => Mathf.Lerp(NeutralLoad01, CommitEndLoad01, t),
                ActionPhase.Extend => Mathf.Lerp(CommitEndLoad01, ExtendEndLoad01, t),
                ActionPhase.Recover => Mathf.Lerp(ExtendEndLoad01, NeutralLoad01, t),
                _ => NeutralLoad01
            };
            return FromLoad(load);
        }

        public static P1WeightTransferPose FromLoad(float forwardLoad01)
        {
            float load = Mathf.Clamp01(forwardLoad01);
            float offset = (load - NeutralLoad01) * VisualOffsetScaleMeters;
            return new P1WeightTransferPose(load, offset);
        }

        private static float Smooth01(float t) => t * t * (3f - 2f * t);
    }
}
