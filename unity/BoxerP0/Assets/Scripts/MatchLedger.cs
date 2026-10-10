using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoxerP0
{
    [Serializable] public sealed class MatchHistoryEntry
    {
        public CoachMatchReview review;
        public string profileId, fighterName, nationality, completedUtc;
        public int reward;
        public bool Valid => review != null && review.Valid && FighterProfile.ValidId(review.id) &&
            (profileId=="" ? fighterName=="BOXER" && nationality=="" :
                FighterProfile.ValidId(profileId) && FighterProfile.Normalize(fighterName,24,out string n) && n==fighterName &&
                FighterProfile.Normalize(nationality,40,out string c) && c==nationality) &&
            completedUtc != null && DateTime.TryParseExact(completedUtc,"yyyy-MM-ddTHH:mm:ss.fffZ",System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,out _) &&
            reward==MatchLedger.Reward(review.result);
        public string Serialize() => Valid ? JsonUtility.ToJson(this) : "";
    }
    [Serializable] public sealed class MatchLedger
    {
        public const int Limit=1000;
        public int schema, balance;
        public MatchHistoryEntry[] entries;
        public static MatchLedger Empty()=>new() {schema=1,balance=0,entries=Array.Empty<MatchHistoryEntry>()};
        public static int Reward(string result)=>result=="PLAYER_WIN"?100:result=="DRAW"?50:result=="OPPONENT_WIN"?25:-1;
        public bool Valid
        {
            get
            {
                if(schema!=1||entries==null||entries.Length>Limit)return false;
                int sum=0;var ids=new HashSet<string>();
                foreach(var e in entries){if(e==null||!e.Valid||!ids.Add(e.review.id))return false;sum+=e.reward;}
                return balance==sum;
            }
        }
        public int Count(string result) {int n=0;foreach(var e in entries)if(e.review.result==result)n++;return n;}
        public MatchHistoryEntry Find(string id) {foreach(var e in entries)if(e.review.id==id)return e;return null;}
        public MatchLedger Append(MatchHistoryEntry entry)
        {
            if(!Valid||entry==null||!entry.Valid)return null;
            var existing=Find(entry.review.id);
            if(existing!=null)return existing.Serialize()==entry.Serialize()?this:null;
            if(entries.Length>=Limit)return null;
            var next=new MatchHistoryEntry[entries.Length+1];Array.Copy(entries,next,entries.Length);next[entries.Length]=JsonUtility.FromJson<MatchHistoryEntry>(entry.Serialize());
            return new MatchLedger {schema=1,balance=balance+entry.reward,entries=next};
        }
        public string Serialize()=>Valid?JsonUtility.ToJson(this):"";
        public static MatchLedger Parse(string json)
        {
            if(string.IsNullOrEmpty(json)||json.Length>1500000)return null;
            try {var value=JsonUtility.FromJson<MatchLedger>(json);return value!=null&&value.Valid?value:null;}
            catch(ArgumentException){return null;}
        }
    }
}
