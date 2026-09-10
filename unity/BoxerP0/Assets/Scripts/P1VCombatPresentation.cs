using System;
using UnityEngine;

namespace BoxerP0
{
    public static class P1VPresentationMath
    {
        public static float OpponentHeightFraction(float cameraDistanceMeters)
        {
            return Mathf.Clamp(1.12f / Mathf.Max(0.7f, cameraDistanceMeters), 0.50f, 0.88f);
        }

        public static string PoseToken(PunchIntent intent)
        {
            if (intent == PunchIntent.None) return "GUARD";
            return PunchLabels.Family(intent) == PunchFamily.Hook ? "HOOK" : "STRAIGHT";
        }

        public static bool ShouldMirrorAttack(PunchIntent intent)
        {
            return intent != PunchIntent.None && !PunchLabels.IsRearHand(intent);
        }

        public static Vector2 MovementControlCenter(float width, float height) => new(width * 0.15f, height * 0.875f);
        public static Vector2 GuardControlCenter(float width, float height) => new(width * 0.50f, height * 0.905f);
        public static Vector2 PunchControlCenter(float width, float height) => new(width * 0.85f, height * 0.875f);
    }

    /// <summary>
    /// P1-V presentation-only combat surface. It renders approved art over the verified
    /// simulation; combat geometry, input semantics and authoritative transforms remain unchanged.
    /// </summary>
    [DefaultExecutionOrder(200)]
    public sealed class P1VCombatPresentation : MonoBehaviour
    {
        private const float TestBoutSeconds = 45f;
        private const float HpLossPerHit = 0.08f;
        private const float PlayerStaminaCost = 0.12f;
        private const float OpponentStaminaCost = 0.10f;

        private static readonly Color Gold = new(0.93f, 0.67f, 0.24f, 1f);
        private static readonly Color WarmWhite = new(0.95f, 0.91f, 0.82f, 1f);
        private static readonly Color Red = new(0.76f, 0.075f, 0.055f, 1f);
        private static readonly Color Amber = new(0.95f, 0.62f, 0.08f, 1f);
        private static readonly Color Ink = new(0.025f, 0.022f, 0.021f, 1f);

        private Phase0Telemetry _telemetry;
        private BoxerInput _input;
        private PlayerBoxer _player;
        private OpponentBoxer _opponent;
        private BoxerBootstrap _bootstrap;
        private Camera _camera;

        private Texture2D _arena;
        private Texture2D _ramirezGuard;
        private Texture2D _ramirezStraight;
        private Texture2D _ramirezHook;
        private Texture2D _playerGlove;
        private Texture2D _circle;
        private Font _font;
        private Material _chromaMaterial;

        private GUIStyle _label;
        private GUIStyle _box;
        private string _trainingStage = "CALIBRATE";
        private string _lastTelemetryEvent = string.Empty;
        private string _lastBoutResult = "PENDING";
        private float _observedBoutStart = -1f;
        private float _playerStamina = 1f;
        private float _opponentStamina = 1f;
        private uint _lastPlayerPunchCount;
        private uint _lastOpponentAttackCount;

        public bool IsReady { get; private set; }

        private void Start()
        {
            _telemetry = FindFirstObjectByType<Phase0Telemetry>();
            _input = FindFirstObjectByType<BoxerInput>();
            _player = FindFirstObjectByType<PlayerBoxer>();
            _opponent = FindFirstObjectByType<OpponentBoxer>();
            _bootstrap = FindFirstObjectByType<BoxerBootstrap>();
            _camera = Camera.main ?? FindFirstObjectByType<Camera>();

            _arena = Resources.Load<Texture2D>("P1V/championship-arena");
            _ramirezGuard = Resources.Load<Texture2D>("P1V/ramirez-guard-chroma");
            _ramirezStraight = Resources.Load<Texture2D>("P1V/ramirez-straight-chroma");
            _ramirezHook = Resources.Load<Texture2D>("P1V/ramirez-hook-chroma");
            _playerGlove = Resources.Load<Texture2D>("P1V/player-glove-left-chroma");
            _font = Resources.Load<Font>("Fonts/Oswald");
            Shader shader = Resources.Load<Shader>("BoxerP1VChromaKey");
            if (shader != null)
            {
                _chromaMaterial = new Material(shader);
                _chromaMaterial.SetFloat("_Threshold", 0.20f);
                _chromaMaterial.SetFloat("_Softness", 0.22f);
            }

            _circle = BuildCircleTexture(128);
            if (_input != null) _lastPlayerPunchCount = _input.PunchEventCount;
            if (_opponent != null) _lastOpponentAttackCount = _opponent.AttackEventCount;
            IsReady = _arena != null && _ramirezGuard != null && _ramirezStraight != null &&
                      _ramirezHook != null && _playerGlove != null && _chromaMaterial != null;
        }

        private void OnDestroy()
        {
            if (_chromaMaterial != null) Destroy(_chromaMaterial);
            if (_circle != null) Destroy(_circle);
        }

        private void Update()
        {
            if (!IsReady || _telemetry == null) return;
            TrackTrainingStage();
            TrackBoutTransition();
            UpdateStamina();
        }

        private void TrackTrainingStage()
        {
            string value = _telemetry.LastEvent ?? string.Empty;
            if (value == _lastTelemetryEvent) return;
            _lastTelemetryEvent = value;
            const string prefix = "TRAINING_STAGE_START_";
            if (value.StartsWith(prefix, StringComparison.Ordinal)) _trainingStage = value.Substring(prefix.Length);
            else if (value == "TRAINING_COMPLETE" || value == "BOUT_START") _trainingStage = string.Empty;
        }

        private void TrackBoutTransition()
        {
            string current = _telemetry.BoutResult ?? "PENDING";
            if (current == _lastBoutResult) return;
            if (current == "IN_PROGRESS")
            {
                _observedBoutStart = Time.unscaledTime;
                _playerStamina = 1f;
                _opponentStamina = 1f;
            }
            _lastBoutResult = current;
        }

        private void UpdateStamina()
        {
            if (_input != null)
            {
                uint count = _input.PunchEventCount;
                uint delta = count - _lastPlayerPunchCount;
                if (delta > 0) _playerStamina = Mathf.Clamp01(_playerStamina - delta * PlayerStaminaCost);
                _lastPlayerPunchCount = count;
            }
            if (_opponent != null)
            {
                uint count = _opponent.AttackEventCount;
                uint delta = count - _lastOpponentAttackCount;
                if (delta > 0) _opponentStamina = Mathf.Clamp01(_opponentStamina - delta * OpponentStaminaCost);
                _lastOpponentAttackCount = count;
            }
            _playerStamina = Mathf.MoveTowards(_playerStamina, 1f, 0.22f * Time.unscaledDeltaTime);
            _opponentStamina = Mathf.MoveTowards(_opponentStamina, 1f, 0.18f * Time.unscaledDeltaTime);
        }

        private void OnGUI()
        {
            if (!IsReady) return;
            EnsureStyles();

            int oldDepth = GUI.depth;
            GUI.depth = 1000;
            DrawArena();
            GUI.depth = 900;
            DrawOpponent();
            GUI.depth = 800;
            DrawPlayerGloves();
            GUI.depth = -50;
            DrawHud();
            DrawControls();
            if (_bootstrap != null && _bootstrap.ShowDeveloperDiagnostics) DrawDiagnostics();
            if (ShouldShowTraining()) DrawTrainingCard();
            if (IsBoutComplete()) DrawResultCard();
            GUI.depth = oldDepth;
        }

        private void EnsureStyles()
        {
            if (_label != null) return;
            _label = new GUIStyle(GUI.skin.label)
            {
                clipping = TextClipping.Clip,
                richText = false,
                wordWrap = false,
                font = _font
            };
            _box = new GUIStyle(GUI.skin.box) { normal = { background = Texture2D.whiteTexture } };
        }

        private void DrawArena()
        {
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _arena, ScaleMode.ScaleAndCrop, false);
            DrawSolid(new Rect(0f, 0f, Screen.width, Screen.height * 0.14f), new Color(0f, 0f, 0f, 0.38f));
            DrawSolid(new Rect(0f, Screen.height * 0.70f, Screen.width, Screen.height * 0.30f), new Color(0f, 0f, 0f, 0.20f));
        }

        private void DrawOpponent()
        {
            if (_opponent == null || _camera == null || Event.current.type != EventType.Repaint) return;
            Vector3 centerWorld = _opponent.transform.position + Vector3.up * 0.90f;
            Vector3 projected = _camera.WorldToScreenPoint(centerWorld);
            if (projected.z <= 0.01f) return;

            float distance = Vector3.Distance(_camera.transform.position, centerWorld);
            float height = Screen.height * P1VPresentationMath.OpponentHeightFraction(distance);
            float width = height * (2f / 3f);
            float centerX = Mathf.Clamp(projected.x, Screen.width * 0.28f, Screen.width * 0.72f);
            float centerY = Mathf.Clamp(Screen.height - projected.y, Screen.height * 0.43f, Screen.height * 0.57f);

            Texture2D pose = _ramirezGuard;
            float phase = 0f;
            bool mirror = false;
            if (_opponent.IsActionBusy)
            {
                phase = Mathf.Clamp01(_opponent.ActionNormalizedPhase(_opponent.CurrentActionPhaseDuration));
                pose = P1VPresentationMath.PoseToken(_opponent.CurrentIntent) == "HOOK" ? _ramirezHook : _ramirezStraight;
                mirror = P1VPresentationMath.ShouldMirrorAttack(_opponent.CurrentIntent);
                float commitment = _opponent.CurrentPhase == ActionPhase.Commit ? phase * 0.35f :
                    _opponent.CurrentPhase == ActionPhase.Extend ? 0.35f + phase * 0.65f : 1f - phase;
                float scale = 1f + commitment * 0.09f;
                height *= scale;
                width *= scale;
                centerY += commitment * Screen.height * 0.018f;
            }

            Rect rect = new(centerX - width * 0.5f, centerY - height * 0.5f, width, height);
            if (_opponent.CurrentPhase == ActionPhase.Recover)
            {
                DrawKeyed(rect, pose, mirror, 1f - phase);
                DrawKeyed(rect, _ramirezGuard, false, phase);
            }
            else if (_opponent.CurrentPhase == ActionPhase.Commit)
            {
                DrawKeyed(rect, _ramirezGuard, false, 1f - phase * 0.55f);
                DrawKeyed(rect, pose, mirror, phase * 0.65f);
            }
            else DrawKeyed(rect, pose, mirror, 1f);
        }

        private void DrawPlayerGloves()
        {
            if (_playerGlove == null || Event.current.type != EventType.Repaint) return;
            float size = Mathf.Min(Screen.height * 0.35f, Screen.width * 0.42f);
            float aspect = (float)_playerGlove.width / _playerGlove.height;
            float gloveWidth = size * aspect;
            float gloveHeight = size;
            Vector2 left = ProjectGlove(_player != null ? _player.LeftGlove : null, new Vector2(Screen.width * 0.18f, Screen.height * 0.85f));
            Vector2 right = ProjectGlove(_player != null ? _player.RightGlove : null, new Vector2(Screen.width * 0.82f, Screen.height * 0.85f));

            Rect leftRect = new(left.x - gloveWidth * 0.50f, left.y - gloveHeight * 0.44f, gloveWidth, gloveHeight);
            Rect rightRect = new(right.x - gloveWidth * 0.50f, right.y - gloveHeight * 0.44f, gloveWidth, gloveHeight);
            DrawKeyed(leftRect, _playerGlove, false, 1f);
            DrawKeyed(rightRect, _playerGlove, true, 1f);
        }

        private Vector2 ProjectGlove(Transform glove, Vector2 fallback)
        {
            if (glove == null || _camera == null) return fallback;
            Vector3 p = _camera.WorldToScreenPoint(glove.position);
            if (p.z <= 0.01f) return fallback;
            return new Vector2(
                Mathf.Clamp(p.x, Screen.width * 0.12f, Screen.width * 0.88f),
                Mathf.Clamp(Screen.height - p.y, Screen.height * 0.70f, Screen.height * 0.91f));
        }

        private void DrawHud()
        {
            float margin = Mathf.Max(10f, Screen.width * 0.016f);
            float top = Mathf.Max(10f, Screen.height * 0.012f);
            float panelWidth = Screen.width * 0.32f;
            float panelHeight = Mathf.Clamp(Screen.height * 0.087f, 78f, 132f);
            Rect playerRect = new(margin, top, panelWidth, panelHeight);
            Rect opponentRect = new(Screen.width - margin - panelWidth, top, panelWidth, panelHeight);
            DrawFighterPanel(playerRect, "LV 12  BOXER", Mathf.Clamp01(1f - (_telemetry?.OpponentHits ?? 0) * HpLossPerHit), _playerStamina, false);
            DrawFighterPanel(opponentRect, "LV 15  RAMIREZ", Mathf.Clamp01(1f - (_telemetry?.PlayerHits ?? 0) * HpLossPerHit), _opponentStamina, true);

            float timerWidth = Screen.width * 0.23f;
            Rect timer = new((Screen.width - timerWidth) * 0.5f, top, timerWidth, panelHeight * 0.90f);
            DrawPanel(timer, new Color(0.02f, 0.018f, 0.016f, 0.88f), Gold, 2f);
            Label(new Rect(timer.x, timer.y + timer.height * 0.05f, timer.width, timer.height * 0.25f), "ROUND 1 / 10", Mathf.RoundToInt(Screen.height / 92f), TextAnchor.MiddleCenter, Gold, true);
            Label(new Rect(timer.x, timer.y + timer.height * 0.25f, timer.width, timer.height * 0.58f), TimerText(), Mathf.RoundToInt(Screen.height / 36f), TextAnchor.MiddleCenter, WarmWhite, true);

            if (_opponent != null && _opponent.CounterWindowOpen)
            {
                Rect badge = new(Screen.width * 0.76f, Screen.height * 0.31f, Screen.width * 0.20f, Screen.height * 0.065f);
                DrawPanel(badge, new Color(0.03f, 0.025f, 0.02f, 0.84f), Gold, 2f);
                Label(badge, "COUNTER READY", Mathf.RoundToInt(Screen.height / 67f), TextAnchor.MiddleCenter, Gold, true);
            }

            Label(new Rect(Screen.width * 0.34f, Screen.height * 0.125f, Screen.width * 0.32f, Screen.height * 0.055f),
                "BOXER", Mathf.RoundToInt(Screen.height / 31f), TextAnchor.MiddleCenter, Gold, true);
            Label(new Rect(Screen.width * 0.32f, Screen.height * 0.168f, Screen.width * 0.36f, Screen.height * 0.025f),
                "FIGHT FROM YOUR OWN EYES", Mathf.RoundToInt(Screen.height / 115f), TextAnchor.MiddleCenter, WarmWhite, true);
        }

        private void DrawFighterPanel(Rect rect, string title, float hp, float stamina, bool right)
        {
            DrawPanel(rect, new Color(0.02f, 0.018f, 0.016f, 0.84f), Gold, 2f);
            TextAnchor align = right ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
            float pad = rect.width * 0.05f;
            Label(new Rect(rect.x + pad, rect.y, rect.width - pad * 2f, rect.height * 0.30f), title,
                Mathf.RoundToInt(Screen.height / 65f), align, WarmWhite, true);
            DrawMeter(new Rect(rect.x + pad, rect.y + rect.height * 0.36f, rect.width - pad * 2f, rect.height * 0.18f), "HP", hp, Red, right);
            DrawMeter(new Rect(rect.x + pad, rect.y + rect.height * 0.65f, rect.width - pad * 2f, rect.height * 0.16f), "STAMINA", stamina, Amber, right);
            if (right && _opponent != null)
            {
                Label(new Rect(rect.x + pad, rect.y + rect.height * 0.82f, rect.width - pad * 2f, rect.height * 0.14f),
                    _opponent.AttributeProfileLabel, Mathf.RoundToInt(Screen.height / 105f), TextAnchor.MiddleRight, Gold, true);
            }
        }

        private void DrawMeter(Rect rect, string name, float fill, Color color, bool right)
        {
            float labelWidth = rect.width * 0.25f;
            Rect labelRect = right ? new Rect(rect.x + rect.width - labelWidth, rect.y, labelWidth, rect.height) : new Rect(rect.x, rect.y, labelWidth, rect.height);
            Rect bar = right ? new Rect(rect.x, rect.y, rect.width - labelWidth - 3f, rect.height) : new Rect(rect.x + labelWidth + 3f, rect.y, rect.width - labelWidth - 3f, rect.height);
            Label(labelRect, name, Mathf.RoundToInt(Screen.height / 93f), right ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft, WarmWhite, true);
            DrawSolid(bar, new Color(0.06f, 0.055f, 0.05f, 0.95f));
            Rect filled = bar;
            filled.width *= Mathf.Clamp01(fill);
            if (right) filled.x = bar.xMax - filled.width;
            DrawSolid(filled, color);
            DrawBorder(bar, new Color(Gold.r, Gold.g, Gold.b, 0.55f), 1f);
        }

        private void DrawControls()
        {
            float moveRadius = Mathf.Min(Screen.width * 0.095f, Screen.height * 0.083f);
            float punchRadius = moveRadius;
            Vector2 move = P1VPresentationMath.MovementControlCenter(Screen.width, Screen.height);
            Vector2 guard = P1VPresentationMath.GuardControlCenter(Screen.width, Screen.height);
            Vector2 punch = P1VPresentationMath.PunchControlCenter(Screen.width, Screen.height);

            DrawMovementControl(move, moveRadius);
            DrawControlDisc(guard, moveRadius * 0.68f, "GUARD", "◆");
            DrawControlDisc(punch, punchRadius, "PUNCH", "●");
            float miniRadius = punchRadius * 0.38f;
            DrawPunchButton(new Vector2(punch.x - punchRadius * 1.25f, punch.y - punchRadius * 1.28f), miniRadius, "TAP", "STRAIGHT");
            DrawPunchButton(new Vector2(punch.x, punch.y - punchRadius * 1.78f), miniRadius, "↑", "UPPERCUT");
            DrawPunchButton(new Vector2(Mathf.Min(Screen.width - miniRadius - 8f, punch.x + punchRadius * 1.10f), punch.y - punchRadius * 1.28f), miniRadius, "↔", "HOOK");
            DrawPunchButton(new Vector2(punch.x, punch.y + punchRadius * 1.30f), miniRadius, "↓", "OVERHAND");
        }

        private void DrawMovementControl(Vector2 center, float radius)
        {
            DrawControlDisc(center, radius, "MOVE", "●");
            int arrows = Mathf.RoundToInt(Screen.height / 57f);
            Label(new Rect(center.x - radius * 0.25f, center.y - radius * 0.80f, radius * 0.50f, radius * 0.40f), "▲", arrows, TextAnchor.MiddleCenter, Gold, true);
            Label(new Rect(center.x - radius * 0.80f, center.y - radius * 0.25f, radius * 0.45f, radius * 0.50f), "◀", arrows, TextAnchor.MiddleCenter, Gold, true);
            Label(new Rect(center.x + radius * 0.35f, center.y - radius * 0.25f, radius * 0.45f, radius * 0.50f), "▶", arrows, TextAnchor.MiddleCenter, Gold, true);
            Label(new Rect(center.x - radius * 0.25f, center.y + radius * 0.38f, radius * 0.50f, radius * 0.40f), "▼", arrows, TextAnchor.MiddleCenter, Gold, true);
        }

        private void DrawPunchButton(Vector2 center, float radius, string symbol, string title)
        {
            Rect outer = new(center.x - radius, center.y - radius, radius * 2f, radius * 2f);
            Rect inner = new(center.x - radius * 0.80f, center.y - radius * 0.80f, radius * 1.60f, radius * 1.60f);
            GUI.color = new Color(Gold.r, Gold.g, Gold.b, 0.88f);
            GUI.DrawTexture(outer, _circle);
            GUI.color = new Color(Ink.r, Ink.g, Ink.b, 0.92f);
            GUI.DrawTexture(inner, _circle);
            GUI.color = Color.white;
            Label(inner, symbol, Mathf.RoundToInt(Screen.height / 67f), TextAnchor.MiddleCenter, Gold, true);
            Label(new Rect(center.x - radius * 1.4f, center.y + radius * 0.70f, radius * 2.8f, radius * 0.68f),
                title, Mathf.RoundToInt(Screen.height / 112f), TextAnchor.MiddleCenter, WarmWhite, true);
        }

        private void DrawControlDisc(Vector2 center, float radius, string title, string symbol)
        {
            Rect outer = new(center.x - radius, center.y - radius, radius * 2f, radius * 2f);
            Rect inner = new(center.x - radius * 0.82f, center.y - radius * 0.82f, radius * 1.64f, radius * 1.64f);
            GUI.color = new Color(Gold.r, Gold.g, Gold.b, 0.82f);
            GUI.DrawTexture(outer, _circle);
            GUI.color = new Color(Ink.r, Ink.g, Ink.b, 0.88f);
            GUI.DrawTexture(inner, _circle);
            GUI.color = Color.white;
            Label(inner, symbol, Mathf.RoundToInt(Screen.height / 48f), TextAnchor.MiddleCenter, Gold, true);
            Label(new Rect(center.x - radius * 1.1f, center.y + radius * 0.82f, radius * 2.2f, radius * 0.55f), title,
                Mathf.RoundToInt(Screen.height / 80f), TextAnchor.MiddleCenter, Gold, true);
        }

        private bool ShouldShowTraining()
        {
            return _telemetry == null || (_telemetry.BoutResult == "PENDING" && !string.IsNullOrEmpty(_trainingStage));
        }

        private void DrawDiagnostics()
        {
            Rect card = new(Screen.width * 0.025f, Screen.height * 0.20f, Screen.width * 0.42f, Screen.height * 0.15f);
            DrawPanel(card, new Color(0.01f, 0.01f, 0.012f, 0.91f), new Color(Gold.r, Gold.g, Gold.b, 0.65f), 1f);
            string action = _opponent != null ? _opponent.ActionLabel : "N/A";
            string profile = _opponent != null ? _opponent.AttributeInspectorText : "N/A";
            string lastEvent = _telemetry != null ? _telemetry.LastEvent : "N/A";
            string copy = $"DEV DIAGNOSTICS · F3\n{profile}\nACTION {action}\nEVENT {lastEvent}";
            Label(new Rect(card.x + 8f, card.y + 6f, card.width - 16f, card.height - 12f), copy,
                Mathf.RoundToInt(Screen.height / 105f), TextAnchor.UpperLeft, WarmWhite, false);
        }

        private void DrawTrainingCard()
        {
            GetTrainingCopy(out string step, out string action, out string hint);
            Rect card = new(Screen.width * 0.12f, Screen.height * 0.16f, Screen.width * 0.76f, Screen.height * 0.16f);
            DrawPanel(card, new Color(0.02f, 0.018f, 0.016f, 0.90f), Gold, 2f);
            Label(new Rect(card.x, card.y + card.height * 0.05f, card.width, card.height * 0.23f), step, Mathf.RoundToInt(Screen.height / 70f), TextAnchor.MiddleCenter, Gold, true);
            Label(new Rect(card.x + 8f, card.y + card.height * 0.28f, card.width - 16f, card.height * 0.34f), action, Mathf.RoundToInt(Screen.height / 42f), TextAnchor.MiddleCenter, WarmWhite, true);
            Label(new Rect(card.x + 8f, card.y + card.height * 0.63f, card.width - 16f, card.height * 0.25f), hint, Mathf.RoundToInt(Screen.height / 79f), TextAnchor.MiddleCenter, WarmWhite, false);
        }

        private void GetTrainingCopy(out string step, out string action, out string hint)
        {
            switch (_trainingStage)
            {
                case "HEADCONTROL": step = "1 / 5   HEAD MOVEMENT"; action = "NGHIÊNG TRÁI  →  PHẢI"; hint = "PHONE = HEAD  ·  LOOK  ·  REACT  ·  STAY READY"; return;
                case "FOOTWORK": step = "2 / 5   FOOTWORK"; action = "DI CHUYỂN ĐỦ 4 HƯỚNG"; hint = "LEFT THUMB = FEET  ·  MOVE  ·  ANGLE  ·  CONTROL"; return;
                case "PUNCHES": step = "3 / 5   PUNCH MECHANICS"; action = "THỬ ĐỦ 4 GIA ĐÌNH ĐÒN"; hint = "TAP STRAIGHT  ·  SWIPE UP / SIDE / DOWN"; return;
                case "GUARD": step = "4 / 5   GUARD"; action = "DỪNG ĐẤM = HIGH GUARD"; hint = "NO TOUCH = GUARD  ·  STAY CALM  ·  STAY PROTECTED"; return;
                case "COUNTER": step = "5 / 5   COUNTER"; action = "ĐỌC ĐÒN  →  NÉ / ĐỠ  →  PHẢN CÔNG"; hint = "COUNTER DURING OPPONENT RECOVERY"; return;
                default: step = "CALIBRATE"; action = "GIỮ ĐIỆN THOẠI Ở TƯ THẾ THOẢI MÁI"; hint = "PHONE = HEAD  ·  CALIBRATE TO ENTER THE RING"; return;
            }
        }

        private bool IsBoutComplete()
        {
            return _telemetry != null && _telemetry.BoutResult != "PENDING" && _telemetry.BoutResult != "IN_PROGRESS";
        }

        private void DrawResultCard()
        {
            string verdict = _telemetry.BoutResult == "PLAYER_WIN" ? "VICTORY" : _telemetry.BoutResult == "OPPONENT_WIN" ? "DEFEAT" : "DRAW";
            Rect card = new(Screen.width * 0.17f, Screen.height * 0.32f, Screen.width * 0.66f, Screen.height * 0.26f);
            DrawPanel(card, new Color(0.018f, 0.016f, 0.014f, 0.94f), Gold, 3f);
            Label(new Rect(card.x, card.y + card.height * 0.08f, card.width, card.height * 0.22f), "BOUT COMPLETE", Mathf.RoundToInt(Screen.height / 63f), TextAnchor.MiddleCenter, Gold, true);
            Label(new Rect(card.x, card.y + card.height * 0.28f, card.width, card.height * 0.35f), verdict, Mathf.RoundToInt(Screen.height / 31f), TextAnchor.MiddleCenter, WarmWhite, true);
            string score = $"BOXER  {_telemetry.PlayerHits} HITS    ·    RAMIREZ  {_telemetry.OpponentHits} HITS";
            Label(new Rect(card.x, card.y + card.height * 0.65f, card.width, card.height * 0.18f), score, Mathf.RoundToInt(Screen.height / 76f), TextAnchor.MiddleCenter, WarmWhite, true);
        }

        private string TimerText()
        {
            if (_telemetry == null || _telemetry.BoutResult == "PENDING") return "TRAINING";
            if (_telemetry.BoutResult != "IN_PROGRESS") return "0:00";
            float remaining = Mathf.Max(0f, TestBoutSeconds - (Time.unscaledTime - _observedBoutStart));
            return $"0:{Mathf.CeilToInt(remaining):00}";
        }

        private void DrawKeyed(Rect rect, Texture texture, bool mirror, float alpha)
        {
            if (texture == null || _chromaMaterial == null || alpha <= 0.001f || Event.current.type != EventType.Repaint) return;
            Rect uv = mirror ? new Rect(1f, 0f, -1f, 1f) : new Rect(0f, 0f, 1f, 1f);
            Graphics.DrawTexture(rect, texture, uv, 0, 0, 0, 0, new Color(1f, 1f, 1f, Mathf.Clamp01(alpha)), _chromaMaterial);
        }

        private void DrawPanel(Rect rect, Color fill, Color border, float thickness)
        {
            DrawSolid(rect, fill);
            DrawBorder(rect, border, thickness);
        }

        private static void DrawBorder(Rect rect, Color color, float thickness)
        {
            DrawSolid(new Rect(rect.x, rect.y, rect.width, thickness), color);
            DrawSolid(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            DrawSolid(new Rect(rect.x, rect.y, thickness, rect.height), color);
            DrawSolid(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private static void DrawSolid(Rect rect, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = old;
        }

        private void Label(Rect rect, string text, int size, TextAnchor alignment, Color color, bool bold)
        {
            _label.fontSize = Mathf.Clamp(size, 9, 46);
            _label.alignment = alignment;
            _label.normal.textColor = color;
            _label.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            _label.wordWrap = true;
            GUI.Label(rect, text, _label);
        }

        private static Texture2D BuildCircleTexture(int size)
        {
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
            {
                name = "P1V Procedural Control Disc",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            Color32[] pixels = new Color32[size * size];
            float center = (size - 1) * 0.5f;
            float radius = size * 0.49f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float distance = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(radius - distance + 1f) * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }
    }
}
