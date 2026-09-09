using System.Globalization;
using UnityEngine;

namespace BoxerP0
{
    /// <summary>
    /// Snapshot of whole-body state at punch start.
    /// E0 fields remain diagnostic. P1-A1 promotes only categorical step direction into
    /// a small straight-punch reach coupling. P1-A2 adds punch family/hand semantics.
    /// P1-A3.1 activates hook effectiveness versus start-of-punch range.
    /// P1-A3.2 promotes punch-start step state into an uppercut base-readiness proxy that
    /// changes only vertical drive. P1-A3.3 makes forward-committed overhands recover longer.
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

        public float A1StraightReachFactor => P1PunchMechanics.EffectiveStraightReachFactor(Intent, StepState);
        public float A3FamilyReachFactor => P1PunchMechanics.EffectiveA3FamilyReachFactor(Intent, DistanceMeters);
        public string A3Mode => P1PunchMechanics.A3Mode(Intent);
        public string A32Mode => P1PunchMechanics.A32Mode(Intent);
        public string A32BaseState => P1PunchMechanics.A32BaseState(Intent, StepState);
        public float A32UppercutDriveFactor => P1PunchMechanics.EffectiveA32UppercutDriveFactor(Intent, StepState);
        public string A33Mode => P1PunchMechanics.A33Mode(Intent);
        public string A33CommitState => P1PunchMechanics.A33CommitState(Intent, StepState);
        public float A33RecoveryFactor => P1PunchMechanics.EffectiveA33RecoveryFactor(Intent, StepState);

        public string ToSemanticEvent(CombatOutcome outcome, bool counter)
        {
            return string.Join(" ",
                "P1_PUNCH",
                $"TYPE={PunchLabels.EventToken(Intent)}",
                $"FAMILY={PunchLabels.Family(Intent).ToString().ToUpperInvariant()}",
                $"HAND={(PunchLabels.IsRearHand(Intent) ? "REAR" : "LEAD")}",
                $"STEP={StepState}",
                $"DIST={F(DistanceMeters)}",
                $"MOVE_FWD={F(MoveForward)}",
                $"MOVE_LAT={F(MoveLateral)}",
                $"HEAD_DEG={F(HeadDegrees)}",
                $"HEAD_OFF={F(HeadOffsetMeters)}",
                $"RANGE={F(RangeFactor)}",
                $"A1_REACH={F(A1StraightReachFactor)}",
                $"A3_MODE={A3Mode}",
                $"A3_FACTOR={F(A3FamilyReachFactor)}",
                $"A32_MODE={A32Mode}",
                $"A32_BASE={A32BaseState}",
                $"A32_DRIVE={F(A32UppercutDriveFactor)}",
                $"A33_MODE={A33Mode}",
                $"A33_COMMIT={A33CommitState}",
                $"A33_RECOVERY={F(A33RecoveryFactor)}",
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

        // P1-A1: one isolated straight-punch coupling.
        public const float A1AdvancingStraightReach = 1.06f;
        public const float A1NeutralStraightReach = 1.00f;
        public const float A1RetreatingStraightReach = 0.94f;

        // P1-A3.1: first family-specific coupling. Hooks should be reliable at close range and
        // progressively give up forward extension when thrown from outside their natural pocket.
        // These values are frozen for the controlled experiment; do not tune during build verification.
        public const float A3HookFullRangeMeters = 1.05f;
        public const float A3HookFalloffEndMeters = 1.25f;
        public const float A3HookFarReachFactor = 0.86f;

        // P1-A3.2: uppercut base readiness from the already-frozen punch-start StepState.
        // NEUTRAL is the smallest current input-derived planted-base proxy. Translating forward
        // or backward still allows the punch but reduces only commit->target vertical travel.
        public const float A32PlantedUppercutDrive = 1.00f;
        public const float A32MovingUppercutDrive = 0.85f;

        // P1-A3.3: forward-committed overhands expose a longer return to guard.
        // Only Recover duration changes; trajectory and combat geometry remain unchanged.
        public const float A33ForwardCommittedRecoveryFactor = 1.20f;

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
            string stepState = ResolveStepState(forward);

            // E0 continuous diagnostic baseline; still not authoritative gameplay logic.
            float rangeFactor = Mathf.Clamp(1f + forward * 0.12f, 0.88f, 1.12f);

            // Coordination remains diagnostic-only. Do not turn this aggregate score into gameplay.
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

        public static string ResolveStepState(float forward)
        {
            return forward > AdvancingThreshold
                ? "ADVANCING"
                : forward < RetreatingThreshold
                    ? "RETREATING"
                    : "NEUTRAL";
        }

        public static bool IsStraightPunch(PunchIntent intent)
        {
            return PunchLabels.Family(intent) == PunchFamily.Straight;
        }

        public static bool IsHook(PunchIntent intent)
        {
            return PunchLabels.Family(intent) == PunchFamily.Hook;
        }

        public static bool IsUppercut(PunchIntent intent)
        {
            return PunchLabels.Family(intent) == PunchFamily.Uppercut;
        }

        public static bool IsOverhand(PunchIntent intent)
        {
            return PunchLabels.Family(intent) == PunchFamily.Overhand;
        }

        public static float EffectiveStraightReachFactor(PunchIntent intent, string stepState)
        {
            if (!IsStraightPunch(intent)) return 1f;

            return stepState switch
            {
                "ADVANCING" => A1AdvancingStraightReach,
                "RETREATING" => A1RetreatingStraightReach,
                _ => A1NeutralStraightReach
            };
        }

        public static Vector3 ApplyA1StraightReach(PunchIntent intent, Vector3 targetPose, string stepState)
        {
            float factor = EffectiveStraightReachFactor(intent, stepState);
            if (Mathf.Approximately(factor, 1f)) return targetPose;

            // Only forward extension changes. Height/lateral aim, timing, radius, damage and non-straights stay unchanged.
            targetPose.z *= factor;
            return targetPose;
        }

        public static string A3Mode(PunchIntent intent)
        {
            return IsHook(intent) ? "HOOK_RANGE" : "NONE";
        }

        public static float EffectiveA3FamilyReachFactor(PunchIntent intent, float distanceMeters)
        {
            if (!IsHook(intent)) return 1f;

            if (distanceMeters <= A3HookFullRangeMeters) return 1f;
            if (distanceMeters >= A3HookFalloffEndMeters) return A3HookFarReachFactor;

            float t = Mathf.InverseLerp(A3HookFullRangeMeters, A3HookFalloffEndMeters, distanceMeters);
            return Mathf.Lerp(1f, A3HookFarReachFactor, t);
        }

        public static Vector3 ApplyA3FamilyCoupling(PunchIntent intent, Vector3 targetPose, float distanceMeters)
        {
            float factor = EffectiveA3FamilyReachFactor(intent, distanceMeters);
            if (Mathf.Approximately(factor, 1f)) return targetPose;

            // A3.1 changes only hook forward extension. Lateral arc, height, timing, radius,
            // damage, guard logic, stamina and all uppercut/overhand behavior stay unchanged.
            targetPose.z *= factor;
            return targetPose;
        }

        public static string A32Mode(PunchIntent intent)
        {
            return IsUppercut(intent) ? "UPPERCUT_BASE" : "NONE";
        }

        public static string A32BaseState(PunchIntent intent, string stepState)
        {
            if (!IsUppercut(intent)) return "NONE";
            return stepState == "NEUTRAL" ? "PLANTED" : "MOVING";
        }

        public static float EffectiveA32UppercutDriveFactor(PunchIntent intent, string stepState)
        {
            if (!IsUppercut(intent)) return 1f;
            return stepState == "NEUTRAL" ? A32PlantedUppercutDrive : A32MovingUppercutDrive;
        }

        public static Vector3 ApplyA32UppercutDrive(
            PunchIntent intent,
            Vector3 commitPose,
            Vector3 targetPose,
            string stepState)
        {
            float factor = EffectiveA32UppercutDriveFactor(intent, stepState);
            if (!IsUppercut(intent) || Mathf.Approximately(factor, 1f)) return targetPose;

            float rise = targetPose.y - commitPose.y;
            targetPose.y = commitPose.y + rise * factor;
            return targetPose;
        }

        public static string A33Mode(PunchIntent intent)
        {
            return IsOverhand(intent) ? "OVERHAND_RECOVERY" : "NONE";
        }

        public static string A33CommitState(PunchIntent intent, string stepState)
        {
            if (!IsOverhand(intent)) return "NONE";
            return stepState == "ADVANCING" ? "FORWARD_COMMITTED" : "UNCOMMITTED";
        }

        public static float EffectiveA33RecoveryFactor(PunchIntent intent, string stepState)
        {
            return IsOverhand(intent) && stepState == "ADVANCING"
                ? A33ForwardCommittedRecoveryFactor
                : 1f;
        }

        public static float EffectiveA33RecoverySeconds(
            PunchIntent intent,
            string stepState,
            float baseRecoverySeconds)
        {
            return Mathf.Max(0f, baseRecoverySeconds) * EffectiveA33RecoveryFactor(intent, stepState);
        }
    }
}
