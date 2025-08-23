using System.Text.Json.Serialization;

namespace GuiBlast.Forms.Model;

/// <summary>
/// Defines conditional visibility rules for form fields, based on the value of another field.
/// Supports both field show/hide and option-level filtering.
/// </summary>
public sealed class VisibilityRule
{
    /// <summary>
    /// The key of the field whose value is evaluated for this rule.
    /// If omitted for an option filter, the option filter is always applied.
    /// </summary>
    public string Field { get; set; } = "";

    /// <summary>
    /// Value that, if equal to the field’s value, will trigger the rule.
    /// </summary>
    [JsonInclude]
    public string? Eq { get; set; }

    /// <summary>
    /// Value that, if not equal to the field’s value, will trigger the rule.
    /// </summary>
    [JsonInclude]
    public string? Neq { get; set; }

    /// <summary>
    /// List of fields to show when the rule condition is met.
    /// </summary>
    [JsonInclude]
    public string[]? Show { get; set; }

    /// <summary>
    /// List of fields to hide when the rule condition is met.
    /// </summary>
    [JsonInclude]
    public string[]? Hide { get; set; }

    /// <summary>
    /// Optional option-level filter that applies to a specific field’s options,
    /// evaluated when this rule’s condition is met (or always if <see cref="Field"/> is empty).
    /// </summary>
    [JsonInclude]
    public OptionVisibilityRule? Options { get; set; }
}
