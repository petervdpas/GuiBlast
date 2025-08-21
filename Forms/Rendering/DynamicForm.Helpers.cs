using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using GuiBlast.Forms.Model;

namespace GuiBlast.Forms.Rendering;

/// <summary>
/// Provides utility functions for normalizing options,
/// type conversion, JSON handling, and validation helpers for dynamic forms.
/// </summary>
public static partial class DynamicFormHelpers
{
    /// <summary>
    /// Normalizes field options, ensuring at least one option is present.  
    /// Returns the field’s <see cref="FieldSpec.Options"/> if available,  
    /// otherwise returns a single placeholder option.
    /// </summary>
    /// <param name="f">The field specification.</param>
    /// <returns>A list of <see cref="Option"/> instances.</returns>
    public static List<Option> NormalizeOptions(FieldSpec f)
    {
        var list = new List<Option>();
        if (f.Options is { Count: > 0 }) list.AddRange(f.Options);
        return list.Count > 0 ? list : [new Option("", "(none)")];
    }

    /// <summary>
    /// Converts a <see cref="JsonElement"/> into a CLR object,
    /// mapping JSON types to .NET primitives where possible.
    /// </summary>
    /// <param name="e">The JSON element.</param>
    /// <returns>
    /// A deserialized value: string, number, bool, array, or <c>null</c>.
    /// For unhandled types, returns the element’s string representation.
    /// </returns>
    public static object? FromJson(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.String => e.GetString(),
        JsonValueKind.Number => e.TryGetInt64(out var i) ? i : (object?)e.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Array => e.EnumerateArray().Select(FromJson).ToArray(),
        JsonValueKind.Null => null,
        _ => e.ToString()
    };

    /// <summary>
    /// Converts a value to a boolean, interpreting strings like "true"/"false".
    /// </summary>
    /// <param name="v">The value to convert.</param>
    /// <returns><c>true</c> if the value represents true; otherwise <c>false</c>.</returns>
    public static bool ToBool(object? v)
        => v is bool b ? b : (v is string s && bool.TryParse(s, out var bb) && bb);

    /// <summary>
    /// Attempts to convert a value to a <see cref="double"/>.
    /// </summary>
    /// <param name="v">The input value.</param>
    /// <returns>The converted double if successful; otherwise <c>null</c>.</returns>
    public static double? ConvertToDouble(object? v)
    {
        return v switch
        {
            null => null,
            double d => d,
            float f => f,
            int i => i,
            long l => l,
            string s when double.TryParse(s, out var ds) => ds,
            _ => null
        };
    }

    /// <summary>
    /// Attempts to parse a value as a <see cref="DateTime"/>.
    /// </summary>
    /// <param name="v">The input value (string or DateTime).</param>
    /// <param name="dt">The parsed date.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise <c>false</c>.</returns>
    public static bool TryToDate(object? v, out DateTime dt)
    {
        dt = default;
        switch (v)
        {
            case DateTime x:
                dt = x;
                return true;
            case string s when DateTime.TryParse(s, out var p):
                dt = p;
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Attempts to parse a value as a <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="v">The input value (string or TimeSpan).</param>
    /// <param name="ts">The parsed time span.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise <c>false</c>.</returns>
    public static bool TryToTime(object? v, out TimeSpan ts)
    {
        ts = TimeSpan.Zero;
        switch (v)
        {
            case TimeSpan t:
                ts = t;
                return true;
            case string s when TimeSpan.TryParse(s, out var p):
                ts = p;
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Serializes a value into a <see cref="JsonElement"/>,  
    /// normalizing types unsupported by default serialization.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <returns>A <see cref="JsonElement"/> representation of the value.</returns>
    public static JsonElement SerializeToElementRelaxed(object? value)
    {
        return value switch
        {
            // Already a JsonElement?
            JsonElement je => je,
            // Normalize types System.Text.Json does not love by default
            TimeSpan ts => JsonSerializer.SerializeToElement(ts.ToString("c")),
            DateTime dt => JsonSerializer.SerializeToElement(dt),
            DateTimeOffset dto => JsonSerializer.SerializeToElement(dto),
            _ => JsonSerializer.SerializeToElement(value)
        };
    }

    /// <summary>
    /// Provides a lightweight regex for email validation.
    /// </summary>
    /// <returns>A compiled <see cref="Regex"/> instance.</returns>
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    public static partial Regex MyRegex();
}
