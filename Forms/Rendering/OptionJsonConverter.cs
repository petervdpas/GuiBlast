using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using GuiBlast.Forms.Model;

namespace GuiBlast.Forms.Rendering;

/// <summary>
/// Custom JSON converter for <see cref="Option"/> values.  
/// Supports deserialization from either:
/// <list type="bullet">
///   <item><description>A simple string (used for both <see cref="Option.Value"/> and <see cref="Option.Label"/>)</description></item>
///   <item><description>An object with <c>value</c>, <c>label</c>, and optional <c>tags</c> properties</description></item>
/// </list>
/// On serialization, always writes an object with <c>value</c>, <c>label</c>, and optional <c>tags</c>.
/// </summary>
public sealed class OptionJsonConverter : JsonConverter<Option>
{
    /// <summary>
    /// Reads JSON and converts it into an <see cref="Option"/> instance.
    /// </summary>
    /// <param name="reader">The JSON reader providing the input.</param>
    /// <param name="typeToConvert">The target type to convert to (ignored).</param>
    /// <param name="options">Serialization options in effect.</param>
    /// <returns>A new <see cref="Option"/> instance.</returns>
    /// <exception cref="JsonException">
    /// Thrown when the JSON token is not a string or an object with
    /// <c>value</c>/<c>label</c>/<c>tags</c>.
    /// </exception>
    public override Option Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
            {
                var s = reader.GetString() ?? "";
                return new Option(s, s, null);
            }

            case JsonTokenType.StartObject:
            {
                using var doc = JsonDocument.ParseValue(ref reader);
                var root = doc.RootElement;

                string? value = null;
                string? label = null;
                string[]? tags = null;

                if (root.TryGetProperty("value", out var v)) value = v.GetString();
                if (root.TryGetProperty("label", out var l)) label = l.GetString();

                if (!root.TryGetProperty("tags", out var t)) return new Option(value, label, tags);
                
                switch (t.ValueKind)
                {
                    case JsonValueKind.Array:
                    {
                        var list = new List<string>();
                        foreach (var e in t.EnumerateArray())
                            if (e.ValueKind == JsonValueKind.String && e.GetString() is { } s)
                                list.Add(s);
                        if (list.Count > 0) tags = list.ToArray();
                        break;
                    }
                    case JsonValueKind.String when t.GetString() is { } one:
                        tags = [one];
                        break;
                    default:
                        throw new JsonException("The 'tags' property must be either a string or an array of strings.");
                }

                return new Option(value, label, tags);
            }

            default:
                throw new JsonException("Option must be a string or an object with 'value'/'label'/'tags'.");
        }
    }

    /// <summary>
    /// Writes an <see cref="Option"/> instance to JSON as an object with
    /// <c>value</c>, <c>label</c>, and optional <c>tags</c>.
    /// </summary>
    /// <param name="writer">The JSON writer to use.</param>
    /// <param name="value">The <see cref="Option"/> instance to serialize.</param>
    /// <param name="options">Serialization options in effect.</param>
    public override void Write(Utf8JsonWriter writer, Option value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("value", value.Value);
        writer.WriteString("label", value.Label);

        if (value.Tags is { Length: > 0 })
        {
            writer.WritePropertyName("tags");
            writer.WriteStartArray();
            foreach (var t in value.Tags)
                writer.WriteStringValue(t);
            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }
}
