using System.Globalization;
using UnityEngine;

namespace BoxerP0
{
    /// <summary>
    /// P1-E0 diagnostic-only snapshot of whole-body state at punch start.
    /// This model MUST NOT alter combat outcomes until a later P1 experiment explicitly promotes it.
    /// </summary>
    public readonly struct P1PunchSnapshot
    {
        public readonly float Timestamp;
        public readonly PunchIntent Intent;
        public readonly Vector3 PlayerPosition;
        public readonly Vector3 OpponentPosition;
        public readonly float DistanceMeters;
        public readonly float MoveForward;
        public readonly float MoveLateral;
        public readonly float HeadDegrees;
        public readonly float HeadOffsetMeters;
        public readonly string StepState;
        public readonly float RangeFactor;
        public readonly float CoordinationScore;

        public P1PunchSnapshot(
            float timestamp,
            PunchIntent intent,
            Vector3 playerPosition,
            Vector3 opponentPosition,
            float distanceMeters,
            float moveForward,
            float moveLateral,
            float headDegrees,
            float headOffsetMeters,
            string stepState,
            float rangeFactor,
            float coordinationScore)
        {
            Timestamp = timestamp;
            Intent = intent;
            PlayerPosition = playerPosition;
            OpponentPosition = opponentPosition;
            DistanceMeters = distanceMeters;
            MoveForward = moveForward;
            MoveLateral = moveLateral;
            HeadDegrees = headDegrees;
            HeadOffsetMeters = headOffsetMeters;
            StepState = stepState;
            RangeFactor = rangeFactor;
            CoordinationScore = coordinationScore;
        }

        public string ToSemanticEvent(CombatOutcome outcome, bool counter)
        {
            return string.Join(" ",
                "P1_PUNCH",
                $"TYPE={PunchLabels.EventToken(Intent)}",
                $"STEP={StepState}",
                $"DIST={F(DistanceMeters)}",
                $"MOVE_FWD={F(MoveForward)}",
                $"MOVE_LAT={F(MoveLateral)}",
                $"HEAD_DEG={F(HeadDegrees)}",
                $"HEAD_OFF={F(HeadOffsetMeters)}",
                $"RANGE={F(RangeFactor)}",
                $"COORD={F(CoordinationScore)}",
                $"OUTCOME={outcome.ToString().ToUpperInvariant()}",
                $"COUNTER={(counter ? 1 : 0)}");
        }

        private static string F(float value) => value.ToString("F3", CultureInfo.InvariantCulture);
    }

    public static class P1PunchMechanics
    {
        private const float AdvancingThreshold = 0.20f;
        private const float RetreatingThreshold = -0.20f;

        public static P1PunchSnapshot Capture(
            PunchIntent intent,
            Vector3 playerPosition,
            Vector3 opponentPosition,
            Vector2 movementIntent,
            float headDegrees,
            float headOffsetMeters)
        {
            Vector3 planar = opponentPosition - playerPosition;
            planar.y = 0f;
            float distance = planar.magnitude;

            float forward = Mathf.Clamp(movementIntent.y, -1f, 1f);
            float lateral = Mathf.Clamp(movementIntent.x, -1f, 1f);
            string stepState = forward > AdvancingThreshold
                ? "ADVANCING"
                : forward < RetreatingThreshold
                    ? "RETREATING"
                    : "NEUTRAL";

            // P1-E0 diagnostic baseline only. This does not modify punch range yet.
            float rangeFactor = Mathf.Clamp(1f + forward * 0.12f, 0.88f, 1.12f);

            // Coordination is deliberately simple/falsifiable for E0 instrumentation.
            float score = 0.72f;
            score += Mathf.Max(0f, forward) * 0.16f;
            score -= Mathf.Max(0f, -forward) * 0.12f;
            score -= Mathf.Abs(lateral) * 0.18f;
            score -= Mathf.Clamp01(Mathf.Abs(headOffsetMeters) / 0.28f) * 0.16f;
            float coordination = Mathf.Clamp01(score);

            return new P1PunchSnapshot(
                Time.unscaledTime,
                intent,
                playerPosition,
                opponentPosition,
                distance,
                forward,
                lateral,
                headDegrees,
                headOffsetMeters,
                stepState,
                rangeFactor,
                coordination);
        }
    }
}
