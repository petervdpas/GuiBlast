using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GuiBlast.Forms.Model;

/// <summary>
/// Defines a single form field specification, including its type, label, validation, and options.
/// </summary>
public sealed class FieldSpec
{
    /// <summary>
    /// Unique key identifying the field. Used to bind values.
    /// </summary>
    public string Key { get; init; } = "";

    /// <summary>
    /// Field type (e.g. text, number, textarea, password, email, select, multiselect,
    /// checkbox, switch, radio, slider, range, date, time, datetime, file, color).
    /// </summary>
    public string Type { get; init; } = "text";

    /// <summary>
    /// Label displayed to the user.
    /// </summary>
    [JsonInclude]
    public string? Label { get; init; }

    /// <summary>
    /// Placeholder text (for inputs such as text boxes).
    /// </summary>
    [JsonInclude]
    public string? Placeholder { get; init; }

    /// <summary>
    /// Whether the field is required.
    /// </summary>
    [JsonInclude]
    public bool? Required { get; init; }

    /// <summary>
    /// Minimum numeric or date/time value.
    /// </summary>
    [JsonInclude]
    public double? Min { get; init; }

    /// <summary>
    /// Maximum numeric or date/time value.
    /// </summary>
    [JsonInclude]
    public double? Max { get; init; }

    /// <summary>
    /// Step value for numeric inputs (e.g. increment size).
    /// </summary>
    [JsonInclude]
    public double? Step { get; init; }

    /// <summary>
    /// Number of rows (for textarea).
    /// </summary>
    [JsonInclude]
    public int? Rows { get; init; }

    /// <summary>
    /// Regex pattern for validation.
    /// </summary>
    [JsonInclude]
    public string? Pattern { get; init; }

    /// <summary>
    /// If <c>true</c>, validates as an email address.
    /// </summary>
    [JsonInclude]
    public bool? Email { get; init; }

    /// <summary>
    /// List of options (for select, multiselect, or radio fields).
    /// </summary>
    [JsonInclude]
    public List<Option>? Options { get; init; }

    /// <summary>
    /// Additional description or help text for the field.
    /// </summary>
    [JsonInclude]
    public string? Description { get; init; }

    /// <summary>
    /// Default value for the field.
    /// </summary>
    [JsonInclude]
    public object? Default { get; init; }
}
