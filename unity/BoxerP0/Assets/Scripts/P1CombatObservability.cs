using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace BoxerP0
{
    public readonly struct P1BiomechanicsObservation
    {
        public readonly PunchIntent Intent;
        public readonly PunchFamily Family;
        public readonly ActionPhase Phase;
        public readonly float Phase01;
        public readonly float DistanceMeters;
        public readonly string StepState;
        public readonly float PelvisYawDegrees;
        public readonly float TorsoYawDegrees;
        public readonly float EffectiveTorsoYawDegrees;
        public readonly float ForwardLoad01;
        public readonly float ForwardOffsetMeters;
        public readonly float HeadOffsetMeters;
        public readonly float GlovePathMeters;
        public readonly CombatOutcome Outcome;
        public readonly string Reason;
        public readonly bool Counter;

        public P1BiomechanicsObservation(
            PunchIntent intent,
            PunchFamily family,
            ActionPhase phase,
            float phase01,
            float distanceMeters,
            string stepState,
            float pelvisYawDegrees,
            float torsoYawDegrees,
            float effectiveTorsoYawDegrees,
            float forwardLoad01,
            float forwardOffsetMeters,
            float headOffsetMeters,
            float glovePathMeters,
            CombatOutcome outcome,
            string reason,
            bool counter)
        {
            Intent = intent;
            Family = family;
            Phase = phase;
            Phase01 = phase01;
            DistanceMeters = distanceMeters;
            StepState = stepState;
            PelvisYawDegrees = pelvisYawDegrees;
            TorsoYawDegrees = torsoYawDegrees;
            EffectiveTorsoYawDegrees = effectiveTorsoYawDegrees;
            ForwardLoad01 = forwardLoad01;
            ForwardOffsetMeters = forwardOffsetMeters;
            HeadOffsetMeters = headOffsetMeters;
            GlovePathMeters = glovePathMeters;
            Outcome = outcome;
            Reason = reason;
            Counter = counter;
        }

        public string Hand => Intent == PunchIntent.None
            ? "NONE"
            : PunchLabels.IsRearHand(Intent) ? "REAR" : "LEAD";

        public string ToSemanticEvent(string actor)
        {
            return string.Join(" ",
                "P1_OBS",
                $"SCHEMA={P1CombatObservability.SchemaVersion}",
                $"ACTOR={P1CombatObservability.NormalizeToken(actor)}",
                $"TYPE={PunchLabels.EventToken(Intent)}",
                $"FAMILY={Family.ToString().ToUpperInvariant()}",
                $"HAND={Hand}",
                $"PHASE={Phase.ToString().ToUpperInvariant()}",
                $"PHASE_T={F(Phase01)}",
                $"DIST={F(DistanceMeters)}",
                $"STEP={P1CombatObservability.NormalizeToken(StepState)}",
                $"PELVIS_YAW={F(PelvisYawDegrees)}",
                $"TORSO_YAW={F(TorsoYawDegrees)}",
                $"TORSO_EFFECTIVE_YAW={F(EffectiveTorsoYawDegrees)}",
                $"FORWARD_LOAD={F(ForwardLoad01)}",
                $"FORWARD_OFF={F(ForwardOffsetMeters)}",
                $"HEAD_OFF={F(HeadOffsetMeters)}",
                $"GLOVE_PATH={F(GlovePathMeters)}",
                $"OUTCOME={Outcome.ToString().ToUpperInvariant()}",
                $"REASON={Reason}",
                $"COUNTER={(Counter ? 1 : 0)}");
        }

        public string ToInspectorText()
        {
            return
                $"BIO {P1CombatObservability.SchemaVersion} {Family.ToString().ToUpperInvariant()}/{Hand} {Phase} t={Phase01:F2}\n" +
                $"DIST {DistanceMeters:F2}m STEP {StepState} HEAD {HeadOffsetMeters:F2}m GLOVE_PATH {GlovePathMeters:F2}m\n" +
                $"PELVIS {PelvisYawDegrees:F1}° TORSO {TorsoYawDegrees:F1}°→{EffectiveTorsoYawDegrees:F1}° LOAD {ForwardLoad01:F2} OFF {ForwardOffsetMeters:F3}m\n" +
                $"OUT {Outcome.ToString().ToUpperInvariant()} REASON {Reason} COUNTER {(Counter ? "YES" : "NO")}";
        }

        private static string F(float value) => value.ToString("F3", CultureInfo.InvariantCulture);
    }

    public static class P1CombatObservability
    {
        public const string SchemaVersion = "P1OBS_V1";

        public static P1BiomechanicsObservation Sample(
            PunchIntent intent,
            ActionPhase phase,
            float normalizedPhase,
            float distanceMeters,
            string stepState,
            float headOffsetMeters,
            float glovePathMeters,
            CombatOutcome outcome,
            string reason,
            bool counter)
        {
            float phase01 = Mathf.Clamp01(FiniteOrZero(normalizedPhase));
            P1BodyRotationPose rotation = P1BodyRotationMath.Sample(intent, phase, phase01);
            P1WeightTransferPose weight = P1WeightTransferMath.Sample(intent, phase, phase01);
            float effectiveTorso = P1StraightBodyCouplingMath.EffectiveTorsoYawDegrees(intent, rotation, weight);

            return new P1BiomechanicsObservation(
                intent,
                PunchLabels.Family(intent),
                phase,
                phase01,
                Mathf.Max(0f, FiniteOrZero(distanceMeters)),
                NormalizeToken(stepState),
                FiniteOrZero(rotation.PelvisYawDegrees),
                FiniteOrZero(rotation.TorsoYawDegrees),
                FiniteOrZero(effectiveTorso),
                Mathf.Clamp01(FiniteOrZero(weight.ForwardLoad01)),
                FiniteOrZero(weight.VisualForwardOffsetMeters),
                FiniteOrZero(headOffsetMeters),
                Mathf.Max(0f, FiniteOrZero(glovePathMeters)),
                outcome,
                NormalizeToken(reason),
                counter);
        }

        public static string NormalizeToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "NONE";

            StringBuilder builder = new(value.Length);
            bool previousUnderscore = false;
            foreach (char raw in value.Trim().ToUpperInvariant())
            {
                char c = char.IsLetterOrDigit(raw) ? raw : '_';
                if (c == '_')
                {
                    if (previousUnderscore) continue;
                    previousUnderscore = true;
                }
                else
                {
                    previousUnderscore = false;
                }
                builder.Append(c);
            }

            string token = builder.ToString().Trim('_');
            return token.Length == 0 ? "NONE" : token;
        }

        private static float FiniteOrZero(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
        }
    }

    public sealed class P1CombatLogBuffer
    {
        private readonly int _capacity;
        private readonly Queue<string> _lines = new();

        public P1CombatLogBuffer(int capacity)
        {
            _capacity = Math.Max(1, capacity);
        }

        public int Count => _lines.Count;
        public string Latest { get; private set; } = "NONE";

        public void Add(string value)
        {
            string line = string.IsNullOrWhiteSpace(value) ? "NONE" : value.Trim();
            while (_lines.Count >= _capacity) _lines.Dequeue();
            _lines.Enqueue(line);
            Latest = line;
        }

        public void Clear()
        {
            _lines.Clear();
            Latest = "NONE";
        }

        public string RenderRecent(int maxLines)
        {
            if (_lines.Count == 0 || maxLines <= 0) return "NONE";
            string[] all = _lines.ToArray();
            int start = Math.Max(0, all.Length - maxLines);
            StringBuilder builder = new();
            for (int i = start; i < all.Length; i++)
            {
                if (builder.Length > 0) builder.Append('\n');
                builder.Append(all[i]);
            }
            return builder.ToString();
        }
    }
}
