using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class MatchLedgerRuntimeChecks
    {
        static int _checks;static StringBuilder _log=new("SCOPE=ISOLATED_EDITOR_LEDGER_CONTROLLER_NOT_PHYSICS_OR_PHONE_UAT\n");
        static void Check(bool ok,string name){_checks++;_log.AppendLine((ok?"PASS ":"FAIL ")+name);if(!ok)throw new Exception(name);}
        public static void Before(BoxerBootstrap b)
        {
            string key=Environment.GetEnvironmentVariable("BOXER_LEDGER_TEST_STORE");
            Check(key!=null&&key.StartsWith("BOXER_TEST_LEDGER_",StringComparison.Ordinal),"only isolated native ledger key");
            Check(b.Wallet.Ready&&b.Wallet.Ledger.balance==0&&b.Wallet.Ledger.entries.Length==0,"fresh controller empty ledger no backfill");
            b.Wallet.Completed(MatchLedgerSelfTests.Entry().review);Check(b.Wallet.Ledger.balance==0&&b.Wallet.PendingCount==0,"uncaptured incomplete review cannot grant reward");
            b.BrowserLedgerState("not json");b.BrowserLedgerState("{\"session\":\"stale\",\"ledger\":{\"schema\":1,\"balance\":100,\"entries\":[]}}");
            Check(b.Wallet.Ledger.balance==0,"malformed stale ledger callback cannot alter wallet");
            Check(b.ShowHistory()&&b.Flow.Screen==ProductScreen.History&&!b.Flow.Gameplay,"actual controller opens locked history");
            b.BeginProductBout();Check(b.Flow.Screen==ProductScreen.History&&b.Wallet.Ledger.balance==0,"history cannot start fight");
            b.BrowserHistoryCommand("retry");Check(b.Wallet.PendingCount==0&&b.Wallet.Ledger.balance==0,"retry without completed entry cannot grant");
            b.BrowserHistoryCommand("home");Check(b.Flow.Screen==ProductScreen.Home,"history Home route");
        }
        public static void After(BoxerBootstrap b)
        {
            var l=b.Wallet.Ledger;
            Check(l.Valid&&l.entries.Length>0&&l.entries[0].review.reason=="KO"&&l.entries[0].reward==100,"real controller terminal callback saved test KO at ordinary reward");
            Check(b.Wallet.PendingCount==0,"all native test completion acknowledgements cleared");
            int sum=0;foreach(var e in l.entries)sum+=MatchLedger.Reward(e.review.result);
            Check(l.balance==sum,"controller wallet matches completed-history sum");
            string key=Environment.GetEnvironmentVariable("BOXER_LEDGER_TEST_STORE");
            Check(MatchLedger.Parse(PlayerPrefs.GetString(key))?.Serialize()==l.Serialize(),"coupled native preference matches confirmed ledger");
            _log.AppendLine("CHECKS="+_checks+" PASS="+_checks+" FAIL=0");
            File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath,"../../../evidence/wave1/history-wallet/ledger-runtime.txt")),_log.ToString());
            PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();
        }
    }
}
