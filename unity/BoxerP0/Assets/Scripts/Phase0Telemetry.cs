using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace BoxerP0
{
    public sealed class Phase0Telemetry : MonoBehaviour
    {
        private StreamWriter _writer;
        private float _nextSample;

        public string LastOutcome { get; private set; } = "NONE";
        public string LastEvent { get; private set; } = "BOOT";
        public string LogPath { get; private set; }

        public BoxerInput InputSource { get; set; }
        public PlayerBoxer Player { get; set; }
        public OpponentBoxer Opponent { get; set; }

        private void Start()
        {
            try
            {
                string directory = Path.Combine(Application.persistentDataPath, "BoxerP0");
                Directory.CreateDirectory(directory);
                LogPath = Path.Combine(directory, $"phase0-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv");
                _writer = new StreamWriter(LogPath, false);
                _writer.WriteLine("utc_seconds,head_input_deg,head_offset_m,move_x,move_y,punch_intent,player_action,opponent_action,guard_state,outcome,counter_state,event");
                _writer.Flush();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"P0 telemetry file unavailable: {ex.Message}");
            }
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextSample || InputSource == null || Player == null || Opponent == null)
            {
                return;
            }

            _nextSample = Time.unscaledTime + 0.1f;
            WriteRow();
        }

        public void RecordOutcome(string actor, CombatOutcome outcome, bool counter)
        {
            LastOutcome = outcome.ToString().ToUpperInvariant();
            LastEvent = counter ? $"{actor}_COUNTER_{LastOutcome}" : $"{actor}_{LastOutcome}";
            WriteRow();
        }

        public void RecordEvent(string value)
        {
            LastEvent = value;
            WriteRow();
        }

        private void WriteRow()
        {
            if (_writer == null || InputSource == null || Player == null || Opponent == null)
            {
                return;
            }

            string line = string.Join(",",
                Time.unscaledTime.ToString("F3", CultureInfo.InvariantCulture),
                InputSource.HeadAngleDegrees.ToString("F2", CultureInfo.InvariantCulture),
                Player.HeadOffset.ToString("F3", CultureInfo.InvariantCulture),
                InputSource.MovementIntent.x.ToString("F2", CultureInfo.InvariantCulture),
                InputSource.MovementIntent.y.ToString("F2", CultureInfo.InvariantCulture),
                InputSource.LastPunchIntent,
                Player.ActionLabel,
                Opponent.ActionLabel,
                Player.GuardActive ? "HIGH" : "OPEN",
                LastOutcome,
                Opponent.CounterWindowOpen ? "OPEN" : "CLOSED",
                LastEvent);
            _writer.WriteLine(line);
        }

        private void OnDestroy()
        {
            if (_writer == null) return;
            _writer.Flush();
            _writer.Dispose();
            _writer = null;
        }
    }
}

