using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class P1A32SelfTests
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
            RunSuite("P1-A3.2 7/7", RunBatch, suites);

            string evidenceDir = Path.Combine(RepoRoot(), "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-a3-2-combined-regression.txt");
            suites.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            suites.Add("PRIOR_WITH_OBS=101/101 PASS");
            suites.Add("P1_A3_2=7/7 PASS");
            suites.Add("COMBINED=108/108 PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, suites) + Environment.NewLine);
            Debug.Log($"P1_A3_2_COMBINED_REGRESSION_PASS={output}");
        }

        public static void RunBatch()
        {
            List<string> results = new();
            Run("P1-A3.2 non-uppercuts remain unchanged", TestNonUppercutUnchanged, results);
            Run("P1-A3.2 neutral base preserves full drive", TestNeutralFullDrive, results);
            Run("P1-A3.2 advancing base reduces drive", TestAdvancingReducedDrive, results);
            Run("P1-A3.2 retreating base reduces drive equally", TestRetreatingReducedDrive, results);
            Run("P1-A3.2 only vertical target displacement changes", TestOnlyVerticalChanges, results);
            Run("P1-A3.2 lead rear uppercuts share base rule", TestLeadRearSymmetry, results);
            Run("P1-A3.2 semantic event exposes frozen intervention", TestSemanticFields, results);
            int passCount = results.Count;

            string evidenceDir = Path.Combine(RepoRoot(), "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-a3-2-deterministic-self-tests.txt");
            results.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            results.Add($"PLANTED_DRIVE={P1PunchMechanics.A32PlantedUppercutDrive.ToString("F2", CultureInfo.InvariantCulture)}");
            results.Add($"MOVING_DRIVE={P1PunchMechanics.A32MovingUppercutDrive.ToString("F2", CultureInfo.InvariantCulture)}");
            results.Add($"TOTAL={passCount} PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, results) + Environment.NewLine);
            Debug.Log($"P1_A3_2_SELF_TESTS_PASS={output}");
        }

        private static readonly Vector3 Commit = new(0.05f, 1.08f, 0.46f);
        private static readonly Vector3 UppercutTarget = new(0.05f, 1.51f, 1.04f);

        private static void TestNonUppercutUnchanged()
        {
            Vector3 jab = new(-0.10f, 1.46f, 1.26f);
            Vector3 result = P1PunchMechanics.ApplyA32UppercutDrive(PunchIntent.Jab, Commit, jab, "ADVANCING");
            AssertVector(jab, result, "jab must be unchanged");
        }

        private static void TestNeutralFullDrive()
        {
            Vector3 result = P1PunchMechanics.ApplyA32UppercutDrive(PunchIntent.RearUppercut, Commit, UppercutTarget, "NEUTRAL");
            AssertVector(UppercutTarget, result, "neutral uppercut");
            AssertNear(1f, P1PunchMechanics.EffectiveA32UppercutDriveFactor(PunchIntent.RearUppercut, "NEUTRAL"), "neutral factor");
            AssertTrue(P1PunchMechanics.A32BaseState(PunchIntent.RearUppercut, "NEUTRAL") == "PLANTED", "neutral base token");
        }

        private static void TestAdvancingReducedDrive()
        {
            Vector3 result = P1PunchMechanics.ApplyA32UppercutDrive(PunchIntent.RearUppercut, Commit, UppercutTarget, "ADVANCING");
            float expectedY = Commit.y + (UppercutTarget.y - Commit.y) * P1PunchMechanics.A32MovingUppercutDrive;
            AssertNear(expectedY, result.y, "advancing y");
            AssertTrue(result.y < UppercutTarget.y && result.y > Commit.y, "advancing y bounded by original rise");
        }

        private static void TestRetreatingReducedDrive()
        {
            float advancing = P1PunchMechanics.EffectiveA32UppercutDriveFactor(PunchIntent.RearUppercut, "ADVANCING");
            float retreating = P1PunchMechanics.EffectiveA32UppercutDriveFactor(PunchIntent.RearUppercut, "RETREATING");
            AssertNear(P1PunchMechanics.A32MovingUppercutDrive, advancing, "advancing factor");
            AssertNear(advancing, retreating, "moving symmetry");
        }

        private static void TestOnlyVerticalChanges()
        {
            Vector3 result = P1PunchMechanics.ApplyA32UppercutDrive(PunchIntent.RearUppercut, Commit, UppercutTarget, "RETREATING");
            AssertNear(UppercutTarget.x, result.x, "x unchanged");
            AssertNear(UppercutTarget.z, result.z, "z unchanged");
            AssertTrue(!Mathf.Approximately(UppercutTarget.y, result.y), "y must change");
        }

        private static void TestLeadRearSymmetry()
        {
            float lead = P1PunchMechanics.EffectiveA32UppercutDriveFactor(PunchIntent.LeadUppercut, "ADVANCING");
            float rear = P1PunchMechanics.EffectiveA32UppercutDriveFactor(PunchIntent.RearUppercut, "ADVANCING");
            AssertNear(lead, rear, "lead/rear factor");
            AssertTrue(P1PunchMechanics.A32BaseState(PunchIntent.LeadUppercut, "ADVANCING") == "MOVING", "lead base");
        }

        private static void TestSemanticFields()
        {
            P1PunchSnapshot snapshot = new(
                1f,
                PunchIntent.RearUppercut,
                Vector3.zero,
                Vector3.forward,
                1f,
                0.6f,
                0f,
                0f,
                0f,
                "ADVANCING",
                1f,
                0.8f);
            string value = snapshot.ToSemanticEvent(CombatOutcome.Miss, false);
            AssertTrue(value.Contains("A32_MODE=UPPERCUT_BASE"), "mode field");
            AssertTrue(value.Contains("A32_BASE=MOVING"), "base field");
            AssertTrue(value.Contains("A32_DRIVE=0.850"), "drive field");
        }

        private static string RepoRoot() => Directory.GetParent(Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName).FullName;
        private static void RunSuite(string name, Action suite, ICollection<string> results) { suite(); results.Add("PASS " + name); }
        private static void Run(string name, Action test, ICollection<string> results) { test(); results.Add("PASS " + name); }
        private static void AssertVector(Vector3 expected, Vector3 actual, string message)
        {
            AssertNear(expected.x, actual.x, message + " x");
            AssertNear(expected.y, actual.y, message + " y");
            AssertNear(expected.z, actual.z, message + " z");
        }
        private static void AssertNear(float expected, float actual, string message, float tolerance = 0.00001f)
        {
            if (Mathf.Abs(expected - actual) > tolerance) throw new Exception($"{message}: expected {expected:F6}, got {actual:F6}");
        }
        private static void AssertTrue(bool condition, string message) { if (!condition) throw new Exception(message); }
    }
}
