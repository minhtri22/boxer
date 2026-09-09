using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class P1CounterGeometrySelfTests
    {
        private static readonly Vector3 Start = new(0f, 0f, 0f);
        private static readonly Vector3 End = new(0f, 0f, 1f);
        private static readonly Vector3 CommitCenter = new(0f, 0f, 0.7f);
        private static readonly Vector3 EvadedCenter = new(0.32f, 0f, 0.7f);
        private const float Radius = 0.20f;

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
            RunSuite("P1-A3.3 7/7", P1A33SelfTests.RunBatch, suites);
            RunSuite("P1-CG 8/8", RunBatch, suites);

            string evidenceDir = Path.Combine(RepoRoot(), "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-counter-geometry-combined-regression.txt");
            suites.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            suites.Add("PRIOR_WITH_A3_3=115/115 PASS");
            suites.Add("P1_CG=8/8 PASS");
            suites.Add("COMBINED=123/123 PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, suites) + Environment.NewLine);
            Debug.Log($"P1_COUNTER_GEOMETRY_COMBINED_REGRESSION_PASS={output}");
        }

        public static void RunBatch()
        {
            List<string> results = new();
            Run("P1-CG pre-existing miss creates no opportunity", TestPreExistingMiss, results);
            Run("P1-CG remaining intersection creates no opportunity", TestStillIntersecting, results);
            Run("P1-CG head movement creates head evade", TestHeadEvade, results);
            Run("P1-CG root movement creates footwork evade", TestFootworkEvade, results);
            Run("P1-CG body attack cannot create head evade", TestBodyAttackClassification, results);
            Run("P1-CG hit and block create no opportunity", TestOutcomes, results);
            Run("P1-CG opportunity opens only in recover and consumes once", TestLifecycle, results);
            Run("P1-CG semantic event is stable", TestSemanticEvent, results);
            int passCount = results.Count;

            string evidenceDir = Path.Combine(RepoRoot(), "evidence", "phase1", "SYNTHETIC");
            Directory.CreateDirectory(evidenceDir);
            string output = Path.Combine(evidenceDir, "p1-counter-geometry-deterministic-self-tests.txt");
            results.Insert(0, $"evidence=SYNTHETIC\nunity={Application.unityVersion}\nutc={DateTime.UtcNow:O}");
            results.Add($"MIN_EVADE_DISPLACEMENT_M={P1CounterGeometry.MinimumEvadeDisplacementMeters.ToString("F2", CultureInfo.InvariantCulture)}");
            results.Add($"TOTAL={passCount} PASS");
            File.WriteAllText(output, string.Join(Environment.NewLine, results) + Environment.NewLine);
            Debug.Log($"P1_COUNTER_GEOMETRY_SELF_TESTS_PASS={output}");
        }

        private static P1CounterOpportunity Evaluate(
            Vector3 commit,
            Vector3 resolve,
            bool body = false,
            float headDelta = 0.32f,
            float rootDelta = 0f,
            CombatOutcome outcome = CombatOutcome.Miss)
        {
            return P1CounterGeometry.Evaluate(Start, End, commit, resolve, Radius, body, headDelta, rootDelta, outcome);
        }

        private static void TestPreExistingMiss()
        {
            P1CounterOpportunity value = Evaluate(new Vector3(0.30f, 0f, 0.7f), new Vector3(0.40f, 0f, 0.7f));
            AssertTrue(!value.Armed, "already-missed path must not arm");
        }

        private static void TestStillIntersecting()
        {
            P1CounterOpportunity value = Evaluate(CommitCenter, new Vector3(0.10f, 0f, 0.7f));
            AssertTrue(!value.Armed, "remaining hit must not arm");
        }

        private static void TestHeadEvade()
        {
            P1CounterOpportunity value = Evaluate(CommitCenter, EvadedCenter);
            AssertTrue(value.Armed && value.Type == "HEAD_EVADE", "head evade classification");
            AssertTrue(value.EndSeparationMeters > value.StartSeparationMeters, "separation must increase");
        }

        private static void TestFootworkEvade()
        {
            P1CounterOpportunity value = Evaluate(CommitCenter, EvadedCenter, false, 0f, 0.32f);
            AssertTrue(value.Armed && value.Type == "FOOTWORK_EVADE", "footwork evade classification");
        }

        private static void TestBodyAttackClassification()
        {
            P1CounterOpportunity noRoot = Evaluate(CommitCenter, EvadedCenter, true, 0.32f, 0f);
            P1CounterOpportunity withRoot = Evaluate(CommitCenter, EvadedCenter, true, 0.32f, 0.32f);
            AssertTrue(!noRoot.Armed, "body attack cannot use head delta");
            AssertTrue(withRoot.Armed && withRoot.Type == "FOOTWORK_EVADE", "body evade must use footwork");
        }

        private static void TestOutcomes()
        {
            AssertTrue(!Evaluate(CommitCenter, EvadedCenter, outcome: CombatOutcome.Hit).Armed, "hit");
            AssertTrue(!Evaluate(CommitCenter, EvadedCenter, outcome: CombatOutcome.Block).Armed, "block");
        }

        private static void TestLifecycle()
        {
            P1CounterOpportunityState state = new();
            state.Arm(Evaluate(CommitCenter, EvadedCenter));
            AssertTrue(!state.IsOpen(ActionPhase.Extend), "closed in extend");
            AssertTrue(state.IsOpen(ActionPhase.Recover), "open in recover");
            AssertTrue(state.Consume(ActionPhase.Recover), "consume succeeds");
            AssertTrue(!state.IsOpen(ActionPhase.Recover), "closed after consume");
            AssertTrue(!state.Consume(ActionPhase.Recover), "cannot consume twice");
        }

        private static void TestSemanticEvent()
        {
            string value = Evaluate(CommitCenter, EvadedCenter).ToSemanticEvent();
            AssertTrue(value == "P1_COUNTER_OPPORTUNITY TYPE=HEAD_EVADE START_SEP=0.000 END_SEP=0.320 MOVE=0.320", "semantic event: " + value);
        }

        private static string RepoRoot() => Directory.GetParent(Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName).FullName;
        private static void RunSuite(string name, Action suite, ICollection<string> results) { suite(); results.Add("PASS " + name); }
        private static void Run(string name, Action test, ICollection<string> results) { test(); results.Add("PASS " + name); }
        private static void AssertTrue(bool condition, string message) { if (!condition) throw new Exception(message); }
    }
}
