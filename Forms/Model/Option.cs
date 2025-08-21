using System.Text.Json.Serialization;

namespace GuiBlast.Forms.Model;

/// <summary>
/// Represents a selectable option (value/label pair) for fields such as
/// <c>select</c>, <c>multiselect</c>, or <c>radio</c>.
/// </summary>
[method: JsonConstructor]
public sealed class Option(string? value = null, string? label = null)
{
    /// <summary>
    /// The underlying value of the option.  
    /// Defaults to <see cref="Label"/> if not provided.
    /// </summary>
    public string Value { get; } = value ?? label ?? "";

    /// <summary>
    /// The display label shown to the user.  
    /// Defaults to <see cref="Value"/> if not provided.
    /// </summary>
    public string Label { get; } = label ?? value ?? "";

    /// <summary>
    /// Returns the <see cref="Label"/> as the string representation.
    /// </summary>
    public override string ToString() => Label;
}
