using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class MatchLedgerSelfTests
    {
        [Serializable] private sealed class Fixture {public string label;public bool valid;public MatchLedger ledger;}
        [Serializable] private sealed class Fixtures {public Fixture[] cases;}
        static string Dir=>Path.GetFullPath(Path.Combine(Application.dataPath,"../../../evidence/wave1/history-wallet"));
        public static MatchHistoryEntry Entry(int number=1,string result="PLAYER_WIN",string reason="POINTS")
        {
            return new MatchHistoryEntry {review=new CoachMatchReview {schema=1,id=number.ToString("x32"),sourceVersion="native-ledger-fixture",result=result,reason=reason,seconds=45,
                playerHP=result=="PLAYER_WIN"?90:result=="DRAW"?80:60,opponentHP=result=="PLAYER_WIN"?60:result=="DRAW"?80:90,
                playerStamina=70.25,playerCapacity=90.5,accepted=6,hits=2,blocked=2,misses=1,received=3,defended=4,opponentMisses=1},
                profileId="",fighterName="BOXER",nationality="",completedUtc="2026-10-11T00:00:00.000Z",reward=MatchLedger.Reward(result)};
        }
        public static void RunAll(){Run();FighterProfileSelfTests.RunAll();}
        public static void Run()
        {
            int pass=0,fail=0;var log=new StringBuilder("SCOPE=PURE_COMPLETED_HISTORY_REWARD_INVARIANTS_NOT_DEVICE_UAT\n");var fixtures=new List<Fixture>();
            void Check(bool ok,string label){if(ok)pass++;else fail++;log.AppendLine((ok?"PASS ":"FAIL ")+label);}
            MatchLedger Clone(MatchLedger l)=>JsonUtility.FromJson<MatchLedger>(JsonUtility.ToJson(l));
            void Shared(MatchLedger l,bool valid,string label){Check(l.Valid==valid,label);fixtures.Add(new Fixture {label=label,valid=valid,ledger=Clone(l)});}
            var empty=MatchLedger.Empty();Shared(empty,true,"empty wallet honest");Check(empty.balance==0&&empty.entries.Length==0,"empty has no paid old review");
            var win=empty.Append(Entry());Shared(win,true,"valid win ledger");Check(win.balance==100&&empty.balance==0&&empty.entries.Length==0,"win 100 without changing prior");
            var draw=win.Append(Entry(2,"DRAW"));Shared(draw,true,"valid draw ledger");Check(draw.balance==150,"draw 50");
            var loss=draw.Append(Entry(3,"OPPONENT_WIN"));Shared(loss,true,"valid loss ledger");Check(loss.balance==175&&loss.Count("PLAYER_WIN")==1&&loss.Count("DRAW")==1&&loss.Count("OPPONENT_WIN")==1,"loss 25 and genuine tally");
            Check(loss.Append(Entry()).Serialize()==loss.Serialize(),"same completed ID retry never pays twice");
            var conflict=Entry();conflict.completedUtc="2026-10-11T01:00:00.000Z";Check(loss.Append(conflict)==null,"same ID different content rejected");
            Check(MatchLedger.Parse(loss.Serialize())?.Serialize()==loss.Serialize(),"exact reload round trip");
            var ko=Entry(4);ko.review.reason="KO";ko.review.seconds=23;ko.review.opponentHP=0;var withKo=loss.Append(ko);Shared(withKo,true,"valid KO win");Check(withKo.balance==275,"KO no extra bonus");
            var koLoss=Entry(5,"OPPONENT_WIN");koLoss.review.reason="KO";koLoss.review.playerHP=0;Shared(withKo.Append(koLoss),true,"valid KO loss");
            var named=Entry(6);named.profileId=new string('a',32);named.fighterName="Nguyễn Trí";named.nationality="Việt Nam";var namedLedger=withKo.Append(named);Shared(namedLedger,true,"named identity snapshot");named.fighterName="Rename after capture";Check(namedLedger.entries[4].fighterName=="Nguyễn Trí","entry is detached snapshot");
            foreach(string country in new[]{"Côte d’Ivoire","대한민국","日本","Timor-Leste"}){var e=Entry(7);e.profileId=new string('b',32);e.fighterName="O'Connor";e.nationality=country;Shared(empty.Append(e),true,"Unicode nationalities "+country);}
            foreach(var edit in new Action<MatchLedger>[] {x=>x.schema=0,x=>x.schema=2,x=>x.balance=-1,x=>x.balance=176,x=>x.entries=null,
                x=>x.entries[0].reward=101,x=>x.entries[0].fighterName="Not BOXER",x=>x.entries[0].nationality="Việt Nam",
                x=>x.entries[0].completedUtc="yesterday",x=>x.entries[0].completedUtc="2026-02-30T00:00:00.000Z",x=>x.entries[0].review.id="fixture",x=>x.entries[0].review.result="IN_PROGRESS",
                x=>x.entries[0].review.hits=100,x=>x.entries[0].review.seconds=46,x=>x.entries[0].review.reason="KO",x=>x.entries[0].review.playerHP=0,x=>x.entries[0].review.opponentHP=101,
                x=>x.entries[0]=null,x=>x.entries[1]=x.entries[0]}){var bad=Clone(loss);edit(bad);Shared(bad,false,"invalid ledger fixture "+fixtures.Count);Check(bad.Serialize()=="","invalid data cannot serialize as confirmed balance");}
            foreach(string json in new[]{"", "null","{}","not json","[]",new string('a',1500001),loss.Serialize().Replace("\"schema\":1,","",StringComparison.Ordinal)})Check(MatchLedger.Parse(json)==null,"invalid/missing-schema storage rejected "+json.Length);
            var nullIdentity=Clone(loss);nullIdentity.entries[0].profileId=null;
            Check(!nullIdentity.Valid,"native null identity rejected before serialization");Check(nullIdentity.Serialize()=="","null cannot be confirmed; JsonUtility would canonicalize raw null to empty so this is not a shared serialized fixture");
            Check(empty.Append(null)==null,"no terminal entry means no reward");var unfinished=Entry();unfinished.review.result="IN_PROGRESS";Check(empty.Append(unfinished)==null,"in-progress cannot earn");
            var capped=empty;for(int i=1;i<=MatchLedger.Limit;i++)capped=capped.Append(Entry(i));Check(capped.Valid&&capped.balance==100000&&capped.entries.Length==1000,"all thousand IDs retained");
            Check(capped.Append(Entry(1001))==null&&capped.entries.Length==1000,"capacity rejects without prune");Check(capped.Append(Entry()).balance==100000,"duplicate at capacity idempotent");
            var flow=new ProductFlow();Check(flow.OpenHistory()&&!flow.Gameplay,"Home opens locked history");Check(!flow.Begin()&&!flow.Begin(true)&&!flow.Preview()&&!flow.OpenProfile()&&!flow.CoachLessonFinished(true),"history cannot play or grant lesson");flow.Home();flow.OpenProfile();Check(flow.OpenHistory(),"Profile history route");flow.Home();flow.Preview();flow.Begin();Check(!flow.OpenHistory(),"Intro cannot open history");flow.IntroFinished();Check(!flow.OpenHistory(),"Fight cannot open history");flow.Finish();Check(flow.OpenHistory(),"Result history route");
            Directory.CreateDirectory(Dir);log.AppendLine($"TOTAL={pass+fail} PASS={pass} FAIL={fail}");File.WriteAllText(Path.Combine(Dir,"ledger-tests.txt"),log.ToString());File.WriteAllText(Path.Combine(Dir,"shared-ledger-fixtures.json"),JsonUtility.ToJson(new Fixtures {cases=fixtures.ToArray()},true));Debug.Log(log.ToString());
            if(fail>0)throw new Exception("Ledger invariant failure");
        }
    }
}
