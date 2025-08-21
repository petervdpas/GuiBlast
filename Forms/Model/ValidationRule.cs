using System.Text.Json.Serialization;

namespace GuiBlast.Forms.Model;

/// <summary>
/// Defines validation constraints for a single form field.
/// </summary>
public sealed class ValidationRule
{
    /// <summary>
    /// Whether the field is required.
    /// </summary>
    public bool? Required { get; set; }

    /// <summary>
    /// Minimum allowed string length.
    /// </summary>
    [JsonInclude]
    public int? MinLen { get; set; }

    /// <summary>
    /// Maximum allowed string length.
    /// </summary>
    [JsonInclude]
    public int? MaxLen { get; set; }

    /// <summary>
    /// Minimum numeric value.
    /// </summary>
    public double? Min { get; set; }

    /// <summary>
    /// Maximum numeric value.
    /// </summary>
    public double? Max { get; set; }

    /// <summary>
    /// Regex pattern the value must match.
    /// </summary>
    public string? Regex { get; set; }

    /// <summary>
    /// If <c>true</c>, validates that the value is an email address.
    /// </summary>
    public bool? Email { get; set; }
}
