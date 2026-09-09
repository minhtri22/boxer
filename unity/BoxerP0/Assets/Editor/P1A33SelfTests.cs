using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class P1A33SelfTests
    {
        private const float BaseRecoverySeconds = 0.28f;

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
            RunSuite("P1-A3.3 7/7", RunBatch, suites);

            string evidenceDir = Path.Combine(RepoRoot(), "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-a3-3-combined-regression.txt");
            suites.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            suites.Add("PRIOR_WITH_A3_2=108/108 PASS");
            suites.Add("P1_A3_3=7/7 PASS");
            suites.Add("COMBINED=115/115 PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, suites) + Environment.NewLine);
            Debug.Log($"P1_A3_3_COMBINED_REGRESSION_PASS={output}");
        }

        public static void RunBatch()
        {
            List<string> results = new();
            Run("P1-A3.3 non-overhands keep base recovery", TestNonOverhandUnchanged, results);
            Run("P1-A3.3 neutral overhand keeps base recovery", TestNeutralOverhand, results);
            Run("P1-A3.3 retreating overhand keeps base recovery", TestRetreatingOverhand, results);
            Run("P1-A3.3 advancing overhand extends recovery", TestAdvancingOverhand, results);
            Run("P1-A3.3 committed recovery duration is exact", TestExactDuration, results);
            Run("P1-A3.3 commit and extend timing remain unchanged", TestOnlyRecoveryChanges, results);
            Run("P1-A3.3 semantic event exposes frozen intervention", TestSemanticFields, results);
            int passCount = results.Count;

            string evidenceDir = Path.Combine(RepoRoot(), "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-a3-3-deterministic-self-tests.txt");
            results.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            results.Add($"BASE_RECOVERY_SECONDS={BaseRecoverySeconds.ToString("F3", CultureInfo.InvariantCulture)}");
            results.Add($"FORWARD_COMMITTED_FACTOR={P1PunchMechanics.A33ForwardCommittedRecoveryFactor.ToString("F2", CultureInfo.InvariantCulture)}");
            results.Add($"TOTAL={passCount} PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, results) + Environment.NewLine);
            Debug.Log($"P1_A3_3_SELF_TESTS_PASS={output}");
        }

        private static void TestNonOverhandUnchanged()
        {
            AssertNear(1f, P1PunchMechanics.EffectiveA33RecoveryFactor(PunchIntent.Cross, "ADVANCING"), "cross factor");
            AssertNear(BaseRecoverySeconds, P1PunchMechanics.EffectiveA33RecoverySeconds(PunchIntent.Cross, "ADVANCING", BaseRecoverySeconds), "cross recovery");
        }

        private static void TestNeutralOverhand()
        {
            AssertNear(1f, P1PunchMechanics.EffectiveA33RecoveryFactor(PunchIntent.RearOverhand, "NEUTRAL"), "neutral factor");
            AssertTrue(P1PunchMechanics.A33CommitState(PunchIntent.RearOverhand, "NEUTRAL") == "UNCOMMITTED", "neutral state");
        }

        private static void TestRetreatingOverhand()
        {
            AssertNear(1f, P1PunchMechanics.EffectiveA33RecoveryFactor(PunchIntent.RearOverhand, "RETREATING"), "retreating factor");
            AssertTrue(P1PunchMechanics.A33CommitState(PunchIntent.RearOverhand, "RETREATING") == "UNCOMMITTED", "retreating state");
        }

        private static void TestAdvancingOverhand()
        {
            AssertNear(P1PunchMechanics.A33ForwardCommittedRecoveryFactor,
                P1PunchMechanics.EffectiveA33RecoveryFactor(PunchIntent.RearOverhand, "ADVANCING"), "advancing factor");
            AssertTrue(P1PunchMechanics.A33CommitState(PunchIntent.RearOverhand, "ADVANCING") == "FORWARD_COMMITTED", "advancing state");
        }

        private static void TestExactDuration()
        {
            float value = P1PunchMechanics.EffectiveA33RecoverySeconds(PunchIntent.RearOverhand, "ADVANCING", BaseRecoverySeconds);
            AssertNear(0.336f, value, "committed duration");
        }

        private static void TestOnlyRecoveryChanges()
        {
            TimedActionState normal = new();
            TimedActionState committed = new();
            AssertTrue(normal.TryStart(PunchIntent.RearOverhand), "normal start");
            AssertTrue(committed.TryStart(PunchIntent.RearOverhand), "committed start");
            normal.Step(0.09f, 0.09f, 0.14f, BaseRecoverySeconds);
            committed.Step(0.09f, 0.09f, 0.14f, 0.336f);
            AssertTrue(normal.Phase == ActionPhase.Extend && committed.Phase == ActionPhase.Extend, "same commit duration");
            normal.Step(0.14f, 0.09f, 0.14f, BaseRecoverySeconds);
            committed.Step(0.14f, 0.09f, 0.14f, 0.336f);
            AssertTrue(normal.Phase == ActionPhase.Recover && committed.Phase == ActionPhase.Recover, "same extend duration");
            normal.Step(BaseRecoverySeconds, 0.09f, 0.14f, BaseRecoverySeconds);
            committed.Step(BaseRecoverySeconds, 0.09f, 0.14f, 0.336f);
            AssertTrue(normal.Phase == ActionPhase.Guard && committed.Phase == ActionPhase.Recover, "only committed recovery remains open");
        }

        private static void TestSemanticFields()
        {
            P1PunchSnapshot snapshot = new(
                1f, PunchIntent.RearOverhand, Vector3.zero, Vector3.forward, 1f,
                0.7f, 0f, 0f, 0f, "ADVANCING", 1f, 0.8f);
            string value = snapshot.ToSemanticEvent(CombatOutcome.Miss, false);
            AssertTrue(value.Contains("A33_MODE=OVERHAND_RECOVERY"), "mode field");
            AssertTrue(value.Contains("A33_COMMIT=FORWARD_COMMITTED"), "commit field");
            AssertTrue(value.Contains("A33_RECOVERY=1.200"), "recovery field");
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
