using System;
using System.Collections.Generic;
using Avalonia.Controls;
using GuiBlast.Forms.Model;

namespace GuiBlast.Forms.Rendering;

/// <summary>
/// Applies <see cref="VisibilityRule"/> logic to form field containers,
/// dynamically showing or hiding controls based on model values.
/// </summary>
public static class DynamicFormVisibility
{
    /// <summary>
    /// Applies visibility rules to form field containers.
    /// </summary>
    /// <param name="rules">
    /// A list of <see cref="VisibilityRule"/> objects that determine
    /// which fields should be shown or hidden.
    /// </param>
    /// <param name="model">
    /// The current form model (field keys mapped to values).
    /// </param>
    /// <param name="containers">
    /// A mapping of field keys to their corresponding UI <see cref="Control"/> containers.
    /// </param>
    /// <remarks>
    /// All containers are reset to visible before rules are applied.  
    /// If multiple rules apply, explicit <c>Hide</c> takes precedence over <c>Show</c>.
    /// </remarks>
    public static void ApplyVisibility(
        List<VisibilityRule>? rules,
        Dictionary<string, object?> model,
        Dictionary<string, Control> containers)
    {
        foreach (var c in containers.Values) c.IsVisible = true; // reset

        if (rules is null) return;

        var show = new HashSet<string>(StringComparer.Ordinal);
        var hide = new HashSet<string>(StringComparer.Ordinal);

        foreach (var r in rules)
        {
            var val = model.TryGetValue(r.Field, out var v) ? v?.ToString() : null;
            var cond = (r.Eq is not null && string.Equals(val, r.Eq, StringComparison.Ordinal)) ||
                       (r.Neq is not null && !string.Equals(val, r.Neq, StringComparison.Ordinal));
            if (!cond) continue;

            if (r.Show is not null) foreach (var k in r.Show) show.Add(k);
            if (r.Hide is not null) foreach (var k in r.Hide) hide.Add(k);
        }

        // Hide takes precedence
        foreach (var k in show)
            if (!hide.Contains(k) && containers.TryGetValue(k, out var c1))
                c1.IsVisible = true;

        foreach (var k in hide)
            if (containers.TryGetValue(k, out var c2))
                c2.IsVisible = false;
    }
}
