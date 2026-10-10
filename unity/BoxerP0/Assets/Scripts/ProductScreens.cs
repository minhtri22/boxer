using UnityEngine;

namespace BoxerP0
{
    public sealed class ProductScreens : MonoBehaviour
    {
        private BoxerBootstrap _bootstrap;
        private Phase0Telemetry _telemetry;
        private GUIStyle _title, _copy, _small, _button;
        private Texture2D _brand;
        private Texture2D _coachPortrait;
        public bool CoachArtLoaded => _coachPortrait != null;
        public float CoachInfoContentHeight { get; private set; }
        public float CoachReviewContentHeight { get; private set; }
        private TrainingGlassBackground _glass;
        private static readonly Color Gold = new(.93f, .67f, .24f);
        private void Start()
        { _bootstrap = GetComponent<BoxerBootstrap>(); _telemetry = FindFirstObjectByType<Phase0Telemetry>(); _brand=Resources.Load<Texture2D>("Product/HomeBrand"); _coachPortrait=Resources.Load<Texture2D>("Product/CoachReference"); }
        private void Update()
        {
            if (_glass == null)
            {
                var camera = FindFirstObjectByType<Camera>();
                if (camera != null) _glass = camera.gameObject.AddComponent<TrainingGlassBackground>();
            }
            if (_glass != null) _glass.enabled = _bootstrap != null && _bootstrap.Flow.Screen == ProductScreen.Onboarding;
        }
        private void OnGUI()
        {
            if (_bootstrap == null || _telemetry == null || _bootstrap.Flow.Screen == ProductScreen.Fight) return;
            Styles();
            Matrix4x4 prior = GUI.matrix; Color color = GUI.color; int depth = GUI.depth;
            GUI.depth = -200;
            try
            {
                bool practice = _bootstrap.Flow.Screen == ProductScreen.Onboarding;
                if (!practice)
                {
                    GUI.color = new Color(.01f,.012f,.014f,.82f);
                    GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height), Texture2D.whiteTexture);
                }
                GUI.color = Color.white;
                Rect safe = Screen.safeArea;
                float scale = Mathf.Min(safe.width / 540, safe.height / 960);
                GUI.matrix = Matrix4x4.TRS(new Vector3(safe.x + (safe.width-540*scale)/2,
                    Screen.height-safe.yMax+(safe.height-960*scale)/2,0),Quaternion.identity,new Vector3(scale,scale,1));
                if (!practice)
                {
                    if (_brand!=null) GUI.DrawTextureWithTexCoords(new Rect(38,35,464,156),_brand,new Rect(.05f,.68f,.90f,.17f));
                    else { GUI.Label(new Rect(30,45,480,90), "BOXER", _title); GUI.Label(new Rect(30,135,480,35), "FIGHT FROM YOUR OWN EYES", _small); }
                }
                switch (_bootstrap.Flow.Screen)
                {
                    case ProductScreen.Home: Home(); break;
                    case ProductScreen.Preview: Preview(); break;
                    case ProductScreen.Result: Result(); break;
                    case ProductScreen.Onboarding: Training(); break;
                    case ProductScreen.Intro: Text(360,"ROUND 1 — RING INTRO",100); break;
                    case ProductScreen.Coach: Coach(); break;
                    case ProductScreen.TrainingInfo: CoachingInfo(); break;
                    case ProductScreen.CoachReview: MatchReview(); break;
                }
            }
            finally { GUI.matrix = prior; GUI.color = color; GUI.depth = depth; }
        }
        private void Styles()
        {
            if (_copy != null) return;
            Font font = Resources.Load<Font>("Fonts/Oswald");
            _copy = new GUIStyle(GUI.skin.label) {font=font,fontSize=24,wordWrap=true,alignment=TextAnchor.MiddleCenter};
            _copy.normal.textColor = new Color(.95f,.91f,.82f);
            _small = new GUIStyle(_copy) { fontSize=20 };
            _title = new GUIStyle(_copy) { fontSize=72,fontStyle=FontStyle.Bold }; _title.normal.textColor=Gold;
            _button = new GUIStyle(GUI.skin.button) { font=font,fontSize=28,fontStyle=FontStyle.Bold,
                padding=new RectOffset(12,12,8,8) };
            _button.normal.textColor=Gold; _button.hover.textColor=Gold; _button.active.textColor=Color.white;
            _button.normal.background=_button.hover.background=_button.active.background=null;
        }
        private void Text(float y, string value, float height = 70)
        { GUI.Label(new Rect(42,y,456,height),value,_copy); }
        private bool Button(float y, string value, bool primary=false, float height=68)
        {
            GUI.color=Gold; GUI.DrawTexture(new Rect(42,y,456,height),Texture2D.whiteTexture);
            GUI.color=primary?Gold:new Color(.025f,.022f,.019f);
            GUI.DrawTexture(new Rect(44,y+2,452,height-4),Texture2D.whiteTexture); GUI.color=Color.white;
            _button.normal.textColor=_button.hover.textColor=primary?new Color(.025f,.022f,.019f):Gold;
            return GUI.Button(new Rect(42,y,456,height),value,_button);
        }
        private void ControlCard(float x,float y,string value)
        {
            GUI.color=Gold;GUI.DrawTexture(new Rect(x,y,220,153),Texture2D.whiteTexture);
            GUI.color=new Color(.025f,.022f,.019f);GUI.DrawTexture(new Rect(x+1,y+1,218,151),Texture2D.whiteTexture);GUI.color=Color.white;
            GUI.Label(new Rect(x+8,y+6,204,141),value,_small);
        }
        private void Home()
        {
            ControlCard(42,245,"PHONE = HEAD\n\nLook, react,\nstay aware.");
            ControlCard(278,245,"LEFT THUMB = FEET\n\nMove, angle,\ncontrol.");
            ControlCard(42,420,"RIGHT THUMB = PUNCH\n\nTap, or quick swipe.\nRelease to punch.");
            ControlCard(278,420,"NO TOUCH = GUARD\n\nRecover, then choose\nyour next attack.");
            if (Button(640,"START",true)) _bootstrap.ShowPreview();
            if (Button(725,"TRAINING / COACH",false,76)) _bootstrap.ShowCoach();
            GUI.Label(new Rect(42,820,456,70),"TẬP ĐIỀU KHIỂN & PHÂN TÍCH TRẬN\nTẤT CẢ TRONG COACH",_small);
        }
        private void Training()
        {
            bool head=_bootstrap.TrainingToken=="HEADCONTROL";
            float offset=head?140:0;
            if (_glass != null) _glass.Draw(new Rect(20,20+offset,500,425));
            GUI.color=Color.white;
            GUI.Label(new Rect(35,25+offset,470,45),"TẬP ĐIỀU KHIỂN — KHÔNG TÍNH ĐIỂM",_small);
            GUI.Label(new Rect(35,72+offset,470,245),_bootstrap.TrainingInstructions,_small);
            bool enabled=GUI.enabled; GUI.enabled=_bootstrap.TrainingReady;
            if (Button(320+offset,"HOÀN THÀNH TẬP",true)) _bootstrap.AdvanceTraining();
            GUI.enabled=enabled;
            if (Button(395+offset,"THOÁT TẬP / COACH")) _bootstrap.ExitTraining();
            if (head)
            {
                if (_glass != null) _glass.Draw(new Rect(20,16,500,122));
                GUI.color=Color.white;
                if (Button(630,"ĐẶT LẠI TƯ THẾ ĐẦU")) _bootstrap.RecenterTrainingHead();
            }
            if (_bootstrap.TrainingToken=="FOOTWORK" || _bootstrap.TrainingToken=="PUNCHES")
            {
                bool feet=_bootstrap.TrainingToken=="FOOTWORK";
                float x=feet?20:285;
                GUI.color=new Color(Gold.r,Gold.g,Gold.b,.14f);
                GUI.DrawTexture(new Rect(x,610,235,285),Texture2D.whiteTexture);GUI.color=Color.white;
                GUI.Label(new Rect(x+8,617,219,55),feet?"VÙNG DI CHUYỂN\nGIỮ + VUỐT 4 HƯỚNG":"VÙNG ĐẤM\nCHẠM / VUỐT DỨT KHOÁT",_small);
            }
            // Drawn only in training, using the same safe-area/portrait coordinate space.
            TrainingGestureGuide.Draw(_bootstrap.TrainingToken,Time.unscaledTime,_small);
        }
        private void Preview()
        {
            Text(220,"YOUR OPPONENT",40); Text(270,"RAMIREZ",70);
            Text(360,"BALANCED\nFinite reach · committed attacks · no homing",100);
            Text(485,"ONE 45-SECOND BOUT\nHP zero ends the bout. Otherwise remaining HP decides.",100);
            Text(600,"STAMINA = ENDURANCE\nThin gold strip = burst capacity.\nLow energy reduces impact and slows recovery.",110);
            if (Button(745,"ENTER RING",true)) _bootstrap.BeginProductBout();
            if (Button(835,"BACK")) _bootstrap.ReturnHome();
        }
        private void Coach()
        {
            if (_coachPortrait != null)
                GUI.DrawTextureWithTexCoords(new Rect(0,200,250,467),_coachPortrait,new Rect(0,.366f,.48f,.504f));
            GUI.color=Gold;
            GUI.Label(new Rect(255,192,270,32),"COACH · TRAINING",_copy); GUI.color=Color.white;
            for (int i=0;i<CoachCatalog.Count;i++)
            {
                var module=(CoachModule)i; var lesson=CoachCatalog.Get(module);
                Rect card=new Rect(260,230+i*90,264,88);
                GUI.color=Gold; GUI.DrawTexture(card,Texture2D.whiteTexture);
                GUI.color=module==_bootstrap.SelectedCoachModule ? new Color(.16f,.115f,.055f) : new Color(.02f,.021f,.024f);
                GUI.DrawTexture(new Rect(card.x+2,card.y+2,card.width-4,card.height-4),Texture2D.whiteTexture); GUI.color=Color.white;
                string completed=(_bootstrap.CoachCompletionMask & CoachCatalog.CompletionBit(module))!=0 ? " · ĐÃ TẬP" : "";
                GUI.Label(new Rect(card.x+10,card.y+5,card.width-20,29),lesson.Title+completed,_small);
                GUI.Label(new Rect(card.x+10,card.y+34,card.width-20,50),lesson.Summary,_small);
                if (GUI.Button(card,GUIContent.none,_button)) _bootstrap.SelectCoachModule(module);
            }
            if (Button(690,"TRẬN TRƯỚC → TẬP BỔ SUNG",false,76)) _bootstrap.ShowMatchReview();
            bool interactive=CoachCatalog.Get(_bootstrap.SelectedCoachModule).Interactive;
            if (Button(770,interactive ? "TRAIN NOW · TẬP NGAY" : "XEM HƯỚNG DẪN",true,76)) _bootstrap.OpenSelectedCoachModule();
            if (Button(855,"HOME",false,76)) _bootstrap.ReturnHome();
            GUI.Label(new Rect(24,934,492,25),"KỸ NĂNG THẬT · KHÔNG NÂNG CHỈ SỐ",_small);
        }
        private void MatchReview()
        {
            GUI.color=Gold; Text(215,"TRẬN TRƯỚC → TẬP BỔ SUNG",55); GUI.color=Color.white;
            var review = _bootstrap.LastMatchReview;
            if (review == null)
            {
                CoachReviewContentHeight = 0;
                Text(340,"CHƯA CÓ DỮ LIỆU TRẬN\n\nHoàn tất một trận trong bản mới để Coach phân tích. Bài tập không tạo số liệu trận đấu.",190);
                Text(560,"Bạn vẫn có thể chọn các bài tập trong Coach.",90);
            }
            else
            {
                string verdict = review.result=="PLAYER_WIN" ? "THẮNG" : review.result=="OPPONENT_WIN" ? "THUA" : "HÒA";
                Text(273,$"RAMIREZ · {verdict} · {review.reason} · {review.seconds:F1}s",40);
                CoachReviewContentHeight = _small.CalcHeight(new GUIContent(review.StatsText),456);
                GUI.Label(new Rect(42,318,456,200),review.StatsText,_small);
                GUI.Label(new Rect(42,518,456,30),"GỢI Ý TẬP · KHÔNG CHẤM ĐIỂM / NÂNG CHỈ SỐ",_small);
                var suggestions = review.Suggestions();
                for (int i=0;i<suggestions.Length;i++)
                {
                    var module = suggestions[i]; var lesson = CoachCatalog.Get(module);
                    var card = new Rect(42,550+i*94,456,88);
                    GUI.color=Gold; GUI.DrawTexture(card,Texture2D.whiteTexture);
                    GUI.color=new Color(.04f,.034f,.023f); GUI.DrawTexture(new Rect(card.x+2,card.y+2,card.width-4,card.height-4),Texture2D.whiteTexture); GUI.color=Color.white;
                    GUI.Label(new Rect(54,card.y+4,432,32),(lesson.Interactive ? "TẬP · " : "ĐỌC · ")+lesson.Title,_copy);
                    GUI.Label(new Rect(54,card.y+36,432,47),review.SuggestionReason(module),_small);
                    if (GUI.Button(card,GUIContent.none,_button)) _bootstrap.TrainFromReview(module);
                }
            }
            if (Button(855,"BACK / COACH",false,76)) _bootstrap.BackToCoach();
        }
        private void CoachingInfo()
        {
            var lesson=CoachCatalog.Get(_bootstrap.SelectedCoachModule);
            GUI.color=Gold; Text(230,lesson.Title,55); GUI.color=Color.white;
            Text(290,"HƯỚNG DẪN · KHÔNG NÂNG CHỈ SỐ",50);
            CoachInfoContentHeight=_small.CalcHeight(new GUIContent(lesson.Instructions),456);
            GUI.Label(new Rect(42,360,456,320),lesson.Instructions,_small);
            if (Button(745,"THỬ TRONG BÀI TẬP ĐẤM",true,76)) _bootstrap.PracticeFromCoachInfo();
            if (Button(835,"BACK / COACH",false,76)) _bootstrap.BackToCoach();
        }
        private void Result()
        {
            var bout = _telemetry.Bout;
            string verdict = bout.Result=="PLAYER_WIN" ? "VICTORY" : bout.Result=="OPPONENT_WIN" ? "DEFEAT" : "DRAW";
            Text(235,verdict,70); Text(315,bout.EndReason=="KO" ? "HP DEPLETED" : "POINTS — REMAINING HP",50);
            Text(405,$"BOXER     HP {bout.Player.HP:F1} / 100\nSTAMINA {bout.Player.Stamina:F1} / 100",90);
            Text(515,$"RAMIREZ   HP {bout.Opponent.HP:F1} / 100\nSTAMINA {bout.Opponent.Stamina:F1} / 100",90);
            Text(615,$"HITS {_telemetry.PlayerHits} / {_telemetry.OpponentHits}   ·   BLOCKS {_telemetry.PlayerBlocks} / {_telemetry.OpponentBlocks}",35);
            if (Button(655,"PHÂN TÍCH / TẬP BỔ SUNG",false,76)) _bootstrap.ShowMatchReview();
            if (Button(745,"REMATCH",true)) _bootstrap.BeginProductBout();
            if (Button(835,"HOME")) _bootstrap.ReturnHome();
        }
    }
}
