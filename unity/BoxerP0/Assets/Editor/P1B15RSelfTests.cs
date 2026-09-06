using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class P1B15RSelfTests
    {
        private const float Upper = 0.34f;
        private const float Forearm = 0.31f;

        public static void RunBatch()
        {
            List<string> results = new();
            Run("P1-B1.5R arm length preservation", TestArmLengthPreservation, results);
            Run("P1-B1.5R max reach clamp", TestMaxReachClamp, results);
            Run("P1-B1.5R straight near extension", TestStraightNearExtension, results);
            Run("P1-B1.5R hook visibly bent", TestHookBent, results);
            Run("P1-B1.5R uppercut elbow lower", TestUppercutLower, results);
            Run("P1-B1.5R overhand elbow higher", TestOverhandHigher, results);
            Run("P1-B1.5R mirrored consistency", TestMirroredConsistency, results);

            string repoRoot = Directory.GetParent(
                Directory.GetParent(
                    Directory.GetParent(Application.dataPath).FullName).FullName).FullName;
            string evidenceDir = Path.Combine(repoRoot, "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-b1-5r-deterministic-self-tests.txt");
            results.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            results.Add($"TOTAL={results.Count - 1} PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, results) + Environment.NewLine);
            Debug.Log($"P1_B1_5R_SELF_TESTS_PASS={output}");
        }

        private static void TestArmLengthPreservation()
        {
            ArmChainSolution s = ArmChainMath.Solve(Vector3.zero, new Vector3(0.12f, 0.1f, 0.52f), new Vector3(-0.4f, 0.1f, 0.1f), Upper, Forearm);
            AssertNear(Upper, Vector3.Distance(s.Shoulder, s.Elbow), 0.001f);
            AssertNear(Forearm, Vector3.Distance(s.Elbow, s.Wrist), 0.001f);
        }

        private static void TestMaxReachClamp()
        {
            ArmChainSolution s = ArmChainMath.Solve(Vector3.zero, new Vector3(0f, 0f, 2f), new Vector3(-1f, 0f, 0f), Upper, Forearm);
            AssertTrue(s.Clamped, "far wrist request must clamp");
            AssertTrue(Vector3.Distance(s.Shoulder, s.Wrist) < Upper + Forearm, "visual wrist must stay inside anatomical envelope");
        }

        private static void TestStraightNearExtension()
        {
            ArmChainSolution s = ArmChainMath.Solve(Vector3.zero, new Vector3(0f, 0.02f, 0.62f), new Vector3(-0.20f, 0.08f, 0f), Upper, Forearm);
            float angle = ArmChainMath.ElbowAngleDegrees(s);
            AssertTrue(angle > 145f && angle < 180f, $"straight elbow should be near extension, got {angle:F1}");
        }

        private static void TestHookBent()
        {
            ArmChainSolution straight = ArmChainMath.Solve(Vector3.zero, new Vector3(0f, 0.02f, 0.62f), new Vector3(-0.20f, 0.08f, 0f), Upper, Forearm);
            ArmChainSolution hook = ArmChainMath.Solve(Vector3.zero, new Vector3(-0.22f, 0.04f, 0.42f), new Vector3(-0.70f, 0.20f, 0.08f), Upper, Forearm);
            float straightAngle = ArmChainMath.ElbowAngleDegrees(straight);
            float hookAngle = ArmChainMath.ElbowAngleDegrees(hook);
            AssertTrue(hookAngle < 140f, $"hook elbow must remain visibly bent, got {hookAngle:F1}");
            AssertTrue(straightAngle - hookAngle > 20f, "hook bend must differ materially from straight");
        }

        private static void TestUppercutLower()
        {
            ArmChainSolution straight = ArmChainMath.Solve(Vector3.zero, new Vector3(0f, 0.02f, 0.52f), new Vector3(-0.28f, 0.10f, -0.12f), Upper, Forearm);
            ArmChainSolution uppercut = ArmChainMath.Solve(Vector3.zero, new Vector3(-0.08f, 0.12f, 0.42f), new Vector3(-0.42f, -0.52f, -0.10f), Upper, Forearm);
            AssertTrue(uppercut.Elbow.y < straight.Elbow.y, "uppercut elbow must load lower than straight");
        }

        private static void TestOverhandHigher()
        {
            ArmChainSolution straight = ArmChainMath.Solve(Vector3.zero, new Vector3(0f, 0.02f, 0.52f), new Vector3(-0.28f, 0.10f, -0.12f), Upper, Forearm);
            ArmChainSolution overhand = ArmChainMath.Solve(Vector3.zero, new Vector3(-0.05f, 0.08f, 0.48f), new Vector3(-0.45f, 0.58f, -0.06f), Upper, Forearm);
            AssertTrue(overhand.Elbow.y > straight.Elbow.y, "overhand elbow must prepare higher than straight");
        }

        private static void TestMirroredConsistency()
        {
            ArmChainSolution left = ArmChainMath.Solve(Vector3.zero, new Vector3(-0.10f, 0.05f, 0.48f), new Vector3(-0.55f, 0.20f, 0.05f), Upper, Forearm);
            ArmChainSolution right = ArmChainMath.Solve(Vector3.zero, new Vector3(0.10f, 0.05f, 0.48f), new Vector3(0.55f, 0.20f, 0.05f), Upper, Forearm);
            AssertNear(Mathf.Abs(left.Elbow.x), Mathf.Abs(right.Elbow.x), 0.001f);
            AssertNear(left.Elbow.y, right.Elbow.y, 0.001f);
            AssertNear(left.Elbow.z, right.Elbow.z, 0.001f);
        }

        private static void Run(string name, Action test, ICollection<string> results)
        {
            test();
            results.Add("PASS " + name);
        }

        private static void AssertNear(float expected, float actual, float tolerance)
        {
            if (Mathf.Abs(expected - actual) > tolerance)
                throw new Exception($"Expected {expected.ToString(CultureInfo.InvariantCulture)}, got {actual.ToString(CultureInfo.InvariantCulture)}");
        }

        private static void AssertTrue(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }
    }
}
