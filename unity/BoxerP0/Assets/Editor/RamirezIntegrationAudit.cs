using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BoxerP0.Editor
{
    [InitializeOnLoad]
    public static class RamirezIntegrationAudit
    {
        static string Root => Path.GetFullPath(Path.Combine(Application.dataPath,"../../.."));
        static string Evidence => Path.GetFullPath(Path.Combine(Root,"evidence","ramirez-unity-round2"));
        static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        static readonly StringBuilder Log=new();
        static double _start,_next;
        static int _frame=-1,_frames,_shot,_punch;
        static float _gloveError,_footError,_maxMs,_sumMs,_rootDrift,_rotationDrift;
        static long _allocated;
        static bool _busy;
        static Vector3 _locked;
        static Quaternion _rotation;
        static readonly HashSet<string> Seen=new();
        static readonly PunchIntent[] Intents={PunchIntent.Jab,PunchIntent.Cross,PunchIntent.LeadHook,PunchIntent.RearHook,PunchIntent.LeadUppercut,PunchIntent.RearUppercut,PunchIntent.LeadOverhand,PunchIntent.RearOverhand};
        static RamirezIntegrationAudit()
        {
            if(SessionState.GetBool("ramirezAudit",false)) EditorApplication.update+=Tick;
        }
        public static void Tests()
        {
            Directory.CreateDirectory(Evidence);
            // Legacy suites write fixed historical paths: preserve bytes and copy this run separately.
            var historical=Directory.GetFiles(Path.Combine(Root,"evidence"),"*",SearchOption.AllDirectories)
                .Where(p=>!p.StartsWith(Evidence,StringComparison.OrdinalIgnoreCase))
                .ToDictionary(p=>p,p=>File.ReadAllBytes(p));
            var log=new StringBuilder(); int failures=0;
            void Run(string name,Action test)
            {
                try { test(); log.AppendLine("PASS "+name); }
                catch(Exception e) { failures++; log.AppendLine("FAIL "+name+" "+e); }
            }
            try
            {
                Run("Round2SelfTests.RunAll",Round2SelfTests.RunAll);
                Run("Blender3DAssetAudit.Run",Blender3DAssetAudit.Run);
                Run("Historical EVEvaluation.ShellTests",EVEvaluation.ShellTests);
                Run("Accepted native rig import",()=>
                {
                    var prefab=Resources.Load<GameObject>("Boxer3D/Ramirez_UAT3");
                    if(prefab==null) throw new Exception("Missing FBX");
                    var rig=RamirezAcceptedRig.Find(prefab.transform,"RAMIREZ_RIG");
                    var meshes=prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
                    log.AppendLine("meshes="+meshes.Length+" triangles="+meshes.Sum(s=>s.sharedMesh.triangles.Length/3));
                    if(meshes.Length!=31) throw new Exception("Expected 31 explicit fighter meshes");
                    foreach(var s in meshes)
                        if(s.sharedMesh.bindposes.Length==0 || s.bones.Any(b=>b==null)) throw new Exception("Invalid bind "+s.name);
                    foreach(string side in new[]{"L","R"})
                    {
                        var upper=RamirezAcceptedRig.Find(rig,"upperarm."+side);
                        var lower=RamirezAcceptedRig.Find(rig,"forearm."+side);
                        var hand=RamirezAcceptedRig.Find(rig,"hand."+side);
                        float u=Vector3.Distance(upper.position,lower.position), l=Vector3.Distance(lower.position,hand.position);
                        log.AppendLine($"side={side} upper_m={u:R} forearm_m={l:R} root_scale={rig.lossyScale:R}");
                        if(Mathf.Abs(u-.29624483f)>.0001f || Mathf.Abs(l-.23955384f)>.0001f) throw new Exception("Native arm scale changed");
                    }
                    if(prefab.GetComponentsInChildren<Camera>().Length>0 || prefab.GetComponentsInChildren<Light>().Length>0) throw new Exception("Authoring aids imported");
                    foreach(var t in prefab.GetComponentsInChildren<Transform>())
                        if(t.name.Contains("REFERENCE") || t.name=="Plane") throw new Exception("Authoring geometry imported");
                });
            }
            finally
            {
                foreach(var entry in historical)
                {
                    if(File.Exists(entry.Key) && !File.ReadAllBytes(entry.Key).SequenceEqual(entry.Value))
                    {
                        string dest=Path.Combine(Evidence,"regressions",Path.GetRelativePath(Path.Combine(Root,"evidence"),entry.Key));
                        Directory.CreateDirectory(Path.GetDirectoryName(dest)); File.Copy(entry.Key,dest,true);
                        File.WriteAllBytes(entry.Key,entry.Value);
                    }
                }
                log.AppendLine("FAILED_SUITES="+failures);
                File.WriteAllText(Path.Combine(Evidence,"tests.txt"),log.ToString());
                Debug.Log(log);
            }
        }
        public static void Runtime()
        {
            Directory.CreateDirectory(Evidence);
            File.Copy(Path.Combine(Application.dataPath,"Scenes/Phase0Boxer.unity"),Path.Combine(Evidence,"scene-before.bytes"),true);
            Phase0SceneBuilder.Build();
            SessionState.SetBool("ramirezAudit",true);
            EditorApplication.update-=Tick;EditorApplication.update+=Tick;
            EditorApplication.isPlaying=true;
        }
        static void Tick()
        {
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling||_frame==Time.frameCount) return;
            _frame=Time.frameCount;
            var follower=UnityEngine.Object.FindFirstObjectByType<Blender3DVisualFollower>();
            var p=UnityEngine.Object.FindFirstObjectByType<PlayerBoxer>();
            var o=UnityEngine.Object.FindFirstObjectByType<OpponentBoxer>();
            var combat=UnityEngine.Object.FindFirstObjectByType<Round2CombatRig>();
            var foot=UnityEngine.Object.FindFirstObjectByType<Round2Footwork>();
            if(follower==null||!follower.Ready||follower.AcceptedRig==null||p==null||o==null) return;
            if(_start==0)
            {
                _start=EditorApplication.timeSinceStartup;
                typeof(BoxerBootstrap).GetMethod("StartBout",Flags).Invoke(UnityEngine.Object.FindFirstObjectByType<BoxerBootstrap>(),null);
                Log.AppendLine("SYNTHETIC_EDITOR_PLAYMODE_NOT_HUMAN_UAT unity="+Application.unityVersion);
                Log.AppendLine("scenario=all opponent punch families through real controller phase progression; external player advance/retreat/lateral; no authority changes");
            }
            double elapsed=EditorApplication.timeSinceStartup-_start;
            var a=follower.AcceptedRig;
            _frames++;_sumMs+=a.LastUpdateMs;_maxMs=Mathf.Max(_maxMs,a.LastUpdateMs);_allocated+=a.LastAllocatedBytes;
            _gloveError=Mathf.Max(_gloveError,a.LastGloveError);_footError=Mathf.Max(_footError,a.LastFootError);
            if(o.IsActionBusy)
            {
                if(!_busy) {_locked=o.transform.position;_rotation=o.transform.rotation;}
                else {_rootDrift=Mathf.Max(_rootDrift,Vector3.Distance(_locked,o.transform.position));_rotationDrift=Mathf.Max(_rotationDrift,Quaternion.Angle(_rotation,o.transform.rotation));}
            }
            _busy=o.IsActionBusy;
            string state=o.CurrentIntent+":"+o.CurrentPhase;
            if(Seen.Add(state)) { Log.AppendLine($"observed={state} t={elapsed:F3}"); Capture("state-"+state.Replace(":","-")); }
            if(elapsed>_next && !o.IsActionBusy && _punch<Intents.Length)
            {
                // Isolated audit fixture: use the existing state machine and frozen durations/targets.
                var action=(TimedActionState)typeof(OpponentBoxer).GetField("_action",Flags).GetValue(o);
                var intent=Intents[_punch++];
                action.TryStart(intent);
                typeof(OpponentBoxer).GetField("_attackTargetLocal",Flags).SetValue(o,Round2Motion.Endpoint(intent,"NEUTRAL",Vector3.Distance(p.transform.position,o.transform.position)));
                typeof(OpponentBoxer).GetField("_resolvedThisAttack",Flags).SetValue(o,false);
                _next=elapsed+1.0;
            }
            if(elapsed>12&&elapsed<15) p.transform.position+=Vector3.back*.25f*Time.deltaTime;
            if(elapsed>18&&elapsed<21) p.transform.position+=Vector3.forward*.30f*Time.deltaTime;
            if(elapsed>24&&elapsed<27) p.transform.position+=Vector3.right*.18f*Time.deltaTime;
            if(elapsed>1+_shot*3&&_shot<11)
            {
                Capture("motion-"+(_shot++).ToString("00"));
                ContactSurface(o,combat);
                Log.AppendLine($"t={elapsed:F2} distance={Vector3.Distance(p.transform.position,o.transform.position):R} state={state} contacts={combat.Contacts} steps={foot.Steps} glove_error_m={_gloveError:R} foot_error_m={_footError:R}");
            }
            if(elapsed<34) return;
            Log.AppendLine($"frames={_frames} adapter_mean_ms={_sumMs/_frames:R} adapter_max_ms={_maxMs:R} adapter_allocated_bytes={_allocated} glove_center_max_error_m={_gloveError:R} foot_max_error_m={_footError:R}");
            Log.AppendLine($"root_commit_drift_m={_rootDrift:R} rotation_commit_drift_deg={_rotationDrift:R} contacts={combat.Contacts} steps={foot.Steps} planted_error_m={foot.PlantedDrift:R}");
            bool ownership=o.GetComponentsInChildren<Renderer>(true).All(r=>!r.enabled)&&p.GetComponentsInChildren<Renderer>(true).All(r=>!r.enabled);
            Log.AppendLine("single_visual_ownership="+ownership);
            Log.AppendLine("native_arm_lengths_m="+a.UpperArmLength.ToString("R")+","+a.ForearmLength.ToString("R"));
            Log.AppendLine("native_bone_length_max_error_m="+a.MaxNativeLengthError.ToString("R"));
            Log.AppendLine("glove_contact_coherence_1mm="+(_gloveError<=.001f?"PASS":"FAIL"));
            Log.AppendLine("performance_scope=Editor only; paired baseline and WebGL frame behavior not established here");
            File.WriteAllText(Path.Combine(Evidence,"runtime.txt"),Log.ToString());
            SessionState.SetBool("ramirezAudit",false);EditorApplication.update-=Tick;
            EditorApplication.isPlaying=false;
            File.Copy(Path.Combine(Evidence,"scene-before.bytes"),Path.Combine(Application.dataPath,"Scenes/Phase0Boxer.unity"),true);
            EditorApplication.Exit(0);
        }
        static void Capture(string name)
        {
            var camera=UnityEngine.Object.FindFirstObjectByType<Camera>();camera.aspect=9f/16f;
            var rt=new RenderTexture(540,960,24);var old=camera.targetTexture;var active=RenderTexture.active;
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            var texture=new Texture2D(540,960,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,540,960),0,0);texture.Apply();
            string folder=Path.Combine(Evidence,"captures");Directory.CreateDirectory(folder);
            File.WriteAllBytes(Path.Combine(folder,name+".png"),texture.EncodeToPNG());
            camera.targetTexture=old;RenderTexture.active=active;
            UnityEngine.Object.DestroyImmediate(texture);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
        }
        static void ContactSurface(OpponentBoxer opponent,Round2CombatRig combat)
        {
            var root=UnityEngine.Object.FindFirstObjectByType<Blender3DVisualFollower>().OpponentVisualRoot;
            var skin=RamirezAcceptedRig.Find(root,"Studio Ramirez continuous sculpt").GetComponent<SkinnedMeshRenderer>();
            var mesh=new Mesh();skin.BakeMesh(mesh);
            var fixture=new GameObject("Audit surface query only");
            fixture.transform.SetPositionAndRotation(skin.transform.position,skin.transform.rotation);
            fixture.transform.localScale=skin.transform.lossyScale;
            var collider=fixture.AddComponent<MeshCollider>();collider.sharedMesh=mesh;
            Physics.SyncTransforms();
            var frame=combat.OpponentFrame();
            foreach(int target in new[]{2,3,4,5,6})
            {
                Vector3 center=frame.Target(target),forward=opponent.transform.forward;
                bool hit=collider.Raycast(new Ray(center+forward*2,-forward),out var contact,4);
                float gap=hit?Vector3.Distance(center+forward*Round2CombatRig.TargetRadius(target),contact.point):float.NaN;
                Log.AppendLine($"surface_target={target} ray_hit={hit} front_contact_mesh_gap_m={gap:R}");
            }
            UnityEngine.Object.DestroyImmediate(fixture);UnityEngine.Object.DestroyImmediate(mesh);
        }
    }
}
