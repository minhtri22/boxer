using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class P1C1SelfTests
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
            RunSuite("P1-C1 7/7", RunBatch, suites);

            string repoRoot = RepoRoot();
            string evidenceDir = Path.Combine(repoRoot, "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-c1-combined-regression.txt");
            suites.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            suites.Add("PRIOR_WITH_C0=71/71 PASS");
            suites.Add("P1_C1=7/7 PASS");
            suites.Add("COMBINED=78/78 PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, suites) + Environment.NewLine);
            Debug.Log($"P1_C1_COMBINED_REGRESSION_PASS={output}");
        }

        public static void RunBatch()
        {
            List<string> results = new();
            Run("P1-C1 guard load is neutral", TestGuardNeutral, results);
            Run("P1-C1 commit preloads rearward", TestCommitPreload, results);
            Run("P1-C1 extend drives forward continuously", TestExtendDrive, results);
            Run("P1-C1 recovery returns neutral continuously", TestRecovery, results);
            Run("P1-C1 state is family independent", TestFamilyIndependent, results);
            Run("P1-C1 state is hand independent", TestHandIndependent, results);
            Run("P1-C1 offset is finite and bounded", TestFiniteBoundedOffset, results);
            int passCount = results.Count;

            string evidenceDir = Path.Combine(RepoRoot(), "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-c1-deterministic-self-tests.txt");
            results.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            results.Add($"NEUTRAL_LOAD={F(P1WeightTransferMath.NeutralLoad01)}");
            results.Add($"COMMIT_END_LOAD={F(P1WeightTransferMath.CommitEndLoad01)}");
            results.Add($"EXTEND_END_LOAD={F(P1WeightTransferMath.ExtendEndLoad01)}");
            results.Add($"TOTAL={passCount} PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, results) + Environment.NewLine);
            Debug.Log($"P1_C1_SELF_TESTS_PASS={output}");
        }

        private static void TestGuardNeutral()
        {
            P1WeightTransferPose pose = P1WeightTransferMath.Sample(PunchIntent.Cross, ActionPhase.Guard, 1f);
            AssertNear(P1WeightTransferMath.NeutralLoad01, pose.ForwardLoad01, "guard load");
            AssertNear(0f, pose.VisualForwardOffsetMeters, "guard offset");
        }

        private static void TestCommitPreload()
        {
            P1WeightTransferPose start = P1WeightTransferMath.Sample(PunchIntent.Cross, ActionPhase.Commit, 0f);
            P1WeightTransferPose end = P1WeightTransferMath.Sample(PunchIntent.Cross, ActionPhase.Commit, 1f);
            AssertNear(P1WeightTransferMath.NeutralLoad01, start.ForwardLoad01, "commit start");
            AssertNear(P1WeightTransferMath.CommitEndLoad01, end.ForwardLoad01, "commit end");
            AssertTrue(end.ForwardLoad01 < P1WeightTransferMath.NeutralLoad01, "commit must preload rearward");
        }

        private static void TestExtendDrive()
        {
            P1WeightTransferPose start = P1WeightTransferMath.Sample(PunchIntent.Cross, ActionPhase.Extend, 0f);
            P1WeightTransferPose end = P1WeightTransferMath.Sample(PunchIntent.Cross, ActionPhase.Extend, 1f);
            AssertNear(P1WeightTransferMath.CommitEndLoad01, start.ForwardLoad01, "extend continuity");
            AssertNear(P1WeightTransferMath.ExtendEndLoad01, end.ForwardLoad01, "extend end");
            AssertTrue(end.ForwardLoad01 > P1WeightTransferMath.NeutralLoad01, "extend must drive forward");
        }

        private static void TestRecovery()
        {
            P1WeightTransferPose start = P1WeightTransferMath.Sample(PunchIntent.Cross, ActionPhase.Recover, 0f);
            P1WeightTransferPose end = P1WeightTransferMath.Sample(PunchIntent.Cross, ActionPhase.Recover, 1f);
            AssertNear(P1WeightTransferMath.ExtendEndLoad01, start.ForwardLoad01, "recover continuity");
            AssertNear(P1WeightTransferMath.NeutralLoad01, end.ForwardLoad01, "recover neutral");
            AssertNear(0f, end.VisualForwardOffsetMeters, "recover offset");
        }

        private static void TestFamilyIndependent()
        {
            float straight = P1WeightTransferMath.Sample(PunchIntent.Cross, ActionPhase.Extend, 0.6f).ForwardLoad01;
            float hook = P1WeightTransferMath.Sample(PunchIntent.RearHook, ActionPhase.Extend, 0.6f).ForwardLoad01;
            AssertNear(straight, hook, "family independence");
        }

        private static void TestHandIndependent()
        {
            float lead = P1WeightTransferMath.Sample(PunchIntent.Jab, ActionPhase.Commit, 0.7f).ForwardLoad01;
            float rear = P1WeightTransferMath.Sample(PunchIntent.Cross, ActionPhase.Commit, 0.7f).ForwardLoad01;
            AssertNear(lead, rear, "hand independence");
        }

        private static void TestFiniteBoundedOffset()
        {
            P1WeightTransferPose preload = P1WeightTransferMath.FromLoad(P1WeightTransferMath.CommitEndLoad01);
            P1WeightTransferPose drive = P1WeightTransferMath.FromLoad(P1WeightTransferMath.ExtendEndLoad01);
            AssertTrue(IsFinite(preload.VisualForwardOffsetMeters) && IsFinite(drive.VisualForwardOffsetMeters), "offset finite");
            AssertNear(-0.02f, preload.VisualForwardOffsetMeters, "preload offset", 0.0001f);
            AssertNear(0.03f, drive.VisualForwardOffsetMeters, "drive offset", 0.0001f);
            AssertTrue(preload.ForwardLoad01 >= 0f && drive.ForwardLoad01 <= 1f, "load bounds");
        }

        private static string RepoRoot() => Directory.GetParent(Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName).FullName;
        private static string F(float value) => value.ToString("F2", CultureInfo.InvariantCulture);
        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private static void RunSuite(string name, Action suite, ICollection<string> results) { suite(); results.Add("PASS " + name); }
        private static void Run(string name, Action test, ICollection<string> results) { test(); results.Add("PASS " + name); }
        private static void AssertNear(float expected, float actual, string message, float tolerance = 0.00001f)
        {
            if (Mathf.Abs(expected - actual) > tolerance) throw new Exception($"{message}: expected {expected:F6}, got {actual:F6}");
        }
        private static void AssertTrue(bool condition, string message) { if (!condition) throw new Exception(message); }
    }
}
