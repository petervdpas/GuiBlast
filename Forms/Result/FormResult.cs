using System.Collections.Generic;

namespace GuiBlast.Forms.Result;

/// <summary>
/// Represents the result of a form after user interaction.
/// </summary>
public sealed class FormResult
{
    /// <summary>
    /// Indicates whether the form was submitted (<c>true</c>) or cancelled (<c>false</c>).
    /// </summary>
    public bool Submitted { get; init; }

    /// <summary>
    /// Dictionary of field values, keyed by field <c>Key</c>.
    /// </summary>
    public Dictionary<string, object?> Values { get; init; } = new();

    /// <summary>
    /// Gets the value of a field by key, or <c>null</c> if not present.
    /// </summary>
    public object? this[string key] => Values.GetValueOrDefault(key);
}
