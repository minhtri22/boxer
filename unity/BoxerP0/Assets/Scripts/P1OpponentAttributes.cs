using System;
using System.Globalization;

namespace BoxerP0
{
    public enum P1OpponentProfile
    {
        Balanced,
        LongReach,
        Pressure,
        FastHands
    }

    public readonly struct P1OpponentAttributeSet
    {
        public P1OpponentAttributeSet(
            P1OpponentProfile profile,
            float reachFactor,
            float attackGapFactor,
            float phaseDurationFactor)
        {
            Profile = profile;
            ReachFactor = reachFactor;
            AttackGapFactor = attackGapFactor;
            PhaseDurationFactor = phaseDurationFactor;
        }

        public P1OpponentProfile Profile { get; }
        public float ReachFactor { get; }
        public float AttackGapFactor { get; }
        public float PhaseDurationFactor { get; }

        public float HeadReachMeters => P1OpponentAttributes.BaselineHeadReachMeters * ReachFactor;
        public float BodyReachMeters => P1OpponentAttributes.BaselineBodyReachMeters * ReachFactor;
        public float AttackGapMinSeconds => P1OpponentAttributes.BaselineAttackGapMinSeconds * AttackGapFactor;
        public float AttackGapMaxSeconds => P1OpponentAttributes.BaselineAttackGapMaxSeconds * AttackGapFactor;
        public float CommitSeconds => P1OpponentAttributes.BaselineCommitSeconds * PhaseDurationFactor;
        public float ExtendSeconds => P1OpponentAttributes.BaselineExtendSeconds * PhaseDurationFactor;
        public float RecoverSeconds => P1OpponentAttributes.BaselineRecoverSeconds * PhaseDurationFactor;

        public string ToInspectorText()
        {
            return $"P1-D {P1OpponentAttributes.ProfileToken(Profile)}  REACH x{F(ReachFactor)}  GAP x{F(AttackGapFactor)}  SPEED x{F(1f / PhaseDurationFactor)}";
        }

        private static string F(float value) => value.ToString("F2", CultureInfo.InvariantCulture);
    }

    public static class P1OpponentAttributes
    {
        public const float BaselineHeadReachMeters = 1.02f;
        public const float BaselineBodyReachMeters = 0.96f;
        public const float BaselineAttackGapMinSeconds = 0.65f;
        public const float BaselineAttackGapMaxSeconds = 1.20f;
        public const float BaselineCommitSeconds = 0.34f;
        public const float BaselineExtendSeconds = 0.17f;
        public const float BaselineRecoverSeconds = 0.48f;

        public const float LongReachFactor = 1.08f;
        public const float PressureGapFactor = 0.80f;
        public const float FastHandsDurationFactor = 0.90f;

        public static P1OpponentAttributeSet Resolve(P1OpponentProfile profile)
        {
            return profile switch
            {
                P1OpponentProfile.LongReach => new P1OpponentAttributeSet(profile, LongReachFactor, 1f, 1f),
                P1OpponentProfile.Pressure => new P1OpponentAttributeSet(profile, 1f, PressureGapFactor, 1f),
                P1OpponentProfile.FastHands => new P1OpponentAttributeSet(profile, 1f, 1f, FastHandsDurationFactor),
                _ => new P1OpponentAttributeSet(P1OpponentProfile.Balanced, 1f, 1f, 1f)
            };
        }

        public static bool TryParse(string raw, out P1OpponentProfile profile)
        {
            string normalized = (raw ?? string.Empty).Trim().Replace("-", string.Empty).Replace("_", string.Empty);
            if (normalized.Equals("longreach", StringComparison.OrdinalIgnoreCase))
            {
                profile = P1OpponentProfile.LongReach;
                return true;
            }
            if (normalized.Equals("pressure", StringComparison.OrdinalIgnoreCase))
            {
                profile = P1OpponentProfile.Pressure;
                return true;
            }
            if (normalized.Equals("fasthands", StringComparison.OrdinalIgnoreCase))
            {
                profile = P1OpponentProfile.FastHands;
                return true;
            }
            if (normalized.Equals("balanced", StringComparison.OrdinalIgnoreCase))
            {
                profile = P1OpponentProfile.Balanced;
                return true;
            }

            profile = P1OpponentProfile.Balanced;
            return false;
        }

        public static string ProfileToken(P1OpponentProfile profile)
        {
            return profile switch
            {
                P1OpponentProfile.LongReach => "LONG_REACH",
                P1OpponentProfile.Pressure => "PRESSURE",
                P1OpponentProfile.FastHands => "FAST_HANDS",
                _ => "BALANCED"
            };
        }
    }
}
