using System.Text.Json.Serialization;

namespace GuiBlast.Forms.Model;

/// <summary>
/// Defines an action (e.g. button) available on a form, such as Save or Cancel.
/// </summary>
public sealed class ActionSpec
{
    /// <summary>
    /// Unique identifier of the action (e.g. "save", "cancel").
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// Display label shown on the button.
    /// </summary>
    public string Label { get; init; } = "";

    /// <summary>
    /// Marks this action as the primary action (highlighted by default).
    /// </summary>
    [JsonInclude]
    public bool Primary { get; init; }

    /// <summary>
    /// If <c>true</c>, clicking this action submits the form.
    /// </summary>
    [JsonInclude]
    public bool Submit { get; init; }

    /// <summary>
    /// If <c>true</c>, clicking this action dismisses the form without submitting.
    /// </summary>
    [JsonInclude]
    public bool Dismiss { get; init; }
}
