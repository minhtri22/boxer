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
        RearHook,
        LeadUppercut,
        RearUppercut,
        LeadOverhand,
        RearOverhand
    }

    public enum PunchFamily
    {
        None,
        Straight,
        Hook,
        Uppercut,
        Overhand
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
        public bool Straight { get; private set; }
        public bool Hook { get; private set; }
        public bool Uppercut { get; private set; }
        public bool Overhand { get; private set; }

        public bool HeadReady => HeadLeft && HeadRight;
        public bool FootworkReady => MoveLeft && MoveRight && MoveForward && MoveBack;
        public bool PunchesReady => Straight && Hook && Uppercut && Overhand;

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
            switch (PunchLabels.Family(intent))
            {
                case PunchFamily.Straight:
                    Straight = true;
                    break;
                case PunchFamily.Hook:
                    Hook = true;
                    break;
                case PunchFamily.Uppercut:
                    Uppercut = true;
                    break;
                case PunchFamily.Overhand:
                    Overhand = true;
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
                PunchIntent.LeadUppercut => "LEAD UPPERCUT",
                PunchIntent.RearUppercut => "REAR UPPERCUT",
                PunchIntent.LeadOverhand => "LEAD OVERHAND",
                PunchIntent.RearOverhand => "REAR OVERHAND",
                _ => "NONE"
            };
        }

        public static PunchFamily Family(PunchIntent intent)
        {
            return intent switch
            {
                PunchIntent.Jab or PunchIntent.Cross => PunchFamily.Straight,
                PunchIntent.LeadHook or PunchIntent.RearHook => PunchFamily.Hook,
                PunchIntent.LeadUppercut or PunchIntent.RearUppercut => PunchFamily.Uppercut,
                PunchIntent.LeadOverhand or PunchIntent.RearOverhand => PunchFamily.Overhand,
                _ => PunchFamily.None
            };
        }

        public static bool IsRearHand(PunchIntent intent)
        {
            return intent == PunchIntent.Cross ||
                   intent == PunchIntent.RearHook ||
                   intent == PunchIntent.RearUppercut ||
                   intent == PunchIntent.RearOverhand;
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

    /// <summary>
    /// P1-A2 gesture vocabulary. Gesture selects punch FAMILY only:
    /// tap = straight, held up = uppercut, held horizontal = hook, held down = overhand.
    /// Hand selection is intentionally separate from gesture classification.
    /// </summary>
    public static class PunchGestureClassifier
    {
        public const float HoldSeconds = 0.12f;

        public static PunchFamily ClassifyFamily(GestureMetrics metrics, float pixelScale = 1f)
        {
            float tapTravel = 24f * pixelScale;
            float swipeTravel = 42f * pixelScale;

            if (metrics.StraightDistance <= tapTravel && metrics.PathLength <= tapTravel * 1.35f)
            {
                return PunchFamily.Straight;
            }

            if (metrics.Duration < HoldSeconds ||
                metrics.PathLength < swipeTravel ||
                metrics.StraightDistance < swipeTravel * 0.65f)
            {
                return PunchFamily.None;
            }

            float absX = Mathf.Abs(metrics.Displacement.x);
            float absY = Mathf.Abs(metrics.Displacement.y);
            if (absX >= absY * 0.85f)
            {
                return PunchFamily.Hook;
            }

            return metrics.Displacement.y > 0f
                ? PunchFamily.Uppercut
                : PunchFamily.Overhand;
        }

        // Compatibility helper for synthetic/editor callers. Runtime touch input uses
        // ClassifyFamily + PunchHandSelector so gesture never directly encodes the hand.
        public static PunchIntent Classify(GestureMetrics metrics, float pixelScale = 1f)
        {
            return PunchHandSelector.Select(ClassifyFamily(metrics, pixelScale), PunchIntent.None);
        }
    }

    public static class PunchHandSelector
    {
        public static PunchIntent Select(PunchFamily family, PunchIntent previousIntent)
        {
            bool previousRear = PunchLabels.IsRearHand(previousIntent);
            bool useRear = family == PunchFamily.Overhand
                ? true
                : previousIntent != PunchIntent.None && !previousRear;

            return family switch
            {
                PunchFamily.Straight => useRear ? PunchIntent.Cross : PunchIntent.Jab,
                PunchFamily.Hook => useRear ? PunchIntent.RearHook : PunchIntent.LeadHook,
                PunchFamily.Uppercut => useRear ? PunchIntent.RearUppercut : PunchIntent.LeadUppercut,
                PunchFamily.Overhand => useRear ? PunchIntent.RearOverhand : PunchIntent.LeadOverhand,
                _ => PunchIntent.None
            };
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

    public static class OpponentReachMath
    {
        public static Vector3 ClampEndpoint(Vector3 start, Vector3 desiredEnd, float maxReach)
        {
            Vector3 delta = desiredEnd - start;
            float distance = delta.magnitude;
            if (distance <= Mathf.Max(0f, maxReach) || distance <= 0.0001f)
            {
                return desiredEnd;
            }

            return start + delta / distance * maxReach;
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
