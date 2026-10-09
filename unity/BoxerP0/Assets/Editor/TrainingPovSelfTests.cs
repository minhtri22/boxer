using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class TrainingPovSelfTests
    {
        public static void Run()
        {
            CombatFairnessSelfTests.Run();
            var log = new StringBuilder("SCOPE=PRESENTATION_GEOMETRY_NOT_RENDERED_REFERENCE_OR_HUMAN_UAT\n");
            int passed = 0, failed = 0;
            void Check(bool ok, string label) { if (ok) passed++; else failed++; log.AppendLine((ok ? "PASS " : "FAIL ") + label); }
            var root = new GameObject("Presentation test anchors");
            var cameraObject = new GameObject("POV projection test");
            var camera = cameraObject.AddComponent<Camera>(); camera.enabled = false;
            camera.fieldOfView = 108; camera.aspect = 9f / 16f; camera.nearClipPlane = .04f;
            camera.transform.position = new Vector3(0, 1.64f, .01f); camera.transform.rotation = Quaternion.Euler(23, 0, 0);
            try
            {
                foreach (bool left in new[] { true, false })
                {
                    var obj = new GameObject(left ? "Left test arm" : "Right test arm"); obj.transform.SetParent(root.transform);
                    var arm = obj.AddComponent<PlayerPOVArmVisual>(); arm.Initialize();
                    Check(obj.GetComponentsInChildren<Collider>().Length == 0, "arm has no collider " + left);
                    foreach (var intent in new[] { PunchIntent.None, PunchIntent.Jab, PunchIntent.Cross, PunchIntent.LeadHook, PunchIntent.RearHook,
                        PunchIntent.LeadUppercut, PunchIntent.RearUppercut, PunchIntent.LeadOverhand, PunchIntent.RearOverhand })
                        foreach (var phase in new[] { ActionPhase.Commit, ActionPhase.Extend, ActionPhase.Recover })
                            foreach (float t in new[] { 0f, .25f, .5f, .75f, 1f })
                            {
                                var solution = Round2Motion.Sample(left, intent, phase, t, Round2Motion.Endpoint(intent, "NEUTRAL", .82f));
                                var position = root.transform.position; var rotation = root.transform.rotation;
                                arm.Apply(solution.Shoulder, solution.Wrist, root.transform, left);
                                var mesh = obj.GetComponent<MeshFilter>().sharedMesh;
                                Check(arm.Wrist == solution.Wrist && arm.Shoulder == solution.Shoulder && root.transform.position == position && root.transform.rotation == rotation,
                                    "read-only shared endpoints " + left + " " + intent + " " + phase + " " + t);
                                var vertices = mesh.vertices;
                                Check(mesh.vertexCount == 800 && mesh.triangles.Length / 3 == 1536 && vertices.All(v => float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.z))
                                    && vertices.Skip(32).Select((v, i) => Vector3.Distance(v, vertices[i])).All(edge => edge < .08f),
                                    "finite bounded continuous mesh " + left + " " + intent + " " + phase + " " + t);
                                if (intent == PunchIntent.None && t == 0)
                                {
                                    var point = camera.WorldToViewportPoint(arm.Elbow);
                                    Check(point.z > 0 && point.x > .02f && point.x < .98f && point.y > .02f && point.y < .98f, "portrait guard elbow inside frustum " + left + " " + phase);
                                    log.AppendLine("elbow_viewport=" + point.ToString("F4"));
                                }
                            }
                }
                Check(Resources.Load<Shader>("TrainingGlassBlur") != null, "blur shader included through Resources");
                Check(Resources.Load<Shader>("POVArmSkin") != null, "skin shader included through Resources");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); UnityEngine.Object.DestroyImmediate(cameraObject); }
            log.AppendLine($"TOTAL={passed + failed} PASS={passed} FAIL={failed}");
            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../evidence/wave1/training-pov"));
            Directory.CreateDirectory(dir); File.WriteAllText(Path.Combine(dir, "presentation-tests.txt"), log.ToString()); Debug.Log(log);
            if (failed != 0) throw new Exception("POV presentation invariant failure");
        }
    }
}
