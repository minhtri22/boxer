using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoxerP0
{
    // Product-side snapshot only. Never writes telemetry, a receipt, vitals or a combat clock.
    [Serializable]
    public sealed class CoachMatchReview
    {
        public int schema;
        public string id, sourceVersion, opponent = "RAMIREZ", result, reason;
        public double seconds, playerHP, opponentHP, playerStamina, playerCapacity;
        public int accepted, hits, blocked, misses, received, defended, opponentMisses;
        public int Resolved => hits + blocked + misses;
        public int Unresolved => accepted - Resolved;
        public double Accuracy => Resolved == 0 ? 0 : 100.0 * hits / Resolved;
        public bool Valid => schema == 1 && !string.IsNullOrEmpty(id) && id.Length <= 80 &&
            !string.IsNullOrEmpty(sourceVersion) && sourceVersion.Length <= 100 && opponent == "RAMIREZ" &&
            (result == "PLAYER_WIN" || result == "OPPONENT_WIN" || result == "DRAW") &&
            (reason == "KO" || reason == "POINTS") && FiniteRange(seconds, 0, 45.001) &&
            FiniteRange(playerHP, 0, 100) && FiniteRange(opponentHP, 0, 100) &&
            FiniteRange(playerStamina, 0, 100) && FiniteRange(playerCapacity, 0, 100) &&
            Count(accepted) && Count(hits) && Count(blocked) && Count(misses) && Count(received) &&
            Count(defended) && Count(opponentMisses) && Resolved <= accepted &&
            (reason == "KO" ? (result == "PLAYER_WIN" && opponentHP == 0) || (result == "OPPONENT_WIN" && playerHP == 0) :
                result == (Math.Abs(playerHP-opponentHP) <= .0001 ? "DRAW" : playerHP > opponentHP ? "PLAYER_WIN" : "OPPONENT_WIN"));
        private static bool Count(int n) => n >= 0 && n <= 10000;
        private static bool FiniteRange(double n, double low, double high) => !double.IsNaN(n) && !double.IsInfinity(n) && n >= low && n <= high;

        public static CoachMatchReview Capture(Phase0Telemetry telemetry, string version)
        {
            if (telemetry == null || telemetry.Player == null || !telemetry.Bout.Ended || telemetry.Bout.Active) return null;
            var bout = telemetry.Bout;
            var review = new CoachMatchReview {
                schema = 1, id = Guid.NewGuid().ToString("N"), sourceVersion = version, result = bout.Result, reason = bout.EndReason,
                seconds = bout.Seconds, playerHP = bout.Player.HP, opponentHP = bout.Opponent.HP,
                playerStamina = bout.Player.Stamina, playerCapacity = bout.Player.Capacity,
                accepted = (int)telemetry.Player.AcceptedPunches, hits = telemetry.PlayerHits,
                blocked = telemetry.OpponentBlocks, misses = telemetry.PlayerMisses,
                received = telemetry.OpponentHits, defended = telemetry.PlayerBlocks, opponentMisses = telemetry.OpponentMisses
            };
            return review.Valid ? review : null;
        }
        public static CoachMatchReview Parse(string json)
        {
            if (string.IsNullOrEmpty(json) || json.Length > 4096) return null;
            try { var value = JsonUtility.FromJson<CoachMatchReview>(json); return value != null && value.Valid ? value : null; }
            catch (ArgumentException) { return null; }
        }
        public string Serialize() => Valid ? JsonUtility.ToJson(this) : string.Empty;

        public CoachModule[] Suggestions()
        {
            if (!Valid) return Array.Empty<CoachModule>();
            var modules = new List<CoachModule>(3);
            if (received > 0) modules.Add(CoachModule.Head);
            if (misses > 0) modules.Add(CoachModule.Footwork);
            modules.Add(CoachModule.Punches);
            if (modules.Count < 3) modules.Add(CoachModule.Conditioning);
            if (modules.Count < 3) modules.Add(CoachModule.Guard);
            return modules.ToArray();
        }
        public string SuggestionReason(CoachModule module) => module switch {
            CoachModule.Head => $"Bị trúng {received} đòn · luyện né và đọc đòn",
            CoachModule.Footwork => $"{misses} đòn hụt · luyện cự ly và góc tiếp cận",
            CoachModule.Punches => Resolved == 0 ? "Chưa có đòn được xét · luyện chạm / vuốt" : $"Trúng {hits}/{Resolved} đòn đã xét · luyện ra đòn",
            CoachModule.Conditioning => $"Stamina cuối {playerStamina:F0} · đọc cách hồi phục",
            CoachModule.Guard => $"Đã đỡ {defended} đòn · đọc hướng dẫn phòng thủ",
            _ => string.Empty
        };
        public string StatsText =>
            $"HP cuối: bạn {playerHP:F1} · Ramirez {opponentHP:F1}\n" +
            $"Đã ra {accepted} · đã xét {Resolved} · chưa xét {Unresolved}\n" +
            $"Trúng {hits} · bị đỡ {blocked} · hụt {misses}\n" +
            (Resolved == 0 ? "Tỉ lệ trúng: chưa có đòn đã xét\n" : $"Tỉ lệ trúng / đòn đã xét: {Accuracy:F1}%\n") +
            $"Bị trúng {received} · đã đỡ {defended} · đối thủ hụt {opponentMisses}\n" +
            $"Stamina cuối {playerStamina:F1} · Capacity cuối {playerCapacity:F1}";
    }
}
