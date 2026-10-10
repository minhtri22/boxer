using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class FighterProfileSelfTests
    {
        public static void RunAll() { Run(); ArenaSurroundSelfTests.RunAll(); }
        public static void Run()
        {
            int pass=0,fail=0; var log=new StringBuilder("SCOPE=LOCAL_IDENTITY_AND_ROUTE_INVARIANTS_NOT_PHONE_UAT\n");
            void Check(bool ok,string name) { if(ok)pass++;else fail++;log.AppendLine((ok?"PASS ":"FAIL ")+name); }
            var p=FighterProfile.Candidate(null,"  Nguyễn   Trí  ","Việt Nam");
            Check(p != null && p.Valid && p.name=="Nguyễn Trí" && p.nationality=="Việt Nam","normalize Unicode and spaces without assumed nationality");
            Check(FighterProfile.Parse(p.Serialize())?.Serialize()==p.Serialize(),"exact identity round trip");
            Check(FighterProfile.Candidate(p,"Minh Trí","Mexico").id==p.id,"rename preserves local ID");
            Check(FighterProfile.Candidate(null,"Ramírez","Mexico").id!=p.id,"separate first save has separate ID");
            Check(FighterProfile.Candidate(null,"Nguye\u0302\u0303n","Việt Nam").name=="Nguyễn","NFC combining marks");
            foreach(string v in new[]{"", " ", "<b>x</b>", "A\nB", "A\tB", "A\u202eB", "A\u200bB", "😎", "\ud800", "-name", new string('a',25), new string('a',97)})
                Check(FighterProfile.Candidate(p,v,"Việt Nam")==null,"invalid name rejected "+v.Length);
            foreach(string v in new[]{"", " ", "<script>", "US\rA", "A\u0000", new string('a',41), new string('a',161)})
                Check(FighterProfile.Candidate(p,"Minh",v)==null,"invalid nationality rejected "+v.Length);
            foreach(string v in new[]{"Việt Nam","Côte d’Ivoire","Timor-Leste","México","대한민국","日本"})
                Check(FighterProfile.Candidate(p,"O'Connor",v)?.Valid==true,"self reported Unicode nationality "+v);
            Check(FighterProfile.Candidate(p,new string('a',24),new string('b',40))?.Valid==true,"exact text limits accepted");
            foreach(string json in new[]{"", "null", "{}", "[1]", "not json", new string('x',2049),p.Serialize().Replace("\"schema\":1,",""),p.Serialize().Replace("\"schema\":1","\"schema\":2")})
                Check(FighterProfile.Parse(json)==null,"malformed unsupported or missing schema rejected "+json.Length);
            foreach(string id in new[]{null,"","fixture",new string('a',31),new string('a',33),new string('A',32),new string('g',32)})
                Check(!FighterProfile.ValidId(id),"invalid ID rejected");
            var flow=new ProductFlow();
            Check(flow.OpenProfile()&&flow.Screen==ProductScreen.Profile&&!flow.Gameplay,"Home opens locked profile");
            Check(!flow.OpenProfile()&&!flow.Preview()&&!flow.Begin()&&!flow.Begin(true)&&!flow.IntroFinished()&&!flow.CoachLessonFinished(true)&&!flow.OpenCoachReview(),"profile cannot start combat or fabricate progress");
            Check(flow.OpenCoach()&&flow.OpenCoachReview()&&!flow.Gameplay,"profile link uses existing Coach review route");
            foreach(ProductScreen screen in Enum.GetValues(typeof(ProductScreen)))
            {
                var f=new ProductFlow();
                if(screen==ProductScreen.Profile)f.OpenProfile();
                if(screen==ProductScreen.Coach||screen==ProductScreen.CoachReview||screen==ProductScreen.TrainingInfo||screen==ProductScreen.Onboarding)f.OpenCoach();
                if(screen==ProductScreen.CoachReview)f.OpenCoachReview();
                if(screen==ProductScreen.TrainingInfo)f.OpenTrainingInfo();
                if(screen==ProductScreen.Onboarding)f.Begin(true);
                if(screen==ProductScreen.Preview||screen==ProductScreen.Intro||screen==ProductScreen.Fight||screen==ProductScreen.Result)f.Preview();
                if(screen==ProductScreen.Intro||screen==ProductScreen.Fight||screen==ProductScreen.Result)f.Begin();
                if(screen==ProductScreen.Fight||screen==ProductScreen.Result)f.IntroFinished();
                if(screen==ProductScreen.Result)f.Finish();
                Check(f.Screen==screen,"test reaches real route "+screen);
                Check(f.OpenProfile()==(screen==ProductScreen.Home),"profile route restricted "+screen);
            }
            log.AppendLine($"TOTAL={pass+fail} PASS={pass} FAIL={fail}");
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../evidence/wave1/fighter-profile"));Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,"profile-tests.txt"),log.ToString());Debug.Log(log.ToString());
            if(fail>0)throw new Exception("Profile invariant failure");
        }
    }
}
