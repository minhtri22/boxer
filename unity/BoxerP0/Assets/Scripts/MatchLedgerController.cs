using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BoxerP0
{
    public sealed class MatchLedgerController:MonoBehaviour
    {
        public const string Preference="BOXER_MATCH_LEDGER_V1";
        private BoxerBootstrap _bootstrap;
        private MatchLedger _ledger;
        private string _session, _fighterId, _fighterName, _nationality;
        private readonly Dictionary<string,MatchHistoryEntry> _pending=new();
        private bool _started;
        private bool _historyOpen, _historyKeyboard;
        public bool Ready=>_ledger!=null;
        public MatchLedger Ledger=>_ledger;
        public string Error {get;private set;}="";
        public int PendingCount=>_pending.Count;
        public string LatestId {get;private set;}="";
        public string RewardStatus => !Ready ? (Error.Length>0?"Không đọc được ví; không xác nhận coin":"Đang đọc ví cục bộ…") :
            PendingCount>0 ? (Error.Length>0?"Chưa lưu thưởng · mở Lịch sử để thử lại":"Đang lưu trận và thưởng…") : $"VÍ CỤC BỘ · {_ledger.balance} COIN";
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void BoxerLedgerCommand(int command,string payload);
#endif
        [Serializable] private sealed class Envelope {public string session,id,error;public MatchLedger ledger;}
        [Serializable] private sealed class Submission {public string session;public MatchHistoryEntry entry;}
        [Serializable] private sealed class View {public bool open;public string status,error;public int pending;public MatchLedger ledger;}
        private string StorageKey
        {
            get
            {
#if UNITY_EDITOR
                string key=Environment.GetEnvironmentVariable("BOXER_LEDGER_TEST_STORE");
                if(key!=null&&key.StartsWith("BOXER_TEST_LEDGER_",StringComparison.Ordinal))return key;
#endif
                return Preference;
            }
        }
        public void Initialize(BoxerBootstrap bootstrap)
        {
            _bootstrap=bootstrap;_session=Guid.NewGuid().ToString("N");
#if UNITY_WEBGL && !UNITY_EDITOR
            BoxerLedgerCommand(0,_session);
#else
            string saved=PlayerPrefs.GetString(StorageKey,"");_ledger=saved==""?MatchLedger.Empty():MatchLedger.Parse(saved);
            if(_ledger==null)Error="Dữ liệu ví không hợp lệ; không tự xóa hay đặt lại số dư.";
#endif
        }
        public void BoutStarted()
        {
            if(_bootstrap.Flow.Screen!=ProductScreen.Fight)return;
            _fighterId=_bootstrap.Profile.Identity;_fighterName=_bootstrap.Profile.DisplayName;_nationality=_bootstrap.Profile.Nationality;
            _started=true;
        }
        public void Completed(CoachMatchReview review)
        {
            if(!_started||_bootstrap.Flow.Screen!=ProductScreen.Result||review==null||!ReferenceEquals(review,_bootstrap.LastMatchReview))return;
            _started=false;
            var entry=new MatchHistoryEntry {review=CoachMatchReview.Parse(review.Serialize()),profileId=_fighterId,fighterName=_fighterName,
                nationality=_nationality,completedUtc=DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ",System.Globalization.CultureInfo.InvariantCulture),reward=MatchLedger.Reward(review.result)};
            if(!entry.Valid)return;
            LatestId=review.id;_pending.Add(review.id,entry);Submit(entry);
        }
        public void Retry() {foreach(var e in new List<MatchHistoryEntry>(_pending.Values))Submit(e);}
        private void Submit(MatchHistoryEntry entry)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            BoxerLedgerCommand(1,JsonUtility.ToJson(new Submission {session=_session,entry=entry}));
#else
            if(_ledger==null)return;
            var next=_ledger.Append(entry);
            if(next==null){Error="Không lưu được: trùng ID khác nội dung hoặc đã đạt giới hạn 1000 trận.";return;}
            try {PlayerPrefs.SetString(StorageKey,next.Serialize());PlayerPrefs.Save();}
            catch(PlayerPrefsException){Error="Không lưu được trên thiết bị; thưởng chưa được xác nhận.";return;}
            _ledger=next;_pending.Remove(entry.review.id);Error="";
#endif
            Publish();
        }
        public void Receive(string payload)
        {
            if(string.IsNullOrEmpty(payload)||payload.Length>1501000)return;
            Envelope state;try{state=JsonUtility.FromJson<Envelope>(payload);}catch(ArgumentException){return;}
            if(state==null||state.session!=_session)return;
            if(!string.IsNullOrEmpty(state.error)){Error=state.error;Publish();return;}
            if(state.ledger==null||!state.ledger.Valid)return;
            if(!string.IsNullOrEmpty(state.id))
            {
                if(!_pending.TryGetValue(state.id,out var entry))return;
                if(state.ledger.Find(state.id)?.Serialize()!=entry.Serialize())return;
                _pending.Remove(state.id);
            }
            if(_ledger==null||state.ledger.entries.Length>=_ledger.entries.Length)_ledger=state.ledger;
            Error="";Publish();
        }
        public void Publish()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            bool open=_bootstrap.Flow.Screen==ProductScreen.History;
            if(open&&!_historyOpen){_historyKeyboard=WebGLInput.captureAllKeyboardInput;WebGLInput.captureAllKeyboardInput=false;}
            else if(!open&&_historyOpen)WebGLInput.captureAllKeyboardInput=_historyKeyboard;
            _historyOpen=open;
            BoxerLedgerCommand(2,JsonUtility.ToJson(new View {open=_bootstrap.Flow.Screen==ProductScreen.History,status=RewardStatus,error=Error,pending=PendingCount,ledger=_ledger}));
#endif
        }
        public void Refresh()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            BoxerLedgerCommand(3,_session);
#endif
            Publish();
        }
        private void OnDisable()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if(_historyOpen)WebGLInput.captureAllKeyboardInput=_historyKeyboard;
            BoxerLedgerCommand(2,"{\"open\":false}");
#endif
        }
    }
}
