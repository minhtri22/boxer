using UnityEngine;

namespace BoxerP0
{
    public enum PunchFeelMode { Legacy, Curve, Fast }

    // The same profile travels with the combat frame into BOTH draw and sweep.
    // Diagnostic A/B never writes outcomes, resources, reach or opponent settings.
    public readonly struct PunchMotionProfile
    {
        public readonly float Commit, Extend, Recover;
        public PunchMotionProfile(float commit, float extend, float recover)
        { Commit = commit; Extend = extend; Recover = recover; }
        public static PunchFeelMode Mode { get; set; } = PunchFeelMode.Fast;
        public static bool Ballistic => Mode != PunchFeelMode.Legacy;
        public static PunchMotionProfile Player(PunchIntent intent)
        {
            if (Mode != PunchFeelMode.Fast) return new(.09f, .14f, .28f);
            return intent switch
            {
                PunchIntent.Jab => new(.04f, .07f, .19f),
                PunchIntent.Cross => new(.06f, .085f, .23f),
                PunchIntent.LeadHook or PunchIntent.RearHook => new(.07f, .095f, .26f),
                PunchIntent.LeadUppercut or PunchIntent.RearUppercut => new(.08f, .10f, .275f),
                PunchIntent.LeadOverhand or PunchIntent.RearOverhand => new(.09f, .105f, .30f),
                _ => new(.09f, .14f, .28f)
            };
        }
        public static float Progress(ActionPhase phase, float t, bool ballistic)
        {
            t = Mathf.Clamp01(t);
            if (!ballistic || phase == ActionPhase.Commit) return Round2Motion.Smooth(t);
            // Nonzero terminal strike velocity; do not ease to a long soft stop
            // before the endpoint. Retraction has a fast pull then guard settle.
            if (phase == ActionPhase.Extend) return t * t * (2.35f - 1.35f * t);
            if (phase == ActionPhase.Recover) return 1f - (1f - t) * (1f - t) * (1f - t);
            return 0f;
        }
        public static void Configure(string url)
        {
            Mode = PunchFeelMode.Fast;
            // Only explicit desktop diagnostics allow comparing legacy/curve modes.
            if (string.IsNullOrEmpty(url) || !url.Contains("desktop=1") || !url.Contains("metrics=1")) return;
            if (url.Contains("punchFeel=legacy")) Mode = PunchFeelMode.Legacy;
            else if (url.Contains("punchFeel=curve")) Mode = PunchFeelMode.Curve;
        }
    }
}
