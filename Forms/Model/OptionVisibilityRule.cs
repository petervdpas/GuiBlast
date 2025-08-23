using System.Text.Json.Serialization;

namespace GuiBlast.Forms.Model;

/// <summary>
/// Describes an option-level filter that applies to the options of a specific field
/// (e.g., filter which <c>select</c>/<c>multiselect</c> options are visible based on their tags).
/// </summary>
public sealed class OptionVisibilityRule
{
    /// <summary>
    /// The target field whose <see cref="Option"/> list will be filtered.
    /// </summary>
    [JsonPropertyName("for")]
    public string For { get; set; } = "";

    /// <summary>
    /// When provided, only options having at least one of these tags remain visible.
    /// If omitted or empty, no include filtering is applied.
    /// </summary>
    [JsonPropertyName("include_tags")]
    public string[]? IncludeTags { get; set; }

    /// <summary>
    /// Options having any of these tags will be removed after include filtering.
    /// If omitted or empty, no exclude filtering is applied.
    /// </summary>
    [JsonPropertyName("exclude_tags")]
    public string[]? ExcludeTags { get; set; }
}