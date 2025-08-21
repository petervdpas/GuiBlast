using System.Text.Json.Serialization;

namespace GuiBlast.Forms.Model;

public sealed class ActionSpec
{
    public string Id { get; init; } = "";
    
    public string Label { get; init; } = "";
    
    [JsonInclude]
    public bool Primary { get; init; }
    
    [JsonInclude]
    public bool Submit { get; init; }
    
    [JsonInclude]
    public bool Dismiss { get; init; }
}
