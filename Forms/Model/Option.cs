using System.Text.Json.Serialization;

namespace GuiBlast.Forms.Model;

[method: JsonConstructor]
public sealed class Option(string? value = null, string? label = null)
{
    public string Value { get; } = value ?? label ?? "";
    public string Label { get; } = label ?? value ?? "";

    public override string ToString() => Label;
}
