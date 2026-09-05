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
            Run("P1-A2 tap straight family", TestGestureTapStraight, results);
            Run("P1-A2 held swipe up uppercut", TestGestureUppercut, results);
            Run("P1-A2 held swipe down overhand", TestGestureOverhand, results);
            Run("P1-A2 held swipe horizontal hook", TestGestureHook, results);
            Run("P1-A2 hand selector", TestPunchHandSelector, results);
            Run("punch labels and families", TestPunchLabels, results);
            Run("geometry hit and miss", TestGeometry, results);
            Run("default fight range can connect", TestDefaultFightRange, results);
            Run("anti-spam state transition", TestActionState, results);
            Run("counter recovery window", TestCounterWindow, results);
            Run("action reset to guard", TestActionReset, results);
            Run("onboarding head progress", TestOnboardingHead, results);
            Run("onboarding footwork progress", TestOnboardingFootwork, results);
            Run("onboarding punch-family progress", TestOnboardingPunches, results);
            Run("P1-A1 straight reach ordering", TestP1A1StraightReachOrdering, results);
            Run("P1-A1 neutral baseline", TestP1A1NeutralBaseline, results);
            Run("P1-A1 non-straights unchanged", TestP1A1NonStraightsUnchanged, results);
            Run("P1-A1 causal same-distance boundary", TestP1A1CausalBoundary, results);
            Run("P1-B1 opponent reach clamp", TestOpponentReachClamp, results);
            Run("P1-B1 in-range endpoint unchanged", TestOpponentReachInsideUnchanged, results);

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

        private static void TestGestureTapStraight()
        {
            GestureMetrics metrics = new(new Vector2(4f, 3f), 6f, 0.10f);
            AssertEqual(PunchFamily.Straight, PunchGestureClassifier.ClassifyFamily(metrics));
        }

        private static void TestGestureUppercut()
        {
            GestureMetrics metrics = new(new Vector2(8f, 120f), 123f, 0.24f);
            AssertEqual(PunchFamily.Uppercut, PunchGestureClassifier.ClassifyFamily(metrics));
        }

        private static void TestGestureOverhand()
        {
            GestureMetrics metrics = new(new Vector2(-6f, -125f), 128f, 0.26f);
            AssertEqual(PunchFamily.Overhand, PunchGestureClassifier.ClassifyFamily(metrics));
        }

        private static void TestGestureHook()
        {
            GestureMetrics metrics = new(new Vector2(150f, 24f), 158f, 0.25f);
            AssertEqual(PunchFamily.Hook, PunchGestureClassifier.ClassifyFamily(metrics));

            GestureMetrics tooFast = new(new Vector2(150f, 10f), 151f, 0.06f);
            AssertEqual(PunchFamily.None, PunchGestureClassifier.ClassifyFamily(tooFast));
        }

        private static void TestPunchHandSelector()
        {
            AssertEqual(PunchIntent.Jab, PunchHandSelector.Select(PunchFamily.Straight, PunchIntent.None));
            AssertEqual(PunchIntent.Cross, PunchHandSelector.Select(PunchFamily.Straight, PunchIntent.Jab));
            AssertEqual(PunchIntent.LeadHook, PunchHandSelector.Select(PunchFamily.Hook, PunchIntent.Cross));
            AssertEqual(PunchIntent.RearUppercut, PunchHandSelector.Select(PunchFamily.Uppercut, PunchIntent.LeadHook));
            AssertEqual(PunchIntent.RearOverhand, PunchHandSelector.Select(PunchFamily.Overhand, PunchIntent.None));
        }

        private static void TestPunchLabels()
        {
            AssertEqual("LEAD JAB", PunchLabels.Display(PunchIntent.Jab));
            AssertEqual("REAR CROSS", PunchLabels.Display(PunchIntent.Cross));
            AssertEqual("LEAD HOOK", PunchLabels.Display(PunchIntent.LeadHook));
            AssertEqual("REAR HOOK", PunchLabels.Display(PunchIntent.RearHook));
            AssertEqual("LEAD UPPERCUT", PunchLabels.Display(PunchIntent.LeadUppercut));
            AssertEqual("REAR UPPERCUT", PunchLabels.Display(PunchIntent.RearUppercut));
            AssertEqual("REAR OVERHAND", PunchLabels.Display(PunchIntent.RearOverhand));
            AssertEqual(PunchFamily.Uppercut, PunchLabels.Family(PunchIntent.LeadUppercut));
            AssertEqual(PunchFamily.Overhand, PunchLabels.Family(PunchIntent.RearOverhand));
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
            progress.ObservePunch(PunchIntent.LeadHook);
            progress.ObservePunch(PunchIntent.LeadUppercut);
            AssertTrue(!progress.PunchesReady, "three punch families must not complete punch drill");
            progress.ObservePunch(PunchIntent.RearOverhand);
            AssertTrue(progress.PunchesReady, "straight/hook/uppercut/overhand must complete punch drill");
        }

        private static void TestP1A1StraightReachOrdering()
        {
            float advancing = P1PunchMechanics.EffectiveStraightReachFactor(PunchIntent.Jab, "ADVANCING");
            float neutral = P1PunchMechanics.EffectiveStraightReachFactor(PunchIntent.Jab, "NEUTRAL");
            float retreating = P1PunchMechanics.EffectiveStraightReachFactor(PunchIntent.Jab, "RETREATING");
            AssertTrue(advancing > neutral, "advancing straight reach must exceed neutral");
            AssertTrue(neutral > retreating, "neutral straight reach must exceed retreating");
            AssertNear(1.06f, advancing, 0.0001f);
            AssertNear(1.00f, neutral, 0.0001f);
            AssertNear(0.94f, retreating, 0.0001f);
        }

        private static void TestP1A1NeutralBaseline()
        {
            Vector3 target = new(0.04f, 1.44f, 1.34f);
            Vector3 applied = P1PunchMechanics.ApplyA1StraightReach(PunchIntent.Cross, target, "NEUTRAL");
            AssertNear(target.x, applied.x, 0.0001f);
            AssertNear(target.y, applied.y, 0.0001f);
            AssertNear(target.z, applied.z, 0.0001f);
        }

        private static void TestP1A1NonStraightsUnchanged()
        {
            Vector3 target = new(0.11f, 1.39f, 1.05f);
            PunchIntent[] nonStraights =
            {
                PunchIntent.LeadHook,
                PunchIntent.RearHook,
                PunchIntent.LeadUppercut,
                PunchIntent.RearUppercut,
                PunchIntent.LeadOverhand,
                PunchIntent.RearOverhand
            };

            foreach (PunchIntent intent in nonStraights)
            {
                Vector3 advancing = P1PunchMechanics.ApplyA1StraightReach(intent, target, "ADVANCING");
                Vector3 retreating = P1PunchMechanics.ApplyA1StraightReach(intent, target, "RETREATING");
                AssertNear(target.z, advancing.z, 0.0001f);
                AssertNear(target.z, retreating.z, 0.0001f);
            }
        }

        private static void TestP1A1CausalBoundary()
        {
            Vector3 start = Vector3.zero;
            Vector3 baseTarget = new(0f, 0f, 1f);
            Vector3 advancing = P1PunchMechanics.ApplyA1StraightReach(PunchIntent.Jab, baseTarget, "ADVANCING");
            Vector3 neutral = P1PunchMechanics.ApplyA1StraightReach(PunchIntent.Jab, baseTarget, "NEUTRAL");
            Vector3 retreating = P1PunchMechanics.ApplyA1StraightReach(PunchIntent.Jab, baseTarget, "RETREATING");

            Vector3 farBoundary = new(0f, 0f, 1.03f);
            AssertTrue(CombatGeometry.SegmentSphereIntersects(start, advancing, farBoundary, 0.01f), "advancing jab must cross far boundary");
            AssertTrue(!CombatGeometry.SegmentSphereIntersects(start, neutral, farBoundary, 0.01f), "neutral jab must not cross far boundary");

            Vector3 nearBoundary = new(0f, 0f, 0.97f);
            AssertTrue(CombatGeometry.SegmentSphereIntersects(start, neutral, nearBoundary, 0.01f), "neutral jab must cross near boundary");
            AssertTrue(!CombatGeometry.SegmentSphereIntersects(start, retreating, nearBoundary, 0.01f), "retreating jab must not cross near boundary");
        }

        private static void TestOpponentReachClamp()
        {
            Vector3 start = Vector3.zero;
            Vector3 desired = new(0f, 0f, 2f);
            Vector3 clamped = OpponentReachMath.ClampEndpoint(start, desired, 1.02f);
            AssertNear(1.02f, Vector3.Distance(start, clamped), 0.0001f);
            AssertTrue(clamped.z < desired.z, "out-of-range target must be clamped before visual/hit resolution");
        }

        private static void TestOpponentReachInsideUnchanged()
        {
            Vector3 start = new(0.1f, 1.3f, 0.2f);
            Vector3 desired = new(0.1f, 1.5f, -0.5f);
            Vector3 resolved = OpponentReachMath.ClampEndpoint(start, desired, 1.02f);
            AssertNear(desired.x, resolved.x, 0.0001f);
            AssertNear(desired.y, resolved.y, 0.0001f);
            AssertNear(desired.z, resolved.z, 0.0001f);
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
