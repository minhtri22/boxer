using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class CoachAnalysisSelfTests
    {
        public static void RunAll() { Run(); CoachUiSelfTests.Run(); PunchFeelSelfTests.RunAll(); }
        public static void Run()
        {
            int pass=0,fail=0;var log=new StringBuilder("SCOPE=COACH_COMPLETED_RECORD_AND_NAVIGATION_NOT_PHONE_UAT\n");
            void Check(bool ok,string label) { if(ok)pass++;else fail++;log.AppendLine((ok?"PASS ":"FAIL ")+label); }
            CoachMatchReview Fixture()=>new() { schema=1,id="native-fixture",sourceVersion="native-test",result="OPPONENT_WIN",reason="POINTS",seconds=45,
                playerHP=72.25,opponentHP=90,playerStamina=55.125,playerCapacity=30.5,accepted=10,hits=2,blocked=5,misses=2,received=4,defended=3,opponentMisses=1 };
            var r=Fixture();Check(r.Valid,"completed valid record");
            Check(r.Resolved==9&&r.Unresolved==1&&Math.Abs(r.Accuracy-200.0/9)<1e-9,"accuracy excludes unresolved and blocked shots are not hits");
            var restored=CoachMatchReview.Parse(r.Serialize());
            Check(restored!=null&&restored.Serialize()==r.Serialize(),"exact serialized round trip preserves endpoint values and counters");
            Check(r.Suggestions().Length==3&&r.Suggestions()[0]==CoachModule.Head&&r.Suggestions()[1]==CoachModule.Footwork&&r.Suggestions()[2]==CoachModule.Punches,"observed received/missed outcomes map to existing drills");
            foreach(var module in r.Suggestions())Check(CoachCatalog.Valid(module)&&r.SuggestionReason(module).Length>0,"real module and evidence reason "+module);
            r.accepted=r.hits=r.blocked=r.misses=r.received=0;
            Check(r.Valid&&r.Accuracy==0&&r.Unresolved==0,"zero shots has defined no-data denominator");
            Check(r.Suggestions()[0]==CoachModule.Punches&&r.Suggestions()[1]==CoachModule.Conditioning&&r.Suggestions()[2]==CoachModule.Guard,"zero shots uses real punch lesson plus labelled information only");
            Check(r.StatsText.Contains("chưa có đòn đã xét"),"zero denominator is not labelled 0 percent accuracy");
            foreach(string json in new[]{"", "null", "{}", "[1]", "not json", new string('x',4097)})Check(CoachMatchReview.Parse(json)==null,"invalid persisted record rejected "+json.Length);
            foreach(var edit in new Action<CoachMatchReview>[] {
                x=>x.schema=0,x=>x.schema=2,x=>x.id=null,x=>x.sourceVersion=null,x=>x.opponent="UNKNOWN",x=>x.result="IN_PROGRESS",x=>x.reason="NONE",
                x=>x.seconds=double.NaN,x=>x.seconds=double.PositiveInfinity,x=>x.seconds=46,x=>x.seconds=-1,x=>x.playerHP=-1,x=>x.opponentHP=101,
                x=>x.playerStamina=double.NaN,x=>x.playerCapacity=101,x=>x.accepted=0,x=>x.hits=-1,x=>x.blocked=10001,x=>x.misses=-1,
                x=>x.received=-1,x=>x.defended=-1,x=>x.opponentMisses=-1,x=>x.result="PLAYER_WIN",x=>x.reason="KO" })
            {var bad=Fixture();edit(bad);Check(!bad.Valid&&bad.Serialize()==string.Empty,"invalid endpoint/counter/schema/verdict cannot persist");}
            Check(CoachMatchReview.Parse(Fixture().Serialize().Replace("\"schema\":1,",""))==null,"missing schema cannot masquerade as current record");
            r=Fixture();r.reason="KO";r.result="PLAYER_WIN";r.opponentHP=0;Check(r.Valid&&CoachMatchReview.Parse(r.Serialize())!=null,"valid HP-zero terminal record retained");
            Check(CoachMatchReview.Capture(null,"test")==null,"no fabricated record without completed telemetry");
            foreach(var screen in new[]{ProductScreen.Home,ProductScreen.Preview,ProductScreen.Intro,ProductScreen.Fight,ProductScreen.Onboarding,ProductScreen.TrainingInfo})
            {
                var flow=new ProductFlow();
                if(screen==ProductScreen.Preview)flow.Preview();
                if(screen==ProductScreen.Intro||screen==ProductScreen.Fight){flow.Preview();flow.Begin();if(screen==ProductScreen.Fight)flow.IntroFinished();}
                if(screen==ProductScreen.Onboarding||screen==ProductScreen.TrainingInfo){flow.OpenCoach();if(screen==ProductScreen.Onboarding)flow.Begin(true);else flow.OpenTrainingInfo();}
                Check(!flow.OpenCoachReview()&&flow.Screen==screen,"review illegal route rejected "+screen);
            }
            var coach=new ProductFlow();coach.OpenCoach();Check(coach.OpenCoachReview()&&!coach.Gameplay,"Coach opens locked review");
            Check(!coach.Begin(true)&&!coach.Begin()&&!coach.CoachLessonFinished(true),"review cannot directly grant progress or start combat");
            Check(coach.BackToCoach()&&coach.Screen==ProductScreen.Coach,"review back routes Coach");
            var result=new ProductFlow();result.Preview();result.Begin();result.IntroFinished();result.Finish();
            Check(result.OpenCoachReview()&&result.Screen==ProductScreen.CoachReview&&!result.Gameplay,"completed Result opens locked review");
            for(int i=0;i<3;i++){var card=new Rect(42,550+i*94,456,88);Check(card.yMax<=855&&card.height*320/540>=44,"supplemental card fits minimum phone target "+i);}
            log.AppendLine($"TOTAL={pass+fail} PASS={pass} FAIL={fail}");
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../evidence/wave1/coach-analysis"));Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,"analysis-tests.txt"),log.ToString());Debug.Log(log);
            if(fail>0)throw new Exception("Coach analysis invariant failure");
        }
    }
}
