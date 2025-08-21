using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using GuiBlast.Forms.Model;

namespace GuiBlast.Forms.Rendering;

/// <summary>
/// Custom JSON converter for <see cref="Option"/> values.  
/// Supports deserializing from either a string or an object with <c>value</c>/<c>label</c>.
/// </summary>
public sealed class OptionJsonConverter : JsonConverter<Option>
{
    /// <summary>
    /// Reads a JSON value and converts it into an <see cref="Option"/> instance.
    /// Accepts either:
    /// <list type="bullet">
    /// <item><description>A simple string (used for both <c>Value</c> and <c>Label</c>)</description></item>
    /// <item><description>An object with <c>value</c> and/or <c>label</c> properties</description></item>
    /// </list>
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The target type (ignored).</param>
    /// <param name="options">Serialization options.</param>
    /// <returns>A deserialized <see cref="Option"/>.</returns>
    /// <exception cref="JsonException">Thrown if the token type is unsupported.</exception>
    public override Option Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
            {
                var s = reader.GetString();
                return new Option(s, s);
            }
            case JsonTokenType.StartObject:
            {
                using var doc = JsonDocument.ParseValue(ref reader);
                var root = doc.RootElement;
                var value = root.TryGetProperty("value", out var v) ? v.GetString() : null;
                var label = root.TryGetProperty("label", out var l) ? l.GetString() : null;
                return new Option(value, label);
            }
            default:
                throw new JsonException("Option must be a string or an object with 'value'/'label'.");
        }
    }

    /// <summary>
    /// Writes an <see cref="Option"/> instance to JSON as an object with <c>value</c> and <c>label</c> properties.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The <see cref="Option"/> to serialize.</param>
    /// <param name="options">Serialization options.</param>
    public override void Write(Utf8JsonWriter writer, Option value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("value", value.Value);
        writer.WriteString("label", value.Label);
        writer.WriteEndObject();
    }
}
