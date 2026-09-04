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
            Run("gesture lead jab", TestGestureJab, results);
            Run("gesture rear cross", TestGestureCross, results);
            Run("gesture lead hook", TestGestureLeadHook, results);
            Run("gesture rear hook", TestGestureRearHook, results);
            Run("punch labels", TestPunchLabels, results);
            Run("geometry hit and miss", TestGeometry, results);
            Run("default fight range can connect", TestDefaultFightRange, results);
            Run("anti-spam state transition", TestActionState, results);
            Run("counter recovery window", TestCounterWindow, results);
            Run("action reset to guard", TestActionReset, results);
            Run("onboarding head progress", TestOnboardingHead, results);
            Run("onboarding footwork progress", TestOnboardingFootwork, results);
            Run("onboarding punch progress", TestOnboardingPunches, results);

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

        private static void TestGestureLeadHook()
        {
            GestureMetrics metrics = new(new Vector2(-180f, 30f), 235f, 0.34f);
            AssertEqual(PunchIntent.LeadHook, PunchGestureClassifier.Classify(metrics));
        }

        private static void TestGestureRearHook()
        {
            GestureMetrics metrics = new(new Vector2(180f, 30f), 235f, 0.34f);
            AssertEqual(PunchIntent.RearHook, PunchGestureClassifier.Classify(metrics));
        }

        private static void TestPunchLabels()
        {
            AssertEqual("LEAD JAB", PunchLabels.Display(PunchIntent.Jab));
            AssertEqual("REAR CROSS", PunchLabels.Display(PunchIntent.Cross));
            AssertEqual("LEAD HOOK", PunchLabels.Display(PunchIntent.LeadHook));
            AssertEqual("REAR HOOK", PunchLabels.Display(PunchIntent.RearHook));
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

        private static void TestActionReset()
        {
            TimedActionState state = new();
            state.TryStart(PunchIntent.RearHook);
            state.ResetToGuard();
            AssertEqual(ActionPhase.Guard, state.Phase);
            AssertEqual(PunchIntent.None, state.Intent);
            AssertTrue(!state.IsBusy, "reset must lock action back to guard");
        }

        private static void TestOnboardingHead()
        {
            OnboardingProgress progress = new();
            progress.ObserveHead(-0.16f);
            AssertTrue(progress.HeadLeft, "left head motion must register");
            AssertTrue(!progress.HeadReady, "one side alone must not complete head drill");
            progress.ObserveHead(0.17f);
            AssertTrue(progress.HeadReady, "both head directions must complete head drill");
        }

        private static void TestOnboardingFootwork()
        {
            OnboardingProgress progress = new();
            progress.ObserveMovement(new Vector2(-0.8f, 0f));
            progress.ObserveMovement(new Vector2(0.8f, 0f));
            progress.ObserveMovement(new Vector2(0f, 0.8f));
            AssertTrue(!progress.FootworkReady, "three directions must not complete footwork drill");
            progress.ObserveMovement(new Vector2(0f, -0.8f));
            AssertTrue(progress.FootworkReady, "four directions must complete footwork drill");
        }

        private static void TestOnboardingPunches()
        {
            OnboardingProgress progress = new();
            progress.ObservePunch(PunchIntent.Jab);
            progress.ObservePunch(PunchIntent.Cross);
            progress.ObservePunch(PunchIntent.LeadHook);
            AssertTrue(!progress.PunchesReady, "three punch types must not complete punch drill");
            progress.ObservePunch(PunchIntent.RearHook);
            AssertTrue(progress.PunchesReady, "all four punch types must complete punch drill");
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
