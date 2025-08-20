using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace GuiBlast
{
    /// Pretty/text + JSON output helpers for FormResult
    public static class FormResultFormatter
    {
        // --- Public API ---

        /// Human-readable lines like: key = value  (arrays shown as [a, b, c])
        public static string ToText(FormResult result)
        {
            var lines = new List<string>(result.Values.Count + 1)
            {
                $"Submitted: {result.Submitted}"
            };

            foreach (var kv in result.Values)
                lines.Add($"{kv.Key} = {FormatValue(kv.Value)}");

            return string.Join(Environment.NewLine, lines);
        }

        /// JSON dump with sane formats (ISO datetimes, TimeSpan as "c", enumerables as arrays)
        public static string ToJson(FormResult result, bool indented = true)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = indented
            };

            // Normalize to JSON-friendly types first
            var normalized = new Dictionary<string, object?>();
            foreach (var kv in result.Values)
                normalized[kv.Key] = Normalize(kv.Value);

            // Include Submitted too (handy for logs)
            var payload = new Dictionary<string, object?>
            {
                ["submitted"] = result.Submitted,
                ["values"] = normalized
            };

            return JsonSerializer.Serialize(payload, options);
        }

        /// Convenience: write pretty text to a writer (Console.Out by default)
        public static void WriteText(FormResult result, System.IO.TextWriter? writer = null)
            => (writer ?? Console.Out).WriteLine(ToText(result));

        // --- Internals ---

        private static string FormatValue(object? v)
        {
            if (v is null) return "null";

            // Flatten strings first (they're IEnumerable too)
            if (v is string s) return s;

            if (v is DateTime dt) return dt.ToString("O", CultureInfo.InvariantCulture);  // ISO 8601
            if (v is DateTimeOffset dto) return dto.ToString("O", CultureInfo.InvariantCulture);
            if (v is TimeSpan ts) return ts.ToString("c", CultureInfo.InvariantCulture);

            // Arrays / lists / IEnumerable
            if (v is IEnumerable en && v is not string)
            {
                var parts = en.Cast<object?>().Select(FormatValue);
                return "[" + string.Join(", ", parts) + "]";
            }

            // Numbers, bools, everything else
            return Convert.ToString(v, CultureInfo.InvariantCulture) ?? v.ToString()!;
        }

        // Convert values to JSON-safe shapes (no custom types leaking through)
        private static object? Normalize(object? v)
        {
            if (v is null) return null;
            if (v is string s) return s;

            if (v is DateTime dt) return dt.ToString("O", CultureInfo.InvariantCulture);
            if (v is DateTimeOffset dto) return dto.ToString("O", CultureInfo.InvariantCulture);
            if (v is TimeSpan ts) return ts.ToString("c", CultureInfo.InvariantCulture);

            if (v is IEnumerable en && v is not string)
            {
                var list = new List<object?>();
                foreach (var item in en) list.Add(Normalize(item));
                return list;
            }

            // primitives, bools, etc.
            return v;
        }
    }

    // Handy extensions so you can call result.ToText() / result.ToJson()
    public static class FormResultExtensions
    {
        public static string ToText(this FormResult result) => FormResultFormatter.ToText(result);
        public static string ToJson(this FormResult result, bool indented = true) => FormResultFormatter.ToJson(result, indented);
        public static void WriteText(this FormResult result, System.IO.TextWriter? writer = null)
            => FormResultFormatter.WriteText(result, writer);
    }
}
