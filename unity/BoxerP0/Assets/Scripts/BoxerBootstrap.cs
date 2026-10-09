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
        private ArmVisualEmbodiment _armVisual;
        private OpponentLegEmbodiment _opponentLegs;
        private OpponentBodyRotationEmbodiment _opponentBodyRotation;
        private OnboardingProgress _training = new();
        private int _trainingTaps;
        private bool _trainingLeadHook, _trainingRearHook;
        private const string TrainingPreference = "BOXER_CONTROL_TRAINING_V2_COMPLETE";

        private float _boutEnd;
        private float _smokeQuitAt = -1f;
        private bool _boutStarted;
        private bool _boutCompleted;
        private int _introSerial;
        private float _editorIntroEnd;
        public string IntroToken => _introSerial.ToString(System.Globalization.CultureInfo.InvariantCulture);
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
        private bool _showDeveloperDiagnostics;
        private P1OpponentProfile _opponentProfile = P1OpponentProfile.Balanced;

        public bool ShowDeveloperDiagnostics => _showDeveloperDiagnostics;
        public ProductFlow Flow { get; } = new();
        public string TrainingToken => Flow.Screen == ProductScreen.Onboarding ? _stage.ToString().ToUpperInvariant() : string.Empty;
        public string TrainingInstructions => GetTrainingText();
        public bool TrainingReady => _stage switch
        {
            OnboardingStage.HeadControl => _training.HeadReady,
            OnboardingStage.Footwork => _training.FootworkReady,
            OnboardingStage.Punches => _training.PunchesReady && _trainingTaps >= 2 && _trainingLeadHook && _trainingRearHook,
            _ => false
        };
        private Vector3 _lastPlayerPosition, _lastOpponentPosition;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            ConfigureSmokeQuit();
            BuildLightingAndRing();
            BuildActors();
            Flow.RestoreTutorialSeen(PlayerPrefs.GetInt(TrainingPreference, 0) == 1);
            _player.PunchAccepted += ObserveTrainingPunch;
            _lastPerfRefreshRealtime = Time.realtimeSinceStartup;
            _nextUiRefresh = _lastPerfRefreshRealtime;

            _stage = OnboardingStage.WaitingForCalibration;
            _boutEnd = float.PositiveInfinity;
            _player?.SetCombatEnabled(false);
            _opponent?.SetCombatEnabled(false);
            _input.SetGameplayInput(false);
            gameObject.AddComponent<ProductScreens>();
            gameObject.AddComponent<Wave1WebAudit>();
            RememberPositions();
            RefreshCachedUi();
        }

        private void Update()
        {
            SampleFrame();

            if (!Application.isMobilePlatform && Input.GetKeyDown(KeyCode.F3))
            {
                _showDeveloperDiagnostics = !_showDeveloperDiagnostics;
                _armVisual?.SetDeveloperDebugVisible(_showDeveloperDiagnostics);
            }

            if (_telemetry.Bout.Active)
            {
                float dt = Time.deltaTime;
                double pm = PlanarSpeed(_player.transform.position, _lastPlayerPosition, dt) / 1.5;
                double om = PlanarSpeed(_opponent.transform.position, _lastOpponentPosition, dt) / 0.36;
                _telemetry.Bout.Tick(dt, _player.CurrentPhase, _opponent.CurrentPhase, pm, om);
            }
            RememberPositions();
            UpdateOnboarding();
#if !UNITY_WEBGL || UNITY_EDITOR
            if (Flow.Screen == ProductScreen.Intro && Time.unscaledTime >= _editorIntroEnd) BrowserIntroReady(IntroToken);
#endif

            if (_boutStarted && !_boutCompleted && _telemetry.Bout.Ended)
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
        private void OnDestroy() { if (_player != null) _player.PunchAccepted -= ObserveTrainingPunch; }

        private static double PlanarSpeed(Vector3 a, Vector3 b, float dt)
        { a.y = b.y = 0; return dt > 0 ? Vector3.Distance(a, b) / dt : 0; }
        private void RememberPositions()
        { _lastPlayerPosition = _player.transform.position; _lastOpponentPosition = _opponent.transform.position; }
        private void ResetActors()
        {
            _input.SetGameplayInput(false);
            _player.ResetForBout(); _opponent.ResetForBout();
            _telemetry.ResetSessionCounters();
            _player.transform.SetPositionAndRotation(new Vector3(0, 0, -0.7f), Quaternion.identity);
            _opponent.transform.SetPositionAndRotation(new Vector3(0, 0, 0.70f), Quaternion.Euler(0, 180, 0));
            _boutStarted = _boutCompleted = false;
            _stage = OnboardingStage.WaitingForCalibration;
            _training = new OnboardingProgress();
            _trainingTaps = 0; _trainingLeadHook = _trainingRearHook = false;
            RememberPositions();
        }
        public void ShowPreview()
        {
            if (!Flow.Preview()) return;
            ResetActors();
        }
        public void BeginProductBout(bool tutorial = false)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!_input.BrowserCalibrated) return;
#endif
            if (!Flow.Begin(tutorial)) return;
            ResetActors();
            if (Flow.Screen == ProductScreen.Onboarding) BeginOnboarding();
            else
            {
                _introSerial++;
                _editorIntroEnd = Time.unscaledTime + 5.166667f;
                RingPresentation.Command(1, _introSerial);
            }
        }
        // Media completion is epoch checked: cancelled/previous-bout callbacks cannot start combat.
        public void BrowserIntroReady(string token)
        {
            if (token != IntroToken || !Flow.IntroFinished()) return;
            StartBout();
        }
        public void BrowserCancelIntro(string token)
        { if (Flow.Screen == ProductScreen.Intro && token == IntroToken) ReturnHome(); }
        public void BeginTraining() => BeginProductBout(true);
        public void RecenterTrainingHead() { if (Flow.Screen == ProductScreen.Onboarding) _input.RecalibrateHead(); }
        public void AdvanceTraining()
        {
            if (Flow.Screen != ProductScreen.Onboarding || !TrainingReady) return;
            if (_stage == OnboardingStage.HeadControl) EnterStage(OnboardingStage.Footwork);
            else if (_stage == OnboardingStage.Footwork) EnterStage(OnboardingStage.Punches);
            else if (_stage == OnboardingStage.Punches)
            {
                Flow.TutorialFinished();
                PlayerPrefs.SetInt(TrainingPreference, 1); PlayerPrefs.Save();
                _telemetry.RecordEvent("CONTROL_TRAINING_COMPLETE");
                ResetActors();
            }
        }
        private void ObserveTrainingPunch(PunchIntent intent)
        {
            if (Flow.Screen != ProductScreen.Onboarding || _stage != OnboardingStage.Punches) return;
            _training.ObservePunch(intent);
            if (PunchLabels.Family(intent) == PunchFamily.Straight) _trainingTaps++;
            if (intent == PunchIntent.LeadHook) _trainingLeadHook = true;
            if (intent == PunchIntent.RearHook) _trainingRearHook = true;
        }
        public void ReturnHome()
        {
            _introSerial++;
            RingPresentation.Command(0, _introSerial);
            Flow.Home(); ResetActors();
        }

        private void UpdateOnboarding()
        {
            if (Flow.Screen != ProductScreen.Onboarding) return;
            if (_input == null || _player == null || _opponent == null || _telemetry == null) return;

            switch (_stage)
            {
                case OnboardingStage.HeadControl:
                    _training.ObserveHead(_player.HeadOffset);
                    break;

                case OnboardingStage.Footwork:
                    _training.ObserveMovement(_input.MovementIntent);
                    break;
            }
        }

        private void EnterStage(OnboardingStage stage)
        {
            if (_stage != OnboardingStage.WaitingForCalibration)
            {
                _telemetry?.RecordEvent($"TRAINING_STAGE_END_{_stage.ToString().ToUpperInvariant()}");
            }

            _stage = stage;
            _telemetry?.RecordEvent($"TRAINING_STAGE_START_{stage.ToString().ToUpperInvariant()}");

            _input.SetGameplayInput(true); // consume navigation release; do not score the menu tap.
            _opponent.SetCombatEnabled(false); // practice never enables an attacking AI.
            switch (stage)
            {
                case OnboardingStage.HeadControl:
                    _player?.SetCombatEnabled(false);
                    _opponent?.SetCombatEnabled(false);
                    break;
                case OnboardingStage.Footwork:
                    _player?.SetCombatEnabled(false);
                    _opponent?.SetCombatEnabled(false);
                    break;
                case OnboardingStage.Punches:
                    _player?.SetCombatEnabled(true);
                    _opponent?.SetCombatEnabled(false);
                    break;
            }
        }

        private void StartBout()
        {
            // Only explicit Preview/Result navigation starts scored combat; training returns Home.
            _stage = OnboardingStage.Bout;
            _boutStarted = true;
            _boutCompleted = false;
            _boutEnd = Time.unscaledTime + BoutSeconds;
            _player?.SetCombatEnabled(true);
            _opponent?.SetCombatEnabled(true);
            // Tutorial contacts are not scored; reset into a fresh bout without recreating actors.
            _player.ResetForBout(); _opponent.ResetForBout();
            _player.transform.SetPositionAndRotation(new Vector3(0, 0, -0.7f), Quaternion.identity);
            _opponent.transform.SetPositionAndRotation(new Vector3(0, 0, 0.70f), Quaternion.Euler(0, 180, 0));
            _telemetry?.RecordBoutStart();
            _player.SetCombatEnabled(true); _opponent.SetCombatEnabled(true);
            _input.SetGameplayInput(true); RememberPositions();
        }

        private void CompleteBout()
        {
            Flow.Finish();
            RingPresentation.Command(2, _introSerial);
            _input.SetGameplayInput(false);
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
                if (argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    if (float.TryParse(argument.Substring(prefix.Length), System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out float seconds) && seconds > 0f)
                    {
                        _smokeQuitAt = Time.unscaledTime + seconds;
                    }
                    continue;
                }

                const string profilePrefix = "-opponentProfile=";
                if (argument.StartsWith(profilePrefix, StringComparison.OrdinalIgnoreCase) &&
                    P1OpponentAttributes.TryParse(argument.Substring(profilePrefix.Length), out P1OpponentProfile profile))
                {
                    _opponentProfile = profile;
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
            P1BiomechanicsObservation biomechanics = _player.CurrentBiomechanicsObservation();
            _debugText =
                $"BUILD {Application.version}  PERF {_perfState}\n" +
                $"FPS {_fpsCurrent:F0} now / {_fpsAverage:F0} avg  FRAME p95 {_frameP95Ms:F1}ms max {_frameMaxMs:F1}ms\n" +
                $"ORIENT age {AgeText(_orientationAgeMs)}  rate {_orientationRate:F1}/s\n" +
                $"TOUCH age {AgeText(_touchAgeMs)}  rate {_touchRate:F1}/s  active {(_input.TouchActive ? "YES" : "NO")}\n" +
                $"STAGE {_stage}  MOTION {_input.BrowserMotionPermission}  SRC {_input.HeadInputSource}\n" +
                $"HEAD {_input.HeadAngleDegrees:F1}° → {_player.HeadOffset:F2}m  MOVE {_input.MovementIntent.x:F2},{_input.MovementIntent.y:F2}\n" +
                $"PUNCH {_input.LastPunchLabel}  PLAYER {_player.ActionLabel}  GUARD {(_player.GuardActive ? "HIGH" : "OPEN")}\n" +
                $"OPP {_opponent.ActionLabel}  COUNTER {_opponent.CounterOpportunityLabel}\n" +
                _opponent.AttributeInspectorText + "\n" +
                $"PLAYER RECOVER {_player.ActiveRecoverSeconds:F3}s\n" +
                $"LAST {_telemetry.LastOutcome} / {_telemetry.LastEvent}  BOUT {GetBoutSecondsRemaining():F0}s\n" +
                biomechanics.ToInspectorText() + "\n" +
                "COMBAT LOG\n" + _telemetry.RecentCombatLog;
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
            camera.fieldOfView = 108f;
            cameraObject.transform.localRotation = Quaternion.Euler(23f, 0f, 0f);
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
            // P1-B2: shrink the visible torso capsule so it no longer reads as a
            // lower-body pedestal. Pelvis + legs are now separate visual geometry.
            // The authoritative combat SphereCollider below is unchanged.
            GameObject opponentBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            opponentBody.name = "Opponent Body";
            opponentBody.transform.SetParent(opponentRoot.transform, false);
            opponentBody.transform.localPosition = new Vector3(0f, 1.24f, 0f);
            opponentBody.transform.localScale = new Vector3(0.50f, 0.32f, 0.38f);
            ApplyColor(opponentBody.GetComponent<Renderer>(), opponentColor);
            CapsuleCollider originalBodyCollider = opponentBody.GetComponent<CapsuleCollider>();
            Destroy(originalBodyCollider);
            SphereCollider opponentBodyCollider = opponentBody.AddComponent<SphereCollider>();
            opponentBodyCollider.center = new Vector3(0f, -0.19f, 0f);
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
            _opponent.ConfigureAttributes(_opponentProfile);

            _telemetry.InputSource = _input;
            _telemetry.Player = _player;
            _telemetry.Opponent = _opponent;

            var round2Rig = gameObject.AddComponent<Round2CombatRig>();
            round2Rig.Initialize(_player, _opponent);
            var feet = gameObject.AddComponent<Round2Footwork>();
            feet.Initialize(_opponent);

        }

        private void OnGUI()
        {
            GUI.skin.label.fontSize = Mathf.Clamp(Screen.height / 52, 12, 22);
            GUI.skin.box.fontSize = GUI.skin.label.fontSize;

            if (_input == null || _player == null || _opponent == null || _telemetry == null) return;

            if (_showDeveloperDiagnostics && _stage != OnboardingStage.Bout && _stage != OnboardingStage.Complete)
            {
                float width = Mathf.Min(Screen.width - 40, 680);
                GUI.Box(new Rect((Screen.width - width) * 0.5f, 20, width, 155), _trainingText);
            }

            if (_showDeveloperDiagnostics)
            {
                GUI.Box(new Rect(20, Mathf.Max(20, Screen.height - 500), Mathf.Min(Screen.width - 40, 780), 480), _debugText);
            }

            if (_showDeveloperDiagnostics && !Application.isMobilePlatform)
            {
                GUI.Box(new Rect(Screen.width - 295, 190, 275, 170),
                    "EDITOR SYNTHETIC\nWASD feet · Q/E head\nJ lead jab · K rear cross\nL lead hook · ; rear hook\nR recalibrate · M audio · H haptic");
            }

            if (_showDeveloperDiagnostics && _boutCompleted)
            {
                float width = Mathf.Min(Screen.width - 40, 620);
                GUI.Box(new Rect((Screen.width - width) * 0.5f, Screen.height * 0.5f - 150, width, 300), _resultText);
            }
        }

        private string GetTrainingText()
        {
            return _stage switch
            {
                OnboardingStage.WaitingForCalibration => "ONBOARDING — CALIBRATE PHONE FIRST",
                OnboardingStage.HeadControl =>
                    $"1 / 3 — NÉ ĐẦU\nNghiêng/lắc điện thoại sang trái rồi phải.\nTRÁI {Mark(_training.HeadLeft)}   PHẢI {Mark(_training.HeadRight)}\nKhông nhận chuyển động? Kiểm tra quyền Motion hoặc trở về Home.",
                OnboardingStage.Footwork =>
                    $"2 / 3 — DI CHUYỂN\nGiữ và vuốt vùng DƯỚI BÊN TRÁI theo 4 hướng.\nTRÁI {Mark(_training.MoveLeft)}  PHẢI {Mark(_training.MoveRight)}\nTIẾN {Mark(_training.MoveForward)}  LÙI {Mark(_training.MoveBack)}",
                OnboardingStage.Punches =>
                    $"3 / 3 — BỐN NHÓM ĐÒN\nVùng DƯỚI BÊN PHẢI: giữ nhẹ rồi vuốt, thả tay để đấm.\nLÊN: móc từ dưới {Mark(_training.Uppercut)}\nXUỐNG: từ trên xuống {Mark(_training.Overhand)}\nNGANG trái/phải: móc ngang {Mark(_trainingLeadHook && _trainingRearHook)}\nCHẠM nhiều lần: jab/cross {Mathf.Min(2,_trainingTaps)}/2\nChờ găng trở lại giữa các đòn; không cần đấm thật nhanh.",
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
