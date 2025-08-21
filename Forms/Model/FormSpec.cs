using System.Collections.Generic;
using System.Text.Json;

namespace GuiBlast.Forms.Model;

public sealed class FormSpec
{
    public string? Id { get; init; }
    public string Title { get; init; } = "Form";
    public SizeSpec? Size { get; init; }
    public Dictionary<string, JsonElement>? Data { get; set; }
    public List<FieldSpec> Fields { get; init; } = [];
    public List<VisibilityRule>? Visibility { get; init; }
    public Dictionary<string, ValidationRule>? Validation { get; init; }
    public List<ActionSpec>? Actions { get; init; }
}