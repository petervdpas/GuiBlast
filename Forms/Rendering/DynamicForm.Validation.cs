using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using GuiBlast.Forms.Model;

namespace GuiBlast.Forms.Rendering;

public static class DynamicFormValidation
{
    public static bool ValidateAll(FormSpec spec, Dictionary<string, object?> model,
        Dictionary<string, TextBlock> errorBlocks)
    {
        bool ok = true;

        // Clear previous errors
        foreach (var eb in errorBlocks.Values)
        {
            eb.IsVisible = false;
            eb.Text = "";
        }

        foreach (var f in spec.Fields)
        {
            if (!errorBlocks.TryGetValue(f.Key, out var err)) continue;
            var v = model.GetValueOrDefault(f.Key);

            var msg = ValidateField(f, spec.Validation, v);

            if (string.IsNullOrEmpty(msg)) continue;

            err.Text = msg;
            err.IsVisible = true;
            ok = false;
        }

        return ok;
    }

    private static string? ValidateField(FieldSpec f, Dictionary<string, ValidationRule>? all, object? v)
    {
        // Merge inline flags + named rules
        var rule = (all != null && all.TryGetValue(f.Key, out var vr)) ? vr : new ValidationRule();
        if (f.Required == true && rule.Required is null) rule.Required = true;
        if (f.Min is not null && rule.Min is null) rule.Min = f.Min;
        if (f.Max is not null && rule.Max is null) rule.Max = f.Max;
        if (!string.IsNullOrWhiteSpace(f.Pattern) && string.IsNullOrWhiteSpace(rule.Regex)) rule.Regex = f.Pattern;
        if (f.Email == true && rule.Email is null) rule.Email = true;

        // Required
        if (rule.Required == true)
        {
            switch (v)
            {
                case null:
                case string s when string.IsNullOrWhiteSpace(s):
                case Array { Length: 0 }:
                    return "Required.";
            }
        }

        // Length
        if (v is string sv)
        {
            if (rule.MinLen is { } minL && sv.Length < minL) return $"Min length: {minL}.";
            if (rule.MaxLen is { } maxL && sv.Length > maxL) return $"Max length: {maxL}.";
        }

        // Numeric
        var vd = DynamicFormHelpers.ConvertToDouble(v);
        if (vd is not null)
        {
            if (rule.Min is { } min && vd < min) return $"Min value: {min}.";
            if (rule.Max is { } max && vd > max) return $"Max value: {max}.";
        }

        // Regex
        if (!string.IsNullOrWhiteSpace(rule.Regex) && v is string rxVal)
        {
            if (!Regex.IsMatch(rxVal, rule.Regex)) return "Invalid format.";
        }

        // Email
        if (rule.Email == true && v is string { Length: > 0 } ev)
        {
            // lightweight check
            if (!DynamicFormHelpers.MyRegex().IsMatch(ev)) return "Invalid email.";
        }

        return null;
    }
}