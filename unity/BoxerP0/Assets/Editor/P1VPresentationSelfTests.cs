using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class P1VPresentationSelfTests
    {
        private const string EvidenceDirectory = "../../../evidence/phase1/SYNTHETIC";

        [MenuItem("Boxer/Run P1-V Presentation Self Tests")]
        public static void Run()
        {
            List<string> results = new();
            Check("P1-V opponent scale is bounded", TestOpponentScaleBounds, results);
            Check("P1-V opponent scale is distance-monotonic", TestOpponentScaleMonotonic, results);
            Check("P1-V guard pose token is stable", TestGuardPose, results);
            Check("P1-V straight pose mapping is stable", TestStraightPose, results);
            Check("P1-V hook pose mapping is stable", TestHookPose, results);
            Check("P1-V lead/rear mirroring is stable", TestMirroring, results);
            Check("P1-V bottom controls preserve left-center-right hierarchy", TestControlHierarchy, results);
            Check("P1-V production resources are present", TestResourcesPresent, results);

            string root = Path.GetFullPath(Path.Combine(Application.dataPath, EvidenceDirectory));
            Directory.CreateDirectory(root);
            string path = Path.Combine(root, "p1-v-presentation-deterministic-self-tests.txt");
            List<string> output = new()
            {
                "evidence=SYNTHETIC",
                $"unity={Application.unityVersion}",
                $"utc={DateTime.UtcNow:O}"
            };
            output.AddRange(results);
            output.Add($"TOTAL={results.Count} PASS");
            File.WriteAllLines(path, output);
            Debug.Log(string.Join("\n", output));
        }

        private static void TestOpponentScaleBounds()
        {
            AssertNear(P1VPresentationMath.OpponentHeightFraction(0.1f), 0.88f, "near clamp");
            AssertNear(P1VPresentationMath.OpponentHeightFraction(9f), 0.50f, "far clamp");
        }

        private static void TestOpponentScaleMonotonic()
        {
            float near = P1VPresentationMath.OpponentHeightFraction(1.4f);
            float far = P1VPresentationMath.OpponentHeightFraction(2.0f);
            AssertTrue(near > far, "near opponent must be larger");
        }

        private static void TestGuardPose() => AssertTrue(P1VPresentationMath.PoseToken(PunchIntent.None) == "GUARD", "guard token");

        private static void TestStraightPose()
        {
            AssertTrue(P1VPresentationMath.PoseToken(PunchIntent.Jab) == "STRAIGHT", "jab token");
            AssertTrue(P1VPresentationMath.PoseToken(PunchIntent.Cross) == "STRAIGHT", "cross token");
            AssertTrue(P1VPresentationMath.PoseToken(PunchIntent.RearOverhand) == "STRAIGHT", "overhand fallback token");
        }

        private static void TestHookPose()
        {
            AssertTrue(P1VPresentationMath.PoseToken(PunchIntent.LeadHook) == "HOOK", "lead hook token");
            AssertTrue(P1VPresentationMath.PoseToken(PunchIntent.RearHook) == "HOOK", "rear hook token");
        }

        private static void TestMirroring()
        {
            AssertTrue(P1VPresentationMath.ShouldMirrorAttack(PunchIntent.Jab), "lead attack mirrors");
            AssertTrue(!P1VPresentationMath.ShouldMirrorAttack(PunchIntent.Cross), "rear attack preserves source orientation");
        }

        private static void TestControlHierarchy()
        {
            Vector2 movement = P1VPresentationMath.MovementControlCenter(1024f, 1536f);
            Vector2 guard = P1VPresentationMath.GuardControlCenter(1024f, 1536f);
            Vector2 punch = P1VPresentationMath.PunchControlCenter(1024f, 1536f);
            AssertTrue(movement.x < guard.x && guard.x < punch.x, "horizontal hierarchy");
            AssertTrue(movement.y > 1536f * 0.80f && guard.y > 1536f * 0.80f && punch.y > 1536f * 0.80f, "bottom hierarchy");
        }

        private static void TestResourcesPresent()
        {
            string[] paths =
            {
                "Assets/Resources/P1V/championship-arena.png",
                "Assets/Resources/P1V/player-glove-left-chroma.png",
                "Assets/Resources/P1V/ramirez-guard-chroma.png",
                "Assets/Resources/P1V/ramirez-straight-chroma.png",
                "Assets/Resources/P1V/ramirez-hook-chroma.png",
                "Assets/Resources/BoxerP1VChromaKey.shader"
            };
            foreach (string path in paths) AssertTrue(File.Exists(Path.GetFullPath(path)), "missing " + path);
        }

        private static void Check(string label, Action test, ICollection<string> results)
        {
            test();
            results.Add("PASS " + label);
        }

        private static void AssertNear(float actual, float expected, string label)
        {
            if (Mathf.Abs(actual - expected) > 0.0001f) throw new InvalidOperationException($"{label}: {actual} != {expected}");
        }

        private static void AssertTrue(bool condition, string label)
        {
            if (!condition) throw new InvalidOperationException(label);
        }
    }
}
