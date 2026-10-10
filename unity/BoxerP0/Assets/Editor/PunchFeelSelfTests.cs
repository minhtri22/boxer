using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Globalization;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class PunchFeelSelfTests
    {
        static readonly StringBuilder Log=new(),Trace=new(),Summary=new();
        static int pass,fail;
        static string Dir=>Path.GetFullPath(Path.Combine(Application.dataPath,"../../../evidence/wave1/"+(Environment.GetEnvironmentVariable("BOXER_WAVE_EVIDENCE")=="fighter-profile"?"fighter-profile":Environment.GetEnvironmentVariable("BOXER_WAVE_EVIDENCE")=="arena-surround"?"arena-surround":Environment.GetEnvironmentVariable("BOXER_WAVE_EVIDENCE")=="bell-repair"?"bell-repair":Environment.GetEnvironmentVariable("BOXER_WAVE_EVIDENCE")=="coach-analysis"?"coach-analysis":Environment.GetEnvironmentVariable("BOXER_WAVE_EVIDENCE")=="coach-ui"?"coach-ui":"punch-feel")));
        static void Check(bool ok,string text) { if(ok)pass++;else fail++;Log.AppendLine((ok?"PASS ":"FAIL ")+text); }
        static bool Near(double a,double b,double tolerance=.001)=>Math.Abs(a-b)<tolerance;
        static readonly PunchIntent[] Intents={PunchIntent.Jab,PunchIntent.Cross,PunchIntent.LeadHook,PunchIntent.RearHook,
            PunchIntent.LeadUppercut,PunchIntent.RearUppercut,PunchIntent.LeadOverhand,PunchIntent.RearOverhand};
        public static void RunAll() { Run();TrainingPovSelfTests.Run(); }
        public static void Run()
        {
            pass=fail=0;Log.Clear();Trace.Clear();Summary.Clear();
            Log.AppendLine("SCOPE=DETERMINISTIC_SHARED_POSE_CONTACT_TIMELINE_AUDIO_NOT_DEVICE_FEEL_UAT");
            Trace.AppendLine("mode,intent,phase,seconds,x,y,z,speed_mps");
            Summary.AppendLine("mode,intent,commit_ms,extend_ms,recovery_ms,peak_extend_speed_mps");
            var before=PunchMotionProfile.Mode;
            Directory.CreateDirectory(Dir);
            try
            {
                foreach(var intent in Intents)
                {
                    float legacy=0,fast=0;
                    foreach(PunchFeelMode mode in Enum.GetValues(typeof(PunchFeelMode)))
                    {
                        PunchMotionProfile.Mode=mode;var profile=PunchMotionProfile.Player(intent);
                        float peak=0;
                        foreach(var phase in new[]{ActionPhase.Commit,ActionPhase.Extend,ActionPhase.Recover})
                        {
                            float duration=phase==ActionPhase.Commit?profile.Commit:phase==ActionPhase.Extend?profile.Extend:profile.Recover;
                            Vector3 prior=Round2Motion.Sample(!PunchLabels.IsRearHand(intent),intent,phase,0,Round2Motion.Endpoint(intent,"NEUTRAL",.735f),PunchMotionProfile.Ballistic).Wrist;
                            for(int i=1;i<=200;i++)
                            {
                                float t=i/200f;var pose=Round2Motion.Sample(!PunchLabels.IsRearHand(intent),intent,phase,t,
                                    Round2Motion.Endpoint(intent,"NEUTRAL",.735f),PunchMotionProfile.Ballistic);
                                float velocity=Vector3.Distance(prior,pose.Wrist)/(duration/200);
                                if(phase==ActionPhase.Extend)peak=Mathf.Max(peak,velocity);
                                Trace.AppendLine(string.Format(CultureInfo.InvariantCulture,"{0},{1},{2},{3:F6},{4:F6},{5:F6},{6:F6},{7:F6}",
                                    mode,intent,phase,t*duration,pose.Wrist.x,pose.Wrist.y,pose.Wrist.z,velocity));
                                Check(float.IsFinite(velocity)&&Vector3.Distance(pose.Shoulder,pose.Wrist)<.66f,"finite anatomical pose "+mode+" "+intent+" "+phase+" "+i);
                                prior=pose.Wrist;
                            }
                        }
                        Summary.AppendLine(string.Format(CultureInfo.InvariantCulture,"{0},{1},{2:F1},{3:F1},{4:F1},{5:F6}",mode,intent,
                            profile.Commit*1000,profile.Extend*1000,profile.Recover*1000,peak));
                        if(mode==PunchFeelMode.Legacy)legacy=peak;if(mode==PunchFeelMode.Fast)fast=peak;
                        foreach(float frame in new[]{1/30f,1/60f,1/120f,.2f})
                        {
                            var state=new TimedActionState();state.TryStart(intent);
                            float duration=profile.Commit+profile.Extend+profile.Recover,elapsed=0;
                            while(elapsed<duration-.000001f){float dt=Mathf.Min(frame,duration-elapsed);state.Step(dt,profile.Commit,profile.Extend,profile.Recover);elapsed+=dt;}
                            state.Step(.000001f,profile.Commit,profile.Extend,profile.Recover);
                            Check(state.Phase==ActionPhase.Guard,"carry-over full cycle "+mode+" "+intent+" dt="+frame);
                            long id=state.ActionId;state.TryStart(intent);Check(state.ActionId==id+1,"distinct action ID "+mode+" "+intent+" "+frame);
                        }
                    }
                    Log.AppendLine(string.Format(CultureInfo.InvariantCulture,"MEASURE {0} legacy_peak={1:F4} fast_peak={2:F4} ratio={3:F4}",intent,legacy,fast,fast/legacy));
                    if(intent==PunchIntent.Jab||intent==PunchIntent.Cross)Check(fast>legacy*1.45f,"jab/cross ballistic speed improves without endpoint inflation "+intent);
                }
                PunchMotionProfile.Mode=PunchFeelMode.Fast;
                var overshoot=new TimedActionState();overshoot.TryStart(PunchIntent.Jab);overshoot.Step(.12f,.04f,.07f,.19f);
                Check(overshoot.Phase==ActionPhase.Recover&&Near(overshoot.PhaseTime,.01f,.000001),"overshoot carried into Recover rather than reset");
                foreach(var gesture in new[]{new Vector2(120,0),new Vector2(-120,0),new Vector2(0,120),new Vector2(0,-120)})
                {
                    Check(PunchGestureClassifier.Resolve(new GestureMetrics(gesture,120,.06f),PunchIntent.None)!=PunchIntent.None,"60ms decisive swipe accepted "+gesture);
                    Check(PunchGestureClassifier.Resolve(new GestureMetrics(gesture,120,.03f),PunchIntent.None)==PunchIntent.None,"30ms accidental swipe rejected "+gesture);
                }
                for(int kind=0;kind<4;kind++)
                {
                    var data=BoxerFeedback.Samples(kind);
                    Check(data.All(v=>float.IsFinite(v)&&Math.Abs(v)<=.95f)&&data.Any(v=>Math.Abs(v)>.03f),"finite audible bounded generated SFX "+kind);
                    Check(data.SequenceEqual(BoxerFeedback.Samples(kind)),"SFX deterministic independent of AI random "+kind);
                    WriteWav(Path.Combine(Dir,"sfx-"+kind+".wav"),data);
                }
                Check(!BoxerFeedback.Samples(0).SequenceEqual(BoxerFeedback.Samples(1)),"head and body sound distinct");
                Check(BoxerFeedback.Pulse(.04f,.035f)==0&&BoxerFeedback.Pulse(0,.035f)==1,"contact material pulse expires at 35ms");
                foreach(var intent in Intents)
                {
                    var reference=Replay(intent,1/480f,.735f);
                    foreach(float frame in new[]{1/30f,1/60f,1/120f,.2f})
                    {
                        var current=Replay(intent,frame,.735f);
                        Check(Near(reference.hp,current.hp)&&reference.hits==current.hits&&reference.blocks==current.blocks&&reference.misses==current.misses,
                            "same solved contact at 30/60/120FPS and 200ms spike "+intent+" "+frame);
                        Check(Near(reference.capacity,current.capacity)&&Near(reference.stamina,current.stamina),"phase-sliced vitals equal high-rate reference "+intent+" "+frame);
                        Check(current.hits+current.blocks+current.misses==1,"one resolution per accepted receipt "+intent+" "+frame);
                    }
                    var far=Replay(intent,.2f,2f);Check(far.hp==100&&far.misses==1&&far.hits==0&&far.blocks==0,"200ms entire Extend not lost, long-range MISS "+intent);
                    foreach(int depletion in new[]{11,18}) // actual misses leave 51.6 / 20.8 endurance
                    {
                        var tiredReference=Replay(intent,1/480f,.735f,depletion);
                        var tired=Replay(intent,.2f,.735f,depletion);
                        Check(Near(tiredReference.hp,tired.hp)&&Near(tiredReference.capacity,tired.capacity)&&Near(tiredReference.stamina,tired.stamina),
                            "low energy contact and recovery remain frame-rate independent "+intent+" depletion="+depletion);
                    }
                }
                foreach(float frame in new[]{1/30f,1/60f,1/120f,.2f})
                {
                    var reference=ReplayBoth(1/480f);var current=ReplayBoth(frame);
                    Check(Near(reference.playerHP,current.playerHP)&&Near(reference.opponentHP,current.opponentHP)&&reference.resolutions==current.resolutions,
                        "simultaneous committed actors contact/vitals stable dt="+frame);
                    Check(current.resolutions==2,"both actors resolve once in simultaneous timeline dt="+frame);
                }
                File.WriteAllText(Path.Combine(Dir,"motion-trace.csv"),Trace.ToString());
                File.WriteAllText(Path.Combine(Dir,"motion-summary.csv"),Summary.ToString());
            }
            finally { PunchMotionProfile.Mode=before; }
            Log.AppendLine($"TOTAL={pass+fail} PASS={pass} FAIL={fail}");
            File.WriteAllText(Path.Combine(Dir,"punch-feel-tests.txt"),Log.ToString());
            Debug.Log($"PUNCH_FEEL_TESTS TOTAL={pass+fail} PASS={pass} FAIL={fail}");
            if(fail!=0)throw new Exception("Punch-feel gate failed: "+fail);
        }
        static (double hp,double capacity,double stamina,int hits,int blocks,int misses) Replay(PunchIntent intent,float frame,float distance,int depletion=0)
        {
            using var fixture=new Fixture(distance);
            for(int i=0;i<depletion;i++){var r=fixture.Telemetry.Bout.Accept(true,PunchIntent.RearOverhand);fixture.Telemetry.Bout.Resolve(true,r,CombatOutcome.Miss,false);}
            typeof(PlayerBoxer).GetMethod("OnPunchRequested",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(fixture.Player,new object[]{intent});
            float elapsed=0;
            while(elapsed<.8f-.000001f){float step=Mathf.Min(frame,.8f-elapsed);fixture.Rig.Simulate(step);elapsed+=step;}
            var t=fixture.Telemetry;return(t.Bout.Opponent.HP,t.Bout.Player.Capacity,t.Bout.Player.Stamina,t.PlayerHits,t.OpponentBlocks,t.PlayerMisses);
        }
        static (double playerHP,double opponentHP,int resolutions) ReplayBoth(float frame)
        {
            using var fixture=new Fixture(.735f);
            typeof(PlayerBoxer).GetMethod("OnPunchRequested",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(fixture.Player,new object[]{PunchIntent.RearOverhand});
            typeof(OpponentBoxer).GetMethod("StartAttack",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(fixture.Telemetry.Opponent,null);
            float elapsed=0;while(elapsed<1.2f-.000001f){float dt=Mathf.Min(frame,1.2f-elapsed);fixture.Rig.Simulate(dt);elapsed+=dt;}
            var t=fixture.Telemetry;return(t.Bout.Player.HP,t.Bout.Opponent.HP,t.PlayerHits+t.OpponentHits+t.PlayerBlocks+t.OpponentBlocks+t.PlayerMisses+t.OpponentMisses);
        }
        sealed class Fixture : IDisposable
        {
            readonly GameObject root=new("Punch timeline fixture");
            public readonly Phase0Telemetry Telemetry;
            public readonly PlayerBoxer Player;
            public readonly Round2CombatRig Rig;
            SphereCollider Sphere(Transform parent,string name)
            { var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);go.name=name;go.transform.SetParent(parent,false);return go.GetComponent<SphereCollider>(); }
            public Fixture(float distance)
            {
                Telemetry=root.AddComponent<Phase0Telemetry>();var input=root.AddComponent<BoxerInput>();
                var p=new GameObject("fixture player");p.transform.SetParent(root.transform);Player=p.AddComponent<PlayerBoxer>();
                var o=new GameObject("fixture opponent");o.transform.SetParent(root.transform);o.transform.position=Vector3.forward*distance;o.transform.rotation=Quaternion.Euler(0,180,0);
                var opponent=o.AddComponent<OpponentBoxer>();
                var ph=Sphere(p.transform,"Player Head");var pb=Sphere(p.transform,"Player Body");
                var pl=Sphere(p.transform,"Player Left Glove");var pr=Sphere(p.transform,"Player Right Glove");
                var oh=Sphere(o.transform,"Opponent Head");var ob=Sphere(o.transform,"Opponent Body");
                var ol=Sphere(o.transform,"Opponent Left Glove");var or=Sphere(o.transform,"Opponent Right Glove");
                Player.Initialize(input,opponent,Telemetry,ph.transform,ph,pb,pl.transform,pl,pr.transform,pr);
                opponent.Initialize(Player,Telemetry,ol.transform,ol,or.transform,or,oh,ob);
                Telemetry.Player=Player;Telemetry.Opponent=opponent;Telemetry.InputSource=input;
                Rig=root.AddComponent<Round2CombatRig>();Rig.Initialize(Player,opponent);
                Telemetry.RecordBoutStart();Player.SetCombatEnabled(true);opponent.SetCombatEnabled(true);
            }
            public void Dispose()=>UnityEngine.Object.DestroyImmediate(root);
        }
        static void WriteWav(string path,float[] samples)
        {
            using var writer=new BinaryWriter(File.Create(path));
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+samples.Length*2);writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
            writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(44100);writer.Write(88200);writer.Write((short)2);writer.Write((short)16);
            writer.Write(Encoding.ASCII.GetBytes("data"));writer.Write(samples.Length*2);foreach(float sample in samples)writer.Write((short)(sample*32767));
        }
    }
}
