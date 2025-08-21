using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using GuiBlast.Forms.Model;

namespace GuiBlast.Forms.Rendering;

public static partial class DynamicFormHelpers
{
    public static List<Option> NormalizeOptions(FieldSpec f)
    {
        var list = new List<Option>();
        if (f.Options is { Count: > 0 }) list.AddRange(f.Options);
        return list.Count > 0 ? list : [new Option("", "(none)")];
    }

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

    public static bool ToBool(object? v)
        => v is bool b ? b : (v is string s && bool.TryParse(s, out var bb) && bb);

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

        // Everything else
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    public static partial Regex MyRegex();
}