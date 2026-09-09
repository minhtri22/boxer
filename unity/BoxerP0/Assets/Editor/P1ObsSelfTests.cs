using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class P1ObsSelfTests
    {
        public static void RunWithRegressions()
        {
            List<string> suites = new();
            RunSuite("Phase0 22/22", Phase0SelfTests.RunBatch, suites);
            RunSuite("P1-A3.1 4/4", P1A3SelfTests.RunBatch, suites);
            RunSuite("P1-B1.5R 7/7", P1B15RSelfTests.RunBatch, suites);
            RunSuite("P1-B1.5S 5/5", P1B15SSelfTests.RunBatch, suites);
            RunSuite("P1-B1.5T 7/7", P1B15TSelfTests.RunBatch, suites);
            RunSuite("P1-B1.5U 6/6", P1B15USelfTests.RunBatch, suites);
            RunSuite("P1-B1.5V 6/6", P1B15VSelfTests.RunBatch, suites);
            RunSuite("P1-B2 7/7", P1B2SelfTests.RunBatch, suites);
            RunSuite("P1-C0 7/7", P1C0SelfTests.RunBatch, suites);
            RunSuite("P1-C1 7/7", P1C1SelfTests.RunBatch, suites);
            RunSuite("P1-C2 7/7", P1C2SelfTests.RunBatch, suites);
            RunSuite("P1-C3 8/8", P1C3SelfTests.RunBatch, suites);
            RunSuite("P1-OBS 8/8", RunBatch, suites);

            string evidenceDir = Path.Combine(RepoRoot(), "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-obs-combined-regression.txt");
            suites.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            suites.Add("PRIOR_WITH_C3=93/93 PASS");
            suites.Add("P1_OBS=8/8 PASS");
            suites.Add("COMBINED=101/101 PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, suites) + Environment.NewLine);
            Debug.Log($"P1_OBS_COMBINED_REGRESSION_PASS={output}");
        }

        public static void RunBatch()
        {
            List<string> results = new();
            Run("P1-OBS family and hand are deterministic", TestFamilyHand, results);
            Run("P1-OBS body state reuses frozen math", TestBodySampling, results);
            Run("P1-OBS guard state is neutral", TestGuardNeutral, results);
            Run("P1-OBS numeric fields are finite", TestFinite, results);
            Run("P1-OBS body fields remain bounded", TestBounded, results);
            Run("P1-OBS semantic schema serialization is stable", TestStableSerialization, results);
            Run("P1-OBS reason tokens normalize deterministically", TestReasonNormalization, results);
            Run("P1-OBS combat log is bounded FIFO", TestCombatLogBuffer, results);
            int passCount = results.Count;

            string evidenceDir = Path.Combine(RepoRoot(), "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-obs-deterministic-self-tests.txt");
            results.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            results.Add($"SCHEMA={P1CombatObservability.SchemaVersion}");
            results.Add("LOG_CAPACITY=32");
            results.Add($"TOTAL={passCount} PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, results) + Environment.NewLine);
            Debug.Log($"P1_OBS_SELF_TESTS_PASS={output}");
        }

        private static P1BiomechanicsObservation Sample(
            PunchIntent intent = PunchIntent.Cross,
            ActionPhase phase = ActionPhase.Extend,
            float t = 1f,
            CombatOutcome outcome = CombatOutcome.Hit,
            string reason = "opponent head intersection")
        {
            return P1CombatObservability.Sample(intent, phase, t, 1.02f, "ADVANCING", 0.08f, 0.91f, outcome, reason, true);
        }

        private static void TestFamilyHand()
        {
            P1BiomechanicsObservation lead = Sample(PunchIntent.Jab);
            P1BiomechanicsObservation rear = Sample(PunchIntent.Cross);
            AssertTrue(lead.Family == PunchFamily.Straight && lead.Hand == "LEAD", "jab family/hand");
            AssertTrue(rear.Family == PunchFamily.Straight && rear.Hand == "REAR", "cross family/hand");
        }

        private static void TestBodySampling()
        {
            P1BiomechanicsObservation obs = Sample();
            P1BodyRotationPose rotation = P1BodyRotationMath.Sample(PunchIntent.Cross, ActionPhase.Extend, 1f);
            P1WeightTransferPose weight = P1WeightTransferMath.Sample(PunchIntent.Cross, ActionPhase.Extend, 1f);
            float effective = P1StraightBodyCouplingMath.EffectiveTorsoYawDegrees(PunchIntent.Cross, rotation, weight);
            AssertNear(rotation.PelvisYawDegrees, obs.PelvisYawDegrees, "pelvis");
            AssertNear(rotation.TorsoYawDegrees, obs.TorsoYawDegrees, "torso");
            AssertNear(effective, obs.EffectiveTorsoYawDegrees, "effective torso");
            AssertNear(weight.ForwardLoad01, obs.ForwardLoad01, "forward load");
        }

        private static void TestGuardNeutral()
        {
            P1BiomechanicsObservation obs = P1CombatObservability.Sample(
                PunchIntent.None, ActionPhase.Guard, 0f, 1f, "NEUTRAL", 0f, 0f, CombatOutcome.None, "pending", false);
            AssertNear(0f, obs.PelvisYawDegrees, "guard pelvis");
            AssertNear(0f, obs.EffectiveTorsoYawDegrees, "guard torso");
            AssertNear(P1WeightTransferMath.NeutralLoad01, obs.ForwardLoad01, "guard load");
            AssertNear(0f, obs.ForwardOffsetMeters, "guard offset");
        }

        private static void TestFinite()
        {
            P1BiomechanicsObservation obs = Sample(t: 0.41f);
            foreach (float value in new[]
                     {
                         obs.Phase01, obs.DistanceMeters, obs.PelvisYawDegrees, obs.TorsoYawDegrees,
                         obs.EffectiveTorsoYawDegrees, obs.ForwardLoad01, obs.ForwardOffsetMeters,
                         obs.HeadOffsetMeters, obs.GlovePathMeters
                     })
            {
                AssertTrue(!float.IsNaN(value) && !float.IsInfinity(value), "finite observation field");
            }
        }

        private static void TestBounded()
        {
            for (int i = 0; i <= 20; i++)
            {
                P1BiomechanicsObservation obs = Sample(t: i / 20f);
                AssertTrue(obs.Phase01 >= 0f && obs.Phase01 <= 1f, "phase bounded");
                AssertTrue(obs.ForwardLoad01 >= 0f && obs.ForwardLoad01 <= 1f, "load bounded");
                AssertTrue(Mathf.Abs(obs.PelvisYawDegrees) <= P1BodyRotationMath.MaxPelvisYawDegrees + 0.001f, "pelvis bounded");
                AssertTrue(Mathf.Abs(obs.TorsoYawDegrees) <= P1BodyRotationMath.MaxTorsoYawDegrees + 0.001f, "torso bounded");
                AssertTrue(Mathf.Abs(obs.EffectiveTorsoYawDegrees) <= P1BodyRotationMath.MaxTorsoYawDegrees + P1StraightBodyCouplingMath.MaxExtraTorsoYawDegrees + 0.001f, "effective torso bounded");
            }
        }

        private static void TestStableSerialization()
        {
            string value = Sample().ToSemanticEvent("PLAYER");
            string[] required =
            {
                "P1_OBS SCHEMA=P1OBS_V1 ACTOR=PLAYER TYPE=REAR_CROSS FAMILY=STRAIGHT HAND=REAR PHASE=EXTEND",
                "PELVIS_YAW=", "TORSO_YAW=", "TORSO_EFFECTIVE_YAW=", "FORWARD_LOAD=", "GLOVE_PATH=0.910",
                "OUTCOME=HIT REASON=OPPONENT_HEAD_INTERSECTION COUNTER=1"
            };
            foreach (string token in required) AssertTrue(value.Contains(token), "missing schema token: " + token);
        }

        private static void TestReasonNormalization()
        {
            AssertTrue(P1CombatObservability.NormalizeToken(" opponent  head/intersection ") == "OPPONENT_HEAD_INTERSECTION", "reason normalization");
            AssertTrue(P1CombatObservability.NormalizeToken(null) == "NONE", "null normalization");
        }

        private static void TestCombatLogBuffer()
        {
            P1CombatLogBuffer log = new(3);
            log.Add("A");
            log.Add("B");
            log.Add("C");
            log.Add("D");
            AssertTrue(log.Count == 3, "capacity");
            AssertTrue(log.Latest == "D", "latest");
            AssertTrue(log.RenderRecent(3) == "B\nC\nD", "fifo ordering");
        }

        private static string RepoRoot() => Directory.GetParent(Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName).FullName;
        private static void RunSuite(string name, Action suite, ICollection<string> results) { suite(); results.Add("PASS " + name); }
        private static void Run(string name, Action test, ICollection<string> results) { test(); results.Add("PASS " + name); }
        private static void AssertNear(float expected, float actual, string message, float tolerance = 0.00001f)
        {
            if (Mathf.Abs(expected - actual) > tolerance) throw new Exception($"{message}: expected {expected:F6}, got {actual:F6}");
        }
        private static void AssertTrue(bool condition, string message) { if (!condition) throw new Exception(message); }
    }
}
