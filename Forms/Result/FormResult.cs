using System.Collections.Generic;

namespace GuiBlast.Forms.Result;

public sealed class FormResult
{
    public bool Submitted { get; init; }
    public Dictionary<string, object?> Values { get; init; } = new();
    public object? this[string key] => Values.GetValueOrDefault(key);
}