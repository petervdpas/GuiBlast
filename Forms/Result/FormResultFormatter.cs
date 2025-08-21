using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace GuiBlast.Forms.Result;

/// <summary>
/// Provides formatting utilities for <see cref="FormResult"/> values,
/// including text and JSON output.
/// </summary>
public static class FormResultFormatter
{
    /// <summary>
    /// Converts the form result into a human-readable text representation.  
    /// Example: <c>key = value</c> (arrays shown as <c>[a, b, c]</c>).
    /// </summary>
    /// <param name="result">The form result to format.</param>
    /// <returns>A plain text string representing the form result.</returns>
    public static string ToText(FormResult result)
    {
        var lines = new List<string>(result.Values.Count + 1)
        {
            $"Submitted: {result.Submitted}"
        };
        lines.AddRange(result.Values.Select(kv => $"{kv.Key} = {FormatValue(kv.Value)}"));

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Converts the form result into a JSON string.  
    /// Ensures correct formatting (ISO 8601 for <see cref="DateTime"/>, "c" for <see cref="TimeSpan"/>, arrays for enumerables).
    /// </summary>
    /// <param name="result">The form result to format.</param>
    /// <param name="indented">Whether to indent the JSON for readability.</param>
    /// <returns>A JSON string representing the form result.</returns>
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

    /// <summary>
    /// Writes the form result as formatted text to a <see cref="System.IO.TextWriter"/>.  
    /// Defaults to <see cref="Console.Out"/> if none is provided.
    /// </summary>
    /// <param name="result">The form result to output.</param>
    /// <param name="writer">The target writer (optional).</param>
    public static void WriteText(FormResult result, System.IO.TextWriter? writer = null)
        => (writer ?? Console.Out).WriteLine(ToText(result));

    // --- Internals ---

    private static string FormatValue(object? v)
    {
        switch (v)
        {
            case null:
                return "null";
            // Flatten strings first (they're IEnumerable too)
            case string s:
                return s;
            case DateTime dt:
                return dt.ToString("O", CultureInfo.InvariantCulture);  // ISO 8601
            case DateTimeOffset dto:
                return dto.ToString("O", CultureInfo.InvariantCulture);
            case TimeSpan ts:
                return ts.ToString("c", CultureInfo.InvariantCulture);
        }

        // Arrays / lists / IEnumerable
        if (v is not (IEnumerable en and not string))
        {
            return Convert.ToString(v, CultureInfo.InvariantCulture) ?? v.ToString()!;
        }
        
        var parts = en.Cast<object?>().Select(FormatValue);
        return "[" + string.Join(", ", parts) + "]";
    }

    // Convert values to JSON-safe shapes (no custom types leaking through)
    private static object? Normalize(object? v)
    {
        return v switch
        {
            null => null,
            string s => s,
            DateTime dt => dt.ToString("O", CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToString("O", CultureInfo.InvariantCulture),
            TimeSpan ts => ts.ToString("c", CultureInfo.InvariantCulture),
            _ => v is IEnumerable en and not string
                ? (from object? item in en select Normalize(item)).ToList()
                :
                // primitives, bool, etc.
                v
        };
    }
}
