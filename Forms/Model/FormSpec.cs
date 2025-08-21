using System.Collections.Generic;
using System.Text.Json;

namespace GuiBlast.Forms.Model;

/// <summary>
/// Defines a full form specification, including metadata, fields,
/// validation, visibility rules, and available actions.
/// </summary>
public sealed class FormSpec
{
    /// <summary>
    /// Optional identifier for the form (useful when tracking multiple forms).
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Title of the form (displayed in the dialog header).
    /// </summary>
    public string Title { get; init; } = "Form";

    /// <summary>
    /// Optional size specification (width/height) of the form window.
    /// </summary>
    public SizeSpec? Size { get; init; }

    /// <summary>
    /// Initial data values for fields, keyed by field name.
    /// </summary>
    public Dictionary<string, JsonElement>? Data { get; set; }

    /// <summary>
    /// Collection of field definitions that make up the form.
    /// </summary>
    public List<FieldSpec> Fields { get; init; } = [];

    /// <summary>
    /// Optional visibility rules to conditionally show/hide fields.
    /// </summary>
    public List<VisibilityRule>? Visibility { get; init; }

    /// <summary>
    /// Validation rules keyed by field name.
    /// </summary>
    public Dictionary<string, ValidationRule>? Validation { get; init; }

    /// <summary>
    /// Actions (buttons) available at the bottom of the form.
    /// </summary>
    public List<ActionSpec>? Actions { get; init; }
}