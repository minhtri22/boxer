using System;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace BoxerP0
{
    // Local identity only. Never an account, reward ledger or combat parameter.
    [Serializable]
    public sealed class FighterProfile
    {
        public int schema;
        public string id, name, nationality;
        public static bool ValidId(string value)
        {
            if (value == null || value.Length != 32) return false;
            foreach (char c in value) if (!(c >= '0' && c <= '9' || c >= 'a' && c <= 'f')) return false;
            return true;
        }
        public static bool Normalize(string value, int limit, out string normalized)
        {
            normalized = null;
            if (string.IsNullOrEmpty(value) || value.Length > limit * 4) return false;
            try
            {
                value = value.Normalize(NormalizationForm.FormC);
                var buffer = new StringBuilder();
                foreach (char c in value)
                {
                    var category = char.GetUnicodeCategory(c);
                    bool letter = category == UnicodeCategory.UppercaseLetter || category == UnicodeCategory.LowercaseLetter ||
                        category == UnicodeCategory.TitlecaseLetter || category == UnicodeCategory.ModifierLetter ||
                        category == UnicodeCategory.OtherLetter || category == UnicodeCategory.NonSpacingMark ||
                        category == UnicodeCategory.SpacingCombiningMark || category == UnicodeCategory.DecimalDigitNumber;
                    if (!letter && c != ' ' && c != '-' && c != '\'' && c != '’' && c != '.') return false;
                    if (c == ' ' && (buffer.Length == 0 || buffer[buffer.Length - 1] == ' ')) continue;
                    buffer.Append(c);
                }
                normalized = buffer.ToString().Trim();
                return normalized.Length > 0 && StringInfo.ParseCombiningCharacters(normalized).Length <= limit &&
                    char.IsLetterOrDigit(normalized, 0);
            }
            catch (ArgumentException) { return false; }
        }
        public static FighterProfile Candidate(FighterProfile saved, string name, string nationality)
        {
            if (!Normalize(name, 24, out string n) || !Normalize(nationality, 40, out string country)) return null;
            return new FighterProfile { schema = 1, id = saved != null && ValidId(saved.id) ? saved.id : Guid.NewGuid().ToString("N"), name = n, nationality = country };
        }
        public bool Valid => schema == 1 && ValidId(id) && Normalize(name, 24, out string n) && n == name &&
            Normalize(nationality, 40, out string c) && c == nationality;
        public string Serialize() => Valid ? JsonUtility.ToJson(this) : string.Empty;
        public static FighterProfile Parse(string json)
        {
            if (string.IsNullOrEmpty(json) || json.Length > 2048) return null;
            try { var value = JsonUtility.FromJson<FighterProfile>(json); return value != null && value.Valid ? value : null; }
            catch (ArgumentException) { return null; }
        }
    }
}
