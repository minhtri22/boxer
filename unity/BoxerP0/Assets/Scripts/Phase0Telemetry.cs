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

        public int PlayerHits { get; private set; }
        public int PlayerCounterHits { get; private set; }
        public int PlayerBlocks { get; private set; }
        public int OpponentHits { get; private set; }
        public int OpponentCounterHits { get; private set; }
        public int OpponentBlocks { get; private set; }
        public string BoutResult { get; private set; } = "PENDING";

        public BoxerInput InputSource { get; set; }
        public PlayerBoxer Player { get; set; }
        public OpponentBoxer Opponent { get; set; }

        private void Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // Safari/WebGL is only the P0 interaction surrogate. Keep authoritative counters and
            // last-event state in memory, but avoid persistent CSV/file-system churn during combat.
            LogPath = "WEB_IN_MEMORY_ONLY";
            return;
#else
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
#endif
        }

        private void Update()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return;
#else
            if (Time.unscaledTime < _nextSample || InputSource == null || Player == null || Opponent == null)
            {
                return;
            }

            _nextSample = Time.unscaledTime + 0.1f;
            WriteRow();
#endif
        }

        public void RecordBoutStart()
        {
            PlayerHits = 0;
            PlayerCounterHits = 0;
            PlayerBlocks = 0;
            OpponentHits = 0;
            OpponentCounterHits = 0;
            OpponentBlocks = 0;
            BoutResult = "IN_PROGRESS";
            LastOutcome = "NONE";
            RecordEvent("BOUT_START");
        }

        public string CompleteBout()
        {
            BoutResult = PlayerHits > OpponentHits
                ? "PLAYER_WIN"
                : PlayerHits < OpponentHits
                    ? "OPPONENT_WIN"
                    : "DRAW";

            RecordEvent($"BOUT_END PLAYER_HITS={PlayerHits} OPPONENT_HITS={OpponentHits}");
            RecordEvent($"RESULT_{BoutResult}");
            return BoutResult;
        }

        public void RecordOutcome(string actor, CombatOutcome outcome, bool counter)
        {
            LastOutcome = outcome.ToString().ToUpperInvariant();
            LastEvent = counter ? $"{actor}_COUNTER_{LastOutcome}" : $"{actor}_{LastOutcome}";

            if (actor == "PLAYER")
            {
                if (outcome == CombatOutcome.Hit)
                {
                    PlayerHits++;
                    if (counter) PlayerCounterHits++;
                }
                else if (outcome == CombatOutcome.Block)
                {
                    OpponentBlocks++;
                }
            }
            else if (actor == "OPPONENT")
            {
                if (outcome == CombatOutcome.Hit)
                {
                    OpponentHits++;
                    if (counter) OpponentCounterHits++;
                }
                else if (outcome == CombatOutcome.Block)
                {
                    PlayerBlocks++;
                }
            }

#if !(UNITY_WEBGL && !UNITY_EDITOR)
            WriteRow();
#endif
        }

        public void RecordEvent(string value)
        {
            LastEvent = value;
#if !(UNITY_WEBGL && !UNITY_EDITOR)
            WriteRow();
#endif
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
                InputSource.LastPunchLabel,
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
