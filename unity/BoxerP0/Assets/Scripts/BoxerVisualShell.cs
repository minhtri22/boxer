using System;
using UnityEngine;

namespace BoxerP0
{
    /// <summary>
    /// Lightweight P0.5 visual shell for the Web surrogate.
    /// Presentation only: reactive HP/stamina are non-authoritative HUD feedback and never gate combat.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class BoxerVisualShell : MonoBehaviour
    {
        private Phase0Telemetry _telemetry;
        private BoxerInput _input;
        private OpponentBoxer _opponent;
        private float _observedBoutStart = -1f;
        private string _lastBoutResult = "PENDING";
        private string _lastTelemetryEvent = string.Empty;
        private string _trainingStage = "CALIBRATE";

        private float _playerHp = 1f;
        private float _opponentHp = 1f;
        private float _playerStamina = 1f;
        private float _opponentStamina = 1f;
        private uint _lastPlayerPunchCount;
        private uint _lastOpponentAttackCount;

        private GUIStyle _hudLabel;
        private GUIStyle _hudSmall;
        private GUIStyle _timerStyle;
        private GUIStyle _trainingStepStyle;
        private GUIStyle _trainingActionStyle;
        private GUIStyle _trainingHintStyle;

        private const float TestBoutSeconds = 45f;
        private const float HpLossPerHit = 0.08f;
        private const float PlayerStaminaCostPerPunch = 0.12f;
        private const float OpponentStaminaCostPerPunch = 0.10f;
        private const float PlayerStaminaRecoveryPerSecond = 0.22f;
        private const float OpponentStaminaRecoveryPerSecond = 0.18f;

        private static readonly Color Charcoal = new(0.045f, 0.050f, 0.060f, 1f);
        private static readonly Color Brick = new(0.12f, 0.065f, 0.055f, 1f);
        private static readonly Color Canvas = new(0.29f, 0.27f, 0.23f, 1f);
        private static readonly Color Rope = new(0.43f, 0.35f, 0.27f, 1f);
        private static readonly Color Gold = new(0.90f, 0.62f, 0.15f, 1f);
        private static readonly Color GloveBlack = new(0.035f, 0.040f, 0.045f, 1f);
        private static readonly Color Skin = new(0.56f, 0.31f, 0.22f, 1f);
        private static readonly Color OpponentShorts = new(0.18f, 0.19f, 0.21f, 1f);
        private static readonly Color CrowdDark = new(0.055f, 0.060f, 0.070f, 1f);
        private static readonly Color WarmLight = new(1.00f, 0.67f, 0.25f, 1f);

        private void Start()
        {
            _telemetry = FindFirstObjectByType<Phase0Telemetry>();
            _input = FindFirstObjectByType<BoxerInput>();
            _opponent = FindFirstObjectByType<OpponentBoxer>();
            if (_input != null) _lastPlayerPunchCount = _input.PunchEventCount;
            if (_opponent != null) _lastOpponentAttackCount = _opponent.AttackEventCount;

            BuildWarehouseShell();
            RestyleExistingRing();
            RestylePlayerGloves();
            RestyleOpponent();
        }

        private void Update()
        {
            if (_telemetry == null) return;

            TrackTrainingStage();
            TrackBoutTransition();
            UpdateReactiveHudState();
        }

        private void TrackTrainingStage()
        {
            string value = _telemetry.LastEvent ?? string.Empty;
            if (value == _lastTelemetryEvent) return;
            _lastTelemetryEvent = value;

            const string prefix = "TRAINING_STAGE_START_";
            if (value.StartsWith(prefix, StringComparison.Ordinal))
            {
                _trainingStage = value.Substring(prefix.Length);
            }
            else if (value == "TRAINING_COMPLETE" || value == "BOUT_START")
            {
                _trainingStage = string.Empty;
            }
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
                if (_input != null) _lastPlayerPunchCount = _input.PunchEventCount;
                if (_opponent != null) _lastOpponentAttackCount = _opponent.AttackEventCount;
            }

            _lastBoutResult = current;
        }

        private void UpdateReactiveHudState()
        {
            _playerHp = Mathf.Clamp01(1f - _telemetry.OpponentHits * HpLossPerHit);
            _opponentHp = Mathf.Clamp01(1f - _telemetry.PlayerHits * HpLossPerHit);

            if (_input != null)
            {
                uint count = _input.PunchEventCount;
                uint delta = count - _lastPlayerPunchCount;
                if (delta > 0)
                {
                    _playerStamina = Mathf.Clamp01(_playerStamina - delta * PlayerStaminaCostPerPunch);
                    _lastPlayerPunchCount = count;
                }
            }

            if (_opponent != null)
            {
                uint count = _opponent.AttackEventCount;
                uint delta = count - _lastOpponentAttackCount;
                if (delta > 0)
                {
                    _opponentStamina = Mathf.Clamp01(_opponentStamina - delta * OpponentStaminaCostPerPunch);
                    _lastOpponentAttackCount = count;
                }
            }

            float dt = Time.unscaledDeltaTime;
            _playerStamina = Mathf.MoveTowards(_playerStamina, 1f, PlayerStaminaRecoveryPerSecond * dt);
            _opponentStamina = Mathf.MoveTowards(_opponentStamina, 1f, OpponentStaminaRecoveryPerSecond * dt);
        }

        private void BuildWarehouseShell()
        {
            CreateDecor(PrimitiveType.Cube, "Warehouse Back Wall", new Vector3(0f, 2.4f, 4.4f), new Vector3(8.0f, 4.8f, 0.18f), Brick);
            CreateDecor(PrimitiveType.Cube, "Warehouse Left Wall", new Vector3(-4.1f, 2.4f, 0.3f), new Vector3(0.18f, 4.8f, 8.0f), Charcoal);
            CreateDecor(PrimitiveType.Cube, "Warehouse Right Wall", new Vector3(4.1f, 2.4f, 0.3f), new Vector3(0.18f, 4.8f, 8.0f), Charcoal);
            CreateDecor(PrimitiveType.Cube, "Warehouse Ceiling", new Vector3(0f, 4.75f, 0.3f), new Vector3(8.2f, 0.12f, 8.2f), Charcoal);

            CreateDecor(PrimitiveType.Cylinder, "Hanging Lamp", new Vector3(0f, 3.7f, 1.3f), new Vector3(0.42f, 0.08f, 0.42f), new Color(0.14f, 0.12f, 0.09f, 1f));
            CreateDecor(PrimitiveType.Sphere, "Hanging Lamp Glow", new Vector3(0f, 3.58f, 1.3f), Vector3.one * 0.19f, WarmLight);

            for (int i = -4; i <= 4; i++)
            {
                float x = i * 0.78f;
                CreateDecor(PrimitiveType.Sphere, "String Bulb", new Vector3(x, 3.05f + Mathf.Abs(i) * 0.04f, 2.5f), Vector3.one * 0.075f, WarmLight);
            }

            CreateDecor(PrimitiveType.Cube, "Crate L", new Vector3(-3.0f, 0.35f, 2.7f), new Vector3(0.75f, 0.70f, 0.75f), new Color(0.24f, 0.15f, 0.08f, 1f));
            CreateDecor(PrimitiveType.Cube, "Crate R", new Vector3(3.05f, 0.27f, 2.9f), new Vector3(0.62f, 0.54f, 0.62f), new Color(0.20f, 0.13f, 0.07f, 1f));

            BuildCrowdRow(-3.30f, 1f);
            BuildCrowdRow(3.30f, -1f);
            BuildBackCrowd();
        }

        private void BuildCrowdRow(float x, float facingSign)
        {
            for (int i = 0; i < 7; i++)
            {
                float z = -1.6f + i * 0.72f;
                float height = 1.45f + (i % 3) * 0.10f;
                GameObject body = CreateDecor(PrimitiveType.Capsule, "Crowd Silhouette", new Vector3(x, height * 0.48f, z), new Vector3(0.34f, height * 0.48f, 0.28f), CrowdDark);
                body.transform.rotation = Quaternion.Euler(0f, facingSign > 0f ? 90f : -90f, 0f);
                CreateDecor(PrimitiveType.Sphere, "Crowd Head", new Vector3(x, height + 0.05f, z), Vector3.one * 0.22f, new Color(0.11f, 0.095f, 0.085f, 1f));
            }
        }

        private void BuildBackCrowd()
        {
            for (int i = 0; i < 9; i++)
            {
                float x = -2.8f + i * 0.70f;
                float z = 3.25f;
                float h = 1.45f + ((i + 1) % 4) * 0.08f;
                CreateDecor(PrimitiveType.Capsule, "Back Crowd", new Vector3(x, h * 0.48f, z), new Vector3(0.30f, h * 0.48f, 0.25f), CrowdDark);
                CreateDecor(PrimitiveType.Sphere, "Back Crowd Head", new Vector3(x, h + 0.04f, z), Vector3.one * 0.20f, new Color(0.10f, 0.085f, 0.075f, 1f));
            }
        }

        private void RestyleExistingRing()
        {
            GameObject floor = GameObject.Find("Neutral Ring Floor");
            if (floor != null)
            {
                floor.name = "Underground Ring Canvas";
                ApplyColor(floor.GetComponent<Renderer>(), Canvas);
            }

            GameObject[] roots = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject go in roots)
            {
                if (go.name == "Ring Rope") ApplyColor(go.GetComponent<Renderer>(), Rope);
            }

            CreateDecor(PrimitiveType.Cylinder, "Generic Championship Medallion", new Vector3(0f, 0.018f, -1.65f), new Vector3(0.48f, 0.018f, 0.48f), Gold).transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            CreateDecor(PrimitiveType.Cube, "Generic Championship Strap", new Vector3(0f, 0.010f, -1.65f), new Vector3(1.85f, 0.018f, 0.30f), new Color(0.04f, 0.18f, 0.10f, 1f));
        }

        private void RestylePlayerGloves()
        {
            RestyleGlove("Player Left Glove");
            RestyleGlove("Player Right Glove");
        }

        private void RestyleGlove(string name)
        {
            GameObject glove = GameObject.Find(name);
            if (glove == null) return;
            ApplyColor(glove.GetComponent<Renderer>(), GloveBlack);

            GameObject cuff = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cuff.name = name + " Gold Cuff";
            cuff.transform.SetParent(glove.transform, false);
            cuff.transform.localPosition = new Vector3(0f, -0.54f, 0f);
            cuff.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            cuff.transform.localScale = new Vector3(0.82f, 0.15f, 0.82f);
            DisableCollider(cuff);
            ApplyColor(cuff.GetComponent<Renderer>(), Gold);

            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = name + " Gold Panel";
            panel.transform.SetParent(glove.transform, false);
            panel.transform.localPosition = new Vector3(0f, 0.03f, 0.48f);
            panel.transform.localScale = new Vector3(0.58f, 0.58f, 0.08f);
            DisableCollider(panel);
            ApplyColor(panel.GetComponent<Renderer>(), new Color(0.72f, 0.47f, 0.10f, 1f));
        }

        private void RestyleOpponent()
        {
            GameObject body = GameObject.Find("Opponent Body");
            GameObject head = GameObject.Find("Opponent Head");
            GameObject root = GameObject.Find("Opponent");
            if (body != null) ApplyColor(body.GetComponent<Renderer>(), Skin);
            if (head != null) ApplyColor(head.GetComponent<Renderer>(), new Color(0.48f, 0.25f, 0.18f, 1f));
            if (root == null) return;

            GameObject shorts = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shorts.name = "Opponent Shorts Visual";
            shorts.transform.SetParent(root.transform, false);
            shorts.transform.localPosition = new Vector3(0f, 0.72f, 0f);
            shorts.transform.localScale = new Vector3(0.72f, 0.33f, 0.48f);
            DisableCollider(shorts);
            ApplyColor(shorts.GetComponent<Renderer>(), OpponentShorts);

            GameObject waistband = GameObject.CreatePrimitive(PrimitiveType.Cube);
            waistband.name = "Opponent Gold Waistband";
            waistband.transform.SetParent(root.transform, false);
            waistband.transform.localPosition = new Vector3(0f, 0.91f, 0f);
            waistband.transform.localScale = new Vector3(0.75f, 0.08f, 0.50f);
            DisableCollider(waistband);
            ApplyColor(waistband.GetComponent<Renderer>(), Gold);

            AddOpponentShoulder(root.transform, new Vector3(-0.38f, 1.37f, 0f));
            AddOpponentShoulder(root.transform, new Vector3(0.38f, 1.37f, 0f));

            GameObject left = GameObject.Find("Opponent Left Glove");
            GameObject right = GameObject.Find("Opponent Right Glove");
            if (left != null) ApplyColor(left.GetComponent<Renderer>(), new Color(0.72f, 0.66f, 0.52f, 1f));
            if (right != null) ApplyColor(right.GetComponent<Renderer>(), new Color(0.72f, 0.66f, 0.52f, 1f));
        }

        private void AddOpponentShoulder(Transform root, Vector3 localPosition)
        {
            GameObject shoulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shoulder.name = "Opponent Shoulder Visual";
            shoulder.transform.SetParent(root, false);
            shoulder.transform.localPosition = localPosition;
            shoulder.transform.localScale = new Vector3(0.30f, 0.38f, 0.30f);
            DisableCollider(shoulder);
            ApplyColor(shoulder.GetComponent<Renderer>(), Skin);
        }

        private void OnGUI()
        {
            EnsureStyles();
            if (ShouldShowTrainingOverlay()) DrawTrainingOverlay();
            DrawGameHud();
        }

        private bool ShouldShowTrainingOverlay()
        {
            if (_telemetry == null) return true;
            return _telemetry.BoutResult == "PENDING" && !string.IsNullOrEmpty(_trainingStage);
        }

        private void EnsureStyles()
        {
            if (_hudLabel != null) return;

            _hudLabel = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Screen.height / 48, 12, 24),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            _hudSmall = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Screen.height / 70, 10, 18),
                alignment = TextAnchor.MiddleLeft
            };
            _timerStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = Mathf.Clamp(Screen.height / 44, 14, 26),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _trainingStepStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Screen.height / 34, 20, 34),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            _trainingActionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Screen.height / 27, 26, 44),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            _trainingHintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Screen.height / 43, 17, 28),
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
        }

        private void DrawTrainingOverlay()
        {
            GetTrainingCopy(out string step, out string action, out string hint);

            float panelHeight = Mathf.Clamp(Screen.height * 0.36f, 245f, 360f);
            Color old = GUI.color;
            GUI.color = new Color(0.025f, 0.025f, 0.030f, 0.94f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, panelHeight), Texture2D.whiteTexture);
            GUI.color = old;

            float side = Mathf.Max(18f, Screen.width * 0.04f);
            float hudBottom = Mathf.Max(100f, Screen.height * 0.13f);
            GUI.Label(new Rect(side, hudBottom, Screen.width - side * 2f, 42f), step, _trainingStepStyle);
            GUI.Label(new Rect(side, hudBottom + 40f, Screen.width - side * 2f, 92f), action, _trainingActionStyle);
            GUI.Label(new Rect(side, hudBottom + 128f, Screen.width - side * 2f, 68f), hint, _trainingHintStyle);
        }

        private void GetTrainingCopy(out string step, out string action, out string hint)
        {
            switch (_trainingStage)
            {
                case "HEADCONTROL":
                    step = "1/5  HEAD CONTROL";
                    action = "NGHIÊNG TRÁI  →  PHẢI";
                    hint = "Phone = head. Cảm nhận đầu nhân vật đi theo chuyển động của máy.";
                    return;
                case "FOOTWORK":
                    step = "2/5  FOOTWORK";
                    action = "DI CHUYỂN ĐỦ 4 HƯỚNG";
                    hint = "Ngón cái trái = chân:  ←  →  ↑  ↓";
                    return;
                case "PUNCHES":
                    step = "3/5  PUNCHES";
                    action = "THỬ 4 ĐÒN BẰNG TAY PHẢI";
                    hint = "JAB  ·  CROSS  ·  LEAD HOOK  ·  REAR HOOK";
                    return;
                case "GUARD":
                    step = "4/5  GUARD";
                    action = "DỪNG ĐẤM = HIGH GUARD";
                    hint = "Không vuốt tay phải. Hãy đỡ 2 đòn của đối thủ.";
                    return;
                case "COUNTER":
                    step = "5/5  COUNTER";
                    action = "ĐỌC ĐÒN  →  NÉ/ĐỠ  →  PHẢN CÔNG";
                    hint = "Phản công khi đối thủ đang hồi đòn.";
                    return;
                default:
                    step = "CALIBRATE";
                    action = "GIỮ ĐIỆN THOẠI Ở TƯ THẾ THOẢI MÁI";
                    hint = "Cho phép Motion, giữ máy thẳng tự nhiên rồi bấm CALIBRATE.";
                    return;
            }
        }

        private void DrawGameHud()
        {
            float margin = Mathf.Max(12f, Screen.width * 0.025f);
            float hudWidth = Mathf.Min(280f, Screen.width * 0.36f);
            float barWidth = hudWidth - 10f;
            float top = Mathf.Max(12f, Screen.height * 0.018f);

            DrawFighterHud(new Rect(margin, top, hudWidth, 88f), "LV 12  BOXER", barWidth, _playerHp, _playerStamina);
            DrawFighterHud(new Rect(Screen.width - margin - hudWidth, top, hudWidth, 88f), "LV 15  OPPONENT", barWidth, _opponentHp, _opponentStamina);

            string timerText = "TRAINING";
            if (_telemetry != null && _telemetry.BoutResult == "IN_PROGRESS" && _observedBoutStart >= 0f)
            {
                float remaining = Mathf.Max(0f, TestBoutSeconds - (Time.unscaledTime - _observedBoutStart));
                timerText = $"0:{Mathf.CeilToInt(remaining):00}";
            }
            else if (_telemetry != null && _telemetry.BoutResult != "PENDING" && _telemetry.BoutResult != "IN_PROGRESS")
            {
                timerText = "0:00";
            }

            float timerWidth = Mathf.Min(115f, Screen.width * 0.17f);
            GUI.Box(new Rect((Screen.width - timerWidth) * 0.5f, top, timerWidth, 46f), timerText, _timerStyle);
            GUI.Label(new Rect((Screen.width - 210f) * 0.5f, top + 48f, 210f, 22f), "P0 REACTIVE HUD", _hudSmall);
        }

        private void DrawFighterHud(Rect rect, string title, float barWidth, float hp, float stamina)
        {
            GUI.Box(rect, string.Empty);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 3f, rect.width - 16f, 24f), title, _hudLabel);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 28f, 68f, 18f), "HP", _hudSmall);
            DrawBar(new Rect(rect.x + 42f, rect.y + 31f, barWidth - 42f, 12f), hp, new Color(0.78f, 0.10f, 0.07f, 1f));
            GUI.Label(new Rect(rect.x + 8f, rect.y + 51f, 68f, 18f), "STAMINA", _hudSmall);
            DrawBar(new Rect(rect.x + 74f, rect.y + 54f, barWidth - 74f, 12f), stamina, new Color(0.94f, 0.67f, 0.10f, 1f));
        }

        private static void DrawBar(Rect rect, float fill, Color color)
        {
            Color old = GUI.color;
            GUI.color = new Color(0.08f, 0.08f, 0.08f, 0.92f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = color;
            Rect filled = rect;
            filled.width *= Mathf.Clamp01(fill);
            GUI.DrawTexture(filled, Texture2D.whiteTexture);
            GUI.color = old;
        }

        private static GameObject CreateDecor(PrimitiveType type, string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = scale;
            DisableCollider(go);
            ApplyColor(go.GetComponent<Renderer>(), color);
            return go;
        }

        private static void DisableCollider(GameObject go)
        {
            Collider collider = go.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
        }

        private static void ApplyColor(Renderer renderer, Color color)
        {
            if (renderer == null) return;
            Shader shader = Resources.Load<Shader>("BoxerP0UnlitColor");
            if (shader != null)
            {
                Material material = new(shader);
                material.color = color;
                renderer.sharedMaterial = material;
                return;
            }

            Material fallback = renderer.material;
            if (fallback == null) return;
            if (fallback.HasProperty("_BaseColor")) fallback.SetColor("_BaseColor", color);
            if (fallback.HasProperty("_Color")) fallback.SetColor("_Color", color);
        }
    }
}
