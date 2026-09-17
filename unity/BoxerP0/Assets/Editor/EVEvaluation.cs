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
        static bool _blenderObserved;
        public static void AuditFrame()
        {
            var shell=UnityEngine.Object.FindFirstObjectByType<EVVisualShell>();
            if(shell==null||!shell.Ready)return;
            var opponent=UnityEngine.Object.FindFirstObjectByType<OpponentBoxer>();
            var player=UnityEngine.Object.FindFirstObjectByType<PlayerBoxer>();
            if(shell.BlenderAssetActive)
            {
                _blenderObserved=true;
                var follower=shell.BlenderFollower;
                bool blenderOk=follower!=null&&follower.Ready;
                blenderOk &= GameObject.Find("Blender Ramirez UAT3")!=null;
                blenderOk &= GameObject.Find("Blender Player Left POV")!=null&&GameObject.Find("Blender Player Right POV")!=null;
                blenderOk &= follower!=null&&follower.VisibleRendererCount>=20;
                _anchorError=Mathf.Max(_anchorError,follower==null?999f:follower.MaxAnchorError);
                blenderOk &= _anchorError<.00001f;
                foreach(Renderer r in opponent.GetComponentsInChildren<Renderer>(true)) blenderOk &= !r.enabled;
                foreach(Renderer r in player.GetComponentsInChildren<Renderer>(true)) blenderOk &= !r.enabled;
                if(!blenderOk)_auditFailures++;
                _auditFrames++;
                return;
            }
            if(shell.Torso!=null) _anchorError=Mathf.Max(_anchorError,Vector3.Distance(shell.Torso.position,opponent.transform.Find("R2 Chest").position));
            bool ok=true;
            ok &= GameObject.Find("EV Ramirez Full Body")==null;
            ok &= GameObject.Find("EV Player Left Glove")==null&&GameObject.Find("EV Player Right Glove")==null;
            ok &= GameObject.Find("EV Rig Head")==null&&GameObject.Find("EV Rig Torso")==null;
            foreach(var actor in new[]{player.transform,opponent.transform})foreach(string side in new[]{"Left","Right"}) {
                bool isPlayer=actor==player.transform;
                string prefix=isPlayer?"Player ":"Opponent ";
                Transform glove=actor.Find(prefix+side+" Glove"),elbow=actor.Find(side+" Elbow"),cuff=glove.Find("EV "+side+" Glove Cuff");
                Transform gloveSurface=glove.Find("EV "+prefix+side+" Glove Surface");
                Renderer anchorRenderer=glove.GetComponent<Renderer>(),surfaceRenderer=gloveSurface!=null?gloveSurface.GetComponent<Renderer>():null;
                ok &= anchorRenderer!=null&&!anchorRenderer.enabled;
                ok &= gloveSurface!=null&&gloveSurface.parent==glove&&surfaceRenderer!=null&&surfaceRenderer.enabled;
                ok &= cuff!=null&&cuff.parent==glove;
                if(cuff!=null) {
                    ok &= cuff.GetComponent<Renderer>()!=null&&cuff.GetComponent<Renderer>().enabled;
                    ok &= Vector3.Angle(cuff.up,glove.position-elbow.position)<.05f;
                }
                else ok=false;
                int shoulders=0;foreach(Transform t in actor)if(t.name==side+" Shoulder"&&t.GetComponent<Renderer>().enabled)shoulders++;
                ok &= shoulders==1;
            }
            foreach(string side in new[]{"Left","Right"}) {
                Transform thigh=opponent.transform.Find(side+" Thigh"),shin=opponent.transform.Find(side+" Shin"),shoe=opponent.transform.Find(side+" Shoe");
                ok &= thigh.GetComponent<Renderer>().enabled&&shin.GetComponent<Renderer>().enabled&&shoe.GetComponent<Renderer>().enabled;
                ok &= thigh.Find("EV "+side+" Trunk Leg")!=null&&shin.Find("EV "+side+" Boot Upper")!=null;
            }
            ok &= _anchorError<.00001f;
            if(!EVVisualShell.SegmentedExperiment)foreach(string n in new[]{"R2 Abdomen","R2 Chest","R2 Left Chest","R2 Right Chest","R2 Neck"})ok &= !opponent.transform.Find(n).GetComponent<Renderer>().enabled;
            if(!ok)_auditFailures++;_auditFrames++;
        }
        public static bool AuditFinal()
        {
            bool pass=_auditFrames>0&&_auditFailures==0;
            string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../evidence/p1-ev"));Directory.CreateDirectory(path);
            string checks=_blenderObserved
                ? "blender_asset_owner,legacy_renderers_hidden,round2_authority_preserved,blender_hand_to_glove_anchor,player_pov_asset_owner"
                : "single_3d_visual_owner,torso_anchor,glove_surface_parent,cuff_parent,cuff_forearm_alignment,single_shoulder,visible_articulated_thigh_shin_shoe,trunk_thigh_attachment,boot_shin_attachment,redundant_torso_renderers_off";
            File.WriteAllText(Path.Combine(path,"ownership-runtime.txt"),$"frames={_auditFrames} failed_frames={_auditFailures} visual_anchor_error_m={_anchorError:R}\nvisual_path={(_blenderObserved?"blender_fbx_follow_round2":"ev_procedural_fallback")}\nchecks={checks}\nRESULT={(pass?"PASS":"FAIL")}\n");
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
            foreach(string segment in new[]{"ramirez-head","ramirez-torso","ramirez-shorts","ramirez-arm","ramirez-glove","ramirez-thigh","ramirez-shin","ramirez-boot","player-glove"})
                Check(Resources.Load<Texture2D>("EV/ReferenceSegments/"+segment)!=null,"3D surface reference texture packaged: "+segment);
            GameObject blenderOpponent=Resources.Load<GameObject>("Boxer3D/Ramirez_UAT3");
            GameObject blenderPlayer=Resources.Load<GameObject>("Boxer3D/PlayerPOVGlove_UAT3");
            Check(blenderOpponent!=null,"Blender Ramirez FBX packaged as Unity resource");
            Check(blenderPlayer!=null,"Blender POV glove FBX packaged as Unity resource");
            int opponentTriangles=blenderOpponent==null?int.MaxValue:Triangles(blenderOpponent);
            int playerTriangles=blenderPlayer==null?int.MaxValue:Triangles(blenderPlayer);
            Check(opponentTriangles<=30000,"Blender Ramirez WebGL triangle budget <= 30k");
            Check(playerTriangles<=5000,"Blender POV glove WebGL triangle budget <= 5k");
            Check(blenderOpponent!=null&&HasBones(blenderOpponent,new[]{"root","pelvis","spine","chest","neck","head","upper_arm.L","forearm.L","hand.L","upper_arm.R","forearm.R","hand.R","thigh.L","shin.L","foot.L","thigh.R","shin.R","foot.R"}),"Blender Ramirez required rig bones present");
            string repoRoot=Path.GetFullPath(Path.Combine(Application.dataPath,"../../.."));
            Check(File.Exists(Path.Combine(repoRoot,"art","blender","source","boxer_uat3_blender_assets.blend")),"Blender source file committed with asset pipeline");
            log.AppendLine($"blender_opponent_triangles={opponentTriangles} blender_player_triangles={playerTriangles}");
            log.AppendLine($"vertex_error_m={maxOutside:R} triangle_error_m={maxInside:R} torso_vertices={vertices.Length} triangles={triangles.Length/3}");
            log.AppendLine("TOTAL="+passed+" FAIL=0");
            UnityEngine.Object.DestroyImmediate(mesh);
            string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../evidence/p1-ev"));Directory.CreateDirectory(path);File.WriteAllText(Path.Combine(path,"geometry-tests.txt"),log.ToString());
            Debug.Log(log);
        }

        static int Triangles(GameObject prefab)
        {
            int count=0;
            foreach(MeshFilter f in prefab.GetComponentsInChildren<MeshFilter>(true))if(f.sharedMesh!=null)count+=f.sharedMesh.triangles.Length/3;
            foreach(SkinnedMeshRenderer r in prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true))if(r.sharedMesh!=null)count+=r.sharedMesh.triangles.Length/3;
            return count;
        }

        static bool HasBones(GameObject prefab,string[] names)
        {
            foreach(string name in names)
            {
                bool found=false;
                foreach(Transform t in prefab.GetComponentsInChildren<Transform>(true))if(t.name==name){found=true;break;}
                if(!found)return false;
            }
            return true;
        }
    }
}
