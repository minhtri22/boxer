using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class P1DOpponentAttributesSelfTests
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
            RunSuite("P1-OBS 8/8", P1ObsSelfTests.RunBatch, suites);
            RunSuite("P1-A3.2 7/7", P1A32SelfTests.RunBatch, suites);
            RunSuite("P1-A3.3 7/7", P1A33SelfTests.RunBatch, suites);
            RunSuite("P1-CG 8/8", P1CounterGeometrySelfTests.RunBatch, suites);
            RunSuite("P1-D 8/8", RunBatch, suites);

            string evidenceDir = Path.Combine(RepoRoot(), "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-d-opponent-attributes-combined-regression.txt");
            suites.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            suites.Add("PRIOR_WITH_COUNTER_GEOMETRY=123/123 PASS");
            suites.Add("P1_D=8/8 PASS");
            suites.Add("COMBINED=131/131 PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, suites) + Environment.NewLine);
            Debug.Log($"P1_D_COMBINED_REGRESSION_PASS={output}");
        }

        public static void RunBatch()
        {
            List<string> results = new();
            Run("P1-D balanced exactly preserves baseline", TestBalancedBaseline, results);
            Run("P1-D long reach changes reach only", TestLongReachIsolation, results);
            Run("P1-D pressure changes attack gap only", TestPressureIsolation, results);
            Run("P1-D fast hands changes phase duration only", TestFastHandsIsolation, results);
            Run("P1-D tactical variables are monotonic", TestMonotonicEffects, results);
            Run("P1-D factors remain bounded", TestBounds, results);
            Run("P1-D profile parser is stable", TestParser, results);
            Run("P1-D inspector schema is stable", TestInspectorSchema, results);
            int passCount = results.Count;

            string evidenceDir = Path.Combine(RepoRoot(), "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-d-opponent-attributes-deterministic-self-tests.txt");
            results.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            results.Add($"LONG_REACH_FACTOR={F(P1OpponentAttributes.LongReachFactor)}");
            results.Add($"PRESSURE_GAP_FACTOR={F(P1OpponentAttributes.PressureGapFactor)}");
            results.Add($"FAST_HANDS_DURATION_FACTOR={F(P1OpponentAttributes.FastHandsDurationFactor)}");
            results.Add($"TOTAL={passCount} PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, results) + Environment.NewLine);
            Debug.Log($"P1_D_SELF_TESTS_PASS={output}");
        }

        private static void TestBalancedBaseline()
        {
            P1OpponentAttributeSet value = P1OpponentAttributes.Resolve(P1OpponentProfile.Balanced);
            AssertNear(1.02f, value.HeadReachMeters);
            AssertNear(0.96f, value.BodyReachMeters);
            AssertNear(0.65f, value.AttackGapMinSeconds);
            AssertNear(1.20f, value.AttackGapMaxSeconds);
            AssertNear(0.34f, value.CommitSeconds);
            AssertNear(0.17f, value.ExtendSeconds);
            AssertNear(0.48f, value.RecoverSeconds);
        }

        private static void TestLongReachIsolation()
        {
            P1OpponentAttributeSet value = P1OpponentAttributes.Resolve(P1OpponentProfile.LongReach);
            AssertNear(1.1016f, value.HeadReachMeters);
            AssertNear(1.0368f, value.BodyReachMeters);
            AssertNear(0.65f, value.AttackGapMinSeconds);
            AssertNear(0.34f, value.CommitSeconds);
            AssertNear(0.17f, value.ExtendSeconds);
            AssertNear(0.48f, value.RecoverSeconds);
        }

        private static void TestPressureIsolation()
        {
            P1OpponentAttributeSet value = P1OpponentAttributes.Resolve(P1OpponentProfile.Pressure);
            AssertNear(1.02f, value.HeadReachMeters);
            AssertNear(0.96f, value.BodyReachMeters);
            AssertNear(0.52f, value.AttackGapMinSeconds);
            AssertNear(0.96f, value.AttackGapMaxSeconds);
            AssertNear(0.34f, value.CommitSeconds);
            AssertNear(0.17f, value.ExtendSeconds);
            AssertNear(0.48f, value.RecoverSeconds);
        }

        private static void TestFastHandsIsolation()
        {
            P1OpponentAttributeSet value = P1OpponentAttributes.Resolve(P1OpponentProfile.FastHands);
            AssertNear(1.02f, value.HeadReachMeters);
            AssertNear(0.65f, value.AttackGapMinSeconds);
            AssertNear(1.20f, value.AttackGapMaxSeconds);
            AssertNear(0.306f, value.CommitSeconds);
            AssertNear(0.153f, value.ExtendSeconds);
            AssertNear(0.432f, value.RecoverSeconds);
        }

        private static void TestMonotonicEffects()
        {
            P1OpponentAttributeSet balanced = P1OpponentAttributes.Resolve(P1OpponentProfile.Balanced);
            AssertTrue(P1OpponentAttributes.Resolve(P1OpponentProfile.LongReach).HeadReachMeters > balanced.HeadReachMeters, "reach must increase");
            AssertTrue(P1OpponentAttributes.Resolve(P1OpponentProfile.Pressure).AttackGapMaxSeconds < balanced.AttackGapMaxSeconds, "gap must shrink");
            AssertTrue(P1OpponentAttributes.Resolve(P1OpponentProfile.FastHands).CommitSeconds < balanced.CommitSeconds, "phase duration must shrink");
        }

        private static void TestBounds()
        {
            foreach (P1OpponentProfile profile in Enum.GetValues(typeof(P1OpponentProfile)))
            {
                P1OpponentAttributeSet value = P1OpponentAttributes.Resolve(profile);
                AssertTrue(value.HeadReachMeters >= 0.90f && value.HeadReachMeters <= 1.15f, "head reach bound");
                AssertTrue(value.AttackGapMinSeconds >= 0.50f && value.AttackGapMaxSeconds <= 1.20f, "gap bound");
                AssertTrue(value.CommitSeconds >= 0.30f && value.RecoverSeconds <= 0.48f, "timing bound");
            }
        }

        private static void TestParser()
        {
            AssertParse("balanced", P1OpponentProfile.Balanced);
            AssertParse("long-reach", P1OpponentProfile.LongReach);
            AssertParse("LONG_REACH", P1OpponentProfile.LongReach);
            AssertParse("pressure", P1OpponentProfile.Pressure);
            AssertParse("fast-hands", P1OpponentProfile.FastHands);
            AssertTrue(!P1OpponentAttributes.TryParse("unknown", out P1OpponentProfile fallback) && fallback == P1OpponentProfile.Balanced, "unknown fallback");
        }

        private static void TestInspectorSchema()
        {
            string value = P1OpponentAttributes.Resolve(P1OpponentProfile.FastHands).ToInspectorText();
            AssertTrue(value == "P1-D FAST_HANDS  REACH x1.00  GAP x1.00  SPEED x1.11", "inspector: " + value);
        }

        private static void AssertParse(string raw, P1OpponentProfile expected)
        {
            AssertTrue(P1OpponentAttributes.TryParse(raw, out P1OpponentProfile actual), "parse failed: " + raw);
            AssertTrue(actual == expected, $"parse {raw}: expected {expected}, got {actual}");
        }

        private static string RepoRoot() => Directory.GetParent(Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName).FullName;
        private static string F(float value) => value.ToString("F2", CultureInfo.InvariantCulture);
        private static void RunSuite(string name, Action suite, ICollection<string> results) { suite(); results.Add("PASS " + name); }
        private static void Run(string name, Action test, ICollection<string> results) { test(); results.Add("PASS " + name); }
        private static void AssertNear(float expected, float actual) { if (Mathf.Abs(expected - actual) > 0.0001f) throw new Exception($"Expected {expected}, got {actual}"); }
        private static void AssertTrue(bool condition, string message) { if (!condition) throw new Exception(message); }
    }
}
