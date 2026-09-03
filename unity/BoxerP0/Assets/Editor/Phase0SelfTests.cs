using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class Phase0SelfTests
    {
        public static void RunBatch()
        {
            List<string> results = new();
            Run("head dead zone", TestHeadDeadZone, results);
            Run("head sign and bound", TestHeadSignAndBound, results);
            Run("gesture jab", TestGestureJab, results);
            Run("gesture cross", TestGestureCross, results);
            Run("gesture hook", TestGestureHook, results);
            Run("geometry hit and miss", TestGeometry, results);
            Run("default fight range can connect", TestDefaultFightRange, results);
            Run("anti-spam state transition", TestActionState, results);
            Run("counter recovery window", TestCounterWindow, results);

            string repoRoot = Directory.GetParent(
                Directory.GetParent(
                    Directory.GetParent(Application.dataPath).FullName).FullName).FullName;
            string evidenceDir = Path.Combine(repoRoot, "evidence", "phase0", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "deterministic-self-tests.txt");
            results.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            results.Add($"TOTAL={results.Count - 1} PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, results) + Environment.NewLine);
            Debug.Log($"P0_SELF_TESTS_PASS={output}");
        }

        private static void Run(string name, Action test, ICollection<string> results)
        {
            test();
            results.Add($"PASS {name}");
        }

        private static void TestHeadDeadZone()
        {
            AssertNear(0f, HeadMotionMath.ResolveOffset(1.5f), 0.0001f);
        }

        private static void TestHeadSignAndBound()
        {
            AssertTrue(HeadMotionMath.ResolveOffset(-10f) < 0f, "negative angle must move head left");
            AssertNear(0.34f, HeadMotionMath.ResolveOffset(30f), 0.0001f);
        }

        private static void TestGestureJab()
        {
            GestureMetrics metrics = new(new Vector2(10f, 110f), 112f, 0.18f);
            AssertEqual(PunchIntent.Jab, PunchGestureClassifier.Classify(metrics));
        }

        private static void TestGestureCross()
        {
            GestureMetrics metrics = new(new Vector2(20f, 255f), 260f, 0.42f);
            AssertEqual(PunchIntent.Cross, PunchGestureClassifier.Classify(metrics));
        }

        private static void TestGestureHook()
        {
            GestureMetrics metrics = new(new Vector2(180f, 30f), 235f, 0.34f);
            AssertEqual(PunchIntent.Hook, PunchGestureClassifier.Classify(metrics));
        }

        private static void TestGeometry()
        {
            AssertTrue(CombatGeometry.SegmentSphereIntersects(
                new Vector3(-1f, 0f, 0f), new Vector3(1f, 0f, 0f), Vector3.zero, 0.1f), "segment should hit sphere");
            AssertTrue(!CombatGeometry.SegmentSphereIntersects(
                new Vector3(-1f, 1f, 0f), new Vector3(1f, 1f, 0f), Vector3.zero, 0.1f), "segment should miss sphere");
        }

        private static void TestDefaultFightRange()
        {
            Vector3 playerRoot = new(0f, 0f, -0.70f);
            Vector3 punchStart = playerRoot + new Vector3(-0.22f, 1.38f, 0.48f);
            Vector3 punchEnd = playerRoot + new Vector3(-0.10f, 1.46f, 1.26f);
            Vector3 opponentHead = new(0f, 1.62f, 0.70f);
            AssertTrue(
                CombatGeometry.SegmentSphereIntersects(punchStart, punchEnd, opponentHead, 0.27f),
                "default jab trajectory must be able to contact the opponent head");
        }

        private static void TestActionState()
        {
            TimedActionState state = new();
            AssertTrue(state.TryStart(PunchIntent.Jab), "first punch should start");
            AssertTrue(!state.TryStart(PunchIntent.Cross), "second punch during commitment must be rejected");
            state.Step(0.10f, 0.09f, 0.14f, 0.28f);
            AssertEqual(ActionPhase.Extend, state.Phase);
            state.Step(0.15f, 0.09f, 0.14f, 0.28f);
            AssertEqual(ActionPhase.Recover, state.Phase);
            state.Step(0.29f, 0.09f, 0.14f, 0.28f);
            AssertEqual(ActionPhase.Guard, state.Phase);
        }

        private static void TestCounterWindow()
        {
            TimedActionState state = new();
            state.TryStart(PunchIntent.Cross);
            state.Step(0.10f, 0.09f, 0.14f, 0.28f);
            AssertTrue(!state.CounterWindowOpen, "counter window must not open during extension");
            state.Step(0.15f, 0.09f, 0.14f, 0.28f);
            AssertTrue(state.CounterWindowOpen, "counter window must open during recovery");
        }

        private static void AssertNear(float expected, float actual, float tolerance)
        {
            if (Mathf.Abs(expected - actual) > tolerance)
            {
                throw new Exception($"Expected {expected.ToString(CultureInfo.InvariantCulture)}, got {actual.ToString(CultureInfo.InvariantCulture)}");
            }
        }

        private static void AssertEqual<T>(T expected, T actual)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new Exception($"Expected {expected}, got {actual}");
            }
        }

        private static void AssertTrue(bool condition, string message = "assertion failed")
        {
            if (!condition) throw new Exception(message);
        }
    }
}
