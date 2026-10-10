using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BoxerP0
{
    public sealed class FighterProfileController : MonoBehaviour
    {
        public const string Preference = "BOXER_FIGHTER_PROFILE_V1";
        private BoxerBootstrap _bootstrap;
        private FighterProfile _saved;
        private string _token;
        private bool _priorKeyboard;
        public string DisplayName => _saved?.name ?? "BOXER";
        public string Nationality => _saved?.nationality ?? string.Empty;
        public string Identity => _saved?.id ?? string.Empty;
        public bool HasSaved => _saved != null;
        public bool InvalidStored { get; private set; }
        public string DraftName = "", DraftNationality = "";
        public string Error { get; private set; } = "";
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void BoxerProfilePublish(string payload);
#endif
        [Serializable] private sealed class View
        { public bool open, hasReview, invalidStored; public string token, name, nationality, id, error; }
        [Serializable] private sealed class Request
        { public string token, action, name, nationality; }
        public void Initialize(BoxerBootstrap bootstrap)
        {
            _bootstrap = bootstrap;
            string stored = PlayerPrefs.GetString(Preference, "");
            _saved = FighterProfile.Parse(stored);
            InvalidStored = stored.Length > 0 && _saved == null;
        }
        public void Open()
        {
            if (_bootstrap.Flow.Screen != ProductScreen.Profile) return;
            Close(); _token = Guid.NewGuid().ToString("N");
            DraftName = _saved?.name ?? ""; DraftNationality = _saved?.nationality ?? ""; Error = "";
#if UNITY_WEBGL && !UNITY_EDITOR
            _priorKeyboard = WebGLInput.captureAllKeyboardInput;
            WebGLInput.captureAllKeyboardInput = false;
#endif
            Publish();
        }
        private void Update() { if (_token != null && _bootstrap.Flow.Screen != ProductScreen.Profile) Close(); }
        private void OnDisable() { Close(); }
        public void Close()
        {
            if (_token == null) return;
            _token = null; DraftName = DraftNationality = ""; Error = "";
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = _priorKeyboard;
#endif
            Publish();
        }
        public void Receive(string payload)
        {
            if (_token == null || _bootstrap.Flow.Screen != ProductScreen.Profile || string.IsNullOrEmpty(payload) || payload.Length > 2048) return;
            Request request;
            try { request = JsonUtility.FromJson<Request>(payload); } catch (ArgumentException) { return; }
            if (request == null || request.token != _token) return;
            if (request.action == "save") { DraftName = request.name; DraftNationality = request.nationality; Save(); }
            else if (request.action == "cancel") Cancel();
            else if (request.action == "review") Review();
        }
        public bool Save()
        {
            if (_token == null || _bootstrap.Flow.Screen != ProductScreen.Profile) return false;
            var candidate = FighterProfile.Candidate(_saved, DraftName, DraftNationality);
            if (candidate == null)
            { Error = "Tên: 1–24 ký tự. Quốc tịch: 1–40 ký tự. Dùng chữ, số, dấu cách, gạch nối hoặc dấu nháy."; Publish(); return false; }
            try { PlayerPrefs.SetString(Preference, candidate.Serialize()); PlayerPrefs.Save(); }
            catch (PlayerPrefsException) { Error = "Không lưu được trên thiết bị. Hãy thử lại; hồ sơ chưa được xác nhận."; Publish(); return false; }
            _saved = candidate; InvalidStored = false; Close(); _bootstrap.ReturnHome(); return true;
        }
        public void Cancel() { if (_token == null || _bootstrap.Flow.Screen != ProductScreen.Profile) return; Close(); _bootstrap.ReturnHome(); }
        public void Review()
        {
            if (_token == null || _bootstrap.Flow.Screen != ProductScreen.Profile || _bootstrap.LastMatchReview == null) return;
            Close(); if (_bootstrap.ShowCoach()) _bootstrap.ShowMatchReview();
        }
        private void Publish()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            BoxerProfilePublish(JsonUtility.ToJson(new View { open = _token != null, token = _token,
                name = DraftName, nationality = DraftNationality, id = Identity, error = Error,
                hasReview = _bootstrap != null && _bootstrap.LastMatchReview != null, invalidStored = InvalidStored }));
#endif
        }
    }
}
