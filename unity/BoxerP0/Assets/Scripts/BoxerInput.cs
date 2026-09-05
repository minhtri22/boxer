using System;
using System.Globalization;
using UnityEngine;

namespace BoxerP0
{
    public sealed class BoxerInput : MonoBehaviour
    {
        public event Action<PunchIntent> PunchRequested;

        public Vector2 MovementIntent { get; private set; }
        public float HeadAngleDegrees { get; private set; }
        public string HeadInputSource { get; private set; } = "SYNTHETIC";
        public PunchIntent LastPunchIntent { get; private set; }
        public PunchFamily LastPunchFamily => PunchLabels.Family(LastPunchIntent);
        public string LastPunchLabel => PunchLabels.Display(LastPunchIntent);
        public string BrowserMotionPermission { get; private set; } = "N/A";
        public float BrowserAlpha { get; private set; }
        public float BrowserBeta { get; private set; }
        public float BrowserGamma { get; private set; }
        public float BrowserNeutralGamma { get; private set; }
        public bool BrowserOrientationReceived { get; private set; }
        public bool BrowserCalibrated { get; private set; }
        public float LastOrientationEventRealtime { get; private set; } = -1f;
        public float LastTouchEventRealtime { get; private set; } = -1f;
        public uint OrientationEventCount { get; private set; }
        public uint TouchEventCount { get; private set; }
        public uint PunchEventCount { get; private set; }
        public bool TouchActive => _leftFinger >= 0 || _rightFinger >= 0;

        private int _leftFinger = -1;
        private Vector2 _leftOrigin;
        private int _rightFinger = -1;
        private Vector2 _rightStart;
        private Vector2 _rightPrevious;
        private float _rightPathLength;
        private float _rightStartTime;
        private Quaternion _neutralAttitude = Quaternion.identity;
        private bool _gyroReady;
        private float _syntheticHeadAngle;
        private bool _syntheticDemo;
        private float _demoStart;
        private int _demoPunchIndex;

        private void Start()
        {
            _syntheticDemo = Array.Exists(Environment.GetCommandLineArgs(), value => value == "-p0SyntheticDemo");
            _demoStart = Time.unscaledTime;
            if (_syntheticDemo)
            {
                HeadInputSource = "SYNTHETIC_DEMO";
                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            HeadInputSource = "WEB_ORIENTATION_PENDING";
            BrowserMotionPermission = "PENDING";
            // P1 UAT startup fix: never freeze the global simulation while Safari motion is pending.
            // Head input may remain neutral until orientation arrives, while footwork/punch/training keep running.
            return;
#endif

            if (Application.isMobilePlatform && SystemInfo.supportsGyroscope)
            {
                Input.gyro.enabled = true;
                _neutralAttitude = Input.gyro.attitude;
                _gyroReady = true;
                HeadInputSource = "REAL_DEVICE";
            }
        }

        private void Update()
        {
            if (_syntheticDemo)
            {
                UpdateSyntheticDemo();
                return;
            }

            UpdateHeadInput();
            UpdateTouchInput();
            UpdateEditorFallback();
        }

        private void UpdateSyntheticDemo()
        {
            float elapsed = Time.unscaledTime - _demoStart;
            MovementIntent = new Vector2(Mathf.Sin(elapsed * 0.9f) * 0.45f, Mathf.Cos(elapsed * 0.7f) * 0.35f);
            HeadAngleDegrees = Mathf.Sin(elapsed * 2.2f) * 14f;

            float nextPunchTime = 1.0f + _demoPunchIndex * 1.05f;
            if (_demoPunchIndex < 4 && elapsed >= nextPunchTime)
            {
                PunchFamily family = _demoPunchIndex switch
                {
                    0 => PunchFamily.Straight,
                    1 => PunchFamily.Hook,
                    2 => PunchFamily.Uppercut,
                    _ => PunchFamily.Overhand
                };
                _demoPunchIndex++;
                RequestPunch(PunchHandSelector.Select(family, LastPunchIntent));
            }
        }

        public void RecalibrateHead()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (BrowserOrientationReceived)
            {
                BrowserNeutralGamma = BrowserGamma;
                BrowserCalibrated = true;
                HeadAngleDegrees = 0f;
                HeadInputSource = "WEB_ORIENTATION";
            }
            return;
#endif
            if (_gyroReady)
            {
                _neutralAttitude = Input.gyro.attitude;
            }
            else
            {
                _syntheticHeadAngle = 0f;
            }
        }

        private void UpdateHeadInput()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (BrowserOrientationReceived && BrowserCalibrated)
            {
                HeadAngleDegrees = Mathf.DeltaAngle(BrowserNeutralGamma, BrowserGamma);
            }
            else
            {
                HeadAngleDegrees = 0f;
            }
            return;
#endif
            if (_gyroReady)
            {
                Quaternion relative = Quaternion.Inverse(_neutralAttitude) * Input.gyro.attitude;
                HeadAngleDegrees = Mathf.DeltaAngle(0f, relative.eulerAngles.z);
                return;
            }

            float axis = 0f;
            if (Input.GetKey(KeyCode.Q)) axis -= 1f;
            if (Input.GetKey(KeyCode.E)) axis += 1f;
            _syntheticHeadAngle = Mathf.MoveTowards(_syntheticHeadAngle, axis * 18f, 90f * Time.deltaTime);
            HeadAngleDegrees = _syntheticHeadAngle;
        }

        private void UpdateTouchInput()
        {
            if (Input.touchCount == 0)
            {
                if (_leftFinger >= 0)
                {
                    _leftFinger = -1;
                    MovementIntent = Vector2.zero;
                }
                return;
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved ||
                    touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    LastTouchEventRealtime = Time.realtimeSinceStartup;
                    TouchEventCount++;
                }

                bool leftHalf = touch.position.x < Screen.width * 0.5f;

                if (touch.phase == TouchPhase.Began)
                {
                    if (leftHalf && _leftFinger < 0)
                    {
                        _leftFinger = touch.fingerId;
                        _leftOrigin = touch.position;
                    }
                    else if (!leftHalf && _rightFinger < 0)
                    {
                        _rightFinger = touch.fingerId;
                        _rightStart = touch.position;
                        _rightPrevious = touch.position;
                        _rightPathLength = 0f;
                        _rightStartTime = Time.unscaledTime;
                    }
                }

                if (touch.fingerId == _leftFinger)
                {
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        _leftFinger = -1;
                        MovementIntent = Vector2.zero;
                    }
                    else
                    {
                        Vector2 drag = (touch.position - _leftOrigin) / Mathf.Max(80f, Screen.dpi * 0.45f);
                        MovementIntent = Vector2.ClampMagnitude(drag, 1f);
                    }
                }

                if (touch.fingerId == _rightFinger)
                {
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        _rightPathLength += Vector2.Distance(_rightPrevious, touch.position);
                        _rightPrevious = touch.position;
                    }

                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        _rightPathLength += Vector2.Distance(_rightPrevious, touch.position);
                        float duration = Mathf.Max(0.001f, Time.unscaledTime - _rightStartTime);
                        float scale = Mathf.Max(1f, Screen.dpi / 160f);
                        GestureMetrics metrics = new(touch.position - _rightStart, _rightPathLength, duration);
                        PunchFamily family = PunchGestureClassifier.ClassifyFamily(metrics, scale);
                        PunchIntent intent = PunchHandSelector.Select(family, LastPunchIntent);
                        _rightFinger = -1;
                        RequestPunch(intent);
                    }
                }
            }
        }

        private void UpdateEditorFallback()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return;
#endif
            if (Application.isMobilePlatform)
            {
                return;
            }

            MovementIntent = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            MovementIntent = Vector2.ClampMagnitude(MovementIntent, 1f);

            if (Input.GetKeyDown(KeyCode.J)) RequestPunch(PunchIntent.Jab);
            if (Input.GetKeyDown(KeyCode.K)) RequestPunch(PunchIntent.Cross);
            if (Input.GetKeyDown(KeyCode.L)) RequestPunch(PunchIntent.LeadHook);
            if (Input.GetKeyDown(KeyCode.Semicolon)) RequestPunch(PunchIntent.RearHook);
            if (Input.GetKeyDown(KeyCode.U)) RequestPunch(PunchIntent.LeadUppercut);
            if (Input.GetKeyDown(KeyCode.I)) RequestPunch(PunchIntent.RearUppercut);
            if (Input.GetKeyDown(KeyCode.O)) RequestPunch(PunchIntent.RearOverhand);
            if (Input.GetKeyDown(KeyCode.R)) RecalibrateHead();
        }

        private void RequestPunch(PunchIntent intent)
        {
            LastPunchIntent = intent;
            if (intent != PunchIntent.None)
            {
                PunchEventCount++;
                PunchRequested?.Invoke(intent);
            }
        }

        public void BrowserSetMotionStatus(string status)
        {
            BrowserMotionPermission = string.IsNullOrWhiteSpace(status) ? "UNKNOWN" : status.Trim().ToUpperInvariant();
            if (BrowserMotionPermission == "GRANTED")
            {
                // BrowserCalibrated also serves as the existing Bootstrap readiness flag.
                // Gameplay is unlocked immediately; the first orientation event establishes the neutral head angle.
                BrowserCalibrated = true;
                if (!BrowserOrientationReceived)
                {
                    HeadInputSource = "WEB_MOTION_GRANTED_WAITING_ORIENTATION";
                }
            }
            else
            {
                BrowserCalibrated = false;
                HeadInputSource = $"WEB_{BrowserMotionPermission}";
            }
        }

        public void BrowserSetOrientation(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload)) return;
            string[] fields = payload.Split('|');
            if (fields.Length != 3) return;
            if (!float.TryParse(fields[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float alpha)) return;
            if (!float.TryParse(fields[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float beta)) return;
            if (!float.TryParse(fields[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float gamma)) return;

            bool firstOrientation = !BrowserOrientationReceived;
            BrowserAlpha = alpha;
            BrowserBeta = beta;
            BrowserGamma = gamma;
            BrowserOrientationReceived = true;
            LastOrientationEventRealtime = Time.realtimeSinceStartup;
            OrientationEventCount++;

            if (firstOrientation && BrowserMotionPermission == "GRANTED")
            {
                // Late sensor delivery is allowed: establish neutral on the first real sample without reload.
                BrowserNeutralGamma = BrowserGamma;
                BrowserCalibrated = true;
                HeadAngleDegrees = 0f;
                HeadInputSource = "WEB_ORIENTATION";
            }
            else if (!BrowserCalibrated)
            {
                HeadInputSource = "WEB_ORIENTATION_READY";
            }
            else
            {
                HeadInputSource = "WEB_ORIENTATION";
            }
        }

        public void BrowserCalibrate(string unused)
        {
            RecalibrateHead();
        }
    }
}
