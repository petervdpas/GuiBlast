// OptionJsonConverter.cs
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GuiBlast
{
    public sealed class OptionJsonConverter : JsonConverter<Option>
    {
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

        public override void Write(Utf8JsonWriter writer, Option value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("value", value.Value);
            writer.WriteString("label", value.Label);
            writer.WriteEndObject();
        }
    }
}
