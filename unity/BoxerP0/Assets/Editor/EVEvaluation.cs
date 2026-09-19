using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BoxerP0.Editor
{
    [InitializeOnLoad]
    public static class EVEvaluation
    {
        static int _auditFrames,_auditFailures;
        static float _anchorError;
        static double _skinMs;static long _skinAlloc;
        public static void AuditFrame()
        {
            var shell=UnityEngine.Object.FindFirstObjectByType<EVVisualShell>();
            if(shell==null||!shell.Ready)return;
            var opponent=UnityEngine.Object.FindFirstObjectByType<OpponentBoxer>();
            var player=UnityEngine.Object.FindFirstObjectByType<PlayerBoxer>();
            var anatomy=UnityEngine.Object.FindFirstObjectByType<EVAnatomy>();
            if(anatomy!=null){_skinMs+=anatomy.LastUpdateMs;_skinAlloc+=anatomy.LastAllocatedBytes;}
            if(shell.Torso!=null) _anchorError=Mathf.Max(_anchorError,Vector3.Distance(shell.Torso.position,opponent.transform.Find("R2 Chest").position));
            bool ok=true;
            foreach(var actor in new[]{player.transform,opponent.transform})foreach(string side in new[]{"Left","Right"}) {
                string prefix=actor==player.transform?"Player ":"Opponent ";
                Transform glove=actor.Find(prefix+side+" Glove"),elbow=actor.Find(side+" Elbow"),cuff=glove.Find("EV "+side+" Glove Cuff");
                ok &= glove.GetComponent<Renderer>().enabled && cuff!=null && cuff.parent==glove;
                if(cuff!=null)ok &= Vector3.Angle(cuff.up,glove.position-elbow.position)<.05f;
                int shoulders=0;foreach(Transform t in actor)if(t.name==side+" Shoulder"&&t.GetComponent<Renderer>().enabled)shoulders++;
                ok &= shoulders==(EVVisualShell.AnatomicalExperiment && actor==opponent.transform?0:1);
            }
            foreach(string side in new[]{"Left","Right"}) {
                Transform thigh=opponent.transform.Find(side+" Thigh"),shin=opponent.transform.Find(side+" Shin");
                ok &= thigh.Find("EV "+side+" Trunk Leg")!=null && shin.Find("EV "+side+" Boot Upper")!=null;
                ok &= opponent.transform.Find(side+" Shoe").GetComponent<Renderer>().enabled;
            }
            ok &= _anchorError<.00001f;
            if(!EVVisualShell.SegmentedExperiment)foreach(string n in new[]{"R2 Abdomen","R2 Chest","R2 Left Chest","R2 Right Chest","R2 Neck"})ok &= !opponent.transform.Find(n).GetComponent<Renderer>().enabled;
            if(!ok)_auditFailures++;_auditFrames++;
        }
        public static bool AuditFinal()
        {
            bool pass=_auditFrames>0&&_auditFailures==0;
            string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../evidence/p1-ev"));Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path,"ownership-runtime.txt"),$"frames={_auditFrames} failed_frames={_auditFailures} torso_anchor_error_m={_anchorError:R} skin_follow_mean_ms={_skinMs/Math.Max(1,_auditFrames):F6} skin_follow_allocated_bytes={_skinAlloc}\nchecks=torso_anchor,cuff_parent,cuff_forearm_alignment,single_shoulder,trunk_thigh_attachment,boot_shin_attachment,redundant_torso_renderers_off\nRESULT={(pass?"PASS":"FAIL")}\n");
            return pass;
        }
        static EVEvaluation() { System.Globalization.CultureInfo.CurrentCulture=System.Globalization.CultureInfo.InvariantCulture; EVVisualShell.SegmentedExperiment=Environment.GetEnvironmentVariable("BOXER_EV_VARIANT")=="A"; }
        public static void RunVisual() { ShellTests(); EVVisualShell.SegmentedExperiment=Environment.GetEnvironmentVariable("BOXER_EV_VARIANT")=="A"; Round2RuntimeAudit.Run(); }
        public static void Tests()
        {
            Round2SelfTests.RunAll();
            ShellTests();
            ContactTests();
        }
        public static void ContactTests()
        {
            var data=EVAnatomy.ReadAsset();EVContactSurface.Initialize(data);var log=new StringBuilder();int samples=0,disagree=0,oldFalseContact=0;float maxGap=0;
            // Independent Unity/PhysX swept-sphere oracle on the visible triangle mesh.
            foreach(bool head in new[]{true,false}) {
                int bone=head?1:0;Vector3 origin=new(0,head?1.62f:1.37f,0);var vertices=new Vector3[data.positions.Length/3];for(int i=0;i<vertices.Length;i++)vertices[i]=data.Point(i)-origin;
                var triangles=new System.Collections.Generic.List<int>();int[] source=head?data.head:data.body;
                for(int i=0;i<source.Length;i+=3){int a=source[i],b=source[i+1],c=source[i+2];if(data.weights[a*10+bone]>.999f&&data.weights[b*10+bone]>.999f&&data.weights[c*10+bone]>.999f){triangles.Add(a);triangles.Add(b);triangles.Add(c);}}
                var mesh=new Mesh{vertices=vertices,triangles=triangles.ToArray()};var go=new GameObject("EV Independent Physics Oracle");go.layer=31;var collider=go.AddComponent<MeshCollider>();collider.sharedMesh=mesh;Physics.SyncTransforms();
                for(int x=-8;x<=8;x++)for(int y=-8;y<=8;y++) {
                    Vector3 a=new(x*.04f,y*.035f,.65f),b=new(x*.04f,y*.035f,-.3f),d=b-a;
                    bool actual=EVContactSurface.SweepLocal(a,b,head,out float t);
                    bool oracle=Physics.SphereCast(a,Round2Motion.GloveRadius,d.normalized,out RaycastHit hit,d.magnitude,1<<31,QueryTriggerInteraction.Ignore);
                    samples++;if(actual!=oracle){disagree++;log.AppendLine($"DISAGREE head={head} x={x} y={y} surface={actual} physics={oracle} t={t:R} physicsDistance={hit.distance:R}");}if(actual)maxGap=Mathf.Max(maxGap,Mathf.Abs(EVContactSurface.Distance(Vector3.Lerp(a,b,t),head)-Round2Motion.GloveRadius));
                    if(head&&Round2Motion.Sweep(a,b,Vector3.zero,Vector3.zero,Round2Motion.GloveRadius+Round2Motion.HeadRadius,out float oldT)&&EVContactSurface.Distance(Vector3.Lerp(a,b,oldT),true)-Round2Motion.GloveRadius>.001f)oldFalseContact++;
                }
                UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(mesh);
            }
            int matrix=0,contact=0,miss=0;
            foreach(PunchIntent intent in new[]{PunchIntent.Jab,PunchIntent.Cross,PunchIntent.LeadHook,PunchIntent.RearHook,PunchIntent.LeadUppercut,PunchIntent.RearUppercut,PunchIntent.LeadOverhand,PunchIntent.RearOverhand})foreach(float distance in new[]{1.35f,.955f,.65f})foreach(string step in new[]{"ADVANCING","NEUTRAL","RETREATING"}) foreach(bool guard in new[]{true,false}) {
                var defender=new Round2Frame{Position=new Vector3(0,0,distance),Rotation=Quaternion.Euler(0,180,0),Intent=PunchIntent.None,Phase=ActionPhase.Guard};
                Vector3 endpoint=Round2Motion.Endpoint(intent,step,distance);bool left=!PunchLabels.IsRearHand(intent);Vector3 previous=Round2Motion.Sample(left,intent,ActionPhase.Extend,0,endpoint).Wrist;int result=-1;
                for(int i=1;i<=120;i++){Vector3 current=Round2Motion.Sample(left,intent,ActionPhase.Extend,i/120f,endpoint).Wrist;int h=Round2CombatRig.SweepTargets(previous,current,defender,defender,guard,0,out float _,true);if(h>=0){result=h;break;}previous=current;}
                matrix++;if(result>=0)contact++;else miss++;log.AppendLine($"MATRIX {intent} distance={distance:F3} step={step} guard={guard} outcome={(result<0?"MISS":result<2?"BLOCK":"HIT")}");
            }
            // Warmed bounded contact work, separate from the Editor render loop.
            for(int i=0;i<100;i++)EVContactSurface.SweepLocal(new Vector3(0,0,.45f),new Vector3(0,0,.15f),true,out _);
            var sw=new System.Diagnostics.Stopwatch();long allocated=GC.GetAllocatedBytesForCurrentThread();sw.Start();for(int i=0;i<5000;i++)EVContactSurface.SweepLocal(new Vector3(0,0,.45f),new Vector3(0,0,.15f),true,out _);sw.Stop();allocated=GC.GetAllocatedBytesForCurrentThread()-allocated;
            log.AppendLine($"oracle_samples={samples} disagreement={disagree} max_contact_surface_gap_m={maxGap:R} old_sphere_early_contact_cases={oldFalseContact} matrix={matrix} contacts={contact} misses={miss} head_triangles={EVContactSurface.HeadTriangles} body_triangles={EVContactSurface.BodyTriangles} warmed_sweep_us={sw.Elapsed.TotalMilliseconds/5:F3} allocated_bytes={allocated}");
            log.AppendLine($"neutral_jab_head_boundary_m={EVContactSurface.JabReachBoundary:R} neutral_cross_head_boundary_m={EVContactSurface.CrossReachBoundary:R} preferred_ai_distance_m={EVContactSurface.PreferredDistance:R} sweep_budget_exhaustions={EVContactSurface.SweepBudgetExhaustions}");
            bool pass=disagree==0&&maxGap<.000011f&&oldFalseContact>0&&matrix==144&&contact>0&&miss>0&&EVContactSurface.SweepBudgetExhaustions==0;log.AppendLine("RESULT="+(pass?"PASS":"FAIL"));
            string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../evidence/p1-ev"));Directory.CreateDirectory(path);File.WriteAllText(Path.Combine(path,"contact-tests.txt"),log.ToString());Debug.Log(log);if(!pass)throw new Exception("EV visible-surface contact oracle failed");
        }
        public static void ShellTests()
        {
            var log=new StringBuilder();int passed=0;
            void Check(bool value,string name) {log.AppendLine((value?"PASS ":"FAIL ")+name);if(!value)throw new Exception(name);passed++;}
            Check(Resources.Load<Texture2D>("EV/RamirezHead")!=null,"Ramirez UV texture packaged as material asset");
            Mesh mesh=EVVisualShell.SphereMesh(48,32);float err=0;
            foreach(Vector3 v in mesh.vertices) err=Mathf.Max(err,Mathf.Abs(v.magnitude-.5f));
            Check(err<.000001f,"glove/head shell preserves exact sphere contact radius");
            UnityEngine.Object.DestroyImmediate(mesh);
            mesh=EVVisualShell.TorsoMesh();float maxOutside=0,maxInside=0;
            Vector3[] cs={new(0,1.16f,0),new(-.165f,1.37f,0),new(0,1.37f,0),new(.165f,1.37f,0),new(0,1.51f,0)};float[] rs={.25f,.18f,.18f,.18f,.1f};
            float Gap(Vector3 p){float d=100;for(int k=0;k<cs.Length;k++)d=Mathf.Min(d,Vector3.Distance(p+Vector3.up*1.37f,cs[k])-rs[k]);return d;}
            var vertices=mesh.vertices;var triangles=mesh.triangles;
            foreach(Vector3 p in vertices)maxOutside=Mathf.Max(maxOutside,Mathf.Abs(Gap(p)));
            for(int i=0;i<triangles.Length;i+=3) {float d=Gap((vertices[triangles[i]]+vertices[triangles[i+1]]+vertices[triangles[i+2]])/3);maxInside=Mathf.Max(maxInside,Mathf.Abs(d));}
            Debug.Log($"EV_SURFACE_ERRORS vertices={maxOutside:R} faces={maxInside:R}");
            Check(maxOutside<.001f,"torso vertices on original contact union within 1mm");
            Check(maxInside<.001f,"torso triangle centers within original M05 1mm tolerance");
            Check(!EVVisualShell.BottomControlsVisible,"bottom control visuals disabled independently of input");
            Check(Resources.Load<Shader>("EVSurface")!=null,"WebGL shell shader resource present");
            log.AppendLine($"vertex_error_m={maxOutside:R} triangle_error_m={maxInside:R} torso_vertices={vertices.Length} triangles={triangles.Length/3}");
            log.AppendLine("TOTAL="+passed+" FAIL=0");
            UnityEngine.Object.DestroyImmediate(mesh);
            string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../evidence/p1-ev"));Directory.CreateDirectory(path);File.WriteAllText(Path.Combine(path,"geometry-tests.txt"),log.ToString());
            Debug.Log(log);
        }
    }
}




