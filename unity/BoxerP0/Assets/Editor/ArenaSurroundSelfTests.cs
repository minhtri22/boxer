using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class ArenaSurroundSelfTests
    {
        static string Dir=>Path.GetFullPath(Path.Combine(Application.dataPath,"../../../evidence/wave1/"+(Environment.GetEnvironmentVariable("BOXER_WAVE_EVIDENCE")=="fighter-profile"?"fighter-profile":"arena-surround")));
        public static void RunAll(){Run();CoachAnalysisSelfTests.RunAll();}
        public static void RunRendered(){RunAll();Render();}
        public static void Run()
        {
            int pass=0,fail=0;var log=new StringBuilder("SCOPE=ARENA_PRESENTATION_NOT_PHONE_VISUAL_UAT\n");
            void Check(bool ok,string label){if(ok)pass++;else fail++;log.AppendLine((ok?"PASS ":"FAIL ")+label);}
            Directory.CreateDirectory(Dir);
            var audience=Resources.Load<Texture2D>("P1V/championship-arena");
            Check(audience!=null&&Resources.Load<Shader>("AudienceFlash")!=null&&Resources.Load<Shader>("EVBackdrop")!=null,"approved crowd and both shaders available");
            var root=new GameObject("Isolated arena diagnostic");
            try
            {
                var arena=root.AddComponent<ArenaSurround>();arena.Initialize(audience);arena.Initialize(audience);
                var meshes=root.GetComponentsInChildren<MeshFilter>();var renderers=root.GetComponentsInChildren<MeshRenderer>();
                Check(arena.StandCount==4&&arena.FlashCount==24&&meshes.Length==28,"idempotent four stands and bounded twenty-four flash nodes");
                Check(meshes.Select(x=>x.sharedMesh).Distinct().Count()==2&&renderers.Select(x=>x.sharedMaterial).Distinct().Count()==2,"two shared meshes and materials only");
                Check(root.GetComponentsInChildren<Collider>().Length==0&&root.GetComponentsInChildren<Rigidbody>().Length==0&&root.GetComponentsInChildren<Light>().Length==0,"no physics or scene lights");
                Check(arena.EnabledFlashes==0&&arena.Strength==0&&arena.ActiveFlash==-1,"initial flashes disabled");
                Check(Vector3.Distance(ArenaSurround.SidePosition(0),new Vector3(0,3,4.24f))<.00001f,"front scenery baseline preserved");
                for(int side=0;side<4;side++)
                {
                    var stand=root.transform.Find("Audience Side "+side);
                    var inward=(ArenaSurround.Center-stand.position);inward.y=0;
                    Check(Vector3.Dot(-stand.forward,inward.normalized)>.999f,"inward-facing scenery "+side);
                    Check(Mathf.Abs(stand.position.x)>3.9f||Mathf.Abs(stand.position.z)>3.8f,"outside combat extents "+side);
                }
                foreach(float time in new[]{-1f,0f,.49f,float.NaN,float.PositiveInfinity,float.NegativeInfinity})
                    Check(ArenaSurround.Sample(time,true).Index==-1,"invalid or pre-delay no flash "+time);
                Check(ArenaSurround.Sample(5,false).Index==-1,"inactive sample suppressed");
                Check(ArenaSurround.Sample(.5f+ArenaSurround.Duration+.01f,true).Index==-1,"pulse end suppressed");
                int mask=0;
                for(int cycle=0;cycle<24;cycle++)
                {
                    var sample=ArenaSurround.Sample(.5f+cycle*1.5f+.08f,true);arena.Apply(sample);mask|=1<<(sample.Index%4);
                    Check(sample.Index==cycle&&sample.Strength>.749f&&sample.Strength<=.75f,"deterministic bounded peak "+cycle);
                    Check(arena.EnabledFlashes==1&&arena.ActiveFlash==cycle,"exactly one actual enabled renderer "+cycle);
                    var pos=ArenaSurround.FlashPosition(cycle);
                    Check(Mathf.Abs(pos.x)>3.8f||Mathf.Abs(pos.z)>3.7f,"flash outside legal area "+cycle);
                }
                Check(mask==15&&arena.ObservedSideMask==15,"all four sides observed");
                arena.Apply(new ArenaSurround.FlashSample(100,.75f));
                Check(arena.EnabledFlashes==0&&arena.ActiveFlash==-1&&arena.Strength==0,"invalid index cannot leave enabled flash");
                arena.Apply(new ArenaSurround.FlashSample(0,50));Check(arena.Strength==.75f&&arena.EnabledFlashes==1,"intensity upper bound enforced");
                arena.enabled=false;
                // Edit-mode unit tests explicitly invoke the callback; compiled UI verifies actual flow resets.
                arena.SendMessage("OnDisable");
                Check(arena.EnabledFlashes==0&&arena.ObservedSideMask==0,"disable callback clears effect and observation");
                Check(root.transform.position==Vector3.zero&&root.transform.rotation==Quaternion.identity,"owner transform unchanged");
                int lit=0;
                for(int i=0;i<3600;i++){var sample=ArenaSurround.Sample(i/100f,true);if(sample.Strength>0)lit++;Check(sample.Index<24&&sample.Strength>=0&&sample.Strength<=.75f,"schedule bound "+i);}
                Check(lit<430&&lit>300,"sparse duty cycle near eleven percent");
            }
            finally{UnityEngine.Object.DestroyImmediate(root);}
            log.AppendLine($"TOTAL={pass+fail} PASS={pass} FAIL={fail}");
            File.WriteAllText(Path.Combine(Dir,"arena-tests.txt"),log.ToString());Debug.Log(log.ToString());
            if(fail>0)throw new Exception("Arena surround invariant failure");
        }
        // Diagnostic-only cameras. Never steer the gameplay camera or fabricate phone orientation evidence.
        public static void Render()
        {
            Directory.CreateDirectory(Dir);
            var root=new GameObject("Isolated crowd render diagnostic");
            var cameraNode=new GameObject("Diagnostic yaw camera");
            var target=new RenderTexture(540,960,24);
            var pixels=new Texture2D(540,960,TextureFormat.RGB24,false);
            try
            {
                var arena=root.AddComponent<ArenaSurround>();arena.Initialize(Resources.Load<Texture2D>("P1V/championship-arena"));
                var camera=cameraNode.AddComponent<Camera>();camera.targetTexture=target;camera.fieldOfView=108;camera.aspect=540f/960;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
                // Only the isolated scenery is visible in these diagnostics.
                foreach(var t in root.GetComponentsInChildren<Transform>())t.gameObject.layer=30;
                camera.cullingMask=1<<30;camera.transform.position=new Vector3(0,1.64f,.01f);
                foreach(int yaw in new[]{0,45,90,180,270})
                {
                    camera.transform.rotation=Quaternion.Euler(0,yaw,0);
                    arena.Apply(new ArenaSurround.FlashSample(-1,0));Capture("crowd-yaw-"+yaw+".png");
                }
                camera.transform.rotation=Quaternion.identity;
                arena.Apply(new ArenaSurround.FlashSample(3,.75f));
                // Look at that side's actual world-space flash; this is explicitly diagnostic framing.
                camera.transform.LookAt(ArenaSurround.FlashPosition(3));camera.fieldOfView=55;
                Capture("flash-on.png");arena.Apply(new ArenaSurround.FlashSample(-1,0));Capture("flash-off.png");
                void Capture(string name)
                {
                    camera.Render();var previous=RenderTexture.active;
                    try{RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,540,960),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(Dir,name),pixels.EncodeToPNG());}
                    finally{RenderTexture.active=previous;}
                }
                File.WriteAllText(Path.Combine(Dir,"render-scope.txt"),"DIAGNOSTIC_ISOLATED_SCENERY_AND_SYNTHETIC_VISUAL_PULSE_NOT_PHONE_UAT\nYAW=0,45,90,180,270\nGAMEPLAY_CAMERA_UNCHANGED\n");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(cameraNode);UnityEngine.Object.DestroyImmediate(root);
                target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);
            }
        }
    }
}
