using System;
using UnityEngine;

namespace BoxerP0
{
    public enum PunchIntent
    {
        None,
        Jab,
        Cross,
        LeadHook,
        RearHook
    }

    public enum CombatOutcome
    {
        None,
        Hit,
        Miss,
        Block
    }

    public enum ActionPhase
    {
        Guard,
        Commit,
        Extend,
        Recover
    }

    public enum OnboardingStage
    {
        WaitingForCalibration,
        HeadControl,
        Footwork,
        Punches,
        Guard,
        Counter,
        Bout,
        Complete
    }

    public sealed class OnboardingProgress
    {
        public bool HeadLeft { get; private set; }
        public bool HeadRight { get; private set; }
        public bool MoveLeft { get; private set; }
        public bool MoveRight { get; private set; }
        public bool MoveForward { get; private set; }
        public bool MoveBack { get; private set; }
        public bool LeadJab { get; private set; }
        public bool RearCross { get; private set; }
        public bool LeadHook { get; private set; }
        public bool RearHook { get; private set; }

        public bool HeadReady => HeadLeft && HeadRight;
        public bool FootworkReady => MoveLeft && MoveRight && MoveForward && MoveBack;
        public bool PunchesReady => LeadJab && RearCross && LeadHook && RearHook;

        public void ObserveHead(float offsetMeters, float threshold = 0.12f)
        {
            if (offsetMeters <= -threshold) HeadLeft = true;
            if (offsetMeters >= threshold) HeadRight = true;
        }

        public void ObserveMovement(Vector2 intent, float threshold = 0.45f)
        {
            if (intent.x <= -threshold) MoveLeft = true;
            if (intent.x >= threshold) MoveRight = true;
            if (intent.y >= threshold) MoveForward = true;
            if (intent.y <= -threshold) MoveBack = true;
        }

        public void ObservePunch(PunchIntent intent)
        {
            switch (intent)
            {
                case PunchIntent.Jab:
                    LeadJab = true;
                    break;
                case PunchIntent.Cross:
                    RearCross = true;
                    break;
                case PunchIntent.LeadHook:
                    LeadHook = true;
                    break;
                case PunchIntent.RearHook:
                    RearHook = true;
                    break;
            }
        }
    }

    public static class PunchLabels
    {
        public static string Display(PunchIntent intent)
        {
            return intent switch
            {
                PunchIntent.Jab => "LEAD JAB",
                PunchIntent.Cross => "REAR CROSS",
                PunchIntent.LeadHook => "LEAD HOOK",
                PunchIntent.RearHook => "REAR HOOK",
                _ => "NONE"
            };
        }

        public static string EventToken(PunchIntent intent)
        {
            return Display(intent).Replace(' ', '_');
        }
    }

    public readonly struct GestureMetrics
    {
        public GestureMetrics(Vector2 displacement, float pathLength, float duration)
        {
            Displacement = displacement;
            PathLength = pathLength;
            Duration = Mathf.Max(duration, 0.001f);
        }

        public Vector2 Displacement { get; }
        public float PathLength { get; }
        public float Duration { get; }
        public float StraightDistance => Displacement.magnitude;
        public float Straightness => PathLength <= 0.001f ? 1f : Mathf.Clamp01(StraightDistance / PathLength);
        public float Speed => PathLength / Duration;
    }

    public static class PunchGestureClassifier
    {
        public static PunchIntent Classify(GestureMetrics metrics, float pixelScale = 1f)
        {
            float minTravel = 42f * pixelScale;
            if (metrics.PathLength < minTravel || metrics.StraightDistance < minTravel * 0.65f)
            {
                return PunchIntent.None;
            }

            float horizontalBias = Mathf.Abs(metrics.Displacement.x) /
                                   Mathf.Max(1f, Mathf.Abs(metrics.Displacement.y));
            bool curvedOrLateral = metrics.Straightness < 0.82f || horizontalBias > 1.15f;
            if (curvedOrLateral)
            {
                return metrics.Displacement.x <= 0f ? PunchIntent.LeadHook : PunchIntent.RearHook;
            }

            bool shortFast = metrics.StraightDistance < 165f * pixelScale && metrics.Speed > 430f * pixelScale;
            return shortFast ? PunchIntent.Jab : PunchIntent.Cross;
        }
    }

    public static class HeadMotionMath
    {
        public static float ResolveOffset(
            float relativeAngleDegrees,
            float deadZoneDegrees = 2.5f,
            float maxAngleDegrees = 18f,
            float maxOffsetMeters = 0.34f)
        {
            float magnitude = Mathf.Abs(relativeAngleDegrees);
            if (magnitude <= deadZoneDegrees)
            {
                return 0f;
            }

            float denominator = Mathf.Max(0.001f, maxAngleDegrees - deadZoneDegrees);
            float normalized = Mathf.Clamp01((magnitude - deadZoneDegrees) / denominator);
            return Mathf.Sign(relativeAngleDegrees) * normalized * maxOffsetMeters;
        }
    }

    public static class CombatGeometry
    {
        public static bool SegmentSphereIntersects(Vector3 start, Vector3 end, Vector3 center, float radius)
        {
            Vector3 segment = end - start;
            float lengthSq = segment.sqrMagnitude;
            if (lengthSq < 0.000001f)
            {
                return (center - start).sqrMagnitude <= radius * radius;
            }

            float t = Mathf.Clamp01(Vector3.Dot(center - start, segment) / lengthSq);
            Vector3 closest = start + segment * t;
            return (center - closest).sqrMagnitude <= radius * radius;
        }
    }

    [Serializable]
    public sealed class TimedActionState
    {
        public ActionPhase Phase { get; private set; } = ActionPhase.Guard;
        public PunchIntent Intent { get; private set; } = PunchIntent.None;
        public float PhaseTime { get; private set; }

        public bool IsBusy => Phase != ActionPhase.Guard;
        public bool CounterWindowOpen => Phase == ActionPhase.Recover;

        public bool TryStart(PunchIntent intent)
        {
            if (intent == PunchIntent.None || IsBusy)
            {
                return false;
            }

            Intent = intent;
            Phase = ActionPhase.Commit;
            PhaseTime = 0f;
            return true;
        }

        public void Step(float deltaTime, float commitSeconds, float extendSeconds, float recoverSeconds)
        {
            if (Phase == ActionPhase.Guard)
            {
                return;
            }

            PhaseTime += Mathf.Max(0f, deltaTime);
            float duration = Phase switch
            {
                ActionPhase.Commit => commitSeconds,
                ActionPhase.Extend => extendSeconds,
                ActionPhase.Recover => recoverSeconds,
                _ => float.PositiveInfinity
            };

            if (PhaseTime < duration)
            {
                return;
            }

            PhaseTime = 0f;
            Phase = Phase switch
            {
                ActionPhase.Commit => ActionPhase.Extend,
                ActionPhase.Extend => ActionPhase.Recover,
                ActionPhase.Recover => ActionPhase.Guard,
                _ => ActionPhase.Guard
            };

            if (Phase == ActionPhase.Guard)
            {
                Intent = PunchIntent.None;
            }
        }

        public void ResetToGuard()
        {
            Phase = ActionPhase.Guard;
            Intent = PunchIntent.None;
            PhaseTime = 0f;
        }

        public float NormalizedPhase(float duration)
        {
            return duration <= 0f ? 1f : Mathf.Clamp01(PhaseTime / duration);
        }
    }
}
