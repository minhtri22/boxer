using System;
using UnityEngine;

namespace BoxerP0
{
    public enum PunchIntent
    {
        None,
        Jab,
        Cross,
        Hook
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
                return PunchIntent.Hook;
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

        public float NormalizedPhase(float duration)
        {
            return duration <= 0f ? 1f : Mathf.Clamp01(PhaseTime / duration);
        }
    }
}

