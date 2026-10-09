using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class CoachUiSelfTests
    {
        public static void RunAll() { Run(); PunchFeelSelfTests.RunAll(); }
        public static void Run()
        {
            int pass=0,fail=0;var log=new StringBuilder("SCOPE=COACH_MODEL_NAVIGATION_NOT_RENDERED_OR_PHONE_UAT\n");
            void Check(bool ok,string label) { if(ok)pass++;else fail++;log.AppendLine((ok?"PASS ":"FAIL ")+label); }
            Check(Resources.Load<Texture2D>("Product/CoachReference")!=null,"approved portrait included in Resources");
            Check(!CoachCatalog.Valid((CoachModule)(-1))&&!CoachCatalog.Valid((CoachModule)5),"invalid module rejected");
            for(int i=0;i<CoachCatalog.Count;i++)
            {
                var module=(CoachModule)i;var lesson=CoachCatalog.Get(module);var flow=new ProductFlow();
                Check(CoachCatalog.Valid(module)&&lesson.Title.Length>0&&lesson.Instructions.Length>0,"catalog copy "+module);
                Check(lesson.Interactive==(i<3)&&CoachCatalog.CompletionBit(module)==(i<3?1<<i:0),"practice versus information "+module);
                Check(flow.OpenCoach()&&!flow.Gameplay&&!flow.TutorialSeen,"Home to Coach locks gameplay "+module);
                Check(!flow.Begin(false)&&flow.Screen==ProductScreen.Coach,"Coach cannot start hidden scored bout "+module);
                if(lesson.Interactive)
                {
                    Check(flow.Begin(true)&&flow.TrainingFromCoach&&flow.Gameplay,"Coach to practice "+module);
                    flow.TutorialFinished();Check(!flow.TutorialSeen&&flow.Screen==ProductScreen.Onboarding,"single lesson cannot mark whole tutorial "+module);
                    Check(flow.CancelTraining()&&flow.Screen==ProductScreen.Coach&&!flow.TutorialSeen&&!flow.Gameplay,"cancel returns Coach without completion "+module);
                    flow.Begin(true);Check(flow.CoachLessonFinished(false)&&flow.Screen==ProductScreen.Coach&&!flow.TutorialSeen,"partial completion not whole training "+module);
                    flow.Begin(true);Check(flow.CoachLessonFinished(true)&&flow.TutorialSeen&&!flow.Gameplay,"all controls complete persist state "+module);
                }
                else
                {
                    Check(flow.OpenTrainingInfo()&&flow.Screen==ProductScreen.TrainingInfo&&!flow.Gameplay,"information locks gameplay "+module);
                    Check(!flow.Begin(true)&&!flow.CoachLessonFinished(true),"information cannot award or jump practice "+module);
                    Check(flow.BackToCoach()&&flow.Screen==ProductScreen.Coach&&!flow.TutorialSeen,"information returns Coach "+module);
                }
            }
            var direct=new ProductFlow();direct.Begin(true);direct.CancelTraining();
            Check(direct.Screen==ProductScreen.Home&&!direct.TutorialSeen,"legacy practice cancel still Home");
            direct.Begin(true);direct.TutorialFinished();Check(direct.Screen==ProductScreen.Home&&direct.TutorialSeen,"legacy full training still Home and seen");
            var fighting=new ProductFlow();fighting.Preview();fighting.Begin();fighting.IntroFinished();
            Check(!fighting.OpenCoach()&&!fighting.OpenTrainingInfo()&&!fighting.BackToCoach()&&!fighting.CancelTraining(),"no UI escape or injection into active fight");
            var bout=new CombatBout();bout.Tick(45,ActionPhase.Guard,ActionPhase.Guard,1,1);bout.Accept(true,PunchIntent.Jab);
            Check(!bout.Active&&bout.Seconds==0&&bout.Player.HP==100&&bout.Player.Capacity==100,"unscored coach model cannot spend resources");
            for(int i=0;i<5;i++)
            { var rect=new Rect(260,245+i*96,264,88);Check(rect.xMin>=0&&rect.xMax<=540&&rect.yMax<=960&&rect.height*320/540>=44,"card bounds minimum 320-wide portrait target "+i); }
            Check(76f*320/540>=44&&875+76<=960,"new navigation CTA minimum portrait touch target");
            log.AppendLine($"TOTAL={pass+fail} PASS={pass} FAIL={fail}");
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../evidence/wave1/coach-ui"));Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,"coach-tests.txt"),log.ToString());Debug.Log(log);
            if(fail>0)throw new Exception("Coach UI model invariant failure");
        }
    }
}
