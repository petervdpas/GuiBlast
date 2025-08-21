using System.Text.Json.Serialization;

namespace GuiBlast.Forms.Model;

/// <summary>
/// Defines conditional visibility rules for form fields, based on the value of another field.
/// </summary>
public sealed class VisibilityRule
{
    /// <summary>
    /// The key of the field whose value is evaluated for this rule.
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
}
