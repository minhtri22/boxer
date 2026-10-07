using UnityEngine;

namespace BoxerP0
{
    public sealed class ProductScreens : MonoBehaviour
    {
        private BoxerBootstrap _bootstrap;
        private Phase0Telemetry _telemetry;
        private GUIStyle _title, _copy, _small, _button;
        private Texture2D _brand;
        private static readonly Color Gold = new(.93f, .67f, .24f);
        private void Start()
        { _bootstrap = GetComponent<BoxerBootstrap>(); _telemetry = FindFirstObjectByType<Phase0Telemetry>(); _brand=Resources.Load<Texture2D>("Product/HomeBrand"); }
        private void OnGUI()
        {
            if (_bootstrap == null || _telemetry == null || _bootstrap.Flow.Gameplay) return;
            Styles();
            Matrix4x4 prior = GUI.matrix; Color color = GUI.color; int depth = GUI.depth;
            GUI.depth = -200;
            try
            {
                GUI.color = new Color(.01f,.012f,.014f,.82f);
                GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height), Texture2D.whiteTexture);
                GUI.color = Color.white;
                Rect safe = Screen.safeArea;
                float scale = Mathf.Min(safe.width / 540, safe.height / 960);
                GUI.matrix = Matrix4x4.TRS(new Vector3(safe.x + (safe.width-540*scale)/2,
                    Screen.height-safe.yMax+(safe.height-960*scale)/2,0),Quaternion.identity,new Vector3(scale,scale,1));
                if (_brand!=null) GUI.DrawTextureWithTexCoords(new Rect(38,35,464,156),_brand,new Rect(.05f,.68f,.90f,.17f));
                else { GUI.Label(new Rect(30,45,480,90), "BOXER", _title); GUI.Label(new Rect(30,135,480,35), "FIGHT FROM YOUR OWN EYES", _small); }
                switch (_bootstrap.Flow.Screen)
                {
                    case ProductScreen.Home: Home(); break;
                    case ProductScreen.Preview: Preview(); break;
                    case ProductScreen.Result: Result(); break;
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
        private bool Button(float y, string value, bool primary=false)
        {
            GUI.color=Gold; GUI.DrawTexture(new Rect(42,y,456,68),Texture2D.whiteTexture);
            GUI.color=primary?Gold:new Color(.025f,.022f,.019f);
            GUI.DrawTexture(new Rect(44,y+2,452,64),Texture2D.whiteTexture); GUI.color=Color.white;
            _button.normal.textColor=_button.hover.textColor=primary?new Color(.025f,.022f,.019f):Gold;
            return GUI.Button(new Rect(42,y,456,68),value,_button);
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
            ControlCard(42,420,"RIGHT THUMB = PUNCH\n\nTap, or hold + swipe\nfor punch families.");
            ControlCard(278,420,"NO TOUCH = GUARD\n\nRecover, then choose\nyour next attack.");
            if (Button(640,"START",true)) _bootstrap.ShowPreview();
            if (Button(725,"PRACTICE CONTROLS")) _bootstrap.BeginProductBout(true);
            GUI.Label(new Rect(42,815,456,65),"CAREER / FULL GYM — NOT AVAILABLE IN WAVE 1",_small);
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
        private void Result()
        {
            var bout = _telemetry.Bout;
            string verdict = bout.Result=="PLAYER_WIN" ? "VICTORY" : bout.Result=="OPPONENT_WIN" ? "DEFEAT" : "DRAW";
            Text(235,verdict,70); Text(315,bout.EndReason=="KO" ? "HP DEPLETED" : "POINTS — REMAINING HP",50);
            Text(405,$"BOXER     HP {bout.Player.HP:F1} / 100\nSTAMINA {bout.Player.Stamina:F1} / 100",90);
            Text(515,$"RAMIREZ   HP {bout.Opponent.HP:F1} / 100\nSTAMINA {bout.Opponent.Stamina:F1} / 100",90);
            Text(615,$"HITS {_telemetry.PlayerHits} / {_telemetry.OpponentHits}   ·   BLOCKS {_telemetry.PlayerBlocks} / {_telemetry.OpponentBlocks}",60);
            if (Button(745,"REMATCH",true)) _bootstrap.BeginProductBout();
            if (Button(835,"HOME")) _bootstrap.ReturnHome();
        }
    }
}
