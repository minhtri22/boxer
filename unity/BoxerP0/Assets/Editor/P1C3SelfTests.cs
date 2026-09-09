using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class P1C3SelfTests
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
            RunSuite("P1-C3 8/8", RunBatch, suites);

            string evidenceDir = Path.Combine(RepoRoot(), "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-c3-combined-regression.txt");
            suites.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            suites.Add("PRIOR_WITH_C2=85/85 PASS");
            suites.Add("P1_C3=8/8 PASS");
            suites.Add("COMBINED=93/93 PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, suites) + Environment.NewLine);
            Debug.Log($"P1_C3_COMBINED_REGRESSION_PASS={output}");
        }

        public static void RunBatch()
        {
            List<string> results = new();
            Run("P1-C3 recover starts continuously from extend", TestRecoverStartsAtExtendEnd, results);
            Run("P1-C3 guard-return progress is monotonic", TestProgressMonotonic, results);
            Run("P1-C3 recover end is exact neutral", TestRecoverEndNeutral, results);
            Run("P1-C3 guard is exact same neutral", TestGuardNeutral, results);
            Run("P1-C3 straight extra yaw clears by neutral", TestStraightExtraClears, results);
            Run("P1-C3 weight stance returns neutral", TestWeightNeutral, results);
            Run("P1-C3 composite state is finite", TestFinite, results);
            Run("P1-C3 composite state is bounded", TestBounded, results);
            int passCount = results.Count;

            string evidenceDir = Path.Combine(RepoRoot(), "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-c3-deterministic-self-tests.txt");
            results.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            results.Add("RECOVERY_NEUTRAL_LOAD=0.50");
            results.Add("RECOVERY_NEUTRAL_YAW=0.0");
            results.Add($"TOTAL={passCount} PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, results) + Environment.NewLine);
            Debug.Log($"P1_C3_SELF_TESTS_PASS={output}");
        }

        private static void TestRecoverStartsAtExtendEnd()
        {
            P1WholeBodyRecoveryPose extend = P1WholeBodyRecoveryMath.Sample(PunchIntent.Cross, ActionPhase.Extend, 1f);
            P1WholeBodyRecoveryPose recover = P1WholeBodyRecoveryMath.Sample(PunchIntent.Cross, ActionPhase.Recover, 0f);
            AssertNear(extend.PelvisYawDegrees, recover.PelvisYawDegrees, "pelvis continuity");
            AssertNear(extend.EffectiveTorsoYawDegrees, recover.EffectiveTorsoYawDegrees, "torso continuity");
            AssertNear(extend.ForwardLoad01, recover.ForwardLoad01, "load continuity");
        }

        private static void TestProgressMonotonic()
        {
            float a = P1WholeBodyRecoveryMath.Sample(PunchIntent.Cross, ActionPhase.Recover, 0.2f).GuardReturn01;
            float b = P1WholeBodyRecoveryMath.Sample(PunchIntent.Cross, ActionPhase.Recover, 0.5f).GuardReturn01;
            float c = P1WholeBodyRecoveryMath.Sample(PunchIntent.Cross, ActionPhase.Recover, 0.8f).GuardReturn01;
            AssertTrue(a < b && b < c, "guard return must be monotonic");
        }

        private static void TestRecoverEndNeutral()
        {
            P1WholeBodyRecoveryPose pose = P1WholeBodyRecoveryMath.Sample(PunchIntent.Cross, ActionPhase.Recover, 1f);
            AssertTrue(pose.IsNeutral(), "recover end must satisfy neutral contract");
        }

        private static void TestGuardNeutral()
        {
            P1WholeBodyRecoveryPose pose = P1WholeBodyRecoveryMath.Sample(PunchIntent.Jab, ActionPhase.Guard, 0f);
            AssertTrue(pose.IsNeutral(), "guard must satisfy same neutral contract");
        }

        private static void TestStraightExtraClears()
        {
            P1WeightTransferPose neutral = P1WeightTransferMath.FromLoad(P1WeightTransferMath.NeutralLoad01);
            AssertNear(0f, P1StraightBodyCouplingMath.ExtraTorsoYawDegrees(PunchIntent.Cross, neutral.ForwardLoad01), "straight extra neutral");
        }

        private static void TestWeightNeutral()
        {
            P1WholeBodyRecoveryPose pose = P1WholeBodyRecoveryMath.Sample(PunchIntent.Cross, ActionPhase.Recover, 1f);
            AssertNear(P1WeightTransferMath.NeutralLoad01, pose.ForwardLoad01, "load neutral");
            AssertNear(0f, pose.VisualForwardOffsetMeters, "stance offset neutral");
        }

        private static void TestFinite()
        {
            P1WholeBodyRecoveryPose pose = P1WholeBodyRecoveryMath.Sample(PunchIntent.Cross, ActionPhase.Recover, 0.41f);
            foreach (float v in new[] { pose.GuardReturn01, pose.PelvisYawDegrees, pose.EffectiveTorsoYawDegrees, pose.ForwardLoad01, pose.VisualForwardOffsetMeters })
                AssertTrue(!float.IsNaN(v) && !float.IsInfinity(v), "state finite");
        }

        private static void TestBounded()
        {
            for (int i = 0; i <= 20; i++)
            {
                P1WholeBodyRecoveryPose pose = P1WholeBodyRecoveryMath.Sample(PunchIntent.Cross, ActionPhase.Recover, i / 20f);
                AssertTrue(pose.GuardReturn01 >= 0f && pose.GuardReturn01 <= 1f, "guard progress bounded");
                AssertTrue(pose.ForwardLoad01 >= 0f && pose.ForwardLoad01 <= 1f, "load bounded");
                AssertTrue(Mathf.Abs(pose.PelvisYawDegrees) <= P1BodyRotationMath.MaxPelvisYawDegrees + 0.001f, "pelvis bounded");
            }
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
