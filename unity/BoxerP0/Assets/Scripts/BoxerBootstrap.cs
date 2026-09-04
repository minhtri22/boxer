using UnityEngine;

namespace BoxerP0
{
    public sealed class BoxerBootstrap : MonoBehaviour
    {
        private BoxerInput _input;
        private PlayerBoxer _player;
        private OpponentBoxer _opponent;
        private Phase0Telemetry _telemetry;
        private BoxerFeedback _feedback;
        private float _instructionUntil;
        private float _boutEnd;
        private float _smokeQuitAt = -1f;
        private bool _boutStarted;
        private bool _boutCompleted;

        private const float BoutSeconds = 90f;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            _instructionUntil = Time.unscaledTime + 6f;
            ConfigureSmokeQuit();
            BuildLightingAndRing();
            BuildActors();

#if UNITY_WEBGL && !UNITY_EDITOR
            _boutStarted = false;
            _boutEnd = float.PositiveInfinity;
#else
            StartBout();
#endif
        }

        private void Update()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!_boutStarted && _input != null && _input.BrowserCalibrated)
            {
                StartBout();
            }
#endif
            if (_boutStarted && !_boutCompleted && Time.unscaledTime >= _boutEnd)
            {
                CompleteBout();
            }

            if (_smokeQuitAt > 0f && Time.unscaledTime >= _smokeQuitAt)
            {
                Debug.Log("P0_SMOKE_COMPLETE");
                Application.Quit(0);
            }
        }

        private void StartBout()
        {
            _boutStarted = true;
            _boutCompleted = false;
            _instructionUntil = Time.unscaledTime + 6f;
            _boutEnd = Time.unscaledTime + BoutSeconds;
            _player?.SetCombatEnabled(true);
            _opponent?.SetCombatEnabled(true);
            _telemetry?.RecordBoutStart();
        }

        private void CompleteBout()
        {
            _boutCompleted = true;
            _player?.SetCombatEnabled(false);
            _opponent?.SetCombatEnabled(false);
            string result = _telemetry?.CompleteBout() ?? "UNKNOWN";
            Debug.Log($"P0_BOUT_COMPLETE {result}");
        }

        private void ConfigureSmokeQuit()
        {
            foreach (string argument in System.Environment.GetCommandLineArgs())
            {
                const string prefix = "-p0SmokeSeconds=";
                if (!argument.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase)) continue;
                if (float.TryParse(argument.Substring(prefix.Length), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float seconds) && seconds > 0f)
                {
                    _smokeQuitAt = Time.unscaledTime + seconds;
                }
            }
        }

        private void BuildLightingAndRing()
        {
            if (FindFirstObjectByType<Light>() == null)
            {
                GameObject lightObject = new("Directional Light");
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.15f;
                lightObject.transform.rotation = Quaternion.Euler(45f, -25f, 0f);
            }

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Neutral Ring Floor";
            floor.transform.position = new Vector3(0f, -0.08f, 0.2f);
            floor.transform.localScale = new Vector3(5.2f, 0.15f, 5.2f);
            ApplyColor(floor.GetComponent<Renderer>(), new Color(0.12f, 0.14f, 0.18f));

            CreateBoundary(new Vector3(0f, 0.6f, 2.75f), new Vector3(5.4f, 0.08f, 0.08f));
            CreateBoundary(new Vector3(0f, 0.6f, -2.35f), new Vector3(5.4f, 0.08f, 0.08f));
            CreateBoundary(new Vector3(2.55f, 0.6f, 0.2f), new Vector3(0.08f, 0.08f, 5.2f));
            CreateBoundary(new Vector3(-2.55f, 0.6f, 0.2f), new Vector3(0.08f, 0.08f, 5.2f));
        }

        private void BuildActors()
        {
            GameObject systems = new("P0 Systems");
            _input = systems.AddComponent<BoxerInput>();
            _telemetry = systems.AddComponent<Phase0Telemetry>();
            _feedback = systems.AddComponent<BoxerFeedback>();

            GameObject playerRoot = new("Player Boxer");
            playerRoot.transform.position = new Vector3(0f, 0f, -0.7f);
            _player = playerRoot.AddComponent<PlayerBoxer>();

            GameObject headObject = new("Player Head");
            headObject.transform.SetParent(playerRoot.transform, false);
            headObject.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            SphereCollider headCollider = headObject.AddComponent<SphereCollider>();
            headCollider.radius = 0.18f;

            GameObject cameraObject = new("POV Camera");
            cameraObject.transform.SetParent(headObject.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 0.02f, 0.01f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 72f;
            camera.nearClipPlane = 0.04f;
            cameraObject.AddComponent<AudioListener>();

            GameObject bodyObject = new("Player Body Target");
            bodyObject.transform.SetParent(playerRoot.transform, false);
            bodyObject.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            SphereCollider bodyCollider = bodyObject.AddComponent<SphereCollider>();
            bodyCollider.radius = 0.29f;

            Color playerColor = new(0.05f, 0.55f, 1.00f);
            (Transform leftPlayerGlove, SphereCollider leftPlayerCollider) = CreateGlove(
                "Player Left Glove", playerRoot.transform, new Vector3(-0.22f, 1.38f, 0.48f), playerColor);
            (Transform rightPlayerGlove, SphereCollider rightPlayerCollider) = CreateGlove(
                "Player Right Glove", playerRoot.transform, new Vector3(0.22f, 1.38f, 0.48f), playerColor);

            GameObject opponentRoot = new("Opponent");
            opponentRoot.transform.position = new Vector3(0f, 0f, 0.70f);
            opponentRoot.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            _opponent = opponentRoot.AddComponent<OpponentBoxer>();

            Color opponentColor = new(1.00f, 0.20f, 0.08f);
            GameObject opponentBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            opponentBody.name = "Opponent Body";
            opponentBody.transform.SetParent(opponentRoot.transform, false);
            opponentBody.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            opponentBody.transform.localScale = new Vector3(0.58f, 0.62f, 0.42f);
            ApplyColor(opponentBody.GetComponent<Renderer>(), opponentColor);
            Collider originalBodyCollider = opponentBody.GetComponent<Collider>();
            Destroy(originalBodyCollider);
            SphereCollider opponentBodyCollider = opponentBody.AddComponent<SphereCollider>();
            opponentBodyCollider.radius = 0.48f;

            GameObject opponentHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            opponentHead.name = "Opponent Head";
            opponentHead.transform.SetParent(opponentRoot.transform, false);
            opponentHead.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            opponentHead.transform.localScale = Vector3.one * 0.36f;
            ApplyColor(opponentHead.GetComponent<Renderer>(), new Color(1.00f, 0.42f, 0.10f));
            SphereCollider opponentHeadCollider = opponentHead.GetComponent<SphereCollider>();
            opponentHeadCollider.radius = 0.5f;

            (Transform leftOpponentGlove, SphereCollider leftOpponentCollider) = CreateGlove(
                "Opponent Left Glove", opponentRoot.transform, new Vector3(-0.22f, 1.38f, 0.45f), opponentColor);
            (Transform rightOpponentGlove, SphereCollider rightOpponentCollider) = CreateGlove(
                "Opponent Right Glove", opponentRoot.transform, new Vector3(0.22f, 1.38f, 0.45f), opponentColor);

            _player.Initialize(
                _input,
                _opponent,
                _telemetry,
                headObject.transform,
                headCollider,
                bodyCollider,
                leftPlayerGlove,
                leftPlayerCollider,
                rightPlayerGlove,
                rightPlayerCollider);

            _opponent.Initialize(
                _player,
                _telemetry,
                leftOpponentGlove,
                leftOpponentCollider,
                rightOpponentGlove,
                rightOpponentCollider,
                opponentHeadCollider,
                opponentBodyCollider);

            _telemetry.InputSource = _input;
            _telemetry.Player = _player;
            _telemetry.Opponent = _opponent;
        }

        private void OnGUI()
        {
            GUI.skin.label.fontSize = Mathf.Clamp(Screen.height / 52, 12, 22);
            GUI.skin.box.fontSize = GUI.skin.label.fontSize;

            if (Time.unscaledTime < _instructionUntil)
            {
                GUI.Box(new Rect(20, 20, Mathf.Min(Screen.width - 40, 650), 125),
                    "Left thumb = feet.\nRight thumb = punch controller.\nPhone = head.\nHook left/inward = lead · hook right/outward = rear.");
            }

            if (_input == null || _player == null || _opponent == null || _telemetry == null) return;

            string debug =
                $"MOTION {_input.BrowserMotionPermission}  SRC {_input.HeadInputSource}\n" +
                $"HEAD {_input.HeadAngleDegrees:F1}° → {_player.HeadOffset:F2}m\n" +
                $"MOVE {_input.MovementIntent.x:F2},{_input.MovementIntent.y:F2}\n" +
                $"PLAYER PUNCH: {_input.LastPunchLabel}\n" +
                $"PLAYER {_player.ActionLabel}  GUARD {(_player.GuardActive ? "HIGH" : "OPEN")}\n" +
                $"OPP {_opponent.ActionLabel}  COUNTER {(_opponent.CounterWindowOpen ? "OPEN" : "CLOSED")}\n" +
                $"LAST {_telemetry.LastOutcome} / {_telemetry.LastEvent}\n" +
                $"BOUT {GetBoutSecondsRemaining():F0}s  FRAME {(Time.unscaledDeltaTime * 1000f):F1}ms";
            GUI.Box(new Rect(20, Screen.height - 235, Mathf.Min(Screen.width - 40, 680), 215), debug);

            if (!Application.isMobilePlatform)
            {
                GUI.Box(new Rect(Screen.width - 295, 20, 275, 170),
                    "EDITOR SYNTHETIC\nWASD feet · Q/E head\nJ lead jab · K rear cross\nL lead hook · ; rear hook\nR recalibrate · M audio · H haptic");
            }

            if (_boutCompleted)
            {
                string resultText = _telemetry.BoutResult switch
                {
                    "PLAYER_WIN" => "PLAYER WIN",
                    "OPPONENT_WIN" => "OPPONENT WIN",
                    "DRAW" => "DRAW",
                    _ => _telemetry.BoutResult
                };

                string summary =
                    "BOUT COMPLETE — P0 TEST ONLY\n\n" +
                    $"PLAYER  Hits {_telemetry.PlayerHits}  Counters {_telemetry.PlayerCounterHits}  Blocks {_telemetry.PlayerBlocks}\n" +
                    $"OPPONENT  Hits {_telemetry.OpponentHits}  Counters {_telemetry.OpponentCounterHits}  Blocks {_telemetry.OpponentBlocks}\n\n" +
                    $"RESULT: {resultText}\n\n" +
                    "Win rule: valid landed hits only.\n" +
                    "Bạn có hiểu thao tác đầu/chân/tay nào tạo ra kết quả vừa rồi không?";

                float width = Mathf.Min(Screen.width - 40, 620);
                GUI.Box(new Rect((Screen.width - width) * 0.5f, Screen.height * 0.5f - 140, width, 280), summary);
            }
        }

        private float GetBoutSecondsRemaining()
        {
            if (!_boutStarted) return BoutSeconds;
            return Mathf.Max(0f, _boutEnd - Time.unscaledTime);
        }

        private static (Transform transform, SphereCollider collider) CreateGlove(
            string name,
            Transform parent,
            Vector3 localPosition,
            Color color)
        {
            GameObject glove = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glove.name = name;
            glove.transform.SetParent(parent, false);
            glove.transform.localPosition = localPosition;
            glove.transform.localScale = new Vector3(0.24f, 0.21f, 0.28f);
            ApplyColor(glove.GetComponent<Renderer>(), color);
            SphereCollider collider = glove.GetComponent<SphereCollider>();
            collider.radius = 0.5f;
            return (glove.transform, collider);
        }

        private static void CreateBoundary(Vector3 position, Vector3 scale)
        {
            GameObject boundary = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boundary.name = "Ring Rope";
            boundary.transform.position = position;
            boundary.transform.localScale = scale;
            ApplyColor(boundary.GetComponent<Renderer>(), new Color(0.72f, 0.74f, 0.78f));
        }

        private static void ApplyColor(Renderer renderer, Color color)
        {
            if (renderer == null) return;

            // Runtime primitives appeared magenta in the first iPhone Web test.
            // Prefer the built-in WebGL-safe unlit shader so P0 colors remain legible.
            Shader shader = Shader.Find("Unlit/Color");
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
