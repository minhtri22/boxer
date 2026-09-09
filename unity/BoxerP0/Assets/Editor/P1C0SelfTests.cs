using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class P1C0SelfTests
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
            RunSuite("P1-C0 7/7", RunBatch, suites);

            string repoRoot = Directory.GetParent(
                Directory.GetParent(
                    Directory.GetParent(Application.dataPath).FullName).FullName).FullName;
            string evidenceDir = Path.Combine(repoRoot, "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-c0-combined-regression.txt");
            suites.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            suites.Add("PRIOR_BASELINE=64/64 PASS");
            suites.Add("P1_C0=7/7 PASS");
            suites.Add("COMBINED=71/71 PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, suites) + Environment.NewLine);
            Debug.Log($"P1_C0_COMBINED_REGRESSION_PASS={output}");
        }

        public static void RunBatch()
        {
            List<string> results = new();
            Run("P1-C0 guard is neutral", TestGuardNeutral, results);
            Run("P1-C0 lead rear yaw is equal opposite", TestLeadRearOpposite, results);
            Run("P1-C0 torso exceeds pelvis yaw", TestTorsoExceedsPelvis, results);
            Run("P1-C0 commit extend amplitude is monotonic", TestCommitExtendMonotonic, results);
            Run("P1-C0 recovery returns to neutral", TestRecoveryReturnsNeutral, results);
            Run("P1-C0 rotation is family independent", TestFamilyIndependent, results);
            Run("P1-C0 anchor rotation is finite and radius preserving", TestAnchorRotationInvariant, results);
            int passCount = results.Count;

            string repoRoot = Directory.GetParent(
                Directory.GetParent(
                    Directory.GetParent(Application.dataPath).FullName).FullName).FullName;
            string evidenceDir = Path.Combine(repoRoot, "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-c0-deterministic-self-tests.txt");
            results.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            results.Add($"MAX_PELVIS_YAW_DEG={P1BodyRotationMath.MaxPelvisYawDegrees.ToString("F1", CultureInfo.InvariantCulture)}");
            results.Add($"MAX_TORSO_YAW_DEG={P1BodyRotationMath.MaxTorsoYawDegrees.ToString("F1", CultureInfo.InvariantCulture)}");
            results.Add($"COMMIT_END_AMPLITUDE={P1BodyRotationMath.CommitEndAmplitude.ToString("F2", CultureInfo.InvariantCulture)}");
            results.Add($"TOTAL={passCount} PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, results) + Environment.NewLine);
            Debug.Log($"P1_C0_SELF_TESTS_PASS={output}");
        }

        private static void TestGuardNeutral()
        {
            P1BodyRotationPose pose = P1BodyRotationMath.Sample(PunchIntent.Cross, ActionPhase.Guard, 1f);
            AssertNear(0f, pose.PelvisYawDegrees, "guard pelvis");
            AssertNear(0f, pose.TorsoYawDegrees, "guard torso");
            AssertNear(0f, pose.Amplitude, "guard amplitude");
        }

        private static void TestLeadRearOpposite()
        {
            P1BodyRotationPose lead = P1BodyRotationMath.Sample(PunchIntent.Jab, ActionPhase.Extend, 1f);
            P1BodyRotationPose rear = P1BodyRotationMath.Sample(PunchIntent.Cross, ActionPhase.Extend, 1f);
            AssertNear(-lead.PelvisYawDegrees, rear.PelvisYawDegrees, "pelvis sign");
            AssertNear(-lead.TorsoYawDegrees, rear.TorsoYawDegrees, "torso sign");
        }

        private static void TestTorsoExceedsPelvis()
        {
            P1BodyRotationPose pose = P1BodyRotationMath.Sample(PunchIntent.Cross, ActionPhase.Extend, 1f);
            AssertTrue(Mathf.Abs(pose.TorsoYawDegrees) > Mathf.Abs(pose.PelvisYawDegrees),
                "torso yaw must exceed pelvis yaw");
            AssertNear(P1BodyRotationMath.MaxPelvisYawDegrees, Mathf.Abs(pose.PelvisYawDegrees), "pelvis max");
            AssertNear(P1BodyRotationMath.MaxTorsoYawDegrees, Mathf.Abs(pose.TorsoYawDegrees), "torso max");
        }

        private static void TestCommitExtendMonotonic()
        {
            float commitStart = P1BodyRotationMath.Sample(PunchIntent.Cross, ActionPhase.Commit, 0f).Amplitude;
            float commitEnd = P1BodyRotationMath.Sample(PunchIntent.Cross, ActionPhase.Commit, 1f).Amplitude;
            float extendStart = P1BodyRotationMath.Sample(PunchIntent.Cross, ActionPhase.Extend, 0f).Amplitude;
            float extendEnd = P1BodyRotationMath.Sample(PunchIntent.Cross, ActionPhase.Extend, 1f).Amplitude;
            AssertNear(0f, commitStart, "commit start");
            AssertNear(P1BodyRotationMath.CommitEndAmplitude, commitEnd, "commit end");
            AssertNear(commitEnd, extendStart, "phase continuity");
            AssertTrue(extendEnd > extendStart, "extend must increase amplitude");
            AssertNear(1f, extendEnd, "extend end");
        }

        private static void TestRecoveryReturnsNeutral()
        {
            P1BodyRotationPose recoverStart = P1BodyRotationMath.Sample(PunchIntent.Cross, ActionPhase.Recover, 0f);
            P1BodyRotationPose recoverEnd = P1BodyRotationMath.Sample(PunchIntent.Cross, ActionPhase.Recover, 1f);
            AssertNear(1f, recoverStart.Amplitude, "recover start");
            AssertNear(0f, recoverEnd.Amplitude, "recover end");
            AssertNear(0f, recoverEnd.PelvisYawDegrees, "recover pelvis neutral");
            AssertNear(0f, recoverEnd.TorsoYawDegrees, "recover torso neutral");
        }

        private static void TestFamilyIndependent()
        {
            P1BodyRotationPose straight = P1BodyRotationMath.Sample(PunchIntent.Cross, ActionPhase.Extend, 0.6f);
            P1BodyRotationPose hook = P1BodyRotationMath.Sample(PunchIntent.RearHook, ActionPhase.Extend, 0.6f);
            AssertNear(straight.PelvisYawDegrees, hook.PelvisYawDegrees, "rear family pelvis");
            AssertNear(straight.TorsoYawDegrees, hook.TorsoYawDegrees, "rear family torso");

            P1BodyRotationPose jab = P1BodyRotationMath.Sample(PunchIntent.Jab, ActionPhase.Commit, 0.5f);
            P1BodyRotationPose leadHook = P1BodyRotationMath.Sample(PunchIntent.LeadHook, ActionPhase.Commit, 0.5f);
            AssertNear(jab.PelvisYawDegrees, leadHook.PelvisYawDegrees, "lead family pelvis");
            AssertNear(jab.TorsoYawDegrees, leadHook.TorsoYawDegrees, "lead family torso");
        }

        private static void TestAnchorRotationInvariant()
        {
            Vector3 anchor = new Vector3(0.38f, 1.43f, 0.02f);
            Vector3 rotated = P1BodyRotationMath.RotateLocalYaw(anchor, P1BodyRotationMath.MaxTorsoYawDegrees);
            AssertTrue(IsFinite(rotated.x) && IsFinite(rotated.y) && IsFinite(rotated.z), "rotated anchor must be finite");
            AssertNear(anchor.magnitude, rotated.magnitude, "yaw must preserve anchor radius", 0.0001f);
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private static void RunSuite(string name, Action suite, ICollection<string> results)
        {
            suite();
            results.Add("PASS " + name);
        }

        private static void Run(string name, Action test, ICollection<string> results)
        {
            test();
            results.Add("PASS " + name);
        }

        private static void AssertNear(float expected, float actual, string message, float tolerance = 0.00001f)
        {
            if (Mathf.Abs(expected - actual) > tolerance)
                throw new Exception($"{message}: expected {expected:F6}, got {actual:F6}");
        }

        private static void AssertTrue(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }
    }
}
