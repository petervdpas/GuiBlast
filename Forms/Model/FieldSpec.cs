using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GuiBlast.Forms.Model;

public sealed class FieldSpec
{
    public string Key { get; init; } = "";
    
    // text, number, textarea, password, email, select, multiselect, checkbox, switch, radio, slider, range, date, time, datetime, file, color
    public string Type { get; init; } = "text";
    
    [JsonInclude]
    public string? Label { get; init; }
    
    [JsonInclude]
    public string? Placeholder { get; init; }
    
    [JsonInclude]
    public bool? Required { get; init; }
    
    [JsonInclude]
    public double? Min { get; init; }
    
    [JsonInclude]
    public double? Max { get; init; }
    
    [JsonInclude]
    public double? Step { get; init; }
    
    [JsonInclude]
    public int? Rows { get; init; }
    
    [JsonInclude]
    public string? Pattern { get; init; }
    
    [JsonInclude]
    public bool? Email { get; init; }
    
    [JsonInclude]
    public List<Option>? Options { get; init; } // for select/radio
    
    [JsonInclude]
    public string? Description { get; init; }
    
    [JsonInclude]
    public object? Default { get; init; }
}