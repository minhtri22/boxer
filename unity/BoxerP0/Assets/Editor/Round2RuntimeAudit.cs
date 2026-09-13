using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BoxerP0.Editor
{
    [InitializeOnLoad]
    public static class Round2RuntimeAudit
    {
        static double _started,_nextPunch;
        static int _shot,_punch;
        static float _maxRigMs,_sumRigMs,_maxPlant;
        static int _frames;
        static int _lastFrame=-1;
        static long _alloc;
        static readonly StringBuilder Log=new StringBuilder();
        static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        static Round2RuntimeAudit()
        {
            if(SessionState.GetBool("r2Audit",false)) EditorApplication.update+=Tick;
        }
        public static void Run()
        {
            Phase0SceneBuilder.Build();
            SessionState.SetBool("r2Audit",true);
            EditorApplication.update-=Tick; EditorApplication.update+=Tick;
            EditorApplication.isPlaying=true;
        }
        static void Tick()
        {
            if(!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            if(_lastFrame==Time.frameCount) return;
            _lastFrame=Time.frameCount;
            var rig=UnityEngine.Object.FindFirstObjectByType<Round2CombatRig>();
            var player=UnityEngine.Object.FindFirstObjectByType<PlayerBoxer>();
            var opponent=UnityEngine.Object.FindFirstObjectByType<OpponentBoxer>();
            if(rig==null || player==null || opponent==null) return;
            if(_started==0)
            {
                _started=EditorApplication.timeSinceStartup;
                var bootstrap=UnityEngine.Object.FindFirstObjectByType<BoxerBootstrap>();
                typeof(BoxerBootstrap).GetMethod("StartBout",Flags).Invoke(bootstrap,null);
                Log.AppendLine("EDITOR_PLAYMODE_SYNTHETIC_NOT_HUMAN_UAT unity="+Application.unityVersion);
                Log.AppendLine("scenario=alternating player punches, target shifts for advance/retreat/lateral; current AI attack state machine; real Update/LateUpdate");
            }
            double elapsed=EditorApplication.timeSinceStartup-_started;
            if(elapsed>_nextPunch && elapsed<18)
            {
                PunchIntent[] intents={PunchIntent.Jab,PunchIntent.Cross,PunchIntent.LeadHook,PunchIntent.RearHook,PunchIntent.LeadUppercut,PunchIntent.RearUppercut,PunchIntent.LeadOverhand,PunchIntent.RearOverhand};
                typeof(PlayerBoxer).GetMethod("OnPunchRequested",Flags).Invoke(player,new object[]{intents[_punch++%intents.Length]});
                _nextPunch=elapsed+0.72;
            }
            // Deterministic external player footwork input fixture, not AI teleportation.
            if(elapsed>6 && elapsed<8) player.transform.position+=Vector3.back*(0.28f*Time.deltaTime);
            if(elapsed>11 && elapsed<13) player.transform.position+=Vector3.forward*(0.35f*Time.deltaTime);
            if(elapsed>15 && elapsed<17) player.transform.position+=Vector3.right*(0.18f*Time.deltaTime);
            _maxRigMs=Mathf.Max(_maxRigMs,rig.LastUpdateMs); _sumRigMs+=rig.LastUpdateMs; _frames++;
            var feet=UnityEngine.Object.FindFirstObjectByType<Round2Footwork>();
            _maxPlant=Mathf.Max(_maxPlant,feet.PlantedDrift);
            if(elapsed>1+_shot*2 && _shot<10)
            {
                Capture(_shot++,player,opponent);
                Log.AppendLine($"t={elapsed:F2} distance={Vector3.Distance(player.transform.position,opponent.transform.position):F4} player={player.ActionLabel} opponent={opponent.ActionLabel} contacts={rig.Contacts} gap={rig.LastContactGap:R} steps={feet.Steps}");
            }
            if(elapsed<22) return;
            Log.AppendLine($"frames={_frames} rig_mean_ms={_sumRigMs/_frames:F5} rig_max_ms={_maxRigMs:F5} max_planted_foot_error_m={_maxPlant:R} contacts={rig.Contacts} samples={rig.Samples}");
            Log.AppendLine("CPU values include Editor overhead. Not WebGL frame-time evidence.");
            Directory.CreateDirectory(DirectoryPath);
            File.WriteAllText(Path.Combine(DirectoryPath,"runtime.txt"),Log.ToString());
            SessionState.SetBool("r2Audit",false);
            EditorApplication.update-=Tick;
            EditorApplication.isPlaying=false;
            EditorApplication.Exit(0);
        }
        static string DirectoryPath => Path.GetFullPath(Path.Combine(Application.dataPath,"../../../evidence/uat-round2/optimization"));
        static void Capture(int index,PlayerBoxer player,OpponentBoxer opponent)
        {
            Camera camera=UnityEngine.Object.FindFirstObjectByType<Camera>();
            camera.aspect=9f/16f;
            var rt=new RenderTexture(900,1600,24);
            var old=camera.targetTexture;
            camera.targetTexture=rt; camera.Render();
            var previous=RenderTexture.active; RenderTexture.active=rt;
            var texture=new Texture2D(900,1600,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,900,1600),0,0); texture.Apply();
            Directory.CreateDirectory(DirectoryPath);
            File.WriteAllBytes(Path.Combine(DirectoryPath,$"motion-{index:00}.png"),texture.EncodeToPNG());
            camera.targetTexture=old; RenderTexture.active=previous;
            UnityEngine.Object.DestroyImmediate(texture); rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
