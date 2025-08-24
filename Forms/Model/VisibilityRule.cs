using System.Text.Json.Serialization;

namespace GuiBlast.Forms.Model;

/// <summary>
/// Conditional visibility and option-filtering rule, evaluated against another field’s value.
/// </summary>
/// <remarks>
/// <para>
/// A rule is considered matched when either:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="Eq"/> equals the controller field’s value, or</description></item>
///   <item><description><see cref="Neq"/> does not equal the controller field’s value.</description></item>
/// </list>
/// <para>
/// For <see cref="Show"/> / <see cref="Hide"/> / <see cref="ShowTags"/> / <see cref="HideTags"/>,
/// a controller <see cref="Field"/> with an <see cref="Eq"/> and/or <see cref="Neq"/> is required.
/// For <see cref="Options"/>, <see cref="Field"/> may be omitted to apply unconditionally.
/// When rules conflict, <strong>hide takes precedence</strong>.
/// </para>
/// </remarks>
public sealed class VisibilityRule
{
    /// <summary>
    /// Controller field key to evaluate for this rule.
    /// Required for <see cref="Show"/>, <see cref="Hide"/>, <see cref="ShowTags"/>, <see cref="HideTags"/>.
    /// May be omitted for unconditional <see cref="Options"/> filters.
    /// </summary>
    public string Field { get; set; } = "";

    /// <summary>
    /// Triggers the rule when the controller field’s value equals this value.
    /// </summary>
    [JsonInclude]
    public string? Eq { get; set; }

    /// <summary>
    /// Triggers the rule when the controller field’s value does <em>not</em> equal this value.
    /// </summary>
    [JsonInclude]
    public string? Neq { get; set; }

    /// <summary>
    /// Field keys to show when the rule matches.
    /// </summary>
    [JsonInclude]
    public string[]? Show { get; set; }

    /// <summary>
    /// Field keys to hide when the rule matches (takes precedence over <see cref="Show"/>).
    /// </summary>
    [JsonInclude]
    public string[]? Hide { get; set; }

    /// <summary>
    /// Show all fields whose containers carry any of these tags (set via <see cref="FieldSpec.Tags"/>)
    /// when the rule matches.
    /// </summary>
    [JsonInclude, JsonPropertyName("show_tags")]
    public string[]? ShowTags { get; set; }

    /// <summary>
    /// Hide all fields whose containers carry any of these tags (set via <see cref="FieldSpec.Tags"/>)
    /// when the rule matches. Hide takes precedence over show.
    /// </summary>
    [JsonInclude, JsonPropertyName("hide_tags")]
    public string[]? HideTags { get; set; }

    /// <summary>
    /// Option-level filtering that targets another field’s options
    /// (e.g., include options with certain tags). If <see cref="Field"/> is omitted,
    /// this filter applies unconditionally.
    /// </summary>
    [JsonInclude]
    public OptionVisibilityRule? Options { get; set; }
}
