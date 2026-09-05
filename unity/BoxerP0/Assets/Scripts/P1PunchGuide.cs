using System;
using UnityEngine;

namespace BoxerP0
{
    /// <summary>
    /// P1-A2 mobile-first gesture guide layered above the legacy P0 punch-training copy.
    /// Presentation only; it does not alter input or combat.
    /// </summary>
    [DefaultExecutionOrder(200)]
    public sealed class P1PunchGuide : MonoBehaviour
    {
        private Phase0Telemetry _telemetry;
        private string _stage = string.Empty;
        private string _lastEvent = string.Empty;
        private GUIStyle _step;
        private GUIStyle _action;
        private GUIStyle _hint;

        private void Start()
        {
            _telemetry = FindFirstObjectByType<Phase0Telemetry>();
        }

        private void Update()
        {
            if (_telemetry == null) return;
            string value = _telemetry.LastEvent ?? string.Empty;
            if (value == _lastEvent) return;
            _lastEvent = value;

            const string prefix = "TRAINING_STAGE_START_";
            if (value.StartsWith(prefix, StringComparison.Ordinal))
            {
                _stage = value.Substring(prefix.Length);
            }
            else if (value == "TRAINING_COMPLETE" || value == "BOUT_START")
            {
                _stage = string.Empty;
            }
        }

        private void OnGUI()
        {
            if (_stage != "PUNCHES") return;
            EnsureStyles();
            GUI.depth = -1000;

            float width = Screen.width;
            float panelHeight = Mathf.Min(Screen.height * 0.36f, 390f);
            Rect panel = new(0f, 0f, width, panelHeight);
            Color prior = GUI.color;
            GUI.color = new Color(0.025f, 0.028f, 0.035f, 0.97f);
            GUI.Box(panel, GUIContent.none);
            GUI.color = prior;

            GUI.Label(new Rect(20f, 16f, width - 40f, 48f), "3/5  PUNCHES", _step);
            GUI.Label(new Rect(20f, 66f, width - 40f, 72f), "TAP = STRAIGHT", _action);
            GUI.Label(
                new Rect(20f, 140f, width - 40f, panelHeight - 150f),
                "GIỮ + ↑  UPPERCUT     ·     GIỮ + ↔  HOOK     ·     GIỮ + ↓  OVERHAND",
                _hint);
        }

        private void EnsureStyles()
        {
            if (_step != null) return;
            int shortSide = Mathf.Min(Screen.width, Screen.height);
            _step = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Mathf.RoundToInt(shortSide * 0.046f), 20, 34),
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.72f, 0.24f) }
            };
            _action = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Mathf.RoundToInt(shortSide * 0.065f), 28, 46),
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            _hint = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                wordWrap = true,
                fontSize = Mathf.Clamp(Mathf.RoundToInt(shortSide * 0.037f), 17, 28),
                normal = { textColor = new Color(0.90f, 0.91f, 0.94f) }
            };
        }
    }
}
