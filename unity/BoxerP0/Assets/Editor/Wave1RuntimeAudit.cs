using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BoxerP0.Editor
{
    [InitializeOnLoad]
    public static class Wave1RuntimeAudit
    {
        private static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        private static readonly StringBuilder Log=new();
        private static int _stage, _frame=-1, _rematches, _checks;
        private static double _start;
        private static string Dir=>Path.GetFullPath(Path.Combine(Application.dataPath,"../../../evidence/wave1/"+(Environment.GetEnvironmentVariable("BOXER_WAVE_EVIDENCE")=="fighter-profile"?"fighter-profile":Environment.GetEnvironmentVariable("BOXER_WAVE_EVIDENCE")=="arena-surround"?"arena-surround":Environment.GetEnvironmentVariable("BOXER_WAVE_EVIDENCE")=="bell-repair"?"bell-repair":Environment.GetEnvironmentVariable("BOXER_WAVE_EVIDENCE")=="coach-analysis"?"coach-analysis":Environment.GetEnvironmentVariable("BOXER_WAVE_EVIDENCE")=="coach-ui"?"coach-ui":Environment.GetEnvironmentVariable("BOXER_WAVE_EVIDENCE")=="punch-feel"?"punch-feel":Environment.GetEnvironmentVariable("BOXER_WAVE_EVIDENCE")=="training-pov"?"training-pov":Environment.GetEnvironmentVariable("BOXER_WAVE_EVIDENCE")=="ring-intro"?"ring-intro":Environment.GetEnvironmentVariable("BOXER_WAVE_EVIDENCE")=="combat-v3"?"combat-v3":"onboarding-v2")));
        static Wave1RuntimeAudit() { if(SessionState.GetBool("wave1Audit",false))EditorApplication.update+=Tick; }
        public static void Run()
        {
            Directory.CreateDirectory(Dir);
            File.Copy(Path.Combine(Application.dataPath,"Scenes/Phase0Boxer.unity"),Path.Combine(Dir,"runtime-scene-before.bytes"),true);
            Phase0SceneBuilder.Build();SessionState.SetBool("wave1Audit",true);EditorApplication.update-=Tick;EditorApplication.update+=Tick;
            EditorApplication.isPlaying=true;
        }
        private static void Check(bool condition,string name)
        { _checks++;Log.AppendLine((condition?"PASS ":"FAIL ")+name); if(!condition)throw new Exception(name); }
        private static void Punch(PlayerBoxer player,PunchIntent intent)
        { typeof(PlayerBoxer).GetMethod("OnPunchRequested",Flags).Invoke(player,new object[]{intent}); }
        private static void BeginScored(BoxerBootstrap b, PlayerBoxer p, OpponentBoxer o, Phase0Telemetry t, BoxerInput input)
        {
            b.BeginProductBout();
            Check(b.Flow.Screen==ProductScreen.Intro&&!t.Bout.Active&&!input.GameplayInput&&!p.CombatEnabled&&!o.CombatEnabled,"ring intro locks input, AI, score");
            Punch(p,PunchIntent.Cross);t.Bout.Tick(6,ActionPhase.Guard,ActionPhase.Guard,1,1);
            Check(t.Bout.Seconds==0&&t.Bout.Player.HP==100&&t.Bout.Opponent.HP==100&&t.Bout.Player.Stamina==100&&t.Bout.Player.Capacity==100,"intro rejects punch and freezes vitals/clock");
            b.BrowserIntroReady("stale");Check(b.Flow.Screen==ProductScreen.Intro&&!t.Bout.Active,"stale media completion rejected");
            // Simulated browser callback for controller wiring ONLY; real media timing is tested in WebGL.
            b.BrowserIntroReady(b.IntroToken);
            Check(b.Flow.Screen==ProductScreen.Fight&&t.Bout.Active&&t.Bout.Seconds==0,"matching media completion starts fresh bout");
            b.BrowserIntroReady(b.IntroToken);Check(t.Bout.Seconds==0,"duplicate media completion does not reset bout");
        }
        private static void Tick()
        {
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling||_frame==Time.frameCount)return;
            _frame=Time.frameCount;
            if(_start==0)_start=EditorApplication.timeSinceStartup;
            try
            {
                if(EditorApplication.timeSinceStartup-_start>60)throw new Exception("runtime audit timeout");
                var b=UnityEngine.Object.FindFirstObjectByType<BoxerBootstrap>();
                var p=UnityEngine.Object.FindFirstObjectByType<PlayerBoxer>();
                var o=UnityEngine.Object.FindFirstObjectByType<OpponentBoxer>();
                var t=UnityEngine.Object.FindFirstObjectByType<Phase0Telemetry>();
                var input=UnityEngine.Object.FindFirstObjectByType<BoxerInput>();
                var f=UnityEngine.Object.FindFirstObjectByType<Blender3DVisualFollower>();
                if(b==null||f==null||!f.Ready)return;
                if(_stage==0)
                {
                    if(Environment.GetEnvironmentVariable("BOXER_WAVE_EVIDENCE")=="fighter-profile")FighterProfileRuntimeChecks.Run(b);
                    Log.AppendLine("SCOPE=SYNTHETIC_EDITOR_CONTROLLER_AND_INJECTED_CONSEQUENCE_TEST_NOT_PHYSICS_OR_HUMAN_UAT");
                    Check(b.Flow.Screen==ProductScreen.Home&&!input.GameplayInput&&!p.CombatEnabled&&!o.CombatEnabled,"home input and combat locked");
                    Punch(p,PunchIntent.Cross);Check(t.Bout.Player.Capacity==100&&!p.IsActionBusy,"menu request rejected without cost");
                    foreach(string side in new[]{"Left","Right"})
                    {
                        var prefab=Resources.Load<GameObject>("Boxer3D/PlayerPOVGlove_"+side+"_W1");
                        Check(prefab!=null,side+" glove imported");
                        var smrs=prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
                        Check(smrs.Length>0&&smrs.All(s=>s.bones.All(bone=>bone!=null)&&s.sharedMesh.bindposes.Length>0),side+" glove valid skinned bind");
                        Check(prefab.GetComponentsInChildren<Collider>().Length==0,side+" no imported combat collider");
                        Log.AppendLine(side+" glove triangles="+smrs.Sum(s=>s.sharedMesh.triangles.Length/3));
                    }
                    Check(f.AcceptedRig!=null,"accepted Ramirez still owns opponent presentation");
                    b.BeginProductBout(true);Check(b.Flow.Screen==ProductScreen.Home,"removed Home practice bypass rejected");
                    b.ShowCoach();b.SelectCoachModule(CoachModule.Head);b.OpenSelectedCoachModule();
                    Check(b.Flow.Screen==ProductScreen.Onboarding&&!t.Bout.Active&&!o.CombatEnabled&&input.GameplayInput,"standalone training has no AI or scored bout");
                    b.AdvanceTraining();Check(b.TrainingToken=="HEADCONTROL"&&!b.TrainingReady,"unperformed practice cannot advance by timer or button");
                    b.ExitTraining();Check(b.Flow.Screen==ProductScreen.Coach&&!input.GameplayInput&&!t.Bout.Active,"training exit returns Coach without starting fight");
                    b.ReturnHome();
                    b.ShowPreview();Check(b.Flow.Screen==ProductScreen.Preview&&!input.GameplayInput,"preview consumes menu input");
                    b.BeginProductBout();string cancelled=b.IntroToken;b.ReturnHome();b.BrowserIntroReady(cancelled);
                    Check(b.Flow.Screen==ProductScreen.Home&&!t.Bout.Active,"cancelled intro cannot start hidden combat");
                    b.ShowPreview();BeginScored(b,p,o,t,input);
                    Punch(p,PunchIntent.Cross); double capacity=t.Bout.Player.Capacity;
                    Check(capacity==84&&Math.Abs(t.Bout.Player.Stamina-96.8)<1e-7,"real player accepted action spends once");
                    Punch(p,PunchIntent.Jab);Check(t.Bout.Player.Capacity==capacity,"real busy rejection costs zero");
                    p.CompleteRound2Punch(CombatOutcome.Hit,"OPPONENT_HEAD_SWEPT_CONTACT",Vector3.zero,Vector3.one);
                    Check(t.Bout.Opponent.HP==91,"controller resolution applies exact committed damage");
                    p.CompleteRound2Punch(CombatOutcome.Hit,"OPPONENT_HEAD_SWEPT_CONTACT",Vector3.zero,Vector3.one);
                    Check(t.Bout.Opponent.HP==91&&t.PlayerHits==1,"controller resolution cannot double count");
                    p.SetCombatEnabled(false);p.SetCombatEnabled(true);
                    p.CompleteRound2Punch(CombatOutcome.Hit,"OPPONENT_HEAD_SWEPT_CONTACT",Vector3.zero,Vector3.one);
                    Check(t.PlayerHits==1&&t.Bout.Opponent.HP==91,"guard-only stale callback cannot change score or HP");
                    for(int i=0;i<12;i++) {var r=t.Bout.Accept(true,PunchIntent.RearOverhand);t.Bout.Resolve(true,r,CombatOutcome.Miss,false);}
                    double quality=t.Bout.Player.Quality;double defenderHP=t.Bout.Opponent.HP;
                    Punch(p,PunchIntent.Cross);
                    // The approved per-intent base changed, NOT the fatigue multiplier.
                    double crossRecovery=PunchMotionProfile.Player(PunchIntent.Cross).Recover;
                    Check(Math.Abs(p.ActiveRecoverSeconds-crossRecovery*(1+.75*(1-quality)))<1e-6&&p.ActiveRecoverSeconds>crossRecovery,"real player fatigue slows recovery");
                    p.CompleteRound2Punch(CombatOutcome.Hit,"OPPONENT_HEAD_SWEPT_CONTACT",Vector3.zero,Vector3.one);
                    Check(Math.Abs(t.Bout.Opponent.HP-(defenderHP-9*quality))<1e-7,"real player fatigue reduces impact");
                    for(int i=0;i<12;i++) {var r=t.Bout.Accept(false,PunchIntent.RearOverhand);t.Bout.Resolve(false,r,CombatOutcome.Miss,false);}
                    o.SetCombatEnabled(false);o.SetCombatEnabled(true);double oq=t.Bout.Opponent.Quality;
                    typeof(OpponentBoxer).GetMethod("StartAttack",Flags).Invoke(o,null);
                    float recover=(float)typeof(OpponentBoxer).GetField("_activeRecoverSeconds",Flags).GetValue(o);
                    Check(Math.Abs(recover-P1OpponentAttributes.Resolve(P1OpponentProfile.Balanced).RecoverSeconds*(1+.75*(1-oq)))<1e-6,"real opponent fatigue slows recovery");
                    var hud=UnityEngine.Object.FindFirstObjectByType<P1VCombatPresentation>();
                    typeof(P1VCombatPresentation).GetMethod("UpdateStamina",Flags).Invoke(hud,null);
                    float shown=(float)typeof(P1VCombatPresentation).GetField("_playerStamina",Flags).GetValue(hud);
                    Check(Math.Abs(shown-t.Bout.Player.Stamina/100)<1e-7,"HUD stamina reads combat state exactly");
                    b.ReturnHome();b.ShowPreview();BeginScored(b,p,o,t,input);
                    Check(t.Bout.Player.HP==100&&t.Bout.Opponent.HP==100&&t.PlayerHits==0&&o.AttackEventCount==0,"next bout resets vitals/counters/AI count");
                    _stage=1;
                }
                else if(_stage==1)
                {
                    Capture("new-gloves-editor-guard.png");
                    foreach(string side in new[]{"Left","Right"})
                    {
                        var root=GameObject.Find("Blender Player "+side+" POV");
                        var thumb=root.GetComponentsInChildren<Renderer>().First(r=>r.name=="POVGloveThumb");
                        var glove=side=="Left"?p.LeftGlove:p.RightGlove;
                        Log.AppendLine(side+" thumb_relative_world_x="+(thumb.bounds.center.x-glove.position.x));
                    }
                    Log.AppendLine("native_ramirez_max_glove_error_m="+f.AcceptedRig.MaxGloveError);
                    for(int i=0;i<100&&t.Bout.Active;i++)
                    {
                        p.SetCombatEnabled(false);p.SetCombatEnabled(true);Punch(p,PunchIntent.RearOverhand);
                        p.CompleteRound2Punch(CombatOutcome.Hit,"OPPONENT_HEAD_SWEPT_CONTACT",Vector3.zero,Vector3.one);
                    }
                    Check(t.Bout.Ended&&t.Bout.EndReason=="KO"&&t.Bout.Opponent.HP==0,"injected controller hits reach HP-zero KO");
                    Check(!p.CombatEnabled&&!o.CombatEnabled,"both controllers immediately locked at KO");
                    double cap=t.Bout.Player.Capacity;Punch(p,PunchIntent.Jab);
                    Check(cap==t.Bout.Player.Capacity,"post-KO input cannot spend");
                    _stage=2;
                }
                else if(_stage==2)
                {
                    if(b.Flow.Screen!=ProductScreen.Result)return;
                    Check(t.BoutResult==(_rematches==0?"PLAYER_WIN":"DRAW")&&!input.GameplayInput,"result transition and input lock "+_rematches);
                    var previous=b.LastMatchReview;
                    Check(previous!=null&&previous.Valid&&previous.playerHP==t.Bout.Player.HP&&previous.opponentHP==t.Bout.Opponent.HP&&previous.hits==t.PlayerHits,"completed result snapshot equals authoritative counters and HP "+_rematches);
                    BeginScored(b,p,o,t,input);_rematches++;
                    Check(ReferenceEquals(previous,b.LastMatchReview),"running rematch does not replace last completed record "+_rematches);
                    Check(b.Flow.Screen==ProductScreen.Fight&&t.Bout.Player.HP==100&&t.Bout.Opponent.HP==100&&t.Bout.Player.Stamina==100&&t.Bout.Player.Capacity==100,"rematch full reset "+_rematches);
                    Check(UnityEngine.Object.FindObjectsByType<PlayerBoxer>(FindObjectsSortMode.None).Length==1&&UnityEngine.Object.FindObjectsByType<OpponentBoxer>(FindObjectsSortMode.None).Length==1,"no duplicate actors "+_rematches);
                    if(_rematches<10)t.Bout.FinishTimeout();else {b.ReturnHome();Check(!input.GameplayInput&&!t.Bout.Active,"return home freezes model");Finish(0);}
                }
            }
            catch(Exception e){Log.AppendLine("FAIL EXCEPTION "+e);Finish(1);}
        }
        private static void Capture(string filename)
        {
            var camera=Camera.main??UnityEngine.Object.FindAnyObjectByType<Camera>();var rt=new RenderTexture(540,960,24);var old=camera.targetTexture;var active=RenderTexture.active;
            float aspect=camera.aspect;camera.aspect=9f/16f;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            var tex=new Texture2D(540,960,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,540,960),0,0);tex.Apply();
            File.WriteAllBytes(Path.Combine(Dir,filename),tex.EncodeToPNG());camera.targetTexture=old;camera.aspect=aspect;RenderTexture.active=active;
            UnityEngine.Object.DestroyImmediate(tex);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
        }
        private static void Finish(int exit)
        {
            Log.AppendLine("CHECKS="+_checks+" EXIT="+exit);File.WriteAllText(Path.Combine(Dir,"controller-runtime.txt"),Log.ToString());Debug.Log(Log);
            SessionState.SetBool("wave1Audit",false);EditorApplication.update-=Tick;EditorApplication.isPlaying=false;
            File.Copy(Path.Combine(Dir,"runtime-scene-before.bytes"),Path.Combine(Application.dataPath,"Scenes/Phase0Boxer.unity"),true);
            EditorApplication.Exit(exit);
        }
    }
}
