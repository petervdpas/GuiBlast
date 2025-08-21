using System.Text.Json.Serialization;

namespace GuiBlast.Forms.Model;

public sealed class VisibilityRule
{
    public string Field { get; set; } = "";
    
    [JsonInclude]
    public string? Eq { get; set; }
    
    [JsonInclude]
    public string? Neq { get; set; }
    
    [JsonInclude]
    public string[]? Show { get; set; }
    
    [JsonInclude]
    public string[]? Hide { get; set; }
}
