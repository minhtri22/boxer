using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class P1A3SelfTests
    {
        public static void RunBatch()
        {
            List<string> results = new();
            Run("P1-A3.1 close hook baseline", TestCloseHookBaseline, results);
            Run("P1-A3.1 far hook falloff", TestFarHookFalloff, results);
            Run("P1-A3.1 non-hook unchanged", TestNonHookUnchanged, results);
            Run("P1-A3.1 causal hook boundary", TestHookCausalBoundary, results);

            string repoRoot = Directory.GetParent(
                Directory.GetParent(
                    Directory.GetParent(Application.dataPath).FullName).FullName).FullName;
            string evidenceDir = Path.Combine(repoRoot, "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-a3-deterministic-self-tests.txt");
            results.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            results.Add($"TOTAL={results.Count - 1} PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, results) + Environment.NewLine);
            Debug.Log($"P1_A3_SELF_TESTS_PASS={output}");
        }

        private static void Run(string name, Action test, ICollection<string> results)
        {
            test();
            results.Add($"PASS {name}");
        }

        private static void TestCloseHookBaseline()
        {
            Vector3 target = new(0.11f, 1.39f, 1.05f);
            float factor = P1PunchMechanics.EffectiveA3FamilyReachFactor(
                PunchIntent.LeadHook,
                P1PunchMechanics.A3HookFullRangeMeters);
            Vector3 applied = P1PunchMechanics.ApplyA3FamilyCoupling(
                PunchIntent.LeadHook,
                target,
                P1PunchMechanics.A3HookFullRangeMeters);

            AssertNear(1f, factor, 0.0001f);
            AssertNear(target.x, applied.x, 0.0001f);
            AssertNear(target.y, applied.y, 0.0001f);
            AssertNear(target.z, applied.z, 0.0001f);
        }

        private static void TestFarHookFalloff()
        {
            Vector3 target = new(0.11f, 1.39f, 1.05f);
            float factor = P1PunchMechanics.EffectiveA3FamilyReachFactor(
                PunchIntent.RearHook,
                P1PunchMechanics.A3HookFalloffEndMeters);
            Vector3 applied = P1PunchMechanics.ApplyA3FamilyCoupling(
                PunchIntent.RearHook,
                target,
                P1PunchMechanics.A3HookFalloffEndMeters);

            AssertNear(P1PunchMechanics.A3HookFarReachFactor, factor, 0.0001f);
            AssertNear(target.z * P1PunchMechanics.A3HookFarReachFactor, applied.z, 0.0001f);
            AssertTrue(applied.z < target.z, "far hook must give up forward extension");
        }

        private static void TestNonHookUnchanged()
        {
            Vector3 target = new(0.04f, 1.50f, 1.24f);
            PunchIntent[] intents =
            {
                PunchIntent.Jab,
                PunchIntent.Cross,
                PunchIntent.LeadUppercut,
                PunchIntent.RearUppercut,
                PunchIntent.LeadOverhand,
                PunchIntent.RearOverhand
            };

            foreach (PunchIntent intent in intents)
            {
                float factor = P1PunchMechanics.EffectiveA3FamilyReachFactor(intent, 1.40f);
                Vector3 applied = P1PunchMechanics.ApplyA3FamilyCoupling(intent, target, 1.40f);
                AssertNear(1f, factor, 0.0001f);
                AssertNear(target.z, applied.z, 0.0001f);
            }
        }

        private static void TestHookCausalBoundary()
        {
            Vector3 start = Vector3.zero;
            Vector3 target = new(0f, 0f, 1f);
            Vector3 closeHook = P1PunchMechanics.ApplyA3FamilyCoupling(PunchIntent.LeadHook, target, 1.00f);
            Vector3 farHook = P1PunchMechanics.ApplyA3FamilyCoupling(PunchIntent.LeadHook, target, 1.30f);
            Vector3 boundary = new(0f, 0f, 0.93f);

            AssertTrue(
                CombatGeometry.SegmentSphereIntersects(start, closeHook, boundary, 0.01f),
                "close hook must cross the controlled boundary");
            AssertTrue(
                !CombatGeometry.SegmentSphereIntersects(start, farHook, boundary, 0.01f),
                "far hook must not cross the same controlled boundary");
        }

        private static void AssertNear(float expected, float actual, float tolerance)
        {
            if (Mathf.Abs(expected - actual) > tolerance)
            {
                throw new Exception($"Expected {expected.ToString(CultureInfo.InvariantCulture)}, got {actual.ToString(CultureInfo.InvariantCulture)}");
            }
        }

        private static void AssertTrue(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }
    }
}
