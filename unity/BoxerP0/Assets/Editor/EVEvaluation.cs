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
        public static void AuditFrame()
        {
            var shell=UnityEngine.Object.FindFirstObjectByType<EVVisualShell>();
            if(shell==null||!shell.Ready)return;
            var opponent=UnityEngine.Object.FindFirstObjectByType<OpponentBoxer>();
            var player=UnityEngine.Object.FindFirstObjectByType<PlayerBoxer>();
            if(shell.Torso!=null) _anchorError=Mathf.Max(_anchorError,Vector3.Distance(shell.Torso.position,opponent.transform.Find("R2 Chest").position));
            bool ok=true;
            foreach(var actor in new[]{player.transform,opponent.transform})foreach(string side in new[]{"Left","Right"}) {
                string prefix=actor==player.transform?"Player ":"Opponent ";
                Transform glove=actor.Find(prefix+side+" Glove"),elbow=actor.Find(side+" Elbow"),cuff=glove.Find("EV "+side+" Glove Cuff");
                ok &= glove.GetComponent<Renderer>().enabled && cuff!=null && cuff.parent==glove;
                if(cuff!=null)ok &= Vector3.Angle(cuff.up,glove.position-elbow.position)<.05f;
                int shoulders=0;foreach(Transform t in actor)if(t.name==side+" Shoulder"&&t.GetComponent<Renderer>().enabled)shoulders++;
                ok &= shoulders==1;
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
            File.WriteAllText(Path.Combine(path,"ownership-runtime.txt"),$"frames={_auditFrames} failed_frames={_auditFailures} torso_anchor_error_m={_anchorError:R}\nchecks=torso_anchor,cuff_parent,cuff_forearm_alignment,single_shoulder,trunk_thigh_attachment,boot_shin_attachment,redundant_torso_renderers_off\nRESULT={(pass?"PASS":"FAIL")}\n");
            return pass;
        }
        static EVEvaluation() { EVVisualShell.SegmentedExperiment=Environment.GetEnvironmentVariable("BOXER_EV_VARIANT")=="A"; }
        public static void RunVisual() { ShellTests(); EVVisualShell.SegmentedExperiment=Environment.GetEnvironmentVariable("BOXER_EV_VARIANT")=="A"; Round2RuntimeAudit.Run(); }
        public static void Tests()
        {
            Round2SelfTests.RunAll();
            ShellTests();
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
