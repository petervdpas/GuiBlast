using System.Text.Json.Serialization;

namespace GuiBlast.Forms.Model;

public sealed class ValidationRule
{
    public bool? Required { get; set; }
    
    [JsonInclude]
    public int? MinLen { get; set; }
    
    [JsonInclude]
    public int? MaxLen { get; set; }
    
    public double? Min { get; set; }
    
    public double? Max { get; set; }
    
    public string? Regex { get; set; }
    
    public bool? Email { get; set; }
}
