using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class P1C2SelfTests
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
            RunSuite("P1-C2 7/7", RunBatch, suites);

            string evidenceDir = Path.Combine(RepoRoot(), "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-c2-combined-regression.txt");
            suites.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            suites.Add("PRIOR_WITH_C1=78/78 PASS");
            suites.Add("P1_C2=7/7 PASS");
            suites.Add("COMBINED=85/85 PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, suites) + Environment.NewLine);
            Debug.Log($"P1_C2_COMBINED_REGRESSION_PASS={output}");
        }

        public static void RunBatch()
        {
            List<string> results = new();
            Run("P1-C2 non-straights receive zero extra yaw", TestNonStraightZero, results);
            Run("P1-C2 neutral and preload receive zero extra yaw", TestNeutralPreloadZero, results);
            Run("P1-C2 straight drive reaches frozen extra yaw", TestMaxStraightDrive, results);
            Run("P1-C2 lead rear straights are equal opposite", TestLeadRearOpposite, results);
            Run("P1-C2 pelvis baseline remains unchanged", TestPelvisUnchanged, results);
            Run("P1-C2 extra yaw is monotonic", TestMonotonic, results);
            Run("P1-C2 extra yaw is finite and bounded", TestFiniteBounded, results);
            int passCount = results.Count;

            string evidenceDir = Path.Combine(RepoRoot(), "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-c2-deterministic-self-tests.txt");
            results.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            results.Add($"MAX_EXTRA_TORSO_YAW_DEG={P1StraightBodyCouplingMath.MaxExtraTorsoYawDegrees.ToString("F1", CultureInfo.InvariantCulture)}");
            results.Add($"TOTAL={passCount} PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, results) + Environment.NewLine);
            Debug.Log($"P1_C2_SELF_TESTS_PASS={output}");
        }

        private static void TestNonStraightZero()
        {
            AssertNear(0f, P1StraightBodyCouplingMath.ExtraTorsoYawDegrees(PunchIntent.RearHook, 0.62f), "hook");
            AssertNear(0f, P1StraightBodyCouplingMath.ExtraTorsoYawDegrees(PunchIntent.RearUppercut, 0.62f), "uppercut");
            AssertNear(0f, P1StraightBodyCouplingMath.ExtraTorsoYawDegrees(PunchIntent.RearOverhand, 0.62f), "overhand");
        }

        private static void TestNeutralPreloadZero()
        {
            AssertNear(0f, P1StraightBodyCouplingMath.ExtraTorsoYawDegrees(PunchIntent.Cross, 0.50f), "neutral");
            AssertNear(0f, P1StraightBodyCouplingMath.ExtraTorsoYawDegrees(PunchIntent.Cross, 0.42f), "preload");
        }

        private static void TestMaxStraightDrive()
        {
            float extra = P1StraightBodyCouplingMath.ExtraTorsoYawDegrees(PunchIntent.Cross, P1WeightTransferMath.ExtendEndLoad01);
            AssertNear(P1StraightBodyCouplingMath.MaxExtraTorsoYawDegrees, extra, "rear max");
        }

        private static void TestLeadRearOpposite()
        {
            float lead = P1StraightBodyCouplingMath.ExtraTorsoYawDegrees(PunchIntent.Jab, 0.62f);
            float rear = P1StraightBodyCouplingMath.ExtraTorsoYawDegrees(PunchIntent.Cross, 0.62f);
            AssertNear(-lead, rear, "lead/rear symmetry");
        }

        private static void TestPelvisUnchanged()
        {
            P1BodyRotationPose basePose = P1BodyRotationMath.Sample(PunchIntent.Cross, ActionPhase.Extend, 1f);
            P1WeightTransferPose weight = P1WeightTransferMath.Sample(PunchIntent.Cross, ActionPhase.Extend, 1f);
            float effectiveTorso = P1StraightBodyCouplingMath.EffectiveTorsoYawDegrees(PunchIntent.Cross, basePose, weight);
            AssertNear(P1BodyRotationMath.MaxPelvisYawDegrees, basePose.PelvisYawDegrees, "pelvis baseline");
            AssertTrue(effectiveTorso > basePose.TorsoYawDegrees, "torso must gain straight drive while pelvis stays base");
        }

        private static void TestMonotonic()
        {
            float a = Mathf.Abs(P1StraightBodyCouplingMath.ExtraTorsoYawDegrees(PunchIntent.Cross, 0.52f));
            float b = Mathf.Abs(P1StraightBodyCouplingMath.ExtraTorsoYawDegrees(PunchIntent.Cross, 0.57f));
            float c = Mathf.Abs(P1StraightBodyCouplingMath.ExtraTorsoYawDegrees(PunchIntent.Cross, 0.62f));
            AssertTrue(a < b && b < c, "extra yaw must increase with drive load");
        }

        private static void TestFiniteBounded()
        {
            for (int i = 0; i <= 20; i++)
            {
                float load = i / 20f;
                float value = P1StraightBodyCouplingMath.ExtraTorsoYawDegrees(PunchIntent.Cross, load);
                AssertTrue(!float.IsNaN(value) && !float.IsInfinity(value), "finite");
                AssertTrue(Mathf.Abs(value) <= P1StraightBodyCouplingMath.MaxExtraTorsoYawDegrees + 0.0001f, "bounded");
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
