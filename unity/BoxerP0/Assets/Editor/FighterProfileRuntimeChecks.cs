using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class FighterProfileRuntimeChecks
    {
        public static void Run(BoxerBootstrap b)
        {
            int count=0; var log=new StringBuilder("SCOPE=EDITOR_PROFILE_LIFECYCLE_WITH_SCOPED_PREFERENCE_BACKUP_NOT_PHONE_UAT\n");
            void Check(bool ok,string text) { count++;log.AppendLine((ok?"PASS ":"FAIL ")+text);if(!ok)throw new Exception(text); }
            string key=FighterProfileController.Preference;
            bool existed=PlayerPrefs.HasKey(key);string prior=PlayerPrefs.GetString(key,"");
            string Token()=> (string)typeof(FighterProfileController).GetField("_token",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(b.Profile);
            string Save(string token)=>"{\"token\":\""+token+"\",\"action\":\"save\",\"name\":\"Minh Trí\",\"nationality\":\"Việt Nam\"}";
            try
            {
                PlayerPrefs.DeleteKey(key);b.Profile.Initialize(b);
                Check(!b.Profile.HasSaved&&b.Profile.DisplayName=="BOXER","unsaved defaults honest");
                Check(b.ShowProfile()&&b.Flow.Screen==ProductScreen.Profile,"native actual profile route");
                string first=Token();Check(FighterProfile.ValidId(first),"session token scoped");
                b.BrowserProfileCommand(Save("stale"));Check(!b.Profile.HasSaved&&!PlayerPrefs.HasKey(key),"stale save cannot persist");
                b.BrowserProfileCommand("not json");Check(!b.Profile.HasSaved,"malformed request rejected");
                b.Profile.DraftName="";b.Profile.DraftNationality="Việt Nam";
                Check(!b.Profile.Save()&&!b.Profile.HasSaved&&!PlayerPrefs.HasKey(key),"invalid save cannot persist");
                b.Profile.DraftName="Draft";b.Profile.Cancel();Check(b.Flow.Screen==ProductScreen.Home&&!PlayerPrefs.HasKey(key),"cancel discards unsaved draft");
                b.BrowserProfileCommand(Save(first));Check(!b.Profile.HasSaved,"callback after close rejected");
                b.ShowProfile();Check(Token()!=first,"reopen creates new session");
                b.BrowserProfileCommand(Save(first));Check(!b.Profile.HasSaved,"previous-session callback rejected");
                b.BrowserProfileCommand(Save(Token()));Check(b.Flow.Screen==ProductScreen.Home&&b.Profile.HasSaved,"matching valid save returns Home");
                string id=b.Profile.Identity,stored=PlayerPrefs.GetString(key);
                Check(FighterProfile.Parse(stored)?.id==id,"actual saved preference validates");
                b.BrowserProfileCommand(Save(first));Check(PlayerPrefs.GetString(key)==stored,"duplicate late save unchanged");
                b.ShowProfile();b.Profile.DraftName="Not saved";b.Profile.Cancel();Check(b.Profile.DisplayName=="Minh Trí"&&PlayerPrefs.GetString(key)==stored,"cancel edits preserves saved identity");
                b.Profile.Initialize(b);Check(b.Profile.Identity==id&&b.Profile.DisplayName=="Minh Trí","reload native preferences retains identity");
                b.ShowProfile();b.Profile.DraftName="Tên mới";b.Profile.DraftNationality="Mexico";
                Check(b.Profile.Save()&&b.Profile.Identity==id&&b.Profile.DisplayName=="Tên mới","explicit rename preserves ID");
                b.ShowPreview();b.BrowserProfileCommand(Save(first));Check(b.Flow.Screen==ProductScreen.Preview&&b.Profile.DisplayName=="Tên mới","offscreen save rejected");
                b.ReturnHome();b.ShowProfile();b.Profile.Close();b.ReturnHome();
                PlayerPrefs.SetString(key,"corrupt");b.Profile.Initialize(b);
                Check(b.Profile.InvalidStored&&!b.Profile.HasSaved&&PlayerPrefs.GetString(key)=="corrupt","corrupt storage reported not silently rewritten");
                b.ShowProfile();b.Profile.Cancel();Check(PlayerPrefs.GetString(key)=="corrupt","cancel preserves corrupt evidence");
            }
            finally
            {
                b.Profile.Close();b.ReturnHome();
                if(existed)PlayerPrefs.SetString(key,prior);else PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();b.Profile.Initialize(b);
                log.AppendLine($"CHECKS={count} PREFERENCE_RESTORED={(PlayerPrefs.HasKey(key)==existed&&PlayerPrefs.GetString(key,"")==prior)}");
                File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath,"../../../evidence/wave1/fighter-profile/profile-runtime.txt")),log.ToString());
            }
        }
    }
}
