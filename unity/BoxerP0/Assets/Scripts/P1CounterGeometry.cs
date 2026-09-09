using System.Globalization;
using UnityEngine;

namespace BoxerP0
{
    public readonly struct P1CounterOpportunity
    {
        public readonly bool Armed;
        public readonly string Type;
        public readonly float StartSeparationMeters;
        public readonly float EndSeparationMeters;
        public readonly float TargetDisplacementMeters;

        public P1CounterOpportunity(
            bool armed,
            string type,
            float startSeparationMeters,
            float endSeparationMeters,
            float targetDisplacementMeters)
        {
            Armed = armed;
            Type = armed ? P1CombatObservability.NormalizeToken(type) : "NONE";
            StartSeparationMeters = Mathf.Max(0f, startSeparationMeters);
            EndSeparationMeters = Mathf.Max(0f, endSeparationMeters);
            TargetDisplacementMeters = Mathf.Max(0f, targetDisplacementMeters);
        }

        public string ToSemanticEvent()
        {
            return string.Join(" ",
                "P1_COUNTER_OPPORTUNITY",
                $"TYPE={Type}",
                $"START_SEP={F(StartSeparationMeters)}",
                $"END_SEP={F(EndSeparationMeters)}",
                $"MOVE={F(TargetDisplacementMeters)}");
        }

        private static string F(float value) => value.ToString("F3", CultureInfo.InvariantCulture);
    }

    public static class P1CounterGeometry
    {
        public const float MinimumEvadeDisplacementMeters = 0.08f;

        public static P1CounterOpportunity Evaluate(
            Vector3 punchStart,
            Vector3 punchEnd,
            Vector3 targetCenterAtCommit,
            Vector3 targetCenterAtResolve,
            float combinedRadius,
            bool bodyAttack,
            float headOffsetDeltaMeters,
            float playerRootDisplacementMeters,
            CombatOutcome outcome)
        {
            float radius = Mathf.Max(0f, combinedRadius);
            float startSeparation = DistanceToSegment(punchStart, punchEnd, targetCenterAtCommit);
            float endSeparation = DistanceToSegment(punchStart, punchEnd, targetCenterAtResolve);
            float targetMove = Vector3.Distance(targetCenterAtCommit, targetCenterAtResolve);

            bool commitWouldHit = startSeparation <= radius;
            bool resolveWouldHit = endSeparation <= radius;
            if (outcome != CombatOutcome.Miss || !commitWouldHit || resolveWouldHit ||
                targetMove < MinimumEvadeDisplacementMeters)
            {
                return new P1CounterOpportunity(false, "NONE", startSeparation, endSeparation, targetMove);
            }

            float headMove = Mathf.Abs(headOffsetDeltaMeters);
            float rootMove = Mathf.Max(0f, playerRootDisplacementMeters);
            if (!bodyAttack && headMove >= MinimumEvadeDisplacementMeters)
            {
                return new P1CounterOpportunity(true, "HEAD_EVADE", startSeparation, endSeparation, targetMove);
            }

            if (rootMove >= MinimumEvadeDisplacementMeters)
            {
                return new P1CounterOpportunity(true, "FOOTWORK_EVADE", startSeparation, endSeparation, targetMove);
            }

            return new P1CounterOpportunity(false, "NONE", startSeparation, endSeparation, targetMove);
        }

        private static float DistanceToSegment(Vector3 start, Vector3 end, Vector3 point)
        {
            Vector3 segment = end - start;
            float lengthSq = segment.sqrMagnitude;
            if (lengthSq < 0.000001f) return Vector3.Distance(point, start);

            float t = Mathf.Clamp01(Vector3.Dot(point - start, segment) / lengthSq);
            return Vector3.Distance(point, start + segment * t);
        }
    }

    public sealed class P1CounterOpportunityState
    {
        public P1CounterOpportunity Opportunity { get; private set; }
        public bool Armed => Opportunity.Armed;
        public string Type => Opportunity.Type ?? "NONE";

        public void Arm(P1CounterOpportunity opportunity)
        {
            Opportunity = opportunity.Armed ? opportunity : default;
        }

        public bool IsOpen(ActionPhase opponentPhase)
        {
            return Armed && opponentPhase == ActionPhase.Recover;
        }

        public bool Consume(ActionPhase opponentPhase)
        {
            if (!IsOpen(opponentPhase)) return false;
            Clear();
            return true;
        }

        public void Clear()
        {
            Opportunity = default;
        }

        public string Label(ActionPhase opponentPhase)
        {
            if (!Armed) return "CLOSED";
            return IsOpen(opponentPhase) ? "OPEN_" + Type : "ARMED_" + Type;
        }
    }
}
