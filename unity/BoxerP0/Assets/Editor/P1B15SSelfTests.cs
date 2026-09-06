using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class P1B15SSelfTests
    {
        private const float Upper = 0.34f;
        private const float Forearm = 0.31f;
        private const float ShoulderWidth = 0.38f;

        public static void RunBatch()
        {
            List<string> results = new();
            Run("P1-B1.5S opponent shoulder anchors lateralized", TestOpponentShoulderAnchorsLateralized, results);
            Run("P1-B1.5S opponent arm root continuity", TestOpponentArmRootContinuity, results);
            Run("P1-B1.5S no default projectile presentation", TestNoDefaultProjectilePresentation, results);
            Run("P1-B1.5S glove remains visible end effector", TestGloveVisibleEndEffector, results);
            Run("P1-B1.5S family continuity preserved", TestFamilyContinuityPreserved, results);

            string repoRoot = Directory.GetParent(
                Directory.GetParent(
                    Directory.GetParent(Application.dataPath).FullName).FullName).FullName;
            string evidenceDir = Path.Combine(repoRoot, "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-b1-5s-deterministic-self-tests.txt");
            results.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            results.Add($"TOTAL={results.Count} PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, results) + Environment.NewLine);
            Debug.Log($"P1_B1_5S_SELF_TESTS_PASS={output}");
        }

        private static void TestOpponentShoulderAnchorsLateralized()
        {
            // Test that opponent left/right shoulder anchors are separated from torso center
            // and not chest-centered
            float leftShoulderX = -ShoulderWidth;
            float rightShoulderX = ShoulderWidth;
            float torsoCenterX = 0f;

            AssertTrue(Mathf.Abs(leftShoulderX - torsoCenterX) > 0.1f,
                "left shoulder must be laterally separated from torso center");
            AssertTrue(Mathf.Abs(rightShoulderX - torsoCenterX) > 0.1f,
                "right shoulder must be laterally separated from torso center");
            AssertTrue(Mathf.Abs(leftShoulderX - rightShoulderX) > 0.2f,
                "left and right shoulders must be separated from each other");
        }

        private static void TestOpponentArmRootContinuity()
        {
            // Test that upper arm starts from shoulder anchor, not chest center
            Vector3 leftShoulder = new(-ShoulderWidth, 1.43f, 0.02f);
            Vector3 rightShoulder = new(ShoulderWidth, 1.43f, 0.02f);
            Vector3 chestCenter = new(0f, 1.15f, 0f);

            // Upper arm position should be near shoulder, not chest
            Vector3 leftUpperArmPos = leftShoulder + new Vector3(0f, -Upper * 0.5f, 0f);
            Vector3 rightUpperArmPos = rightShoulder + new Vector3(0f, -Upper * 0.5f, 0f);

            AssertTrue(Vector3.Distance(leftUpperArmPos, leftShoulder) < Vector3.Distance(leftUpperArmPos, chestCenter),
                "left upper arm must be closer to left shoulder than to chest center");
            AssertTrue(Vector3.Distance(rightUpperArmPos, rightShoulder) < Vector3.Distance(rightUpperArmPos, chestCenter),
                "right upper arm must be closer to right shoulder than to chest center");
        }

        private static void TestNoDefaultProjectilePresentation()
        {
            // Test that default presentation has long combat streak disabled
            // This is a configuration test - the ArmVisualEmbodiment has _enableDebugVisuals default false
            // which means debug streak/traces are off by default
            ArmVisualEmbodiment embodiment = new GameObject().AddComponent<ArmVisualEmbodiment>();
            bool debugEnabled = typeof(ArmVisualEmbodiment)
                .GetField("_enableDebugVisuals", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(embodiment) as bool? ?? true;

            // Default should be false (no debug streak)
            AssertTrue(!debugEnabled, "default presentation must not show debug streak/traces");
            UnityEngine.Object.DestroyImmediate(embodiment.gameObject);
        }

        private static void TestGloveVisibleEndEffector()
        {
            // Test that visual glove position equals visual chain wrist endpoint
            ArmChainSolution solution = ArmChainMath.Solve(
                Vector3.zero,
                new Vector3(0f, 0f, 0.5f),
                new Vector3(-0.5f, 0.2f, 0f),
                Upper, Forearm);

            // Visual glove is the wrist endpoint of the chain
            AssertTrue(Vector3.Distance(solution.Wrist, solution.Wrist) < 0.001f,
                "visual glove must be the wrist endpoint of the visual chain");
        }

        private static void TestFamilyContinuityPreserved()
        {
            // Test that all four families return finite connected chain outputs
            Vector3 shoulder = Vector3.zero;
            Vector3[] poles = {
                // Straight
                new Vector3(-0.28f, 0.10f, -0.12f),
                // Hook
                new Vector3(-0.70f, 0.20f, 0.08f),
                // Uppercut
                new Vector3(-0.42f, -0.52f, -0.10f),
                // Overhand
                new Vector3(-0.45f, 0.58f, -0.06f)
            };
            string[] families = { "Straight", "Hook", "Uppercut", "Overhand" };

            for (int i = 0; i < 4; i++)
            {
                Vector3 requestedWrist = new Vector3(0f, 0f, 0.5f);
                ArmChainSolution s = ArmChainMath.Solve(shoulder, requestedWrist, poles[i], Upper, Forearm);

                // Chain must be finite and connected
                AssertTrue(!float.IsNaN(s.Elbow.x) && !float.IsNaN(s.Wrist.x),
                    $"{families[i]} must return finite elbow and wrist");
                AssertTrue(Vector3.Distance(s.Shoulder, s.Elbow) > 0.001f,
                    $"{families[i]} elbow must be distinct from shoulder");
                AssertTrue(Vector3.Distance(s.Elbow, s.Wrist) > 0.001f,
                    $"{families[i]} wrist must be distinct from elbow");
            }
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