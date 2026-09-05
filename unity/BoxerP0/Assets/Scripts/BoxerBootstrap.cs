using System;
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
        private readonly OnboardingProgress _training = new();

        private float _boutEnd;
        private float _stageEnd;
        private float _smokeQuitAt = -1f;
        private bool _boutStarted;
        private bool _boutCompleted;
        private int _guardBlockBaseline;
        private int _counterHitBaseline;
        private OnboardingStage _stage = OnboardingStage.WaitingForCalibration;

        private const float BoutSeconds = 45f;
        private const int PerfWindowSize = 180;
        private const float UiRefreshSeconds = 0.25f;

        private readonly float[] _frameMs = new float[PerfWindowSize];
        private readonly float[] _frameScratch = new float[PerfWindowSize];
        private int _frameCount;
        private int _frameCursor;
        private float _nextUiRefresh;
        private float _lastPerfRefreshRealtime;
        private uint _lastOrientationCount;
        private uint _lastTouchCount;
        private float _fpsCurrent;
        private float _fpsAverage;
        private float _frameP95Ms;
        private float _frameMaxMs;
        private float _orientationAgeMs = -1f;
        private float _touchAgeMs = -1f;
        private float _orientationRate;
        private float _touchRate;
        private string _perfState = "OK";
        private string _debugText = string.Empty;
        private string _trainingText = string.Empty;
        private string _resultText = string.Empty;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            ConfigureSmokeQuit();
            BuildLightingAndRing();
            BuildActors();
            _lastPerfRefreshRealtime = Time.realtimeSinceStartup;
            _nextUiRefresh = _lastPerfRefreshRealtime;

#if UNITY_WEBGL && !UNITY_EDITOR
            _stage = OnboardingStage.WaitingForCalibration;
            _boutEnd = float.PositiveInfinity;
            _player?.SetCombatEnabled(false);
            _opponent?.SetCombatEnabled(false);
#else
            BeginOnboarding();
#endif
            RefreshCachedUi();
        }

        private void Update()
        {
            SampleFrame();

#if UNITY_WEBGL && !UNITY_EDITOR
            if (_stage == OnboardingStage.WaitingForCalibration && _input != null && _input.BrowserCalibrated)
            {
                BeginOnboarding();
            }
#endif
            UpdateOnboarding();

            if (_boutStarted && !_boutCompleted && Time.unscaledTime >= _boutEnd)
            {
                CompleteBout();
            }

            if (_smokeQuitAt > 0f && Time.unscaledTime >= _smokeQuitAt)
            {
                Debug.Log("P0_SMOKE_COMPLETE");
                Application.Quit(0);
            }

            if (Time.realtimeSinceStartup >= _nextUiRefresh)
            {
                RefreshCachedUi();
            }
        }

        private void BeginOnboarding()
        {
            EnterStage(OnboardingStage.HeadControl);
        }

        private void UpdateOnboarding()
        {
            if (_input == null || _player == null || _opponent == null || _telemetry == null) return;

            switch (_stage)
            {
                case OnboardingStage.HeadControl:
                    _training.ObserveHead(_player.HeadOffset);
                    if (_training.HeadReady || StageTimedOut()) EnterStage(OnboardingStage.Footwork);
                    break;

                case OnboardingStage.Footwork:
                    _training.ObserveMovement(_input.MovementIntent);
                    if (_training.FootworkReady || StageTimedOut()) EnterStage(OnboardingStage.Punches);
                    break;

                case OnboardingStage.Punches:
                    _training.ObservePunch(_input.LastPunchIntent);
                    if (_training.PunchesReady || StageTimedOut()) EnterStage(OnboardingStage.Guard);
                    break;

                case OnboardingStage.Guard:
                    if (_telemetry.PlayerBlocks - _guardBlockBaseline >= 2 || StageTimedOut())
                    {
                        EnterStage(OnboardingStage.Counter);
                    }
                    break;

                case OnboardingStage.Counter:
                    if (_telemetry.PlayerCounterHits - _counterHitBaseline >= 1 || StageTimedOut())
                    {
                        StartBout();
                    }
                    break;
            }
        }

        private bool StageTimedOut()
        {
            return Time.unscaledTime >= _stageEnd;
        }

        private void EnterStage(OnboardingStage stage)
        {
            if (_stage != OnboardingStage.WaitingForCalibration)
            {
                _telemetry?.RecordEvent($"TRAINING_STAGE_END_{_stage.ToString().ToUpperInvariant()}");
            }

            _stage = stage;
            _telemetry?.RecordEvent($"TRAINING_STAGE_START_{stage.ToString().ToUpperInvariant()}");

            switch (stage)
            {
                case OnboardingStage.HeadControl:
                    _stageEnd = Time.unscaledTime + 10f;
                    _player?.SetCombatEnabled(false);
                    _opponent?.SetCombatEnabled(false);
                    break;
                case OnboardingStage.Footwork:
                    _stageEnd = Time.unscaledTime + 12f;
                    _player?.SetCombatEnabled(false);
                    _opponent?.SetCombatEnabled(false);
                    break;
                case OnboardingStage.Punches:
                    _stageEnd = Time.unscaledTime + 18f;
                    _player?.SetCombatEnabled(true);
                    _opponent?.SetCombatEnabled(false);
                    break;
                case OnboardingStage.Guard:
                    _stageEnd = Time.unscaledTime + 12f;
                    _guardBlockBaseline = _telemetry?.PlayerBlocks ?? 0;
                    _player?.SetCombatEnabled(true);
                    _opponent?.SetCombatEnabled(true);
                    break;
                case OnboardingStage.Counter:
                    _stageEnd = Time.unscaledTime + 18f;
                    _counterHitBaseline = _telemetry?.PlayerCounterHits ?? 0;
                    _player?.SetCombatEnabled(true);
                    _opponent?.SetCombatEnabled(true);
                    break;
            }
        }

        private void StartBout()
        {
            _telemetry?.RecordEvent("TRAINING_COMPLETE");
            _stage = OnboardingStage.Bout;
            _boutStarted = true;
            _boutCompleted = false;
            _boutEnd = Time.unscaledTime + BoutSeconds;
            _player?.SetCombatEnabled(true);
            _opponent?.SetCombatEnabled(true);
            _telemetry?.RecordBoutStart();
        }

        private void CompleteBout()
        {
            _boutCompleted = true;
            _stage = OnboardingStage.Complete;
            _player?.SetCombatEnabled(false);
            _opponent?.SetCombatEnabled(false);
            string result = _telemetry?.CompleteBout() ?? "UNKNOWN";
            _resultText = BuildResultText();
            Debug.Log($"P0_BOUT_COMPLETE {result}");
        }

        private void ConfigureSmokeQuit()
        {
            foreach (string argument in Environment.GetCommandLineArgs())
            {
                const string prefix = "-p0SmokeSeconds=";
                if (!argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
                if (float.TryParse(argument.Substring(prefix.Length), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float seconds) && seconds > 0f)
                {
                    _smokeQuitAt = Time.unscaledTime + seconds;
                }
            }
        }

        private void SampleFrame()
        {
            float ms = Mathf.Max(0f, Time.unscaledDeltaTime * 1000f);
            _frameMs[_frameCursor] = ms;
            _frameCursor = (_frameCursor + 1) % PerfWindowSize;
            if (_frameCount < PerfWindowSize) _frameCount++;
        }

        private void RefreshCachedUi()
        {
            if (_input == null || _player == null || _opponent == null || _telemetry == null) return;

            float now = Time.realtimeSinceStartup;
            float interval = Mathf.Max(0.001f, now - _lastPerfRefreshRealtime);
            _lastPerfRefreshRealtime = now;
            _nextUiRefresh = now + UiRefreshSeconds;

            float totalMs = 0f;
            int count = _frameCount;
            for (int i = 0; i < count; i++)
            {
                float value = _frameMs[i];
                _frameScratch[i] = value;
                totalMs += value;
            }

            if (count > 0)
            {
                Array.Sort(_frameScratch, 0, count);
                float currentMs = Mathf.Max(0.001f, Time.unscaledDeltaTime * 1000f);
                float avgMs = Mathf.Max(0.001f, totalMs / count);
                int p95Index = Mathf.Clamp(Mathf.CeilToInt(count * 0.95f) - 1, 0, count - 1);
                _fpsCurrent = 1000f / currentMs;
                _fpsAverage = 1000f / avgMs;
                _frameP95Ms = _frameScratch[p95Index];
                _frameMaxMs = _frameScratch[count - 1];
            }

            uint orientationCount = _input.OrientationEventCount;
            uint touchCount = _input.TouchEventCount;
            _orientationRate = (orientationCount - _lastOrientationCount) / interval;
            _touchRate = (touchCount - _lastTouchCount) / interval;
            _lastOrientationCount = orientationCount;
            _lastTouchCount = touchCount;

            _orientationAgeMs = _input.LastOrientationEventRealtime < 0f
                ? -1f
                : Mathf.Max(0f, (now - _input.LastOrientationEventRealtime) * 1000f);
            _touchAgeMs = _input.LastTouchEventRealtime < 0f
                ? -1f
                : Mathf.Max(0f, (now - _input.LastTouchEventRealtime) * 1000f);

            bool inputStalled = (_input.BrowserCalibrated && _orientationAgeMs >= 1000f) ||
                                (_input.TouchActive && _touchAgeMs >= 1000f);
            if (_frameMaxMs >= 500f || inputStalled)
            {
                _perfState = "STALLED";
            }
            else if (_frameP95Ms >= 45f)
            {
                _perfState = "DEGRADED";
            }
            else
            {
                _perfState = "OK";
            }

            _trainingText = GetTrainingText();
            _debugText =
                $"BUILD {Application.version}  PERF {_perfState}\n" +
                $"FPS {_fpsCurrent:F0} now / {_fpsAverage:F0} avg  FRAME p95 {_frameP95Ms:F1}ms max {_frameMaxMs:F1}ms\n" +
                $"ORIENT age {AgeText(_orientationAgeMs)}  rate {_orientationRate:F1}/s\n" +
                $"TOUCH age {AgeText(_touchAgeMs)}  rate {_touchRate:F1}/s  active {(_input.TouchActive ? "YES" : "NO")}\n" +
                $"STAGE {_stage}  MOTION {_input.BrowserMotionPermission}  SRC {_input.HeadInputSource}\n" +
                $"HEAD {_input.HeadAngleDegrees:F1}° → {_player.HeadOffset:F2}m  MOVE {_input.MovementIntent.x:F2},{_input.MovementIntent.y:F2}\n" +
                $"PUNCH {_input.LastPunchLabel}  PLAYER {_player.ActionLabel}  GUARD {(_player.GuardActive ? "HIGH" : "OPEN")}\n" +
                $"OPP {_opponent.ActionLabel}  COUNTER {(_opponent.CounterWindowOpen ? "OPEN" : "CLOSED")}\n" +
                $"LAST {_telemetry.LastOutcome} / {_telemetry.LastEvent}  BOUT {GetBoutSecondsRemaining():F0}s";
        }

        private static string AgeText(float ageMs)
        {
            return ageMs < 0f ? "N/A" : $"{ageMs:F0}ms";
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
            CapsuleCollider originalBodyCollider = opponentBody.GetComponent<CapsuleCollider>();
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

            if (_input == null || _player == null || _opponent == null || _telemetry == null) return;

            if (_stage != OnboardingStage.Bout && _stage != OnboardingStage.Complete)
            {
                float width = Mathf.Min(Screen.width - 40, 680);
                GUI.Box(new Rect((Screen.width - width) * 0.5f, 20, width, 155), _trainingText);
            }

            GUI.Box(new Rect(20, Screen.height - 330, Mathf.Min(Screen.width - 40, 720), 310), _debugText);

            if (!Application.isMobilePlatform)
            {
                GUI.Box(new Rect(Screen.width - 295, 190, 275, 170),
                    "EDITOR SYNTHETIC\nWASD feet · Q/E head\nJ lead jab · K rear cross\nL lead hook · ; rear hook\nR recalibrate · M audio · H haptic");
            }

            if (_boutCompleted)
            {
                float width = Mathf.Min(Screen.width - 40, 620);
                GUI.Box(new Rect((Screen.width - width) * 0.5f, Screen.height * 0.5f - 150, width, 300), _resultText);
            }
        }

        private string GetTrainingText()
        {
            float seconds = Mathf.Max(0f, _stageEnd - Time.unscaledTime);
            return _stage switch
            {
                OnboardingStage.WaitingForCalibration => "ONBOARDING — CALIBRATE PHONE FIRST",
                OnboardingStage.HeadControl =>
                    $"1/5 HEAD CONTROL  {seconds:F0}s\nPhone = head. Nghiêng đầu/máy sang TRÁI rồi PHẢI.\nLEFT {Mark(_training.HeadLeft)}   RIGHT {Mark(_training.HeadRight)}",
                OnboardingStage.Footwork =>
                    $"2/5 FOOTWORK  {seconds:F0}s\nLeft thumb = feet. Thử đủ 4 hướng.\nLEFT {Mark(_training.MoveLeft)}  RIGHT {Mark(_training.MoveRight)}  FORWARD {Mark(_training.MoveForward)}  BACK {Mark(_training.MoveBack)}",
                OnboardingStage.Punches =>
                    $"3/5 PUNCHES  {seconds:F0}s\nRight thumb = punch controller. Làm đủ 4 đòn.\nJAB {Mark(_training.LeadJab)}  CROSS {Mark(_training.RearCross)}  LEAD HOOK {Mark(_training.LeadHook)}  REAR HOOK {Mark(_training.RearHook)}",
                OnboardingStage.Guard =>
                    $"4/5 GUARD  {seconds:F0}s\nDỪNG vuốt tay phải = trở về HIGH GUARD. Đỡ 2 đòn.\nBLOCKS {Mathf.Max(0, _telemetry.PlayerBlocks - _guardBlockBaseline)}/2",
                OnboardingStage.Counter =>
                    $"5/5 COUNTER  {seconds:F0}s\nĐọc đòn → né/đỡ → phản công trong recovery. Làm 1 counter.\nCOUNTERS {Mathf.Max(0, _telemetry.PlayerCounterHits - _counterHitBaseline)}/1",
                _ => string.Empty
            };
        }

        private string BuildResultText()
        {
            string resultText = _telemetry.BoutResult switch
            {
                "PLAYER_WIN" => "PLAYER WIN",
                "OPPONENT_WIN" => "OPPONENT WIN",
                "DRAW" => "DRAW",
                _ => _telemetry.BoutResult
            };

            return
                "BOUT COMPLETE — P0 TEST ONLY\n\n" +
                $"PLAYER  Hits {_telemetry.PlayerHits}  Counters {_telemetry.PlayerCounterHits}  Blocks {_telemetry.PlayerBlocks}\n" +
                $"OPPONENT  Hits {_telemetry.OpponentHits}  Counters {_telemetry.OpponentCounterHits}  Blocks {_telemetry.OpponentBlocks}\n\n" +
                $"RESULT: {resultText}\n\n" +
                "Win rule: valid landed hits only.\n" +
                "Sau onboarding: control còn quá tải không? Bạn có bắt đầu đọc đối thủ thay vì chỉ spam đấm không?";
        }

        private static string Mark(bool value) => value ? "OK" : "—";

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
            BoxCollider boundaryCollider = boundary.GetComponent<BoxCollider>();
            if (boundaryCollider != null) boundaryCollider.enabled = false;
            ApplyColor(boundary.GetComponent<Renderer>(), new Color(0.72f, 0.74f, 0.78f));
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
