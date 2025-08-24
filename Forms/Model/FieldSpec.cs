using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GuiBlast.Forms.Model;

/// <summary>
/// Describes a single form field: type, label, validation, defaults, and (when applicable) options.
/// </summary>
public sealed class FieldSpec
{
    /// <summary>
    /// Unique key for this field (used as the value key in the result model).
    /// </summary>
    public string Key { get; init; } = "";

    /// <summary>
    /// Field type (e.g., <c>text</c>, <c>number</c>, <c>textarea</c>, <c>password</c>, <c>email</c>,
    /// <c>select</c>, <c>multiselect</c>, <c>checkbox</c>, <c>switch</c>, <c>radio</c>, <c>slider</c>,
    /// <c>range</c>, <c>date</c>, <c>time</c>, <c>datetime</c>, <c>file</c>, <c>color</c>).
    /// </summary>
    public string Type { get; init; } = "text";

    /// <summary>
    /// Display label shown to the user.
    /// </summary>
    [JsonInclude]
    public string? Label { get; init; }

    /// <summary>
    /// Placeholder text for inputs that support it (e.g., text/textarea).
    /// </summary>
    [JsonInclude]
    public string? Placeholder { get; init; }

    /// <summary>
    /// When <c>true</c>, the field must be provided (basic required check).
    /// </summary>
    [JsonInclude]
    public bool? Required { get; init; }

    /// <summary>
    /// Minimum allowed value (numeric or date/time; interpretation depends on <see cref="Type"/>).
    /// </summary>
    [JsonInclude]
    public double? Min { get; init; }

    /// <summary>
    /// Maximum allowed value (numeric or date/time; interpretation depends on <see cref="Type"/>).
    /// </summary>
    [JsonInclude]
    public double? Max { get; init; }

    /// <summary>
    /// Step size for numeric inputs (e.g., increment amount).
    /// </summary>
    [JsonInclude]
    public double? Step { get; init; }

    /// <summary>
    /// Number of rows for a <c>textarea</c>.
    /// </summary>
    [JsonInclude]
    public int? Rows { get; init; }

    /// <summary>
    /// Regular expression pattern used for validation (applies to text-like inputs).
    /// </summary>
    [JsonInclude]
    public string? Pattern { get; init; }

    /// <summary>
    /// When <c>true</c>, validates the value as an email address.
    /// </summary>
    [JsonInclude]
    public bool? Email { get; init; }

    /// <summary>
    /// Options for selectable inputs (<c>select</c>, <c>multiselect</c>, <c>radio</c>).
    /// Ignored for other field types.
    /// </summary>
    [JsonInclude]
    public List<Option>? Options { get; init; }

    /// <summary>
    /// Optional help/description text shown with the field.
    /// </summary>
    [JsonInclude]
    public string? Description { get; init; }

    /// <summary>
    /// Initial value for the field (type depends on <see cref="Type"/>).
    /// </summary>
    [JsonInclude]
    public object? Default { get; init; }

    /// <summary>
    /// Field-level tags used by visibility rules (<see cref="VisibilityRule.ShowTags"/> / <see cref="VisibilityRule.HideTags"/>).
    /// </summary>
    [JsonInclude]
    public string[]? Tags { get; set; }
}
