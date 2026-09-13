using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BoxerP0.Editor
{
    // A negative-evidence diagnostic, separate from the frozen regression suites.
    // Uses real Unity math and OpponentBoxer resolution. Does not modify gameplay.
    public static class Uat2ContactAudit
    {
        public static void Run()
        {
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
            MethodInfo endpoint = typeof(PlayerBoxer).GetMethod("PunchTargetLocal", flags);
            MethodInfo commitment = typeof(PlayerBoxer).GetMethod("PunchCommitPose", flags);
            if (endpoint == null || commitment == null) throw new Exception("Player pose API changed");
            var output = new StringBuilder();
            output.AppendLine("evidence=UNITY_EDITOR_DETERMINISTIC_CONTACT_AUDIT_NOT_VISUAL_UAT");
            output.AppendLine("unity=" + Application.unityVersion);
            output.AppendLine("utc=" + DateTime.UtcNow.ToString("O"));
            output.AppendLine("fixture=Bootstrap default relative root distance 1.4m; neutral jab; original collider sizes and guard positions");
            GameObject root = new GameObject("UAT2 isolated opponent fixture");
            try
            {
                root.transform.position = new Vector3(0f, 0f, 1.4f);
                root.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                OpponentBoxer opponent = root.AddComponent<OpponentBoxer>();
                SphereCollider left = Sphere(root.transform, "Opponent Left Glove", new Vector3(-0.22f, 1.38f, 0.45f), new Vector3(0.24f, 0.21f, 0.28f), 0.5f);
                SphereCollider right = Sphere(root.transform, "Opponent Right Glove", new Vector3(0.22f, 1.38f, 0.45f), new Vector3(0.24f, 0.21f, 0.28f), 0.5f);
                SphereCollider head = Sphere(root.transform, "Opponent Head", new Vector3(0f, 1.62f, 0f), Vector3.one * 0.36f, 0.5f);
                SphereCollider body = Sphere(root.transform, "Opponent Body", new Vector3(0f, 1.24f, 0f), new Vector3(0.50f, 0.32f, 0.38f), 0.48f);
                body.center = new Vector3(0f, -0.19f, 0f);
                opponent.Initialize(null, null, left.transform, left, right.transform, right, head, body);

                Vector3 guard = new Vector3(-0.22f, 1.38f, 0.48f);
                Vector3 target = (Vector3)endpoint.Invoke(null, new object[] { PunchIntent.Jab, true });
                Vector3 commit = (Vector3)commitment.Invoke(null, new object[] { PunchIntent.Jab, guard, true });
                CombatOutcome result = opponent.ResolveIncomingPunch(guard, target, 0.09f, out string reason);
                output.AppendLine("authoritative_outcome=" + result);
                output.AppendLine("authoritative_reason=" + reason);

                // Actual ArmChainMath wrist solver, sampled over the complete extension.
                // Check the *authoritative* target volumes: even these have no visual glove contact.
                float minimumGap = float.PositiveInfinity;
                SphereCollider[] targets = { left, right, head, body };
                Vector3 shoulder = new Vector3(-0.38f, 1.43f, 0.02f);
                for (int i = 0; i <= 256; i++)
                {
                    float t = i / 256f;
                    float smooth = t * t * (3f - 2f * t);
                    Vector3 requested = Vector3.Lerp(commit, target, smooth);
                    ArmChainSolution solution = ArmChainMath.Solve(shoulder, requested,
                        shoulder + new Vector3(-0.28f, 0.10f, -0.12f),
                        ArmVisualEmbodiment.UpperArmLength, ArmVisualEmbodiment.ForearmLength);
                    foreach (SphereCollider sphere in targets)
                    {
                        Vector3 scale = sphere.transform.lossyScale;
                        float targetRadius = sphere.radius * Mathf.Max(scale.x, scale.y, scale.z);
                        float gap = Vector3.Distance(solution.Wrist, sphere.transform.TransformPoint(sphere.center)) - 0.115f - targetRadius;
                        minimumGap = Mathf.Min(minimumGap, gap);
                    }
                }
                output.AppendLine("minimum_visual_glove_surface_to_any_authoritative_target_surface_m=" + minimumGap.ToString("F6", CultureInfo.InvariantCulture));
                bool mismatch = result != CombatOutcome.Miss && minimumGap > 0f;
                output.AppendLine("M05_CONTACT_COHERENCE=" + (mismatch ? "FAIL" : "NOT_REPRODUCED"));
                output.AppendLine("note=Positive gap means no visual-glove overlap at any sampled extension pose. This is negative baseline evidence, not a fix or a passing regression test.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
            string repo = Path.GetFullPath(Path.Combine(Application.dataPath, "../../.."));
            string directory = Path.Combine(repo, "evidence", "uat-round2");
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "unity-contact-audit.txt"), output.ToString());
            Debug.Log(output.ToString());
        }

        private static SphereCollider Sphere(Transform parent, string name, Vector3 position, Vector3 scale, float radius)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            SphereCollider sphere = go.AddComponent<SphereCollider>();
            sphere.radius = radius;
            return sphere;
        }
    }
}
