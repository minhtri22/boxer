using System;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class Wave1SelfTests
    {
        private static int _passed, _failed;
        private static readonly StringBuilder Log = new();
        private static void Check(bool ok, string name)
        { if(ok)_passed++;else _failed++; Log.AppendLine((ok?"PASS ":"FAIL ")+name); }
        private static bool Near(double a,double b) => Math.Abs(a-b)<1e-7;
        public static void Run()
        {
            _passed=_failed=0; Log.Clear();
            var b=new CombatBout();
            Check(!b.Active && Near(b.Player.HP,100),"inactive initial state full resources");
            b.Accept(true,PunchIntent.Cross); b.Tick(3,ActionPhase.Guard,ActionPhase.Guard,0,0);
            Check(Near(b.Seconds,0)&&Near(b.Player.Capacity,100),"menu/tutorial no spend or regeneration");
            foreach(PunchIntent intent in new[]{PunchIntent.Jab,PunchIntent.Cross,PunchIntent.LeadHook,PunchIntent.RearHook,PunchIntent.LeadUppercut,PunchIntent.RearUppercut,PunchIntent.LeadOverhand,PunchIntent.RearOverhand})
            {
                b.Start(); var r=b.Accept(true,intent);
                Check(Near(b.Player.Capacity,100-CombatBout.Cost(intent))&&Near(b.Player.Stamina,100-CombatBout.Cost(intent)*.2),intent+" accepted cost exact");
                Check(Near(r.Quality,1)&&Near(r.RecoveryFactor,1),intent+" quality captured before cost");
                Check(b.Resolve(true,r,CombatOutcome.Hit,false)&&Near(b.Opponent.HP,100-CombatBout.BaseDamage(intent)),intent+" hit damage exact");
                double hp=b.Opponent.HP;
                Check(!b.Resolve(true,r,CombatOutcome.Hit,false)&&Near(b.Opponent.HP,hp),intent+" duplicate resolution ignored");
                b.Start(); r=b.Accept(false,intent); b.Resolve(false,r,CombatOutcome.Hit,false);
                Check(Near(b.Player.HP,hp),intent+" player/opponent symmetric");
            }
            b.Start(); var miss=b.Accept(true,PunchIntent.Cross);
            Check(b.Resolve(true,miss,CombatOutcome.Miss,false)&&Near(b.Opponent.HP,100),"MISS no HP change");
            Check(Near(b.Player.Capacity,84),"MISS still pays accepted action cost");
            b.Start(); var block=b.Accept(true,PunchIntent.Cross); b.Resolve(true,block,CombatOutcome.Block,false);
            Check(Near(b.Opponent.HP,100)&&Near(b.Opponent.Capacity,96),"BLOCK zero HP and bounded guard cost");
            b.Start(); var body=b.Accept(true,PunchIntent.Cross); b.Resolve(true,body,CombatOutcome.Hit,true);
            Check(Near(b.Opponent.HP,91)&&Near(b.Opponent.Stamina,96.85),"BODY hit stamina consequence exact");
            b.Start(); var honest=b.Accept(true,PunchIntent.Jab);
            b.Resolve(true,new AttackReceipt(honest.Id,PunchIntent.RearOverhand,1000),CombatOutcome.Hit,false);
            Check(Near(b.Opponent.HP,94),"caller cannot forge receipt quality or intent");
            b.Start(); var stale=b.Accept(true,PunchIntent.Cross); b.Start(); var fresh=b.Accept(true,PunchIntent.Jab);
            Check(fresh.Id!=stale.Id&&!b.Resolve(true,stale,CombatOutcome.Hit,false)&&Near(b.Opponent.HP,100),"previous-bout receipt invalid after reset");
            Check(!b.Resolve(false,fresh,CombatOutcome.Hit,false),"receipt cannot switch actor");
            Check(!b.Resolve(true,fresh,(CombatOutcome)99,false),"invalid outcome cannot consume receipt");
            Check(b.Resolve(true,fresh,CombatOutcome.Hit,false),"valid resolution after invalid attempt");
            double cap=b.Player.Capacity; b.Accept(true,PunchIntent.None); b.Accept(true,(PunchIntent)99);
            Check(Near(cap,b.Player.Capacity),"invalid or no-action request costs zero");
            b.Start(); double previous=2;
            for(int i=0;i<12;i++)
            {
                var r=b.Accept(true,PunchIntent.RearOverhand);
                Check(r.Quality<=previous&&r.Quality>=.4&&r.Quality<=1&&r.RecoveryFactor>=1&&r.RecoveryFactor<=1.45,"fatigue monotonic bounded snapshot "+i);
                previous=r.Quality; b.Resolve(true,r,CombatOutcome.Miss,false);
            }
            Check(Near(b.Player.Capacity,0)&&b.Player.Stamina>=0,"exhaustion clamp no negative resource");
            double st=b.Player.Stamina; b.Tick(1,ActionPhase.Guard,ActionPhase.Guard,0,0);
            Check(Near(b.Player.Capacity,18)&&Near(b.Player.Stamina,st+4),"guard capacity18 stamina4 per simulation second");
            b.Tick(1,ActionPhase.Recover,ActionPhase.Guard,0,0);
            Check(Near(b.Player.Capacity,24)&&Near(b.Player.Stamina,st+5),"recovery capacity6 stamina1 per second");
            b.Tick(1,ActionPhase.Extend,ActionPhase.Guard,1,0);
            Check(Near(b.Player.Capacity,24)&&Near(b.Player.Stamina,st+3),"active phase no regen actual locomotion cost");
            double hp0=b.Player.HP; b.Tick(10,ActionPhase.Guard,ActionPhase.Guard,0,0);
            Check(Near(b.Player.HP,hp0)&&b.Player.Capacity<=100&&b.Player.Stamina<=100,"HP never regenerates and regen caps");
            var same=new CombatBout(); same.Start(); var sr=same.Accept(true,PunchIntent.Cross); same.Resolve(true,sr,CombatOutcome.Miss,false);
            b.Start(); sr=b.Accept(true,PunchIntent.Cross); b.Resolve(true,sr,CombatOutcome.Miss,false);
            same.Tick(1,ActionPhase.Recover,ActionPhase.Guard,0,0);
            for(int i=0;i<120;i++) b.Tick(1.0/120,ActionPhase.Recover,ActionPhase.Guard,0,0);
            Check(Near(b.Player.Capacity,same.Player.Capacity)&&Near(b.Player.Stamina,same.Player.Stamina),"dt partition accounting tolerance");
            b.Start(); b.Tick(50,ActionPhase.Guard,ActionPhase.Guard,0,0);
            Check(b.Ended&&b.Result=="DRAW"&&b.EndReason=="POINTS"&&Near(b.Seconds,45),"timeout clamp and draw");
            b.Start(); var hit=b.Accept(true,PunchIntent.Cross); b.Resolve(true,hit,CombatOutcome.Hit,false); b.FinishTimeout();
            Check(b.Result=="PLAYER_WIN"&&b.EndReason=="POINTS","timeout winner by remaining HP");
            b.Start(); for(int i=0;i<100&&b.Active;i++) {var r=b.Accept(false,PunchIntent.RearOverhand);b.Resolve(false,r,CombatOutcome.Hit,false);}
            Check(b.Ended&&!b.Active&&b.Result=="OPPONENT_WIN"&&b.EndReason=="KO"&&Near(b.Player.HP,0),"HP zero ends bout exactly");
            double frozenSt=b.Opponent.Stamina; var after=b.Accept(false,PunchIntent.Jab); b.Tick(10,ActionPhase.Guard,ActionPhase.Guard,0,0);
            Check(after.Id==0&&!b.Resolve(false,hit,CombatOutcome.Hit,false)&&Near(b.Opponent.Stamina,frozenSt),"no writes after KO");
            b.Reset(); Check(!b.Ended&&b.Result=="PENDING"&&Near(b.Player.HP,100)&&Near(b.Player.Stamina,100)&&Near(b.Player.Capacity,100),"full rematch reset");
            try{b.Tick(double.NaN,ActionPhase.Guard,ActionPhase.Guard,0,0);Check(false,"invalid dt rejected");}catch(ArgumentOutOfRangeException){Check(true,"invalid dt rejected");}
            var f=new ProductFlow(); Check(f.Screen==ProductScreen.Home&&!f.Gameplay,"home blocks gameplay");
            Check(!f.Begin()&&f.Preview()&&f.Begin()&&f.Screen==ProductScreen.Fight,"first bout starts immediately without tutorial");
            Check(!f.Preview()&&!f.Begin(),"illegal mid-bout navigation rejected");
            f.TutorialFinished(); Check(f.Screen==ProductScreen.Fight&&f.Gameplay&&!f.TutorialSeen,"combat cannot falsely complete training");
            f.Finish();Check(f.Screen==ProductScreen.Result&&!f.Gameplay,"result blocks gameplay");
            Check(f.Begin()&&f.Screen==ProductScreen.Fight,"rematch skips already-shown tutorial");
            f.Home();Check(f.Preview()&&f.Begin()&&f.Screen==ProductScreen.Fight,"home preview next bout route");
            f.Home();Check(f.Begin(true)&&f.Screen==ProductScreen.Onboarding,"explicit practice replays tutorial");
            Check(!f.Preview()&&!f.Begin(),"training cannot silently start combat");
            f.TutorialFinished();Check(f.Screen==ProductScreen.Home&&!f.Gameplay&&f.TutorialSeen,"training completion returns Home not Fight");
            var restored=new ProductFlow();restored.RestoreTutorialSeen(true);
            Check(restored.TutorialSeen&&restored.Screen==ProductScreen.Home,"completed training preference restores without auto training");
            Check(restored.Preview()&&restored.Begin()&&restored.Screen==ProductScreen.Fight,"trained player still starts fight immediately");
            var progress=new OnboardingProgress();progress.ObserveHead(-.13f);progress.ObserveHead(.13f);
            Check(progress.HeadReady&&!progress.FootworkReady&&!progress.PunchesReady,"head practice isolated from other skills");
            progress.ObserveMovement(Vector2.left);progress.ObserveMovement(Vector2.right);progress.ObserveMovement(Vector2.up);progress.ObserveMovement(Vector2.down);
            Check(progress.FootworkReady&&!progress.PunchesReady,"movement practice requires four directions");
            foreach(var gesture in new[]{new GestureMetrics(Vector2.zero,0,.1f),new GestureMetrics(new Vector2(0,100),100,.25f),new GestureMetrics(new Vector2(0,-100),100,.25f),new GestureMetrics(new Vector2(100,0),100,.25f)})progress.ObservePunch(PunchGestureClassifier.Resolve(gesture,PunchIntent.None));
            Check(progress.PunchesReady,"tap up down horizontal map to all four families");
            string[] guideStages={"HEADCONTROL","HEADCONTROL","FOOTWORK","FOOTWORK","FOOTWORK","FOOTWORK","PUNCHES","PUNCHES","PUNCHES","PUNCHES","PUNCHES"};
            int[] guideSteps={0,1,0,1,2,3,0,1,2,3,4};
            string[] guideCues={"HEAD_LEFT","HEAD_RIGHT","MOVE_UP","MOVE_DOWN","MOVE_LEFT","MOVE_RIGHT","PUNCH_DOWN","PUNCH_UP","PUNCH_RIGHT","PUNCH_LEFT","TAP_REPEAT"};
            Vector2[] directions={Vector2.left,Vector2.right,Vector2.down,Vector2.up,Vector2.left,Vector2.right,Vector2.up,Vector2.down,Vector2.right,Vector2.left,Vector2.zero};
            for(int i=0;i<guideCues.Length;i++)
                Check(TrainingGestureGuide.Cue(guideStages[i],guideSteps[i]*2.4f+.1f)==guideCues[i]&&TrainingGestureGuide.Direction(guideCues[i])==directions[i],"light guide mapping "+guideCues[i]);
            Check(TrainingGestureGuide.Cue("PUNCHES",12.1f)=="PUNCH_DOWN","light guide loops all punch examples");
            Check(TrainingGestureGuide.Cue("",2)==string.Empty&&TrainingGestureGuide.Cue("Fight",2)==string.Empty,"light guide absent outside training");
            var illustrative=new OnboardingProgress();TrainingGestureGuide.Cue("PUNCHES",10);
            Check(!illustrative.HeadReady&&!illustrative.FootworkReady&&!illustrative.PunchesReady,"illustrative guide does not grant practice progress");
            Log.AppendLine($"TOTAL={_passed+_failed} PASS={_passed} FAIL={_failed}");
            string suite=Environment.GetEnvironmentVariable("BOXER_WAVE_EVIDENCE")=="combat-v3"?"combat-v3":"onboarding-v2";
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../evidence/wave1/"+suite));Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,"vitals-tests.txt"),Log.ToString()); Debug.Log(Log);
            if(_failed!=0)throw new Exception("Wave1 vitals/flow invariant failure");
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,"../../.."));
            var historical=Directory.GetFiles(Path.Combine(root,"evidence"),"*",SearchOption.AllDirectories)
                .Where(p=>!p.StartsWith(dir,StringComparison.OrdinalIgnoreCase)&&new[]{".txt",".csv",".json"}.Contains(Path.GetExtension(p)))
                .ToDictionary(p=>p,p=>File.ReadAllBytes(p));
            try { Round2SelfTests.RunAll(); }
            finally
            {
                foreach(var old in historical)
                    if(File.Exists(old.Key)&&!File.ReadAllBytes(old.Key).SequenceEqual(old.Value))
                    {
                        string dest=Path.Combine(dir,"regressions",Path.GetRelativePath(Path.Combine(root,"evidence"),old.Key));
                        Directory.CreateDirectory(Path.GetDirectoryName(dest)); File.Copy(old.Key,dest,true);
                        File.WriteAllBytes(old.Key,old.Value);
                    }
            }
        }
    }
}
